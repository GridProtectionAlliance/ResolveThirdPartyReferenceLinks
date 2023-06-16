using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Xunit;
using Xunit.Abstractions;

namespace ResolveThirdPartyReferenceLinks.Tests
{
    public class XmlReadTest
    {
        private readonly ITestOutputHelper m_testOutputHelper;

        public XmlReadTest(ITestOutputHelper testOutputHelper)
        {
            m_testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ConfigReadTest()
        {
            using Stream? cfgStream = 
                Assembly.GetExecutingAssembly().GetManifestResourceStream("ResolveThirdPartyReferenceLinks.Tests.TestConfig.xml");

            new ResolveThirdPartyReferenceLinksComponent
                .Factory()
                .Create()
                .Initialize(
                    new XPathDocument(cfgStream)
                    .CreateNavigator()
                    .SelectSingleNode(
                        XPathExpression.Compile("//component")
                    )
                );
        }

        [Fact]
        public void UrlResolveTest()
        {
            BuildComponentCore? comp = new ResolveThirdPartyReferenceLinksComponent.Factory().Create();

            using Stream? cfgStream = 
                Assembly.GetExecutingAssembly().GetManifestResourceStream("ResolveThirdPartyReferenceLinks.Tests.TestConfig.xml");

            comp.Initialize(
                new XPathDocument(cfgStream)
                .CreateNavigator()
                .SelectSingleNode(
                    XPathExpression.Compile("//component")
                )
            );

            using Stream? testStream = 
                Assembly.GetExecutingAssembly().GetManifestResourceStream("ResolveThirdPartyReferenceLinks.Tests.ResolveTest.xml");

            XmlDocument doc = new();
            doc.Load(testStream);

            comp.Apply(doc, "T:Rhino.Geometry.Brep");
            comp.Apply(doc, "T:Autodesk.Revit.DB.Face");

            m_testOutputHelper.WriteLine(doc.OuterXml);
        }
    }
}