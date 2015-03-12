using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace System.ComponentModel.Composition
{
	/// <summary />
	internal sealed class CatalogService : ICatalogService
	{
		private readonly AggregateCatalog _aggregateCatalog;

		/// <summary>
		/// Initializes a new instance of the <see cref="CatalogService"/> class.
		/// </summary>
		/// <param name="aggregateCatalog">The aggregate catalog.</param>
		public CatalogService(AggregateCatalog aggregateCatalog)
		{
			_aggregateCatalog = aggregateCatalog;
		}
#if SILVERLIGHT
		private readonly Dictionary<Uri, XapCatalog> _catalogs = new Dictionary<Uri, XapCatalog>();


		/// <inheritdoc/>
		public XapCatalog AddXap(string relativeUri,
		                                Action<DownloadProgressChangedEventArgs> progressAction = null,
		                                Action<AsyncCompletedEventArgs> completedAction = null)
		{
			Uri uri = new Uri(relativeUri, UriKind.Relative);
			return AddXap(uri, progressAction, completedAction);
		}

		/// <inheritdoc/>
		public XapCatalog AddXap(Uri uri,
		                                Action<DownloadProgressChangedEventArgs> progressAction,
		                                Action<AsyncCompletedEventArgs> completedAction)
		{
			Debug.WriteLine("CatalogService: Request for {0}", uri);
			if (_catalogs.ContainsKey(uri))
			{
				Debug.WriteLine("CatalogService: {0} already downloaded", uri);
				return _catalogs[uri];
			}
			XapCatalog catalog = new XapCatalog(uri);
			if (progressAction != null)
			{
				catalog.DownloadProgressChanged += (s, e) => progressAction(e);
			}
			if (completedAction != null)
			{
				catalog.DownloadCompleted += (s, e) => completedAction(e);
			}
			else
			{
				catalog.DownloadCompleted += (s, e) =>
				{
					if (e.Error != null)
					{
						throw e.Error;
					}
				};
			}
#if DEBUG
			catalog.DownloadCompleted += (s, e) => Debug.WriteLine("CatalogService: Download completed for {0}", uri);
#endif
			catalog.DownloadAsync();
			Debug.WriteLine("CatalogService: Download Started for {0}", uri);
			_catalogs[uri] = catalog;
			_aggregateCatalog.Catalogs.Add(catalog);
			return catalog;
		}

		/// <inheritdoc/>
		public void RemoveXap(string relativeUri)
		{
			RemoveXap(new Uri(relativeUri, UriKind.Relative));
		}

		/// <inheritdoc/>
		public void RemoveXap(Uri uri)
		{
			XapCatalog catalog;
			if (_catalogs.TryGetValue(uri, out catalog))
			{
				_aggregateCatalog.Catalogs.Remove(catalog);
			}
		}
#else
		private readonly Dictionary<string, DirectoryCatalog> _catalogs = new Dictionary<string, DirectoryCatalog>();

		/// <inheritdoc/>
		public void AddDirectory(string path)
		{
			if (_catalogs.ContainsKey(path) || !Directory.Exists(path))
			{
				return;
			}
			DirectoryCatalog catalog = new DirectoryCatalog(path);
			_catalogs[path] = catalog;
			_aggregateCatalog.Catalogs.Add(catalog);
		}

		/// <inheritdoc/>
		public void AddDirectory(string path, string searchPattern)
		{
			if (_catalogs.ContainsKey(path) || !Directory.Exists(path))
			{
				return;
			}
			DirectoryCatalog catalog = new DirectoryCatalog(path, searchPattern);
			_catalogs[path] = catalog;
			_aggregateCatalog.Catalogs.Add(catalog);
		}

		/// <inheritdoc/>
		public void RemoveDirectory(string path)
		{
			DirectoryCatalog catalog;
			if (_catalogs.TryGetValue(path, out catalog))
			{
				_aggregateCatalog.Catalogs.Remove(catalog);
			}
		}
#endif
	}
}