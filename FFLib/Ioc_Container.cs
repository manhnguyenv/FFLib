using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib
{
    public interface IIocContainer
    {
        IDiResolver DiResolver { get; }
    }
}
