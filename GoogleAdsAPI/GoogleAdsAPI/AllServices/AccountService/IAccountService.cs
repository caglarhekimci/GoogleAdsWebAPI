using Digital.Domain.Results;
using Google.Ads.GoogleAds.V12.Resources;
using static Google.Ads.GoogleAds.V12.Enums.AccessRoleEnum.Types;

namespace GoogleAdsAPI.ServicesAPI.AccountService
{
    public interface IAccountService
    {
        IDataResult<List<string>> ListAccessibleCustomers(); 
        IDataResult<List<CustomerClient>> GetAccountHierarchy(long? managerCustomerId, long? loginCustomerId);
        IDataResult<string> MerchantCenterLink(long customerId, long merchantCenterAccountId);
        IDataResult<Customer> CreateCustomer(long managerCustomerId);

        IDataResult<Customer> GetAccountInformation(long customerId);

        IDataResult<ChangeEvent> GetChangeDetail(long customerId);

        IDataResult<List<string>> GetChangeSummary(long customerId);

        IDataResult<string> GetPendingInvitations(long customerId);

        IDataResult<string> InviteUserWithAccessRole(long customerId,string emailAddress,AccessRole accessRole);

        IDataResult<string> UpdateUserAccess(long customerId, string emailAddress, AccessRole accessRole);

    }
}
