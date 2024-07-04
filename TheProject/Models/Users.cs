namespace TheProject.Models
{
    public class Users
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; }  = string.Empty ;

        public byte[] PasswordHash { get; set; } = new byte[0];

        public byte[] PasswordSalt { get; set; } = new byte[0];

        public string Address { get; set; } = string.Empty;

        public string Phone {  get; set; } = string.Empty;
            
        public string Dob { get; set; } = string.Empty;

        public string Type {  get; set; } = string.Empty;

        public string Image { get; set; } = string.Empty;

    }
}
