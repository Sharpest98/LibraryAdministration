using LibraryAdministration.Database.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace LibraryAdministration.Database.Models
{
    public class Book : ModelBase
    {
        [Key]
        public int Id { get; set; }
        public required string Author { get; set; }
        public required string Title { get; set; }
        public required int ReleaseYear { get; set; }
        public required string Genres { get; set; }
        public Reader? Reader { get; set; }
        public int? ReaderId { get; set; }
    }
}
