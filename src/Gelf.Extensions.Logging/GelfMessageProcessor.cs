﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Gelf.Extensions.Logging
{
    public class GelfMessageProcessor
    {
        private readonly IGelfClient _gelfClient;
        private readonly BufferBlock<GelfMessage> _messageBuffer;

        private Task _processorTask = Task.CompletedTask;

        public GelfMessageProcessor(IGelfClient gelfClient)
        {
            _gelfClient = gelfClient;
            _messageBuffer = new BufferBlock<GelfMessage>(new DataflowBlockOptions
            {
                BoundedCapacity = 10000
            });
        }

        public void Start()
        {
            _processorTask = Task.Run(StartAsync);
        }

        private async Task StartAsync()
        {
            while (!_messageBuffer.Completion.IsCompleted)
            {
                try
                {
                    var message = await _messageBuffer.ReceiveAsync();
                    await _gelfClient.SendMessageAsync(message);
                }
                catch (InvalidOperationException)
                {
                    // The source completed without providing data to receive.
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unhandled exception while sending GELF message{Environment.NewLine}{{0}}", ex);
                }
            }
        }

        public void Stop()
        {
            _messageBuffer.Complete();
            _processorTask.Wait();
        }

        public void SendMessage(GelfMessage message)
        {
            if (!_messageBuffer.Post(message))
            {
                Trace.TraceError("Failed to add GELF message to buffer");
            }
        }
    }
}
