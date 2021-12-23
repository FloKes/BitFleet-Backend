using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Contracts.V1.Responses.AuthenticationResponses;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace BitFleet.IntegrationTests
{
    public class IntegrationTest : IDisposable
    {
        public readonly HttpClient testClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _serviceScope;

        public IntegrationTest()
        {

            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        //here we set up an in memory database
                        services.RemoveAll(typeof(DataContext));
                        services.AddDbContext<DataContext>(options => { options.UseInMemoryDatabase("TestDb"); });
                    });
                });

            // Configure the in-memory test server, and create an HttpClient for interacting with it
            testClient = appFactory.CreateClient();
            _serviceProvider = appFactory.Services;
            _serviceScope = _serviceProvider.CreateScope();
        }


        protected async Task AuthenticateAsync(string username)
        {
            testClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("bearer", await GetJwtAsync(username));
        }


        protected async Task<Car> SeedCarAsync()
        {
            var newCar = new Car
            {
                Id = Guid.NewGuid(),
                Brand = "Audi",
                Model = "A5",
                ModelYear = 2018,
                FuelType = "Gasoline",
                KilometersPerLiter = 15.6F ,

                //we add +1 because if someone adds a service when the car didn't go on any rides, calculating the cost/km will result 
                //in division by 0
                Mileage = 1500 + 1,
                MileageWhenBought = 1500,
                KilometersNeededBeforeService = 10000,
                KilometersSinceLastService = 0,
                CarCosts = new CarCosts
                {
                    Id = Guid.NewGuid(),
                }
            };

            var dataContext = _serviceScope.ServiceProvider.GetRequiredService<DataContext>();
            await dataContext.Cars.AddAsync(newCar);
            var created = await dataContext.SaveChangesAsync();

            if (created>0)
            {
                return newCar;
            }

            return null;
        }

        protected async Task<VehicleService> SeedVehicleService(Car car,bool isScheduled, bool isActive, bool isFinished)
        {
            var vehicleService = new VehicleService
            {
                Id = Guid.NewGuid(),
                Car = car,
                Cost = 50,
                DateTime = DateTime.UtcNow,
                Description = "Maybe fixed, maybe not, maybe scheduled",
                IsScheduled = isScheduled,
                IsActive = isActive,
                IsFinished = isFinished
            };
            var dataContext = _serviceScope.ServiceProvider.GetRequiredService<DataContext>();
            await dataContext.VehicleServices.AddAsync(vehicleService);
            var created = await dataContext.SaveChangesAsync();

            if (created>0)
            {
                return vehicleService;
            }

            return null;
        }


        protected async Task<RideResponse> CreateRideAsync(string carId)
        {

            var request = new CreateRideRequest
            {
                CarId = carId,
                StartLocation = "Horsens",
                EndLocation = "Aarhus"
            };

            var response = await testClient.PostAsJsonAsync(ApiRoutes.Rides.Create, request);
            return await response.Content.ReadAsAsync<RideResponse>();
        }

 
        protected async Task SeedUsers()
        {
            var roleManager = _serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = _serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var roles = new[] {"SuperAdmin","Admin", "CarUser"};

            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role)) continue;

                var roleToCreate = new IdentityRole(role);
                await roleManager.CreateAsync(roleToCreate);
            }


            var userNames = new[] {"SuperAdmin", "Admin", "CarUser"};

            //move to seeder class
            foreach (var userName in userNames)
            {
                if (await userManager.FindByNameAsync(userName) == null)
                {
                    var newUserId = Guid.NewGuid();
                    var newUserRole = userName;
                    var newUser = new IdentityUser
                    {
                        Id = newUserId.ToString(),
                        UserName = userName,
                        Email = userName +"@test.com"
                    };
                    await userManager.CreateAsync(newUser, "Test1234.");
                    await userManager.AddToRoleAsync(newUser, newUserRole);
                    await userManager.AddClaimAsync(newUser, new Claim("FirstName", userName.ToLower()));
                    await userManager.AddClaimAsync(newUser, new Claim("LastName", userName.ToUpper()));
                    await userManager.AddClaimAsync(newUser, new Claim("IsOnRide", "false"));
                }
            }
        }

        private async Task<string> GetJwtAsync(string username)
        {
            var response =await testClient.PostAsJsonAsync(ApiRoutes.Identity.Login, new LoginRequest
            {
                Username = username,
                Password = "Test1234."
            });

            var registrationResponse = await response.Content.ReadAsAsync<AuthSuccessResponse>();
            return registrationResponse.Token;
        }
        
        public void Dispose()
        {
            using var serviceScope = _serviceProvider.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<DataContext>();
            context.Database.EnsureDeleted();
        }
    }
}