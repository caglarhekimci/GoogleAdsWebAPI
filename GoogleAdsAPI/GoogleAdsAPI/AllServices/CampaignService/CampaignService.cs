using Digital.Domain.Results;
using Google.Ads.Gax.Examples;
using Google.Ads.GoogleAds.Lib;
using Google.Api.Gax;
using Google.Protobuf.Collections;
using GoogleAdsAPI.Utilities.Helpers;
using static Google.Ads.GoogleAds.V12.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.CampaignStatusEnum.Types;
using static Google.Ads.GoogleAds.V12.Resources.Campaign.Types;
using static Google.Ads.GoogleAds.V12.Enums.BudgetDeliveryMethodEnum.Types;
using Google.Ads.GoogleAds.V12.Services;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Common;
using Google.Ads.GoogleAds.V12.Errors;
using System.Reflection;
using CsvHelper;
using System.Globalization;
using static Google.Rpc.Context.AttributeContext.Types;

namespace GoogleAdsAPI.ServicesAPI.CampaignService
{
    public class CampaignService : ICampaignService
    {
        private readonly GoogleAdsClient _client; 
       

        public CampaignService(GoogleAdsClient client)
        {
            _client = client;
        }
        public IDataResult<string> CampaignReportToCsv(long customerId, string outputFilePath)
        {


            GoogleAdsServiceClient googleAdsServiceClient =
               _client.GetService(Services.V12.GoogleAdsService);
            string query = @"
                SELECT
                    campaign.id,
                    campaign.name,
                    metrics.impressions,
                    metrics.clicks,
                    metrics.cost_micros
                FROM campaign";

            string result = "";


            try
            {
                googleAdsServiceClient.SearchStream(customerId.ToString(), query,
              delegate (SearchGoogleAdsStreamResponse response)
              {
                  if (response.Results.Count() == 0)
                  {
                      result += "No results found!";
                  }
                  var responseField = response.FieldMask;
                  CsvFile csvFile = new CsvFile();

                  // Set the header for the CSV file.
                  csvFile.Headers.AddRange(response.FieldMask.Paths);

                  // Iterate over all returned rows and extract the information.
                  foreach (GoogleAdsRow googleAdsRow in response.Results)
                  {
                      csvFile.Records.Add(new string[]
                      {
                            googleAdsRow.Campaign.Id.ToString(),
                            googleAdsRow.Campaign.Name,
                            googleAdsRow.Metrics.Impressions.ToString(),
                            googleAdsRow.Metrics.Clicks.ToString(),
                            googleAdsRow.Metrics.CostMicros.ToString()
                      });
                  }

                  if (outputFilePath == null)
                  {
                      outputFilePath =
                          Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                          Path.DirectorySeparatorChar +
                          GetType().Name +
                          DateTime.Now.ToString("-yyyyMMMMdd-HHmmss") + ".csv";
                  }
                  else if (!outputFilePath.EndsWith(".csv"))
                  {
                      outputFilePath += ".csv";
                  }

                  // Create the file with the specified path, write all lines, and close it.
                  csvFile.Write(outputFilePath);
                  result += $"Successfully wrote {response.Results.Count()} entries to {outputFilePath}.";

              });
                return new SuccessDataResult<string>(result);
            }
            catch (GoogleAdsException ex)
            {

                return new ErrorDataResult<string>(ex.Message);
            }


        }

        public IDataResult<string> CreateCampaign(long customerId, string campaignName)
        {


            // Get the CampaignService.
            CampaignServiceClient campaignService = _client.GetService(Services.V12.CampaignService);
            string budget = CreateBudget(customerId);
            List<CampaignOperation> operations = new List<CampaignOperation>();

            for (int i = 0; i < NUM_CAMPAIGNS_TO_CREATE; i++)
            {
                Campaign campaign = new Campaign()
                {
                    Name = campaignName/* + ExampleUtilities.GetRandomString()*/,
                    AdvertisingChannelType = AdvertisingChannelType.Search,

                    // Recommendation: Set the campaign to PAUSED when creating it to prevent
                    // the ads from immediately serving. Set to ENABLED once you've added
                    // targeting and the ads are ready to serve
                    Status = CampaignStatus.Paused,

                    // Set the bidding strategy and budget.
                    ManualCpc = new ManualCpc(),
                    CampaignBudget = budget,

                    // Set the campaign network options.
                    NetworkSettings = new NetworkSettings
                    {
                        TargetGoogleSearch = true,
                        TargetSearchNetwork = true,
                        TargetContentNetwork = true,
                        TargetPartnerSearchNetwork = false
                    },

                    // Optional: Set the start date.
                    StartDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd"),

                    // Optional: Set the end date.
                    EndDate = DateTime.Now.AddYears(1).ToString("yyyyMMdd"),
                };


                // Create the operation.
                operations.Add(new CampaignOperation() { Create = campaign });
            }
            string response = "";
            try
            {
                // Add the campaigns.
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), operations);

