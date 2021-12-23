using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitFleet.Domain;

namespace BitFleet.Services.Interfaces
{
    public interface IVehicleServiceService
    {

        Task<List<VehicleService>> GetVehicleServicesAsync();

        Task<VehicleService> GetVehicleServiceByIdAsync(Guid vehicleServiceId);

        Task<bool> CreateVehicleServiceAsync(VehicleService vehicleService);

        Task<bool> UpdateVehicleServiceAsync(VehicleService vehicleServiceToUpdate);

        Task<bool> DeleteVehicleServiceAsync(Guid vehicleServiceId);
        
    }
}