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

            [XmlAttribute("sandcastleTarget")]
            public bool SandcastleTarget { get; set; }
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

        public virtual string FormatTitle(string title, Action<Exception> handleException)
        {
            try
            {
                if (string.IsNullOrEmpty(title))
                    return string.Empty;

                // Remove type prefix
                int indexOfColon = title.IndexOf(":", StringComparison.Ordinal);
                string type = string.Empty;

                if (indexOfColon > -1)
                {
                    title = title.Substring(indexOfColon + 1);
                    type = title.Substring(0, indexOfColon);
                }

                // handle generic types
                int indexOfParen = title.IndexOf("(", StringComparison.Ordinal);
                string parameters = string.Empty;

                if (indexOfParen > -1)
                {
                    // Fix up generic type parameters
                    parameters = UpdateGenericType(title.Substring(indexOfParen).Replace('{', '<').Replace('}', '>'));
                    title = title.Substring(0, indexOfParen);
                }

                title = $"{RemoveGenericTypeSuffixes(title)}{parameters}";

                // Namespace titles should always be fully qualified
                if (type.Equals("N", StringComparison.OrdinalIgnoreCase) || TargetMatcher.FullyQualifiedMemberName)
                    return title;

                indexOfParen = title.IndexOf("(", StringComparison.Ordinal);
                parameters = string.Empty;

                if (indexOfParen > -1)
                {
                    parameters = title.Substring(indexOfParen + 1, title.Length - indexOfParen - 2);
                    title = title.Substring(0, indexOfParen);

                    string[] parameterList = parameters.Split(',');

                    for (int i = 0; i < parameterList.Length; i++)
                    {
                        string parameter = parameterList[i].Trim();
                        
                        int indexOfLessThan = parameter.IndexOf("<", StringComparison.Ordinal);
                        string generics = string.Empty;
                        
                        if (indexOfLessThan > -1)
                        {
                            generics = parameter.Substring(indexOfLessThan + 1, parameter.Length - indexOfLessThan - 2);
                            parameter = parameter.Substring(0, indexOfLessThan);

                            string[] genericList = generics.Split(',');

                            for (int j = 0; j < genericList.Length; j++)
                                genericList[j] = RemoveNamespace(genericList[j].Trim());

                            generics = string.Join(", ", genericList);
                        }

                        parameter = RemoveNamespace(parameter.Trim());

                        if (!string.IsNullOrEmpty(generics))
                            parameter += $"<{generics}>";

                        parameterList[i] = parameter;
                    }

                    parameters = string.Join(", ", parameterList);
                }

                title = RemoveNamespace(title);

                if (!string.IsNullOrEmpty(parameters))
                    title += $"({parameters})";
            }
            catch (Exception ex)
            {
                handleException(ex);
            }

            return title;
        }

        private static readonly Regex s_updateGenericType = new(@"`(\d+)", RegexOptions.Compiled);
        private static readonly Regex s_findGenericType = new(@"`\d+", RegexOptions.Compiled);

        private static string UpdateGenericType(string input) =>
            s_updateGenericType.Replace(input, match => $"T{(int.Parse(match.Groups[1].Value) + 1)}");

        private static string RemoveGenericTypeSuffixes(string input) =>
            s_findGenericType.Replace(input, string.Empty);
        
        private static string RemoveNamespace(string memberName)
        {
            int lastIndexOfDot = memberName.LastIndexOf(".", StringComparison.Ordinal);

            if (lastIndexOfDot > -1 && lastIndexOfDot < memberName.Length - 1)
                memberName = memberName.Substring(lastIndexOfDot + 1);

            return memberName;
        }

        public abstract (Uri, string target, string rel) CreateUrl(string target);
    }
}