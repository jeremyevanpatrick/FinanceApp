using FinanceApp2.Shared.Errors;
using FinanceApp2.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Shared.Services.Responses;

namespace FinanceApp2.Api.Controllers
{
    public abstract class ControllerBaseExtended : ControllerBase
    {
        protected ObjectResult Problem400(string detail, string errorCode, List<Link>? links = null)
        {
            return ProblemWithErrorCode(StatusCodes.Status400BadRequest, detail, errorCode, links);
        }

        protected ObjectResult Problem401(string detail, string errorCode, List<Link>? links = null)
        {
            return ProblemWithErrorCode(StatusCodes.Status401Unauthorized, detail, errorCode, links);
        }

        protected ObjectResult Problem404(string detail, string errorCode, List<Link>? links = null)
        {
            return ProblemWithErrorCode(StatusCodes.Status404NotFound, detail, errorCode, links);
        }

        protected ObjectResult Problem409(string detail, string errorCode, List<Link>? links = null)
        {
            return ProblemWithErrorCode(StatusCodes.Status409Conflict, detail, errorCode, links);
        }

        protected ObjectResult Problem500()
        {
            return ProblemWithErrorCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.", ApiErrorCodes.INTERNAL_SERVER_ERROR);
        }

        protected ObjectResult ProblemWithErrorCode(int statusCode, string detail, string errorCode, List<Link>? links = null)
        {
            string title = ReasonPhrases.GetReasonPhrase(statusCode);
            string path = HttpContext.Request.Path;

            var apiErrorResponse = new ApiErrorResponse
            {
                Title = title,
                Status = statusCode,
                Detail = detail,
                ErrorCode = errorCode,
                Instance = path
            };

            if (links != null)
            {
                apiErrorResponse.Links = links;
            }

            return StatusCode(statusCode, apiErrorResponse);
        }
    }
}
