using E_Cinema.Data;
using E_Cinema.Models;
using E_Cinema.ModelViews;
using E_Cinema.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Web;

namespace E_Cinema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (registerVM == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (EmailExist(registerVM.Email))
                {
                    return BadRequest("Email Is Not Available");
                }
                if (!IsEmailValid(registerVM.Email))
                {
                    return BadRequest("Email Is Not Valid");
                }

                if (UserNameExist(registerVM.Name))
                {
                    return BadRequest("UserName " + registerVM.Name + " is already taken.");
                }

                var user = new ApplicationUser
                {
                    UserName = registerVM.Name,
                    Email = registerVM.Email,
                    //PasswordHash = registerVM.Password
                };

                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmLink = Url.Action("RegistrationConfirm", "Account", new
                    {ID = user.Id,Token = HttpUtility.UrlEncode(token)}, Request.Scheme);

                    return Ok(confirmLink);
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpGet]
        [Route("RegistrationConfirm")]
        public async Task<IActionResult> RegistrationConfirm(string Id, string Token)
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Token))
                return NotFound();

            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(Token));

            if (result.Succeeded)
            {
                return Ok("Registration Success");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        private bool UserNameExist(string username)
        {
            return _db.Users.Any(u => u.UserName == username);
        }

        // if you register with email is ready exist in db return Bad Request
        private bool EmailExist(string email)
        {
            return _db.Users.Any(u => u.Email == email);
        }
        private bool IsEmailValid(string email)
        {
            Regex re = new Regex(@"[a-z0-9]+@[a-z]+\.[a-z]{2,3}");
            if (re.IsMatch(email))
            {
                return true;
            }
            return false;
        }


        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (loginVM == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(loginVM.Email);
            if (user == null)
                return NotFound();

            if (!user.EmailConfirmed)
            {
                return Unauthorized("Email is not confirmed yet!");
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);
            if (result.Succeeded)
            {
                return Ok("Login Success");
            }
            else if(result.IsLockedOut)
            {
                return Unauthorized("User Account Is Locked!");
            }

            return StatusCode(StatusCodes.Status204NoContent);

        }

    }
}
