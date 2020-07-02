namespace Common.Types {
    public class Proxy {
        public string IP { get; set; }
        public string Port { get; set; }

        public string GetAddress() {
            return IP + ":" + Port;
        }

        public Proxy(string ip, string port) {
            IP = ip;
            Port = port;
        }
    }
}
