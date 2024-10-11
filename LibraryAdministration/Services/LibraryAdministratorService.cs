using LibraryAdministration.Database;
using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.Database;
using LibraryAdministration.Models.DTO.AccountDTO;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdministration.Services
{
    public class LibraryAdministratorService : ILibraryAdministratorService
    {
        private readonly DataContext _context;
        public LibraryAdministratorService(DataContext dataContext)
        { 
            _context = dataContext;
        }
    
        public async Task UserRegistration(LibraryAdministratorDTO libraryAdministratorDTO)
        {
            var libraryAdministrator = await _context.LibraryAdministrator.FirstOrDefaultAsync(u => u.UserName == libraryAdministratorDTO.UserName);
            if (libraryAdministrator != null)
            {
                throw new UserAlreadyExistsException(libraryAdministrator.UserName);
            }

            CreatePasswordHash(libraryAdministratorDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            libraryAdministrator = new LibraryAdministrator
            {
                UserName = libraryAdministratorDTO.UserName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Name = libraryAdministratorDTO.Name,
                LastName = libraryAdministratorDTO.LastName
            };

            _context.LibraryAdministrator.Add(libraryAdministrator);
            await _context.SaveChangesAsync();
        }

        public async Task Login(LoginDTO loginDTO)
        {
            var libraryAdministrator = await _context.LibraryAdministrator.FirstOrDefaultAsync(l => l.UserName == loginDTO.UserName);
            if(libraryAdministrator == null)
            {
                throw new InvalidCredentialsException();
            }
            
            if(!VerifyPasswordHash(loginDTO.Password, libraryAdministrator.PasswordHash, libraryAdministrator.PasswordSalt))
            {
                throw new InvalidCredentialsException();
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
