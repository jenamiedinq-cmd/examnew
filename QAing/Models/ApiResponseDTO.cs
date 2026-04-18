using System.Text.Json.Serialization;

namespace QAing.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("movie")]
        public MovieData Movie { get; set; }
    }

    public class MovieData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("posterUrl")]
        public string PosterUrl { get; set; }

        [JsonPropertyName("trailerLink")]
        public string TrailerLink { get; set; }

        [JsonPropertyName("isWatched")]
        public bool IsWatched { get; set; }
    }
}