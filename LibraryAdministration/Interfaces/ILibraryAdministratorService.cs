using LibraryAdministration.Models.DTO.AccountDTO;

namespace LibraryAdministration.Interfaces
{
    public interface ILibraryAdministratorService
    {
        Task UserRegistration(LibraryAdministratorDTO libraryAdministratorDTO);

        Task Login(LoginDTO loginDTO);
    }
}
