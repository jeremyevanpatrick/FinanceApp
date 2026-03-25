using FinanceApp2.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp2.Api.Controllers
{
    public abstract class ControllerBaseExtended : ControllerBase
    {
        protected ObjectResult Problem400(string detail, ResponseErrorCodes errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status400BadRequest, detail, errorCode);
        }

        protected ObjectResult Problem401(string detail, ResponseErrorCodes errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status401Unauthorized, detail, errorCode);
        }

        protected ObjectResult Problem404(string detail, ResponseErrorCodes errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status404NotFound, detail, errorCode);
        }

        protected ObjectResult Problem409(string detail, ResponseErrorCodes errorCode)
        {
            return ProblemWithErrorCode(StatusCodes.Status409Conflict, detail, errorCode);
        }

        protected ObjectResult Problem500()
        {
            return Problem("An unexpected error occurred. Please try again later.");
        }

        protected ObjectResult ProblemWithErrorCode(int statusCode, string detail, ResponseErrorCodes errorCode)
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
