using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace EdFi.Ods.Entities.Common
{
    public class ItemAddedEventArgs
    {
        public ItemAddedEventArgs(object item)
        {
            Item = item;
        }

        public object Item { get; private set; }
    }
    
    public class AddingItemEventArgs
    {
        public AddingItemEventArgs(object item)
        {
            Item = item;
        }

        public object Item { get; private set; }
        public bool Cancel { get; set; }
    }

    public delegate void AddingItemEventHandler(object sender, AddingItemEventArgs e);
    public delegate void ItemAddedEventHandler(object sender, ItemAddedEventArgs e);

    public class ListAdapterWithAddNotifications<T> : IList<T>
    {
        protected readonly IList<T> Source;

        public ListAdapterWithAddNotifications(IList<T> source) : this(source, null) { }

        public ListAdapterWithAddNotifications(IList<T> source, ItemAddedEventHandler addedEventHandler)
        : this (source, addedEventHandler, null) { }

        public ListAdapterWithAddNotifications(IList<T> source, ItemAddedEventHandler addedEventHandler,
            AddingItemEventHandler addingEventHandler)
        {
            this.Source = source;

            if (addingEventHandler != null)
            {
                this.AddingItem += addingEventHandler;

                // Fire events for existing source items, removing them as necessary
                source
                    .Where(x => !ShouldIncludeItem(x))
                    .ToList()
                    .ForEach(x => source.Remove(x));
            }

            if (addedEventHandler != null)
            {
                this.ItemAdded += addedEventHandler;

                // Fire events for existing source items
                foreach (var item in source)
                    OnItemAdded(item);
            }
        }

        public event ItemAddedEventHandler ItemAdded; 

        private void OnItemAdded(T item)
        {
            if (ItemAdded != null)
            {
                var args = new ItemAddedEventArgs(item);
                ItemAdded(this, args);
            }
        }

        public event AddingItemEventHandler AddingItem;

        private void OnAddingItem(T item, out bool cancel)
        {
            if (AddingItem != null)
            {
                var args = new AddingItemEventArgs(item);
                AddingItem(this, args);
                cancel = args.Cancel;
            }
            else
            {
                cancel = false;
            }
        }

        private bool ShouldIncludeItem(T item)
        {
            bool cancel;

            OnAddingItem(item, out cancel);

            return !cancel;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (!ShouldIncludeItem(item))
                return;

            Source.Add(item);

            OnItemAdded(item);
        }

        public void Clear()
        {
            Source.Clear();
        }

        public bool Contains(T item)
        {
            return Source.Contains((T) item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var item in Source)
                array[arrayIndex++] = item;
        }

        public virtual bool Remove(T item)
        {
            return Source.Remove((T) item);
        }

        public int Count
        {
            get { return Source.Count; }
        }

        public bool IsReadOnly
        {
            get { return Source.IsReadOnly; }
        }

        public int IndexOf(T item)
        {
            return Source.IndexOf((T) item);
        }

        public virtual void Insert(int index, T item)
        {
            Source.Insert(index, (T) item);
        }

        public virtual void RemoveAt(int index)
        {
            Source.RemoveAt(index);
        }

        public virtual T this[int index]
        {
            get { return Source[index]; }
            set { Source[index] = (T) value; }
        }
        
    }

    public class CovariantIListAdapterWithAddNotifications<TBase, TDerived> : CovariantIListAdapter<TBase, TDerived>//, INotifyCollectionChanged    //public class ObservableCovariantIListAdapter<TBase, TDerived> : CovariantIListAdapter<TBase, TDerived>, INotifyCollectionChanged
        where TDerived : TBase
    {
        public CovariantIListAdapterWithAddNotifications(IList<TDerived> source) : this(source, null) { }

        public CovariantIListAdapterWithAddNotifications(IList<TDerived> source, ItemAddedEventHandler addedEventHandler)
        : this (source, addedEventHandler, null) { }

        public CovariantIListAdapterWithAddNotifications(IList<TDerived> source, ItemAddedEventHandler addedEventHandler,
            AddingItemEventHandler addingEventHandler)
            : base(source)
        {
            if (addingEventHandler != null)
            {
                this.AddingItem += addingEventHandler;

                // Fire events for existing source items, removing them as necessary
                source
                    .Where(x => !ShouldIncludeItem(x))
                    .ToList()
                    .ForEach(x => source.Remove(x));
            }

            if (addedEventHandler != null)
            {
                this.ItemAdded += addedEventHandler;

                // Fire events for existing source items
                foreach (var item in source)
                    OnItemAdded(item);
            }
        }

        public override void Add(TBase item)
        {
            if (!ShouldIncludeItem(item))
                return;

            base.Add(item);

            OnItemAdded(item);
        }

        public event ItemAddedEventHandler ItemAdded;
        
        private void OnItemAdded(TBase item)
        {
            if (ItemAdded != null)
            {
                var args = new ItemAddedEventArgs(item);
                ItemAdded(this, args);
            }
        }

        public event AddingItemEventHandler AddingItem;

        private void OnAddingItem(TBase item, out bool cancel)
        {
            if (AddingItem != null)
            {
                var args = new AddingItemEventArgs(item);
                AddingItem(this, args);
                cancel = args.Cancel;
            }
            else
            {
                cancel = false;
            }
        }

        private bool ShouldIncludeItem(TBase item)
        {
            bool cancel;

            OnAddingItem(item, out cancel);

            return !cancel;
        }
    }

    public class CovariantIListAdapter<TBase, TDerived> : IList<TBase>
        where TDerived : TBase
    {
        protected readonly IList<TDerived> Source;

        public CovariantIListAdapter(IList<TDerived> source)
        {
            this.Source = source;
        }

        public IEnumerator<TBase> GetEnumerator()
        {
            foreach (var item in Source)
                yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(TBase item)
        {
            Source.Add((TDerived) item);
        }

        public virtual void Clear()
        {
            Source.Clear();
        }

        public bool Contains(TBase item)
        {
            return Source.Contains((TDerived) item);
        }

        public void CopyTo(TBase[] array, int arrayIndex)
        {
            foreach (var item in Source)
                array[arrayIndex++] = item;
        }

        public virtual bool Remove(TBase item)
        {
            return Source.Remove((TDerived) item);
        }

        public int Count
        {
            get { return Source.Count; }
        }

        public bool IsReadOnly
        {
            get { return Source.IsReadOnly; }
        }

        public int IndexOf(TBase item)
        {
            return Source.IndexOf((TDerived) item);
        }

        public virtual void Insert(int index, TBase item)
        {
            Source.Insert(index, (TDerived) item);
        }

        public virtual void RemoveAt(int index)
        {
            Source.RemoveAt(index);
        }

        public virtual TBase this[int index]
        {
            get { return Source[index]; }
            set { Source[index] = (TDerived) value; }
        }
    }
}