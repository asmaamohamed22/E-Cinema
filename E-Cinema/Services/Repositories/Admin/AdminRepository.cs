using E_Cinema.Data;
using E_Cinema.Models;
using E_Cinema.ModelViews.users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Cinema.Services.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task<ApplicationUser> AddUser(AddUserModel model)
        {
            if (model == null)
            {
                return null;
            }
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.EmailConfirm,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return user;
            }
            return null;
        }
    }
}
