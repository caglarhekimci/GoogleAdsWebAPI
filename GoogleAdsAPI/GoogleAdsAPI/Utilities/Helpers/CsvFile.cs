using Digital.Domain.Results;
using GoogleAdsAPI.Constants.Messages;
using System.Globalization;
using System.Text;

namespace GoogleAdsAPI.Utilities.Helpers
{
    public class CsvFile
    {
        //
        // Summary:
        //     Headers in the csv file.
        private List<string> headers;

        //
        // Summary:
        //     Records in the csv file.
        private List<string[]> records;

        //
        // Summary:
        //     List of CSV file headers.
        public List<string> Headers => headers;

        //
        // Summary:
        //     List of records in the CSV file.
        public List<string[]> Records => records;

        //
        // Summary:
        //     Public constructor.
        public CsvFile()
        {
            headers = new List<string>();
            records = new List<string[]>();
        }

        //
        // Summary:
        //     Reads the contents of the CSV file into memory.
        //
        // Parameters:
        //   fileName:
        //     Full path to the csv file.
        //
        //   hasHeaders:
        //     True, if the first line of the csv file is a header.
        public void Read(string fileName, bool hasHeaders)
        {
            Load(fileName, hasHeaders);
        }

        //
        // Summary:
        //     Reads the contents of the CSV string into memory.
        //
        // Parameters:
        //   contents:
        //     Text to be parsed as csv file contents.
        //
        //   hasHeaders:
        //     True, if the first line of the csv file contents is a header.
        public void ReadFromString(string contents, bool hasHeaders)
        {
            Parse(contents, hasHeaders);
        }

        //
        // Summary:
        //     Writes the contents of the CsvFile object into a file.
        //
        // Parameters:
        //   fileName:
        //     The full path of the file to which the contents are to be written.
        //
        // Remarks:
        //     The file will have headers only if Google.Api.Ads.Common.Util.CsvFile.Headers
        //     are set for this object.
        public void Write(string fileName)
        {
            using StreamWriter streamWriter = new StreamWriter(fileName);
            if (Headers.Count != 0)
            {
                StringBuilder stringBuilder = ConvertRowToCsvString(Headers.ToArray());
                streamWriter.WriteLine(stringBuilder.ToString().TrimEnd(new char[1] { ',' }));
            }

            foreach (string[] record in Records)
            {
                StringBuilder stringBuilder2 = ConvertRowToCsvString(record);
                streamWriter.WriteLine(stringBuilder2.ToString().TrimEnd(new char[1] { ',' }));
            }
        }

        //
        // Summary:
        //     Converts a csv row item collection into a csv string.
        //
        // Parameters:
        //   rowItems:
        //     An array of string items which represents one row in CSV file.
        //
        // Returns:
        //     A StringBuilder object representing the stringized row. You can call a ToString()
        //     to get the stringized representation for this row.
        private static StringBuilder ConvertRowToCsvString(string[] rowItems)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < rowItems.Length; i++)
            {
                string text = rowItems[i];
                text = text.Replace("\"", "\"\"");
                if (text.Contains(",") || text.Contains("\""))
                {
                    text = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", text);
                }

                stringBuilder.Append(text + ",");
            }

            return stringBuilder;
        }

        //
        // Summary:
        //     Parses a csv file and loads it into memory.
        //
        // Parameters:
        //   filePath:
        //     Full path to the csv file.
        //
        //   hasHeaders:
        //     True, if the first line of the csv file is a header.
        private void Load(string filePath, bool hasHeaders)
        {
            using StreamReader streamReader = new StreamReader(filePath);
            string contents = streamReader.ReadToEnd();
            Parse(contents, hasHeaders);
        }

        //
        // Summary:
        //     Parses a csv file's contents and loads it into memory.
        //
        // Parameters:
        //   contents:
        //     File contents that should be parsed into memory.
        //
        //   hasHeaders:
        //     True, if the first line of the csv file is a header.
        private void Parse(string contents, bool hasHeaders)
        {
            string[] array = contents.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length != 0)
            {
                int num = 0;
                if (hasHeaders)
                {
                    headers = new List<string>(SplitCsvLine(array[0], StringSplitOptions.None));
                    num = 1;
                }
                else
                {
                    headers = null;
                }

                records = new List<string[]>();
                for (int i = num; i < array.Length; i++)
                {
                    string[] item = SplitCsvLine(array[i], StringSplitOptions.None);
                    records.Add(item);
                }
            }
        }

        //
        // Summary:
        //     Splits a csv line into its components.
        //
        // Parameters:
        //   text:
        //     The comma separated line to be split into components.
        //
        //   options:
        //     The string splitting options.
        //
        // Returns:
        //     The items, broken down as an array of strings.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     Thrown if text is null.
        //
        //   T:Google.Api.Ads.Common.Util.CsvException:
        //     Thrown if the csv string is malformed.
        private static string[] SplitCsvLine(string text, StringSplitOptions options)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            int startIndex = 0;
            List<string> list = new List<string>();
            bool flag = false;
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '"':
                        flag = !flag;
                        break;
                    case ',':
                        if (!flag)
                        {
                            list.AddRange(ExtractAndAddItem(text, startIndex, i, options));
                            startIndex = i + 1;
                        }

                        break;
                }
            }

            list.AddRange(ExtractAndAddItem(text, startIndex, text.Length, options));
            if (flag)
            {
                throw new ArgumentNullException("Quotes Not Closed In Csv Line");
            }

            return list.ToArray();
        }

        //
        // Summary:
        //     Extracts one token identified by SplitCsvLine.
        //
        // Parameters:
        //   text:
        //     The original comma separated line.
        //
        //   startIndex:
        //     Start index for the item just identified.
        //
        //   endIndex:
        //     Stop index for the item just identified.
        //
        //   options:
        //     The string split options to be used while extracting the token.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     Thrown if text is null or empty.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     Thrown if 0 < startIndex <= endIndex <= text.Length is not met.
        private static string[] ExtractAndAddItem(string text, int startIndex, int endIndex, StringSplitOptions options)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            if (startIndex < 0 || startIndex > text.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", CommonErrorMessages.StringIndexOutOfBounds);
            }

            if (endIndex > text.Length)
            {
                throw new ArgumentOutOfRangeException("endIndex", CommonErrorMessages.StringIndexOutOfBounds);
            }

            if (endIndex < startIndex)
            {
                throw new ArgumentOutOfRangeException(CommonErrorMessages.StartIndexShouldBeLessThanEndIndex);
            }

            string empty = string.Empty;
            empty = text.Substring(startIndex, endIndex - startIndex);
            empty = empty.Replace("\"\"", "\"");
            if (empty.Length >= 2 && empty[0] == '"' && empty[empty.Length - 1] == '"')
            {
                empty = empty.Substring(1, empty.Length - 2);
            }

            if (options == StringSplitOptions.None || (options == StringSplitOptions.RemoveEmptyEntries && !string.IsNullOrEmpty(empty)))
            {
                list.Add(empty);
            }

            return list.ToArray();
        }
    }
}
