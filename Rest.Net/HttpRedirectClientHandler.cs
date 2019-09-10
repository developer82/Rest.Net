using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rest.Net
{
    class HttpRedirectClientHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                var location = response.Headers.Location;
                if (location == null)
                {
                    return response;
                }

                using (var clone = await CloneRequest(request, location))
                {
                    response = await base.SendAsync(clone, cancellationToken);
                }
            }
            return response;
        }


        private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request, Uri location)
        {
            var clone = new HttpRequestMessage(request.Method, location);

            if (request.Content != null)
            {
                clone.Content = await CloneContent(request);
                if (request.Content.Headers != null)
                {
                    CloneHeaders(clone, request);
                }
            }

            clone.Version = request.Version;
            CloneProperties(clone, request);
            CloneKeyValuePairs(clone, request);
            return clone;
        }

        private async Task<StreamContent> CloneContent(HttpRequestMessage request)
        {
            var memstrm = new MemoryStream();
            await request.Content.CopyToAsync(memstrm).ConfigureAwait(false);
            memstrm.Position = 0;
            return new StreamContent(memstrm);
        }

        private void CloneHeaders(HttpRequestMessage clone, HttpRequestMessage request)
        {
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.Add(header.Key, header.Value);
            }
        }

        private void CloneProperties(HttpRequestMessage clone, HttpRequestMessage request)
        {
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
        }

        private void CloneKeyValuePairs(HttpRequestMessage clone, HttpRequestMessage request)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
