using FinanceApp2.Shared.Models;
using System.Threading.Channels;

namespace FinanceApp2.Api.Services
{
    public class ErrorLogQueue : IErrorLogQueue
    {
        private readonly Channel<Error> _channel;

        public ErrorLogQueue()
        {
            _channel = Channel.CreateUnbounded<Error>();
        }

        public ChannelReader<Error> Reader => _channel.Reader;

        public void Enqueue(Error error)
        {
            _channel.Writer.TryWrite(error);
        }
    }
}
