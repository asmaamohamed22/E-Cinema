using E_Cinema.Data;
using E_Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace E_Cinema.Services.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _db;
        public AdminRepository(ApplicationDbContext db)
        {
            _db = db;
        }    
        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            return await _db.Users.ToListAsync();
        }
    }
}
