using System;
using System.Net.Http;
using System.Text;
using Rest.Net.Exceptions;
using Rest.Net.Interfaces;

namespace Rest.Net
{
    public class RestRequest : IRestRequest
    {
        public string Path { get; private set; }
        public string InnerProperty { get; }
        public bool RequiresAuthentication { get; set; }
        public RestCollection Parameters { get; private set; } = new RestCollection(RestCollection.CollectionType.QueryStringParameter);
        public RestCollection Headers { get; private set; } = new RestCollection(RestCollection.CollectionType.Header);
        public HttpMethod Method { get; }
        public HttpContent Content { get; private set; }
        
        public RestRequest(string path, Http.Method method, string innerProperty = null)
        {
            Path = path;
            Method = GetHttpMethodFromRequest(method);
            InnerProperty = innerProperty;
        }

        public RestRequest(string path, HttpMethod method, string innerProperty = null)
        {
            Path = path;
            Method = method;
            InnerProperty = innerProperty;
        }

        public void AddUrlSegment(string identifier, string value)
        {
            Path = Path.Replace(identifier, value);
        }

        public void AddParameter(string name, string value)
        {
            Parameters.AddOrModify(name, value);
        }

        public void AddHeader(string name, string value)
        {
            Headers.AddOrModify(name, value);
        }

        public void AddFile(string name, string path)
        {
            throw new DidNotGetAroundToItOpenGithubIssueException();
        }

        public void SetContent(string content)
        {
            SetContent(new StringContent(content, Encoding.UTF8, Headers.ContentType));
        }

        public void SetContent(object content, string contentType)
        {
            Headers.Add("Content-Type", contentType);
            Content = (HttpContent) content;
        }

        public void SetContent(StringContent stringContent)
        {
            Content = stringContent;
        }

        public void SetContent(FormUrlEncodedContent formUrlEncodedContent)
        {
            Content = formUrlEncodedContent;
        }

        public void SetAuthentication(Http.AuthenticationMethod authenticationMethod, string token)
        {
            // TODO: add support for more authntication types
            string authentication = null;

            switch (authenticationMethod)
            {
                case Http.AuthenticationMethod.Basic:
                    authentication = "Basic " + token;
                    break;
                
                case Http.AuthenticationMethod.Bearer:
                    authentication = "Bearer " + token;
                    break;
            }

            if (string.IsNullOrEmpty(Headers.AuthorizationHeader))
            {
                Headers.Add("Authorization", authentication);
            }
            else
            {
                Headers["Authorization"] = authentication;
            }
        }
        
        private HttpMethod GetHttpMethodFromRequest(Http.Method method)
        {
            switch (method)
            {
                case Http.Method.DELETE:
                    return HttpMethod.Delete;
                case Http.Method.GET:
                    return HttpMethod.Get;
                case Http.Method.HEAD:
                    return HttpMethod.Head;
                case Http.Method.OPTIONS:
                    return HttpMethod.Options;
                case Http.Method.POST:
                    return HttpMethod.Post;
                case Http.Method.PUT:
                    return HttpMethod.Put;
                case Http.Method.TRACE:
                    return HttpMethod.Trace;

                default:
                    return HttpMethod.Get;
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
