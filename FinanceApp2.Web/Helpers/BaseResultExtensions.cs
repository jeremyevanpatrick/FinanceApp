using FinanceApp2.Shared.Services.DTOs;

namespace FinanceApp2.Web.Helpers
{
    public static class BaseResultExtensions
    {
        public static async Task HandleAsync(
            this Task<BaseResult> task,
            Action onSuccess,
            Action<string> onError)
        {
            var result = await task;

            if (result.IsSuccess)
            {
                onSuccess();
            }
            else
            {
                onError(result.ErrorMessage);
            }

        }

        public static async Task HandleAsync<T>(
            this Task<BaseResult<T>> task,
            Action<T> onSuccess,
            Action<string> onError)
        {
            var result = await task;

            if (result.IsSuccess)
            {
                onSuccess(result.Result);
            }
            else
            {
                onError(result.ErrorMessage);
            }

        }
    }

}