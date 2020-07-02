using System.Threading;
using NotLiteCode.Client;
using NotLiteCode.Network;
using NotLiteCode.Serialization;

namespace ClownAIO {
    internal class Globals {
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static readonly NLCSocket NlcSocket = new NLCSocket(new GroBufSerializationProvider(), true, true);
        public static readonly Client Client = new Client(NlcSocket);
    }
}
