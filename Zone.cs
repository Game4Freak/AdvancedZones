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
        public static string[] flagTypes = { "noDamage", "noVehicleDamage", "noLockpick", "noPlayerDamage", "noBuild" };
        public static string[] flagDescs = { "No damage on structures or barricades", "No damage on vehicles", "No lockpick on vehicles", "No damage on players", "No placing of buildables"};
        public static int noDamage = 0;
        public static int noVehicleDamage = 1;
        public static int noLockpick = 2;
        public static int noPlayerDamage = 3;
        public static int noBuild = 4;

        public Zone()
        {
            name = "";
            nodes = new Node[] { };
            flags = new List<int>();
        }

        public Zone(string zoneName)
        {
            name = zoneName;
            nodes = new Node[] { };
            flags = new List<int>();
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
            flags.Add(flag);
        }

        public void removeFlag(int flag)
        {
            foreach (var f in flags)
            {
                if (f == flag)
                {
                    flags.Remove(f);
                }
            }
        }

        public bool hasFlag(int flag)
        {
            foreach (var f in flags)
            {
                if (f == flag)
                {
                    return true;
                }
            }
            return false;
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

        public bool isReady()
        {
            if (nodes.Length > 2)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
