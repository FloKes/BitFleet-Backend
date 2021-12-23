using System;

namespace BitFleet.Services.Interfaces
{
    public interface IUriService
    {
        Uri GetCarUri(string carId);

        Uri GetAllCarsUri();

        Uri GetRideUri(string rideId);

        Uri GetAllRidesUri();

        Uri GetVehicleServiceUri(string vehicleServiceId);
    }
}