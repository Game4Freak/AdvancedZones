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
        public static string[] flagTypes = { "noDamage", "noVehicleDamage", "noLockpick", "noPlayerDamage", "noBuild", "noItemEquip", "noTireDamage" };
        public static string[] flagDescs = { "No damage on structures or barricades", "No damage on vehicles", "No lockpick on vehicles", "No damage on players", "No placing of buildables", "No equiping of specific items", "No damage on tires" };
        public static int noDamage = 0;
        public static int noVehicleDamage = 1;
        public static int noLockpick = 2;
        public static int noPlayerDamage = 3;
        public static int noBuild = 4;
        public static int noItemEquip = 5;
        public static int noTireDamage = 6;

        public Zone()
        {
            name = "";
            nodes = new Node[] { };
            flags = new List<int>();
            blockedBuildables = new List<string>();
            blockedEquips = new List<string>();
        }

        public Zone(string zoneName)
        {
            name = zoneName;
            nodes = new Node[] { };
            flags = new List<int>();
            blockedBuildables = new List<string>();
            blockedEquips = new List<string>();
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

        public bool isReady()
        {
            return nodes.Length > 2;
        }
    }
}
