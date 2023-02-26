using Digital.Domain.Results;

namespace GoogleAdsAPI.ServicesAPI.KeywordService
{
    public interface IKeywordService
    {
        List<string> SearchWord(string[] keywordTexts);

        IDataResult<string> AddKeyword(long customerId, long adGroupId,string keywordText);
    }
}
