using System.ComponentModel.DataAnnotations;

namespace UInsure.WebApi.DavidJames.DataModels
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }
        public int PolicyId { get; set; }
        public Policy Policy { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string Postcode { get; set; }
    }
}
