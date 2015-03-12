using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	internal class XElementExportProvider : ExportProvider
	{
		private readonly Dictionary<string, Export> _exportsDictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="XElementExportProvider"/> class.
		/// Includes *.xml and *.config by default.
		/// </summary>
		/// <param name="path">The path. Defaults to Directory.GetCurrentDirectory()</param>
		/// <param name="filters">A list of additional file filters to include.</param>
		public XElementExportProvider(string path = null, IEnumerable<string> filters = null)
		{
			if (path == null)
			{
				path = Directory.GetCurrentDirectory();
			}
			List<string> include = new List<string>(new[] { "*.xml", "*.config" });
			if (filters != null)
			{
				foreach (string filter in filters.Where(filter => !string.IsNullOrWhiteSpace(filter)).Where(filter => !include.Contains(filter)))
				{
					include.Add(filter);
				}
			}

			List<string> xmlFiles = new List<string>(include.SelectMany(ext => Directory.GetFiles(path, ext)));

			_exportsDictionary = xmlFiles.Select(filePath => new FileInfo(filePath)).ToDictionary(
				fileInfo => fileInfo.Name,
				fileInfo =>
				{
					ExportDefinition def = new ExportDefinition(fileInfo.Name, null);
					Export e = new Export(def, () => XElement.Load(fileInfo.FullName));
					return e;
				});
		}

		/// <summary>
		/// Returns all exports that match the constraint defined by the specified definition.
		/// </summary>
		/// <param name="definition">The <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition"/> that defines the conditions of the
		/// <see cref="T:System.ComponentModel.Composition.Primitives.Export"/> objects to return.</param>
		/// <param name="atomicComposition"></param>
		/// <returns></returns>
		/// <result>
		/// An <see cref="T:System.Collections.Generic.IEnumerable`1"/> of <see cref="T:System.ComponentModel.Composition.Primitives.Export"/> objects that match
		/// the conditions defined by <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition"/>, if found; otherwise, an
		/// empty <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
		///   </result>
		protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
		{
			List<Export> exports = new List<Export>();
			if (_exportsDictionary.ContainsKey(definition.ContractName))
			{
				exports.Add(_exportsDictionary[definition.ContractName]);
			}
			return exports;
		}
	}
}