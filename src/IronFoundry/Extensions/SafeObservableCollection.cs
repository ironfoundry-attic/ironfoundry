namespace System.Collections.ObjectModel
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using IronFoundry.Types;

    [Serializable]
    public class SafeObservableCollection<T> : ObservableCollection<T>
    {
        public SafeObservableCollection() : base() { }

        public SafeObservableCollection(IEnumerable<T> x ) : base(x) { }

        public SafeObservableCollection(List<T> x)
            : base(x) { }

        public void SafeAdd(T x)
        {
            base.Items.Add(x);
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, x));
        }        

        public void Synchronize(SafeObservableCollection<T> toSynchronizeWith, IEqualityComparer<T> comparer)
        {
            if (toSynchronizeWith != null)
            {
                var newItems = toSynchronizeWith.Except(this, comparer).ToList();
                var removeItems = this.Except(toSynchronizeWith, comparer).ToList();

                foreach (var item in newItems)
                {
                    this.Add(item);
                }

                foreach (var item in removeItems)
                {
                    var toRemove = this.SingleOrDefault((i) => comparer.Equals(i, item));
                    if (toRemove != null)
                    {
                        this.Remove(toRemove);
                    }
                }

                var existingItems = this.Intersect(toSynchronizeWith, comparer).ToList();
                foreach (var item in existingItems)
                {
                    var currentItem = this.SingleOrDefault((i) => comparer.Equals(i, item));
                    var newItem = toSynchronizeWith.SingleOrDefault((i) => comparer.Equals(i, item));
                    if (currentItem != null && newItem != null)
                    {
                        var mergable = currentItem as IMergeable<T>;
                        if (mergable != null)
                            mergable.Merge(newItem);
                        else
                            currentItem = newItem;
                    }
                    
                }
            }
        }
    }
}