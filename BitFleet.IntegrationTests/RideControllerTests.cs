using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests.Queries;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using FluentAssertions;
using Xunit;

namespace BitFleet.IntegrationTests
{
    [Collection("BitFleetIntegrationTests")]
    public class RideControllerTests : IntegrationTest
    {
        [Fact]
        public async Task GetAllRides_ReturnsRideResponses()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");

            var createdCar = await SeedCarAsync();
            await CreateRideAsync(createdCar.Id.ToString());

            //Act
            var response = await testClient.GetAsync(ApiRoutes.Rides.GetAll);

            //Assert
            var responseList = await response.Content.ReadAsAsync<List<RideResponse>>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            responseList.Should().NotBeEmpty();
            responseList.Count.Should().Be(1);
        }


        [Fact]
        public async Task GetAllRides_WithIsActiveTrueQuery_ReturnsNotEmptyList()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();
            await CreateRideAsync(createdCar.Id.ToString());
            
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["IsActive"] = "true";
            string queryString = query.ToString();
            //Act
            var response = await testClient.GetAsync(ApiRoutes.Rides.GetAll + "/?" + queryString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<RideResponse>>()).Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllRides_WithIsActiveFalseQuery_ReturnsEmptyList()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();
            await CreateRideAsync(createdCar.Id.ToString());
            

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["IsActive"] = "false";
            string queryString = query.ToString();
            //Act
            var response = await testClient.GetAsync(ApiRoutes.Rides.GetAll + "/?" + queryString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<RideResponse>>()).Should().BeEmpty();
        }


        [Fact]
        public async Task StopRide_ReturnsStoppedRideResponse()
        {
            //Arrange
            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();
            var createdRide = await CreateRideAsync(createdCar.Id.ToString());
            var stopRideRequest = new StopRideRequest
            {
                EndMileage = 2000
            };

            var mimeJson = "application/json";
            var requestUri = ApiRoutes.Rides.Update
                .Replace("{rideId}", createdRide.Id.ToString());

            var httpContent = new ObjectContent(
                stopRideRequest.GetType(), stopRideRequest, new JsonMediaTypeFormatter(), mimeJson);

            //Act
            var response = await testClient.PatchAsync(requestUri, httpContent);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var rideResponse = await response.Content.ReadAsAsync<RideResponse>();
            rideResponse.Should().NotBeNull();
            rideResponse.IsActive.Should().Be(false);
            rideResponse.Car.IsOnRide.Should().Be(false);
            rideResponse.User.IsOnRide.Should().Be(false);
        }


        [Fact]
        public async Task DeleteRide_ReturnsNoContentResponse()
        {
            //Arrange

            await SeedUsers();
            await AuthenticateAsync("admin");
            var createdCar = await SeedCarAsync();
            var createdRide = await CreateRideAsync(createdCar.Id.ToString());

            var updateRequest = new StopRideRequest
            {
                EndMileage = 2000
            };
            var mimeJson = "application/json";
            var requestUri = ApiRoutes.Rides.Update
                .Replace("{rideId}", createdRide.Id.ToString());
            var httpContent = new ObjectContent(updateRequest.GetType(), updateRequest, new JsonMediaTypeFormatter(),
                mimeJson);
            await testClient.PatchAsync(requestUri, httpContent);

            //Act

            var deleted = await testClient.DeleteAsync(ApiRoutes.Rides.Delete
                .Replace("{rideId}", createdRide.Id.ToString()));

            //Assert
            deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}