using FinanceApp2.Shared.Models;

namespace FinanceApp2.Api.Services
{
    public interface IErrorLogQueue
    {
        void Enqueue(Error error);
    }
}