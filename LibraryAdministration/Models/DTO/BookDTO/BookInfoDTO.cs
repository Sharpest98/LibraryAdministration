namespace LibraryAdministration.Models.DTO.BookDTO
{
    public class BookInfoDTO
    {
        public required int Id { get; set; }
        public required string Author { get; set; }
        public required string Title { get; set; }
        public required int ReleaseYear { get; set; }
        public required string Genre { get; set; }
        public int? ReaderId { get; set; }

        public string? ReaderName { get; set; }
        public string? ReaderLastName { get; set; }
        public string? ReaderAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
