namespace Rest.Net
{
    public class Http
    {
        public enum Method
        {
            DELETE,
            GET,
            HEAD,
            OPTIONS,
            POST,
            PUT,
            TRACE
        }

        public enum AuthenticationMethod
        {
            Basic,
            Bearer
        }
    }
}
