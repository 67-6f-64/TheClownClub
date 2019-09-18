using Newtonsoft.Json;
using NotLiteCode.Serializer;
using System.Text;

namespace ClownClubServer.NLC {
    public class JsonSerializationProvider : ISerializationProdiver {
        public T Deserialize<T>(byte[] data) {
            return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(data));
        }

        public byte[] Serialize<T>(T data) {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(data));
        }
    }
}
