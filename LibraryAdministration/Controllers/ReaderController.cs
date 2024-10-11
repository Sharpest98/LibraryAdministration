using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.DTO.ReaderDTO;
using LibraryAdministration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAdministration.Controllers
{
    [Authorize]
    public class ReaderController : Controller
    {
        private readonly IReaderService _readerService;

        public ReaderController(IReaderService readerService)
        {
            _readerService = readerService;
        }

        public async Task<IActionResult> UpdateReaderPage(int id)
        {
            var reader = await _readerService.GetInfoAboutReader(id);
            return View("Views/Pages/UpdateReaderPage.cshtml", reader);
        }
        public IActionResult AddReaderPage()
        {
            return View("Views/Pages/AddReader.cshtml");
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var readers = await _readerService.GetAll();
            return View("Views/Pages/Readers.cshtml", readers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReader([Bind]CreateReaderDTO createReaderDTO)
        {
            try
            {
                var reader = await _readerService.CreateReader(createReaderDTO);
                return await GetInfoAboutReader(reader.Id);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        
        public async Task<IActionResult> UpdateReader([Bind]UpdateReaderDTO updateReaderDTO)
        {
            try
            {
                var reader = await _readerService.UpdateReader(updateReaderDTO);
                return await GetInfoAboutReader(reader.Id);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ObjectNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        public async Task<IActionResult> DeleteReader(int id)
        {
            try
            {
                await _readerService.DeleteReader(id);
                TempData["SuccessMessage"] = "Book was successfully deleted.";
                return await Index();
            }
            catch(ObjectNotFoundException ex) 
            {
                return NotFound(ex.Message);
            }
            catch(ImpossibleToDeleteReaderException )
            {
                TempData["ImpossibleToDeleteReader"] = "It is impossible to delere Reader with taken books from the library.";
                return await GetInfoAboutReader(id);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetInfoAboutReader(int id)
        {
            try
            {
                var reader =await _readerService.GetInfoAboutReader(id);
                return View("Views/Pages/InfoAboutReader.cshtml", reader);
            }catch(ObjectNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindReader([Bind("SearchByText")]SearchReaderDTO searchReaderDTO)
        {
                var reader = await _readerService.SearchReader(searchReaderDTO);
                return View("Views/Pages/Readers.cshtml", reader);           
        }
        [HttpGet]
        public async Task<IActionResult> SortReader(SortReaderDTO sortReaderDTO)
        {
            var readers = await _readerService.SortReader(sortReaderDTO);
            return View("Views/Pages/Readers.cshtml", readers);
        }
    }
}
