using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rest.Net.Interfaces
{
    public interface IRestClient
    {
        /// <summary>
        /// Gets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        Uri BaseUrl { get; }

        int RequestCount { get; }

        long AvgRequestTimeMs { get; }

        /// <summary>
        /// Sets the base URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        void SetBaseUrl(string url);

        /// <summary>
        /// Executes a specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>IRestResponse&lt;string&gt;</returns>
        IRestResponse<string> Execute(IRestRequest request);

        /// <summary>
        /// Executes a specified request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns>IRestResponse&lt;T&gt;</returns>
        IRestResponse<T> Execute<T>(IRestRequest request);

        /// <summary>
        /// Executes a specified request asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IRestResponse<string>> ExecuteAsync(IRestRequest request);

        /// <summary>
        /// Executes a specified request asynchronously with cancellation token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IRestResponse<string>> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a specified request asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request);
        
        /// <summary>
        /// Executes a specified request asynchronously with cancellation token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, CancellationToken cancellationToken);
        
        /// <summary>
        /// Executes a specified request asynchronously with a callback function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        IRestRequestAsyncHandler ExecuteAsync<T>(IRestRequest request, Action<IRestResponse<T>> callback);

        /// <summary>
        /// Adds a global querystring parameter. If a parameter with the same name exists in the request it will override the global one.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void AddParameter(string name, string value);

        /// <summary>
        /// Adds a global header. If a header with the same name exists in the request it will override the global one.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        void AddHeader(string name, string value);

        /// <summary>
        /// Keep and return statistics on requests.
        /// </summary>
        void UseStats();

        IRestResponse<string> Get(string path);

        IRestResponse<T> Get<T>(string path);

        Task<IRestResponse<string>> GetAsync(string path);

        Task<IRestResponse<string>> GetAsync(string path, CancellationToken cancellationToken);

        Task<IRestResponse<T>> GetAsync<T>(string path);

        Task<IRestResponse<T>> GetAsync<T>(string path, CancellationToken cancellationToken);

        IRestRequestAsyncHandler GetAsync<T>(string path, Action<IRestResponse<T>> callback);

        IRestResponse<string> Put(string path, object body);

        IRestResponse<T> Put<T>(string path, object body);

        Task<IRestResponse<string>> PutAsync(string path, object body);

        Task<IRestResponse<string>> PutAsync(string path, object body, CancellationToken cancellationToken);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body, CancellationToken cancellationToken);

        IRestRequestAsyncHandler PutAsync<T>(string path, object body, Action<IRestResponse<T>> callback);

        IRestResponse<string> Post(string path, object body);

        IRestResponse<T> Post<T>(string path, object body);

        Task<IRestResponse<string>> PostAsync(string path, object body);

        Task<IRestResponse<string>> PostAsync(string path, object body, CancellationToken cancellationToken);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body, CancellationToken cancellationToken);

        IRestRequestAsyncHandler PostAsync<T>(string path, object body, Action<IRestResponse<T>> callback);

        IRestResponse<string> Delete(string path, object body);

        IRestResponse<T> Delete<T>(string path, object body);

        Task<IRestResponse<string>> DeleteAsync(string path, object body);

        Task<IRestResponse<string>> DeleteAsync(string path, object body, CancellationToken cancellationToken);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, CancellationToken cancellationToken);

        IRestRequestAsyncHandler DeleteAsync<T>(string path, object body, Action<IRestResponse<T>> callback);
    }
}
