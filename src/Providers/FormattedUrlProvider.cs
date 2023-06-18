using System;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Linq;
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

            Collection<UrlProviderTargetFormatter.TargetFormatterStep> steps = TargetFormatter?.Steps ?? 
                new Collection<UrlProviderTargetFormatter.TargetFormatterStep>();

            // if target is for SHFB and no steps are defined, apply default steps
            if (TargetMatcher.SandcastleTarget && steps.Count == 0)
            {
                // Add default <replace pattern=":|\.|`" with="_" />
                steps.Add(new UrlProviderTargetFormatter.TargetFormatterReplaceStep
                {
                    Pattern = @":|\.|`",
                    Replacement = "_"
                });

                // Add default <replace pattern="\(([^\)]*)\)" with="" />
                steps.Add(new UrlProviderTargetFormatter.TargetFormatterReplaceStep
                {
                    Pattern = @"\(([^\)]*)\)",
                    Replacement = ""
                });
            }

            // apply formatting steps
            formattedTarget = steps.Aggregate(formattedTarget, 
                (current, step) => step.Apply(current));

            // generate url
            string url = urlFormat.Replace("{target}", formattedTarget);

            url = (Parameters ?? new Collection<UrlProviderParameter>()).Aggregate(url, 
                (current, param) => current.Replace($"{{{param.Name}}}", param.Value));

            return (new Uri(url), UrlFormatter.Target, UrlFormatter.Rel);
        }
    }
}