using System.Text.Json.Serialization;

namespace Shared.Services.Responses
{
    public class ProblemResponse
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("instance")]
        public string? Instance { get; set; }

        // Optional extension fields
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

    }
}
