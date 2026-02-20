namespace UInsure.WebApi.DavidJamesForbes.Models
{
    public class RefundModel
    {
        public required string RefundReference { get; set; }
        public required string Type { get; set; }
        public decimal Amount { get; set; }
    }
}
