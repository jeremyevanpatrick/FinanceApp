namespace FinanceApp2.Shared.Services.DTOs
{
    public class BaseResult
    {
        public BaseResult() { }
        public BaseResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
    }

    public class BaseResult<T> : BaseResult
    {
        public BaseResult() { }
        public BaseResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
        public BaseResult(T result)
        {
            Result = result;
        }

        public T Result { get; set; }
    }
}
