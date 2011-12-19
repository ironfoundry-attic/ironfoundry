using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudFoundry.Net.Types
{
    public interface IMergeable<T>
    {
        void Merge(T obj);
    }
}
