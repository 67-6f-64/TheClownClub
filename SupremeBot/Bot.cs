using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SupremeBot {
    public enum BotStep {
        FindProduct,
        FindStyleAndSize,
        AddToCart,
        ParseCheckout,
        Captcha,
        Checkout
    }

    public class Bot {
        private static Random random = new Random();
        public CancellationToken m_Cancel { get; private set; }
        public string m_Id { get; private set; }
        public string m_Status { get; set; }
        public string m_Proxy { get; private set; }
        public string m_CsrfToken { get; set; }
        public string m_CheckoutPostData { get; set; }
        public Browser m_Browser { get; private set; }
        public Page m_Page { get; private set; }
        public BotStep m_Step { get; private set; }
       
        public long m_SizeId { get; set; }
        public long m_StyleId { get; set; }

        public static string RandomString(int length) {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task Init() {
            m_Status = "Initializing browser...";
            await InitBrowser();
            m_Status = "Initializing page...";
            await GeneratePage().ContinueWith(t => m_Page = t.Result);
        }
        private async Task InitBrowser() {
            var currentDirectory = Directory.GetCurrentDirectory();
            var downloadPath = Path.Combine(currentDirectory, "ChromeRuntime");

            if (!Directory.Exists(downloadPath)) {
                Directory.CreateDirectory(downloadPath);
            }

            var browserFetcherOptions = new BrowserFetcherOptions { Path = downloadPath };
            var browserFetcher = new BrowserFetcher(browserFetcherOptions);
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);

            var executablePath = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultRevision);

            if (string.IsNullOrEmpty(executablePath)) {
                throw new Exception("Failed to initialize browser.");
            }

            List<string> args = new List<string> {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-infobars",
                "--window-position=0,0",
                "--ignore-certifcate-errors",
                "--ignore-certifcate-errors-spki-list",
                $"--user-agent=\"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36 {m_Id}\""
            };

            if (!m_Proxy.Equals("NONE"))
                args.Add(string.Format("--proxy-server={0}", m_Proxy));

            var options = new LaunchOptions {
                Headless = false,
                IgnoreHTTPSErrors = true,
                ExecutablePath = executablePath,
                Args = args.ToArray()
            };

            m_Browser = await Puppeteer.LaunchAsync(options);
        }
        private async Task<Page> GeneratePage() {
            var page = await m_Browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
            await page.EvaluateFunctionOnNewDocumentAsync(@"function() {
                delete navigator.__proto__.webdriver 
                /* global MimeType MimeTypeArray PluginArray */

                // Disguise custom functions as being native
                const makeFnsNative = (fns = []) => {
                  const oldCall = Function.prototype.call
                  function call () {
                    return oldCall.apply(this, arguments)
                  }
                  // eslint-disable-next-line
                  Function.prototype.call = call

                  const nativeToStringFunctionString = Error.toString().replace(
                    /Error/g,
                    'toString'
                  )
                  const oldToString = Function.prototype.toString

                  function functionToString () {
                    for (const fn of fns) {
                      if (this === fn.ref) {
                        return `function ${fn.name}() { [native code] }`
                      }
                    }

                    if (this === functionToString) {
                      return nativeToStringFunctionString
                    }
                    return oldCall.call(oldToString, this)
                  }
                  // eslint-disable-next-line
                  Function.prototype.toString = functionToString
                }

                const mockedFns = []

                const fakeData = {
                  mimeTypes: [
                    {
                      type: 'application/pdf',
                      suffixes: 'pdf',
                      description: '',
                      __pluginName: 'Chrome PDF Viewer'
                    },
                    {
                      type: 'application/x-google-chrome-pdf',
                      suffixes: 'pdf',
                      description: 'Portable Document Format',
                      __pluginName: 'Chrome PDF Plugin'
                    },
                    {
                      type: 'application/x-nacl',
                      suffixes: '',
                      description: 'Native Client Executable',
                      enabledPlugin: Plugin,
                      __pluginName: 'Native Client'
                    },
                    {
                      type: 'application/x-pnacl',
                      suffixes: '',
                      description: 'Portable Native Client Executable',
                      __pluginName: 'Native Client'
                    }
                  ],
                  plugins: [
                    {
                      name: 'Chrome PDF Plugin',
                      filename: 'internal-pdf-viewer',
                      description: 'Portable Document Format'
                    },
                    {
                      name: 'Chrome PDF Viewer',
                      filename: 'mhjfbmdgcfjbbpaeojofohoefgiehjai',
                      description: ''
                    },
                    {
                      name: 'Native Client',
                      filename: 'internal-nacl-plugin',
                      description: ''
                    }
                  ],
                  fns: {
                    namedItem: instanceName => {
                      // Returns the Plugin/MimeType with the specified name.
                      const fn = function (name) {
                        if (!arguments.length) {
                          throw new TypeError(
                            `Failed to execute 'namedItem' on '${instanceName}': 1 argument required, but only 0 present.`
                          )
                        }
                        return this[name] || null
                      }
                      mockedFns.push({ ref: fn, name: 'namedItem' })
                      return fn
                    },
                    item: instanceName => {
                      // Returns the Plugin/MimeType at the specified index into the array.
                      const fn = function (index) {
                        if (!arguments.length) {
                          throw new TypeError(
                            `Failed to execute 'namedItem' on '${instanceName}': 1 argument required, but only 0 present.`
                          )
                        }
                        return this[index] || null
                      }
                      mockedFns.push({ ref: fn, name: 'item' })
                      return fn
                    },
                    refresh: instanceName => {
                      // Refreshes all plugins on the current page, optionally reloading documents.
                      const fn = function () {
                        return undefined
                      }
                      mockedFns.push({ ref: fn, name: 'refresh' })
                      return fn
                    }
                  }
                }
                // Poor mans _.pluck
                const getSubset = (keys, obj) =>
                  keys.reduce((a, c) => ({ ...a, [c]: obj[c] }), {})

                function generateMimeTypeArray () {
                  const arr = fakeData.mimeTypes
                    .map(obj => getSubset(['type', 'suffixes', 'description'], obj))
                    .map(obj => Object.setPrototypeOf(obj, MimeType.prototype))
                  arr.forEach(obj => {
                    arr[obj.type] = obj
                  })

                  // Mock functions
                  arr.namedItem = fakeData.fns.namedItem('MimeTypeArray')
                  arr.item = fakeData.fns.item('MimeTypeArray')

                  return Object.setPrototypeOf(arr, MimeTypeArray.prototype)
                }

                const mimeTypeArray = generateMimeTypeArray()
                Object.defineProperty(navigator, 'mimeTypes', {
                  get: () => mimeTypeArray
                })

                function generatePluginArray () {
                  const arr = fakeData.plugins
                    .map(obj => getSubset(['name', 'filename', 'description'], obj))
                    .map(obj => {
                      const mimes = fakeData.mimeTypes.filter(
                        m => m.__pluginName === obj.name
                      )
                      // Add mimetypes
                      mimes.forEach((mime, index) => {
                        navigator.mimeTypes[mime.type].enabledPlugin = obj
                        obj[mime.type] = navigator.mimeTypes[mime.type]
                        obj[index] = navigator.mimeTypes[mime.type]
                      })
                      obj.length = mimes.length
                      return obj
                    })
                    .map(obj => {
                      // Mock functions
                      obj.namedItem = fakeData.fns.namedItem('Plugin')
                      obj.item = fakeData.fns.item('Plugin')
                      return obj
                    })
                    .map(obj => Object.setPrototypeOf(obj, Plugin.prototype))
                  arr.forEach(obj => {
                    arr[obj.name] = obj
                  })

                  // Mock functions
                  arr.namedItem = fakeData.fns.namedItem('PluginArray')
                  arr.item = fakeData.fns.item('PluginArray')
                  arr.refresh = fakeData.fns.refresh('PluginArray')

                  return Object.setPrototypeOf(arr, PluginArray.prototype)
                }

                const pluginArray = generatePluginArray()
                Object.defineProperty(navigator, 'plugins', {
                  get: () => pluginArray
                })

                // Make mockedFns toString() representation resemble a native function
                makeFnsNative(mockedFns)

                window.chrome = {
                    runtime: {}
                }

                try {
                    /* global WebGLRenderingContext */
                    const getParameter = WebGLRenderingContext.getParameter
                    WebGLRenderingContext.prototype.getParameter = function (parameter) {
                      // UNMASKED_VENDOR_WEBGL
                      if (parameter === 37445) {
                        return 'Intel Inc.'
                      }
                      // UNMASKED_RENDERER_WEBGL
                      if (parameter === 37446) {
                        return 'Intel Iris OpenGL Engine'
                      }
                      return getParameter(parameter)
                    }
                } catch (err) {}

                try {
                    if (window.outerWidth && window.outerHeight) {
                      return // nothing to do here
                    }
                    const windowFrame = 85 // probably OS and WM dependent
                    window.outerWidth = window.innerWidth
                    window.outerHeight = window.innerHeight + windowFrame
                } catch (err) {}

                const originalQuery = window.navigator.permissions.query
                // eslint-disable-next-line
                window.navigator.permissions.__proto__.query = parameters =>
                  parameters.name === 'notifications'
                    ? Promise.resolve({ state: Notification.permission }) //eslint-disable-line
                    : originalQuery(parameters)

                // Inspired by: https://github.com/ikarienator/phantomjs_hide_and_seek/blob/master/5.spoofFunctionBind.js
                const oldCall = Function.prototype.call
                function call () {
                  return oldCall.apply(this, arguments)
                }
                // eslint-disable-next-line
                Function.prototype.call = call

                const nativeToStringFunctionString = Error.toString().replace(
                  /Error/g,
                  'toString'
                )
                const oldToString = Function.prototype.toString

                function functionToString () {
                  if (this === window.navigator.permissions.query) {
                    return 'function query() { [native code] }'
                  }
                  if (this === functionToString) {
                    return nativeToStringFunctionString
                  }
                  return oldCall.call(oldToString, this)
                }
                // eslint-disable-next-line
                Function.prototype.toString = functionToString
            }
            ");
            await page.SetRequestInterceptionAsync(true);
            page.Request += (sender, e) => {
                if (e.Request.Url.EndsWith("add.json")) {
                    Payload payload = new Payload { Method = HttpMethod.Post, Headers = e.Request.Headers };
                    payload.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    payload.PostData = $"st={m_StyleId}&s={m_SizeId}&qty=1";
                    e.Request.ContinueAsync(payload);
                }
                else if (e.Request.Url.EndsWith("checkout.json")) {
                    Payload payload = new Payload { Method = HttpMethod.Post, Headers = e.Request.Headers };
                    payload.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                    payload.Headers.Add("X-CSRF-Token", m_CsrfToken);
                    payload.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    payload.Headers.Add("Referer", "https://www.supremenewyork.com/checkout");
                    payload.PostData = m_CheckoutPostData;
                    e.Request.ContinueAsync(payload);
                }
                else if (e.Request.Url.EndsWith("mobile_stock.json") || Regex.Match(e.Request.Url, @"shop\/\d{6}.json").Success) {
                    Payload payload = new Payload { Method = HttpMethod.Get, Headers = e.Request.Headers };
                    payload.Headers.Add("Accept", "application/json");
                    payload.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    payload.Headers.Add("Referer", "https://www.supremenewyork.com/mobile");
                    payload.Headers.Add("Cache-Control", "max-age=0");
                    e.Request.ContinueAsync(payload);
                }
                else if (e.Request.Url.EndsWith("/clown-recaptcha-solve")) {
                    e.Request.ContinueAsync();
                    if (e.Request.PostData != null) {

                    }
                }
                else if (e.Request.Url.EndsWith("/clown-recaptcha")) {
                    e.Request.ContinueAsync();
                }
                else {
                    if (m_Step.Equals(BotStep.Captcha)) {
                        if (e.Request.ResourceType.Equals(ResourceType.Image)
                            || e.Request.ResourceType.Equals(ResourceType.StyleSheet)
                            || e.Request.ResourceType.Equals(ResourceType.Font))
                            e.Request.AbortAsync();
                        else
                            e.Request.ContinueAsync();
                    }
                    else {
                        if (e.Request.ResourceType.Equals(ResourceType.Image)
                            || e.Request.ResourceType.Equals(ResourceType.StyleSheet)
                            || e.Request.ResourceType.Equals(ResourceType.Font)
                            || e.Request.ResourceType.Equals(ResourceType.Script))
                            e.Request.AbortAsync();
                        else
                            e.Request.ContinueAsync();
                    }
                }
            };
            return page;
        }

        public Bot(string proxy = "") {
            m_Id = RandomString(6);
            if (!string.IsNullOrEmpty(m_Proxy) && !proxy.Equals("0.0.0.0:65535"))
                m_Proxy = proxy;
            else
                m_Proxy = "NONE";
            m_Status = "Idle";
        }
    }
}
