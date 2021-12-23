namespace BitFleet.Contracts.V1.Requests.UpdateRequests
{
    public class UpdateMalfunctionRequest
    {
        public string RepairDescription { get; set; }

        public bool IsActive { get; set; }

        public float? RepairCost { get; set; }
    }
}