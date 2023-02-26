using Digital.Domain.Extensions;
using Google.Ads.GoogleAds.V12.Resources;
using GoogleAdsAPI.ServicesAPI.KeywordService;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KeywordController : ControllerBase
    {      
        private readonly IKeywordService _keywordService;

        public KeywordController( IKeywordService keywordService)
        {            
            _keywordService = keywordService;
        }
                
        [HttpPost("Adword")]
        public IActionResult GetAdWords(string[] adWord)
        {
            var result = _keywordService.SearchWord(adWord);

            return Ok(result);
        }
        [HttpPost("AddKeyword")]
        public IActionResult AddKeyword(long customerId, long adGroupId, string keywordText)
        {
            var result = _keywordService.AddKeyword(customerId, adGroupId, keywordText);

            if (result.Succes)            
                return Ok(result);

            return BadRequest(result.Message);  
            
            
        }
    }
}
