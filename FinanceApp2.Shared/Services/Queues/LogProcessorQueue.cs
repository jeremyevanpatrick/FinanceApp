using FinanceApp2.Shared.Models;
using System.Threading.Channels;

namespace FinanceApp2.Shared.Services.Queues
{
    public class LogProcessorQueue : ILogProcessorQueue
    {
        private readonly Channel<ApplicationLog> _channel;

        public LogProcessorQueue()
        {
            _channel = Channel.CreateUnbounded<ApplicationLog>();
        }

        public ChannelReader<ApplicationLog> Reader => _channel.Reader;

        public void Enqueue(ApplicationLog applicationLog)
        {
            _channel.Writer.TryWrite(applicationLog);
        }
    }
}
