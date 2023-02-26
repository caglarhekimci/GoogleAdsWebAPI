using Digital.Domain.Results;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using Google.Protobuf.Collections;

namespace GoogleAdsAPI.ServicesAPI.CampaignService
{
    public interface ICampaignService
    { 
        IDataResult<RepeatedField<GoogleAdsRow>> GetCampaign(long customerId); 
        IDataResult<List<string>> GetSummurizeCampaign(long customerId);
        IDataResult<List<Campaign>> GenericCampaign(string customerId, string searchCriteria);
        IDataResult<string> CreateCampaign(long customerId, string campaignName);
        IResultBase RemoveCampaign(long customerId, long campaignId);
        IDataResult<string> CampaignReportToCsv(long customerId, string outputFilePath);
    }
}
