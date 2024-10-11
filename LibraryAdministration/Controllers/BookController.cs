using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.DTO.BookDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAdministration.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        public IActionResult ReturnBookPage()
        {
            return View("Views/Pages/ReturnBookPage.cshtml");
        }
        public IActionResult TakeBookPage()
        {
            return View("Views/Pages/TakeBookPage.cshtml");
        }
        public async Task<IActionResult> UpdateBookPage(int id)
        {
            var book = await _bookService.GetBook(id);
            return View("Views/Pages/UpdateBookPage.cshtml", book );
        }
        public IActionResult AddBookPage()
        {
            return View("Views/Pages/AddBook.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAll();           
            return View("Views/Pages/Books.cshtml", books);
        }

        
        // To check if Bind attribute is working without providing the exact property names
        [HttpPost]
        public async Task<IActionResult> CreateBook([Bind] CreateBookDTO createBookDTO)
        {
            try
            {
                var book = await _bookService.CreateBook(createBookDTO);
                return await GetInfoAboutBook(book.Id);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }
        }

        
        public async Task<IActionResult> UpdateBook([Bind] UpdateBookDTO updateBookDTO)
        {
            try
            {
                var book = await _bookService.UpdateBook(updateBookDTO);
                return await GetInfoAboutBook(book.Id);
            }
            catch (ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }
            catch(ObjectNotFoundException e) 
            {
                return NotFound(e.Message);
            }
        }
        
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                await _bookService.DeleteBook(id);
                //TODO  view of success deleting
                TempData["SuccessMessage"] = "Book was successfully deleted.";
                return await Index();
            }
            catch(ObjectNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch(ImpossibleToDeleteBookException)
            {
                TempData["ImpossibleToDelete"] = "It is impossible to delete book what is taken by reader";
                return await GetInfoAboutBook(id);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInfoAboutBook(int id)
        {
            try
            {
                var book = await _bookService.GetBook(id);
                return View("Views/Pages/InfoAboutBook.cshtml",book);
            }catch(ObjectNotFoundException) 
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindBooks([Bind("SearchText, YearOfRelease")]SearchBookDTO searchBookDTO)
        {
                var books = await _bookService.SearchBook(searchBookDTO);
                return View("Views/Pages/Books.cshtml", books);
        }
        [HttpGet]
        public async Task<IActionResult> SortBook(SortBookDTO sortBookDTO)
        {
            var books = await _bookService.Sort(sortBookDTO);
            return View("Views/Pages/Books.cshtml", books);
        }

        public async Task<IActionResult> TakeBook([Bind] BookReaderDTO takeBookDTO)
        {
            try
            {
                await _bookService.TakeBookFromLibrary(takeBookDTO);
                return await Index();
            }
            catch (ObjectNotFoundException)
            {
                TempData["IncorrectInput"] = "Book ID or Reader ID is not found";
                return TakeBookPage();
            }
            catch (BookIsAlreadyTakenException)
            {
                TempData["BookIsAlreadyTaken"] = "Book is already taken. Check if the data is entered correctly";
                return TakeBookPage();
            }
            
        }
        public async Task<IActionResult> ReturnBook([Bind] BookReaderDTO returnBookDTO)
        {
            try
            {
                await _bookService.ReturnBookToLibrary(returnBookDTO);
                return await Index();
            }
            catch (ObjectNotFoundException)
            {
                TempData["IncorrectInput"] = "Book ID or Reader ID is not found";
                return ReturnBookPage();
            }
            catch (BookIsAlreadyInLibraryException)
            {
                TempData["BookIsInLibrary"] = "Book is already in the Library. Check if the data is entered correctly";
                return ReturnBookPage();
            }
            

        }

    }
}
