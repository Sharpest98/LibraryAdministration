using System.ComponentModel.DataAnnotations;

namespace LibraryAdministration.Models.Database
{
    public class LibraryAdministrator
    {
        [Key]
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
    }
}
