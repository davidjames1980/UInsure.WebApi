namespace UInsure.WebApi.DavidJames.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public Policy Policy { get; set; }
        public string PaymentReference { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
    }
}
