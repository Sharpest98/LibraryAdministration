using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAdministration.Database.Models.Common
{
    public abstract class ModelBase
    {
        public DateTime CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public required string CreatedBy { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
