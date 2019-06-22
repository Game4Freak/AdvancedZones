using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Game4Freak.AdvancedZones
{
    public class Zone
    {
        [XmlAttribute("name")]
        public string name;
        [XmlArrayItem(ElementName = "node")]
        public Node[] nodes;
        [XmlArrayItem(ElementName = "heightNode")]
        public HeightNode[] heightNodes;
        [XmlArrayItem(ElementName = "flag")]
        public List<string> flags;
        [XmlArrayItem(ElementName = "buildBlocklist")]
        public List<string> buildBlocklists;
        [XmlArrayItem(ElementName = "equipBlocklist")]
        public List<string> equipBlocklists;
        [XmlArrayItem(ElementName = "enterAddGroup")]
        public List<string> enterAddGroups;
        [XmlArrayItem(ElementName = "enterRemoveGroup")]
        public List<string> enterRemoveGroups;
        [XmlArrayItem(ElementName = "leaveAddGroup")]
        public List<string> leaveAddGroups;
        [XmlArrayItem(ElementName = "leaveRemoveGroup")]
        public List<string> leaveRemoveGroups;
        [XmlArrayItem(ElementName = "enterMessage")]
        public List<string> enterMessages;
        [XmlArrayItem(ElementName = "leaveMessage")]
        public List<string> leaveMessages;
        [XmlArrayItem(ElementName = "enterAddEffect")]
        public List<ushort> enterAddEffects;
        [XmlArrayItem(ElementName = "leaveAddEffect")]
        public List<ushort> leaveAddEffects;
        [XmlArrayItem(ElementName = "enterRemoveEffect")]
        public List<ushort> enterRemoveEffects;
        [XmlArrayItem(ElementName = "leaveRemoveEffect")]
        public List<ushort> leaveRemoveEffects;
        [XmlArrayItem(ElementName = "parameter")]
        public List<Parameter> parameters;

        public static string[] flagTypes = { "noDamage", "noVehicleDamage", "noLockpick", "noPlayerDamage", "noBuild", "noItemEquip", "noTireDamage", "noEnter", "noLeave", "enterMessage", "leaveMessage", "enterAddGroup", "enterRemoveGroup", "leaveAddGroup", "leaveRemoveGroup", "noZombie", "infiniteGenerator", "noVehicleCarjack",
            "noPvP", "noVehicleSiphoning", "enterAddEffect", "leaveAddEffect", "enterRemoveEffect", "leaveRemoveEffect", "noAnimalDamage", "noZombieDamage" };
        public static string[] flagDescs = { "No damage on structures or barricades", "No damage on vehicles", "No lockpick on vehicles", "No damage on players", "No placing of specific buildables", "No equiping of specific items", "No damage on tires", "No entering the zone", "No leaving the zone", "Message on entering the zone",
            "Message on leaving the zone", "Group added on entering the zone", "Group removed on entering the zone", "Group added on leaving the zone", "Group removed on leaving the zone", "No zombies", "Infinitely running generators", "No carjacking of vehicles", "No damage on players from other players", "No siphoning of vehicles",
            "Effect on entering the zone", "Effect on leaving the zone", "Effect cleared on entering the zone", "Effect cleared on leaving the zone", "No damge to animals", "No damage to zombies" };
        public static int noDamage = 0;
        public static int noVehicleDamage = 1;
        public static int noLockpick = 2;
        public static int noPlayerDamage = 3;
        public static int noBuild = 4;
        public static int noItemEquip = 5;
        public static int noTireDamage = 6;
        public static int noEnter = 7;
        public static int noLeave = 8;
        public static int enterMessage = 9;
        public static int leaveMessage = 10;
        public static int enterAddGroup = 11;
        public static int enterRemoveGroup = 12;
        public static int leaveAddGroup = 13;
        public static int leaveRemoveGroup = 14;
        public static int noZombie = 15;
        public static int infiniteGenerator = 16;
        public static int noVehicleCarjack = 17;
        public static int noPvP = 18;
        public static int noVehicleSiphoning = 19;
        public static int enterAddEffect = 20;
        public static int leaveAddEffect = 21;
        public static int enterRemoveEffect = 22;
        public static int leaveRemoveEffect = 23;
        public static int noAnimalDamage = 24;
        public static int noZombieDamage = 25;

        public Zone()
        {
            name = "";
            nodes = new Node[] { };
            heightNodes = new HeightNode[2];
            flags = new List<string>();
            buildBlocklists = new List<string>();
            equipBlocklists = new List<string>();
            enterAddGroups = new List<string>();
            enterRemoveGroups = new List<string>();
            leaveAddGroups = new List<string>();
            leaveRemoveGroups = new List<string>();
            enterMessages = new List<string>();
            leaveMessages = new List<string>();
            parameters = new List<Parameter>();
        }
        
        public Zone(string zoneName)
        {
            name = zoneName;
            nodes = new Node[] { };
            heightNodes = new HeightNode[2];
            flags = new List<string>();
            buildBlocklists = new List<string>();
            equipBlocklists = new List<string>();
            enterAddGroups = new List<string>();
            enterRemoveGroups = new List<string>();
            leaveAddGroups = new List<string>();
            leaveRemoveGroups = new List<string>();
            enterMessages = new List<string>();
            leaveMessages = new List<string>();
            parameters = new List<Parameter>();
        }

        public void addNode(Node node)
        {
            List<Node> nodeList = new List<Node>();
            for (int i = 0; i < nodes.Length; i++)
            {
                nodeList.Add(nodes[i]);
            }
            nodeList.Add(node);
            nodes = nodeList.ToArray();
        }

        public void removeNode(int nodeNum)
        {
            nodes[nodeNum] = null;
            List<Node> nodeList = new List<Node>();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    nodeList.Add(nodes[i]);
                }
            }
            nodes = nodeList.ToArray();
        }

        public void addHeightNode(HeightNode heightNode)
        {
            if (heightNodes[0] == null)
                heightNodes[0] = heightNode;
            if (heightNode.isUpper != heightNodes[0].isUpper)
                heightNodes[1] = heightNode;
            else
                heightNodes[0] = heightNode;
        }

        public void removeHeightNode(bool isUpper)
        {
            if (heightNodes[0] == null)
                return;
            if (heightNodes[0].isUpper == isUpper)
            {
                if (heightNodes[1] != null)
                {
                    heightNodes[0] = heightNodes[1];
                    heightNodes[1] = null;
                    return;
                }
                heightNodes[0] = null;
                return;
            }
            else if (heightNodes[1] != null)
            {
                heightNodes[1] = null;
                return;
            }
        }

        public void addFlag(string flag)
        {
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
            }
        }

        public void removeFlag(string flag)
        {
            if (flags.Contains(flag))
            {
                flags.Remove(flag);
            }
        }

        public bool hasFlag(string flag)
        {
            return flags.Contains(flag);
        }

        public void addBuildBlocklist(string blocklistName)
        {
            if (!buildBlocklists.Contains(blocklistName))
            {
                buildBlocklists.Add(blocklistName);
            }
        }

        public void removeBuildBlocklist(string blocklistName)
        {
            if (buildBlocklists.Contains(blocklistName))
            {
                buildBlocklists.Remove(blocklistName);
            }
        }

        public void addEquipBlocklist(string blocklistName)
        {
            if (!equipBlocklists.Contains(blocklistName))
            {
                equipBlocklists.Add(blocklistName);
            }
        }

        public void removeEquipBlocklist(string blocklistName)
        {
            if (equipBlocklists.Contains(blocklistName))
            {
                equipBlocklists.Remove(blocklistName);
            }
        }

        public void addEnterAddGroup(string group)
        {
            if (!enterAddGroups.Contains(group))
            {
                enterAddGroups.Add(group);
            }
        }

        public void removeEnterAddGroup(string group)
        {
            if (enterAddGroups.Contains(group))
            {
                enterAddGroups.Remove(group);
            }
        }

        public void addEnterRemoveGroup(string group)
        {
            if (!enterRemoveGroups.Contains(group))
            {
                enterRemoveGroups.Add(group);
            }
        }

        public void removeEnterRemoveGroup(string group)
        {
            if (enterRemoveGroups.Contains(group))
            {
                enterRemoveGroups.Remove(group);
            }
        }

        public void addLeaveAddGroup(string group)
        {
            if (!leaveAddGroups.Contains(group))
            {
                leaveAddGroups.Add(group);
            }
        }

        public void removeLeaveAddGroup(string group)
        {
            if (leaveAddGroups.Contains(group))
            {
                leaveAddGroups.Remove(group);
            }
        }

        public void addLeaveRemoveGroup(string group)
        {
            if (!leaveRemoveGroups.Contains(group))
            {
                leaveRemoveGroups.Add(group);
            }
        }

        public void removeLeaveRemoveGroup(string group)
        {
            if (leaveRemoveGroups.Contains(group))
            {
                leaveRemoveGroups.Remove(group);
            }
        }

        public void addEnterMessage(string message)
        {
            if (!enterMessages.Contains(message))
            {
                enterMessages.Add(message);
            }
        }

        public void removeEnterMessage(string message)
        {
            if (enterMessages.Contains(message))
            {
                enterMessages.Remove(message);
            }
        }

        public void addLeaveMessage(string message)
        {
            if (!leaveMessages.Contains(message))
            {
                leaveMessages.Add(message);
            }
        }

        public void removeLeaveMessage(string message)
        {
            if (leaveMessages.Contains(message))
            {
                leaveMessages.Remove(message);
            }
        }

        public void addEnterAddEffect(ushort effect)
        {
            if (!enterAddEffects.Contains(effect))
            {
                enterAddEffects.Add(effect);
            }
        }

        public void removeEnterAddEffect(ushort effect)
        {
            if (enterAddEffects.Contains(effect))
            {
                enterAddEffects.Remove(effect);
            }
        }

        public void addLeaveAddEffect(ushort effect)
        {
            if (!leaveAddEffects.Contains(effect))
            {
                leaveAddEffects.Add(effect);
            }
        }

        public void removeLeaveAddEffect(ushort effect)
        {
            if (leaveAddEffects.Contains(effect))
            {
                leaveAddEffects.Remove(effect);
            }
        }

        public void addEnterRemoveEffect(ushort effect)
        {
            if (!enterRemoveEffects.Contains(effect))
            {
                enterRemoveEffects.Add(effect);
            }
        }

        public void removeEnterRemoveEffect(ushort effect)
        {
            if (enterRemoveEffects.Contains(effect))
            {
                enterRemoveEffects.Remove(effect);
            }
        }

        public void addLeaveRemoveEffect(ushort effect)
        {
            if (!leaveRemoveEffects.Contains(effect))
            {
                leaveRemoveEffects.Add(effect);
            }
        }

        public void removeLeaveRemoveEffect(ushort effect)
        {
            if (leaveRemoveEffects.Contains(effect))
            {
                leaveRemoveEffects.Remove(effect);
            }
        }

        public void addParameter(string parameterName, List<string> parameterValues)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.name == parameterName)
                    return;
            }
            parameters.Add(new Parameter(parameterName, parameterValues));
        }

        public void removeParameter(string parameterName)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.name == parameterName)
                {
                    parameters.Remove(parameter);
                    return;
                }
            }
        }

        public string getName()
        {
            return name;
        }

        public Node[] getNodes()
        {
            return nodes;
        }

        public HeightNode[] GetHeightNodes()
        {
            return heightNodes;
        }

        public List<string> getFlags()
        {
            return flags;
        }

        public List<string> getBuildBlocklists()
        {
            return buildBlocklists;
        }
        
        public List<string> getEquipBlocklists()
        {
            return equipBlocklists;
        }

        public List<string> getEnterAddGroups()
        {
            return enterAddGroups;
        }

        public List<string> getEnterRemoveGroups()
        {
            return enterRemoveGroups;
        }

        public List<string> getLeaveAddGroups()
        {
            return leaveAddGroups;
        }

        public List<string> getLeaveRemoveGroups()
        {
            return leaveRemoveGroups;
        }

        public List<string> getEnterMessages()
        {
            return enterMessages;
        }

        public List<string> getLeaveMessages()
        {
            return leaveMessages;
        }

        public List<ushort> getEnterAddEffects()
        {
            return enterAddEffects;
        }

        public List<ushort> getLeaveAddEffects()
        {
            return leaveAddEffects;
        }

        public List<ushort> getEnterRemoveEffects()
        {
            return enterRemoveEffects;
        }

        public List<ushort> getLeaveRemoveEffects()
        {
            return leaveRemoveEffects;
        }

        public List<Parameter> GetParameters()
        {
            return parameters;
        }

        public bool isReady()
        {
            return nodes.Length > 2;
        }
    }
}
