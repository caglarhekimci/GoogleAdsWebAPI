
using GoogleAdsAPI.ServicesAPI;
using GoogleAdsAPI.ServicesAPI.CampaignService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
       
        public CampaignController(ICampaignService campaigns)
        {
            _campaignService = campaigns;
        } 

        [HttpPost("GetCampaign")]
        public IActionResult GetCampaign(long customerId)
        {
            var result = _campaignService.GetCampaign(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("GetCampaignSummurize")]
        public IActionResult GetSummurizeCampaign(long customerId)
        {
            var result = _campaignService.GetSummurizeCampaign(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("GetCampaignGeneric")]
        public IActionResult GetCampaignGeneric(string customerId, string searchCriteria)
        {
            var result = _campaignService.GenericCampaign(customerId, searchCriteria);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("CreateCampaign")]
        public IActionResult CreateAdGroup(long customerId, string campaignName)
        {
            var result = _campaignService.CreateCampaign(customerId, campaignName);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("RemoveCampaign")]
        public IActionResult RemoveCampaign(long customerId, long campaignId)
        {
            var result = _campaignService.RemoveCampaign(customerId, campaignId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("CampaignReportToCsv")]
        public IActionResult CampaignReportToCsv(long customerId, string outputFilePath)
        {
            var result = _campaignService.CampaignReportToCsv(customerId, outputFilePath);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
    }
}
