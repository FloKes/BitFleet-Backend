using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Options;
using BitFleet.Services.DTOs;
using BitFleet.Services.Interfaces;

namespace BitFleet.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtOptions _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityService(UserManager<IdentityUser> userManager, JwtOptions jwtSettings,
            TokenValidationParameters tokenValidationParameters, DataContext dataContext,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _dataContext = dataContext;
            _roleManager = roleManager;
        }

        public async Task<AuthenticationResult> RegisterAsync(string username, string password, string firstName,
            string lastName, string email, string role)
        {
            var existingUser = await _userManager.FindByNameAsync(username);

            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"A user with this username already exists"}
                };
            }

            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"A role with that name doesn't exist"}
                };
            }

            var newUserId = Guid.NewGuid();
            var newUser = new IdentityUser
            {
                Id = newUserId.ToString(),
                UserName = username,
                Email = email
            };

            var createdUser = await _userManager.CreateAsync(newUser, password);

            if (!createdUser.Succeeded)
            {
                return new AuthenticationResult
                {
                    Errors = createdUser.Errors.Select(x => x.Description)
                };
            }

            await _userManager.AddClaimAsync(newUser, new Claim("FirstName", firstName));
            await _userManager.AddClaimAsync(newUser, new Claim("LastName", lastName));
            await _userManager.AddClaimAsync(newUser, new Claim("IsOnRide", "false"));

            await _userManager.AddToRoleAsync(newUser, role);

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"User does not exist"}
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"User/password combination is wrong"}
                };
            }

            return await GenerateAuthenticationResultForUserAsync(user);
        }


        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("userId", user.Id)
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}