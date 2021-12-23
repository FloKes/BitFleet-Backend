using System;

namespace BitFleet.Services.DTOs
{
    public class RideSummaryDto
    {
        public Guid RideId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public string CarBrand { get; set; }

        public string CarModel { get; set; }

        public string StartLocation { get; set; }

        public string EndLocation { get; set; }

        public string StartDate { get; set; }

        public string StartTime { get; set; }

        public string EndDate { get; set; }

        public string EndTime { get; set; }
    }
}