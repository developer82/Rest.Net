using Rest.Net.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Rest.Net.Authenticators
{
    public class BasicAuthenticator : IAuthentication
    {
        private readonly string _token;

        public BasicAuthenticator(string username, string password)
        {
            _token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        }

        public Task SetRequestAuthentication(IRestRequest request)
        {
            request.SetAuthentication(Http.AuthenticationMethod.Basic, _token);
            return Task.FromResult(0);
        }
    }
}
