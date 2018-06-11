using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public class UdpGelfClient : IGelfClient, IDisposable
    {
        private const int MaxChunks = 128;
        private const int MaxChunkSize = 8192;
        private const int MessageHeaderSize = 12;
        private const int MessageIdSize = 8;
        private const int MaxMessageBodySize = MaxChunkSize - MessageHeaderSize;

        private readonly UdpClient _udpClient;
        private readonly GelfLoggerOptions _options;
        private readonly Random _random;

        public UdpGelfClient(GelfLoggerOptions options)
        {
            _options = options;
            _udpClient = new UdpClient();
            _random = new Random();
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message.ToJson());

            if (_options.CompressUdp && messageBytes.Length > _options.UdpCompressionThreshold)
            {
                messageBytes = await CompressMessageAsync(messageBytes).ConfigureAwait(false);
            }

            foreach (var messageChunk in ChunkMessage(messageBytes))
            {
                await _udpClient.SendAsync(messageChunk, messageChunk.Length, _options.Host, _options.Port)
                    .ConfigureAwait(false);
            }
        }

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

        private static byte[] GetMessageBytes(GelfMessage message)
        {
            var messageJson = JObject.FromObject(message);

            foreach (var field in message.AdditionalFields)
            {

                if(IsNumeric(field.Value))
                {
                    messageJson[$"_{field.Key}"] = JToken.FromObject(field.Value);
                }
                else
                {
                    messageJson[$"_{field.Key}"] = field.Value?.ToString();
                }
            }

            var messageString = JsonConvert.SerializeObject(messageJson, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return Encoding.UTF8.GetBytes(messageString);
        }

        private static async Task<byte[]> CompressMessageAsync(byte[] messageBytes)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    await gzipStream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false);
                }
                return outputStream.ToArray();
            }
        }

        private IEnumerable<byte[]> ChunkMessage(byte[] messageBytes)
        {
            if (messageBytes.Length < MaxChunkSize)
            {
                yield return messageBytes;
                yield break;
            }

            var sequenceCount = (int) Math.Ceiling(messageBytes.Length / (double) MaxMessageBodySize);
            if (sequenceCount > MaxChunks)
            {
                Debug.Fail($"GELF message contains {sequenceCount} chunks, exceeding the maximum of {MaxChunks}.");
                yield break;
            }

            var messageId = GetMessageId();
            for (var sequenceNumber = 0; sequenceNumber < sequenceCount; sequenceNumber++)
            {
                var messageHeader = GetMessageHeader(sequenceNumber, sequenceCount, messageId);
                var chunkStartIndex = sequenceNumber * MaxMessageBodySize;
                var messageBodySize = Math.Min(messageBytes.Length - chunkStartIndex, MaxMessageBodySize);
                var chunk = new byte[messageBodySize + MessageHeaderSize];

                Array.Copy(messageHeader, chunk, MessageHeaderSize);
                Array.ConstrainedCopy(messageBytes, chunkStartIndex, chunk, MessageHeaderSize, messageBodySize);

                yield return chunk;
            }
        }

        private byte[] GetMessageId()
        {
            var messageId = new byte[8];
            _random.NextBytes(messageId);
            return messageId;
        }

        private static byte[] GetMessageHeader(int sequenceNumber, int sequenceCount, byte[] messageId)
        {
            var header = new byte[MessageHeaderSize];
            header[0] = 0x1e;
            header[1] = 0x0f;

            Array.ConstrainedCopy(messageId, 0, header, 2, MessageIdSize);

            header[10] = Convert.ToByte(sequenceNumber);
            header[11] = Convert.ToByte(sequenceCount);

            return header;
        }

        public void Dispose()
        {
            _udpClient.Dispose();
        }
    }
}
