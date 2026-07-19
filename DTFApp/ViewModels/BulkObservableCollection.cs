using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DTFApp.ViewModels
{
    /// <summary>
    /// ObservableCollection that supports bulk updates with minimal
    /// INotifyCollectionChanged traffic. Preferred pattern:
    /// using (collection.DeferNotifications()) { ... }
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private int _bulkDepth;

        public BulkObservableCollection() { }

        public BulkObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public BulkObservableCollection(List<T> list) : base(list) { }

        /// <summary>
        /// Begins suppressing change notifications. Must be paired with
        /// a call to EndBulkOperation or a Dispose token.
        /// </summary>
        public void BeginBulkOperation()
        {
            _bulkDepth++;
        }

        /// <summary>
        /// Ends the current bulk operation and raises the deferred
        /// notification(s) if this is the outermost bulk scope.
        /// </summary>
        public void EndBulkOperation()
        {
            if (_bulkDepth == 0)
                throw new InvalidOperationException("No bulk operation is in progress.");

            _bulkDepth--;
            if (_bulkDepth == 0)
            {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Preferred API. Returns a token that ends the bulk operation when disposed.
        /// </summary>
        public BulkOperationDisposable DeferNotifications()
        {
            BeginBulkOperation();
            return new BulkOperationDisposable(this);
        }

        /// <summary>
        /// Add many items with one notification (batched Add). Much cheaper than
        /// BeginBulkOperation/EndBulkOperation for pure append scenarios.
        /// </summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var list = items as List<T> ?? items.ToList();
            if (list.Count == 0)
                return;

            int startIndex = Count;
            foreach (var item in list)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                (IList)list,
                startIndex));
        }

        /// <summary>
        /// Insert many contiguous items with one notification.
        /// </summary>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var list = items as List<T> ?? items.ToList();
            if (list.Count == 0)
                return;

            for (int i = list.Count - 1; i >= 0; i--)
                Items.Insert(index, list[i]);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                (IList)list,
                index));
        }

        /// <summary>
        /// Remove all occurrences of the specified items with one notification.
        /// Raises a batched Remove event if any items were removed.
        /// </summary>
        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var removed = new List<T>();
            foreach (var item in items)
            {
                if (Items.Remove(item))
                    removed.Add(item);
            }

            if (removed.Count == 0)
                return;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                (IList)removed,
                -1));
        }

        /// <summary>
        /// Replace the entire collection with one Reset notification instead of
        /// clearing then adding individual items.
        /// </summary>
        public void ReplaceAll(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void PerformBulkAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (DeferNotifications())
            {
                action();
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_bulkDepth == 0)
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_bulkDepth == 0)
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Lightweight disposable token for using statements. Struct in C# 7.3
        /// avoids an allocation per bulk scope.
        /// </summary>
        public readonly struct BulkOperationDisposable : IDisposable
        {
            private readonly BulkObservableCollection<T> _owner;

            internal BulkOperationDisposable(BulkObservableCollection<T> owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                _owner?.EndBulkOperation();
            }
        }
    }
}
