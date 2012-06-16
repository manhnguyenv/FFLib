using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Data
{
    interface ISupportsIsDirty
    {
        bool IsDirty();
    }

    interface ISupportsIsNew
    {
        bool IsNew();
    }
}
