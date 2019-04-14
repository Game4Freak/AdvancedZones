using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game4Freak.AdvancedZones
{
    public class Zone
    {
        private string name;
        private Node[] nodes;
        private List<int> flags;
        private List<string> blockedBuildables;
        private List<string> blockedEquips;
        private List<string> enterAddGroups;
        private List<string> enterRemoveGroups;
        private List<string> leaveAddGroups;
        private List<string> leaveRemoveGroups;
        public static string[] flagTypes = { "noDamage", "noVehicleDamage", "noLockpick", "noPlayerDamage", "noBuild", "noItemEquip", "noTireDamage", "noEnter", "noLeave", "enterMessage", "leaveMessage", "enterAddGroup", "enterRemoveGroup", "leaveAddGroup", "leaveRemoveGroup" };
        public static string[] flagDescs = { "No damage on structures or barricades", "No damage on vehicles", "No lockpick on vehicles", "No damage on players", "No placing of buildables", "No equiping of specific items", "No damage on tires",
            "No entering the zone", "No leaving the zone", "Message on entering the zone", "message on leaving the zone", "Group added on entering the zone", "Group removed on entering the zone", "Group added on leaving the zone", "Group removed on leaving the zone" };
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

        public Zone()
        {
            name = "";
            nodes = new Node[] { };
            flags = new List<int>();
            blockedBuildables = new List<string>();
            blockedEquips = new List<string>();
            enterAddGroups = new List<string>();
            enterRemoveGroups = new List<string>();
            leaveAddGroups = new List<string>();
            leaveRemoveGroups = new List<string>();
        }

        public Zone(string zoneName)
        {
            name = zoneName;
            nodes = new Node[] { };
            flags = new List<int>();
            blockedBuildables = new List<string>();
            blockedEquips = new List<string>();
            enterAddGroups = new List<string>();
            enterRemoveGroups = new List<string>();
            leaveAddGroups = new List<string>();
            leaveRemoveGroups = new List<string>();
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

        public void addFlag(int flag)
        {
            if (!flags.Contains(flag))
            {
                flags.Add(flag);
            }
        }

        public void removeFlag(int flag)
        {
            if (flags.Contains(flag))
            {
                flags.Remove(flag);
            }
        }

        public bool hasFlag(int flag)
        {
            return flags.Contains(flag);
        }

        public void addBlockedBuildable(string blockedBuildable)
        {
            if (!blockedBuildables.Contains(blockedBuildable))
            {
                blockedBuildables.Add(blockedBuildable);
            }
        }

        public void removeBlockedBuildable(string blockedBuildable)
        {
            if (blockedBuildables.Contains(blockedBuildable))
            {
                blockedBuildables.Remove(blockedBuildable);
            }
        }

        public void addBlockedEquip(string blockedEquip)
        {
            if (!blockedEquips.Contains(blockedEquip))
            {
                blockedEquips.Add(blockedEquip);
            }
        }

        public void removeBlockedEquip(string blockedEquip)
        {
            if (blockedEquips.Contains(blockedEquip))
            {
                blockedEquips.Remove(blockedEquip);
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

        public string getName()
        {
            return name;
        }

        public Node[] getNodes()
        {
            return nodes;
        }

        public List<int> getFlags()
        {
            return flags;
        }

        public List<string> getBlockedBuildables()
        {
            return blockedBuildables;
        }
        
        public List<string> getBlockedEquips()
        {
            return blockedEquips;
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

        public bool isReady()
        {
            return nodes.Length > 2;
        }
    }
}
