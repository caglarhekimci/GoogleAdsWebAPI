using Digital.Domain.Extensions;
using Google.Ads.GoogleAds.V12.Resources;

namespace GoogleAdsAPI.Models
{
    public class CampaignObject
    {
        public CampaignObject() { }
        public CampaignObject(object pObj)
        {
            //obj = pObj;
            Properties = pObj.GetPropertyDict();
        }
        public object obj;
        public Dictionary<string, string> Properties;

        private static Dictionary<int, CampaignObject> _ObjectDict;
        //public static Dictionary<int, CampaignObject> ObjectDict
        //{
        //    get
        //    {
        //        if (_ObjectDict == null)
        //            InitSites();
        //        return _ObjectDict;
        //    }
        //}
       
       
    }
}
