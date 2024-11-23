using System.Collections;
using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable> GetUsersWithRoles();

        Task<IList<string>> GetUserRoles(AppUser user);

        Task<IdentityResult> AddToRoles(AppUser user, IList<string> selectedRoles, IList<string> userRoles);

        Task<IdentityResult> RemoveFromRoles(AppUser user, IList<string> selectedRoles, IList<string> userRoles);
    }
}