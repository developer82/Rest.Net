using System.Threading;
using System.Threading.Tasks;
using Rest.Net.Interfaces;

namespace Rest.Net
{
    public class RestRequestAsyncHandler<T> : IRestRequestAsyncHandler
    {
        internal Task<IRestResponse<T>> ExecutionTask { get; set; }
        internal CancellationTokenSource TokenSource { get; set; }
        
        public void Abort()
        {
            TokenSource?.Cancel();
        }
    }
}
