using System.Threading.Tasks;
using BitFleet.Domain;
using BitFleet.Services.DTOs;

namespace BitFleet.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterAsync(string username, string password, string firstName, string lastName, string email, string role);

        Task<AuthenticationResult> LoginAsync(string username, string password);
    }
}