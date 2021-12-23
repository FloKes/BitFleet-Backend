using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services.DTOs;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BitFleet.Services
{
    public class RideService : IRideService
    {
        private readonly DataContext _dataContext;


        public RideService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Ride>> GetAllRidesAsync(GetAllRidesFilter filter = null)
        {
            var queryable = _dataContext.Rides.AsNoTracking()
                .Include(type => type.Car).AsNoTracking()
                .Include(type => type.User).AsNoTracking()
                .AsQueryable();

            if (filter != null)
            {
                queryable = AddFiltersOnRide(filter, queryable);
            }

            var rideList = await queryable.ToListAsync();


            return rideList;
        }


        public async Task<Ride> GetRideByIdAsync(Guid rideId)
        {
            var ride = await _dataContext.Rides
                .Include(type => type.Car)
                .Include(type => type.Car.CarCosts)
                .Include(type => type.User)
                .SingleOrDefaultAsync(x => x.Id == rideId);

            
            return ride;
        }

        public async Task<Ride> GetActiveRideByUserIdAsync(string userId)
        {
            var ride = await _dataContext.Rides
                .Include(type => type.Car)
                .Include(type => type.Car.CarCosts)
                .Include(type => type.User)
                .SingleOrDefaultAsync(x => x.User.Id.Equals(userId) && x.IsActive == true);
            return ride ?? null;
        }


        public async Task<Ride> CreateRideAsync(Ride ride)
        {
            ride.Car.IsOnRide = true;
            await _dataContext.Rides.AddAsync(ride);

            var created = await _dataContext.SaveChangesAsync();
            return created <= 0 ? null : ride;
        }

        public async Task<bool> DeleteRideAsync(Guid rideId)
        {
            var ride = await _dataContext.Rides.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == rideId);
            if (ride == null || ride.IsActive == true)
            {
                return false;
            }

            _dataContext.Rides.Remove(ride);

            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }


        public async Task<bool> UpdateRideAsync()
        {
            var updated = await _dataContext.SaveChangesAsync();

            return updated > 0;
        }

        private static IQueryable<Ride> AddFiltersOnRide(GetAllRidesFilter filter, IQueryable<Ride> queryable)
        {
            if (!filter.CarId.Equals(Guid.Empty))
            {
                queryable = queryable.Where(x => x.Car.Id.Equals(filter.CarId));
            }

            if (!string.IsNullOrEmpty(filter.UserId))
            {
                queryable = queryable.Where(x => x.UserId.Equals(filter.UserId));
            }

            if (filter.IsActive != null)
            {
                queryable = queryable.Where(x => x.IsActive.Equals(filter.IsActive.Value));
            }

            return queryable;
        }
    }
}