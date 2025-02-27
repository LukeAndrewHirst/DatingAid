using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {

        public string KnownAs { get; set; }

        public string Gender { get; set; }

        public string Introduction { get; set; }

        public string Intrests { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public List<Photo> Photos { get; set; } = [];

        public List<UserLike> LikedByUsers { get; set; } = [];

        public List<UserLike> LikedUsers { get; set; } = [];

        public List<Message> MessagesSent { get; set; }

        public List<Message> MessagesReceived { get; set; }

        public ICollection<AppUserRole> UserRoles { get; set; } = [];

        public int GetAge()
        {
            return DateOfBirth.CalculateAge();
        }
    }
}