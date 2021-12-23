using System;
using BitFleet.Contracts.V1.Responses;

namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class UpdateVehicleServiceRequest
    {
        public string Description { get; set; }

        public float? Cost { get; set; }

        public bool? IsFinished { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsScheduled { get; set; }

        public DateTime? DateTime { get; set; }
    }
}