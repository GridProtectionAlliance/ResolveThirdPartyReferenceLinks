using System;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ResolveThirdPartyReferenceLinks.Providers
{
    public class FormattedUrlProvider : UrlProviderBase
    {
        public class UrlFormatterAction
        {
            [XmlAttribute("format")]
            public string Format { get; set; } = default!;

            [XmlAttribute("target")]
            public string Target { get; set;} = "_blank";

            [XmlAttribute("rel")]
            public string Rel { get; set; } = "noreferrer";
        }

        public class UrlProviderTargetFormatter
        {
            public abstract class TargetFormatterStep
            {
                public abstract string Apply(string target);
            }

            public class TargetFormatterReplaceStep : TargetFormatterStep
            {
                [XmlAttribute("pattern")]
                public string Pattern { get; set; } = "T:";

                [XmlAttribute("with")]
                public string Replacement { get; set; } = "T_";

                public override string Apply(string target) =>
                    Pattern is { } pattern ? 
                        new Regex(pattern).Replace(target, Replacement) : 
                        target;
            }

            [XmlArray("steps")]
            [XmlArrayItem("replace", typeof(TargetFormatterReplaceStep))]
            public Collection<TargetFormatterStep>? Steps { get; set; }
        }

        [XmlElement("targetFormatter")]
        public UrlProviderTargetFormatter? TargetFormatter { get; set; }

        [XmlElement("urlFormatter")]
        public UrlFormatterAction? UrlFormatter { get; set; }

        public override (Uri, string target, string rel) CreateUrl(string target)
        {
            if (UrlFormatter?.Format is not { } urlFormat)
                return (new Uri(string.Empty), string.Empty, string.Empty);

            // generate title
            string formattedTarget = target;
                
            foreach (UrlProviderTargetFormatter.TargetFormatterStep step in TargetFormatter?.Steps ?? new Collection<UrlProviderTargetFormatter.TargetFormatterStep>())
                formattedTarget = step.Apply(formattedTarget);

            // generate url
            string url = urlFormat.Replace("{target}", formattedTarget);
                
            foreach (UrlProviderParameter param in Parameters ?? new Collection<UrlProviderParameter>())
                url = url.Replace($"{{{param.Name}}}", param.Value);

            return (new Uri(url), UrlFormatter.Target, UrlFormatter.Rel);
        }
    }
}