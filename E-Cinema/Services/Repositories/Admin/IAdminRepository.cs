using E_Cinema.Models;
using E_Cinema.ModelViews.users;

namespace E_Cinema.Services.Repositories.Admin
{
    public interface IAdminRepository
    {
        Task<ApplicationUser> AddUser(AddUserModel model);
        Task<IEnumerable<ApplicationUser>> GetUsers();
        Task<ApplicationUser> GetUserAsync(string id);
    }
}