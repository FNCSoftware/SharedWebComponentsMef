using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;

namespace System.ComponentModel.Composition
{
	/// <summary>
	///     Helper class for debugging MEF
	/// </summary>
	internal class IoCDebugger
	{
		private const string DbgMefCatalog = "MEF: Found catalog: {0}";
		private const string DbgMefKey = "      With key: {0} = {1}";
		private const string DbgMefPart = "MEF: Found part: {0}";
		private const string DbgMefWithExport = "   With export:";
		private const string DbgMefWithImport = "   With import: {0}";
		private const string DbgMefWithKey = "   With key: {0} = {1}";
		private const string MsgAddExport = "Added Export:";
		private const string MsgChangeContract = "Changed contracts:";
		private const string MsgRemovedExport = "Removed Export:";
		private readonly CompositionContainer _container;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="container">The container to debug</param>
		public IoCDebugger(CompositionContainer container)
		{
			_container = container;
			_container.ExportsChanged += ExportsChanged;
			DebugCatalog((AggregateCatalog)container.Catalog);
		}

		public void Close()
		{
			LogInfo("MEF Debugger shutting down.");
			_container.ExportsChanged -= ExportsChanged;
		}

		/// <summary>
		///     Debug the catalog
		/// </summary>
		/// <param name="srcCatalog">The source catalog</param>
		private void DebugCatalog(AggregateCatalog srcCatalog)
		{
			foreach (ComposablePartCatalog catalog in srcCatalog.Catalogs)
			{
				LogInfo(DbgMefCatalog, catalog);

				foreach (ComposablePartDefinition part in catalog.Parts)
				{
					LogInfo(DbgMefPart, part);

					if (part.Metadata != null)
					{
						foreach (string key in part.Metadata.Keys)
						{
							LogInfo(DbgMefWithKey, key, part.Metadata[key]);
						}
					}

					foreach (ImportDefinition import in part.ImportDefinitions)
					{
						LogInfo(DbgMefWithImport, import);
					}

					ParseExports(DbgMefWithExport, part.ExportDefinitions);

					foreach (ExportDefinition export in part.ExportDefinitions)
					{
						LogInfo("{0} {1}", DbgMefWithExport, export);

						if (export.Metadata == null)
						{
							continue;
						}

						foreach (string key in export.Metadata.Keys)
						{
							LogInfo(DbgMefKey, key, export.Metadata[key]);
						}
					}
				}
			}
		}

		private void ExportsChanged(object sender, ExportsChangeEventArgs args)
		{
			try
			{
				if (args.AddedExports != null)
				{
					ParseExports(MsgAddExport, args.AddedExports);
				}

				if (args.RemovedExports != null)
				{
					ParseExports(MsgRemovedExport, args.RemovedExports);
				}

				if (args.ChangedContractNames != null)
				{
					bool first = true;
					foreach (string contract in args.ChangedContractNames)
					{
						if (first)
						{
							LogInfo(MsgChangeContract);
							first = false;
						}
						LogInfo(" ==>{0}", contract);
					}
				}
			}
			catch (Exception ex)
			{
				LogWarn(ex.Message);
			}
		}

		/// <summary>
		///     Parse the exports
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="exports"></param>
		private void ParseExports(string tag, IEnumerable<ExportDefinition> exports)
		{
			foreach (ExportDefinition export in exports)
			{
				LogInfo("{0} {1}", tag, export);

				if (export.Metadata == null)
				{
					continue;
				}

				foreach (string key in export.Metadata.Keys)
				{
					LogInfo(DbgMefKey, key, export.Metadata[key]);
				}
			}
		}

		private static void LogInfo(string format, params object[] args)
		{
			Debug.WriteLine(string.Format("Info: " + format, args));
		}

		private static void LogWarn(string format, params object[] args)
		{
			Debug.WriteLine(string.Format("Warn: " + format, args));
		}
	}
}