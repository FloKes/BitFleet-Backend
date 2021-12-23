using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BitFleet.Domain
{
    public class Ride
    {
        [Key] public Guid Id { get; set; }

        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public Car Car { get; set; }

        public Malfunction Malfunction { get; set; }

        public bool IsActive { get; set; }

        public string StartLocation { get; set; }

        public string EndLocation { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public int StartMileage { get; set; }

        public int EndMileage { get; set; }
    }
}