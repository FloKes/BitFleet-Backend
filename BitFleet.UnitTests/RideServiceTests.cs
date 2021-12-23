using System;
using System.Threading.Tasks;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services;
using BitFleet.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BitFleet.UnitTests
{
    //Collection puts the test in a collection, which disables it from running in parallel and affecting
    //the other test classes, it's a bit slower as well
    [Collection("BitFleetUnitTests")]
    public class RideServiceTests : IDisposable
    {
        private readonly RideService _sut;
        private readonly DataContext _dataContext;

        public RideServiceTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseInMemoryDatabase("testDb");
            _dataContext = new DataContext(optionsBuilder.Options);
            _sut = new RideService(_dataContext);
        }

        [Fact]
        public async Task GetRideByIdAsync_ShouldReturnRide_WhenRideExists()
        {
            // Arrange
            var rideId = Guid.NewGuid();
            var ride = new Ride
            {
                Id = rideId,
                Car = new Car(),
                IsActive = true
            };
            await _dataContext.Rides.AddAsync(ride);
            await _dataContext.SaveChangesAsync();

            // Act
            var returnedRide = await _sut.GetRideByIdAsync(rideId);

            // Assert
            returnedRide.Should().NotBeNull();
        }


        [Fact]
        public async Task GetAllRidesAsync_ShouldReturnRideList_WhenRidesExists()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var ride = new Ride
                {
                    Id = Guid.NewGuid(),
                    Car = new Car(),
                    IsActive = true
                };
                await _dataContext.Rides.AddAsync(ride);
            }

            await _dataContext.SaveChangesAsync();

            // Act
            var rideList = await _sut.GetAllRidesAsync();

            // Assert
            rideList.Should().NotBeNull();
            rideList.Should().NotBeEmpty();
            rideList.Count.Should().Be(5);
        }


        [Fact]
        public async Task GetRidesAsync_WithIsActiveAsFilter_ShouldReturnList_WithActiveRidesOnly()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var active = i % 2 == 0;
                var ride = new Ride
                {
                    Id = Guid.NewGuid(),
                    Car = new Car(),
                    IsActive = active
                };
                await _dataContext.Rides.AddAsync(ride);
            }

            await _dataContext.SaveChangesAsync();
            var filter = new GetAllRidesFilter
            {
                IsActive = true
            };

            // Act
            var rideList = await _sut.GetAllRidesAsync(filter);

            // Assert
            rideList.Should().NotBeNull();
            rideList.Should().NotBeEmpty();
            rideList.Count.Should().Be(3);
        }


        [Fact]
        public async Task StopRideAsync_ShouldReturnTrue_WhenRideIsStopped()
        {
            // Arrange
            var rideId = Guid.NewGuid();
            var startMileage = 10;
            var endMileage = 20;
            var ride = new Ride
            {
                Id = rideId,
                Car = new Car(),
                IsActive = true,
                StartMileage = startMileage
            };
            await _dataContext.Rides.AddAsync(ride);
            await _dataContext.SaveChangesAsync();
            var rideToBeUpdated = await _sut.GetRideByIdAsync(ride.Id);
            rideToBeUpdated.EndMileage = endMileage;
            rideToBeUpdated.IsActive = false;

            // Act
            var rideResponse = await _sut.UpdateRideAsync();

            // Assert
            rideResponse.Should().BeTrue();
        }


        public void Dispose()
        {
            _dataContext.Database.EnsureDeleted();
        }
    }
}