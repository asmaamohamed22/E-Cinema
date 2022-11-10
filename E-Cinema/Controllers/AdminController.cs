using E_Cinema.Models;
using E_Cinema.Services.Repositories.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Cinema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
