using API.Extensions;

namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string KnownAs { get; set; }

        public string Gender { get; set; }

        public string Introduction { get; set; }

        public string Intrests { get; set; }

        public DateOnly DateOfBirth { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public List<Photo> Photos { get; set; } = [];

        public byte[] PasshwordHash { get; set; } = [];

        public byte[] PasshwordSalt { get; set; } = [];

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public int GetAge()
        {
            return DateOfBirth.CalculateAge();
        }
    }
}