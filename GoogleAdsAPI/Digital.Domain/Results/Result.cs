using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Domain.Results
{
    public class Result : IResultBase
    {
        public Result(bool success,string message):this(success)
        {
           
            Message = message;
        }
        public Result(bool success)
        {
            Succes= success;
        }

        public bool Succes {get;set;}

        public string Message { get; set; }
    }
}
