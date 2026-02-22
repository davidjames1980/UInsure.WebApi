using System.Text.RegularExpressions;

namespace UInsure.WebApi.DavidJames.Services.Extensions
{
    public static partial class PostcodeValidatorExtensions
    {
        [GeneratedRegex(@"^[A-Z]{1,2}[0-9][0-9A-Z]?\s*[0-9][A-BD-HJLNP-UW-Z]{2}$",
            RegexOptions.IgnoreCase)]
        private static partial Regex UkPostcodeRegex();

        public static bool IsValidUkPostcode(this string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                return false;

            return UkPostcodeRegex().IsMatch(postcode.Trim());
        }
    }
}