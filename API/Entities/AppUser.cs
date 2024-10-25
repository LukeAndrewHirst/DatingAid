namespace API.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public byte[] PasshwordHash { get; set; }

        public byte[] PasshwordSalt { get; set; }
    }
}