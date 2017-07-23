using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gelf.Extensions.Logging
{
    public class UdpGelfClient : IGelfClient, IDisposable
    {
        private const int MaxChunks = 128;
        private const int MaxMessageChunkSize = 8192;
        private const int MessageHeaderSize = 12;
        private const int MessageIdSize = 8;
        private const int MaxMessageBodySize = MaxMessageChunkSize - MessageHeaderSize;

        private readonly UdpClient _udpClient;
        private readonly GelfLoggerOptions _options;

        public UdpGelfClient(GelfLoggerOptions options)
        {
            _options = options;
            _udpClient = new UdpClient();
        }

        public async Task SendMessageAsync(GelfMessage message)
        {
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = await CompressMessageAsync(Encoding.UTF8.GetBytes(messageJson)).ConfigureAwait(false);

            foreach (var messageChunk in ChunkMessage(messageBytes))
            {
                await _udpClient.SendAsync(messageChunk, messageChunk.Length, _options.Hostname, _options.Port)
                    .ConfigureAwait(false);
            }
        }

        private static async Task<byte[]> CompressMessageAsync(byte[] messageBytes)
        {
            using (var inputStream = new MemoryStream(messageBytes))
            using (var outputStream = new MemoryStream())
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                await inputStream.CopyToAsync(gzipStream).ConfigureAwait(false);
                return outputStream.ToArray();
            }
        }

        private static IEnumerable<byte[]> ChunkMessage(byte[] messageBytes)
        {
            if (messageBytes.Length < MaxMessageChunkSize)
            {
                yield return messageBytes;
            }

            var sequenceCount = (int) Math.Ceiling(messageBytes.Length / (double) MaxMessageBodySize);
            if (sequenceCount > MaxChunks)
            {
                Debug.Fail($"GELF message contains {sequenceCount} chunks, exceeding the maximum of {MaxChunks}.");
                yield break;
            }

            for (var sequenceNumber = 0; sequenceNumber < sequenceCount; sequenceNumber++)
            {
                var chunkStartIndex = sequenceNumber * MaxMessageBodySize;
                var chunkSize = Math.Min(messageBytes.Length - chunkStartIndex, MaxMessageBodySize);

                var messageHeader = GetMessageHeader(sequenceNumber, sequenceCount);
                var chunk = new byte[chunkSize];

                Array.Copy(messageHeader, chunk, messageHeader.Length);
                Array.ConstrainedCopy(messageBytes, chunkStartIndex, chunk, MessageHeaderSize, chunkSize);

                yield return chunk;
            }
        }

        private static byte[] GetMessageHeader(int sequenceNumber, int sequenceCount)
        {
            var header = new byte[MessageHeaderSize];
            header[0] = 0x1e;
            header[1] = 0x0f;

            var messageId = Guid.NewGuid().ToByteArray();   // TODO: Better message ID.
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
