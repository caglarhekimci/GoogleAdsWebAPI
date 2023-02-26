using Digital.Domain.Results;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V12.Common;
using Google.Ads.GoogleAds.V12.Enums;
using Google.Ads.GoogleAds.V12.Errors;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using GoogleAdsAPI.Models;
using Newtonsoft.Json;
using static Google.Ads.GoogleAds.V12.Enums.AdGroupCriterionStatusEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.KeywordMatchTypeEnum.Types;

namespace GoogleAdsAPI.ServicesAPI.KeywordService
{
    public class KeywordService : IKeywordService
    {
        
        private readonly GoogleAdsClient _client;

        public KeywordService(GoogleAdsClient googleAdsClient)
        {
           
            _client = googleAdsClient;
        }

        public IDataResult<string> AddKeyword(long customerId, long adGroupId, string keywordText)
        {
            if (string.IsNullOrEmpty(keywordText))
            {
                keywordText = KEYWORD_TEXT;
            }
            AdGroupCriterionServiceClient adGroupCriterionService =
               _client.GetService(Services.V12.AdGroupCriterionService);  

            // Create a keyword.
            AdGroupCriterion criterion = new AdGroupCriterion()
            {
                AdGroup = ResourceNames.AdGroup(customerId, adGroupId),
                Status = AdGroupCriterionStatus.Enabled,
                Keyword = new KeywordInfo()
                {
                    Text = keywordText,
                    MatchType = KeywordMatchType.Exact
                }
            };

            // Create the operation.
            AdGroupCriterionOperation operation = new AdGroupCriterionOperation()
            {
                Create = criterion,
            };

            try
            {
                // Add the keywords.
                MutateAdGroupCriteriaResponse retVal =
                    adGroupCriterionService.MutateAdGroupCriteria(customerId.ToString(),
                        new AdGroupCriterionOperation[] { operation });
                string result = "";
                // Display the results.
                if (retVal.Results.Count > 0)
                {

                    foreach (MutateAdGroupCriterionResult newCriterion in retVal.Results)
                    {
                         result +=$"Created keyword with resource ID = '{newCriterion.ResourceName}'.";
                    }
                    return new SuccessDataResult<string>(result);
                }
                else
                {
                   result +="No keywords were added.";
                    return new SuccessDataResult<string>(result);
                }
            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<string>(e.Message);
            }
        }

        public List<string> SearchWord(string[] keywordTexts)
        {
            List<string> keyResults = new List<string>(); 
            var keywordPlanIdeaService = _client.GetService(Services.V12.KeywordPlanIdeaService);

            if (keywordTexts.Length == 0)
            {
                throw new ArgumentException("At least one keyword is required!");
            }

            var request = new GenerateKeywordIdeasRequest
            {
                CustomerId = _client.Config.LoginCustomerId,
                KeywordSeed = new KeywordSeed()
            };

            request.KeywordSeed.Keywords.AddRange(keywordTexts);


            //Set Locations
            request.GeoTargetConstants.Add(ResourceNames.GeoTargetConstant(21167));

            //Set Language
            request.Language = ResourceNames.LanguageConstant(1000);

            //Set Network
            //Google or GoogleWithPartners
            request.KeywordPlanNetwork = KeywordPlanNetworkEnum.Types.KeywordPlanNetwork.GoogleSearch;

            try
            {
                // Generate keyword ideas based on the specified parameters.
                var response =
                    keywordPlanIdeaService.GenerateKeywordIdeas(request);

                foreach (var result in response)
                {
                    var metrics = result.KeywordIdeaMetrics;

                    if (metrics != null)
                    {

                        var lowBid = Convert.ToDecimal(metrics.LowTopOfPageBidMicros) / 1000000;
                        var highBid = Convert.ToDecimal(metrics.LowTopOfPageBidMicros) / 1000000;


                        string json = JsonConvert.SerializeObject(new
                        {
                            results = new List<KeywordResult>()
                            {
                                new KeywordResult { Keyword=result.Text, SearchVolume=metrics.AvgMonthlySearches,Competition=metrics.CompetitionIndex,LowCPCBid=lowBid,HighCPCBid=highBid },
                              }
                        });


                        keyResults.Add(json);

                    }
                    else
                    {

                    }

                }
                return keyResults;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure!");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }

        private const string KEYWORD_TEXT = "mars cruise";
    }
}
