using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gelf.Extensions.Logging
{
    public static class GelfMessageExtensions
    {
        public static string ToJson(this GelfMessage message)
        {
            var messageJson = JObject.FromObject(message);

            foreach (var field in message.AdditionalFields)
            {
                messageJson[$"_{field.Key}"] = field.Value?.ToString();
            }

            return JsonConvert.SerializeObject(messageJson, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
