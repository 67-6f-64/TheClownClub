using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Types {
    public class Http2WinHttpHandler : WinHttpHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            request.Version = new Version(2, 0);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
