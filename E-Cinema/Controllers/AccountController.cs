using E_Cinema.Data;
using E_Cinema.Models;
using E_Cinema.ModelViews;
using E_Cinema.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace E_Cinema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                    return BadRequest("Email Is Used");
                }
                if (!IsEmailValid(registerVM.Email))
                {
                    return BadRequest("Email Is Not Valid");
                }

                if (UserNameExist(registerVM.UserName))
                {
                    return BadRequest("UserName is used");
                }
                
                var user = new ApplicationUser
                {
                    UserName = registerVM.UserName,
                    Email = registerVM.Email
                };

                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (result.Succeeded)
                {
                    /////http://localhost:/Account/RegistrationConfirm?ID=545435&Token=5435354gw34523
                    //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var encodeToken = Encoding.UTF8.GetBytes(token);
                    //var newToken = WebEncoders.Base64UrlEncode(encodeToken);

                    //var confirmLink = $"http://localhost:4200/registerconfirm?ID={user.Id}&Token={newToken}";
                    //var txt = "Please confirm your registration at our sute";
                    //var link = "<a href=\"" + confirmLink + "\">Confirm registration</a>";
                    //var title = "Registration Confirm";
                    //if (await SendGridAPI.Execute(user.Email, user.UserName, txt, link, title))
                    //{
                    //    return StatusCode(StatusCodes.Status200OK);
                    //}
                    return StatusCode(StatusCodes.Status200OK);
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
        public async Task<IActionResult> RegistrationConfirm(string ID, string Token)
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Token))
                return NotFound();

            var user = await _userManager.FindByIdAsync(ID);
            if (user == null)
                return NotFound();

            var newToken = WebEncoders.Base64UrlDecode(Token);
            var encodeToken = Encoding.UTF8.GetString(newToken);

            var result = await _userManager.ConfirmEmailAsync(user, encodeToken);

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
            await CreateRoles();
            await CreateAdmin();
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

            var userName = HttpContext.User.Identity.Name;
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id != null || userName != null)
            {
                return BadRequest($"user id:{id} is exists");
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);
            if (result.Succeeded)
            {
                if (await _roleManager.RoleExistsAsync("User"))
                {
                    if(!await _userManager.IsInRoleAsync(user, "User") && !await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }

                var roleName = await GetRoleNameByUserId(user.Id);
                if (roleName != null)
                {
                    HttpContext.Response.Cookies.Append(
                    "name", "value",
                    new CookieOptions() { SameSite = SameSiteMode.Lax });
                    AddCookies(user.UserName, roleName, user.Id, loginVM.RememberMe, user.Email);
                }

                return Ok("Login Success");
            }
            else if(result.IsLockedOut)
            {
                return Unauthorized("User Account Is Locked!");
            }

            return StatusCode(StatusCodes.Status204NoContent);

        }

        private async Task<string> GetRoleNameByUserId(string userId)
        {
            var userRole = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRole != null)
            {
                return await _db.Roles.Where(x => x.Id == userRole.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
            }
            return null;
        }

        [HttpGet]
        [Route("UserExists")]
        public async Task<IActionResult> UserExists(string username)
        {
            var exist = await _db.Users.AnyAsync(x => x.UserName == username);
            if (exist)
            {
                return Ok("Username is Already Exists");
            }
            return BadRequest("Username is available");
        }

        [HttpGet]
        [Route("EmailExists")]
        public async Task<IActionResult> EmailExists(string email)
        {
            var exist = await _db.Users.AnyAsync(x => x.Email == email);
            if (exist)
            {
                return Ok("Email is Already Exists");
            }
            return BadRequest("Email is available");
        }

        private async Task CreateAdmin()
        {
            var admin = await _userManager.FindByNameAsync("Admin");
            if(admin == null)
            {
                var user = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    PhoneNumber = "01123456789",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "123@Asmaa");

                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        }

        private async Task CreateRoles()
        {
            if(_roleManager.Roles.Count() < 1)
            { 
                var role = new ApplicationRole
                {
                    Name = "Admin"
                };
                await _roleManager.CreateAsync(role);

                role = new ApplicationRole
                {
                    Name = "User"
                };
                await _roleManager.CreateAsync(role);
            }
           
        }

        private async void AddCookies(string username, string roleName, string userId, bool remember, string email)
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, roleName),
            };

            var claimIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);

            if (remember)
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(10)
                };

                await HttpContext.SignInAsync
                (
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimIdentity),
                   authProperties
                );
            }
            else
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = false,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync
                (
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimIdentity),
                   authProperties
                );
            }
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(10)
            };
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authProperties);
            return Ok();
        }

        [HttpGet]
        [Route("GetRoleName/{email}")]
        public async Task<string> GetRoleName(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var userRole = await _db.UserRoles.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (userRole != null)
                {
                    return await _db.Roles.Where(x => x.Id == userRole.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
                }
            }

            return null;
        }

        [Authorize]
        [HttpGet]
        [Route("CheckUserClaims/{email}&{role}")]
        public IActionResult CheckUserClaims(string email, string role)
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userEmail != null && userRole != null && id != null)
            {
                if (email == userEmail && role == userRole)
                {
                    return Ok();
                }
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [Route("ForgetPassword/{email}")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (email == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodeToken = Encoding.UTF8.GetBytes(token);
            var newToken = WebEncoders.Base64UrlEncode(encodeToken);

            var confirmLink = $"http://localhost:4200/passwordconfirm?ID={user.Id}&Token={newToken}";
            var txt = "Please confirm password";
            var link = "<a href=\"" + confirmLink + "\">Passowrd confirm</a>";
            var title = "Passowrd confirm";
            if (await SendGridAPI.Execute(user.Email, user.UserName, txt, link, title))
            {
                return new ObjectResult(new { token = newToken });
            }

            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                var newToken = WebEncoders.Base64UrlDecode(model.Token);
                var encodeToken = Encoding.UTF8.GetString(newToken);

                var result = await _userManager.ResetPasswordAsync(user, encodeToken, model.Password);
                if (result.Succeeded)
                {
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
