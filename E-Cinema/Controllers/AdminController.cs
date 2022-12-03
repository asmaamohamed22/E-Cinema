using E_Cinema.Models;
using E_Cinema.ModelViews.users;
using E_Cinema.Services.Repositories.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace E_Cinema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _repo;
        public AdminController(IAdminRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IEnumerable<ApplicationUser>> GetAllUsers()
        {
            var users = await _repo.GetUsers();
            if(users == null)
            {
                return null;
            }
            return users;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<IActionResult> AddUser(AddUserModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _repo.AddUser(model);
                if(user != null)
                {
                    return Ok(user);
                }
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("GetUser/{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _repo.GetUserAsync(id);
            if (user != null)
            {
                return user;
            }
            return BadRequest();
        }
    }
}
