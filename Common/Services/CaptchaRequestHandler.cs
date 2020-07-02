using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CefSharp;
using CefSharp.Handler;

namespace Common.Services {
    public class CaptchaRequestHandler : RequestHandler {
        private string Domain { get; }
        private string SiteKey { get; }

        public CaptchaRequestHandler(string domain, string sitekey) {
            Domain = domain;
            SiteKey = sitekey;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling) {
            return new CaptchaResourceHandler(Domain, SiteKey);
        }
    }
}
