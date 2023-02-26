using Google.Ads.GoogleAds.Lib;
using GoogleAdsAPI.ServicesAPI;
using GoogleAdsAPI.ServicesAPI.AdService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdController : ControllerBase
    { 
        private readonly IAdService _adService;

        public AdController(IAdService adService)
        {
            _adService = adService;
        }

        [HttpPost("CreateResponsiveSearchAd")]
        public IActionResult CreateResponsiveSearchAd(long customerId, long adGroupId, string headLine)
        {
            var result = _adService.CreateResponsiveSearchAd(customerId, adGroupId, headLine);

            if (result.Succes)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }
        [HttpPost("RunAds")]
        public IActionResult RunAds( long customerId, long adGroupId)
        {
            _adService.Run(customerId, adGroupId);
             
            return Ok();
        }
    }
}
