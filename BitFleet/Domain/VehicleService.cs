using System;
using System.ComponentModel.DataAnnotations;

namespace BitFleet.Domain
{
    public class VehicleService
    {
        [Key] public Guid Id { get; set; }

        [Required] public Car Car { get; set; }

        public string Description { get; set; }

        public float Cost { get; set; }

        public bool IsFinished { get; set; }

        public bool IsActive { get; set; }

        public bool IsScheduled { get; set; }

        public DateTime DateTime { get; set; }
    }
}