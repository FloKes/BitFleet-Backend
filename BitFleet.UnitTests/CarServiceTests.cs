using System;
using System.Linq;
using System.Threading.Tasks;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BitFleet.UnitTests
{
    [Collection("BitFleetUnitTests")]
    public class CarServiceTests : IDisposable
    {
        private readonly CarService _sut;
        private readonly DataContext _dataContext;

        public CarServiceTests()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseInMemoryDatabase("testDb");
            _dataContext = new DataContext(optionsBuilder.Options);
            _sut = new CarService(_dataContext);
        }


        [Fact]
        public async Task GetCarsAsync_ShouldReturnNotEmptyList_WhenCarsExists()
        {
            // Arrange
            for (int i = 0; i < 5; i++)
            {
                var car = new Car
                {
                    Id = Guid.NewGuid(),
                    Brand = "Something",
                    Model = "something",
                    CarCosts = new CarCosts()
                };
                await _dataContext.Cars.AddAsync(car);
            }

            await _dataContext.SaveChangesAsync();

            // Act
            var response = await _sut.GetCarsAsync();

            // Assert
            response.Should().NotBeNull();
            response.Should().NotBeEmpty();
            response.Count.Should().Be(5);
        }


        [Fact]
        public async Task GetCarsAsync_WithBrandAsFilter_ShouldReturnList_WithOnlyCarsOfThatBrand()
        {
            // Arrange
            var carBrands = new[] {"Mercedes", "Audi", "Audi", "Volvo"};

            foreach (var carBrand in carBrands)
            {
                await _dataContext.Cars.AddAsync(new Car
                {
                    Id = Guid.NewGuid(),
                    Brand = carBrand,
                    Model = carBrand.Remove(1),
                    CarCosts = new CarCosts()
                });
            }

            await _dataContext.SaveChangesAsync();

            var filter = new GetAllCarsFilter
            {
                Brand = "Audi"
            };

            // Act
            var response = await _sut.GetCarsAsync(filter);

            // Assert
            response.Should().NotBeEmpty();
            response.Count.Should().Be(2);
            response.Any(x => !x.Brand.Equals("Audi")).Should().BeFalse();
        }


        [Fact]
        public async Task GetCarByIdAsync_ShouldReturnCar_WhenCarExists()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new Car
            {
                Id = carId,
                Brand = "Audi",
                Model = "Qq",
                CarCosts = new CarCosts()
            };
            await _dataContext.Cars.AddAsync(car);
            await _dataContext.SaveChangesAsync();

            // Act
            var response = await _sut.GetCarByIdAsync(carId);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(carId);
        }


        [Fact]
        public async Task GetCarCostsByCarIdAsync_ShouldReturnCarCosts_WhenCarExists()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new Car
            {
                Id = carId,
                Brand = "Audi",
                Model = "Qq",
                CarCosts = new CarCosts()
            };
            await _dataContext.Cars.AddAsync(car);
            await _dataContext.SaveChangesAsync();

            // Act
            var response = await _sut.GetCarCostsByCarIdAsync(carId);

            // Assert
            response.Should().NotBeNull();
            response.CarId.Should().Be(carId);
        }


        [Fact]
        public async Task UpdateCarAsync_ShouldReturnTrue_WhenCarIsUpdated()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new Car
            {
                Id = carId,
                Brand = "Audi",
                Model = "Qq",
                CarCosts = new CarCosts()
            };
            await _dataContext.Cars.AddAsync(car);
            await _dataContext.SaveChangesAsync();
            var carToUpdate = await _dataContext.Cars.FindAsync(carId);

            // Act
            carToUpdate.Brand = "Mercedes";
            carToUpdate.CarCosts.FuelCosts = 500;
            var response = await _sut.UpdateCarAsync();
            var updatedCar = await _dataContext.Cars.FindAsync(carId);


            // Assert
            response.Should().BeTrue();
            updatedCar.Id.Should().Be(carId);
            updatedCar.Brand.Should().Be("Mercedes");
            updatedCar.CarCosts.FuelCosts.Should().Be(500);
        }


        [Fact]
        public async Task GetCarByIdAsync_ShouldReturnNull_WhenCarDoesNotExists()
        {
            // Arrange
            var carId = Guid.NewGuid();
            // Act
            var response = await _sut.GetCarByIdAsync(carId);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task CreateCarAsync_ShouldReturnTrue_WhenCarIsCreated()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new Car
            {
                Id = carId,
                Brand = "Audi",
                Model = "Qq",
                CarCosts = new CarCosts()
            };
            // Act
            var response = await _sut.CreateCarAsync(car);

            // Assert
            response.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteCarAsync_ShouldReturnTrue_WhenCarIsDeleted()
        {
            // Arrange
            var carId = Guid.NewGuid();
            var car = new Car
            {
                Id = carId,
                Brand = "Audi",
                Model = "Qq",
                CarCosts = new CarCosts()
            };
            await _dataContext.Cars.AddAsync(car);
            await _dataContext.SaveChangesAsync();

            // Act
            var response = await _sut.DeleteCarAsync(carId);

            // Assert
            response.Should().BeTrue();
        }


        public void Dispose()
        {
            _dataContext.Database.EnsureDeleted();
        }
    }
}