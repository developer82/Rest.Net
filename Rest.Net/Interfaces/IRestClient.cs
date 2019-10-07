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
        /// Get or Set an authenticator to easyly implement authntication with an API
        /// </summary>
        /// <value>
        /// The authenticator instance
        /// </value>
        IAuthentication Authentication { get; set; }

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

        /// <summary>
        /// Executes a specified request asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <param name="anonymousTypeObject">Anonymous object to return type.</param>
        /// <returns></returns>
        Task<IRestResponse<T>> ExecuteAsync<T>(IRestRequest request, T anonymousTypeObject);
        #endregion

        #region GET
        Task<IRestResponse<string>> GetAsync(string path, bool requiresAuthentication);

        Task<IRestResponse<T>> GetAsync<T>(string path, bool requiresAuthentication);

        Task<IRestResponse<T>> GetAsync<T>(string path, T anonymousTypeObject, bool requiresAuthentication);

        Task<IRestResponse<T>> GetAsync<T>(string path, string innerProperty, bool requiresAuthentication);
        #endregion

        #region PUT
        Task<IRestResponse<string>> PutAsync(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> PutAsync<T>(string path, object body, T anonymousTypeObject, bool requiresAuthentication);
        Task<IRestResponse<T>> PutAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication);
        #endregion

        #region POST
        Task<IRestResponse<string>> PostAsync(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> PostAsync<T>(string path, object body,  T anonymousTypeObject, bool requiresAuthentication);
        Task<IRestResponse<T>> PostAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication);
        #endregion

        #region DELETE
        Task<IRestResponse<string>> DeleteAsync(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, bool requiresAuthentication);

        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, T anonymousTypeObject, bool requiresAuthentication);
        Task<IRestResponse<T>> DeleteAsync<T>(string path, object body, string innerProperty, bool requiresAuthentication);
        #endregion
    }
}
