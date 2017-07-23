using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Gelf.Extensions.Logging
{
    public class GelfMessageProcessor : IDisposable
    {
        private readonly IGelfClient _gelfClient;
        private readonly BufferBlock<GelfMessage> _messageBuffer;
        private Task _processorTask;

        public GelfMessageProcessor(IGelfClient gelfClient)
        {
            _gelfClient = gelfClient;
            _messageBuffer = new BufferBlock<GelfMessage>();     // TODO: Limit capacity?
        }

        public void Start()
        {
            _processorTask = StartAsync();
        }

        private async Task StartAsync()
        {
            while (!_messageBuffer.Completion.IsCompleted)
            {
                // TODO: What happens when completed?
                var message = await _messageBuffer.ReceiveAsync().ConfigureAwait(false);

                try
                {
                    await _gelfClient.SendMessageAsync(message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.Fail("Unhandled exception while sending GELF message.", ex.ToString());
                }
            }
        }

        public void Stop()
        {
            _messageBuffer.Complete();
            Task.WaitAll(_messageBuffer.Completion, _processorTask);     // TODO: Timeouts, exceptions.
        }

        public void SendMessage(GelfMessage message)
        {
            if (!_messageBuffer.Post(message))
            {
                Debug.Fail("Failed to add GELF message to buffer.");
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
