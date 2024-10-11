namespace LibraryAdministration.Models.DTO.BookDTO
{
    public class CreateBookDTO
    {
        public required string Author { get; set; }
        public required string Title { get; set; }
        public required int ReleaseYear { get; set; }
        public required string Genre { get; set; }
    }
}
