using Digital.Domain.Results;
using Google.Ads.GoogleAds.Lib;

namespace GoogleAdsAPI.ServicesAPI.AdService
{
    public interface IAdService
    {
        IResultBase CreateResponsiveSearchAd(long customerId, long adGroupId, string headLine);
        void Run(long customerId, long adGroupId);
    }
}
