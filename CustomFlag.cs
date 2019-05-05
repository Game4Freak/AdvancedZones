using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Game4Freak.AdvancedZones
{
    public class CustomFlag
    {
        [XmlAttribute("name")]
        public string name;
        [XmlElement("id")]
        public int id;
        [XmlElement("description")]
        public string description;

        public CustomFlag()
        {
        }

        public CustomFlag(string newName, int newID, string newDescription)
        {
            name = newName;
            id = newID;
            description = newDescription;
        }
    }
}
