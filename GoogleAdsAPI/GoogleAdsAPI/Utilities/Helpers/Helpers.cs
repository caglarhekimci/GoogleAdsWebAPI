using Newtonsoft.Json;

namespace GoogleAdsAPI.Utilities.Helpers
{
    public static class Helpers
    {
        private static readonly JsonSerializerSettings _options
                 = new() { NullValueHandling = NullValueHandling.Ignore };
        public static object SimpleWrite(object obj, string fileName)
        {

            var jsonString = JsonConvert.SerializeObject(obj);

            File.WriteAllText(fileName, jsonString);

            return fileName;
        }
    }
}
