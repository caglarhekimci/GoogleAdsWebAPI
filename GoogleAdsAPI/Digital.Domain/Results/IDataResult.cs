using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Digital.Domain.Results
{
    public interface IDataResult<T>: IResultBase
    {
        T Data { get;  }
    }
}
