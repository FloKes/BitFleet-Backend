using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Extensions;
using BitFleet.Services.DTOs;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BitFleet.Services
{
    public class MalfunctionService : IMalfunctionService
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<IdentityUser> _userManager;

        public MalfunctionService(DataContext dataContext, UserManager<IdentityUser> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        public async Task<List<MalfunctionDto>> GetAllMalfunctionsAsync()
        {
            var malfunctions = await _dataContext.Malfunctions.AsNoTracking()
                .Include(x => x.Car).AsNoTracking()
                .Include(x => x.User).AsNoTracking()
                .Include(x => x.Ride).AsNoTracking()
                .ToListAsync();

            var malfunctionDtoList = new List<MalfunctionDto>();

            foreach (var malfunction in malfunctions)
            {
                malfunctionDtoList.Add(await GenerateMalfunctionDtoAsync(malfunction));
            }

            return malfunctionDtoList;
        }

        public async Task<Malfunction> GetMalfunctionByIdAsync(Guid malfunctionId)
        {
            var malfunction = await _dataContext.Malfunctions
                .Include(x => x.Car)
                .Include(x => x.User)
                .Include(x => x.Ride)
                .SingleOrDefaultAsync(x => x.Id.Equals(malfunctionId));

            return malfunction;
        }


        public async Task<MalfunctionDto> UpdateMalfunctionAsync(Malfunction malfunction)
        {
            var updated = await _dataContext.SaveChangesAsync();

            if (updated <= 0)
            {
                return null;
            }

            var malfunctionDto = await GenerateMalfunctionDtoAsync(malfunction);
            return malfunctionDto;
        }

        public async Task<bool> DeleteMalfunctionAsync(Guid malfunctionId)
        {
            var malfunction = await GetMalfunctionByIdAsync(malfunctionId);

            if (malfunction == null || malfunction.IsActive == true)
            {
                return false;
            }

            _dataContext.Malfunctions.Remove(malfunction);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }

        public async Task<bool> CreateMalfunctionAsync(Malfunction malfunction)
        {
            _dataContext.Add(malfunction);
            var created = await _dataContext.SaveChangesAsync();

            return created > 0;
        }


        public async Task<MalfunctionDto> GenerateMalfunctionDtoAsync(Malfunction malfunction)
        {
            Guid? rideId = null;
            string userId = null;
            string userFirstName = null;
            string userLastName = null;

            if (malfunction.Ride != null)
            {
                rideId = malfunction.Ride.Id;
            }

            if (malfunction.User != null)
            {
                userId = malfunction.User.Id;
                userFirstName = await _userManager.GetFirstNameAsync(malfunction.User);
                userLastName = await _userManager.GetLastNameAsync(malfunction.User);
            }

            var malfunctionDto = new MalfunctionDto
            {
                Id = malfunction.Id,
                RepairCost = malfunction.RepairCost,
                Description = malfunction.Description,
                IsActive = malfunction.IsActive,
                RepairDescription = malfunction.RepairDescription,
                CarId = malfunction.Car.Id,
                CarBrand = malfunction.Car.Brand,
                CarModel = malfunction.Car.Model,
                UserId = userId,
                UserFirstName = userFirstName,
                UserLastName = userLastName,
                RideId = rideId
            };

            return malfunctionDto;
        }
    }
}