using System;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ResolveThirdPartyReferenceLinks.Providers
{
    public abstract class UrlProviderBase
    {
        [XmlAttribute("title")]
        public string? Title { get; set; }

        public class UrlProviderTargetMatcher
        {
            [XmlAttribute("pattern")]
            public string Pattern { get; set; } = default!;

            [XmlAttribute("fullyQualifiedMemberName")]
            public bool FullyQualifiedMemberName { get; set; } = true;
        }

        [XmlElement("targetMatcher")]
        public UrlProviderTargetMatcher TargetMatcher { get; set; } = default!;

        public class UrlProviderParameter
        {
            [XmlAttribute("name")]
            public string Name { get; set; } = default!;

            [XmlAttribute("default")]
            public string? DefaultValue { get; set; }

            public string? Value { get; set; }
        }

        [XmlArray("parameters")]
        [XmlArrayItem("parameter", typeof(UrlProviderParameter))]
        public Collection<UrlProviderParameter>? Parameters { get; set; }

        public virtual bool IsMatch(string target) => 
            TargetMatcher.Pattern is { } pattern && new Regex(pattern).IsMatch(target);

        public virtual string FormatTitle(string title)
        {
            if (TargetMatcher.FullyQualifiedMemberName)
                return title;

            int lastIndexOfDot = title.LastIndexOf(".", StringComparison.Ordinal);

            if (lastIndexOfDot > -1 && lastIndexOfDot < title.Length - 1)
                title = title.Substring(lastIndexOfDot + 1);

            return title;
        }

        public abstract (Uri, string target, string rel) CreateUrl(string target);
    }
}