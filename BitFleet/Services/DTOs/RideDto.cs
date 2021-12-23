using System;

namespace BitFleet.Services.DTOs
{
    public class RideDto
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }

        public string StartLocation { get; set; }

        public string EndLocation { get; set; }

        public string StartDate { get; set; }

        public string StartTime { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        //these are string because it's easier to display on the frontend than if the value was null, 
        //if the value is not set, its "-" by default
        public string StartMileage { get; set; }

        public string EndMileage { get; set; }

        public CarDto Car { get; set; }

        public UserDto User { get; set; }
    }
}