using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitFleet.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VehicleServiceController : Controller
    {
        private readonly IVehicleServiceService _vehicleServiceService;
        private readonly ICarService _carService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public VehicleServiceController(IMapper mapper, IVehicleServiceService vehicleServiceService,
            ICarService carService, IUriService uriService)
        {
            _mapper = mapper;
            _vehicleServiceService = vehicleServiceService;
            _carService = carService;
            _uriService = uriService;
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.VehicleServices.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            //var filter = _mapper.Map<GetAllCarsFilter>(query);

            var vehicleServices = await _vehicleServiceService.GetVehicleServicesAsync();
            var vehicleServicesResponse = _mapper.Map<List<VehicleServiceResponse>>(vehicleServices);

            return Ok(vehicleServicesResponse);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.VehicleServices.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid vehicleServiceId)
        {
            var vehicleService = await _vehicleServiceService.GetVehicleServiceByIdAsync(vehicleServiceId);

            if (vehicleService == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<VehicleServiceResponse>(vehicleService));
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost(ApiRoutes.VehicleServices.Create)]
        public async Task<IActionResult> Create([FromBody] CreateVehicleServiceRequest request)
        {
            var car = await _carService.GetCarByIdAsync(request.CarId);

            var newVehicleServiceId = Guid.NewGuid();
            var newVehicleService = new VehicleService
            {
                Id = newVehicleServiceId,
                Car = car,
                Cost = request.Cost,
                DateTime = request.DateTime,
                Description = request.Description,
                IsFinished = request.IsFinished,
                IsActive = request.IsActive,
                IsScheduled = request.IsScheduled
            };

            newVehicleService.Car.IsScheduledForService = request.IsScheduled;
            newVehicleService.Car.IsOnService = request.IsActive;

            if (newVehicleService.IsFinished)
            {
                newVehicleService.Car.NeedsService = false;
                newVehicleService.Car.IsOnService = false;
                newVehicleService.Car.IsScheduledForService = false;
                newVehicleService.Car.KilometersSinceLastService = 0;
                newVehicleService.Car.CarCosts.ServiceCosts += newVehicleService.Cost;
                newVehicleService.Car.CarCosts.ServiceCostsPerKm = newVehicleService.Car.CarCosts.ServiceCosts / (newVehicleService.Car.Mileage - newVehicleService.Car.MileageWhenBought);
                newVehicleService.Car.CarCosts.TotalCosts = newVehicleService.Car.CarCosts.ServiceCosts + newVehicleService.Car.CarCosts.FuelCosts;
                newVehicleService.Car.CarCosts.TotalCostsPerKm = newVehicleService.Car.CarCosts.TotalCosts / (newVehicleService.Car.Mileage - newVehicleService.Car.MileageWhenBought);
            }

            await _vehicleServiceService.CreateVehicleServiceAsync(newVehicleService);

            var locationUri = _uriService.GetVehicleServiceUri(newVehicleService.Id.ToString());
            return Created(locationUri, _mapper.Map<VehicleServiceResponse>(newVehicleService));
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete(ApiRoutes.VehicleServices.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid vehicleServiceId)
        {
            var vehicleService = await _vehicleServiceService.GetVehicleServiceByIdAsync(vehicleServiceId);

            //we are getting the car object again because we need to access the CarCosts property of the car
            var car = await _carService.GetCarByIdAsync(vehicleService.Car.Id);

            car.CarCosts.ServiceCosts -= vehicleService.Cost;
            car.CarCosts.ServiceCostsPerKm = car.CarCosts.ServiceCosts / (car.Mileage - car.MileageWhenBought);
            car.CarCosts.TotalCosts = car.CarCosts.ServiceCosts + car.CarCosts.FuelCosts;
            car.CarCosts.TotalCostsPerKm = car.CarCosts.TotalCosts/
                                           (car.Mileage - car.MileageWhenBought);


            var deleted = await _vehicleServiceService.DeleteVehicleServiceAsync(vehicleServiceId);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPatch(ApiRoutes.VehicleServices.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid vehicleServiceId,
            [FromBody] UpdateVehicleServiceRequest request)
        {
            var vehicleService = await _vehicleServiceService.GetVehicleServiceByIdAsync(vehicleServiceId);
            var car = await _carService.GetCarByIdAsync(vehicleService.Car.Id);

            ApplyUpdatesToVehicleServiceAndCar(vehicleService, car, request);

            var updated = await _vehicleServiceService.UpdateVehicleServiceAsync(vehicleService);

            if (updated)
            {
                return Ok(_mapper.Map<VehicleServiceResponse>(vehicleService));
            }

            return NotFound();
        }


        private static void ApplyUpdatesToVehicleServiceAndCar(VehicleService vehicleService, Car car,
            UpdateVehicleServiceRequest request)
        {
            if (!string.IsNullOrEmpty(request.Description))
            {
                vehicleService.Description = request.Description;
            }

            if (request.Cost != null)
            {
                vehicleService.Cost = request.Cost.Value;
            }

            if (request.IsFinished != null)
            {
                vehicleService.IsFinished = request.IsFinished.Value;
                if (vehicleService.IsFinished)
                {
                    car.IsOnService = false;
                    car.NeedsService = false;
                    car.KilometersSinceLastService = 0;
                    car.CarCosts.ServiceCosts += vehicleService.Cost;
                    car.CarCosts.ServiceCostsPerKm = car.CarCosts.ServiceCosts / (car.Mileage - car.MileageWhenBought);
                    car.CarCosts.TotalCosts = car.CarCosts.ServiceCosts + car.CarCosts.FuelCosts;
                    car.CarCosts.TotalCostsPerKm = car.CarCosts.TotalCosts / (car.Mileage - car.MileageWhenBought);
                    vehicleService.IsActive = false;
                    vehicleService.IsScheduled = false;
                }
            }

            if (request.IsActive != null)
            {
                vehicleService.IsActive = request.IsActive.Value;
                car.IsOnService = vehicleService.IsActive;
            }

            if (request.IsScheduled != null)
            {
                vehicleService.IsScheduled = request.IsScheduled.Value;
                car.IsScheduledForService = vehicleService.IsScheduled;
            }

            if (request.DateTime != null)
            {
                vehicleService.DateTime = request.DateTime.Value;
            }
        }
    }
}