namespace System.Threading
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class Lock : IDisposable
	{
		/// <summary>
		/// Generates a disposable object encapsulating a read lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable ForRead()
		{
			return new ReadLock(this);
		}

		/// <summary>
		/// Generates a disposable object encapsulating an upgradable read lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable ForReadUpgradable()
		{
			return new UpgradableReadLock(this);
		}

		/// <summary>
		/// Generates a disposable object encapsulating a write lock.
		/// </summary>
		/// <returns></returns>
		public IDisposable ForWrite()
		{
			return new WriteLock(this);
		}

#if (!SILVERLIGHT)
		private readonly ReaderWriterLockSlim _slim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private int _isDisposed;

		private void EnterReadLock()
		{
			_slim.EnterReadLock();
		}

		private void EnterUpgradableReadLock()
		{
			_slim.EnterUpgradeableReadLock();
		}

		private void EnterWriteLock()
		{
			_slim.EnterWriteLock();
		}

		private void ExitReadLock()
		{
			_slim.ExitReadLock();
		}

		private void ExitUpgradableReadLock()
		{
			_slim.ExitUpgradeableReadLock();
		}

		private void ExitWriteLock()
		{
			_slim.ExitWriteLock();
		}

		void IDisposable.Dispose()
		{
			if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
			{
				_slim.Dispose();
			}
		}
#else
		// ReaderWriterLockSlim is not yet implemented on SilverLight
		// Satisfies our requirements until it is implemented
		private readonly object _theLock = new object();

		private void EnterReadLock()
		{
			Monitor.Enter(_theLock);
		}

		private void EnterUpgradableReadLock()
		{
			Monitor.Enter(_theLock);
		}

		private void EnterWriteLock()
		{
			Monitor.Enter(_theLock);
		}

		private void ExitReadLock()
		{
			Monitor.Exit(_theLock);
		}

		private void ExitUpgradableReadLock()
		{
			Monitor.Exit(_theLock);
		}

		private void ExitWriteLock()
		{
			Monitor.Exit(_theLock);
		}

		void IDisposable.Dispose() {}
#endif

		internal struct ReadLock : IDisposable
		{
			private readonly Lock _lock;
			private int _isDisposed;

			internal ReadLock(Lock @lock)
			{
				_isDisposed = 0;
				_lock = @lock;
				_lock.EnterReadLock();
			}

			void IDisposable.Dispose()
			{
				if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
				{
					_lock.ExitReadLock();
				}
			}
		}

		internal struct UpgradableReadLock : IDisposable
		{
			private readonly Lock _lock;
			private int _isDisposed;

			internal UpgradableReadLock(Lock @lock)
			{
				_isDisposed = 0;
				_lock = @lock;
				_lock.EnterUpgradableReadLock();
			}

			void IDisposable.Dispose()
			{
				if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
				{
					_lock.ExitUpgradableReadLock();
				}
			}
		}

		internal struct WriteLock : IDisposable
		{
			private readonly Lock _lock;
			private int _isDisposed;

			internal WriteLock(Lock @lock)
			{
				_isDisposed = 0;
				_lock = @lock;
				_lock.EnterWriteLock();
			}

			void IDisposable.Dispose()
			{
				if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
				{
					_lock.ExitWriteLock();
				}
			}
		}

	}
}