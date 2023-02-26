using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Domain.Extensions
{
    public static class ExceptionExtensions
    {
        public static string RedmillTrace(this Exception ex)
        {
            string fReturn = String.Format("ERROR: {0}\n\nSource: {1}\n\nStackTrace: {2}\n"
                , (ex.Message ?? ""), (ex.Source ?? ""), (ex.StackTrace ?? ""));

            Exception? exInner = ex.InnerException;
            for (int i = 1; i < 10 && exInner != null; ++i)
            {
                fReturn += String.Format("\nInner Exception({0})-- (ERROR: {1}\n\nSource: {2}\n\nStackTrace: {3})\n", i
                                        , (exInner.Message ?? ""), (exInner.Source ?? ""), (exInner.StackTrace ?? ""));
                exInner = exInner.InnerException;
            }

            return fReturn;
        }

        public static string ToFullBlownString(this System.Exception e, int maxLevel = 10)
        {
            var sb = new StringBuilder();
            var exception = e;
            var counter = 1;
            while (exception != null && counter <= maxLevel)
            {
                sb.AppendLine($"{counter}-> Level: {counter}");
                sb.AppendLine($"{counter}-> Message: {exception.Message}");
                sb.AppendLine($"{counter}-> Source: {exception.Source}");
                sb.AppendLine($"{counter}-> Target Site: {exception.TargetSite}");
                sb.AppendLine($"{counter}-> Stack Trace: {exception.StackTrace}");

                exception = exception.InnerException;
                counter++;
            }

            return sb.ToString();
        }
    }
}
