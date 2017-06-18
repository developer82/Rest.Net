using System.Collections.Generic;
using System.Net;

namespace Rest.Net
{
    public class RestCollection : Dictionary<string, string>
    {
        public enum CollectionType
        {
            None,
            Header,
            QueryStringParameter
        }

        public string ContentType { get; private set; } = "text/plain";
        public string AuthorizationHeader { get; private set; }

        private readonly CollectionType _collectionType = CollectionType.None;

        public RestCollection()
        {
            
        }

        public RestCollection(CollectionType collectionType)
        {
            _collectionType = collectionType;
        }

        public void AddOrModify(string key, string value)
        {
            if (_collectionType == CollectionType.QueryStringParameter)
            {
                key = WebUtility.UrlEncode(key);
                value = WebUtility.UrlEncode(value);
            }

            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                Add(key, value);
            }

            if (_collectionType == CollectionType.Header)
            {
                string lowercaseName = key.ToLower();
                if (lowercaseName == "content-type")
                {
                    ContentType = value;
                }
                else if (lowercaseName == "authentication")
                {
                    AuthorizationHeader = key;
                }
            }
        }

        public void MergeWith(RestCollection mergee)
        {
            foreach (var item in mergee)
            {
                AddOrModify(item.Key, item.Value);
            }
        }
    }
}