                // Display the results.
                if (retVal.Results.Count > 0)
                {
                    foreach (MutateCampaignResult newCampaign in retVal.Results)
                    {
                        response += $"Campaign with resource ID = '{newCampaign.ResourceName}' was added.";
                    }
                    return new SuccessDataResult<string>(response);
                }
                else
                {
                    response += "No campaigns were added.";
                    return new ErrorDataResult<string>(response);
                }

            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<string>(e.Message);
            }
        }

        public IDataResult<List<Campaign>> GenericCampaign(string customerId, string searchCriteria)
        {


            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = _client.GetService(
                Services.V12.GoogleAdsService);

            SearchGoogleAdsRequest request = new SearchGoogleAdsRequest()
            {
                CustomerId = customerId,
                Query = searchCriteria,
                PageSize = 10
            };
            try
            {
                PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse = googleAdsService.Search(request);
                List<Campaign> campaigns = new List<Campaign>();
                foreach (GoogleAdsRow row in searchPagedResponse)
                {
                    Campaign campaign = row.Campaign;

                    campaigns.Add(campaign);                   
                }
              

                return new SuccessDataResult<List<Campaign>>(campaigns);
            }
            catch (GoogleAdsException ex)
            {

                return new ErrorDataResult<List<Campaign>>(ex.Message);
            }


        }

        public IDataResult<RepeatedField<GoogleAdsRow>> GetCampaign(long customerId)
        {


            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = _client.GetService( Services.V12.GoogleAdsService);


            string query = @"SELECT 
                campaign.id, 
                campaign.name, 
                campaign_budget.amount_micros, 
                campaign.status, 
                campaign.serving_status, 
                campaign.start_date, 
                campaign.advertising_channel_sub_type, 
                metrics.average_cpc, 
                metrics.average_cpm, 
                metrics.clicks, 
                metrics.interactions, 
                metrics.interaction_rate, 
                metrics.impressions, 
                metrics.ctr, 
                metrics.all_conversions, 
                metrics.all_conversions_value, 
                metrics.cost_micros, 
                metrics.cost_per_all_conversions
            FROM campaign ";



            try
            {
                RepeatedField<GoogleAdsRow> result = RequestMethods.SearchRequest(customerId.ToString(), query, googleAdsService);


                return new SuccessDataResult<RepeatedField<GoogleAdsRow>>(result);
            }
            catch (Exception ex)
            {

                return new ErrorDataResult<RepeatedField<GoogleAdsRow>>(ex.Message);
            }


        }

        public IDataResult<List<string>> GetSummurizeCampaign(long customerId)
        {

            GoogleAdsServiceClient googleAdsService = _client.GetService(
               Services.V12.GoogleAdsService);            
            string query =
                @"SELECT
                      campaign.id,
                      campaign.name,
                      campaign.status,
                      campaign.advertising_channel_type,
                      campaign.advertising_channel_sub_type,
                      campaign.start_date,
                      campaign.end_date,   
                      campaign.serving_status,
                      campaign.network_settings.target_google_search,
                      campaign.network_settings.target_search_network,
                      campaign.network_settings.target_content_network,
                      campaign.network_settings.target_partner_search_network
                    FROM
                      campaign";
            List<string> rows = new List<string>();
            try
            {
                googleAdsService.SearchStream(customerId.ToString(), query,
                  delegate (SearchGoogleAdsStreamResponse resp)
                  {

                      foreach (GoogleAdsRow row in resp.Results)
                      {
                          string rowString = @"Campaign with ID " +
                                          $"{row.Campaign.Id}, name " +
                                          $"'{row.Campaign.Name}', status " +
                                          $"'{row.Campaign.Status}', advertising channel type " +
                                          $"'{row.Campaign.AdvertisingChannelType}', advertising channel subtype " +
                                          $"'{row.Campaign.AdvertisingChannelSubType}', start date " +
                                          $"'{row.Campaign.StartDate}', end date " +
                                          $"'{row.Campaign.EndDate}', serving status " +
                                          $"'{row.Campaign.ServingStatus}', target Google search " +
                                          $"'{row.Campaign.NetworkSettings.TargetGoogleSearch}', target search network " +
                                          $"'{row.Campaign.NetworkSettings.TargetSearchNetwork}', target content network " +
                                          $"'{row.Campaign.NetworkSettings.TargetContentNetwork}', target partner search network " +
                                          $"'{row.Campaign.NetworkSettings.TargetPartnerSearchNetwork}'.";
                          rows.Add(rowString);
                      }
                  });
                return new SuccessDataResult<List<string>>(rows);
            }
            catch (GoogleAdsException ex)
            {

                return new ErrorDataResult<List<string>>(ex.Message);
            }


        }

        public IResultBase RemoveCampaign(long customerId, long campaignId)
        {


            CampaignServiceClient campaignService = _client.GetService(Services.V12.CampaignService);

            // Create the operation, and set the Remove field to the resource name of the
            // campaign to be removed.
            CampaignOperation operation = new CampaignOperation()
            {
                Remove = ResourceNames.Campaign(customerId, campaignId)
            };
            try
            {
                // Remove the campaign.
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), new CampaignOperation[] { operation });
                string result = "";
                // Display the results.
                foreach (MutateCampaignResult removedCampaign in retVal.Results)
                {
                    result += $"Campaign with resource name = '{removedCampaign.ResourceName}' was removed.";
                }
                return new SuccessResult(result);
            }
            catch (GoogleAdsException e)
            {
                return new ErrorResult(e.Message);
            }
        }
        public string CreateBudget(long customerId)
        {
            // Get the BudgetService.
            CampaignBudgetServiceClient budgetService = _client.GetService(
                Services.V12.CampaignBudgetService);

            // Create the campaign budget.
            CampaignBudget budget = new CampaignBudget()
            {
                Name = "Interplanetary Cruise Budget #" + ExampleUtilities.GetRandomString(),
                DeliveryMethod = BudgetDeliveryMethod.Standard,
                AmountMicros = 500000
            };

            // Create the operation.
            CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
            {
                Create = budget
            };


            MutateCampaignBudgetsResponse response = budgetService.MutateCampaignBudgets(
            customerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
            return response.Results[0].ResourceName;




        }

        private const int NUM_CAMPAIGNS_TO_CREATE = 1;
    }
}
