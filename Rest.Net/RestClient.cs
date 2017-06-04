using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rest.Net.Exceptions;
using Rest.Net.Interfaces;

namespace Rest.Net
{
    public class RestClient : IRestClient
    {
        public Uri BaseUrl => _httpClient.BaseAddress;

        public int RequestCount { get; private set; }
        public long AvgRequestTimeMs { get; private set; }

        public RestCollection Parameters { get; private set; } = new RestCollection(RestCollection.CollectionType.QueryStringParameter);
        public RestCollection Headers { get; private set; } = new RestCollection(RestCollection.CollectionType.Header);

        private readonly HttpClient _httpClient = new HttpClient();
        private bool _useStats = false;
        private long _totalTime = 0;
        
        public RestClient()
        {

        }

        public RestClient(string url)
        {
            _httpClient.BaseAddress = new Uri(url);
        }

        public void SetBaseUrl(string url)
        {
            _httpClient.BaseAddress = new Uri(url);
        }

        public IRestResponse<string> Execute(IRestRequest request)
        {
            return Execute<string>(request);
        }

        public IRestResponse<T> Execute<T>(IRestRequest request)
        {
            var responseTask = ExecuteAsync<T>(request);
            return responseTask.Result;
        }

        public Task<IRestResponse<string>> ExecuteAsync(IRestRequest request)
        {
            return ExecuteAsync<string>(request);
        }

        public Task<IRestResponse<string>> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken)
        {
            return ExecuteAsync<string>(request, cancellationToken);
        }

