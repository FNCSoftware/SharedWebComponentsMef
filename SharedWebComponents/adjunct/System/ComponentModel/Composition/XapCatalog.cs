#if SILVERLIGHT
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace System.ComponentModel.Composition
{
	/// <summary>
	/// Implements a MEF catalog that supports Asynchronous download of Silverlight Xap files.
	/// </summary>
	internal class XapCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
	{
		private readonly Lock _lock = new Lock();
		private readonly Uri _uri;
		private AggregateCatalog _catalogCollection = new AggregateCatalog();
		private volatile bool _isDisposed;
		private int _state = State.Created;
		private WebClient _webClient;

		/// <summary>
		/// Construct a Deployment catalog with the parts from the main Xap.
		/// </summary>
		public XapCatalog()
		{
			DiscoverParts(XapPackage.CurrentAssemblies);
			_state = State.Initialized;
		}

		/// <summary>
		/// Construct a Deployment catalog with a string form relative uri.
		/// </summary>
		/// <value>
		///     A relative Uri to the Download Xap file
		///     <see cref="XapCatalog"/>.
		/// </value>
		/// <exception cref="ArgumentException">
		///     The argument is null or an empty string.
		/// </exception>
		public XapCatalog(string uriRelative)
		{
			if (string.IsNullOrEmpty(uriRelative))
			{
				throw new ArgumentNullException("uriRelative");
			}
			_uri = new Uri(uriRelative, UriKind.Relative);
		}

		/// <summary>
		/// Construct a Deployment catalog with the parts from uri.
		/// </summary>
		/// <value>
		///     A Uri to the Download Xap file
		///     <see cref="System.Uri"/>.
		/// </value>
		/// <exception cref="ArgumentException">
		///     The argument is null.
		/// </exception>
		public XapCatalog(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			_uri = uri;
		}

		/// <summary>
		///     Gets the part definitions of the Deployment catalog.
		/// </summary>
		/// <value>
		///     A <see cref="IQueryable{T}"/> of <see cref="ComposablePartDefinition"/> objects of the 
		///     <see cref="XapCatalog"/>.
		/// </value>
		/// <exception cref="ObjectDisposedException">
		///     The <see cref="XapCatalog"/> has been disposed of.
		/// </exception>
		public override IQueryable<ComposablePartDefinition> Parts
		{
			get
			{
				ThrowIfDisposed();
				return _catalogCollection.Parts;
			}
		}

		/// <summary>
		///     Gets the Uri of this catalog
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		///     The <see cref="XapCatalog"/> has been disposed of.
		/// </exception>
		public Uri Uri
		{
			get
			{
				ThrowIfDisposed();
				return _uri;
			}
		}

		/// <summary>
		/// Retrieve or create the WebClient.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		///     The <see cref="XapCatalog"/> has been disposed of.
		/// </exception>
		private WebClient WebClient
		{
			get
			{
				ThrowIfDisposed();
				if (_webClient == null)
				{
					Interlocked.CompareExchange(ref _webClient, new WebClient(), null);
				}
				return _webClient;
			}
		}

		/// <summary>
		/// Notify when the contents of the Catalog has changed.
		/// </summary>
		public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

		/// <summary>
		/// Notify when the contents of the Catalog is changing.
		/// </summary>
		public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

		/// <summary>
		/// Notify when the download has been completed.
		/// </summary>
		public event EventHandler<AsyncCompletedEventArgs> DownloadCompleted;

		/// <summary>
		/// Notify when the contents of the Progress of the download has changed.
		/// </summary>
		public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

		/// <summary>
		/// Cancel the async operation.
		/// </summary>
		public void CancelAsync()
		{
			ThrowIfDisposed();
			MutateStateOrThrow(State.DownloadCancelled, State.DownloadStarted);
			WebClient.CancelAsync();
		}

		/// <summary>
		/// Begin the asynchronous download.
		/// </summary>
		public void DownloadAsync()
		{
			ThrowIfDisposed();

			if (Interlocked.CompareExchange(ref _state, State.DownloadStarted, State.Created) == State.Created)
			{
				// Created with Downloadable content do download
				WebClient.OpenReadCompleted += HandleOpenReadCompleted;
				WebClient.DownloadProgressChanged += HandleDownloadProgressChanged;
				WebClient.OpenReadAsync(Uri, this);
			}
			else
			{
				// Created with LocalAssemblies 
				MutateStateOrThrow(State.DownloadCompleted, State.Initialized);

				OnDownloadCompleted(new AsyncCompletedEventArgs(null, false, this));
			}
		}

		/// <summary>
		///     Returns the export definitions that match the constraint defined by the specified definition.
		/// </summary>
		/// <param name="definition">
		///     The <see cref="ImportDefinition"/> that defines the conditions of the 
		///     <see cref="ExportDefinition"/> objects to return.
		/// </param>
		/// <returns>
		///     An <see cref="IEnumerable{T}"/> of <see cref="Tuple{T1, T2}"/> containing the 
		///     <see cref="ExportDefinition"/> objects and their associated 
		///     <see cref="ComposablePartDefinition"/> for objects that match the constraint defined 
		///     by <paramref name="definition"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="definition"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		///     The <see cref="XapCatalog"/> has been disposed of.
		/// </exception>
		public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
		{
			ThrowIfDisposed();
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}

			return _catalogCollection.GetExports(definition);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (!_isDisposed)
					{
						AggregateCatalog catalogs = null;
						bool disposeLock = false;
						try
						{
							using (new WriteLock(_lock))
							{
								if (!_isDisposed)
								{
									disposeLock = true;
									catalogs = _catalogCollection;
									_catalogCollection = null;
									_isDisposed = true;
								}
							}
						}
						finally
						{
							if (catalogs != null)
							{
								catalogs.Dispose();
							}

							if (disposeLock)
							{
								_lock.Dispose();
							}
						}
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		/// <summary>
		///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changed"/> event.
		/// </summary>
		/// <param name="e">
		///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
		/// </param>
		protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
		{
			EventHandler<ComposablePartCatalogChangeEventArgs> changedEvent = Changed;
			if (changedEvent != null)
			{
				changedEvent(this, e);
			}
		}

		/// <summary>
		///     Raises the <see cref="INotifyComposablePartCatalogChanged.Changing"/> event.
		/// </summary>
		/// <param name="e">
		///     An <see cref="ComposablePartCatalogChangeEventArgs"/> containing the data for the event.
		/// </param>
		protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
		{
			EventHandler<ComposablePartCatalogChangeEventArgs> changingEvent = Changing;
			if (changingEvent != null)
			{
				changingEvent(this, e);
			}
		}

		/// <summary>
		///     Raises the <see cref="DownloadCompleted"/> event.
		/// </summary>
		/// <param name="e">
		///     An <see cref="AsyncCompletedEventArgs"/> containing the data for the event.
		/// </param>
		protected virtual void OnDownloadCompleted(AsyncCompletedEventArgs e)
		{
			EventHandler<AsyncCompletedEventArgs> downloadCompletedEvent = DownloadCompleted;
			if (downloadCompletedEvent != null)
			{
				downloadCompletedEvent(this, e);
			}
		}

		/// <summary>
		///     Raises the <see cref="DownloadProgressChanged"/> event.
		/// </summary>
		/// <param name="e">
		///     An <see cref="ProgressChangedEventArgs"/> containing the data for the event.
		/// </param>
		protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs e)
		{
			EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedEvent = DownloadProgressChanged;
			if (downloadProgressChangedEvent != null)
			{
				downloadProgressChangedEvent(this, e);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="assemblies">
		/// </param>
		/// <exception cref="ObjectDisposedException">
		///     The <see cref="XapCatalog"/> has been disposed of.
		/// </exception>
		private void DiscoverParts(IEnumerable<Assembly> assemblies)
		{
			ThrowIfDisposed();

			List<ComposablePartDefinition> addedDefinitions = new List<ComposablePartDefinition>();
			Dictionary<string, ComposablePartCatalog> addedCatalogs = new Dictionary<string, ComposablePartCatalog>();
			using (new ReadLock(_lock))
			{
				foreach (Assembly assembly in assemblies)
				{
					if (addedCatalogs.ContainsKey(assembly.FullName))
					{
						// Nothing to do because the assembly has already been added.
						continue;
					}

					AssemblyCatalog catalog = new AssemblyCatalog(assembly);
					addedDefinitions.AddRange(catalog.Parts);
					addedCatalogs.Add(assembly.FullName, catalog);
				}
			}

			// Generate notifications
			using (AtomicComposition atomicComposition = new AtomicComposition())
			{
				ComposablePartCatalogChangeEventArgs changingArgs = new ComposablePartCatalogChangeEventArgs(
					addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), atomicComposition);
				OnChanging(changingArgs);

				using (new WriteLock(_lock))
				{
					foreach (KeyValuePair<string, ComposablePartCatalog> item in addedCatalogs)
					{
						_catalogCollection.Catalogs.Add(item.Value);
					}
				}
				atomicComposition.Complete();
			}

			ComposablePartCatalogChangeEventArgs changedArgs = new ComposablePartCatalogChangeEventArgs(
				addedDefinitions, Enumerable.Empty<ComposablePartDefinition>(), null);
			OnChanged(changedArgs);
		}

		private void HandleDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			EventHandler<DownloadProgressChangedEventArgs> downloadProgressChangedEvent = DownloadProgressChanged;
			if (downloadProgressChangedEvent != null)
			{
				downloadProgressChangedEvent(this, e);
			}
		}

		private void HandleOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
		{
			Exception error = e.Error;
			bool cancelled = e.Cancelled;

			// Possible valid current states are DownloadStarted and DownloadCancelled.
			int currentState = Interlocked.CompareExchange(ref _state, State.DownloadCompleted, State.DownloadStarted);

			if (currentState != State.DownloadStarted)
			{
				cancelled = true;
			}

			if (error == null && !cancelled)
			{
				try
				{
					IEnumerable<Assembly> assemblies = XapPackage.LoadPackagedAssemblies(e.Result);
					DiscoverParts(assemblies);
				}
				catch (Exception ex)
				{
					error = new InvalidOperationException("Strings.InvalidOperationException_ErrorReadingXap", ex);
				}
			}

			OnDownloadCompleted(new AsyncCompletedEventArgs(error, cancelled, this));
		}

		private void MutateStateOrThrow(int toState, int fromState)
		{
			int currentState = Interlocked.CompareExchange(ref _state, toState, fromState);
			if (currentState != fromState)
			{
				throw new InvalidOperationException("Strings.InvalidOperationException_DeploymentCatalogInvalidStateChange");
			}
		}

		private void ThrowIfDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().ToString());
			}
		}

		private static class State
		{
			public const int Created = 0;
			public const int DownloadCancelled = 4000;
			public const int DownloadCompleted = 3000;
			public const int DownloadStarted = 2000;
			public const int Initialized = 1000;
		}
	}
}
#endif