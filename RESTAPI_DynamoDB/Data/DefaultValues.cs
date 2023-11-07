using RESTAPI_DynamoDB.Models;
using RESTAPI_DynamoDB.Utilities;

namespace RESTAPI_DynamoDB.Data
{
    public class DefaultValues
    {
        public static readonly List<Song> songs = new()
        {
            new Song() {Title = "Master of Puppets", Album = "Master of Puppets", Artist = "Metallica", Genre = "Heavy Metal", ReleaseYear = 1986},
            new Song() {Title = "Hail to the King", Album = "Hail to the King", Artist = "Avenged Sevenfold", Genre = "Heavy Metal", ReleaseYear = 2013},
            new Song() {Title = "Breathing", Album = "Ocean Avenue", Artist = "Yellowcard", Genre = "Pop Punk", ReleaseYear = 2003},
            new Song() {Title = "Lying from You", Album = "Meteora", Artist = "Linkin Park", Genre = "Alternative", ReleaseYear = 2003},
            new Song() {Title = "Faint", Album = "Meteora", Artist = "Linkin Park", Genre = "Alternative", ReleaseYear = 2003,}
        };

        public static readonly List<User> users = new()
        {
            new User() {Username = "admin", Password = PasswordHasher.SHA512Hasher("admin"), Age = 100, Role = "admin"},
            new User() {Username = "guest", Password = PasswordHasher.SHA512Hasher("password"), Age = 21, Role = "basic"}
        };
    }
}
