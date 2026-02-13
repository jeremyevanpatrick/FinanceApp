using FinanceApp2.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    public abstract class ControllerBaseExtended : ControllerBase
    {
        protected ObjectResult Problem400(string detail, ResponseErrorCodes errorCode)
        {
            return CreateProblem(StatusCodes.Status400BadRequest, detail, errorCode);
        }

        protected ObjectResult Problem401(string detail, ResponseErrorCodes errorCode)
        {
            return CreateProblem(StatusCodes.Status401Unauthorized, detail, errorCode);
        }

        private ObjectResult CreateProblem(int statusCode, string detail, ResponseErrorCodes errorCode)
        {
            return Problem(
                detail: detail,
                statusCode: statusCode,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = errorCode.ToString()
                });
        }
    }
}
