using System;

namespace BitFleet.Contracts.V1.Requests.CreateRequests
{
    public class CreateVehicleServiceRequest
    {
        public Guid CarId { get; set; }

        public float Cost { get; set; }

        public string Description { get; set; }

        public bool IsFinished { get; set; }

        public bool IsActive { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime DateTime { get; set; }
    }
}