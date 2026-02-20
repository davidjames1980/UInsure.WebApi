namespace UInsure.WebApi.DavidJamesForbes.DataModels
{
    public class Policyholder
    {
        public int Id { get; set; }

        public int PolicyId { get; set; }
        public Policy Policy { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }

    }
}
