using FluentAssertions;
using LibraryAdministration.Database;
using LibraryAdministration.Exceptions;
using LibraryAdministration.Models.DTO.AccountDTO;
using LibraryAdministration.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdministration.Tests
{
    
    public class LibraryAdministratorServiceTests
    {
        private LibraryAdministratorService _libraryAdministratorService;
        private DataContext _dataContext;
        private SqliteConnection _connection;

        [SetUp]
        public void OneTimeSetup()
        {
            var connection = new SqliteConnectionStringBuilder()
            {
                DataSource = ":memory:"
            }.ToString();

            _connection = new SqliteConnection(connection);
            _connection.Open();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(_connection)
                .Options;

            _dataContext = new DataContext(options);
            _dataContext.Database.EnsureCreated();

            _libraryAdministratorService = new LibraryAdministratorService(_dataContext);
        }

        [TearDown]
        public void OneTimeTearDown()
        {
            _connection.Close();
            _connection.Dispose();
            _dataContext.Database.EnsureDeleted();
            _dataContext.Dispose();
        }

        [Test]
        public void Should_RegisterUser()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            Assert.DoesNotThrowAsync(async () => await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO));
        }

        [Test]
        public async Task Should_NotRegisterUser_UserNameAlreadyExist()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO);
            var exception = Assert.ThrowsAsync<UserAlreadyExistsException>( async() => await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO));
            exception.Message.Should().Contain(libraryAdministratiorDTO.UserName);
        }

        [Test]
        public async Task Should_Login()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO);

            var loginDTO = new LoginDTO()
            {
                UserName = libraryAdministratiorDTO.UserName,
                Password = libraryAdministratiorDTO.Password
            };

            Assert.DoesNotThrowAsync(() => _libraryAdministratorService.Login(loginDTO));
        }

        [Test]
        public async Task Should_NotLogin_InvalidUserName()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO);

            var loginDTO = new LoginDTO()
            {
                UserName = "Uncorrect",
                Password = libraryAdministratiorDTO.Password
            };

            Assert.ThrowsAsync<InvalidCredentialsException>(() => _libraryAdministratorService.Login(loginDTO));
        }

        [Test]
        public async Task Should_NotLogin_InvalidPassword()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO);

            var loginDTO = new LoginDTO()
            {
                UserName = libraryAdministratiorDTO.UserName,
                Password = "Wrong"
            };

            Assert.ThrowsAsync<InvalidCredentialsException>(() => _libraryAdministratorService.Login(loginDTO));
        }


        private LibraryAdministratorDTO GetLibraryAdministratorDTO()
        {
            return new LibraryAdministratorDTO()
            {
                UserName = "Admin",
                Name = "Name",
                LastName = "LastName",
                Password = "Password"
            };

        }
    }
}
