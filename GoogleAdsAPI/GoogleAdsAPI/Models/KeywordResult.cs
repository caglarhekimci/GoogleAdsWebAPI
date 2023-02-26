namespace GoogleAdsAPI.Models
{
    public class KeywordResult
    {
        public string Keyword { get; set; } 
        public double SearchVolume { get; set; }
        public double Competition { get; set; } 
        public decimal LowCPCBid { get; set; }
        public decimal HighCPCBid { get; set; }
         
    }
}
