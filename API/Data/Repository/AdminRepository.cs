using System.Collections;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repository
{
    public class AdminRepository(UserManager<AppUser> userManager) : IAdminRepository
    {
        public async Task<IEnumerable> GetUsersWithRoles()
        {
           return await userManager.Users.OrderBy(u => u.UserName).Select(u => new{
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();
        }

        public async Task<IList<string>> GetUserRoles(AppUser user)
        {
            return await userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AddToRoles(AppUser user, IList<string> selectedRoles, IList<string> userRoles)
        {
            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            return result;
        }

        public async Task<IdentityResult> RemoveFromRoles(AppUser user, IList<string> selectedRoles, IList<string> userRoles)
        {
            var result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            return result;
        }
    }
}