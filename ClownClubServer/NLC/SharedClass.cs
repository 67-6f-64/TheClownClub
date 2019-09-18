using NotLiteCode.Server;
using System;

namespace ClownClubServer.NLC {
    class SharedClass : IDisposable {
        [NLCCall("Hacker man")]
        public void HelloWorld() {

        }

        public void Dispose() { }
    }
}
