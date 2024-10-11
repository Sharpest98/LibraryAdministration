using AutoMapper;
using LibraryAdministration.Database;
using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.Database;
using LibraryAdministration.Models.DTO.BookDTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryAdministration.Services
{
    public class BookService : IBookService
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public BookService(
            DataContext dataContext,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<BookDTO> CreateBook(CreateBookDTO createBookDTO)
        {
            if (createBookDTO == null)
            {
                throw new ArgumentNullException(nameof(createBookDTO));
            }
            var book  = _mapper.Map<Book>(createBookDTO);

            book.CreatedOn = DateTime.Now;
           // book.CreatedBy = "default";//_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;
            book.CreatedBy = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;


            _dataContext.Book.Add(book);
            await _dataContext.SaveChangesAsync();
            return _mapper.Map<BookDTO>(book);

        }

        public async Task DeleteBook(int id)
        {
            var book = await _dataContext.Book.FirstOrDefaultAsync( book => book.Id == id);
            if (book is null)
            {
                throw new ObjectNotFoundException(nameof(Book), id);
            }
            if (book.ReaderId != null)
            {
                throw new ImpossibleToDeleteBookException();
            }

            _dataContext.Book.Remove(book);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<BookDTO>> GetAll()
        {
            var books = await _dataContext.Book.Select(x => _mapper.Map<BookDTO>(x)).ToListAsync();
            
            return books;
        }

        public async Task<BookInfoDTO> GetBook(int id)
        {
            var book = await _dataContext.Book.Include(x => x.Reader).FirstOrDefaultAsync(b => b.Id == id);
            if(book is null)
            {
                throw new ObjectNotFoundException(nameof(Book),id);
            }
            BookInfoDTO bookInfo = new BookInfoDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ReleaseYear = book.ReleaseYear,
                Genre = book.Genre
            };
            if(book.Reader != null)
            {
                bookInfo.ReaderName = book.Reader.Name;
                bookInfo.ReaderLastName = book.Reader.LastName;
                bookInfo.ReaderAddress = book.Reader.Address;
                bookInfo.PhoneNumber = book.Reader.PhoneNumber;
            }


            return bookInfo;
        }

        public async Task ReturnBookToLibrary(BookReaderDTO returnBookDTO)
        {
            if (returnBookDTO.BookId != 0 && returnBookDTO.ReaderId != 0)
            {
                var book = await _dataContext.Book.FirstOrDefaultAsync(b => b.Id == returnBookDTO.BookId);
                var reader = await _dataContext.Reader.FirstOrDefaultAsync(b => b.Id == returnBookDTO.ReaderId);
                if (book == null)
                {
                    throw new ObjectNotFoundException(nameof(Book), returnBookDTO.BookId);
                }
                if (reader == null)
                {
                    throw new ObjectNotFoundException(nameof(Reader), returnBookDTO.ReaderId);
                }
                if (book.ReaderId == null)
                {
                    throw new BookIsAlreadyInLibraryException(returnBookDTO.BookId);
                }
                if (book.ReaderId != returnBookDTO.ReaderId)
                {
                    throw new ObjectNotFoundException(nameof(Reader), returnBookDTO.ReaderId);
                }
                

                book.ReaderId = null;
                await _dataContext.SaveChangesAsync();
            }
            else
            {
                throw new InvalidDataProvidedException();
            }
        }

        public async Task<List<BookDTO>> SearchBook(SearchBookDTO searchBookDTO)
        {
            if(searchBookDTO?.SearchText != null && searchBookDTO?.YearOfRelease != null)
            {
                var books = await _dataContext.Book.Where(x =>
                    (x.Title.ToLower().Contains(searchBookDTO.SearchText.ToLower()) ||
                    x.Author.ToLower().Contains(searchBookDTO.SearchText.ToLower()) ||
                    x.Genre.ToLower().Contains(searchBookDTO.SearchText.ToLower())) &&
                    x.ReleaseYear == searchBookDTO.YearOfRelease
                    ).Select(x => _mapper.Map<BookDTO>(x)).ToListAsync();
                return books;
            }
            else if (searchBookDTO?.SearchText is not null)
            {
                var books = await _dataContext.Book.Where(x =>
                    x.Title.ToLower().Contains(searchBookDTO.SearchText.ToLower()) ||
                    x.Author.ToLower().Contains(searchBookDTO.SearchText.ToLower()) ||
                    x.Genre.ToLower().Contains(searchBookDTO.SearchText.ToLower())
                    ).Select(x => _mapper.Map<BookDTO>(x)).ToListAsync();
                return books;
            }
            else if (searchBookDTO?.YearOfRelease is not null)
            {
                var books = await _dataContext.Book.Where(x => x.ReleaseYear == searchBookDTO.YearOfRelease)
                    .Select(x => _mapper.Map<BookDTO>(x))
                    .ToListAsync();
                return books;
            }
            return await _dataContext.Book.Select(x => _mapper.Map<BookDTO>(x)).ToListAsync();
            


        }

        public async Task<List<BookDTO>> Sort(SortBookDTO sortDTO)
        {
            var books = await _dataContext.Book.Select(x => _mapper.Map<BookDTO>(x)).ToListAsync();
            if (sortDTO.SortByReleaseYearAsc)
            {
                return books.OrderBy(x => x.ReleaseYear).ToList();
            }
            else if (sortDTO.SortByReleaseYearDesc)
            {
                return books.OrderByDescending(x => x.ReleaseYear).ToList();
            }
            else if (sortDTO.SortByTitleAsc)
            {
                return books.OrderBy(x => x.Title).ToList();
            }
            else if (sortDTO.SortByTitleDesc)
            {
                return books.OrderByDescending(x => x.Title).ToList();
            }

            return books;
        }

        public async Task TakeBookFromLibrary(BookReaderDTO takeBookDTO)
        {
            if (takeBookDTO.BookId != 0 && takeBookDTO.ReaderId != 0)
            {
                var book = await _dataContext.Book.FirstOrDefaultAsync(b => b.Id == takeBookDTO.BookId);
                var reader = await _dataContext.Reader.FirstOrDefaultAsync(b => b.Id == takeBookDTO.ReaderId);
                if (book == null)
                {
                    throw new ObjectNotFoundException(nameof(Book), takeBookDTO.BookId);
                }
                if (reader == null)
                {
                    throw new ObjectNotFoundException(nameof(Reader), takeBookDTO.ReaderId);
                }
                if (book.ReaderId != null)
                {
                    throw new BookIsAlreadyTakenException(takeBookDTO.BookId);
                }

                book.ReaderId = takeBookDTO.ReaderId;
                await _dataContext.SaveChangesAsync();

            }
            else
            {
                throw new InvalidDataProvidedException();
            }
        }

        public async Task<BookDTO> UpdateBook(UpdateBookDTO updateBookDTO)
        {
            if (updateBookDTO == null)
            {
                throw new ArgumentNullException(nameof(updateBookDTO));
            }

            var book = await _dataContext.Book.FirstOrDefaultAsync(b => b.Id == updateBookDTO.Id);
            if (book == null)
            {
                throw new ObjectNotFoundException(nameof(Book), updateBookDTO.Id);
            }

            _mapper.Map(updateBookDTO, book);
            book.LastModifiedBy = _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name);
            book.LastModifiedOn = DateTime.Now;
            _dataContext.Book.Update(book);
            await _dataContext.SaveChangesAsync();
            
            return _mapper.Map<BookDTO>(book);
        }
    }
}
