using FinanceApp2.Shared.Models;
using System.Text.Json.Serialization;

namespace Shared.Services.Responses
{
    public class ApiErrorResponse : Resource
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }
        
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; }

        [JsonPropertyName("errors")]
        public List<ResponseErrorItem>? Errors { get; set; }

        [JsonPropertyName("instance")]
        public string? Instance { get; set; }
    }

    public class ResponseErrorItem
    {
        [JsonPropertyName("field")]
        public string? Field { get; set; }

        [JsonPropertyName("messages")]
        public List<string>? Messages { get; set; }
    }

}
