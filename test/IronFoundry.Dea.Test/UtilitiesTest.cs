using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace IronFoundry.Dea.Test
{
    public class UtiltiesTest
    {
        [Fact]
        public void GetFileSizeStringTest_Bytes()
        {
            long size = 1023;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1023B", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_KilobytesLower()
        {
            long size = 1024;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1K", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_KilobytesUpper()
        {
            long size = 1048575;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1023K", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_MegabytesLower()
        {
            long size = 1048576;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1M", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_MegabytesUpper()
        {
            long size = 1073741823;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1023M", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_GigabytesLower()
        {
            long size = 1073741824;
            var sizeString = Utility.GetFileSizeString(size);
            Assert.Equal("1G", sizeString);
        }        
    }
}
