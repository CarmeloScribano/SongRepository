using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RESTAPI_DynamoDB.Models;
using System.Net.Http.Headers;
using System.Text;

namespace RESTAPI_DynamoDB.Test
{
    [TestClass]
    public class SongTests
    {
        private const string defaultValue = "string";
        private readonly HttpClient _httpClient;



        // Song object used as a test value.
        private readonly Song testSong = new()
        {
            Title = defaultValue,
            Album = defaultValue,
            Artist = defaultValue,
            Genre = defaultValue,
            ReleaseYear = 0,
        };
        // Song object used as a test value for updates.
        private readonly Song updatedSong = new()
        {
            Title = defaultValue,
            Album = defaultValue,
            Artist = defaultValue + "-updated",
            Genre = defaultValue + "-updated",
            ReleaseYear = 1,
        };



        public SongTests()
        {
            _httpClient = new WebApplicationFactory<Program>().CreateDefaultClient();

            string token = GetUserToken(new UserLogin()).Result;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        
        
        /// <summary>
        /// Gets all Songs from the server.
        /// </summary>
        [TestMethod]
        public void GetAllSongs_Test()
        {
            List<Song> songs = CreateGetRequest("GetAllSongs").Result;

            foreach (Song song in songs)
            {
                Assert.IsNotNull(song.Title);
                Assert.IsNotNull(song.Album);
                Assert.IsNotNull(song.Artist);
                Assert.IsNotNull(song.Genre);
                Assert.IsNotNull(song.ReleaseYear);
            }
        }

        /// <summary>
        /// Creates a new song and then tries to create a Song that already exists.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CreateSong_Test()
        {
            //Create a new Song
            await _httpClient.DeleteAsync($"/api/Song/DeleteSong/{testSong.Title}/{testSong.Album}");
            Song song = CreatePostRequest("CreateSong", testSong).Result;
            AssertionTesting(song, testSong);

            //Create a Song that already exists
            HttpResponseMessage response = CreateBasicPostRequest("CreateSong", testSong).Result;
            Assert.AreEqual("Conflict", response.StatusCode.ToString());
        }

        /// <summary>
        /// Gets Songs by Title and then tries with a non-existent Title.
        /// </summary>
        [TestMethod]
        public void GetSongsByTitle_Test()
        {
            //Get songs by Title
            _ = CreatePostRequest("UpdateSong", testSong).Result;
            List<Song> songs = CreateGetRequest($"GetSongsByTitle/{testSong.Title}").Result;
            foreach (Song song in songs)
                AssertionTesting(song, testSong);

            //Get songs with a Title that does not exist
            songs = CreateGetRequest($"GetSongsByTitle/randomValue").Result;
            Assert.AreEqual(0, songs.Count);
        }

        /// <summary>
        /// Gets Songs by Album and then tries with a non-existent Album.
        /// </summary>
        [TestMethod]
        public void GetSongsByAlbum_Test()
        {
            //Get songs by Album
            _ = CreatePostRequest("UpdateSong", testSong).Result;
            List<Song> songs = CreateGetRequest($"GetSongsByAlbum/{testSong.Album}").Result;
            foreach (Song song in songs)
                AssertionTesting(song, testSong);

            //Get songs with a Album that does not exist
            songs = CreateGetRequest($"GetSongsByAlbum/randomValue").Result;
            Assert.AreEqual(0, songs.Count);
        }

        /// <summary>
        /// Gets Songs by Artist and then tries with a non-existent Artist.
        /// </summary>
        [TestMethod]
        public void GetSongsByArtist_Test()
        {
            //Get songs by Artist
            _ = CreatePostRequest("UpdateSong", testSong).Result;
            List<Song> songs = CreateGetRequest($"GetSongsByArtist/{testSong.Artist}").Result;
            foreach (Song song in songs)
                AssertionTesting(song, testSong);

            //Get songs with a Artist that does not exist
            songs = CreateGetRequest($"GetSongsByArtist/randomValue").Result;
            Assert.AreEqual(0, songs.Count);
        }

        /// <summary>
        /// Gets Songs by Genre and then tries with a non-existent Genre.
        /// </summary>
        [TestMethod]
        public void GetSongsByGenre_Test()
        {
            //Get songs by Genre
            _ = CreatePostRequest("UpdateSong", testSong).Result;
            List<Song> songs = CreateGetRequest($"GetSongsByGenre/{testSong.Genre}").Result;
            foreach (Song song in songs)
                AssertionTesting(song, testSong);

            //Get songs with a Artist that does not exist
            songs = CreateGetRequest($"GetSongsByGenre/randomValue").Result;
            Assert.AreEqual(0, songs.Count);
        }

        /// <summary>
        /// Gets Songs by Release Year and then tries with a non-existent Release Year.
        /// </summary>
        [TestMethod]
        public void GetSongsByReleaseYear_Test()
        {
            //Get songs by Release Year
            _ = CreatePostRequest("UpdateSong", testSong).Result;
            List<Song> songs = CreateGetRequest($"GetSongsByReleaseYear/{testSong.ReleaseYear}").Result;
            foreach (Song song in songs)
                AssertionTesting(song, testSong);

            //Get songs with a Release Year that does not exist
            songs = CreateGetRequest($"GetSongsByReleaseYear/1234567890").Result;
            Assert.AreEqual(0, songs.Count);
        }

        /// <summary>
        /// Updates a Song and then tries to update a non-existent Song.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task UpdateSong_Test()
        {
            //Update an existing Song
            await _httpClient.DeleteAsync($"/api/Song/DeleteSong/{testSong.Title}/{testSong.Album}");
            _ = CreatePostRequest("CreateSong", testSong).Result;
            Song song = CreatePutRequest("UpdateSong", updatedSong).Result;
            AssertionTesting(song, updatedSong);

            //Update a Song that does not exists
            await _httpClient.DeleteAsync($"/api/Song/DeleteSong/{testSong.Title}/{testSong.Album}");
            HttpResponseMessage response = CreateBasicPutRequest("UpdateSong", updatedSong).Result;
            Assert.AreEqual("NotFound", response.StatusCode.ToString());
        }

        /// <summary>
        /// Deletes a Song and then tries to delete a non-existent Song.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task DeleteSong_Test()
        {
            //Create and delete an item
            _ = CreatePostRequest("CreateSong", testSong);
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/Song/DeleteSong/{testSong.Title}/{testSong.Album}");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            //Try deleting an item that does not exist
            response = await _httpClient.DeleteAsync($"/api/Song/DeleteSong/{testSong.Title}/{testSong.Album}");
            Assert.AreEqual("NotFound", response.StatusCode.ToString());
        }


        //--------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Creates a Get request and returns the Song object received.
        /// </summary>
        /// <param name="request">String containing the desired url to hit.</param>
        /// <returns>Song object that was returned by the Get request.</returns>
        private async Task<List<Song>> CreateGetRequest(string request)
        {
            var response = await _httpClient.GetAsync("/api/Song/" + request);
            var stringResult = await response.Content.ReadAsStringAsync();
            List<Song>? result = JsonConvert.DeserializeObject<List<Song>>(stringResult);

            return result ?? new List<Song> { };
        }

        /// <summary>
        /// Creates a Post request and returns the Song object received.
        /// </summary>
        /// <param name="request">String containing the desired url to hit.</param>
        /// <param name="song">Song desired to send.</param>
        /// <returns>Song object that was returned by the Post request.</returns>
        private async Task<Song> CreatePostRequest(string request, Song song)
        {
            var postJson = JsonConvert.SerializeObject(song);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            var stringResult = await _httpClient.PostAsync("/api/Song/" + request, payload).Result.Content.ReadAsStringAsync();
            Song? result = JsonConvert.DeserializeObject<Song>(stringResult);

            return result ?? new Song { };
        }

        /// <summary>
        /// Creates a Post request and returns the HttpResponseMessage object received.
        /// </summary>
        /// <param name="request">String containing the desired url to hit.</param>
        /// <param name="song">Song desired to send.</param>
        /// <returns>HttpResponseMessage object that was returned by the Post request.</returns>
        private async Task<HttpResponseMessage> CreateBasicPostRequest(string request, Song song)
        {
            var postJson = JsonConvert.SerializeObject(song);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync("/api/Song/" + request, payload);
        }

        /// <summary>
        /// Creates a Put request and returns the Song object received.
        /// </summary>
        /// <param name="request">String containing the desired url to hit.</param>
        /// <param name="song">Song desired to send.</param>
        /// <returns>Song object that was returned by the Put request.</returns>
        private async Task<Song> CreatePutRequest(string request, Song song)
        {
            var postJson = JsonConvert.SerializeObject(song);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            var stringResult = await _httpClient.PutAsync("/api/Song/" + request, payload).Result.Content.ReadAsStringAsync();
            Song? result = JsonConvert.DeserializeObject<Song>(stringResult);

            return result ?? new Song { };
        }

        /// <summary>
        /// Creates a Put request and returns the HttpResponseMessage object received.
        /// </summary>
        /// <param name="request">String containing the desired url to hit.</param>
        /// <param name="song">Song desired to send.</param>
        /// <returns>HttpResponseMessage object that was returned by the Put request.</returns>
        private async Task<HttpResponseMessage> CreateBasicPutRequest(string request, Song song)
        {
            var postJson = JsonConvert.SerializeObject(song);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync("/api/Song/" + request, payload);
        }

        /// <summary>
        /// Complete assertion testing of a Song object between two supplied Song objects.
        /// </summary>
        /// <param name="testedSong">Song being tested.</param>
        /// <param name="testingValues">Song containing the values desired to test with.</param>
        private static void AssertionTesting(Song testedSong, Song testingValues)
        {
            Assert.AreEqual(testingValues.Title, testedSong.Title);
            Assert.AreEqual(testingValues.Album, testedSong.Album);
            Assert.AreEqual(testingValues.Artist, testedSong.Artist);
            Assert.AreEqual(testingValues.Genre, testedSong.Genre);
            Assert.AreEqual(testingValues.ReleaseYear, testedSong.ReleaseYear);
        }

        /// <summary>
        /// Creates a Post request and returns bearer token of the specified user.
        /// </summary>
        /// <param name="user">User desired to send.</param>
        /// <returns>String containing the token.</returns>
        private async Task<string> GetUserToken(UserLogin user)
        {
            var postJson = JsonConvert.SerializeObject(user);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync($"/api/UserLogin/Login/{user.Username}/{user.Password}", payload).Result.Content.ReadAsStringAsync();
        }
    }
}