using System;
using BitFleet.Domain;
using Microsoft.AspNetCore.Identity;

namespace BitFleet.Contracts.V1.Responses
{
    public class RideResponse
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; }

        public string StartLocation { get; set; }

        public string EndLocation { get; set; }

        public string StartDate { get; set; }

        public string StartTime { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        public string StartMileage { get; set; }

        public string EndMileage { get; set; }

        public CarResponse Car { get; set; }

        public UserResponse User { get; set; }
    }
}