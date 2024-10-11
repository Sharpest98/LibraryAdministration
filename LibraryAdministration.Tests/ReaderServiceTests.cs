using AutoMapper;
using FluentAssertions;
using LibraryAdministration.Database;
using LibraryAdministration.Exceptions;
using LibraryAdministration.Models.Database;
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
    internal class ReaderServiceTests
    {
        private ReaderService _readerService;
        private BookService _bookService;
        private DataContext _dataContext;
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
        public async Task Should_Create_Reader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            createdReader.Should().NotBeNull();
            createdReader.Name.Should().Be(createReaderDTO.Name);
            createdReader.LastName.Should().Be(createReaderDTO.LastName);
            createdReader.Address.Should().Be(createReaderDTO.Address);
            createdReader.PhoneNumber.Should().Be(createReaderDTO.PhoneNumber);
        }

        [Test]
         public void Should_NotCreate_Reader_DTOIsNull()
        {
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await _readerService.CreateReader(null!));
            exception.Message.Should().Contain("createReaderDTO");
        }

        [Test]
        public async Task Should_Delete_Reader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            Assert.DoesNotThrowAsync(async () => await _readerService.DeleteReader(createdReader.Id));
        }

        [Test]
        public void Should_NotDelete_ReaderIsNotFound()
        {
            int id = 0;
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _readerService.DeleteReader(id));
            exception.Message.Should().Contain(nameof(Reader));
            exception.Message.Should().Contain(id.ToString());
        }

        [Test]
        public async Task Should_NotDelete_ReaderHasBook()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var takeBookDto = new BookReaderDTO()
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id,
            };
            await _bookService.TakeBookFromLibrary(takeBookDto);

            var exception = Assert.ThrowsAsync<ImpossibleToDeleteReaderException>(async () => await _readerService.DeleteReader(createdReader.Id));
        }

        [Test]
        public async Task Should_GetAll_Readers()
        {
            var createReaderDTO = GetCreateReaderDTO();
            await _readerService.CreateReader(createReaderDTO);
            await _readerService.CreateReader(createReaderDTO);

            var readers = await _readerService.GetAll();
            readers.Should().HaveCount(2);

        }

        [Test]
        public async Task Should_GetInfoAbout_Reader()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var reader = await _readerService.GetInfoAboutReader(createdReader.Id);
            reader.Should().NotBeNull();
            reader.Name.Should().Be(createdReader.Name);
            reader.LastName.Should().Be(createdReader.LastName);
            reader.PhoneNumber.Should().Be(createdReader.PhoneNumber);
            reader.Address.Should().Be(createdReader.Address);
        }

        [Test]
        public void Should_NotGetInfoAbout_Reader_IsNotFound()
        {
            int id = 0;
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _readerService.GetInfoAboutReader(id));
            exception.Message.Should().Contain(nameof(Reader));
            exception.Message.Should().Contain(id.ToString());
        }

        [Test]
        public async Task Should_Find_AllReaders_ByText()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var searchDTO = new SearchReaderDTO()
            {
                SearchByText = "Address"
            };

            var readers = await _readerService.SearchReader(searchDTO);
            readers.Should().HaveCount(2);
            foreach (var reader in readers)
            {
                reader.Address.Should().Contain("Address");
            }
        }

        [Test]
        public async Task Should_Find_OneReader_ByText()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var searchDTO = new SearchReaderDTO()
            {
                SearchByText = "Io"
            };

            var readers = await _readerService.SearchReader(searchDTO);
            readers.Should().HaveCount(1);
            foreach (var reader in readers)
            {
                reader.Name.Should().Contain("Io");
                reader.LastName.Should().Contain("Io");
            }
        }

        [Test]
        public async Task Should_SortReaders_ByNameAsc()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var sortDTO = new SortReaderDTO()
            {
                SortByNameAsc = true
            };

            var readers = await _readerService.SortReader(sortDTO);
            readers[0].Name.Should().Be(createdReader2.Name);
        }

        [Test]
        public async Task Should_SortReaders_ByNameDesc()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var sortDTO = new SortReaderDTO()
            {
                SortByNameDesc = true
            };

            var readers = await _readerService.SortReader(sortDTO);
            readers[0].Name.Should().Be(createdReader.Name);
        }

        [Test]
        public async Task Should_SortReaders_ByLastNameAsc()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var sortDTO = new SortReaderDTO()
            {
                SortByLastNameAsc = true
            };

            var readers = await _readerService.SortReader(sortDTO);
            readers[0].LastName.Should().Be(createdReader2.LastName);
        }

        [Test]
        public async Task Should_SortReaders_ByLastNameDesc()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);

            var createReaderDTO2 = new CreateReaderDTO()
            {
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Address",
                PhoneNumber = "27180991"
            };
            var createdReader2 = await _readerService.CreateReader(createReaderDTO2);
            var sortDTO = new SortReaderDTO()
            {
                SortByLastNameDesc = true
            };

            var readers = await _readerService.SortReader(sortDTO);
            readers[0].LastName.Should().Be(createdReader.LastName);
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

            var updatedReader  = await _readerService.UpdateReader(updateReaderDTO);

            updatedReader.Should().NotBeNull();
            updatedReader.Id.Should().Be(updatedReader.Id);
            updatedReader.Name.Should().Be(updateReaderDTO.Name);
            updatedReader.LastName.Should().Be(updateReaderDTO.LastName);
            updatedReader.Address.Should().Be(updateReaderDTO.Address);
            updatedReader.PhoneNumber.Should().Be(updateReaderDTO.PhoneNumber);
        }

        [Test]
        public void Should_NotUpdate_Reader_IsNull()
        {
            var exception = Assert.ThrowsAsync< ArgumentNullException>( async() => await _readerService.UpdateReader(null!));
            exception.Message.Should().Contain("updateReaderDTO");
        }

        [Test]
        public async Task Should_NotUpdate_Reader_IsNotFound()
        {
            var createReaderDTO = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDTO);
            int wrongId = 10;
            var updateReaderDTO = new UpdateReaderDTO()
            {
                Id = wrongId,
                Name = "Ioan",
                LastName = "Ionov",
                Address = "Moldova street 99",
                PhoneNumber = "27180991"
            };

            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _readerService.UpdateReader(updateReaderDTO));
            exception.Message.Should().Contain(nameof(Reader));
            exception.Message.Should().Contain(wrongId.ToString());
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
    }
}
