using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Contracts.V1.Responses.ErrorResponses;
using BitFleet.Domain;
using BitFleet.Extensions;
using BitFleet.Services;
using BitFleet.Services.DTOs;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly IRideService _rideService;
        private readonly IMalfunctionService _malfunctionService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IRideService rideService,
            UserManager<IdentityUser> userManager, IMalfunctionService malfunctionService)
        {
            _rideService = rideService;
            _userManager = userManager;
            _malfunctionService = malfunctionService;
        }


        //Maybe revision
        //
        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var usersList = await _userManager.GetUsersAsync();
            var userResponsesList = new List<UserResponse>();

            foreach (var user in usersList)
            {
                userResponsesList.Add(await _userManager.GenerateUserResponse(user));
            }

            return Ok(userResponsesList);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Users.Get)]
        public async Task<IActionResult> Get(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userResponse = await _userManager.GenerateUserResponse(user);
            return Ok(userResponse);
        }


        [HttpGet(ApiRoutes.Users.GetActiveRide)]
        public async Task<IActionResult> GetActiveRideForUser([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("No user with that id exists");
            }

            var ride = await _rideService.GetActiveRideByUserIdAsync(userId);

            if (ride == null)
            {
                return NotFound("User has no active rides");
            }

            var rideResponse = await GenerateRideResponse(ride);
            return Ok(rideResponse);
        }


        [HttpPatch(ApiRoutes.Users.StopActiveRide)]
        public async Task<IActionResult> StopActiveRideForUser([FromRoute] string userId,
            [FromBody] StopRideRequest request)
        {
            var ride = await _rideService.GetActiveRideByUserIdAsync(userId);

            if (ride == null)
            {
                return NotFound("User has no active rides");
            }

            var updated =
                await UpdateStoppedRideAndRelatedEntities(ride, request.EndMileage, request.MalfunctionDescription);

            if (!updated)
            {
                return BadRequest();
            }

            var rideResponse = await GenerateRideResponse(ride);
            return Ok(rideResponse);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var deleted = await _userManager.DeleteAsync(user);

            if (deleted != null)
            {
                return NoContent();
            }

            return NotFound();
        }


        [HttpPatch(ApiRoutes.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] string userId, [FromBody] UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(request.UserName))
            {
                user.UserName = request.UserName;
                var updated = await _userManager.UpdateAsync(user);

                if (updated.Errors.Count() != 0)
                {
                    return BadRequest(updated.Errors.First().Code);
                }
            }

            if (!string.IsNullOrEmpty(request.OldPassword) && !string.IsNullOrEmpty(request.NewPassword))
            {
                var changed = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (changed.Errors.Count() != 0)
                {
                    return BadRequest(changed.Errors.First().Code);
                }
            }

            return Ok();
        }


        private async Task<bool> UpdateStoppedRideAndRelatedEntities(Ride ride, int endMileage,
            string malfunctionDescription = null)
        {
            ride.IsActive = false;
            ride.EndMileage = endMileage;
            ride.EndDateTime = DateTime.UtcNow;
            ride.Car.IsOnRide = false;
            ride.Car.Mileage = ride.EndMileage;
            ride.Car.KilometersSinceLastService += ride.EndMileage - ride.StartMileage;
            ride.Car.CarCosts.FuelCostsPerKm = ride.Car.CarCosts.FuelCosts / ride.Car.Mileage;
            ride.Car.CarCosts.ServiceCostsPerKm = ride.Car.CarCosts.ServiceCosts / ride.Car.Mileage;
            ride.Car.CarCosts.TotalCostsPerKm = ride.Car.CarCosts.TotalCosts / ride.Car.Mileage;
            if (ride.Car.KilometersSinceLastService >= ride.Car.KilometersNeededBeforeService)
            {
                ride.Car.NeedsService = true;
            }

            var updated = await _rideService.UpdateRideAsync();
            if (updated == false)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(malfunctionDescription))
            {
                var malfunction = new Malfunction
                {
                    Id = Guid.NewGuid(),
                    Car = ride.Car,
                    RepairCost = null,
                    Description = malfunctionDescription,
                    IsActive = true,
                    RepairDescription = null,
                    RideId = ride.Id,
                    Ride = ride,
                    User = ride.User
                };
                await _malfunctionService.CreateMalfunctionAsync(malfunction);
            }

            var user = await _userManager.FindByIdAsync(ride.UserId);
            await _userManager.SetOnRideAsync(user, "false");
            return true;
        }


        private async Task<RideResponse> GenerateRideResponse(Ride ride)
        {
            var startDate = ride.StartDateTime.AddHours(2).ToShortDateString();
            var startTime = ride.StartDateTime.AddHours(2).ToLongTimeString();
            var endMileage = "-";
            var endDate = "-";

            var endTime = "-";
            if (!ride.IsActive)
            {
                endMileage = ride.EndMileage.ToString();
                endDate = ride.EndDateTime.AddHours(2).ToShortDateString();
                endTime = ride.EndDateTime.AddHours(2).ToLongTimeString();
            }

            //Ideally we would use HATEOAS for including the link to the car data for the ride, but for simplicity we are including it in the ride response

            CarResponse carResponse = null;
            if (ride.Car != null)
            {
                carResponse = new CarResponse
                {
                    Brand = ride.Car.Brand,
                    Model = ride.Car.Model,
                    ModelYear = ride.Car.ModelYear,
                    FuelType = ride.Car.FuelType,
                    KilometersPerLiter = ride.Car.KilometersPerLiter,
                    IsOnRide = ride.Car.IsOnRide,
                    Id = ride.Car.Id,
                    Mileage = ride.Car.Mileage,
                    NeedsService = ride.Car.NeedsService,
                    KilometersNeededBeforeService = ride.Car.KilometersNeededBeforeService,
                    KilometersSinceLastService = ride.Car.KilometersSinceLastService,
                    IsOnService = ride.Car.IsOnService,
                    IsScheduledForService = ride.Car.IsScheduledForService
                };
            }

            UserResponse userResponse = null;
            if (ride.User != null)
            {
                userResponse = await _userManager.GenerateUserResponse(ride.User);
            }

            var rideResponse = new RideResponse
            {
                Id = ride.Id,
                StartLocation = ride.StartLocation,
                EndLocation = ride.EndLocation,
                IsActive = ride.IsActive,
                StartMileage = ride.StartMileage.ToString(),
                EndMileage = endMileage,
                StartDate = startDate,
                StartTime = startTime,
                EndDate = endDate,
                EndTime = endTime,
                Car = carResponse,
                User = userResponse
            };
            return rideResponse;
        }
    }
}