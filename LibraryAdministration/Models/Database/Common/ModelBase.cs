using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAdministration.Models.Database.Common
{
    public abstract class ModelBase
    {
        public required DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public required string CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
