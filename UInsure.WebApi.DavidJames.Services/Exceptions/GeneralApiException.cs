namespace UInsure.WebApi.DavidJames.Services.Exceptions
{
    // So we want to expose API usage errors to consumers that are useful, clean, informative but obsfucate what is happening behind the scenes
    // I like to create custom exceptions. I'd create quite a few but I am going to follow KISS for now
    public class GeneralApiException : Exception
    {
        public GeneralApiException(string message) : base(message)
        {

        }
    }
}
