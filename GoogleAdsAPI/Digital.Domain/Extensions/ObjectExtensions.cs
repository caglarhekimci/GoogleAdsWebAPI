using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Digital.Domain.Extensions
{
    public static class objectExtensions
    {
        #region Comparison
        public static bool EqualsIC(this object source, string toCheck)
        {
            if (source == null) return false;

            return source.ToStr().Equals(toCheck, StringComparison.InvariantCultureIgnoreCase);
        }
        public static bool ContainsIC(this object source, string toCheck)
        {
            if (source == null) return false;

            return source.ToStr().Contains(toCheck, StringComparison.InvariantCultureIgnoreCase);
        }
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
        #endregion Comparison

        #region Conversion
        public static bool ToBool(this object value)
        {
            return value == DBNull.Value || value == null ? false : Convert.ToBoolean(value);
        }
        public static Int32 ToInt(this object value)
        {
            return value == DBNull.Value || value == null ? 0 : Convert.ToInt32(value);
        }
        public static Int32? ToIntN(this object value)
        {
            if (value == DBNull.Value || value == null)
                return null;
            else
                return Convert.ToInt32(value);

        }
        public static long ToLong(this object value)
        {
            return value == DBNull.Value || value == null ? 0 : Convert.ToInt64(value);
        }
        public static long? ToLongN(this object value)
        {
            if (value == DBNull.Value || value == null) return null;
            return Convert.ToInt64(value);
        }
        public static double ToDbl(this object value)
        {
            double fReturn = 0;
            if (value != null && value != DBNull.Value)
                double.TryParse(value.ToString(), out fReturn);

            return fReturn;
        }
        public static double? ToDblN(this object value, int? decimals = null, bool isAllowZero = true)
        {
            if (value == DBNull.Value || value == null) return null;

            double fReturn = Convert.ToDouble(value);
            if (isAllowZero == false && fReturn == 0) return null;

            if (decimals == null)
                return fReturn;
            else
                return Math.Round(fReturn, (int)decimals);
        }

        public static DateTime ToDate(this object value)
        {
            return value == DBNull.Value || value == null ? DateTime.MinValue : Convert.ToDateTime(value);
        }
        public static DateTime ToDateTimeDB(this object value)
        {
            return value == DBNull.Value || value == null ? DateTime.MinValue : Convert.ToDateTime(value);
        }
        public static string ToStr(this object value)
        {
            return value == DBNull.Value || value == null ? null : Convert.ToString(value);
        }
        public static string ToStrFromDate(this DateTime? dt, string pFormat = "yyyy-MM-dd")
        {
            //Default: ISO 8601 standard: 
            return dt == null ? "" : Convert.ToDateTime(dt).ToString(pFormat);
        }
        #endregion Conversion

        public static Dictionary<string, string> GetPropertyDict(this object self, bool pisClass = false)
        {
            Dictionary<string, string> fReturn = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var Properties = self.GetType().GetProperties();

            foreach (var item in Properties.Where(e => e.CanRead && e.CanWrite
                                                        && (pisClass || e.PropertyType.Equals(typeof(string))
                                                                     || e.PropertyType.Equals(typeof(int))
                                                                     || e.PropertyType.Equals(typeof(double))
                                                                     || e.PropertyType.Equals(typeof(bool))
                                                                     || e.PropertyType.Equals(typeof(DateTime))
                                                                     || e.PropertyType.Equals(typeof(String))
                                                                     || e.PropertyType.Equals(typeof(int?))
                                                                     || e.PropertyType.Equals(typeof(double?))
                                                                     || e.PropertyType.Equals(typeof(bool?))
                                                                     || e.PropertyType.Equals(typeof(DateTime?))

                                                                     || e.PropertyType.Equals(typeof(byte))
                                                                     || e.PropertyType.Equals(typeof(long))
                                                                     || e.PropertyType.Equals(typeof(short))
                                                                     || e.PropertyType.Equals(typeof(float))
                                                                     || e.PropertyType.Equals(typeof(decimal))
                                                                     || e.PropertyType.Equals(typeof(char))

                                                                     || e.PropertyType.Equals(typeof(byte?))
                                                                     || e.PropertyType.Equals(typeof(long?))
                                                                     || e.PropertyType.Equals(typeof(short?))
                                                                     || e.PropertyType.Equals(typeof(float?))
                                                                     || e.PropertyType.Equals(typeof(decimal?))
                                                                     || e.PropertyType.Equals(typeof(char?))
                                                                     )))
            {
                var val = item.GetValue(self);
                fReturn.Add(item.Name, val?.ToString());
            }
            return fReturn;
        }
        public static string GetPropertyValue(this object self, string pProperty)
        {
            PropertyInfo pProp = self.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals(pProperty, StringComparison.InvariantCultureIgnoreCase));
            if (pProp != null && pProp.CanRead)
                return pProp.GetValue(self)?.ToString();
            return null;
        }
        public static PropertyInfo SetPropertyValue(this object self, string pProperty, object pValue, PropertyInfo pProp = null)
        {
            if (pProp == null) pProp = self.GetType().GetProperties().FirstOrDefault(e => e.Name.Equals(pProperty, StringComparison.InvariantCultureIgnoreCase));
            if (pProp != null && pProp.CanWrite)
                pProp.SetValue(self, pValue, null);
            return pProp;
        }

        public static string RemoveStr(this object value, params object[] args)
        {
            string fReturn = value == DBNull.Value || value == null ? null : Convert.ToString(value);
            foreach (var item in args)
                fReturn = fReturn.Replace(item.ToString(), "");

            return fReturn;
        }
        class CustomResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty prop = base.CreateProperty(member, memberSerialization);
                var propInfo = member as PropertyInfo;
                if (propInfo != null)
                {
                    if (propInfo.GetMethod.IsVirtual && !propInfo.GetMethod.IsFinal
                        || propInfo.GetCustomAttributes(false).Any(a => a is XmlIgnoreAttribute)
                        || propInfo.Name.StartsWith("$")
                        || propInfo.Name.StartsWith("Entity"))
                    {
                        prop.ShouldSerialize = obj => false;
                    }
                }
                return prop;
            }
        }
        public static string ToJson2(object self)
        {
            if (self == null) return "{}";
            // Serializer settings
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CustomResolver();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;

            return JsonConvert.SerializeObject(self, settings);
        }
        public static string ToJson2(this IEnumerable<object> self)
        {
            if (self == null) return "[]";
            // Serializer settings
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ContractResolver = new CustomResolver();
            settings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;

            return JsonConvert.SerializeObject(self, settings);
        }

        #region Copy from one object to another
        public static void CopyPropertiesFrom(this object self, object source, bool pIsCheckType = true, List<string> pExclude = null, bool pisClass = true, bool pisVirtualGet = false)
        {
            List<PropertyInfo> fromProperties = source.GetType().GetProperties().ToList();

            Dictionary<string, PropertyInfo> DictToProperties = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in self.GetType().GetProperties())
                if (item.SetMethod != null)
                    DictToProperties.Add(item.Name, item);

            if (pExclude != null)
                foreach (var item in pExclude)
                {
                    var fromProperty = fromProperties.FirstOrDefault(e => e.Name == item);
                    if (fromProperty != null)
                        fromProperties.Remove(fromProperty);
                }

            foreach (var from in fromProperties.Where(e => e.CanRead
                                                                && (e.PropertyType.Name.Equals("String") || pisClass || (!pisClass && !e.PropertyType.IsClass))                                                               
                                                                && (pisVirtualGet || !e.GetMethod.IsVirtual)))
            {
                if (DictToProperties.ContainsKey(from.Name))
                {
                    var to = DictToProperties[from.Name];
                    object value = from.GetValue(source);
                    if ((!pIsCheckType || pIsCheckType && to.PropertyType == from.PropertyType)
                            && value != to.GetValue(self))
                    {
                        if (pIsCheckType)
                            to.SetValue(self, value);
                        else if (value != null)
                            to.SetValue(self, value);
                        else if (to.PropertyType.IsGenericType && to.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            to.SetValue(self, null);
                    }
                }
            }
        }
        public static void CopyPropertiesFromDict(this object self, Dictionary<string, string> source, List<string> pExclude = null,
            bool isAllowEmptyString = false)
        {
            #region Make a list of properties to copy
            var toProperties = self.GetType().GetProperties();
            if (pExclude != null)
                foreach (var item in pExclude)
                    if (source.ContainsKey(item))
                        source.Remove(item);

            Dictionary<string, PropertyInfo> toPropertiesDict = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            //String Class is OK
            foreach (PropertyInfo prop in self.GetType().GetProperties().Where(e => e.CanWrite && (e.PropertyType == typeof(String) || !e.PropertyType.IsClass)))
                toPropertiesDict.Add(prop.Name, prop);
            #endregion Make a list of properties to copy

            #region Copy Values
            int i; double d; bool b; DateTime dt;
            foreach (var fromProperty in source.Keys)
            {
                if (toPropertiesDict.ContainsKey(fromProperty))
                {
                    PropertyInfo prop = toPropertiesDict[fromProperty];
                    Type T = prop.PropertyType;

                    if (T == null)
                        prop.SetValue(self, null, null);
                    else if (T == typeof(int) || T == typeof(int?))
                    {
                        if (int.TryParse(source[fromProperty], out i))
                            prop.SetValue(self, i, null);
                        else if (T == typeof(int?))
                            prop.SetValue(self, null, null);
                    }
                    else if (T == typeof(double) || T == typeof(double?))
                    {
                        if (double.TryParse(source[fromProperty], out d))
                            prop.SetValue(self, d, null);
                        else if (T == typeof(double?))
                            prop.SetValue(self, null, null);
                    }
                    else if (T == typeof(bool) || T == typeof(bool?))
                    {
                        if (bool.TryParse(source[fromProperty], out b))
                            prop.SetValue(self, b, null);
                        else if (T == typeof(bool?))
                            prop.SetValue(self, null, null);
                    }
                    else if (T == typeof(DateTime) || T == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(source[fromProperty], out dt))
                            prop.SetValue(self, dt, null);
                        else if (T == typeof(DateTime?))
                            prop.SetValue(self, null, null);
                    }
                    else
                    {
                        if (isAllowEmptyString)
                            prop.SetValue(self, source[fromProperty], null);
                        else if (source[fromProperty] != "")
                            prop.SetValue(self, source[fromProperty], null);
                        else
                            prop.SetValue(self, null, null);
                    }
                }
            }
            #endregion Copy Values
        }
        public static T DeepCopy<T>(this T a)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(a));
        }
        #endregion Copy from one object to another

        #region Update properties by factor
        public static void UpdatePropertiesByFactor(this object self, List<string> pFields, double pFactor)
        {
            // Get list of properties //
            List<PropertyInfo> fSelfProperties = self.GetType().GetProperties().Where(e => e.CanRead && !e.GetMethod.IsVirtual && pFields.Contains(e.Name)).ToList();

            // Compare list of properties to fields //
            foreach (PropertyInfo fProperty in fSelfProperties)
            {
                object fValue = fProperty.GetValue(self);
                double fPreviousValue;
                if (double.TryParse(fValue.ToStr(), out fPreviousValue) && fPreviousValue != 0)
                {
                    double fNewValue = fPreviousValue * pFactor;
                    fProperty.SetValue(self, fNewValue);
                }
            }
        }



        #endregion

        #region Wrap Methods
        public static string Wrap(this object source, string value = "|")
        {
            return value + (source?.ToString() ?? "") + value; // (char)224;
        }
        public static string WrapR(this object source, string value = "|")
        {
            return (source?.ToString() ?? "") + value;
        }
        public static string WrapL(this object source, string value = "|")
        {
            return value + (source?.ToString() ?? ""); // (char)224;
        }

        #endregion Wrap Methods
    }
}
