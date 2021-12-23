using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services.DTOs;

namespace BitFleet.Services.Interfaces
{
    public interface IRideService
    {
        Task<List<Ride>> GetAllRidesAsync(GetAllRidesFilter filter = null);

        Task<Ride> GetRideByIdAsync(Guid rideId);

        Task<Ride> CreateRideAsync(Ride ride);

        //Task<RideDto> StopRideAsync(Guid rideId, int endMileage, string malfunctionDescription = null);

        Task<bool> UpdateRideAsync();

        Task<bool> DeleteRideAsync(Guid rideId);

        Task<Ride> GetActiveRideByUserIdAsync(string userId);
    }
}