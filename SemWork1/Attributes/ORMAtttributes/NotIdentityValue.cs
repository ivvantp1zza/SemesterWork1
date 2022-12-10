using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.Attributes.ORMAtttributes
{
    internal class NotIdentityValue : Attribute
    {
        public string PropName { get; private set; }

        public NotIdentityValue(string propName)
        {
            PropName = propName;
        }
    }
}
