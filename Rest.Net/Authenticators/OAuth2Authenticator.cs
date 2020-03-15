using Newtonsoft.Json;
using Rest.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Net.Authenticators
{
    public class OAuth2Authenticator : IAuthentication
    {
        public enum GrantType { Password, ClientCredentials, RefreshToken }

        private string
            _issuer,
            _grantType,
            _clientId,
            _clientSecret,
            _username,
            _password,
            _token,
            _refreshToken;

        private DateTime _tokenExpiration = DateTime.MinValue;

        public OAuth2Authenticator(string issuer, string clientId, string clientSecret, string token, GrantType grantType = GrantType.RefreshToken)
        {
            if (grantType == GrantType.RefreshToken)
            {
                _refreshToken = token;
            }
            else
            {
                _token = token;
            }
            Init(issuer, grantType, clientId, clientSecret);
        }

        public OAuth2Authenticator(string issuer, string clientId, string clientSecret)
        {
            Init(issuer, GrantType.ClientCredentials, clientId, clientSecret);
        }

        public OAuth2Authenticator(string issuer, string clientId, string clientSecret, string username, string password)
        {
            Init(issuer, GrantType.Password, clientId, clientSecret, username, password);
        }
        
        private void Init(string issuer, GrantType grantType, string clientId, string clientSecret, string username = null, string password = null)
        {
            EvalGrantType(grantType);
            _issuer = issuer;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
        }

        private void EvalGrantType(GrantType grantType)
        {
            switch (grantType)
            {
                case GrantType.Password:
                    _grantType = "password";
                    break;
                case GrantType.ClientCredentials:
                    _grantType = "client_credentials";
                    break;
                case GrantType.RefreshToken:
                    _grantType = "refresh_token";
                    break;
            }
        }

        public async Task SetRequestAuthentication(IRestRequest request)
        {
            if (_token == null || DateTime.Now > _tokenExpiration)
            {
                await Login();
            }

            request.SetAuthentication(Http.AuthenticationMethod.Bearer, _token);
        }

        public async Task<AuthResponse> Login()
        {
            IRestClient client = new RestClient(_issuer);
            IRestRequest authRequest = new RestRequest("/connect/token", Http.Method.POST);
            authRequest.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            authRequest.RequiresAuthentication = false;

            Dictionary<string, string> formContent = new Dictionary<string, string>()
            {
                {"grant_type", _grantType},
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"username", _username},
                {"password", _password},
                {"refresh_token", _refreshToken}
            };

            authRequest.SetContent(new FormUrlEncodedContent(formContent));
            var response = await client.ExecuteAsync<AuthResponse>(authRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Data.AccessToken))
            {
                _token = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
                _tokenExpiration = DateTime.Now.AddSeconds(response.Data.ExpiresIn - 60);
            }
            else
            {
                _token = null;
                _refreshToken = null;
                _tokenExpiration = DateTime.MinValue;
            }

            return response.Data;
        }
    }

    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        public string Error { get; set; }
    }
}
