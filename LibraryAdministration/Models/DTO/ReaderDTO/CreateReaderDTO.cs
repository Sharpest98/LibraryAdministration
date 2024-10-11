namespace LibraryAdministration.Models.DTO.ReaderDTO
{
    public class CreateReaderDTO
    {
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
