using Digital.Domain.Results;
using Google.Ads.GoogleAds.V12.Resources;

namespace GoogleAdsAPI.ServicesAPI.AdGroupService
{
    public interface IAdGroupService
    {

        IDataResult<List<string>> GetAdGroup(long customerId); 
        IDataResult<List<AdGroup>> GenericAdGroup(string customerId, string searchCriteria, long? campaignId); 
        IResultBase CreateAdGroup(long customerId, long campaignId, string adGroupName);
        Dictionary<string, object> GetDynamicAdGroup(long? customerId,long? campaignId);
    }
}
