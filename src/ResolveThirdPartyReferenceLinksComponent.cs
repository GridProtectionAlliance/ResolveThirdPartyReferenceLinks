//#define INSPECT

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using ResolveThirdPartyReferenceLinks.Providers;
using System.Text.RegularExpressions;

namespace ResolveThirdPartyReferenceLinks
{
    public class ResolveThirdPartyReferenceLinksComponent : BuildComponentCore
    {
        // Build component factory for MEF
        [BuildComponentExport("Resolve ThirdParty Reference Links", IsVisible = true,
            Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
            Description = "This build component is used to resolve links to third-party documentation sources.\r\n" +
                          "NOTE: Configuration is currently managed manually in the '.shfbproj' file, see documentation and examples at:\r\n"+
                          "https://github.com/GridProtectionAlliance/ResolveThirdPartyReferenceLinks")]
        public sealed class Factory : BuildComponentFactory
        {
            public Factory()
            {
                ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.After, "Transform Component");
            }

            public override BuildComponentCore Create() => 
                new ResolveThirdPartyReferenceLinksComponent(BuildAssembler);
        }

        protected ResolveThirdPartyReferenceLinksComponent(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }

        private List<UrlProviderBase> Providers { get; } = new();

        // Abstract method implementations
        public override void Initialize(XPathNavigator configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            if (configuration.SelectSingleNode("configuration") is { } navigator)
            {
                using StringReader stringReader = new(navigator.OuterXml);
                using XmlReader xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });

                XmlSerializer serializer = new(typeof(ResolverConfiguration));
                ResolverConfiguration configs = (ResolverConfiguration)serializer.Deserialize(xmlReader);

                WriteMessage(MessageLevel.Info, $"Found {configs.UrlProviders?.Count ?? 0} providers...");

                foreach (UrlProviderBase provider in configs.UrlProviders ?? new Collection<UrlProviderBase>())
                {
                    foreach (UrlProviderBase.UrlProviderParameter param in provider.Parameters ?? new Collection<UrlProviderBase.UrlProviderParameter>())
                    {
                        if (configuration.SelectSingleNode(param.Name) is { } pathParam)
                            param.Value = pathParam.GetAttribute("value", string.Empty);
                    }

                    WriteMessage(MessageLevel.Info, $"Adding URL provider: {provider.Title ?? provider.ToString()}");
                    Providers.Add(provider);
                }
            }
            else
            {
                WriteMessage(MessageLevel.Error, "Config file is not provided.");
            }
        }

        public override void Apply(XmlDocument document, string key)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (key is null)
                throw new ArgumentNullException(nameof(key));

            // find all reference links in given document
            XPathNavigator[] referenceLinks =
                document.CreateNavigator()?.Select(XPathExpression.Compile("//referenceLink")).ToArray() ??
                Array.Empty<XPathNavigator>();

        #if DEBUG && INSPECT
            try
            {
                static string removeInvalidChars(string source) => 
                    Path.GetInvalidFileNameChars().Aggregate(source, (current, invalidChar) => 
                        current.Replace(invalidChar.ToString(), "_"));

                File.WriteAllText($"D:\\Projects\\ResolveThirdPartyReferenceLinks\\src\\bin\\Debug\\{removeInvalidChars(key)}.txt", 
                        string.Join(Environment.NewLine, referenceLinks.Select(link => link.GetAttribute("target", string.Empty))));

                File.WriteAllText($"D:\\Projects\\ResolveThirdPartyReferenceLinks\\src\\bin\\Debug\\{removeInvalidChars(key)}.html",
                    document.InnerXml);
            }
            catch (Exception ex)
            {
                WriteMessage(MessageLevel.Warn, $"Failed to export reference links for \"{key}\": {ex.Message}");
            }
        #endif

            // all reference links that are a direct descendant of a <td> element are member definitions
            // in the VS2013 and Default2022 Sandcastle presentation styles. Knowing this allows us to
            // determine which methods are overloads and specifically target the proper SHFB URL:
            Dictionary<string, int> overloadMethodCount = new();
            Dictionary<string, string> overloadMethodNames = new();

            foreach (XPathNavigator refLink in referenceLinks)
            {
                XPathNavigator parent = refLink.Clone();

                if (!parent.MoveToParent())
                    continue;

                // make sure parent node is a <td> element
                if (!string.Equals(parent.Name, "td", StringComparison.Ordinal))
                    continue;

                // make sure reference link is the first direct descendant of the <td> element
                parent.MoveToFirstChild();

                if (!parent.IsSamePosition(refLink))
                    continue;

                string? target = refLink.GetAttribute("target", string.Empty);

                if (string.IsNullOrEmpty(target))
                    continue;

                // remove any parameters from the target to get the base method name
                string baseMethodName = GetBaseMethodName(target);

                if (overloadMethodCount.TryGetValue(baseMethodName, out int count))
                    count++;

                overloadMethodCount[baseMethodName] = count;

                if (count > 0)
                    overloadMethodNames[target] = $"{target}_{count}";
            }

            WriteMessage(MessageLevel.Info, $"Resolving URL for {key}");

            foreach (XPathNavigator refLink in referenceLinks)
            {
                string? target = refLink.GetAttribute("target", string.Empty);

                if (string.IsNullOrEmpty(target))
                    continue;

                WriteMessage(MessageLevel.Info, $"Found reference link to {target}");

                // try to match target with a provider
                foreach (UrlProviderBase provider in Providers.Where(provider => provider.IsMatch(target)))
                {
                    bool sandcastleTarget = provider.TargetMatcher.SandcastleTarget;

                    WriteMessage(MessageLevel.Info, $"Converting reference link for {(sandcastleTarget ? "SHFB" : "external")} target: {target}");

                    // create title 
                    string title = target;

                    // format title for hyper-link and reduce to member name only, if requested
                    title = provider.FormatTitle(title, ex => WriteMessage(MessageLevel.Warn, $"Failed to format title: {ex.Message}"));

                    // check for SHFB method overloads
                    if (sandcastleTarget && overloadMethodNames.TryGetValue(target, out string overloadTarget))
                        target = overloadTarget;

                    // write hyper-link
                    WriteHrefFor(refLink, provider.CreateUrl(target), title);
                    break;
                }
            }
        }

        private static void WriteHrefFor(XPathNavigator linkNode, (Uri, string, string) anchorParams, string contents)
        {
            XmlWriter writer = linkNode.InsertAfter();

            (Uri uri, string target, string rel) = anchorParams;

            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", uri.AbsoluteUri);
            writer.WriteAttributeString("target", target);
            writer.WriteAttributeString("rel", rel);
            writer.WriteString(contents);
            writer.WriteEndElement();
            writer.Close();

            linkNode.DeleteSelf();
        }

    #if DEBUG
        public new void WriteMessage(MessageLevel level, string message, params object[] args)
        {
            base.WriteMessage(level, message, args);
            Console.WriteLine($"[{level}] {message}");
        }
    #endif

        private static readonly Regex s_baseMethodName = new(@"\(([^\)]*)\)", RegexOptions.Compiled);

        private static string GetBaseMethodName(string methodName) => 
            s_baseMethodName.Replace(methodName, string.Empty);
    }
}