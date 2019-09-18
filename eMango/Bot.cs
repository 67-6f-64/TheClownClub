using ActivityGen.Tasks;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityGen {
    public class Bot {
        public CancellationToken m_Cancel { get; private set; }
        public string m_Email { get; private set; }
        public string m_Password { get; private set; }
        public string m_Status { get; set; }
        public string m_Proxy { get; private set; }
        public byte[] m_CatpchaImage { get; set; }
        public string m_CaptchaText { get; set; }
        public Browser m_Browser { get; private set; }
        public Page m_Page { get; private set; }

        public async Task Init() {
            m_Status = "Initializing browser...";
            await InitBrowser();
            m_Status = "Initializing page...";
            await GeneratePage().ContinueWith(t => m_Page = t.Result);
            var task = new SignInTask();
            await task.Do(this).ContinueWith(t => t.Result == BotTaskResult.Success ? m_Status = "Signed in. Delaying..." : m_Status = "Failed to sign in.");
            if (m_Status.Equals("Signed in. Delaying..."))
                await Task.Delay(MainWindow.m_LoginDelay);
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
                "--user-agent=\"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36\""
            };

            if (!m_Proxy.Equals("NONE"))
                args.Add(string.Format("--proxy-server={0}", m_Proxy));

            var options = new LaunchOptions {
                Headless = true,
                IgnoreHTTPSErrors = true,
                ExecutablePath = executablePath,
                Args = args.ToArray()
            };

            m_Browser = await Puppeteer.LaunchAsync(options);
        }
        private async Task<Page> GeneratePage() {
            var page = await m_Browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
            await page.SetJavaScriptEnabledAsync(true);
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
            return page;
        }

        public Bot(string email, string password, string proxy = "") {
            m_Email = email;
            m_Password = password;
            if (!string.IsNullOrEmpty(m_Proxy) && !proxy.Equals("0.0.0.0:65535"))
                m_Proxy = proxy;
            else
                m_Proxy = "NONE";
            m_Status = "Idle";
        }

        public async Task RunTask(BotTask task) {
            await task.Do(this).ContinueWith(t => t.Result == BotTaskResult.Success ? m_Status = "Completed task." : m_Status = "Failed task.");
        }

        public async Task Run(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                switch (Utils.RandomNumber(0, 1)) {
                    case 0: {
                            await RunTask(new YoutubeTask());
                            break;
                        }
                    case 1: {
                            await RunTask(new NewsTask());
                            break;
                        }
                }
                if (token.IsCancellationRequested)
                    break;
                m_Status = "Delaying...";
                await Task.Delay(MainWindow.m_TaskDelay);
            }

            m_Status = "Idle";
        }
    }
}
