using System.Text;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IronFoundry.Models
{
    public class SystemServiceEqualityComparer : IEqualityComparer<SystemService>
    {
        public bool Equals(SystemService c1, SystemService c2)
        {
            return c1.Vendor.Equals(c2.Vendor);
        }

        public int GetHashCode(SystemService c)
        {
            return c.Vendor.GetHashCode();
        }
    }
}