using Google.Ads.Gax.Lib;
using Google.Ads.GoogleAds.V12.Services;
using Google.Protobuf.Collections;

namespace GoogleAdsAPI.Utilities.Helpers
{
    public static class RequestMethods
    {
        public static RepeatedField<GoogleAdsRow> SearchRequest(string customerID, string query, GoogleAdsServiceClient googleAdsService)
        {
            RepeatedField<GoogleAdsRow> results = new RepeatedField<GoogleAdsRow>();

            // Issue a search request.
            googleAdsService.SearchStream(customerID, query,
                delegate (SearchGoogleAdsStreamResponse resp)
                {
                    for (int i = 0; i < resp.Results.Count; i++)
                    {
                        if (resp.Results[i] != null)
                        {
                            
                            results.Add(resp.Results[i]);
                        }
                    }
                }
            );

            return results;
        }
    }

}