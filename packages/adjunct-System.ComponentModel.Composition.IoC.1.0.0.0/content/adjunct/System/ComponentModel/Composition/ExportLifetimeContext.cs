namespace System.ComponentModel.Composition
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class ExportLifetimeContext<T> : IDisposable
	{
		private readonly Action _disposeAction;
		private readonly T _value;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExportLifetimeContext&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="disposeAction">The dispose action.</param>
		public ExportLifetimeContext(T value, Action disposeAction)
		{
			_value = value;
			_disposeAction = disposeAction;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <value>The value.</value>
		public T Value
		{
			get { return _value; }
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (_disposeAction != null)
			{
				_disposeAction.Invoke();
			}
		}
	}
}