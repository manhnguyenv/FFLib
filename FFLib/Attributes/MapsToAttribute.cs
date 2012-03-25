using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MapsToAttribute : Attribute
    {

        public MapsToAttribute(string PropertyName)
        {
            this._propertyName = PropertyName;
        }

        private string _propertyName;

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

    }
}
