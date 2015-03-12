using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDirectoryCatalogService
	{
		/// <summary>
		/// Adds the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		void AddDirectory(string path);

		/// <summary>
		/// Adds the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="searchPattern">The search pattern.</param>
		void AddDirectory(string path, string searchPattern);

		/// <summary>
		/// Removes the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		void RemoveDirectory(string path);
	}

	/// <summary>
	/// 
	/// </summary>
	public sealed class DirectoryCatalogService : IDirectoryCatalogService
	{
		private readonly AggregateCatalog _aggregateCatalog;
		private readonly Dictionary<string, DirectoryCatalog> _catalogs = new Dictionary<string, DirectoryCatalog>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryCatalogService"/> class.
		/// </summary>
		/// <param name="aggregateCatalog">The aggregate catalog.</param>
		public DirectoryCatalogService(AggregateCatalog aggregateCatalog = null)
		{
			_aggregateCatalog = aggregateCatalog ?? new AggregateCatalog();
		}

		/// <summary>
		/// Adds the directory.
		/// </summary>
		/// <param name="path">The path.</param>
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

		/// <summary>
		/// Adds the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="searchPattern">The search pattern.</param>
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

		/// <summary>
		/// Removes the directory.
		/// </summary>
		/// <param name="path">The path.</param>
		public void RemoveDirectory(string path)
		{
			DirectoryCatalog catalog;
			if (_catalogs.TryGetValue(path, out catalog))
			{
				_aggregateCatalog.Catalogs.Remove(catalog);
			}
		}
	}
}