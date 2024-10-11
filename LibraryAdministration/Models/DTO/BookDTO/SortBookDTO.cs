namespace LibraryAdministration.Models.DTO.BookDTO
{
    public class SortBookDTO
    {
        public bool SortByTitleAsc { get; set; } = false;
        public bool SortByTitleDesc { get; set; } = false;
        public bool SortByReleaseYearAsc { get; set; } = false;
        public bool SortByReleaseYearDesc { get; set; } = false;
    }
}
