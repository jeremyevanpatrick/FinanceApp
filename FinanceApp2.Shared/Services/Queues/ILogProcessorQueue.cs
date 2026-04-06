using FinanceApp2.Shared.Models;
using System.Threading.Channels;

namespace FinanceApp2.Shared.Services.Queues
{
    public interface ILogProcessorQueue
    {
        void Enqueue(ApplicationLog applicationLog);

        ChannelReader<ApplicationLog> Reader { get; }
    }
}