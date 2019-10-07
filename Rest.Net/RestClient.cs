using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rest.Net.Exceptions;
using Rest.Net.Interfaces;

namespace Rest.Net
{
    /// <summary>
    /// Rest client for making http calls to a specific server
    /// </summary>
    public class RestClient : IRestClient
    {
        private bool _useStats = false;
        private long _totalTime = 0;
        private string _absolutePath = string.Empty;
        private Uri _baseUrl;

        public Uri BaseUrl => _baseUrl;
        public int RequestCount { get; private set; }
        public long AvgRequestTimeMs { get; private set; }
        public RestCollection Parameters { get; private set; } = new RestCollection(RestCollection.CollectionType.QueryStringParameter);
        public RestCollection Headers { get; private set; } = new RestCollection(RestCollection.CollectionType.Header);
        public IAuthentication Authentication { get; set; }

        public RestClient() { }

        public RestClient(string url)
        {
            SetBaseUrl(url);
        }

        public void SetBaseUrl(string url)
        {
            _baseUrl = new Uri(url);
            if (_baseUrl.AbsolutePath.Length > 1)
            {
                _absolutePath = _baseUrl.AbsolutePath;
            }
        }

        public void AddParameter(string name, string value)
        {
            Parameters.AddOrModify(name, value);
        }

        public void AddHeader(string name, string value)
        {
            Headers.AddOrModify(name, value);
        }

        public void UseStats()
        {
            _useStats = true;
        }

        #region ExecuteAsync
        public async Task<IRestResponse<string>> ExecuteAsync(IRestRequest request)
        {
            return await ExecuteAsync<string>(request);
        }

        public async Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request)
        {
            var response = new RestResponse<T>();

            if (request.RequiresAuthentication && Authentication != null)
            {
                await Authentication?.SetRequestAuthentication(request);
            }

            var stopwatch = StartStatsCount();
            var httpResponseMessage = await ExecuteRequest(request);
            StopStatsCount<T>(stopwatch, response);

            await SerializeResponse(request, httpResponseMessage, response);

            int statusCode = (int) httpResponseMessage.StatusCode;
            if (statusCode < 200 || statusCode > 300)
            {
                response.IsError = true;
            }

            response.StatusCode = httpResponseMessage.StatusCode;
            response.Code = statusCode;
            response.OriginalHttpResponseMessage = httpResponseMessage;
            
            return response;
        }

