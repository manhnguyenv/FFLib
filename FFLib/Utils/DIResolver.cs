using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib
{
    public interface IDiResolver
    {
        Object Resolve(Type objType);
        T Resolve<T>();
    }
}
