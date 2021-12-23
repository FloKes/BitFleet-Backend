namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class StopRideRequest
    {
        public int EndMileage { get; set; }

        public string MalfunctionDescription { get; set; }
    }
}