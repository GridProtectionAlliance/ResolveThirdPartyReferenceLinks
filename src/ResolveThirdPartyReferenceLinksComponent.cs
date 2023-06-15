using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Resources;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using ResolveThirdPartyReferenceLinks.Providers;
using Sandcastle.Core;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en")]

namespace ResolveThirdPartyReferenceLinks
{
    public class ResolveThirdPartyReferenceLinksComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        [BuildComponentExport("Resolve ThirdParty Reference Links", IsVisible = true)]
        public sealed class Factory : BuildComponentFactory
        {
            public Factory()
            {
                ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.After, "Transform Component");
            }

            public override BuildComponentCore Create()
            {
                return new ResolveThirdPartyReferenceLinksComponent(this.BuildAssembler);
            }
        }
        #endregion

        #region Constructor
        protected ResolveThirdPartyReferenceLinksComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Abstract method implementations
        List<UrlProviderBase> Providers { get; } = new List<UrlProviderBase>();

        public override void Initialize(XPathNavigator configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (configuration.SelectSingleNode("configuration") is XPathNavigator xconfigs)
            {
                using (StringReader sread = new StringReader(xconfigs.OuterXml))
                using (XmlReader reader = XmlReader.Create(sread, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ResolverConfiguration));
                    ResolverConfiguration configs = (ResolverConfiguration)serializer.Deserialize(reader);

                    WriteMessage(MessageLevel.Info, $"Found {configs.UrlProviders?.Count ?? 0} providers...");
                    foreach (var provider in configs.UrlProviders ?? new Collection<UrlProviderBase>())
                    {
                        foreach (var param in provider.Parameters ?? new Collection<UrlProviderBase.UrlProviderParameter>())
                            if (configuration.SelectSingleNode(param.Name) is XPathNavigator xparam)
                                param.Value = xparam.GetAttribute("value", string.Empty);

                        WriteMessage(MessageLevel.Info, $"Adding URL provider: {provider.Title ?? provider.ToString()}");
                        Providers.Add(provider);
                    }
                }
            }
            else
                Console.WriteLine("Config file is not provided.");
        }

        public override void Apply(XmlDocument document, string key)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            // find all reference links in given document
            XPathNavigator[] referenceLinks =
                document.CreateNavigator().Select(XPathExpression.Compile("//referenceLink")).ToArray();

            WriteMessage(MessageLevel.Info, $"Resolving URL for {key}");

            foreach (XPathNavigator refLink in referenceLinks)
            {
                string target = refLink.GetAttribute("target", string.Empty);
                WriteMessage(MessageLevel.Info, $"Found reference link to {target}");

                // try to match target with a provider
                foreach (UrlProviderBase provider in Providers)
                    if (provider.IsMatch(target))
                    {
                        WriteMessage(MessageLevel.Info, $"Converting reference link for {target}");

                        // create title for hyperlink
                        string title = target;
                        if (title.IndexOf(":", StringComparison.OrdinalIgnoreCase) is int index && index > -1)
                            title = title.Substring(index + 1);

                        // write hyperlink
                        WriteHrefFor(refLink, provider.CreateUrl(target), title);
                        break;
                    }
            }
        }

        static void WriteHrefFor(XPathNavigator linkNode, Uri uri, string contents)
        {
            var writer = linkNode.InsertAfter();
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", uri.AbsoluteUri);
            writer.WriteAttributeString("target", "_blank");
            writer.WriteAttributeString("rel", "noopener noreferrer");
            writer.WriteString(contents);
            writer.WriteEndElement();
            writer.Close();
            linkNode.DeleteSelf();
        }

#if DEBUG
#pragma warning disable
        public new void WriteMessage(MessageLevel level, string message, params object[] _) => Console.WriteLine($"[{level}] {message}");
#pragma warning restore
#endif
        #endregion
    }
}
