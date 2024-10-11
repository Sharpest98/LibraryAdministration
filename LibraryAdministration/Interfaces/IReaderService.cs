using LibraryAdministration.Models.Database;
using LibraryAdministration.Models.DTO.ReaderDTO;

namespace LibraryAdministration.Interfaces
{
    public interface IReaderService
    {
        Task<ReaderDTO> CreateReader(CreateReaderDTO createReaderDTO);

        Task<ReaderDTO> UpdateReader(UpdateReaderDTO updateReaderDTO);

        Task DeleteReader(int id);

        Task<List<ReaderDTO>> GetAll();

        Task<List<ReaderDTO>> SearchReader(SearchReaderDTO searchReaderDTO);

        Task<List<ReaderDTO>> SortReader(SortReaderDTO sortDTO);

        Task<ReaderInfoDTO> GetInfoAboutReader(int id);
    }
}
