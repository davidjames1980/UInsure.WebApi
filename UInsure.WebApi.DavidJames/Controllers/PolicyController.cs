using Microsoft.AspNetCore.Mvc;
using UInsure.WebApi.DavidJames.Models;
using UInsure.WebApi.DavidJames.Models.Requests;
using UInsure.WebApi.DavidJames.Services;
using UInsure.WebApi.DavidJames.Services.Exceptions;

namespace UInsure.WebApi.DavidJames.Controllers
{
    /// <summary>
    /// I like to follow the 'resources/id/command' RESTful naming convention.  Policy is not plural as a resource
    /// so I have kept it at PolicyController and adjusted the route.
    /// 
    /// I will make all of the endpoints async as standard in .NET core to ensure scalability and throughput even though this
    /// is just a coding test
    /// 
    /// My approach to catching exceptions is as follows:
    /// I want to catch business exceptions aka when the caller has asked us to do something we cannot do. These are our GeneralApiExceptions
    /// In Enterprise we would likely want something more specific and multiple ExceptionTypes.  This also assists us in unit testing.  Expected/business
    /// exceptions can be happily caught and delivered to the caller in informative terms
    /// 
    /// Unhandled exceptions (expectional exceptions!) we want to flow through to our exception handler and we certainly don't want to be showing
    /// call stacks etc so 500 it when not in dev
    /// 
    /// It is completely debatable if UnprocessableEntity 422 is the best result to send but I've chosen this for new based on its definition. 'I cannot complete
    /// the request for this reason - nothing has exploded. We just cannot process it.' :)
    /// 
    /// </summary>
    [ApiController]
    [Route("api/policies")]
    public class PolicyController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PolicyController(IPolicyService policyService)
        {
            _policyService = policyService;
        }

        [HttpPost]
        public async Task<IActionResult> SellPolicy([FromBody] PolicyModel policy)
        {
            try
            {
                var result = await _policyService.SellPolicy(policy);

                // Since the requirements and the rest of the API depends on unique reference, I see no
                // reason to return much more than a conformation of created (201). For security we don't want to expose anything more than is required.
                //
                // With regard to successful changes I like to return something useful (although uniqueRef isn't useful as the caller created it
                // - I'll raise this question in the further docs) plus a human message.
                return CreatedAtAction(
                    nameof(GetPolicy),
                        new { uniqueReference = result.UniqueReference },
                        new { uniqueReference = result.UniqueReference, message = $"Policy '{policy.UniqueReference}' created successfully." }
                );
            }
            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Anything involving DateTime can be trix because we get tunnel vision sometimes
        /// and think everyone works on GMT, UK formats or assuming they are using .NET for that matter. Date-times can be a nightmare
        /// so we are going to use datetime offset and a request model for maintainability
        /// 
        /// There is also an argument this could be PATCH since I'd image we don't delete the resource.  We would mark it as cancelled
        /// </summary>
        [HttpPost("{uniqueReference}/cancellation")]
        public async Task<IActionResult> CancelPolicy(string uniqueReference, [FromBody] CancelPolicyRequest cancelPolicyRequest)
        {
            try
            {
                var refundAmount = await _policyService.CancelPolicy(uniqueReference, cancelPolicyRequest.CancellationDate.UtcDateTime);
                return Ok(new
                {
                    uniqueReference,
                    refundAmount,
                    message = $"Policy '{uniqueReference}' has been cancelled and £{refundAmount} will be refunded."
                });
            }
            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{uniqueReference}/renewals")]
        public async Task<IActionResult> RenewPolicy(string uniqueReference)
        {
            try
            {
                var renewedPolicy = await _policyService.RenewPolicy(uniqueReference);

                return Ok(new { uniqueReference, renewedPolicy.StartDate, renewedPolicy.EndDate, message = $"Policy '{uniqueReference} has been renewed." });
            }

            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{uniqueReference}/cancellation-quote")]
        public async Task<IActionResult> CalculateCancellationRefund(string uniqueReference, [FromQuery] CancelPolicyRequest cancellationQuoteRequest)
        {
            try
            {
                var refundAmount = await _policyService.CalculateCancellationRefund(uniqueReference, 
                    cancellationQuoteRequest.CancellationDate.UtcDateTime);
                
                return Ok(new { RefundAmount = refundAmount });
            }
            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{uniqueReference}/renewal-eligibility")]
        public async Task<IActionResult> CanRenewPolicy(string uniqueReference)
        {
            try
            {
                var canRenew = await _policyService.CanRenewPolicy(uniqueReference);
                return Ok(new { CanRenew = canRenew });
            }
            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{uniqueReference}")]
        public async Task<IActionResult> GetPolicy(string uniqueReference)
        {
            try
            {
                var policy = await _policyService.GetPolicy(uniqueReference);
                return Ok(policy);
            }
            catch (GeneralApiException ex)
            {
                return UnprocessableEntity(ex.Message);
            }
            catch (PolicyNotFoundException)
            {
                return NotFound();
            }
        }


    }
}
