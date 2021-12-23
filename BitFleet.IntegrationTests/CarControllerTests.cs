using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Domain;
using BitFleet.Services.DTOs;
using FluentAssertions;
using Xunit;

namespace BitFleet.IntegrationTests
{
    [Collection("BitFleetIntegrationTests")]
    public class CarControllerTests : IntegrationTest
    {
        [Fact]
        public async Task GetAll_WithoutAnyCars_ReturnsEmptyResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Cars.GetAll);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<CarResponse>>()).Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAvailable_WhenCarIsOnRide_ReturnsNull()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("carUser");
            var createdCar = await SeedCarAsync();
            await CreateRideAsync(createdCar.Id.ToString());

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Cars.GetAllAvailable);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<CarResponse>>()).Should().BeEmpty();
        }


        [Fact]
        public async Task GetAllAvailable_WhenCarUser_ReturnsCarResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("carUser");
            var createdCar = await SeedCarAsync();

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Cars.GetAllAvailable);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<CarResponse>>()).Should().NotBeEmpty();
        }


        [Fact]
        public async Task GetAll_WhenCarUser_ReturnsUnauthorized()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("carUser");

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Cars.GetAll);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }


        [Fact]
        public async Task Get_ReturnsCarResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Cars.Get
                .Replace("{carId}", createdCar.Id.ToString()));

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<CarResponse>()).Should().NotBeNull();
        }


        [Fact]
        public async Task Create_ReturnsCreatedCarResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var request = new CreateCarRequest
            {
                Brand = "Audi",
                Model = "Q7",
                ModelYear = 2019,
                FuelType = "diesel",
                KilometersNeededBeforeService = 10000,
                KilometersPerLiter = 20,
                MileageWhenBought = 1500
            };

            //Act

            var response = await testClient.PostAsJsonAsync(ApiRoutes.Cars.Create, request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            (await response.Content.ReadAsAsync<CarResponse>()).Should().NotBeNull();
        }


        [Fact]
        public async Task UpdateCar_ReturnsCarResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();
            var updateRequest = new UpdateCarRequest
            {
                Brand = "Mercedes",
                Model = "S2",
                ModelYear = 2020,
                NeedsService = true
            };

            var mimeJson = "application/json";
            var requestUri = ApiRoutes.Cars.Update
                .Replace("{carId}", createdCar.Id.ToString());

            var httpContent = new ObjectContent(updateRequest.GetType(), updateRequest, new JsonMediaTypeFormatter(),
                mimeJson);

            //Act
            var response = await testClient.PatchAsync(requestUri, httpContent);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<CarResponse>()).Should().NotBeNull();
        }


        [Fact]
        public async Task DeleteCar_ReturnsNoContentResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();

            //Act

            var deleted = await testClient.DeleteAsync(ApiRoutes.Cars.Delete
                .Replace("{carId}", createdCar.Id.ToString()));
            //Assert
            deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetCarCosts_ReturnsCarCostsResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();

            //Act

            var response = await testClient.GetAsync(ApiRoutes.Cars.GetCarCosts
                .Replace("{carId}", createdCar.Id.ToString()));
            //Assert

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<CarResponse>()).Should().NotBeNull();
        }


        [Fact]
        public async Task GetAll_Users_ReturnsNotEmptyResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Users.GetAll);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<UserDto>>()).Find(x => x.Username.Equals("SuperAdmin"))
                .Should().NotBe(null);
        }
    }
}