using AutoMapper;
using AutoMapper.Features;
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
using System.Security.Claims;

namespace LibraryAdministration.Tests
{
    public class BookServiceTests
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
        public async Task Should_Create_Book()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            createdBook.Should().NotBeNull();
            createdBook.Author.Should().Be(createBookDto.Author);
            createdBook.Title.Should().Be(createBookDto.Title);
            createdBook.ReleaseYear.Should().Be(createBookDto.ReleaseYear);
            createdBook.Genre.Should().Be(createBookDto.Genre);
        }

        [Test]
        public void Should_NotCreate_Book_DTO_IsNull()
        {
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await _bookService.CreateBook(null!));
            exception.Message.Should().Contain("createBookDTO");
        }

        [Test]
        public void Should_NotDelete_Book_ObjectIsNotFound()
        {
            var id = 0;
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.DeleteBook(id));
            exception.Message.Should().Contain(nameof(Book));
            exception.Message.Should().Contain(id.ToString());
        }

        [Test]
        public async Task Should_NotDelete_Book_ReaderIdIsNotNull()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var takeFromLibraryDto = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id
            };

            await _bookService.TakeBookFromLibrary(takeFromLibraryDto);

            Assert.ThrowsAsync<ImpossibleToDeleteBookException>(async () => await _bookService.DeleteBook(createdBook.Id));
        }

        [Test]
        public async Task Should_Delete_Book()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            Assert.DoesNotThrowAsync(async () => await _bookService.DeleteBook(createdBook.Id));
        }

        [Test]
        public async Task Should_GetAll_Book()
        {
            var bookDto = GetCreateBookDTO();
            await _bookService.CreateBook(bookDto);
            await _bookService.CreateBook(bookDto);

            var books = await _bookService.GetAll();
            books.Should().HaveCount(2);
        }

        [Test]
        public async Task Should_GetBook()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var bookInfo = await _bookService.GetBook(createdBook.Id);
            bookInfo.Should().NotBeNull();

            bookInfo.Id.Should().Be(createdBook.Id);
            bookInfo.Author.Should().Be(createdBook.Author);
            bookInfo.Title.Should().Be(createdBook.Title);
            bookInfo.Genre.Should().Be(createdBook.Genre);
            bookInfo.ReleaseYear.Should().Be(createdBook.ReleaseYear);
        }

        [Test]
        public void Should_NotGet_Book_ObjectNotFound()
        {
            int id = 0;
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.GetBook(id));
            exception.Message.Should().Contain(nameof(Book));
            exception.Message.Should().Contain(id.ToString());
        }

        [Test]
        public async Task Should_ReturnBook()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var takeFromLibraryDto = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id
            };

            await _bookService.TakeBookFromLibrary(takeFromLibraryDto);

            Assert.DoesNotThrowAsync(async () => await _bookService.ReturnBookToLibrary(takeFromLibraryDto));
        }

        [Test]
        public async Task Should_NotReturnBook_ReaderIsNotFound()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            int wrongReaderId = 10;
            var returnToLibrary = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = wrongReaderId
            };

            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.ReturnBookToLibrary(returnToLibrary));
            exception.Message.Should().Contain(nameof(Reader));
            exception.Message.Should().Contain(wrongReaderId.ToString());
        }

        [Test]
        public async Task Should_NotReturnBook_BookIsNotFound()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            int wrongId = 10;
            var returnToLibrary = new BookReaderDTO
            {
                BookId = wrongId,
                ReaderId = createdReader.Id
            };

            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.ReturnBookToLibrary(returnToLibrary));
            exception.Message.Should().Contain(nameof(Book));
            exception.Message.Should().Contain(wrongId.ToString());
        }

        [Test]
        public async Task Should_NotReturnBook_BookIsAlreadyInLibrary()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var returnToLibrary = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id
            };

            var exception = Assert.ThrowsAsync<BookIsAlreadyInLibraryException>(async () => await _bookService.ReturnBookToLibrary(returnToLibrary));
            exception.Message.Should().Contain(createdBook.Id.ToString());
        }

        [Test]
        public async Task Should_NotReturnBook_InvalidDataProvided()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var returnToLibrary = new BookReaderDTO
            {
                BookId = 0,
                ReaderId = 0
            };

            Assert.ThrowsAsync<InvalidDataProvidedException>(async () => await _bookService.ReturnBookToLibrary(returnToLibrary));
        }

        [Test]
        public async Task Should_Find_OneBook_ByText()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var searchInput = new SearchBookDTO()
            {
                SearchText = "Ivan"
            };
            var foundBook = await _bookService.SearchBook(searchInput);

            foreach (var book in foundBook)
            {
                book.Author.Should().Contain("Ivan");
            }
        }

        [Test]
        public async Task Should_Find_All_Books_ByText()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var searchInput = new SearchBookDTO()
            {
                SearchText = "book"
            };
            var foundBook = await _bookService.SearchBook(searchInput);

            foreach (var book in foundBook)
            {
                book.Title.ToLower().Should().Contain("book");
            }
        }

        [Test]
        public async Task Should_Find_All_Books_ByYear()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2024,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);

            var searchInput = new SearchBookDTO()
            {
                YearOfRelease = 2024
            };
            var foundBook = await _bookService.SearchBook(searchInput);

            foreach (var book in foundBook)
            {
                book.ReleaseYear.Should().Be(2024);
            }
        }

        [Test]
        public async Task Should_Find_One_Books_ByYear()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var searchInput = new SearchBookDTO()
            {
                YearOfRelease = 2020
            };
            var foundBook = await _bookService.SearchBook(searchInput);

            foreach (var book in foundBook)
            {
                book.ReleaseYear.Should().Be(2020);
            }
        }
        [Test]
        public async Task Should_Sort_Books_ByYearAsc()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);

            var sortCriteria = new SortBookDTO()
            {
                SortByReleaseYearAsc = true
            };
            var sortedBooks = await _bookService.Sort(sortCriteria);
            sortedBooks[0].ReleaseYear.Should().Be(createdBookDTO2.ReleaseYear);
        }
        [Test]
        public async Task Should_Sort_Books_ByYearDesc()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var sortCriteria = new SortBookDTO()
            {
                SortByReleaseYearDesc = true
            };
            var sortedBooks = await _bookService.Sort(sortCriteria);
            sortedBooks[0].ReleaseYear.Should().Be(createdBook.ReleaseYear);
        }

        [Test]
        public async Task Should_Sort_Books_ByTitleAsc()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var sortCriteria = new SortBookDTO()
            {
                SortByTitleAsc = true
            };
            var sortedBooks = await _bookService.Sort(sortCriteria);
            sortedBooks[0].Title.Should().Be(createdBookDTO2.Title);
        }

        [Test]
        public async Task Should_Sort_Books_ByTitleDesc()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);
            var createdBookDTO2 = new CreateBookDTO()
            {
                Author = "Ivan",
                Title = "Book",
                ReleaseYear = 2020,
                Genre = "Comedy"
            };
            var createdBook2 = await _bookService.CreateBook(createdBookDTO2);
            var sortCriteria = new SortBookDTO()
            {
                SortByTitleDesc = true
            };
            var sortedBooks = await _bookService.Sort(sortCriteria);
            sortedBooks[0].Title.Should().Be(createdBook.Title);
        }

        [Test]
        public async Task Should_TakeBook()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var takeBookFromLibrary = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id
            };
            Assert.DoesNotThrowAsync(async () => await _bookService.TakeBookFromLibrary(takeBookFromLibrary));
        }

        [Test]
        public async Task Should_NotTakeBook_InvalidDataProvided()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var takeBookFromLibrary = new BookReaderDTO
            {
                BookId = 0,
                ReaderId = 0
            };
            Assert.ThrowsAsync<InvalidDataProvidedException>(async () => await _bookService.TakeBookFromLibrary(takeBookFromLibrary));
        }

        [Test]
        public async Task Should_NotTakeBook_BookIsNotFound()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            int WrongId = 10;
            var takeBookFromLibrary = new BookReaderDTO
            {
                BookId = WrongId,
                ReaderId = createdReader.Id
            };
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.TakeBookFromLibrary(takeBookFromLibrary));
            exception.Message.Contains(nameof(Book));
            exception.Message.Contains(WrongId.ToString());
        }

        [Test]
        public async Task Should_NotTakeBook_ReaderIsNotFound()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            int WrongId = 10;
            var takeBookFromLibrary = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = WrongId
            };
            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.TakeBookFromLibrary(takeBookFromLibrary));
            exception.Message.Contains(nameof(Reader));
            exception.Message.Contains(WrongId.ToString());
        }

        [Test]
        public async Task Should_NotTakeBook_BookIsAlreadyTaken()
        {
            var createBookDto = GetCreateBookDTO();
            var createdBook = await _bookService.CreateBook(createBookDto);

            var createReaderDto = GetCreateReaderDTO();
            var createdReader = await _readerService.CreateReader(createReaderDto);

            var takeBookFromLibrary = new BookReaderDTO
            {
                BookId = createdBook.Id,
                ReaderId = createdReader.Id
            };
            await _bookService.TakeBookFromLibrary(takeBookFromLibrary);
            var exception = Assert.ThrowsAsync<BookIsAlreadyTakenException>(async () => await _bookService.TakeBookFromLibrary(takeBookFromLibrary));
            exception.Message.Contains(createdReader.Id.ToString());
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

            var updatedBook =  await _bookService.UpdateBook(updateBookDto);
            updatedBook.Should().NotBeNull();
            updatedBook.Id.Should().Be(updateBookDto.Id);
            updatedBook.Author.Should().Be(updateBookDto.Author);
            updatedBook.Title.Should().Be(updateBookDto.Title);
            updatedBook.ReleaseYear.Should().Be(updateBookDto.ReleaseYear);
            updatedBook.Genre.Should().Be(updateBookDto.Genre);
        }

        [Test]
        public void Should_NotUpdate_Book_IsNull()
        {
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await _bookService.UpdateBook(null!));
            exception.Message.Should().Contain("updateBookDTO");
        }

        [Test]
        public void Should_NotUpdate_Book_IsNotFound()
        {
            var updateBookDto = new UpdateBookDTO()
            {
                Id = 1,
                Author = "UpdatedAuthor",
                Title = "UpdatedTitle",
                ReleaseYear = 1900,
                Genre = "UpdatedGenre"
            };

            var exception = Assert.ThrowsAsync<ObjectNotFoundException>(async () => await _bookService.UpdateBook(updateBookDto));
            exception.Message.Should().Contain(nameof(Book));
            exception.Message.Should().Contain("1");
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


    }
}