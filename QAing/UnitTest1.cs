using NUnit.Framework;
using QAing.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;

namespace Movie
{
    [TestFixture]
    public class MovieTests
    {
        private RestClient client;
        private static string createdMovieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("toto@abv.bg", "5152535455");

            var options = new RestClientOptions("http://144.91.123.158:5000/")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var authClient = new RestClient("http://144.91.123.158:5000/");
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = authClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                return content.GetProperty("accessToken").GetString();
            }
            throw new Exception("Auth failed! Check credentials.");
        }

        [Order(1)]
        [Test]
        public void CreateMovie_WithRequiredFields_ShouldReturnOk()
        {
            var request = new RestRequest("api/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "Inception",
                description = "A great movie",
                posterUrl = "http://images.com/poster.jpg",
                trailerLink = "https://www.youtube.com/watch?v=8hP9D6kZseM", 
                isWatched = true
            });

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                TestContext.WriteLine($"Create Failed: {response.Content}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(content.Msg, Is.EqualTo("Movie created successfully!"));

            Assert.That(content.Movie, Is.Not.Null);
            Assert.That(content.Movie.Id, Is.Not.Null.And.Not.Empty);

            createdMovieId = content.Movie.Id.ToString();
        }

        [Order(2)]
        [Test]
        public void EditMovie_ShouldReturnOk()
        {
            var request = new RestRequest("api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId);

            request.AddJsonBody(new
            {
                title = "Updated Title",
                description = "Updated Description"
            });

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(content.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("api/Catalog/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonSerializer.Deserialize<List<MovieData>>(response.Content);

            Assert.That(movies, Is.Not.Null);
            Assert.That(movies.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void DeleteMovie_ShouldReturnOk()
        {
            var request = new RestRequest("api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(content.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_MissingFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("api/Movie/Create", Method.Post);
            request.AddJsonBody(new { title = "" }); 

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {
            var request = new RestRequest("api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "999999"); 

            request.AddJsonBody(new
            {
                title = "Updated Title",
                description = "Updated Description"
            });

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            var request = new RestRequest("api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "999999");

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var content = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(content.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown() => client?.Dispose();
    }
}