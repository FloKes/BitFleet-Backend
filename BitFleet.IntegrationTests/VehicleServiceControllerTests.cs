using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using FluentAssertions;
using Xunit;

namespace BitFleet.IntegrationTests
{
    [Collection("BitFleetIntegrationTests")]
    public class VehicleServiceControllerTests : IntegrationTest
    {
        
        [Fact]
        public async Task GetAll_WithoutAnyVehicleServices_ReturnsEmptyResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");

            //Act
            var response = await testClient.GetAsync(ApiRoutes.VehicleServices.GetAll);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<VehicleServiceResponse>>()).Should().BeEmpty();
        }


        [Fact]
        public async Task GetAll_WithExistingVehicleServices_ReturnsVehicleServiceResponses()
        {
            //Arrange
            var createdCar = await SeedCarAsync();
            var isScheduled = false;
            var isActive = false;
            var isFinished = true;
            var createdVehicleService = await SeedVehicleService(createdCar,isScheduled,isActive,isFinished);

            await SeedUsers();
            await AuthenticateAsync("admin");

            //Act
            var response = await testClient.GetAsync(ApiRoutes.VehicleServices.GetAll);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<VehicleServiceResponse>>()).Should().NotBeEmpty();
        }


        [Fact]
        public async Task Create_ReturnsCreatedVehicleServiceResponse()
        {
            //Arrange
            var createdCar = await SeedCarAsync();
            await SeedUsers();
            await AuthenticateAsync("admin");
            var request = new CreateVehicleServiceRequest
            {
               CarId = createdCar.Id,
               Cost = 50,
               DateTime = DateTime.UtcNow,
               Description = "Somethings wrong with the car",
               IsActive = true,
               IsScheduled = false,
               IsFinished = false
            };

            //Act

            var response = await testClient.PostAsJsonAsync(ApiRoutes.VehicleServices.Create, request);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsAsync<VehicleServiceResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Car.IsOnService.Should().BeTrue();
        }


        [Fact]
        public async Task UpdateCar_ReturnsCarResponse()
        {
            //Arrange
            var createdCar = await SeedCarAsync();
            var isScheduled = false;
            var isActive = true;
            var isFinished = false;
            var createdVehicleService = await SeedVehicleService(createdCar,isScheduled,isActive,isFinished);

            await SeedUsers();
            await AuthenticateAsync("admin");
            var updateRequest = new UpdateVehicleServiceRequest
            {
                Cost = 100,
                DateTime = DateTime.UtcNow,
                Description = "All fixed",
                IsFinished = true
            };

            var mimeJson = "application/json";
            var requestUri = ApiRoutes.VehicleServices.Update
                .Replace("{vehicleServiceId}", createdVehicleService.Id.ToString());

            var httpContent = new ObjectContent(updateRequest.GetType(), updateRequest, new JsonMediaTypeFormatter(),
                mimeJson);

            //Act
            var response = await testClient.PatchAsync(requestUri, httpContent);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsAsync<VehicleServiceResponse>();

            responseContent.Should().NotBeNull();
            responseContent.IsFinished.Should().BeTrue();
            responseContent.IsActive.Should().BeFalse();
            responseContent.IsScheduled.Should().BeFalse();
            responseContent.Car.IsOnService.Should().BeFalse();
        }

    }
}