        public Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request)
        {
            var cancellationToken = new CancellationToken();
            return ExecuteAsync<T>(request, cancellationToken);
        }

        public IRestRequestAsyncHandler ExecuteAsync<T>(IRestRequest request, Action<IRestResponse<T>> callback)
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            RestRequestAsyncHandler<T> requestAsyncHandler = new RestRequestAsyncHandler<T>
            {
                ExecutionTask = ExecuteAsync<T>(request, cancellationToken),
                TokenSource = tokenSource
            };

            requestAsyncHandler.ExecutionTask.ContinueWith((antecedent) =>
            {
                callback.Invoke(antecedent.Result);
            }, cancellationToken);

            return requestAsyncHandler;
        }

        public Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken)
        {
            Task<IRestResponse<T>> result = Task<IRestResponse<T>>.Factory.StartNew(() =>
            {
                var response = new RestResponse<T>();

                Stopwatch stopwatch = null;
                if (_useStats)
                {
                    RequestCount++;
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }

                var httpResponseMessage = ExecuteRequest(request).Result;

                if (_useStats)
                {
                    stopwatch.Stop();
                    response.RequestTime = stopwatch.Elapsed;
                    response.RequestTimeMs = stopwatch.ElapsedMilliseconds;
                    _totalTime += stopwatch.ElapsedMilliseconds;
                    AvgRequestTimeMs = Convert.ToInt64(_totalTime / RequestCount);
                }
                
                var typeOfT = typeof(T);
                var responseBytes = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
                var rawData = Encoding.UTF8.GetString(responseBytes);

                var contentType =
                    httpResponseMessage.Content.Headers.FirstOrDefault(h => h.Key.ToLower().Equals("content-type"));

                if (contentType.Key != null)
                {
                    if (contentType.Value.First().StartsWith("application/json") && typeOfT.GetTypeInfo().IsClass)
                    {
                        try
                        {
                            object resultObject;
                            if (typeOfT != typeof(string))
                            {
                                resultObject = JsonConvert.DeserializeObject<T>(rawData);
                                response.Data = (T)resultObject;
                            }
                            else
                            {
                                response.Data = (T)Convert.ChangeType(rawData, typeof(T));
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

                int statusCode = (int) httpResponseMessage.StatusCode;
                if (statusCode < 200 || statusCode > 300)
                {
                    response.IsError = true;
                }

                response.RawData = rawData;
                response.StatusCode = httpResponseMessage.StatusCode;
                response.Code = statusCode;
                response.OriginalHttpResponseMessage = httpResponseMessage;
                
                return response;
            }, cancellationToken);

            return result;
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

        private Task<HttpResponseMessage> ExecuteRequest(IRestRequest request)
        {
            string queryString = CreateQueryStringFromRequest(request);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, request.Path + queryString);
            
            CreateHeadersFromRequest(request, httpRequestMessage);
            httpRequestMessage.Content = request.Content;

            var httpResponseMessage = _httpClient.SendAsync(httpRequestMessage);
            return httpResponseMessage;
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
                httpRequestMessage.Headers.Add(header.Key, header.Value);
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

        private Task<IRestResponse<T>> GenerateRestRequestAndExecute<T>(string path, HttpMethod method, object body, CancellationToken cancellationToken = default(CancellationToken), Action<IRestResponse<T>> callback = null)
        {
            path = UrlEncodePath(path);
            IRestRequest request = new RestRequest(path, method);
            if (body != null)
            {
                request.AddHeader("Content-Type", "application/json");
                string stringContent = JsonConvert.SerializeObject(body);
                request.AddContent(stringContent);
            }

            return ExecuteAsync<T>(request, cancellationToken);
        }
        
        public IRestResponse<string> Get(string path)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Get, null).Result;
        }

        public IRestResponse<T> Get<T>(string path)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, null).Result;
        }

        public Task<IRestResponse<string>> GetAsync(string path)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Get, null);
        }

        public Task<IRestResponse<string>> GetAsync(string path, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Get, null, cancellationToken);
        }

        public Task<IRestResponse<T>> GetAsync<T>(string path)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, null);
        }

        public Task<IRestResponse<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Get, null, cancellationToken);
        }

        public IRestRequestAsyncHandler GetAsync<T>(string path, Action<IRestResponse<T>> callback)
        {
            throw new DidNotGetAroundToItOpenGithubIssueException();
        }

        public IRestResponse<string> Put(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Put, body).Result;
        }

        public IRestResponse<T> Put<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, body).Result;
        }

        public Task<IRestResponse<string>> PutAsync(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Put, body);
        }

        public Task<IRestResponse<string>> PutAsync(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Put, body, cancellationToken);
        }

        public Task<IRestResponse<T>> PutAsync<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, body);
        }

        public Task<IRestResponse<T>> PutAsync<T>(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Put, body, cancellationToken);
        }

        public IRestRequestAsyncHandler PutAsync<T>(string path, object body, Action<IRestResponse<T>> callback)
        {
            throw new DidNotGetAroundToItOpenGithubIssueException();
        }

        public IRestResponse<string> Post(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Post, body).Result;
        }

        public IRestResponse<T> Post<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, body).Result;
        }

        public Task<IRestResponse<string>> PostAsync(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Post, body);
        }

        public Task<IRestResponse<string>> PostAsync(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Post, body, cancellationToken);
        }

        public Task<IRestResponse<T>> PostAsync<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, body);
        }

        public Task<IRestResponse<T>> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Post, body, cancellationToken);
        }

        public IRestRequestAsyncHandler PostAsync<T>(string path, object body, Action<IRestResponse<T>> callback)
        {
            throw new DidNotGetAroundToItOpenGithubIssueException();
        }

        public IRestResponse<string> Delete(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Delete, body).Result;
        }

        public IRestResponse<T> Delete<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, body).Result;
        }

        public Task<IRestResponse<string>> DeleteAsync(string path, object body)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Delete, body);
        }

        public Task<IRestResponse<string>> DeleteAsync(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<string>(path, HttpMethod.Delete, body, cancellationToken);
        }

        public Task<IRestResponse<T>> DeleteAsync<T>(string path, object body)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, body);
        }

        public Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, CancellationToken cancellationToken)
        {
            return GenerateRestRequestAndExecute<T>(path, HttpMethod.Delete, body, cancellationToken);
        }

        public IRestRequestAsyncHandler DeleteAsync<T>(string path, object body, Action<IRestResponse<T>> callback)
        {
            throw new DidNotGetAroundToItOpenGithubIssueException();
        }
    }
}
