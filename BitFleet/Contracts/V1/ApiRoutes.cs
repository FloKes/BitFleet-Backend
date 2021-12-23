using System.Diagnostics;

namespace BitFleet.Contracts.V1
{
    public static class ApiRoutes
    {
        public const string Root = "api";

        public const string Version = "v1";

        public const string Base = Root + "/" + Version;


        public static class Cars
        {
            public const string Create = Base + "/cars";

            public const string GetAll = Base + "/cars";

            public const string GetAllAvailable = Base + "/cars/available";

            public const string Get = Base + "/cars/{carId}";

            public const string GetCarCosts = Base + "/cars/{carId}/costs";

            public const string Update = Base + "/cars/{carId}";

            public const string UpdateCarCosts = Base + "/cars/{carId}/costs";

            public const string Delete = Base + "/cars/{carId}";
        }

        public static class Rides
        {
            public const string Create = Base + "/rides";

            public const string GetAll = Base + "/rides";

            public const string Get = Base + "/rides/{rideId}";

            public const string Update = Base + "/rides/{rideId}";

            public const string Delete = Base + "/rides/{rideId}";
        }

        public static class Malfunctions
        {
            public const string GetAll = Base + "/malfunctions";

            public const string Get = Base + "/malfunctions/{malfunctionId}";

            public const string Update = Base + "/malfunctions/{malfunctionId}";

            public const string Delete = Base + "/malfunctions/{malfunctionId}";
        }

        public static class VehicleServices
        {
            public const string Create = Base + "/vehicleServices";

            public const string GetAll = Base + "/vehicleServices";

            public const string Get = Base + "/vehicleServices/{vehicleServiceId}";

            public const string Update = Base + "/vehicleServices/{vehicleServiceId}";

            public const string Delete = Base + "/vehicleServices/{vehicleServiceId}";
        }


        public static class Users
        {
            public const string GetAll = Base + "/users";

            public const string Get = Base + "/users/{userId}";

            public const string Update = Base + "/users/{userId}";

            public const string Delete = Base + "/users/{userId}";

            public const string GetActiveRide = Base + "/users/{userId}/activeRide";

            public const string StopActiveRide = Base + "/users/{userId}/activeRide";
        }


        //Breaks the RESTful principle with having verbs in endpoints
        //The whole identity section should be in a separate identity server, which doesn't need to be REST api
        //This is done for simplicity sake
        public static class Identity
        {
            public const string Login = Base + "/identity/login";

            public const string Register = Base + "/identity/register";

            public const string Refresh = Base + "/identity/refresh";
        }
    }
}