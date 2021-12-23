using System;
using BitFleet.Contracts.V1;
using BitFleet.Services.Interfaces;

namespace BitFleet.Services
{
    public class UriService : IUriService
    {
        private readonly string _baseUri;

        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Uri GetCarUri(string carId)
        {
            return new Uri(_baseUri + ApiRoutes.Cars.Get.Replace("{carId}", carId));
        }

        public Uri GetAllCarsUri()
        {
            return new Uri(_baseUri);
        }

        public Uri GetRideUri(string rideId)
        {
            return new Uri(_baseUri + ApiRoutes.Rides.Get.Replace("{rideId}",rideId));
        }

        public Uri GetAllRidesUri()
        {
            return new Uri(_baseUri);
        }

        public Uri GetVehicleServiceUri(string vehicleServiceId)
        {
            return new Uri(_baseUri + ApiRoutes.VehicleServices.Get.Replace("{vehicleServiceId}",vehicleServiceId));
        }
    }
}