namespace webApp.Classes
{
    public class User
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        private static int _counter = 1;

        public List<int> listingIDs =new();
        public bool isVerified { get; set; }

        public string PhoneNumber { get; set; }
        public User()
        {
            Id = _counter++;
        }

        public User(int Id,string name, string email, string password, string role)
        {
            this.Id = Id;
            Name = name;
            Email = email;
            Password = password;
            Role = role;
        }
    }
}
