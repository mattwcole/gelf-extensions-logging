using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Gelf.Extensions.Logging
{
    public static class GelfMessageExtensions
    {
        public static string ToJson(this GelfMessage message)
        {
            using var stream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(stream);

            jsonWriter.WriteStartObject();
            jsonWriter.WriteStringUnlessNull("version", message.Version);
            jsonWriter.WriteStringUnlessNull("host", message.Host);
            jsonWriter.WriteStringUnlessNull("short_message", message.ShortMessage);
            jsonWriter.WriteNumber("timestamp", message.Timestamp);
            jsonWriter.WriteNumber("level", (int) message.Level);
            jsonWriter.WriteStringUnlessNull("_logger", message.Logger);
            jsonWriter.WriteStringUnlessNull("_exception", message.Exception);
            jsonWriter.WriteNumberUnlessNull("_event_id", message.EventId);
            jsonWriter.WriteStringUnlessNull("_event_name", message.EventName);

            foreach (var field in message.AdditionalFields)
            {
                WriteAdditionalField(jsonWriter, field);
            }

            jsonWriter.WriteEndObject();
            jsonWriter.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static void WriteAdditionalField(Utf8JsonWriter jsonWriter, KeyValuePair<string, object> field)
        {
            var key = $"_{field.Key}";

            switch (field.Value)
            {
                case null:
                    break;
                case sbyte value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case byte value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case short value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case ushort value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case int value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case uint value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case long value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case ulong value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case float value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case double value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                case decimal value:
                    jsonWriter.WriteNumber(key, value);
                    break;
                default:
                    jsonWriter.WriteString(key, field.Value.ToString());
                    break;
            }
        }

        private static void WriteStringUnlessNull(this Utf8JsonWriter jsonWriter, string propertyName, string? value)
        {
            if (value != null)
            {
                jsonWriter.WriteString(propertyName, value);
            }
        }

        private static void WriteNumberUnlessNull(this Utf8JsonWriter jsonWriter, string propertyName, int? value)
        {
            if (value != null)
            {
                jsonWriter.WriteNumber(propertyName, value.Value);
            }
        }
    }
}
