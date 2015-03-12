
namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ExportFactory<T>
	{
		private readonly Func<Tuple<T, Action>> _exportLifetimeContextCreator;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExportFactory&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="exportLifetimeContextCreator">The export lifetime context creator.</param>
		public ExportFactory(Func<Tuple<T, Action>> exportLifetimeContextCreator)
		{
			if (exportLifetimeContextCreator == null)
			{
				throw new ArgumentNullException("exportLifetimeContextCreator");
			}
			_exportLifetimeContextCreator = exportLifetimeContextCreator;
		}

		/// <summary>
		/// Creates the export.
		/// </summary>
		/// <returns></returns>
		public ExportLifetimeContext<T> CreateExport()
		{
			Tuple<T, Action> untypedLifetimeContext = _exportLifetimeContextCreator.Invoke();
			return new ExportLifetimeContext<T>(untypedLifetimeContext.Item1, untypedLifetimeContext.Item2);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
	internal class ExportFactory<T, TMetadata> : ExportFactory<T>
	{
		private readonly TMetadata _metadata;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExportFactory&lt;T, TMetadata&gt;"/> class.
		/// </summary>
		/// <param name="exportLifetimeContextCreator">The export lifetime context creator.</param>
		/// <param name="metadata">The metadata.</param>
		public ExportFactory(Func<Tuple<T, Action>> exportLifetimeContextCreator, TMetadata metadata)
			: base(exportLifetimeContextCreator)
		{
			_metadata = metadata;
		}

		/// <summary>
		/// Gets the metadata.
		/// </summary>
		/// <value>The metadata.</value>
		public TMetadata Metadata
		{
			get { return _metadata; }
		}
	}
}