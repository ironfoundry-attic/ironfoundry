namespace IronFoundry.Test
{
    using Dea;
    using Xunit;

    public class UtiltiesTest
    {
        [Fact]
        public void GetFileSizeStringTest_Bytes()
        {
            var sizeString = Utility.GetFileSizeString(1023);
            Assert.Equal("1023B", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_KilobytesLower()
        {
            var sizeString = Utility.GetFileSizeString(1024);
            Assert.Equal("1K", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_KilobytesUpper()
        {
            var sizeString = Utility.GetFileSizeString(948575);
            Assert.Equal("926.34K", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_MegabytesLower()
        {
            var sizeString = Utility.GetFileSizeString(1048576);
            Assert.Equal("1M", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_MegabytesUpper()
        {
            var sizeString = Utility.GetFileSizeString(973741823);
            Assert.Equal("928.63M", sizeString);
        }

        [Fact]
        public void GetFileSizeStringTest_GigabytesLower()
        {
            var sizeString = Utility.GetFileSizeString(1073741824);
            Assert.Equal("1G", sizeString);
        }
    }
}