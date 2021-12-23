using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitFleet.Services
{
    public class VehicleServiceService : IVehicleServiceService
    {
        private readonly DataContext _dataContext;

        public VehicleServiceService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<VehicleService>> GetVehicleServicesAsync()
        {
            var queryable = _dataContext.VehicleServices.AsNoTracking()
                .Include(x => x.Car).AsNoTracking()
                .AsQueryable();

            //queryable = AddFiltersOnQuery(filter, queryable);

            return await queryable.ToListAsync();
        }

        public async Task<VehicleService> GetVehicleServiceByIdAsync(Guid vehicleServiceId)
        {
            var vehicleService = await _dataContext.VehicleServices.Include(x=>x.Car)
                .SingleOrDefaultAsync(x => x.Id == vehicleServiceId);

            return vehicleService;
        }

        public async Task<bool> CreateVehicleServiceAsync(VehicleService vehicleService)
        {
            await _dataContext.VehicleServices.AddAsync(vehicleService);
            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> UpdateVehicleServiceAsync(VehicleService vehicleServiceToUpdate)
        {
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteVehicleServiceAsync(Guid vehicleServiceId)
        {
            var vehicleService = await GetVehicleServiceByIdAsync(vehicleServiceId);

            if (vehicleService == null)
            {
                return false;
            }

            _dataContext.VehicleServices.Remove(vehicleService);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }
    }
}