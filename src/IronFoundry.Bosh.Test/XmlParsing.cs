namespace IronFoundry.Bosh.Test
{
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using IronFoundry.Bosh.Properties;
    using Xunit;

    public class XmlParsing
    {
        [Fact]
        public void Can_Modify_Unattend_XML_File()
        {
            string newValue = "BLARGH";

            string unattendXml = Resources.UnattendXML;
            var xdoc = XDocument.Parse(unattendXml);
            XNamespace ns = xdoc.Root.GetDefaultNamespace();
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
            nsMgr.AddNamespace("ns", ns.NamespaceName);
            var element = xdoc.XPathSelectElement(@"/ns:unattend/ns:settings/ns:component/ns:ComputerName", nsMgr);
            element.Value = newValue;

            var ele = xdoc.Descendants().Single(x => x.Name.LocalName == "ComputerName");
            Assert.Equal(newValue, ele.Value);
        }
    }
}
