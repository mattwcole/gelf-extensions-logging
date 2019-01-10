using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gelf.Extensions.Logging
{
    public static class GelfMessageExtensions
    {
        private static bool IsNumeric(object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }

        public static string ToJson(this GelfMessage message)
        {
            var messageJson = JObject.FromObject(message, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            });

            foreach (var field in message.AdditionalFields)
            {
                if (IsNumeric(field.Value))
                {
                    messageJson[$"_{field.Key}"] = JToken.FromObject(field.Value);
                }
                else
                {
                    messageJson[$"_{field.Key}"] = field.Value?.ToString();
                }
            }

            return messageJson.ToString(Formatting.None);
        }
    }
}
