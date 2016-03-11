using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Extensions
{
    public static class ObjectExtensions
    {
        public static int NotNull(this int? self)
        {
            if (self != null) return self.Value;
            return 0;
        }

        public static string NotNull(this string self)
        {
            if (self != null) return self;
            return string.Empty;
        }

        public static T NotNull<T>(this T self) where T :class,new()
        {
            if (self != null) return self;
            return new T();
        }

        public static T[] NotNull<T>(this T[] self)
        {
            if (self != null) return self;
            return new T[] { };
        }
    }
}
