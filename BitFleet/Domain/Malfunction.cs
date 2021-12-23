using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BitFleet.Domain
{
    public class Malfunction
    {
        [Key]
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string RepairDescription { get; set; }

        public bool IsActive { get; set; }

        public float? RepairCost { get; set; }

        [Required] 
        public Car Car { get; set; }

        public Guid? RideId { get; set; }

        [ForeignKey(nameof(RideId))]
        public Ride Ride { get; set; }

        public string UserId { get; set; }

        public IdentityUser User { get; set; }
    }
}