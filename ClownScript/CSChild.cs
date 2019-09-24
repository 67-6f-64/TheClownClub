using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSScript = System.Func<ClownScript.CSManager, int>;

namespace ClownScript {
    /// <summary>
    /// Handler for all successive calls after the initial Execute in the CSManager.
    /// </summary>
    public class CSChild {
        private readonly CSManager csMngr;
        private readonly int identifier;
        public CSChild(CSManager mngr, int retVal) {
            csMngr = mngr;
            identifier = retVal;
        }

        public CSChild Then(params CSScript[] func) {
            return new CSChild(csMngr, func[identifier](csMngr));
        }
    }
}
