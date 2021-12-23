using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Requests.Queries;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Contracts.V1.Responses.ErrorResponses;
using BitFleet.Domain;
using BitFleet.Extensions;
using BitFleet.Filters;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RideController : Controller
    {
        private readonly IRideService _rideService;
        private readonly ICarService _carService;
        private readonly IMalfunctionService _malfunctionService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly UserManager<IdentityUser> _userManager;

        public RideController(IRideService rideService, ICarService carService, UserManager<IdentityUser> userManager,
             IMapper mapper, IUriService uriService, IMalfunctionService malfunctionService)
        {
            _rideService = rideService;
            _carService = carService;
            _userManager = userManager;
            _mapper = mapper;
            _uriService = uriService;
            _malfunctionService = malfunctionService;
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Rides.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllRidesQuery query)
        {
            var filter = _mapper.Map<GetAllRidesFilter>(query);
            var rides = await _rideService.GetAllRidesAsync(filter);
            var rideListResponses = new List<RideResponse>();

            foreach (var ride in rides)
            {
                rideListResponses.Add(await GenerateRideResponse(ride));
            }

            return Ok(rideListResponses);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Rides.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid rideId)
        {
            var ride = await _rideService.GetRideByIdAsync(rideId);
            if (ride == null)
            {
                return NotFound();
            }

            var rideResponse = await GenerateRideResponse(ride);

            return Ok(rideResponse);
        }


        [HttpPost(ApiRoutes.Rides.Create)]
        public async Task<IActionResult> Create([FromBody] CreateRideRequest request)
        {
            var newRideId = Guid.NewGuid();
            var car = await _carService.GetCarByIdAsync(Guid.Parse(request.CarId));
            var userId = HttpContext.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            if (await _userManager.IsOnRideAsync(user))
            {
                return BadRequest(new ErrorResponse(new ErrorModel {Message = "You are already on a ride"}));
            }

            if (car == null)
            {
                return BadRequest(new ErrorResponse(new ErrorModel {Message = "Car not found"}));
            }

            if (car.IsOnRide)
            {
                return BadRequest(new ErrorResponse(new ErrorModel {Message = "Selected car is unavailable"}));
            }

            var ride = new Ride
            {
                Id = newRideId,
                UserId = userId,
                User = user,
                Car = car,
                IsActive = true,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.MaxValue,
                StartLocation = request.StartLocation,
                EndLocation = request.EndLocation,
                StartMileage = car.Mileage,
                EndMileage = 0
            };

            var createdRide = await _rideService.CreateRideAsync(ride);
            if (createdRide == null)
            {
                return UnprocessableEntity(new ErrorResponse(new ErrorModel {Message = "Ride unable to be created"}));
            }

            await _userManager.SetOnRideAsync(user, "true");

            var locationUri = _uriService.GetRideUri(createdRide.Id.ToString());
            var rideResponse = await GenerateRideResponse(createdRide);
            return Created(locationUri, rideResponse);
        }


        [HttpPatch(ApiRoutes.Rides.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid rideId, [FromBody] StopRideRequest request)
        {
            var ride = await _rideService.GetRideByIdAsync(rideId);

            var updated =
                await UpdateStoppedRideAndRelatedEntities(ride, request.EndMileage, request.MalfunctionDescription);

            if (!updated)
            {
                return BadRequest();
            }

            var rideResponse = await GenerateRideResponse(ride);
            return Ok(_mapper.Map<RideResponse>(rideResponse));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete(ApiRoutes.Rides.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid rideId)
        {
            var deleted = await _rideService.DeleteRideAsync(rideId);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
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