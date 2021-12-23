using System;

namespace BitFleet.Filters
{
    public class GetAllRidesFilter
    {
        public string UserId { get; set; }

        public Guid CarId { get; set; }

        public bool? IsActive { get; set; }
    }
}