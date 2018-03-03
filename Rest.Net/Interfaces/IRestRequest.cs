using System.Net.Http;

namespace Rest.Net.Interfaces
{
    public interface IRestRequest
    {
        string Path { get; }
        HttpMethod Method{ get; }
        HttpContent Content { get; }
        RestCollection Parameters { get; }
        RestCollection Headers { get; }

        void AddUrlSegment(string identifier, string value);
        void AddParameter(string name, string value);
        void AddHeader(string name, string value);
        void AddFile(string name, string path);
        void SetContent(string content);
        void SetContent(object content, string contentType);
        void SetContent(StringContent stringContent);
        void SetContent(FormUrlEncodedContent formUrlEncodedContent);
        void SetAuthentication(Http.AuthenticationMethod authenticationMethod, string token);
        void SetAuthentication(Http.AuthenticationMethod authenticationMethod, string username, string password);
    }
}
