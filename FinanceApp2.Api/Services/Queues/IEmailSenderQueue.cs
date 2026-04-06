using FinanceApp2.Api.Models;
using System.Threading.Channels;

namespace FinanceApp2.Api.Services.Queues
{
    public interface IEmailSenderQueue
    {
        void Enqueue(EmailDetails emailDetails);

        ChannelReader<EmailDetails> Reader { get; }
    }
}