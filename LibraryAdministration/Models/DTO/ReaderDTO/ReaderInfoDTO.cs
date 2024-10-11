using LibraryAdministration.Models.Database;
namespace LibraryAdministration.Models.DTO.ReaderDTO
{
    public class ReaderInfoDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }

        public List<BookDTO.BookDTO>? ListOfBooks { get; set; }
    }
}
