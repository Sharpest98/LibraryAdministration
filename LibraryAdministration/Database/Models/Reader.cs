using LibraryAdministration.Database.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace LibraryAdministration.Database.Models
{
    public class Reader : ModelBase
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public required string PhoneNumber { get; set; }
    }
}
