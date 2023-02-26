using Digital.Domain.Results;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using Google.Protobuf.Collections;
using GoogleAdsAPI.Models;

namespace GoogleAdsAPI.ServicesAPI
{
    public interface IGoogleAdsService
    { 
        List<T> GetResults<T>(long? customerId, string resource);  

    }
}
