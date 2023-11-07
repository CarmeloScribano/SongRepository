using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTAPI_DynamoDB.Models;

namespace RESTAPI_DynamoDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : Controller
    {
        private readonly IDynamoDBContext _context;

        public SongController(IDynamoDBContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Creates a Song.
        /// </summary>
        /// <param name="song">The Song object desired to create.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="201">Success creating the Song.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="409">Song already exists.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("CreateSong")]
        [HttpPost]
        public async Task<ActionResult<Song>> CreateSong(Song song)
        {
            if(string.IsNullOrEmpty(song.Title.Trim()) || string.IsNullOrEmpty(song.Album.Trim()))
            {
                return StatusCode(400);
            }

            var verify = await _context
                .LoadAsync<Song>(song.Title.Trim(), song.Album.Trim());

            if (verify != null)
            {
                return StatusCode(409);
            }

            await _context.SaveAsync(song);
            return CreatedAtAction("GetSongsByTitle", new { title = song.Title.Trim() }, song);
        }

        /// <summary>
        /// Gets a prediction on a Song rating based on the user and Song supplied by the User.
        /// </summary>
        /// <param name="userId">
        /// The dataset used to train this model only includes the base three users which are:
        /// Administrator, ID: 1; 
        /// Guest, ID: 2; 
        /// WGU, ID: 3
        /// </param>
        /// <param name="songId">
        /// It also only includes 5 basic songs which are:
        /// Hail to the King, Avenged Sevenfold, ID: 1; 
        /// Faint, Linkin Park, ID: 2; 
        /// Master of Puppets, Metallica, ID: 3; 
        /// Breathing, Yellowcard, ID: 4; 
        /// Lying from You, Linkin Park, ID: 5
        /// </param>
        /// <returns>The Song recommendation.</returns>
        /// <response code="200">Success getting the Song recommentation.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongRecommendation")]
        [HttpGet]
        public IActionResult GetSongRecommendation(int userId, int songId)
        {
            if (userId < 1 || userId > 3)
            {
                return Ok("User ID was invalid, please enter an ID between 1 and 3");
            }
            else if (songId < 1 || songId > 5)
            {
                return Ok("Song ID was invalid, please enter an ID between 1 and 5");
            }

            var songRating = MLModel.Predict(new MLModel.ModelInput()
            {
                UserId = userId,
                SongId = songId,
            }).Score;

            string userName;
            string songName;

            switch (userId)
            {
                case 1: userName = "Administrator"; break;
                case 2: userName = "Guest"; break;
                case 3: userName = "WGU"; break;
                default: userName = "invalid User"; break;
            }

            switch (userId)
            {
                case 1: songName = "Hail to the King"; break;
                case 2: songName = "Faint"; break;
                case 3: songName = "Master of Puppets"; break;
                case 4: songName = "Breathing"; break;
                case 5: songName = "Lying from You"; break;
                default: songName = "invalid Song"; break;
            }

            var result = "The Song rating prediction for the Song " + songName + " on the User " + userName + " is " + Math.Round(songRating, 2);

            return Ok(result);
        }


        /// <summary>
        /// Gets a list of all Songs
        /// </summary>
        /// <returns>The list of Songs</returns>
        /// <response code="200">Success getting all the Songs.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetAllSongs")]
        [HttpGet]
        public async Task<IActionResult> GetAllSongs()
        {
            var conditions = new List<ScanCondition>();
            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }

        /// <summary>
        /// Gets Songs based on a specified Title.
        /// </summary>
        /// <param name="title">Title of the Songs that are desired.</param>
        /// <returns>Songs matching the supplied Title.</returns>
        /// <response code="200">Success getting the Songs by Title.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongsByTitle/{title}")]
        [HttpGet]
        public async Task<IActionResult> GetSongsByTitle(string title)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Title", ScanOperator.Equal, title.Trim())
            };

            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }


        /// <summary>
        /// Gets the list of Songs based on an Album.
        /// </summary>
        /// <param name="album">Album of the Songs that are desired.</param>
        /// <returns>The list of Songs</returns>
        /// <response code="200">Success getting the Songs by Album.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongsByAlbum/{album}")]
        [HttpGet]
        public async Task<IActionResult> GetSongsByAlbum(string album)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Album", ScanOperator.Equal, album.Trim())
            };

            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }


        /// <summary>
        /// Gets the list of Songs based on an Artist.
        /// </summary>
        /// <param name="artist">Artist of the Songs that are desired.</param>
        /// <returns>The list of Songs</returns>
        /// <response code="200">Success getting the Songs by Artist.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongsByArtist/{artist}")]
        [HttpGet]
        public async Task<IActionResult> GetSongsByArtist(string artist)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Artist", ScanOperator.Equal, artist.Trim())
            };

            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }


        /// <summary>
        /// Gets the list of Songs based on Genre.
        /// </summary>
        /// <param name="genre">Genre of the Songs that are desired.</param>
        /// <returns>The list of Songs</returns>
        /// <response code="200">Success getting the Songs by Genre.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongsByGenre/{genre}")]
        [HttpGet]
        public async Task<IActionResult> GetSongsByGenre(string genre)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("Genre", ScanOperator.Equal, genre.Trim())
            };

            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }


        /// <summary>
        /// Gets the list of Songs based on Release Year.
        /// </summary>
        /// <param name="releaseYear">Genre of the Songs that are desired.</param>
        /// <returns>The list of Songs</returns>
        /// <response code="200">Success getting the Songs by Release Year.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = $"{Roles.Administrator}, {Roles.Basic}")]
        [Route("GetSongsByReleaseYear/{releaseYear}")]
        [HttpGet]
        public async Task<IActionResult> GetSongsByReleaseYear(int releaseYear)
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("ReleaseYear", ScanOperator.Equal, releaseYear)
            };

            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return Ok(songs);
        }


        /// <summary>
        /// Update a specific Song that matches the specified Title and Album.
        /// </summary>
        /// <param name="song">The Song object desired to update.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="201">Success updating the Song.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="404">The Song desired to update was not found.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("UpdateSong")]
        [HttpPut]
        public async Task<IActionResult> UpdateSong(Song song)
        {
            if (string.IsNullOrEmpty(song.Title.Trim()) || string.IsNullOrEmpty(song.Album.Trim()))
            {
                return StatusCode(400);
            }

            Song specificItem = await _context.LoadAsync<Song>(song.Title.Trim(), song.Album.Trim());
            if (specificItem == null)
            {
                return NotFound();
            }

            try
            {
                await _context.SaveAsync(song);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SongExists(song.Title.Trim(), song.Album.Trim()).Result)
                {
                    NotFound();
                }
                else
                {
                    return StatusCode(503);
                }
            }

            return CreatedAtAction("GetSongsByTitle", new { title = song.Title.Trim() }, song);
        }


        /// <summary>
        /// Deletes a desired Song based on its Title and Album.
        /// </summary>
        /// <param name="title">Title of the Song that is desired.</param>
        /// <param name="album">Album of the Song that is desired.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="204">Success deleting the Songs.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="404">The Song desired to delete was not found.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("DeleteSong/{title}/{album}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSong(string title, string album)
        {
            var song = await _context.LoadAsync<Song>(title.Trim(), album.Trim());
            if (song == null)
            {
                return NotFound();
            }

            await _context.DeleteAsync(song);

            return NoContent();
        }


        /// <summary>
        /// Private function that verifies if a Song already exists.
        /// </summary>
        /// <param name="title">Title of the Song that is desired.</param>
        /// <param name="album">Album of the Song that is desired.</param>
        /// <returns>A Boolean stating if the Song already exists.</returns>
        [NonAction]
        private async Task<bool> SongExists(string title, string album)
        {
            var conditions = new List<ScanCondition>();
            var songs = await _context
                .ScanAsync<Song>(conditions)
                .GetRemainingAsync();

            return songs.ToList().Any(x => x.Title == title.Trim() && x.Album == album.Trim());
        }
    }
}
