using LibraryAdministration.Models.DTO.BookDTO;

namespace LibraryAdministration.Interfaces
{
    public interface IBookService
    {
        Task<BookDTO> CreateBook(CreateBookDTO createBookDTO);

        Task<BookDTO> UpdateBook(UpdateBookDTO updateBookDTO);

        Task DeleteBook(int id);

        Task<List<BookDTO>> GetAll();

        Task<List<BookDTO>> SearchBook(SearchBookDTO searchBookDTO);

        Task<List<BookDTO>> Sort(SortBookDTO sortDTO);

        Task<BookInfoDTO> GetBook(int id);

        Task TakeBookFromLibrary(BookReaderDTO takeBookDTO);

        Task ReturnBookToLibrary(BookReaderDTO returnBookDTO);


    }
}
