namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class UpdateCarRequest
    {
        public string Brand { get; set; }

        public string Model { get; set; }

        public int? ModelYear { get; set; }

        public bool? NeedsService { get; set; }

        public bool? IsOnService { get; set; }

        public bool? IsOnRide { get; set; }
    }
}