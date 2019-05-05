using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Game4Freak.AdvancedZones
{
    public class Parameter
    {
        [XmlAttribute("name")]
        public string name;
        [XmlArrayItem(ElementName = "value")]
        public List<string> values;
        
        public Parameter()
        {
        }

        public Parameter(string newName, List<string> newValues)
        {
            name = newName;
            values = newValues;
        }
    }
}
