using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSScript = System.Func<ClownScript.CSManager, int>;

namespace ClownScript {
    /// <summary>
    /// Used to manage tasks/"scripts" and determines which script should be called by the return value of the script before it.
    /// </summary>
    public class CSManager {
        private readonly RequestManager requestManager;

        public CSManager() {
            requestManager = new RequestManager();
        }

        public CSChild Execute(CSScript script) {
            return new CSChild(this, script(this));
        }

        public RequestManager GetRequestManager() {
            return requestManager;
        }
    }
}
