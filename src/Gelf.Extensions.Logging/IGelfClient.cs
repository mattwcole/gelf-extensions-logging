using System.Threading.Tasks;

namespace Gelf.Extensions.Logging
{
    public interface IGelfClient
    {
        Task SendMessageAsync(GelfMessage message);
    }
}
