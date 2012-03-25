using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Attributes
{
    public enum HtmlEncoding { StripHtml, EncodeHtml, AllowSafeHtml, AllowUnsafeHtml };

    [AttributeUsage(AttributeTargets.Property)]
    public class HtmlEncodingAttribute : Attribute
    {
        
        public HtmlEncodingAttribute(HtmlEncoding encodingOption)
        {
            _encodingOption = encodingOption;
        }

        private HtmlEncoding _encodingOption;

        public HtmlEncoding EncodingOption
        {
            get { return _encodingOption; }
            set { _encodingOption = value; }
        }
    }
}

