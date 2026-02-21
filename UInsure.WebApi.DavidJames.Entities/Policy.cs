using System.ComponentModel.DataAnnotations;

namespace UInsure.WebApi.DavidJames.DataModels
{
    public class Policy
    {
        [Key]
        public int Id { get; set; }
        public required string UniqueReference { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public bool HasClaims { get; set; }
        public bool AutoRenew { get; set; }
        public List<Policyholder> Policyholders { get; set; } = new();
        public Property Property { get; set; }
        public Payment Payment { get; set; }
    }
}
