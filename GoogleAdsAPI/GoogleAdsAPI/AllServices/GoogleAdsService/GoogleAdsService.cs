using Digital.Domain.Results;
using Google.Ads.Gax.Examples;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V12.Common;
using Google.Ads.GoogleAds.V12.Enums;
using Google.Ads.GoogleAds.V12.Errors;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using Google.Api.Gax;
using Google.Protobuf.Collections;
using GoogleAdsAPI.Utilities.Helpers;
using System.Reflection;
using static Google.Ads.GoogleAds.V12.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.BudgetDeliveryMethodEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.CampaignStatusEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.ServedAssetFieldTypeEnum.Types;
using static Google.Ads.GoogleAds.V12.Resources.Campaign.Types;


namespace GoogleAdsAPI.ServicesAPI
{
    public class GoogleAdsService : IGoogleAdsService
    {

        private readonly GoogleAdsClient _client;

        public GoogleAdsService(GoogleAdsClient client)
        {
            _client = client;
        } 
        public List<T> GetResults<T>(long? customerId, string resource)
        {


            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = _client.GetService(Services.V12.GoogleAdsService);
            System.Type resourceType = typeof(T);

            // Create a query that will retrieve the specified resource.
            string query = $"SELECT campaign.id,campaign.name FROM {resource.ToLower()}";
            var results = new List<T>();
            
           
                // Issue a search request.
                googleAdsService.SearchStream(customerId.ToString(), query, 
                    delegate (SearchGoogleAdsStreamResponse response)
                    {
                      
                        foreach (GoogleAdsRow googleAdsRow in response.Results)
                        {
                            var obj = Activator.CreateInstance(resourceType);
                            foreach (PropertyInfo prop in resourceType.GetProperties())
                            {
                                var googleAdsRowProp = googleAdsRow.GetType().GetProperty(resource);
                                if (googleAdsRowProp != null && prop.PropertyType == googleAdsRowProp.PropertyType)
                                {
                                    prop.SetValue(obj, googleAdsRowProp.GetValue(googleAdsRow, null));
                                }
                            }
                            results.Add((T)obj);
                        }
                    });
               

                // Use reflection to populate the results list.
                
                return results;
            
            

        }         
        
    }


}


