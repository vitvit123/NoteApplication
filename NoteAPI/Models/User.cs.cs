namespace NoteAppApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        // New properties for lockout
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

}
