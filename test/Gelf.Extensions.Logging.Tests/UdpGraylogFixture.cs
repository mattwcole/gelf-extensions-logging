﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Gelf.Extensions.Logging.Tests
{
    public class UdpGraylogFixture : GraylogFixture
    {
        public UdpGraylogFixture() : base()
        {
            GraylogInputHost = Environment.GetEnvironmentVariable("GRAYLOG_HOST") ?? "localhost";
            GraylogInputPort = 12201;
        }

        protected override async Task<string> CreateInputAsync()
        {
            List<dynamic> existingInputs = (await _httpClient.GetAsync("system/inputs")).inputs;
            var input = existingInputs.SingleOrDefault(i => i.attributes.port == GraylogInputPort);
            if (input != null)
            {
                return input.id;
            }

            var newInputRequest = new
            {
                title = "Gelf.Extensions.Logging.Udp.Tests",
                global = true,
                type = "org.graylog2.inputs.gelf.udp.GELFUDPInput",
                configuration = new
                {
                    bind_address = "0.0.0.0",
                    decompress_size_limit = 8388608,
                    override_source = default(object),
                    port = GraylogInputPort,
                    recv_buffer_size = 212992
                }
            };

            var newInputResponse = await _httpClient.PostAsync(newInputRequest, "system/inputs");
            return newInputResponse.id;
        }
    }
}