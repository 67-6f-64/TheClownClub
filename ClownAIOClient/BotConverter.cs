using System;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Supreme;

namespace ClownAIO {
    public class BotConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Bot);
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var bot = default(Bot);
            switch (jsonObject["BotType"].Value<int>()) {
                case (int)BotType.Supreme:
                    bot = new Supreme.SupremeBot();
                    break;
            }

            serializer.Populate(jsonObject.CreateReader(), bot);
            return bot;
        }
    }
}
