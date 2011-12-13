using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Extensions
{
    [Serializable]
    public class SafeObservableCollection<T> : ObservableCollection<T>
    {
        public SafeObservableCollection() : base()
        {
            
        }

        public SafeObservableCollection(IEnumerable<T> x ) : base(x)
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
    }
}
