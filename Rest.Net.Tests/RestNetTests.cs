using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rest.Net.Authenticators;
using Rest.Net.Interfaces;

namespace Rest.Net.Tests
{
    [TestClass]
    public class RestNetTests
    {
        public RestNetTests()
        {

        }

        [TestMethod]
        public void ShouldBeAbleToExecuteGetRequestWithSpecificPoperty()
        {
            RestClient client = new RestClient("https://dummyapi.io/api/");
            IRestResponse<List<User>> result = client.GetAsync<List<User>>("user", "data").Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBeGreaterThan(0);
            result.Data[0].ShouldNotBeNull();
        }

        [TestMethod]
        public void ShouldBeAbleToExecuteGetRequest()
        {
            RestClient client = new RestClient("https://jsonplaceholder.typicode.com/");
            IRestResponse<List<Post>> result = client.GetAsync<List<Post>>("posts").Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBeGreaterThan(0);
            result.Data[0].ShouldNotBeNull();
        }

        [TestMethod]
        public void ShouldBeAbleToExecuteGetRequestWithAnonymousDefinition()
        {
            var userDef = new
            {
                Id = 0,
                NameTitle = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Image = string.Empty
            };

            RestClient client = new RestClient("https://dummyapi.io/api");
            var result = client.GetAsync("user/1", userDef).Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBeEqualTo(1);
        }

        [TestMethod]
        public void ShouldBeAbleToExecutePostRequest()
        {
            var postDef = new
            {
                Id = 0
            };

            RestClient client = new RestClient("https://jsonplaceholder.typicode.com/");
            var result = client.PostAsync("posts", null, postDef).Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.Created);
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public void ShouldBeAbleToExecutePutRequest()
        {
            var postDef = new
            {
                Id = 0
            };

            RestClient client = new RestClient("https://jsonplaceholder.typicode.com/");
            var result = client.PutAsync("posts/1", null, postDef).Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public void ShouldBeAbleToExecuteDeleteRequest()
        {
            var postDef = new
            {
                Id = 0
            };

            RestClient client = new RestClient("https://jsonplaceholder.typicode.com/");
            var result = client.DeleteAsync("posts/1", null, postDef).Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
        }

        [TestMethod]
        public void ShouldBeAbleToUseOAuth2AuthenticatorUsingClientCredentialsFlow()
        {
            RestClient client = new RestClient("http://localhost:5000/");
            client.Authentication = new OAuth2Authenticator("http://localhost:5000", "client", "secret");
            IRestResponse<List<string>> result = client.GetAsync<List<string>>("/api/values/").Result;
            result = client.GetAsync<List<string>>("/api/values/").Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public void ShouldBeAbleToUseOAuth2AuthenticatorUsingPasswordFlow()
        {
            RestClient client = new RestClient("http://localhost:5000/");
            client.Authentication = new OAuth2Authenticator("http://localhost:5000", "ro.client", "secret", "alice", "password");
            IRestResponse <List<string>> result = client.GetAsync<List<string>>("/api/values/").Result;
            result = client.GetAsync<List<string>>("/api/values/").Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBeGreaterThan(0);
        }

        [TestMethod]
        public void ShouldBeAbleToUseOAuth2AuthenticatorUsingRefreshToken()
        {
            string refreshToken = GetRefreshToken("ro.client", "secret", "alice", "password");

            RestClient client = new RestClient("http://localhost:5000/");
            client.Authentication = new OAuth2Authenticator("http://localhost:5000", "ro.client", "secret", refreshToken);
            IRestResponse<List<string>> result = client.GetAsync<List<string>>("/api/values/").Result;
            result = client.GetAsync<List<string>>("/api/values/").Result;

            result.ShouldNotBeNull();
            result.IsError.ShouldBeFalse();
            result.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            result.Data.ShouldNotBeNull();
            result.Data.Count.ShouldBeGreaterThan(0);
        }

        private string GetRefreshToken(string clientId, string clientSecret, string username, string password)
        {
            IRestClient client = new RestClient("http://localhost:5000");
            IRestRequest authRequest = new RestRequest("/connect/token", Http.Method.POST);
            authRequest.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            authRequest.RequiresAuthentication = false;

            Dictionary<string, string> formContent = new Dictionary<string, string>()
            {
                {"grant_type", "password"},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"username", username},
                {"password", password}
            };

            var autoResponseDef = new
            {
                access_token = "",
                refresh_token = "",
                expires_in = "",
                error = ""
            };

            authRequest.SetContent(new FormUrlEncodedContent(formContent));
            var response = client.ExecuteAsync(authRequest, autoResponseDef).Result;

            response.ShouldNotBeNull();
            response.IsError.ShouldBeFalse();
            response.StatusCode.ShouldBeEqualTo(HttpStatusCode.OK);
            response.Data.ShouldNotBeNull();

            return response.Data.refresh_token;
        }
    }
}
