using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    public interface IMergeable<T>
    {
        void Merge(T obj);
    }
}