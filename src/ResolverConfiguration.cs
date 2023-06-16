using System.Xml.Serialization;
using System.Collections.ObjectModel;
using ResolveThirdPartyReferenceLinks.Providers;

namespace ResolveThirdPartyReferenceLinks
{
    [XmlRoot("configuration")]
    public class ResolverConfiguration
    {
        [XmlArray("urlProviders")]
        [XmlArrayItem("formattedProvider", typeof(FormattedUrlProvider))]
        [XmlArrayItem("dictProvider", typeof(DictionaryUrlProvider))]
        public Collection<UrlProviderBase>? UrlProviders { get; set; }
    }
}