using System.ComponentModel.DataAnnotations;

namespace BitFleet.Contracts.V1.Requests
{
    public class RegistrationRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; }
    }
}