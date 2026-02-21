using UInsure.WebApi.DavidJames.DataModels;

namespace UInsure.WebApi.DavidJames.Services.Exceptions
{
    // Now usually I would create many of these.  But I feel that is overkill for a tech test e.g. I'll add a really specific one
    // below.  They are super useful in TDD and dividing expected business / unexpected exceptions.  I have not used this as I feel it would
    // be overkill for a tech test
    public class PolicyUnderageException : Exception
    {
        public PolicyUnderageException(string uniquePolicyId, Policyholder policyHolder) :
            base($"Policy holder {policyHolder.FirstName} {policyHolder.LastName} does not meet the threshold of being the age of 16 at the date of policy start.")
        {

        }
    }
}
