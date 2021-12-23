using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BitFleet.Contracts.V1;
using BitFleet.Contracts.V1.Requests;
using BitFleet.Contracts.V1.Requests.CreateRequests;
using BitFleet.Contracts.V1.Requests.Queries;
using BitFleet.Contracts.V1.Requests.UpdateRequests;
using BitFleet.Contracts.V1.Responses;
using BitFleet.Data;
using BitFleet.Domain;
using BitFleet.Filters;
using BitFleet.Services;
using BitFleet.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualBasic;

namespace BitFleet.Controllers.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class CarController : Controller
    {
        private readonly ICarService _carService;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;

        public CarController(ICarService carService, IMapper mapper, IUriService uriService)
        {
            _carService = carService;
            _mapper = mapper;
            _uriService = uriService;
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Cars.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCarsQuery query = null)
        {
            var filter = _mapper.Map<GetAllCarsFilter>(query);

            var cars = await _carService.GetCarsAsync(filter);
            var carsResponse = _mapper.Map<List<CarResponse>>(cars);

            return Ok(carsResponse);
        }


        //exists so normal car users can see available cars
        [Authorize(Roles = "Admin, SuperAdmin, CarUser")]
        [HttpGet(ApiRoutes.Cars.GetAllAvailable)]
        public async Task<IActionResult> GetAllAvailable()
        {
            var filter = new GetAllCarsFilter {IsOnRide = false, IsOnService = false};

            var cars = await _carService.GetCarsAsync(filter);
            var carsResponse = _mapper.Map<List<CarResponse>>(cars);

            return Ok(carsResponse);
        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Cars.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid carId)
        {
            var car = await _carService.GetCarByIdAsync(carId);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CarResponse>(car));
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPost(ApiRoutes.Cars.Create)]
        public async Task<IActionResult> Create([FromBody] CreateCarRequest request)
        {
            var newCarId = Guid.NewGuid();
            var newCar = new Car
            {
                Id = newCarId,
                Brand = request.Brand,
                Model = request.Model,
                ModelYear = request.ModelYear,
                FuelType = request.FuelType,
                KilometersPerLiter = request.KilometersPerLiter,

                //we add +1 because if someone adds a service when the car didn't go on any rides, calculating the cost/km will result 
                //in division by 0
                Mileage = request.MileageWhenBought + 1,
                MileageWhenBought = request.MileageWhenBought,
                KilometersNeededBeforeService = request.KilometersNeededBeforeService,
                KilometersSinceLastService = 0,
                CarCosts = new CarCosts
                {
                    Id = Guid.NewGuid(),
                }
            };

            await _carService.CreateCarAsync(newCar);

            var locationUri = _uriService.GetCarUri(newCar.Id.ToString());
            return Created(locationUri, _mapper.Map<CarResponse>(newCar));
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpDelete(ApiRoutes.Cars.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid carId)
        {
            var deleted = await _carService.DeleteCarAsync(carId);

            if (deleted)
            {
                return NoContent();
            }

            return NotFound();
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpGet(ApiRoutes.Cars.GetCarCosts)]
        public async Task<IActionResult> GetCarCosts([FromRoute] Guid carId)
        {
            var carCosts = await _carService.GetCarCostsByCarIdAsync(carId);

            if (carCosts == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CarCostsResponse>(carCosts));
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPatch(ApiRoutes.Cars.UpdateCarCosts)]
        public async Task<IActionResult> UpdateCarCosts([FromRoute] Guid carId,
            [FromBody] UpdateCarCostsRequest request)
        {
            var car = await _carService.GetCarByIdAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            var carCosts = car.CarCosts;

            ApplyUpdatesToCarCosts(car, request);

            var updated = await _carService.UpdateCarAsync();
            if (updated)
            {
                return Ok(_mapper.Map<CarCostsResponse>(carCosts));
            }

            return UnprocessableEntity();
        }


        [Authorize(Roles = "Admin, SuperAdmin")]
        [HttpPatch(ApiRoutes.Cars.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid carId, [FromBody] UpdateCarRequest request)
        {
            var car = await _carService.GetCarByIdAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            ApplyUpdatesToCar(car, request);

            var updated = await _carService.UpdateCarAsync();
            if (updated)
            {
                return Ok(_mapper.Map<CarResponse>(car));
            }

            return UnprocessableEntity();
        }

        private static void ApplyUpdatesToCar(Car car, UpdateCarRequest request)
        {
            if (!string.IsNullOrEmpty(request.Brand))
            {
                car.Brand = request.Brand;
            }

            if (!string.IsNullOrEmpty(request.Model))
            {
                car.Model = request.Model;
            }

            if (request.ModelYear != null)
            {
                car.ModelYear = request.ModelYear.Value;
            }

            if (request.NeedsService != null)
            {
                var needsService = request.NeedsService.Value;
                car.NeedsService = needsService;
            }

            if (request.IsOnRide != null)
            {
                var isOnRide = request.IsOnRide.Value;
                car.NeedsService = isOnRide;
            }

            if (request.IsOnService != null)
            {
                var isOnService = request.IsOnService.Value;
                car.IsOnService = isOnService;
            }
        }


        private static void ApplyUpdatesToCarCosts(Car car, UpdateCarCostsRequest request)
        {
            if (request.ServiceCosts != null)
            {
                car.CarCosts.ServiceCosts = request.ServiceCosts.Value;
            }

            if (request.ServiceCostsToAdd != null)
            {
                car.CarCosts.ServiceCosts += request.ServiceCostsToAdd.Value;
            }

            if (request.FuelCosts != null)
            {
                car.CarCosts.FuelCosts = request.FuelCosts.Value;
            }

            if (request.FuelCostsToAdd != null)
            {
                car.CarCosts.FuelCosts += request.FuelCostsToAdd.Value;
            }

            car.CarCosts.FuelCostsPerKm = car.CarCosts.FuelCosts / (car.Mileage - car.MileageWhenBought);
            car.CarCosts.ServiceCostsPerKm = car.CarCosts.ServiceCosts / (car.Mileage - car.MileageWhenBought);
            car.CarCosts.TotalCosts=car.CarCosts.FuelCosts + car.CarCosts.ServiceCosts ;
            car.CarCosts.TotalCostsPerKm = car.CarCosts.TotalCosts / (car.Mileage - car.MileageWhenBought);
        }
    }
}