using System;
using System.Threading.Tasks;

namespace Gelf.Extensions.Logging;

public interface IGelfClient : IDisposable
{
    Task SendMessageAsync(GelfMessage message);
}