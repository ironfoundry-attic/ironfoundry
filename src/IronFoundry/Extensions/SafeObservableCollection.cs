using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronFoundry.Extensions
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using Models;

    [Serializable]
    public class SafeObservableCollection<T> : ObservableCollection<T> where T : class
    {
        public SafeObservableCollection()
        {
        }

        public SafeObservableCollection(IEnumerable<T> x) : base(x)
        {
        }

        public SafeObservableCollection(List<T> x)
            : base(x)
        {
        }

        public void SafeAdd(T x)
        {
            base.Items.Add(x);
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, x));
        }

        public void Synchronize(SafeObservableCollection<T> toSynchronizeWith, IEqualityComparer<T> comparer)
        {
            if (toSynchronizeWith != null)
            {
                List<T> newItems = toSynchronizeWith.Except(this, comparer).ToList();
                List<T> removeItems = this.Except(toSynchronizeWith, comparer).ToList();

                foreach (T item in newItems)
                {
                    Add(item);
                }

                foreach (T item in removeItems)
                {
                    T toRemove = this.SingleOrDefault((i) => comparer.Equals(i, item));
                    if (toRemove != null)
                    {
                        Remove(toRemove);
                    }
                }

                List<T> existingItems = this.Intersect(toSynchronizeWith, comparer).ToList();
                foreach (T item in existingItems)
                {
                    T currentItem = this.SingleOrDefault((i) => comparer.Equals(i, item));
                    T newItem = toSynchronizeWith.SingleOrDefault((i) => comparer.Equals(i, item));
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