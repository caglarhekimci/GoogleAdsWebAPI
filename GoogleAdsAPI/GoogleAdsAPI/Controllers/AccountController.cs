using GoogleAdsAPI.ServicesAPI;
using GoogleAdsAPI.ServicesAPI.AccountService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Google.Ads.GoogleAds.V12.Enums.AccessRoleEnum.Types;

namespace GoogleAdsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    { 
        private readonly IAccountService _accountService;

        public AccountController(IAccountService account)
        { 
            _accountService = account;

        }
        [HttpPost("ListAccessibleCustomers")]
        public IActionResult ListAccessibleCustomers()
        {
            var result = _accountService.ListAccessibleCustomers();

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("GetAccountHierarchy")]
        public IActionResult GetAccountHierarchy(long? managerCustomerId, long? loginCustomerId)
        {
            var result = _accountService.GetAccountHierarchy(managerCustomerId, loginCustomerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("MerchantCenterLink")]
        public IActionResult MerchantCenterLink(long loginCustomerId,long merchantCenterAccountId)
        {
            var result = _accountService.MerchantCenterLink(loginCustomerId, merchantCenterAccountId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("CreateCustomer")]
        public IActionResult CreateCustomer(long managerCustomerId)
        {
            var result = _accountService.CreateCustomer(managerCustomerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("GetAccountInformation")]
        public IActionResult GetAccountInformation(long customerId)
        {
            var result = _accountService.GetAccountInformation(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("GetChangeDetail")]
        public IActionResult GetChangeDetail(long customerId)
        {
            var result = _accountService.GetChangeDetail(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("GetChangeSummary")]
        public IActionResult GetChangeSummary(long customerId)
        {
            var result = _accountService.GetChangeSummary(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }

        [HttpPost("GetPendingInvitations")]
        public IActionResult GetPendingInvitations(long customerId)
        {
            var result = _accountService.GetPendingInvitations(customerId);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("InviteUserWithAccessRole")]
        public IActionResult InviteUserWithAccessRole(long customerId,string emailAddress,AccessRole accessRole)
        {
            var result = _accountService.InviteUserWithAccessRole(customerId, emailAddress, accessRole);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
        [HttpPost("UpdateUserAccess")]
        public IActionResult UpdateUserAccess(long customerId, string emailAddress, AccessRole accessRole)
        {
            var result = _accountService.UpdateUserAccess(customerId, emailAddress, accessRole);

            if (result.Succes)
                return Ok(result);

            return BadRequest(result.Message);
        }
    }
}
