using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services.DTOs;
using BitFleet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;

namespace BitFleet.Services
{
    public class CarService : ICarService
    {
        private readonly DataContext _dataContext;

        public CarService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        public async Task<List<Car>> GetCarsAsync(GetAllCarsFilter filter = null)
        {
            var queryable = _dataContext.Cars.AsNoTracking().AsQueryable();

            if (filter != null)
            {
                queryable = AddFiltersOnQuery(filter, queryable);
            }

            return await queryable.ToListAsync();
        }

        public async Task<Car> GetCarByIdAsync(Guid carId)
        {
            var car = await _dataContext.Cars.Include(x=>x.CarCosts)
                .SingleOrDefaultAsync(x => x.Id == carId);
            return car;
        }

        public async Task<CarCosts> GetCarCostsByCarIdAsync(Guid carId)
        {
            var car = await GetCarByIdAsync(carId);

            //null propagation
            var carCosts = car?.CarCosts;

            return carCosts;
        }


        public async Task<bool> CreateCarAsync(Car car)
        {
            await _dataContext.Cars.AddAsync(car);

            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }


        public async Task<bool> UpdateCarAsync()
        {
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }


        public async Task<bool> DeleteCarAsync(Guid carId)
        {
            var car = await GetCarByIdAsync(carId);

            if (car == null || car.IsOnRide == true)
            {
                return false;
            }

            _dataContext.Cars.Remove(car);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }


        private static IQueryable<Car> AddFiltersOnQuery(GetAllCarsFilter filter, IQueryable<Car> queryable)
        {
            if (filter.IsOnRide!= null)
            {
                var isOnRide = filter.IsOnRide.Value;
                queryable = queryable.Where(x => x.IsOnRide.Equals(isOnRide));
            }

            if (filter.IsOnService != null)
            {
                var isOnService= filter.IsOnService.Value;
                queryable = queryable.Where(x => x.IsOnService.Equals(isOnService));
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                queryable = queryable.Where(x => x.Brand.Equals(filter.Brand));
            }

            return queryable;
        }
    }
}