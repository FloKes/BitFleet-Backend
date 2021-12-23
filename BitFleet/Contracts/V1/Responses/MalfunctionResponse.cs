using System;

namespace BitFleet.Contracts.V1.Responses
{
    public class MalfunctionResponse
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string RepairDescription { get; set; }

        public bool IsActive { get; set; }

        public float? RepairCost { get; set; }

        public Guid CarId { get; set; }

        public string CarBrand { get; set; }

        public string CarModel { get; set; }

        public string UserId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public Guid? RideId { get; set; }
    }
}