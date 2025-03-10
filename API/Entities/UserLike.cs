namespace API.Entities
{
    public class UserLike
    {
        public AppUser SourceUser { get; set; } = null!;

        public int SourceUserId { get; set; }

        public AppUser TargetUser { get; set; }

        public int TargetUserId { get; set; }
    }
}