using AutoMapper;
using FluentAssertions;
using LibraryAdministration.Database;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.DTO.AccountDTO;
using LibraryAdministration.Models.DTO.BookDTO;
using LibraryAdministration.Models.DTO.ReaderDTO;
using LibraryAdministration.Models.Mapping;
using LibraryAdministration.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibraryAdministration.Tests
{
    internal class DataBaseTests
    {
        private BookService _bookService;
        private ReaderService _readerService;
        private DataContext _dataContext;
        private LibraryAdministratorService _libraryAdministratorService;
        private SqliteConnection _connection;

        [SetUp]
        public void OneTimeSetup()
        {
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                 {
                    new Claim(ClaimTypes.Name, "default"),
                    new Claim(ClaimTypes.Role, "default")
                 }))
            };
            var httpContext = new Mock<IHttpContextAccessor>();
            httpContext.Setup(x => x.HttpContext).Returns(context);

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

            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            var mapper = new Mapper(configuration);
            _bookService = new BookService(_dataContext, httpContext.Object, mapper);
            _readerService = new ReaderService(_dataContext, httpContext.Object, mapper);
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
        public async Task Should_Create_Book()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            _dataContext.Book.First().Should().NotBeNull();
            _dataContext.Book.First().Author.Should().Be(createBookDto.Author);
            _dataContext.Book.First().Title.Should().Be(createBookDto.Title);
            _dataContext.Book.First().ReleaseYear.Should().Be(createBookDto.ReleaseYear);
            _dataContext.Book.First().Genre.Should().Be(createBookDto.Genre);
        }

        [Test]
        public async Task Should_Create_Reader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            _dataContext.Reader.First().Should().NotBeNull();
            _dataContext.Reader.First().Name.Should().Be(createReaderDTO.Name);
            _dataContext.Reader.First().LastName.Should().Be(createReaderDTO.LastName);
            _dataContext.Reader.First().Address.Should().Be(createReaderDTO.Address);
            _dataContext.Reader.First().PhoneNumber.Should().Be(createReaderDTO.PhoneNumber);
        }

        [Test]
        public async Task Should_Delete_Book()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            _dataContext.Book.Count().Should().Be(1);
            await _bookService.DeleteBook(createdBook.Id);
            _dataContext.Book.Count().Should().Be(0);
        }

        [Test]
        public async Task Should_Delete_Reader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);
            _dataContext.Reader.Count().Should().Be(1);
            await _readerService.DeleteReader(createdReader.Id);
            _dataContext.Reader.Count().Should().Be(0);
        }

        [Test]
        public async Task Should_UpdateBook()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var updateBookDto = new UpdateBookDTO()
            {
                Id = createdBook.Id,
                Author = "UpdatedAuthor",
                Title = "UpdatedTitle",
                ReleaseYear = 1900,
                Genre = "UpdatedGenre"
            };

            await _bookService.UpdateBook(updateBookDto);
            _dataContext.Book.First().Should().NotBeNull();
            _dataContext.Book.First().Id.Should().Be(updateBookDto.Id);
            _dataContext.Book.First().Author.Should().Be(updateBookDto.Author);
            _dataContext.Book.First().Title.Should().Be(updateBookDto.Title);
            _dataContext.Book.First().ReleaseYear.Should().Be(updateBookDto.ReleaseYear);
            _dataContext.Book.First().Genre.Should().Be(updateBookDto.Genre);
        }

        [Test]
        public async Task Should_UpdateReader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var updateReaderDTO = new UpdateReaderDTO()
            {
                Id = createdReader.Id,
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Moldova street 99",
                PhoneNumber = "27180991"
            };

            await _readerService.UpdateReader(updateReaderDTO);

            _dataContext.Reader.First().Should().NotBeNull();
            _dataContext.Reader.First().Id.Should().Be(updateReaderDTO.Id);
            _dataContext.Reader.First().Name.Should().Be(updateReaderDTO.Name);
            _dataContext.Reader.First().LastName.Should().Be(updateReaderDTO.LastName);
            _dataContext.Reader.First().Address.Should().Be(updateReaderDTO.Address);
            _dataContext.Reader.First().PhoneNumber.Should().Be(updateReaderDTO.PhoneNumber);
        }

        [Test]
        public async Task Should_RegisterUser()
        {
            var libraryAdministratiorDTO = GetLibraryAdministratorDTO();
            await _libraryAdministratorService.UserRegistration(libraryAdministratiorDTO);

            _dataContext.LibraryAdministrator.Should().NotBeNull();
            _dataContext.LibraryAdministrator.First().UserName.Should().Be(libraryAdministratiorDTO.UserName);
            _dataContext.LibraryAdministrator.First().Name.Should().Be(libraryAdministratiorDTO.Name);
            _dataContext.LibraryAdministrator.First().LastName.Should().Be(libraryAdministratiorDTO.LastName);
        }

        private CreateReaderDTO GetCreateReaderDTO()
        {
            return new CreateReaderDTO()
            {
                Name = "Name",
                LastName = "LastName",
                Address = "Address",
                PhoneNumber = "1234567890"
            };
        }
        private CreateBookDTO GetCreateBookDTO()
        {
            return new CreateBookDTO()
            {
                Author = "Kostja",
                Title = "Name of book",
                ReleaseYear = 2024,
                Genre = "Roman"
            };
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
