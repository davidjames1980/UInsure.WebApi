using System.ComponentModel.DataAnnotations;

namespace UInsure.WebApi.DavidJamesForbes.Models
{
    public class PolicyModel
    {
        public required string UniqueReference { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Amount { get; set; }
        public bool HasClaims { get; set; }
        public bool AutoRenew { get; set; }
        public List<PolicyholderModel> Policyholders { get; set; } = new();
        public PropertyModel Property { get; set; } = new();
        public PaymentTypeModel PaymentType { get; set; } = new();
    }
}
