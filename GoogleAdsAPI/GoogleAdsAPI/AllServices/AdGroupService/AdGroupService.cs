using Digital.Domain.Results;
using Google.Ads.Gax.Util;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V12.Common;
using Google.Ads.GoogleAds.V12.Enums;
using Google.Ads.GoogleAds.V12.Errors;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using Google.Api.Gax;
using Google.Protobuf.WellKnownTypes;
using GoogleAdsAPI.Utilities.Helpers;

namespace GoogleAdsAPI.ServicesAPI.AdGroupService
{
    public class AdGroupService : IAdGroupService
    {
        private readonly GoogleAdsClient _client;
        private const int NUM_ADGROUPS_TO_CREATE = 1;

        public AdGroupService(GoogleAdsClient client)
        {
            _client = client;
        }

        public IResultBase CreateAdGroup(long customerId, long campaignId, string adGroupName)
        {

            AdGroupServiceClient adGroupService = _client.GetService(Services.V12.AdGroupService);

            List<AdGroupOperation> operations = new List<AdGroupOperation>();
            for (int i = 0; i < NUM_ADGROUPS_TO_CREATE; i++)
            {
                // Create the ad group.
                AdGroup adGroup = new AdGroup()
                {
                    Name = adGroupName,
                    Status = AdGroupStatusEnum.Types.AdGroupStatus.Enabled,
                    Campaign = ResourceNames.Campaign(customerId, campaignId),

                    // Set the ad group bids.
                    CpcBidMicros = 10000000
                };

                // Create the operation.
                AdGroupOperation operation = new AdGroupOperation()
                {
                    Create = adGroup
                };
                operations.Add(operation);
            }
            try
            {
                // Create the ad groups.
                MutateAdGroupsResponse response = adGroupService.MutateAdGroups(
                    customerId.ToString(), operations);
                string result = "";
                // Display the results.
                foreach (MutateAdGroupResult newAdGroup in response.Results)
                {
                    result += $"Ad group with resource name '{newAdGroup.ResourceName}' was created.";
                }
                return new SuccessResult(result);
            }
            catch (GoogleAdsException e)
            {
                return new ErrorResult(e.Message);
            }
        }

        public IDataResult<List<AdGroup>> GenericAdGroup(string customerId, string searchCriteria, long? campaignId)
        {


            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = _client.GetService(
                Google.Ads.GoogleAds.Services.V12.GoogleAdsService);

            SearchGoogleAdsRequest request = new SearchGoogleAdsRequest()
            {
                CustomerId = customerId,
                Query = searchCriteria,
                PageSize = 10
            };
            if (campaignId != null)
            {
                request.Query += $" WHERE campaign.id = {campaignId}";
            }
            try
            {
                PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse = googleAdsService.Search(request);
                List<AdGroup> adGroups = new List<AdGroup>();

                foreach (GoogleAdsRow row in searchPagedResponse)
                {
                    AdGroup adGroup = row.AdGroup;

                    adGroups.Add(adGroup);
                }
                return new SuccessDataResult<List<AdGroup>>(adGroups);
            }
            catch (GoogleAdsException ex)
            {

                return new ErrorDataResult<List<AdGroup>>(ex.Message);
            }



        }
         
        public IDataResult<List<string>> GetAdGroup(long customerId)
        {


            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = _client.GetService(
                Google.Ads.GoogleAds.Services.V12.GoogleAdsService);

            string searchQuery = "SELECT campaign.id, ad_group.id, ad_group.name,ad_group.status FROM ad_group";


            // Create a request that will retrieve all ads using pages of the specified page size.
            PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse = googleAdsService.Search(customerId.ToString(), searchQuery);
            List<string> stringValue = new List<string>();
            try
            {
                foreach (GoogleAdsRow googleAdsRow in searchPagedResponse)
                {
                    AdGroup adGroup = googleAdsRow.AdGroup;
                    if (adGroup != null)
                    {
                        string value = $"Ad group with ID {adGroup.Id} and name '{adGroup.Name}' was found. Campaign Id:{googleAdsRow.Campaign.Id} Status:{adGroup.Status} ";
                        stringValue.Add(value);
                    }
                }
                return new SuccessDataResult<List<string>>(stringValue);
            }
            catch (GoogleAdsException ex)
            {

                return new ErrorDataResult<List<string>>(ex.Message);
            }

        }
         
        public Dictionary<string, object> GetDynamicAdGroup(long? customerId,long? campaignId)
        {
           

            GoogleAdsServiceClient googleAdsService = _client.GetService(Services.V12.GoogleAdsService);
            AdGroup ad = new AdGroup();
            Campaign campaign = new Campaign();
            var searchQuery = Builder.BuildQuery(campaign, "campaign",campaignId).ToLower();
            Metrics metricsResult = new Metrics();

            System.Type campaignResourceType = typeof(Campaign);
            string fieldToCheck = "budget";
            FieldMask fieldMask = FieldMask.FromString(fieldToCheck);


            // Create a request that will retrieve all ads using pages of the specified page size.
            PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse = googleAdsService.Search(customerId.ToString(), searchQuery);

            //Create an empty dictionary
            Dictionary<string, object> adGroupData = new Dictionary<string, object>();

            foreach (GoogleAdsRow googleAdsRow in searchPagedResponse)
            {
                // Get the properties of the GoogleAdsRow class
                var properties = googleAdsRow.GetType().GetProperties().Where(p => p.Name == "Campaign");
                var metrics = googleAdsRow.GetType().GetProperties().Where(p => p.Name == "Metrics");
                // Iterate through the properties and add them to the dictionary
                foreach (var property in properties)
                {
                    var value = property.GetValue(googleAdsRow);
                    if (value != null)
                    {
                        campaign = value as Campaign;
                        adGroupData.Add(campaign.Name, campaign);
                    }

                }
                foreach (var metric in metrics)
                {
                    var value = metric.GetValue(googleAdsRow);
                    if (value != null)
                    {
                        metricsResult = value as Metrics;
                        adGroupData.Add(campaign.Name+"_Metrics", metricsResult);
                    }
                }
            }


            // Return the dictionary
            return adGroupData;
        }
    }
}
