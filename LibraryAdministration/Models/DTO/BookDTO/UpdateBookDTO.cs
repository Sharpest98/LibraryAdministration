using LibraryAdministration.Models.Database;

namespace LibraryAdministration.Models.DTO.BookDTO
{
    public class UpdateBookDTO
    {
        public required int Id { get; set; }
        public string? Author { get; set; }
        public string? Title { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Genre { get; set; }
        
    }
}
