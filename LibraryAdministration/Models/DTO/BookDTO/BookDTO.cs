using LibraryAdministration.Models.Database;

namespace LibraryAdministration.Models.DTO.BookDTO
{
    public class BookDTO
    {
        public required int Id { get; set; }
        public required string Author { get; set; }
        public required string Title { get; set; }
        public required int ReleaseYear { get; set; }
        public required string Genre { get; set; }
        public int? ReaderId { get; set; }
    }
}
