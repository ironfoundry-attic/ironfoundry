using System.Collections.Generic;
using System.Linq;
using IronFoundry.Types;

namespace IronFoundry.Extensions
{    
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IronFoundry.Types;

    public static class ObservableCollectionExtensionMethods
    {
        public static void Synchronize<T>(this SafeObservableCollection<T> argThis, SafeObservableCollection<T> toSynchronizeWith, IEqualityComparer<T> comparer)
        {
            if (toSynchronizeWith != null)
            {
                var newItems = toSynchronizeWith.Except(argThis, comparer).ToList();
                var removeItems = argThis.Except(toSynchronizeWith, comparer).ToList();
                
                foreach (var item in newItems)
                    argThis.Add(item);

                foreach (var item in removeItems)
                {
                    var toRemove = argThis.SingleOrDefault((i) => comparer.Equals(i, item));
                    if (toRemove != null)
                        argThis.Remove(toRemove);
                }

                var existingItems = argThis.Intersect(toSynchronizeWith, comparer).ToList();
                foreach (var item in existingItems)
                {
                    var currentItem = argThis.SingleOrDefault((i) => comparer.Equals(i, item));
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
