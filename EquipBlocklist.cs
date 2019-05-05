using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Game4Freak.AdvancedZones
{
    public class EquipBlocklist
    {
        [XmlAttribute("name")]
        public string name;
        [XmlArrayItem(ElementName = "itemID")]
        public List<int> itemIDs;

        public EquipBlocklist()
        {
        }
        
        public EquipBlocklist(string newName)
        {
            name = newName;
            itemIDs = new List<int>();
        }

        public EquipBlocklist(string newName, List<int> newItemIDs)
        {
            name = newName;
            itemIDs = newItemIDs;
        }

        public void addItem(int itemID)
        {
            if (!itemIDs.Contains(itemID))
            {
                itemIDs.Add(itemID);
            }
        }

        public void removeItem(int itemID)
        {
            if (itemIDs.Contains(itemID))
            {
                itemIDs.Remove(itemID);
            }
        }

        public bool hasItem(int itemID)
        {
            return itemIDs.Contains(itemID);
        }
    }
}
