using System;
using System.Net;
using System.Net.Http;
using Rest.Net.Interfaces;

namespace Rest.Net
{
    public class RestResponse<T> : IRestResponse<T>
    {
        public T Data { get; internal set; }
        public int Code { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public Exception Exception { get; internal set; }
        public HttpResponseMessage OriginalHttpResponseMessage { get; internal set; }
        public object RawData { get; internal set; }
        public bool IsError { get; internal set; }
        public TimeSpan RequestTime { get; internal set; }
        public long RequestTimeMs { get; internal set; }
    }
}
