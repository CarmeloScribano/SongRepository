using System.Diagnostics.CodeAnalysis;

namespace RESTAPI_DynamoDB.Models
{
    public class Song
    {
        private string title = "Default Title";
        private string album = "Defautl Album";

        [NotNull]
        public string Title { get => title; set => title = value; }
        [NotNull]
        public string Album { get => album; set => album = value; }
        public string? Artist { get; set; }
        public string? Genre { get; set; }
        public int? ReleaseYear { get; set; }
    }
}
