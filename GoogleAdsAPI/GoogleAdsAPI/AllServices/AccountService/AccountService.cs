using Digital.Domain.Results;
using Google.Ads.Gax.Util;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V12.Errors;
using Google.Ads.GoogleAds.V12.Resources;
using Google.Ads.GoogleAds.V12.Services;
using Google.Api.Gax;
using Google.Protobuf;
using Google.Ads.GoogleAds;
using static Google.Ads.GoogleAds.V12.Enums.ChangeEventResourceTypeEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.ChangeStatusResourceTypeEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.MerchantCenterLinkStatusEnum.Types;
using static Google.Ads.GoogleAds.V12.Enums.ResourceChangeOperationEnum.Types;
using static Google.Ads.GoogleAds.V12.Resources.ChangeEvent.Types;
using System.Net.Mail;
using static Google.Ads.GoogleAds.V12.Enums.AccessRoleEnum.Types;

namespace GoogleAdsAPI.ServicesAPI.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly GoogleAdsClient _client;

        public AccountService(GoogleAdsClient client)
        {
            _client = client;
        }
        public IDataResult<List<CustomerClient>> GetAccountHierarchy(long? managerCustomerId = null, long? loginCustomerId = null)
        {

            if (loginCustomerId.HasValue)
            {
                _client.Config.LoginCustomerId = loginCustomerId.Value.ToString();
            }

            GoogleAdsServiceClient googleAdsServiceClient = _client.GetService(Services.V12.GoogleAdsService);


            CustomerServiceClient customerServiceClient =
                _client.GetService(Services.V12.CustomerService);

            // List of Customer IDs to handle.
            List<long> seedCustomerIds = new List<long>();
            if (managerCustomerId.HasValue)
            {
                seedCustomerIds.Add(managerCustomerId.Value);
            }
            else
            {
                string[] customerResourceNames = customerServiceClient.ListAccessibleCustomers();

                foreach (string customerResourceName in customerResourceNames)
                {
                    CustomerName customerName = CustomerName.Parse(customerResourceName);
                    seedCustomerIds.Add(long.Parse(customerName.CustomerId));
                }
            }
            const string query = @"SELECT
                                    customer_client.client_customer,
                                    customer_client.level,
                                    customer_client.manager,
                                    customer_client.descriptive_name,
                                    customer_client.currency_code,
                                    customer_client.time_zone,
                                    customer_client.id
                                FROM customer_client
                                WHERE
                                    customer_client.level <= 1";
            List<CustomerClient> resultList = new List<CustomerClient>();
            Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts =
               new Dictionary<long, List<CustomerClient>>();
            string result = "";
            foreach (long seedCustomerId in seedCustomerIds)
            {
                Queue<long> unprocessedCustomerIds = new Queue<long>();
                unprocessedCustomerIds.Enqueue(seedCustomerId);
                CustomerClient rootCustomerClient = null;

                while (unprocessedCustomerIds.Count > 0)
                {
                    managerCustomerId = unprocessedCustomerIds.Dequeue();
                    PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response =
                        googleAdsServiceClient.Search(
                            managerCustomerId.ToString(),
                            query,
                            pageSize: 1000
                        );

                    // Iterate over all rows in all pages to get all customer clients under the
                    // specified customer's hierarchy.
                    foreach (GoogleAdsRow googleAdsRow in response)
                    {
                        CustomerClient customerClient = googleAdsRow.CustomerClient;

                        // The customer client that with level 0 is the specified customer.
                        if (customerClient.Level == 0)
                        {
                            if (rootCustomerClient == null)
                            {
                                rootCustomerClient = customerClient;
                            }

                            continue;
                        }

                        // For all level-1 (direct child) accounts that are a manager account,
                        // the above query will be run against them to create a Dictionary of
                        // managers mapped to their child accounts for printing the hierarchy
                        // afterwards.
                        if (!customerIdsToChildAccounts.ContainsKey(managerCustomerId.Value))
                            customerIdsToChildAccounts.Add(managerCustomerId.Value,
                                new List<CustomerClient>());

                        customerIdsToChildAccounts[managerCustomerId.Value].Add(customerClient);

                        if (customerClient.Manager)
                            // A customer can be managed by multiple managers, so to prevent
                            // visiting the same customer many times, we need to check if it's
                            // already in the Dictionary.
                            if (!customerIdsToChildAccounts.ContainsKey(customerClient.Id) &&
                                customerClient.Level == 1)
                                unprocessedCustomerIds.Enqueue(customerClient.Id);
                    }
                }

                if (rootCustomerClient != null)
                {
                    result += $"The hierarchy of customer ID {rootCustomerClient.Id} is printed below:";
                    resultList = PrintAccountHierarchy(rootCustomerClient, customerIdsToChildAccounts, 0);
                    //result += resultPrint;
                    return new SuccessDataResult<List<CustomerClient>>(resultList);
                }
                else
                {
                    return new ErrorDataResult<List<CustomerClient>>($"Customer ID {managerCustomerId} is likely a test account, so its customer client " +
                        " information cannot be retrieved.");
                }
            }
            return new ErrorDataResult<List<CustomerClient>>($"Customer ID {managerCustomerId} is likely a test account, so its customer client " +
                        " information cannot be retrieved.");
        }

        public IDataResult<List<string>> ListAccessibleCustomers()
        {


            CustomerServiceClient customerService = _client.GetService(Services.V12.CustomerService);

            List<string> customers = new List<string>();
            try
            {
                // Retrieve the list of customer resources.
                var customerResourceNames = customerService.ListAccessibleCustomers();

                // Display the result.
                foreach (var customerResourceName in customerResourceNames)
                {
                    customers.Add($"Found customer with resource name = '{customerResourceName}'.");
                }
                return new SuccessDataResult<List<string>>(customers);
            }
            catch (GoogleAdsException e)
            {

                return new ErrorDataResult<List<string>>(e.Message);
            }
        }

        public IDataResult<string> MerchantCenterLink(long customerId, long merchantCenterAccountId)
        {
            // Get the MerchantCenterLink.
            MerchantCenterLinkServiceClient merchantCenterLinkService = _client.GetService(Google.Ads.GoogleAds.Services.V12.MerchantCenterLinkService);
            string result = "";

            try
            {
                // Retrieve all the existing Merchant Center links.
                ListMerchantCenterLinksResponse response =
                    merchantCenterLinkService.ListMerchantCenterLinks(customerId.ToString());

                result += $"{response.MerchantCenterLinks.Count} Merchant Center link(s)" +
                    $" found with the following details:";

                // Iterate the results, and filter for links with pending status.
                foreach (MerchantCenterLink merchantCenterLink in response.MerchantCenterLinks)
                {
                    result += $"Link '{merchantCenterLink.ResourceName}' has status " +
                         $"'{merchantCenterLink.Status}'.";

                    // Checks if there is a link for the Merchant Center account we are looking
                    // for, then only approves the link if it is in a 'PENDING' state.
                    if (merchantCenterLink.Status == MerchantCenterLinkStatus.Pending &&
                        merchantCenterLink.Id == merchantCenterAccountId)
                    {
                        UpdateMerchantCenterLinkStatus(customerId, merchantCenterLinkService,
                            merchantCenterLink, MerchantCenterLinkStatus.Enabled);
                    }
                }
                return new SuccessDataResult<string>(result);
            }
            catch (GoogleAdsException e)
            {

                return new ErrorDataResult<string>(e.Message);
            }

        }

        public IDataResult<Customer> CreateCustomer(long managerCustomerId)
        {
            CustomerServiceClient customerService = _client.GetService(Google.Ads.GoogleAds.Services.V12.CustomerService);

            Customer customer = new Customer()
            {
                DescriptiveName = $"Account created with CustomerService on '{DateTime.Now}'",

                // For a list of valid currency codes and time zones see this documentation:
                // https://developers.google.com/google-ads/api/reference/data/codes-formats#codes_formats.
                CurrencyCode = "USD",
                TimeZone = "America/New_York",

                // The below values are optional. For more information about URL
                // options see: https://support.google.com/google-ads/answer/6305348.
                TrackingUrlTemplate = "{lpurl}?device={device}",
                FinalUrlSuffix = "keyword={keyword}&matchtype={matchtype}&adgroupid={adgroupid}"
            };

            try
            {
                // Create the account.
                CreateCustomerClientResponse response = customerService.CreateCustomerClient(
                    managerCustomerId.ToString(), customer);

                // Display the result.
                string result = $"Created a customer with resource name " +
                   $"'{response.ResourceName}' under the manager account with customer " +
                   $"ID '{managerCustomerId}'";
                return new SuccessDataResult<Customer>(customer, result);
            }
            catch (GoogleAdsException e)
            {

                return new ErrorDataResult<Customer>(e.Message);
            }
        }

        public IDataResult<Customer> GetAccountInformation(long customerId)
        {
            GoogleAdsServiceClient googleAdsService = _client.GetService(
              Google.Ads.GoogleAds.Services.V12.GoogleAdsService);

            string query = @"SELECT customer.id, customer.descriptive_name,  
                customer.currency_code, customer.time_zone, customer.tracking_url_template,  
                customer.auto_tagging_enabled FROM customer LIMIT 1";

            SearchGoogleAdsRequest request = new SearchGoogleAdsRequest()
            {
                CustomerId = customerId.ToString(),
                Query = query
            };

            try
            {
                // Issue the search request.
                Customer customer = googleAdsService.Search(request).First().Customer;

                // Print account information.
                string result = $@"Customer with ID {customer.Id}, descriptive name '{customer.DescriptiveName}', currency 
                    code '{customer.CurrencyCode}', timezone '{customer.TimeZone}', tracking URL template '{customer.TrackingUrlTemplate}' and auto tagging " +
                     "enabled '{ customer.AutoTaggingEnabled}' was retrieved.";

                return new SuccessDataResult<Customer>(customer, result);
            }
            catch (GoogleAdsException e)
            {

                return new ErrorDataResult<Customer>(e.Message);
            }
        }

        public IDataResult<ChangeEvent> GetChangeDetail(long customerId)
        {
            GoogleAdsServiceClient googleAdsService = _client.GetService(
                Google.Ads.GoogleAds.Services.V12.GoogleAdsService);

            string startDate = DateTime.Today.Subtract(TimeSpan.FromDays(25)).ToString("yyyyMMdd");
            string endDate = DateTime.Today.Add(TimeSpan.FromDays(1)).ToString("yyyyMMdd");
            string searchQuery = $@"
                SELECT
                    change_event.resource_name,
                    change_event.change_date_time,
                    change_event.change_resource_name,
                    change_event.user_email,
                    change_event.client_type,
                    change_event.change_resource_type,
                    change_event.old_resource,
                    change_event.new_resource,
                    change_event.resource_change_operation,
                    change_event.changed_fields
                FROM
                    change_event
                WHERE
                    change_event.change_date_time >= '{startDate}' AND
                    change_event.change_date_time <= '{endDate}'
                ORDER BY
                    change_event.change_date_time DESC
                LIMIT 5";

            string result = "";
            ChangeEvent changeEvent;
            try
            {
                // Issue a search request.
                googleAdsService.SearchStream(customerId.ToString(), searchQuery,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {

                        // Display the results.
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            changeEvent = googleAdsRow.ChangeEvent;
                            ChangedResource oldResource = changeEvent.OldResource;
                            ChangedResource newResource = changeEvent.NewResource;

                            bool knownResourceType = true;
                            IMessage oldResourceEntity = null;
                            IMessage newResourceEntity = null;

                            switch (changeEvent.ChangeResourceType)
                            {
                                case ChangeEventResourceType.Ad:
                                    oldResourceEntity = oldResource.Ad;
                                    newResourceEntity = newResource.Ad;
                                    break;

                                case ChangeEventResourceType.AdGroup:
                                    oldResourceEntity = oldResource.AdGroup;
                                    newResourceEntity = newResource.AdGroup;
                                    break;

                                case ChangeEventResourceType.AdGroupAd:
                                    oldResourceEntity = oldResource.AdGroupAd;
                                    newResourceEntity = newResource.AdGroupAd;
                                    break;

                                case ChangeEventResourceType.AdGroupAsset:
                                    oldResourceEntity = oldResource.AdGroupAsset;
                                    newResourceEntity = newResource.AdGroupAsset;
                                    break;

                                case ChangeEventResourceType.AdGroupBidModifier:
                                    oldResourceEntity = oldResource.AdGroupBidModifier;
                                    newResourceEntity = newResource.AdGroupBidModifier;
                                    break;

                                case ChangeEventResourceType.AdGroupCriterion:
                                    oldResourceEntity = oldResource.AdGroupCriterion;
                                    newResourceEntity = newResource.AdGroupCriterion;
                                    break;

                                case ChangeEventResourceType.AdGroupFeed:
                                    oldResourceEntity = oldResource.AdGroupFeed;
                                    newResourceEntity = newResource.AdGroupFeed;
                                    break;

                                case ChangeEventResourceType.Asset:
                                    oldResourceEntity = oldResource.Asset;
                                    newResourceEntity = newResource.Asset;
                                    break;

                                case ChangeEventResourceType.AssetSet:
                                    oldResourceEntity = oldResource.AssetSet;
                                    newResourceEntity = newResource.AssetSet;
                                    break;

                                case ChangeEventResourceType.AssetSetAsset:
                                    oldResourceEntity = oldResource.AssetSetAsset;
                                    newResourceEntity = newResource.AssetSetAsset;
                                    break;

                                case ChangeEventResourceType.Campaign:
                                    oldResourceEntity = oldResource.Campaign;
                                    newResourceEntity = newResource.Campaign;
                                    break;

                                case ChangeEventResourceType.CampaignAsset:
                                    oldResourceEntity = oldResource.CampaignAsset;
                                    newResourceEntity = newResource.CampaignAsset;
                                    break;

                                case ChangeEventResourceType.CampaignAssetSet:
                                    oldResourceEntity = oldResource.CampaignAssetSet;
                                    newResourceEntity = newResource.CampaignAssetSet;
                                    break;

                                case ChangeEventResourceType.CampaignBudget:
                                    oldResourceEntity = oldResource.CampaignBudget;
                                    newResourceEntity = newResource.CampaignBudget;
                                    break;

                                case ChangeEventResourceType.CampaignCriterion:
                                    oldResourceEntity = oldResource.CampaignCriterion;
                                    newResourceEntity = newResource.CampaignCriterion;
                                    break;

                                case ChangeEventResourceType.CampaignFeed:
                                    oldResourceEntity = oldResource.CampaignFeed;
                                    newResourceEntity = newResource.CampaignFeed;
                                    break;

                                case ChangeEventResourceType.CustomerAsset:
                                    oldResourceEntity = oldResource.CustomerAsset;
                                    newResourceEntity = newResource.CustomerAsset;
                                    break;

                                case ChangeEventResourceType.Feed:
                                    oldResourceEntity = oldResource.Feed;
                                    newResourceEntity = newResource.Feed;
                                    break;

                                case ChangeEventResourceType.FeedItem:
                                    oldResourceEntity = oldResource.FeedItem;
                                    newResourceEntity = newResource.FeedItem;
                                    break;

                                default:
                                    knownResourceType = false;
                                    break;
                            }

                            if (!knownResourceType)
                            {
                                result += @$"Unknown change_resource_type 
                                    '{changeEvent.ChangeResourceType}'.";
                                continue;
                            }

                            result += $"On #{changeEvent.ChangeDateTime}, user " +
                                $"{changeEvent.UserEmail} used interface {changeEvent.ClientType} " +
                                $"to perform a(n) '{changeEvent.ResourceChangeOperation}' " +
                                $"operation on a '{changeEvent.ChangeResourceType}' with " +
                                $"resource name {changeEvent.ChangeResourceName}.";

                            foreach (string fieldMaskPath in changeEvent.ChangedFields.Paths)
                            {
                                if (changeEvent.ResourceChangeOperation ==
                                    ResourceChangeOperation.Create)
                                {
                                    object newValue = FieldMasks.GetFieldValue(
                                        fieldMaskPath, newResourceEntity);
                                    result += $"\t{fieldMaskPath} set to '{newValue}'.";
                                }
                                else if (changeEvent.ResourceChangeOperation ==
                                    ResourceChangeOperation.Update)
                                {
                                    object oldValue = FieldMasks.GetFieldValue(fieldMaskPath,
                                        oldResourceEntity);
                                    object newValue = FieldMasks.GetFieldValue(fieldMaskPath,
                                        newResourceEntity);

                                    result += $"\t{fieldMaskPath} changed from " + $"'{oldValue}' to '{newValue}'.";
                                }
                            }
                        }

                    });
                return new SuccessDataResult<ChangeEvent>(result);

            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<ChangeEvent>(e.Message);

            }

        }

        public IDataResult<List<string>> GetChangeSummary(long customerId)
        {
            GoogleAdsServiceClient googleAdsService = _client.GetService(
                Google.Ads.GoogleAds.Services.V12.GoogleAdsService);

            string searchQuery = @"
                SELECT
                    change_status.resource_name,
                    change_status.last_change_date_time,
                    change_status.resource_type,
                    change_status.campaign,
                    change_status.ad_group,
                    change_status.resource_status,
                    change_status.ad_group_ad,
                    change_status.ad_group_criterion,
                    change_status.campaign_criterion
                FROM change_status
                WHERE
                    change_status.last_change_date_time DURING LAST_14_DAYS
                ORDER BY change_status.last_change_date_time
                LIMIT 10000";

            // Create a request that will retrieve all changes using pages of the specified
            // page size.
            SearchGoogleAdsRequest request = new SearchGoogleAdsRequest()
            {
                PageSize = PAGE_SIZE,
                Query = searchQuery,
                CustomerId = customerId.ToString()
            };
            List<string> results = new List<string>();
            string result = "";
            try
            {
                // Issue the search request.
                PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse =
                    googleAdsService.Search(request);

                // Iterate over all rows in all pages and prints the requested field values for the
                // campaign in each row.
                foreach (GoogleAdsRow googleAdsRow in searchPagedResponse)
                {
                    result += $@"Last change: {googleAdsRow.ChangeStatus.LastChangeDateTime}, Resource type: {googleAdsRow.ChangeStatus.ResourceType}, 
                        Resource name: {googleAdsRow.ChangeStatus.ResourceName}, Resource status: {googleAdsRow.ChangeStatus.ResourceStatus},
                       Specific resource name: {SpecificResourceName(googleAdsRow.ChangeStatus.ResourceType, googleAdsRow)}";


                    results.Add(result);
                }

                return new SuccessDataResult<List<string>>(results, "");
            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<List<string>>(e.Message);
            }

        }

        public IDataResult<string> GetPendingInvitations(long customerId)
        {
            GoogleAdsServiceClient googleAdsService = _client.GetService(
              Services.V12.GoogleAdsService);
            string query = @"SELECT
                    customer_user_access_invitation.invitation_id,
                    customer_user_access_invitation.email_address,
                    customer_user_access_invitation.access_role,
                    customer_user_access_invitation.creation_date_time
                  FROM
                    customer_user_access_invitation
                  WHERE
                    customer_user_access_invitation.invitation_status=PENDING";
            string result = "";
            try
            {
                // Issue a search request.
                googleAdsService.SearchStream(customerId.ToString(), query,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            result += $@"A pending invitation with invitation ID = {googleAdsRow.CustomerUserAccessInvitation.InvitationId}, 
                                email address = '{googleAdsRow.CustomerUserAccessInvitation.EmailAddress}', access role = '{googleAdsRow.CustomerUserAccessInvitation.AccessRole}' and created on {googleAdsRow.CustomerUserAccessInvitation.CreationDateTime}
                                 was found.";
                        }
                    }
                );
                return new SuccessDataResult<string>(result);
            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<string>(e.Message);
            }

        }





        private List<CustomerClient> PrintAccountHierarchy(CustomerClient customerClient, Dictionary<long, List<CustomerClient>> customerIdsToChildAccounts, int depth)
        {
            string result = "";


            //if (depth == 0)
            //    result += "Customer ID (Descriptive Name, Currency Code, Time Zone)";


            long customerId = customerClient.Id;
            result += $"{customerId} ({customerClient.DescriptiveName}, {customerClient.CurrencyCode}, {customerClient.TimeZone})";

            List<CustomerClient> listOfCustomer = new List<CustomerClient>();

            // Recursively call this function for all child accounts of $customerClient.
            if (customerIdsToChildAccounts.ContainsKey(customerId))
            {
                foreach (CustomerClient childAccount in customerIdsToChildAccounts[customerId])
                {
                    //result += "-----";
                    PrintAccountHierarchy(childAccount, customerIdsToChildAccounts, depth + 1);
                    listOfCustomer.Add(childAccount);
                }

            }
            return listOfCustomer;
        }
        private string SpecificResourceName(ChangeStatusResourceType resourceType, GoogleAdsRow row)
        {
            string resourceName;
            switch (resourceType)
            {
                case ChangeStatusResourceType.AdGroup:
                    resourceName = row.ChangeStatus.AdGroup;
                    break;

                case ChangeStatusResourceType.AdGroupAd:
                    resourceName = row.ChangeStatus.AdGroupAd;
                    break;

                case ChangeStatusResourceType.AdGroupCriterion:
                    resourceName = row.ChangeStatus.AdGroupCriterion;
                    break;

                case ChangeStatusResourceType.Campaign:
                    resourceName = row.ChangeStatus.Campaign;
                    break;

                case ChangeStatusResourceType.CampaignCriterion:
                    resourceName = row.ChangeStatus.CampaignCriterion;
                    break;

                case ChangeStatusResourceType.Unknown:
                case ChangeStatusResourceType.Unspecified:
                default:
                    resourceName = "";
                    break;
            }
            return resourceName;
        }

        private static void UpdateMerchantCenterLinkStatus(long customerId,
         MerchantCenterLinkServiceClient merchantCenterLinkService,
         MerchantCenterLink merchantCenterLink, MerchantCenterLinkStatus status)
        {
            // Enables the pending link.
            MerchantCenterLink linkToUpdate = new MerchantCenterLink()
            {
                ResourceName = merchantCenterLink.ResourceName,
                Status = status
            };

            // Creates an operation.
            MerchantCenterLinkOperation operation = new MerchantCenterLinkOperation()
            {
                Update = linkToUpdate,
                UpdateMask = FieldMasks.AllSetFieldsOf(linkToUpdate)
            };

            // Updates the link.
            MutateMerchantCenterLinkResponse mutateResponse =
                merchantCenterLinkService.MutateMerchantCenterLink(
                    customerId.ToString(), operation);
            string result = "";

            result += $"The status of Merchant Center Link with resource name " +
                 $"'{mutateResponse.Result.ResourceName}' to Google Ads account : " +
                 $"{customerId} was updated to {status}.";
        }

        public IDataResult<string> InviteUserWithAccessRole(long customerId, string emailAddress, AccessRole accessRole)
        {
            CustomerUserAccessInvitationServiceClient service = _client.GetService(
              Services.V12.CustomerUserAccessInvitationService);

            MutateCustomerUserAccessInvitationRequest invitationRequest =
               new MutateCustomerUserAccessInvitationRequest()
               {
                   CustomerId = customerId.ToString(),
                   Operation = new CustomerUserAccessInvitationOperation()
                   {
                       Create = new CustomerUserAccessInvitation()
                       {
                           EmailAddress = emailAddress,
                           AccessRole = accessRole
                       },
                   }
               };
            string result = "";
            try
            {
                var response = service.MutateCustomerUserAccessInvitation(invitationRequest);

                result += @$"Customer user access invitation was sent for customerId = 
                    {customerId} to email address = {emailAddress} and access role = {accessRole}. The invitation resource
                    name is {response.Result.ResourceName}.";

                return new SuccessDataResult<string>(result);
            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<string>(e.Message + e.Status);
            }
        }

        public IDataResult<string> UpdateUserAccess(long customerId, string emailAddress, AccessRole accessRole)
        {
            try
            {
                string result = "";
                long? userId = GetUserAccess(_client, customerId, emailAddress);
                if (userId != null)
                {
                    result += ModifyUserAccess(_client, customerId, userId.Value, accessRole);
                }
                return new SuccessDataResult<string>(result);
            }
            catch (GoogleAdsException e)
            {
                return new ErrorDataResult<string>(e.Message);
            }
        }

        private long? GetUserAccess(GoogleAdsClient client, long customerId, string emailAddress)
        {
            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = client.GetService(
                Services.V12.GoogleAdsService);

            // Create the search query. Use the LIKE query for filtering to ignore the text case
            // for email address when searching for a match.
            string searchQuery = "Select customer_user_access.user_id, " +
                "customer_user_access.email_address, customer_user_access.access_role," +
                "customer_user_access.access_creation_date_time from customer_user_access " +
                $"where customer_user_access.email_address LIKE '{emailAddress}'";

            // Retrieves the user accesses.
            PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> searchPagedResponse =
                googleAdsService.Search(customerId.ToString(), searchQuery);

            GoogleAdsRow result = searchPagedResponse.FirstOrDefault();
            string answer = "";
            // Displays the results.
            if (result != null)
            {
                CustomerUserAccess access = result.CustomerUserAccess;
                answer += $@"Customer user access with User ID = {access.UserId}, Email Address = 
                    {access.EmailAddress}, Access Role = {access.AccessRole} and Creation Time = {access.AccessCreationDateTime} was found in
                    Customer ID: {customerId}.";

                return access.UserId;
            }
            else
            {

                return null;
            }
        }

        private string ModifyUserAccess(GoogleAdsClient client, long customerId, long userId,
           AccessRole accessRole)
        {
            // Get the CustomerUserAccessService.
            CustomerUserAccessServiceClient userAccessService = client.GetService(
                Services.V12.CustomerUserAccessService);

            // Creates the modified user access.
            CustomerUserAccess userAccess = new CustomerUserAccess()
            {
                ResourceName = ResourceNames.CustomerUserAccess(customerId, userId),
                AccessRole = accessRole
            };

            // Creates the operation.
            CustomerUserAccessOperation operation = new CustomerUserAccessOperation()
            {
                Update = userAccess,
                UpdateMask = FieldMasks.AllSetFieldsOf(userAccess)
            };

            // Updates the user access.
            MutateCustomerUserAccessResponse response =
                userAccessService.MutateCustomerUserAccess(
                    customerId.ToString(), operation);

            // Displays the result.
            return $"Successfully modified customer user access with " +
                 $"resource name '{response.Result.ResourceName}'.";
        }
        private const int PAGE_SIZE = 1_000;
    }

    

    
}
