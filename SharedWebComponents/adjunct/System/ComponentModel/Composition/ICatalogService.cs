namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ICatalogService
	{
#if SILVERLIGHT
		/// <summary>
		/// Adds the xap.
		/// </summary>
		/// <param name="relativeUri">The relative URI.</param>
		/// <param name="progressAction">The progress action.</param>
		/// <param name="completedAction">The completed action.</param>
		/// <returns>The XapCatalog wrapping the xap</returns>
		XapCatalog AddXap(string relativeUri,
		                         Action<DownloadProgressChangedEventArgs> progressAction = null,
		                         Action<AsyncCompletedEventArgs> completedAction = null);

		/// <summary>
		/// Adds the xap.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="progressAction">The progress action.</param>
		/// <param name="completedAction">The completed action.</param>
		/// <returns>The XapCatalog wrapping the xap</returns>
		XapCatalog AddXap(Uri uri,
		                         Action<DownloadProgressChangedEventArgs> progressAction = null,
		                         Action<AsyncCompletedEventArgs> completedAction = null);

		/// <summary>
		/// Removes the xap.
		/// </summary>
		/// <param name="relativeUri">The relative URI.</param>
		void RemoveXap(string relativeUri);

		/// <summary>
		/// Removes the xap.
		/// </summary>
		/// <param name="uri">The URI.</param>
		void RemoveXap(Uri uri);
#else
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
#endif
	}
}