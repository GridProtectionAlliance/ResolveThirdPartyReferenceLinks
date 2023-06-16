using System;

namespace ResolveThirdPartyReferenceLinks.Providers
{
    public class DictionaryUrlProvider : UrlProviderBase
    {
        public override (Uri, string target, string rel) CreateUrl(string target)
        {
            throw new NotImplementedException();
        }
    }
}