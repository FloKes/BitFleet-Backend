using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitFleet.Domain;
using BitFleet.Filters;

namespace BitFleet.Services.Interfaces
{
    public interface ICarService
    {
        Task<List<Car>> GetCarsAsync(GetAllCarsFilter filter = null);

        Task<Car> GetCarByIdAsync(Guid carId);

        Task<CarCosts> GetCarCostsByCarIdAsync(Guid carId);

        Task<bool> CreateCarAsync(Car car);

        Task<bool> UpdateCarAsync();

        Task<bool> DeleteCarAsync(Guid carId);
    }
}