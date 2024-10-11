using LibraryAdministration.Exceptions;
using LibraryAdministration.Interfaces;
using LibraryAdministration.Models.DTO.AccountDTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAdministration.Controllers
{
    public class LibraryAdministratorController : Controller
    {
        private readonly ILibraryAdministratorService _libraryAdministratorService;
        private readonly ILogger<LibraryAdministratorController> _logger;

        public LibraryAdministratorController(ILibraryAdministratorService libraryAdministratorService, ILogger<LibraryAdministratorController> logger)
        {
            _libraryAdministratorService = libraryAdministratorService;
            _logger = logger;
        }
        
        public ActionResult RegistrationPage()
        {
            return View("Views/Home/Registration.cshtml");
        }
        
        public ActionResult Index()
        {
            return View("Views/Home/Index.cshtml");
        }
        [HttpPost("register")]
        public async Task<IActionResult> Registration([Bind("UserName, Name, LastName")] LibraryAdministratorDTO libraryAdministratorDTO)
        {
            var userPassword = Request.Form["pass"];
            libraryAdministratorDTO.Password = userPassword!;

            try
            {
                await _libraryAdministratorService.UserRegistration(libraryAdministratorDTO);
            }
            catch (UserAlreadyExistsException e)
            {
                _logger.LogError(e.Message);
                TempData["UserNameAlreadyExists"] = e.Message;
                return RegistrationPage();
            }

            return Index();
        }

        
        public async Task<IActionResult> Login([Bind("UserName")] LoginDTO loginDTO)
        {
            var userPassword = Request.Form["pass"];
            loginDTO.Password = userPassword!;

            try
            {
                await _libraryAdministratorService.Login(loginDTO);
            }
            catch (InvalidCredentialsException e)
            {
                _logger.LogError(e.Message);
                TempData["InvalidCredentials"] = e.Message;
                return Index();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginDTO.UserName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                //ExpiresUtc = DateTime.UtcNow.AddMinutes(1)
                
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToAction("Index", "Book");
        }
        
    }
}
