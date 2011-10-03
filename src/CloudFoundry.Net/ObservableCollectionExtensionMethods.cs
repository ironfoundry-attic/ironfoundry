using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;

namespace CloudFoundry.Net
{
    public static class ObservableCollectionExtensionMethods
    {
        public static void Synchronize<T>(this ObservableCollection<T> argThis, ObservableCollection<T> toSynchronizeWith, IEqualityComparer<T> comparer)
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
        }
    }
}
