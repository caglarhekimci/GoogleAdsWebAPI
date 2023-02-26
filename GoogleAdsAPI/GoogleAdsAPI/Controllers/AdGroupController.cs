
using GoogleAdsAPI.ServicesAPI;
using GoogleAdsAPI.ServicesAPI.AdGroupService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdGroupController : ControllerBase
    {
        private readonly IAdGroupService _adGroupService; 

        public AdGroupController(IAdGroupService adGroupService)
        {
            _adGroupService= adGroupService;
        }

        [HttpPost("GetAdGroup")]
        public IActionResult GetAdGroup(long customerId)
        {
            var result = _adGroupService.GetAdGroup(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("GetAdGroupGeneric")]
        public IActionResult GetAdGroupGeneric(string customerId, string searchCriteria, long? campaignId)
        {
            var result = _adGroupService.GenericAdGroup(customerId, searchCriteria, campaignId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("CreateAdGroup")]
        public IActionResult CreateAdGroup(long customerId, long campaignId, string adGroupName)
        {
            var result = _adGroupService.CreateAdGroup(customerId, campaignId, adGroupName);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("GetDynamicAdGroup")]
        public IActionResult GetDynamicAdGroup(long? customerId,long? campaignId)
        {
            var result = _adGroupService.GetDynamicAdGroup(customerId,campaignId);


            //return new JsonResult(new { SRC = fSource.GetPropertyDict() });
            return Ok(result);

        }
    }
}
