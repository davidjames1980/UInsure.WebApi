namespace UInsure.WebApi.DavidJamesForbes.DataModels
{
    public class Refund
    {
        public required string RefundReference { get; set; }
        public required string Type { get; set; }
        public decimal Amount { get; set; }
    }
}
