namespace BitFleet.Services.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }

        public bool IsOnRide { get; set; }
    }
}