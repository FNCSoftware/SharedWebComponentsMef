using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

#if SILVERLIGHT
using System.Windows;
#endif

namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	internal static class IoC
	{
		internal static readonly Func<CompositionContainer> CreateDefaultContainer;
		private static readonly Lock __lock = new Lock();
		private static CompositionContainer __container;
		private static IoCDebugger __debugger;
		private static bool __isDebugEnabled;

		static IoC()
		{
			CreateDefaultContainer = () =>
			{
				IEnumerable<ComposablePartCatalog> assemblyCatalogs =
					GetAssemblyList().Select<Assembly, ComposablePartCatalog>(assembly => new AssemblyCatalog(assembly));

				AggregateCatalog aggCatalog = new AggregateCatalog(assemblyCatalogs);

				return new CompositionContainer(aggCatalog);
			};
		}

		/// <summary>
		/// Gets the global container.
		/// </summary>
		/// <value>The global container.</value>
		/// <remarks>This property will eventuall become internal, so limit external use of it</remarks>
		public static CompositionContainer GlobalContainer
		{
			get
			{
				CompositionContainer container;
				TryGetOrCreateContainer(CreateDefaultContainer, out container);
				return container;
			}
		}

		/// <summary>
		/// Gets a value indicating whether thie global container initialized.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the global container initialized; otherwise, <c>false</c>.
		/// </value>
		public static bool IsGlobalContainerInitialized
		{
			get { return __container != null; }
		}

		/// <summary>
		/// Enables Debug logging if a debugger is attached.
		/// </summary>
		public static void EnableDebugger()
		{
			__isDebugEnabled = true;
			if ((Debugger.IsAttached) && (__container != null))
			{
				__debugger = new IoCDebugger(__container);
			}
		}

		/// <summary>
		/// Gets an instance of the specified type with the optional specified contractName.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static object Get(Type type, string contractName = null)
		{
			string contract = string.IsNullOrEmpty(contractName) ? AttributedModelServices.GetContractName(type) : contractName;
			CompositionContainer container = GlobalContainer;
			object export;
			using (__lock.ForWrite())
			{
				export = container.GetExportedValue<object>(contract);
			}
			if (export.GetType() != type)
			{
				throw new CompositionContractMismatchException();
			}
			return export;
		}

		/// <summary>
		/// Gets an instance of the specified type with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static T Get<T>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExportedValue<T>(contractName);
			}
		}

		/// <summary>
		/// Gets an instance of the specified type (with metadata) with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static Lazy<T, TMetadata> Get<T, TMetadata>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExport<T, TMetadata>(contractName);
			}
		}

		/// <summary>
		/// Gets all instances of the specified type with the optional specified contractName.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static IEnumerable<object> GetAll(Type type, string contractName = null)
		{
			string contract = string.IsNullOrEmpty(contractName) ? AttributedModelServices.GetContractName(type) : contractName;
			CompositionContainer container = GlobalContainer;
			IEnumerable<object> allExports;
			using (__lock.ForWrite())
			{
				allExports = container.GetExportedValues<object>(contract);
			}
			return allExports.Where(export => export.GetType() == type);
		}

		/// <summary>
		/// Gets all instances of the specified type with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static IEnumerable<T> GetAll<T>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExportedValues<T>(contractName);
			}
		}

		/// <summary>
		/// Gets all instances of the specified type (with metadata) with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static IEnumerable<Lazy<T, TMetadata>> GetAll<T, TMetadata>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExports<T, TMetadata>(contractName);
			}
		}

		/// <summary>
		///     This method can be used to initialize the global container in the case where the default container doesn't provide
		///     enough flexibility. 
		///     
		///     If this method is needed it should be called exactly once and as early as possible in the application host.
		/// </summary>
		/// <param name="container">
		///     <see cref="CompositionContainer"/> that should be used instead of the default global container.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="container"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     Either <see cref="Initialize(CompositionContainer)" /> or <see cref="Initialize(ComposablePartCatalog[])" />
		///     has already been called or someone has already made use of the global container. In either case you need to 
		///		ensure that it  is called only once and that it is called early in the application host startup code.
		/// </exception>
		public static void Initialize(CompositionContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			CompositionContainer globalContainer;
			bool alreadyCreated = TryGetOrCreateContainer(() => container, out globalContainer);

			if (alreadyCreated)
			{
				throw new InvalidOperationException("The global container is already initialized.");
			}
		}

		/// <summary>
		///     This method can be used to initialize the global container in the case where the default container doesn't provide
		///     enough flexibility. 
		///     
		///     If this method is needed it should be called exactly once and as early as possible in the application host.
		/// </summary>
		/// <param name="catalogs">
		///     An array of <see cref="ComposablePartCatalog"/> that should be used to initialize the 
		///     <see cref="CompositionContainer"/> with.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="catalogs"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     Either <see cref="Initialize(CompositionContainer)" /> or <see cref="Initialize(ComposablePartCatalog[])" />
		///     has already been called or someone has already made use of the global container. In either case you need to 
		///		ensure that it  is called only once and that it is called early in the application host startup code.
		/// </exception>
		public static CompositionContainer Initialize(params ComposablePartCatalog[] catalogs)
		{
			AggregateCatalog aggregateCatalog = new AggregateCatalog(catalogs);
			CompositionContainer container = new CompositionContainer(aggregateCatalog);
			try
			{
				Initialize(container);
			}
			catch
			{
				container.Dispose();

				// NOTE : this is important, as this prevents the disposal of the catalogs passed as input arguments
				aggregateCatalog.Catalogs.Clear();
				aggregateCatalog.Dispose();

				throw;
			}

			return container;
		}

		/// <summary>
		/// Registers the specified value in the composition container.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="exportedValue">The exported value.</param>
		/// <param name="contractName">Name of the contract.</param>
		public static void Register<T>(T exportedValue, string contractName = null)
		{
			string contract = string.IsNullOrEmpty(contractName) ? AttributedModelServices.GetContractName(typeof(T)) : contractName;
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				container.ComposeExportedValue(contract, exportedValue);
			}
		}

		/// <summary>
		/// Resets the container instance to null to support testing with multiple configurations in the same unit test batch.
		/// </summary>
		public static void ResetContainer()
		{
			if (__debugger != null)
			{
				__debugger.Close();
			}
			__debugger = null;
			__container = null;
		}

		/// <summary>
		///     Will satisfy the imports on a object instance based on a global <see cref="CompositionContainer"/>. 
		///		By default if no <see cref="CompositionContainer"/> is registered the first time this is called it will be 
		///		initialized to a catalog that contains all the assemblies loaded by the initial application XAP or in the
		///		current AppDomain.
		/// </summary>
		/// <param name="attributedPart">
		///     Object instance that contains <see cref="ImportAttribute"/>s that need to be satisfied.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="attributedPart"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="attributedPart"/> contains <see cref="ExportAttribute"/>s applied on its type.
		/// </exception>
		/// <exception cref="ChangeRejectedException">
		///     One or more of the imports on the object instance could not be satisfied.
		/// </exception>
		/// <exception cref="CompositionException">
		///     One or more of the imports on the object instance caused an error while composing.
		/// </exception>
		public static void SatisfyImports(object attributedPart)
		{
			if (attributedPart == null)
			{
				throw new ArgumentNullException("attributedPart");
			}
			ComposablePart part = AttributedModelServices.CreatePart(attributedPart);
			SatisfyImports(part);
		}

		/// <summary>
		///     Will satisfy the imports on a object instance based on a global <see cref="CompositionContainer"/>. 
		///		By default if no <see cref="CompositionContainer"/> is registered the first time this is called it will be 
		///		initialized to a catalog that contains all the assemblies loaded by the initial application XAP or in the
		///		current AppDomain.
		/// </summary>
		/// <param name="part">
		///     Part with imports that need to be satisfied.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="part"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		///     <paramref name="part"/> contains <see cref="ExportAttribute"/>s applied on its type.
		/// </exception>
		/// <exception cref="ChangeRejectedException">
		///     One or more of the imports on the object instance could not be satisfied.
		/// </exception>
		/// <exception cref="CompositionException">
		///     One or more of the imports on the object instance caused an error while composing.
		/// </exception>
		public static void SatisfyImports(ComposablePart part)
		{
			if (part == null)
			{
				throw new ArgumentNullException("part");
			}

			CompositionBatch batch = new CompositionBatch();

			batch.AddPart(part);

			if (part.ExportDefinitions.Any())
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Type {0} has unsatisfied exports", part), "part");
			}

			GlobalContainer.Compose(batch);
		}

		/// <summary>
		/// Tries to get an instance of the specified type with the optional specified contractName.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static object TryGet(Type type, string contractName = null)
		{
			string contract = string.IsNullOrEmpty(contractName) ? AttributedModelServices.GetContractName(type) : contractName;
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExportedValueOrDefault<object>(contract);
			}
		}

		/// <summary>
		/// Tries to get an instance of the specified type with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static T TryGet<T>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExportedValueOrDefault<T>(contractName);
			}
		}

		/// <summary>
		/// Tries to get an instance of the specified type (with metadata) with the optional specified contractName.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns></returns>
		public static Lazy<T, TMetadata> TryGet<T, TMetadata>(string contractName = null)
		{
			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				return container.GetExports<T, TMetadata>(contractName).SingleOrDefault();
			}
		}

		/// <summary>
		/// Tries the satisfy imports.
		/// </summary>
		/// <param name="attributedPart">The attributed part.</param>
		public static void TrySatisfyImports(object attributedPart)
		{
			if (attributedPart == null)
			{
				throw new ArgumentNullException("attributedPart");
			}
			ComposablePart part = AttributedModelServices.CreatePart(attributedPart);
			TrySatisfyImports(part);
		}

		/// <summary>
		/// Tries the satisfy imports.
		/// </summary>
		/// <param name="part">The part.</param>
		public static void TrySatisfyImports(ComposablePart part)
		{
			if (part == null)
			{
				throw new ArgumentNullException("part");
			}

			CompositionBatch batch = new CompositionBatch();

			batch.AddPart(part);

			if (part.ExportDefinitions.Any())
			{
				return;
			}

			CompositionContainer container = GlobalContainer;
			using (__lock.ForWrite())
			{
				try
				{
					container.Compose(batch);
				}
				catch {}
			}
		}

		internal static bool TryGetOrCreateContainer(Func<CompositionContainer> createContainer, out CompositionContainer globalContainer)
		{
			bool alreadyCreated = true;
			if (__container == null)
			{
				CompositionContainer container = createContainer();
				using (__lock.ForWrite())
				{
					if (__container == null)
					{
						Thread.MemoryBarrier();
						__container = container;
						alreadyCreated = false;
						if ((Debugger.IsAttached) && (__isDebugEnabled))
						{
							__debugger = new IoCDebugger(__container);
						}
					}
				}
			}
			globalContainer = __container;
			return alreadyCreated;
		}

		/// <summary>
		/// Gets the assembly list. This method is the only Silverlight specific code dependency in CompositionHost
		/// </summary>
		/// <returns></returns>
		private static IEnumerable<Assembly> GetAssemblyList()
		{
#if SILVERLIGHT

	// While this may seem like somewhat of a hack, walking the AssemblyParts in the active 
	// deployment object is the only way to get the list of assemblies loaded by the XAP. 
	// Keep in mind that calling Load on an assembly that is already loaded will
	// be a no-op and simply return the already loaded assembly object.
			return (from part in Deployment.Current.Parts
			        let resourceInfo = Application.GetResourceStream(new Uri(part.Source, UriKind.Relative))
			        where resourceInfo != null
			        select part.Load(resourceInfo.Stream)).ToList();
#else
			return AppDomain.CurrentDomain.GetAssemblies();
#endif
		}
	}
}