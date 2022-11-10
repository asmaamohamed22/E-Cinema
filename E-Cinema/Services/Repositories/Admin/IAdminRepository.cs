using E_Cinema.Models;

namespace E_Cinema.Services.Repositories.Admin
{
    public interface IAdminRepository
    {
        Task<IEnumerable<ApplicationUser>> GetUsers();
    }
}