using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Data;
using BitFleet.Services.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BitFleet.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<string> GetFirstNameAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);

            var firstName = claims.Single(claim =>
                claim.Type.Equals("FirstName")).Value.ToString();

            return firstName;
        }

        public static async Task<string> GetLastNameAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);

            var lastName = claims.Single(claim =>
                claim.Type.Equals("LastName")).Value.ToString();

            return lastName;
        }

        public static async Task<string> GetRoleAsync(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return roles[0];
        }

        public static async Task<bool> IsOnRideAsync(this UserManager<IdentityUser> userManager,IdentityUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var isOnRide = bool.Parse(claims.Single(claim => claim.Type.Equals("IsOnRide")).Value.ToString());

            return isOnRide;
        }


        public static async Task<UserResponse> SetOnRideAsync(this UserManager<IdentityUser> userManager,IdentityUser user, string isOnRide)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var isOnRideClaim = claims.Single(claim => claim.Type.Equals("IsOnRide"));

            await userManager.RemoveClaimAsync(user, isOnRideClaim);
            await userManager.AddClaimAsync(user, new Claim("IsOnRide", isOnRide));

            var userResponse = await GenerateUserResponse(userManager,user);
            return userResponse;
        }

        public static async Task<UserResponse> GenerateUserResponse(this UserManager<IdentityUser> userManager, IdentityUser user)
        {
            var userResponse = new UserResponse
            {
                Id = user.Id,
                FirstName = await GetFirstNameAsync(userManager,user),
                LastName = await GetLastNameAsync(userManager,user),
                IsOnRide = await IsOnRideAsync(userManager,user),
                Role = await GetRoleAsync(userManager,user),
                Username = user.UserName
            };

            return userResponse;
        }


        public static async Task<List<IdentityUser>> GetUsersAsync(this UserManager<IdentityUser> userManager)
        {
            var users = await userManager.Users.ToListAsync();
            return users;
        }

    }
}