        public async Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, T anonymousTypeObject)
        {
            return await ExecuteAsync<T>(request);
        }
        #endregion

        #region GET
        public async Task<IRestResponse<string>> GetAsync(string path, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<string>(path, HttpMethod.Get, requiresAuthentication);
        }

        public async Task<IRestResponse<T>> GetAsync<T>(string path, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, requiresAuthentication);
        }

        public async Task<IRestResponse<T>> GetAsync<T>(string path, T anonymousTypeObject, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, requiresAuthentication);
        }

        public async Task<IRestResponse<T>> GetAsync<T>(string path, string innerProperty, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, requiresAuthentication, null, innerProperty);
        }
        #endregion

        #region PUT
        public async Task<IRestResponse<string>> PutAsync(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<string>(path, HttpMethod.Put, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PutAsync<T>(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PutAsync<T>(string path, object body, T anonymousTypeObject, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PutAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, requiresAuthentication, body, innerProperty);
        }
        #endregion

        #region POST
        public async Task<IRestResponse<string>> PostAsync(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<string>(path, HttpMethod.Post, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PostAsync<T>(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PostAsync<T>(string path, object body, T anonymousTypeObject, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> PostAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, requiresAuthentication, body, innerProperty);
        }
        #endregion

        #region DELETE
        public async Task<IRestResponse<string>> DeleteAsync(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<string>(path, HttpMethod.Delete, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, T anonymousTypeObject, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, requiresAuthentication, body, null);
        }

        public async Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication = true)
        {
            return await GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, requiresAuthentication, body, innerProperty);
        }
        #endregion

        private async Task<HttpResponseMessage> ExecuteRequest(IRestRequest request)
        {
            string queryString = CreateQueryStringFromRequest(request);

            string path = BuildPath(_absolutePath, request.Path);
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, path + queryString);

            httpRequestMessage.Content = request.Content;
            CreateHeadersFromRequest(request, httpRequestMessage);

            var handler = new HttpRedirectClientHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient httpClient = new HttpClient(handler);
            httpClient.BaseAddress = _baseUrl;
            return await httpClient.SendAsync(httpRequestMessage);
        }

        private string BuildPath(params string[] parts)
        {
            string result = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];

                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                if (part.StartsWith("/"))
                {
                    part = part.Substring(1, part.Length - 1);
                }
                result += "/" + part;
            }

            return result;
        }

        private string CreateQueryStringFromRequest(IRestRequest request)
        {
            string result = string.Empty;

            request.Parameters.MergeWith(Parameters);
            foreach (var parameter in request.Parameters)
            {
                result += $"{(string.IsNullOrEmpty(result) ? "?" : "&")}{parameter.Key}={parameter.Value}";
            }

            return result;
        }

        private void CreateHeadersFromRequest(IRestRequest request, HttpRequestMessage httpRequestMessage)
        {
            Headers.MergeWith(request.Headers);
            foreach (var header in Headers)
            {
                if (header.Key.ToLower() == "content-type") // we ignore this header in request headers because this header is part of the content that is sent to the server (see StringContent initialization in request)
                {
                    continue;
                }
                else if (header.Key.ToLower() == "authorization")
                {
                    string[] authHeader = header.Value.Split(' ');
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(authHeader[0], authHeader[1]);
                }
                else
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private string UrlEncodePath(string path)
        {
            if (path.IndexOf('?') == -1)
            {
                return path;
            }

            string[] urlQuerystringParts = path.Split('?');
            string[] qsParts = urlQuerystringParts[1].Split('&');

            string result = string.Empty;
            foreach (var qsPart in qsParts)
            {
                result += result == string.Empty ? "?" : "&";

                string key = qsPart.Substring(0, qsPart.IndexOf('=') + 1);
                result += key;

                string value = WebUtility.UrlEncode(qsPart.Remove(0, qsPart.IndexOf('=') + 1));
                result += value;
            }

            return urlQuerystringParts[0] + result;
        }

        private async Task<IRestResponse<T>> GenerateRestRequestAndExecute<T>(string path, HttpMethod method, bool requiresAuthentication = true, object body = null, string innerProperty = null)
        {
            path = UrlEncodePath(path);
            IRestRequest request = new RestRequest(path, method, innerProperty);
            if (body != null)
            {
                request.AddHeader("Content-Type", "application/json");
                string stringContent = JsonConvert.SerializeObject(body);
                request.SetContent(stringContent);
            }
            request.RequiresAuthentication = requiresAuthentication;

            return await ExecuteAsync<T>(request);
        }

        private async Task SerializeResponse<T>(IRestRequest request, HttpResponseMessage httpResponseMessage, RestResponse<T> response)
        {
            var typeOfT = typeof(T);
            var contentType =
                httpResponseMessage.Content.Headers.FirstOrDefault(h => h.Key.ToLower().Equals("content-type"));
            var responseBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
            var rawData = Encoding.UTF8.GetString(responseBytes);
            response.RawData = rawData;

            if (contentType.Key != null)
            {
                if (contentType.Value.First().StartsWith("application/json") && typeOfT.GetTypeInfo().IsClass)
                {
                    try
                    {
                        if (typeOfT == typeof(string))
                        {
                            response.Data = (T)Convert.ChangeType(rawData, typeof(T));
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(request.InnerProperty))
                            {
                                object resultObject = JsonConvert.DeserializeObject<T>(rawData);
                                response.Data = (T)resultObject;
                            }
                            else
                            {
                                using (var stringReader = new StringReader(rawData))
                                using (var jsonReader = new JsonTextReader(stringReader))
                                {
                                    while (jsonReader.Read())
                                    {
                                        if (jsonReader.TokenType == JsonToken.PropertyName &&
                                            (string)jsonReader.Value == request.InnerProperty)
                                        {
                                            jsonReader.Read();

                                            var serializer = new JsonSerializer();
                                            response.Data = serializer.Deserialize<T>(jsonReader);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        response.Exception = new SerializationException(typeOfT.ToString(), ex);
                        response.IsError = true;
                    }
                }
            }
            else
            {
                try
                {
                    response.Data = (T)Convert.ChangeType(rawData, typeof(T));
                }
                catch (Exception ex)
                {
                    response.Exception = new ConversionException(typeOfT.ToString(), ex);
                    response.IsError = true;
                }
            }
        }

        private Stopwatch StartStatsCount()
        {
            if (!_useStats) return null;

            RequestCount++;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        private void StopStatsCount<T>(Stopwatch stopwatch, RestResponse<T> response)
        {
            if (!_useStats) return;
            
            stopwatch.Stop();
            response.RequestTime = stopwatch.Elapsed;
            response.RequestTimeMs = stopwatch.ElapsedMilliseconds;
            _totalTime += stopwatch.ElapsedMilliseconds;
            AvgRequestTimeMs = Convert.ToInt64(_totalTime / RequestCount);
        }
    }
}
