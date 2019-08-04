using System;
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

        #region ExecuteAsync
        /// <summary>
        /// Executes a specified request asynchronously.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IRestResponse<string>> ExecuteAsync(IRestRequest request);

        /// <summary>
        /// Executes a specified request asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request);
        #endregion

        #region GET
        Task<IRestResponse<string>> GetAsync(string path);

        Task<IRestResponse<T>> GetAsync<T>(string path);

        Task<IRestResponse<T>> GetAsync<T>(string path, T anonymousTypeObject);

        Task<IRestResponse<T>> GetAsync<T>(string path, string innerProperty);
        #endregion

        #region PUT
        Task<IRestResponse<string>> PutAsync(string path, object body);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body, T anonymousTypeObject);
        #endregion

        #region POST
        Task<IRestResponse<string>> PostAsync(string path, object body);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body,  T anonymousTypeObject);
        #endregion

        #region DELETE
        Task<IRestResponse<string>> DeleteAsync(string path, object body);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, T anonymousTypeObject);
        #endregion
    }
}
