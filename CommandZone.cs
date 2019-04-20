using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Core;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using UnityEngine;

namespace Game4Freak.AdvancedZones
{
    public class CommandZone : IRocketCommand
    {
        public string Name
        {
            get { return "zone"; }
        }
        public string Help
        {
            get { return "administrate zones"; }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Syntax
        {
            get { return "wiki"; }
        }

        public List<string> Aliases
        {
            get { return new List<string> { "azone" }; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "advancedzones" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length != 1 && command.Length != 2 && command.Length != 3 && command.Length != 4 && command.Length != 5 && command.Length != 6)
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
            // ADD
            else if (command[0].ToLower() == "add")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add /zone add <zone|node|flag|block|group> <zonename> <flag|equip|build|enter|leave> <blockList|add|remove> <group>", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    string zoneName = command[2];
                    bool isValid = true;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == zoneName.ToLower())
                        {
                            isValid = false;
                        }
                    }
                    if (isValid)
                    {
                        AdvancedZones.Instance.Configuration.Instance.ZoneNames.Add(zoneName);
                        AdvancedZones.Instance.Configuration.Instance.ZoneNodes.Add(new List<float[]>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneFlags.Add(new List<int>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.Add(new List<string>());
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Added zone: " + zoneName, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "This name already exists", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "node")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add node <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    int x = 0;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Add(new float[] { player.Position.x, player.Position.z, player.Position.y });
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Added node at x: " + player.Position.x + ", z: " + player.Position.z + " to zone: " + z, UnityEngine.Color.cyan);
                            return;
                        }
                        x++;
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "flag")
                {
                    if (command.Length < 4)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add flag <zonename> <flag>", UnityEngine.Color.red);
                        return;
                    }
                    bool isFlag = false;
                    int flagNum = -1;
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        if (command[3].ToLower() == Zone.flagTypes[i].ToLower())
                        {
                            isFlag = true;
                            flagNum = i;
                        }
                    }
                    if (isFlag)
                    {
                        int x = 0;
                        foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                        {
                            if (z.ToLower() == command[2].ToLower())
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x).Contains(flagNum))
                                {
                                    UnturnedChat.Say(caller, "The zone: " + command[2] + " already has the flag: " + Zone.flagTypes[flagNum], UnityEngine.Color.red);
                                    return;
                                }
                                AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x).Add(flagNum);
                                UnturnedChat.Say(caller, "Added flag " + Zone.flagTypes[flagNum] + " to zone: " + z, UnityEngine.Color.cyan);
                                if (flagNum == Zone.enterMessage && AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(x).Count == 0)
                                {
                                    AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(x).Add("Now entering the zone: " + AdvancedZones.Instance.Configuration.Instance.ZoneNames.ElementAt(x));
                                    UnturnedChat.Say(caller, "Added default message on entering", UnityEngine.Color.cyan);
                                }
                                else if (flagNum == Zone.leaveMessage && AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(x).Count == 0)
                                {
                                    AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(x).Add("Now leaving the zone: " + AdvancedZones.Instance.Configuration.Instance.ZoneNames.ElementAt(x));
                                    UnturnedChat.Say(caller, "Added default message on leaving", UnityEngine.Color.cyan);
                                }
                                AdvancedZones.Instance.Configuration.Save();
                                return;
                            }
                            x++;
                        }
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                    else
                    {
                        string message = "Invalid! Try flags: ";
                        for (int i = 0; i < Zone.flagTypes.Length; i++)
                        {
                            message = message + Zone.flagTypes[i] + ", ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "block")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "equip")
                            {
                                foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                                {
                                    if (blockList.ToLower() == command[4].ToLower())
                                    {
                                        foreach (var zoneBlockList in AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                        {
                                            if (zoneBlockList.ToLower() == command[4].ToLower())
                                            {
                                                UnturnedChat.Say(caller, "Zone already got the BlockList: " + zoneBlockList, UnityEngine.Color.red);
                                                return;
                                            }
                                        }
                                        AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(blockList);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Added BlockList: " + blockList + " to zone: " + z, UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                            else if (command[3].ToLower() == "build")
                            {
                                foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                                {
                                    if (blockList.ToLower() == command[4])
                                    {
                                        foreach (var zoneBlockList in AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                        {
                                            if (zoneBlockList.ToLower() == command[4])
                                            {
                                                UnturnedChat.Say(caller, "Zone already got the BlockList: " + zoneBlockList, UnityEngine.Color.red);
                                                return;
                                            }
                                        }
                                        AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(blockList);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Added BlockList: " + blockList + " to zone: " + z, UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone add block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "group")
                {
                    if (command.Length < 6)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "enter")
                            {
                                if (command[4].ToLower() == "add")
                                {
                                    foreach (var enterAddGroup in AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (enterAddGroup.ToLower() == command[5])
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the group: " + enterAddGroup, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[5]);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + z + " to add on entering", UnityEngine.Color.cyan);
                                    return;
                                }
                                else if (command[4].ToLower() == "remove")
                                {
                                    foreach (var enterRemoveGroup in AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (enterRemoveGroup.ToLower() == command[5])
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the group: " + enterRemoveGroup, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[5]);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + z + " to remove on entering", UnityEngine.Color.cyan);
                                    return;
                                }
                            }
                            else if (command[3].ToLower() == "leave")
                            {
                                if (command[4].ToLower() == "add")
                                {
                                    foreach (var leaveAddGroup in AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (leaveAddGroup.ToLower() == command[5])
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the group: " + leaveAddGroup, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[5]);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + z + " to add on leaveing", UnityEngine.Color.cyan);
                                    return;
                                }
                                else if (command[4].ToLower() == "remove")
                                {
                                    foreach (var leaveRemoveGroup in AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (leaveRemoveGroup.ToLower() == command[5])
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the group: " + leaveRemoveGroup, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[5]);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + z + " to remove on leaveing", UnityEngine.Color.cyan);
                                    return;
                                }
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone add group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "message")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "enter")
                            {
                                AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[4]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added message: " + command[4] + " to zone: " + z + " on entering", UnityEngine.Color.cyan);
                                return;
                            }
                            else if (command[3].ToLower() == "leave")
                            {
                                AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Add(command[4]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added message: " + command[4] + " to zone: " + z + " on leaving", UnityEngine.Color.cyan);
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone add message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add /zone add <zone|node|flag|block|group> <zonename> <flag|equip|build|enter|leave> <blockList|add|remove> <group>", UnityEngine.Color.red);
                    return;
                }
            }
            // REMOVE
            else if (command[0].ToLower() == "remove")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag|block|group> <zonename> <node|flag|equip|build|enter|leave> <blockList|add|remove> <group>", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    int x = 0;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            AdvancedZones.Instance.Configuration.Instance.ZoneNames.Remove(z);
                            AdvancedZones.Instance.Configuration.Instance.ZoneNodes.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneFlags.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.RemoveAt(x);
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Removed zone: " + z, UnityEngine.Color.cyan);
                            return;
                        }
                        x++;
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "node")
                {
                    if (command.Length < 4)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove node <zonename> <node>", UnityEngine.Color.red);
                        return;
                    }
                    int nodeNum = -1;
                    if (int.TryParse(command[3], out nodeNum))
                    {
                        int x = 0;
                        foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                        {
                            if (z.ToLower() == command[2].ToLower())
                            {
                                if (nodeNum < AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Count())
                                {
                                    AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).RemoveAt(nodeNum);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed node (" + nodeNum + ") from zone: " + z, UnityEngine.Color.cyan);
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not has the node: (" + nodeNum + ")", UnityEngine.Color.red);
                                }
                                return;
                            }
                            x++;
                        }
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! " + command[3] + "is not a node", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "flag")
                {
                    if (command.Length < 4)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove flag <zonename> <flag>", UnityEngine.Color.red);
                        return;
                    }
                    bool isFlag = false;
                    int flagNum = -1;
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        if (command[3].ToLower() == Zone.flagTypes[i].ToLower())
                        {
                            isFlag = true;
                            flagNum = i;
                        }
                    }
                    if (isFlag)
                    {
                        int x = 0;
                        foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                        {
                            if (z.ToLower() == command[2].ToLower())
                            {
                                foreach (var f in AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x))
                                {
                                    if (f == flagNum)
                                    {
                                        AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x).Remove(f);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed flag " + Zone.flagTypes[flagNum] + " from zone: " + z, UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + z + " does not has the flag: " + Zone.flagTypes[flagNum], UnityEngine.Color.red);
                                return;
                            }
                            x++;
                        }
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                    else
                    {
                        string message = "Invalid! Try flags: ";
                        for (int i = 0; i < Zone.flagTypes.Length; i++)
                        {
                            message = message + Zone.flagTypes[i] + ", ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "block")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "equip")
                            {
                                foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                                {
                                    if (blockList.ToLower() == command[4].ToLower())
                                    {
                                        foreach (var zoneBlockList in AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                        {
                                            if (zoneBlockList.ToLower() == command[4].ToLower())
                                            {
                                                AdvancedZones.Instance.Configuration.Instance.ZoneBlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(blockList);
                                                AdvancedZones.Instance.Configuration.Save();
                                                UnturnedChat.Say(caller, "Removed BlockList: " + blockList + " from zone: " + z, UnityEngine.Color.cyan);
                                                return;
                                            }
                                        }
                                        UnturnedChat.Say(caller, "The zone: " + z + " does not have the BlockList: " + blockList, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                            else if (command[3].ToLower() == "build")
                            {
                                foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                                {
                                    if (blockList.ToLower() == command[4])
                                    {
                                        foreach (var zoneBlockList in AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                        {
                                            if (zoneBlockList.ToLower() == command[4])
                                            {
                                                AdvancedZones.Instance.Configuration.Instance.ZoneBlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(blockList);
                                                AdvancedZones.Instance.Configuration.Save();
                                                UnturnedChat.Say(caller, "Removed BlockList: " + blockList + " from zone: " + z, UnityEngine.Color.cyan);
                                                return;
                                            }
                                        }
                                        UnturnedChat.Say(caller, "The zone: " + z + " does not have the BlockList: " + blockList, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone remove block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "group")
                {
                    if (command.Length < 6)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "enter")
                            {
                                if (command[4].ToLower() == "add")
                                {
                                    foreach (var enterAddGroup in AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (enterAddGroup.ToLower() == command[5])
                                        {
                                            AdvancedZones.Instance.Configuration.Instance.ZoneEnterAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(enterAddGroup);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed group: " + enterAddGroup + " from zone: " + z + " from add on entering", UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not have the group: " + command[5], UnityEngine.Color.red);
                                    return;
                                }
                                else if (command[4].ToLower() == "remove")
                                {
                                    foreach (var enterRemoveGroup in AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (enterRemoveGroup.ToLower() == command[5])
                                        {
                                            AdvancedZones.Instance.Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(enterRemoveGroup);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed group: " + enterRemoveGroup + " from zone: " + z + " from remove on entering", UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not have the group: " + command[5], UnityEngine.Color.red);
                                    return;
                                }
                            }
                            else if (command[3].ToLower() == "leave")
                            {
                                if (command[4].ToLower() == "add")
                                {
                                    foreach (var leaveAddGroup in AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (leaveAddGroup.ToLower() == command[5])
                                        {
                                            AdvancedZones.Instance.Configuration.Instance.ZoneLeaveAddGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(leaveAddGroup);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed group: " + leaveAddGroup + " from zone: " + z + " from add on leaveing", UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not have the group: " + command[5], UnityEngine.Color.red);
                                    return;
                                }
                                else if (command[4].ToLower() == "remove")
                                {
                                    foreach (var leaveRemoveGroup in AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)))
                                    {
                                        if (leaveRemoveGroup.ToLower() == command[5])
                                        {
                                            AdvancedZones.Instance.Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Remove(leaveRemoveGroup);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed group: " + leaveRemoveGroup + " from zone: " + z + " from remove on leaveing", UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not have the group: " + command[5], UnityEngine.Color.red);
                                    return;
                                }
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone remove group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "message")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                        return;
                    }
                    int messageIndex = -1;
                    if (!int.TryParse(command[4], out messageIndex)) return;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (command[3].ToLower() == "enter")
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Count < messageIndex)
                                {
                                    UnturnedChat.Say(caller, "Removed message: " + AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).ElementAt(messageIndex) + " from zone: " + z + " on entering", UnityEngine.Color.cyan);
                                    AdvancedZones.Instance.Configuration.Instance.ZoneEnterMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).RemoveAt(messageIndex);
                                    AdvancedZones.Instance.Configuration.Save();
                                    return;
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, "The Message at (" + messageIndex + ") does not exist", UnityEngine.Color.red);
                                }
                            }
                            else if (command[3].ToLower() == "leave")
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).Count < messageIndex)
                                {
                                    UnturnedChat.Say(caller, "Removed message: " + AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).ElementAt(messageIndex) + " from zone: " + z + " on leaving", UnityEngine.Color.cyan);
                                    AdvancedZones.Instance.Configuration.Instance.ZoneLeaveMessages.ElementAt(AdvancedZones.Instance.Configuration.Instance.ZoneNames.IndexOf(z)).RemoveAt(messageIndex);
                                    AdvancedZones.Instance.Configuration.Save();
                                    return;
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, "The Message at (" + messageIndex + ") does not exist", UnityEngine.Color.red);
                                }
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "Invalid! Try /zone remove message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    return;
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag|block|group> <zonename> <node|flag|equip|build|enter|leave> <blockList|add|remove> <group>", UnityEngine.Color.red);
                    return;
                }
            }
            // REPLACE
            else if (command[0].ToLower() == "replace")
            {
                if (command.Length < 4)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone replace <zone|node> <zonename> <newzonename|node>", UnityEngine.Color.red);
                    return;
                }
                if (command[1].ToLower() == "zone")
                {
                    int x = 0;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            AdvancedZones.Instance.Configuration.Instance.ZoneNames[x] = command[3];
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Renamed zone: " + z + " to: " + command[3], UnityEngine.Color.cyan);
                            return;
                        }
                        x++;
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "node")
                {
                    int nodeNum = -1;
                    if (!int.TryParse(command[3], out nodeNum))
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone replace node <zonename> <node>", UnityEngine.Color.red);
                        return;
                    }
                    int x = 0;
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                    {
                        if (z.ToLower() == command[2].ToLower())
                        {
                            if (AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Count < nodeNum)
                            {
                                UnturnedChat.Say(caller, "The zone: " + command[2] + " does not have the node: " + nodeNum, UnityEngine.Color.red);
                                return;
                            }
                            AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x)[nodeNum] = new float[] { player.Position.x, player.Position.z, player.Position.y };
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Replaced node: " + nodeNum + " at x: " + AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum)[0] + ", z: " + AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum)[1] + " on zone: " + z
                                + " with x: " + player.Position.x + ", z: " + player.Position.z, UnityEngine.Color.cyan);
                            return;
                        }
                        x++;
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
            }
            // LIST
            else if (command[0].ToLower() == "list")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags|blocklists|groups> <zonename>", UnityEngine.Color.red);
                    return;
                }
                if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            string message = "";
                            if (z.isReady())
                            {
                                message = "Zone: " + z.getName() + "(ready): Flags: ";
                            }
                            else
                            {
                                message = "Zone: " + z.getName() + "(notReady): Flags: ";
                            }
                            foreach (var f in z.getFlags())
                            {
                                message = message + Zone.flagTypes[f] + ", ";
                            }
                            message = message.Substring(0, message.Length - 2) + "; Nodes: ";
                            for (int i = 0; i < z.getNodes().Length; i++)
                            {
                                message = message + "(" + i + ") x:" + z.getNodes()[i].getX() + " z: " + z.getNodes()[i].getZ() + ", ";
                            }
                            UnturnedChat.Say(caller, message.Substring(0, message.Length - 2) + ";", UnityEngine.Color.cyan);
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "zones")
                {
                    if (command.Length < 2)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list zones", UnityEngine.Color.red);
                        return;
                    }
                    string message = "Serverzones: ";
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.isReady())
                        {
                            message = message + z.getName() + "(ready)" + "{";
                        }
                        else
                        {
                            message = message + z.getName() + "(notReady)" + "{";
                        }
                        foreach (var f in z.getFlags())
                        {
                            message = message + Zone.flagTypes[f] + ",";
                        }
                        message = message + "}, ";
                    }
                    if (message == "Serverzones: ")
                    {
                        UnturnedChat.Say(caller, "There are no serverzones", UnityEngine.Color.red);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                    }
                }
                else if (command[1].ToLower() == "nodes")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list nodes <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            string message = "Nodes of zone: " + z.getName() + ": ";
                            for (int i = 0; i < z.getNodes().Length; i++)
                            {
                                message = message + "(" + i + ") x:" + z.getNodes()[i].getX() + " z: " + z.getNodes()[i].getZ() + ", ";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "flags")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list flags <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            string message = "Flags of zone: " + z.getName() + ": ";
                            foreach (var f in z.getFlags())
                            {
                                message = message + Zone.flagTypes[f] + ", ";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "blocklists")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list blocklists <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            string message = "BlockLists of zone: " + z.getName() + ": Equip{";
                            foreach (var blocked in z.getBlockedEquips())
                            {
                                message = message + blocked + ", ";
                            }
                            message = message + "}, Build{";
                            foreach (var blocked in z.getBlockedBuildables())
                            {
                                message = message + blocked + ", ";
                            }
                            UnturnedChat.Say(caller, message + "}", UnityEngine.Color.cyan);
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "groups")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list groups <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            string message = "Groups of zone: " + z.getName() + ": Enter{Add{";
                            foreach (var enterAddGroup in z.getEnterAddGroups())
                            {
                                message = message + enterAddGroup + ", ";
                            }
                            message = message + "}, Remove{";
                            foreach (var enterRemoveGroup in z.getEnterRemoveGroups())
                            {
                                message = message + enterRemoveGroup + ", ";
                            }
                            message = message + "}}, Leave{Add{";
                            foreach (var leaveAddGroup in z.getLeaveAddGroups())
                            {
                                message = message + leaveAddGroup + ", ";
                            }
                            message = message + "}, Remove{";
                            foreach (var leaveRemoveGroup in z.getLeaveRemoveGroups())
                            {
                                message = message + leaveRemoveGroup + ", ";
                            }
                            UnturnedChat.Say(caller, message + "}}", UnityEngine.Color.cyan);
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else if (command[1].ToLower() == "messages")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list messages <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.convertConfigToZone())
                    {
                        if (z.getName().ToLower() == command[2].ToLower())
                        {
                            UnturnedChat.Say(caller, "Messages of zone: " + z.getName() + " on entering:", UnityEngine.Color.cyan);
                            int x = 0;
                            foreach (var enterMessage in z.getEnterMessages())
                            {
                                UnturnedChat.Say(caller, "(" + x + ") " + enterMessage, UnityEngine.Color.cyan);
                                x++;
                            }
                            UnturnedChat.Say(caller, "Messages of zone: " + z.getName() + " on leaving:", UnityEngine.Color.cyan);
                            x = 0;
                            foreach (var leaveMessage in z.getleaveMessages())
                            {
                                UnturnedChat.Say(caller, "(" + x + ") " + leaveMessage, UnityEngine.Color.cyan);
                            }
                            return;
                        }
                    }
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags|blocklists|groups> <zonename>", UnityEngine.Color.red);
                    return;
                }
            }
            // INZONE
            else if (command[0].ToLower() == "inzone")
            {
                string message = "You are in the zones: ";
                foreach (var z in AdvancedZones.Instance.getPositionZones(player.Position))
                {
                    message = message + z.getName() + "{";
                    foreach (var f in z.getFlags())
                    {
                        message = message + Zone.flagTypes[f] + ",";
                    }
                    message = message + "}, ";
                }
                if (message == "You are in the zones: ")
                {
                    UnturnedChat.Say(caller, "You are in no zone", UnityEngine.Color.red);
                }
                else
                {
                    UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                }
            }
            // GETPOS
            else if (command[0].ToLower() == "getpos")
            {
                if (command.Length == 1)
                {
                    UnturnedChat.Say(caller, "Your position: x: " + player.Position.x + ", z: " + player.Position.z, UnityEngine.Color.cyan);
                }
                else
                {
                    UnturnedPlayer target = UnturnedPlayer.FromName(command[1]);
                    if (target != null)
                    {
                        UnturnedChat.Say(caller, target.DisplayName + "'s position: x: " + target.Position.x + ", z: " + player.Position.z, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Could not find player", UnityEngine.Color.red);
                    }
                }
            }
            // TP
            else if (command[0].ToLower() == "tp")
            {
                if (command.Length < 4)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone tp node <zonename> <node>", UnityEngine.Color.red);
                    return;
                }
                if (command[1].ToLower() == "node")
                {
                    int nodeNum = -1;
                    if (int.TryParse(command[3], out nodeNum))
                    {
                        int x = 0;
                        foreach (var z in AdvancedZones.Instance.Configuration.Instance.ZoneNames)
                        {
                            if (z.ToLower() == command[2].ToLower())
                            {
                                if (nodeNum < AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Count())
                                {
                                    player.Teleport(new Vector3(AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum)[0], AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum)[2], AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum)[1]), 0);
                                    UnturnedChat.Say(caller, "Teleported to node (" + nodeNum + ") from zone: " + z, UnityEngine.Color.cyan);
                                }
                                else
                                {
                                    UnturnedChat.Say(caller, "The zone: " + z + " does not has the node: (" + nodeNum + ")", UnityEngine.Color.red);
                                }
                                return;
                            }
                            x++;
                        }
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! " + command[3] + "is not a node", UnityEngine.Color.red);
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone tp node <zonename> <node>", UnityEngine.Color.red);
                    return;
                }
            }
            // FLAGS
            else if (command[0].ToLower() == "flags")
            {
                string message = "Avaliable Flags: ";
                for (int i = 0; i < Zone.flagTypes.Length; i++)
                {
                    message += Zone.flagTypes[i] + " (" + Zone.flagDescs[i] + "), ";
                }
                UnturnedChat.Say(caller, message , UnityEngine.Color.cyan);
                return;
            }
            // BLOCKLIST
            else if (command[0].ToLower() == "blocklist")
            {
                if (command.Length < 3)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone blockList <add|remove|list|addItem|removeItem> <equip|build> <blockList> <itemID>", UnityEngine.Color.red);
                    return;
                }
                if (command[1].ToLower() == "add")
                {
                    if (command[2].ToLower() == "equip")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList add equip <blockList>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                        {
                            if (equipBlockNames.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "The blockList: " + command[3] + " already exists", UnityEngine.Color.red);
                                return;
                            }
                        }
                        AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.Add(command[3]);
                        AdvancedZones.Instance.Configuration.Instance.BlockedEquip.Add(new List<int>());
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Added BlockList: " + command[3], UnityEngine.Color.cyan);
                        return;
                    }
                    else if (command[2].ToLower() == "build")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList add build <blockList>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                        {
                            if (buildBlockNames.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "The blockList: " + command[3] + " already exists", UnityEngine.Color.red);
                                return;
                            }
                        }
                        AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.Add(command[3]);
                        AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.Add(new List<int>());
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Added BlockList: " + command[3], UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone blockList add <equip|block> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "remove")
                {
                    if (command[2].ToLower() == "equip")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList remove equip <blockList>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                        {
                            if (equipBlockNames.ToLower() == command[3].ToLower())
                            {
                                AdvancedZones.Instance.Configuration.Instance.BlockedEquip.RemoveAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames));
                                AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.Remove(equipBlockNames);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Removed BlockList: " + command[3], UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else if (command[2].ToLower() == "build")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList remove build <blockList>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                        {
                            if (buildBlockNames.ToLower() == command[3].ToLower())
                            {
                                AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.RemoveAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames));
                                AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.Remove(buildBlockNames);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Removed BlockList: " + command[3], UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone blockList remove <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "list")
                {
                    if (command[2].ToLower() == "equip")
                    {
                        if (command.Length < 4)
                        {
                            string message = "BlockLists:";
                            foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                            {
                                message += " " + equipBlockNames + ",";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        }
                        else
                        {
                            string message = "BlockList: ";
                            foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                            {
                                if (equipBlockNames.ToLower() == command[3].ToLower())
                                {
                                    message += equipBlockNames + " {IDs:";
                                    foreach (var itemId in AdvancedZones.Instance.Configuration.Instance.BlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames)))
                                    {
                                        message += " " + itemId + ",";
                                    }
                                    UnturnedChat.Say(caller, message + "}", UnityEngine.Color.cyan);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "The BlockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        }
                    }
                    else if (command[2].ToLower() == "build")
                    {
                        if (command.Length < 4)
                        {
                            string message = "BlockLists:";
                            foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                            {
                                message += " " + buildBlockNames + ",";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        }
                        else
                        {
                            string message = "BlockList: ";
                            foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                            {
                                if (buildBlockNames.ToLower() == command[3].ToLower())
                                {
                                    message += buildBlockNames + " {IDs:";
                                    foreach (var itemId in AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames)))
                                    {
                                        message += " " + itemId + ",";
                                    }
                                    UnturnedChat.Say(caller, message + "}", UnityEngine.Color.cyan);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "The BlockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone blockList list <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "additem")
                {
                    if (command[2].ToLower() == "equip")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList addItem equip <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        int itemID;
                        if (!int.TryParse(command[4], out itemID)){
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList addItem equip <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                        {
                            if (equipBlockNames.ToLower() == command[3].ToLower())
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.BlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames)).Contains(itemID))
                                {
                                    UnturnedChat.Say(caller, "The BlockList: " + command[3] + " already contains " + itemID, UnityEngine.Color.red);
                                    return;
                                }
                                AdvancedZones.Instance.Configuration.Instance.BlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames)).Add(itemID);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added Item: " + command[4] + " to BlockList: " + command[3], UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else if (command[2].ToLower() == "build")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList addItem build <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        int itemID;
                        if (!int.TryParse(command[4], out itemID))
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList addItem build <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                        {
                            if (buildBlockNames.ToLower() == command[3].ToLower())
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames)).Contains(itemID))
                                {
                                    UnturnedChat.Say(caller, "The BlockList: " + command[3] + " already contains " + itemID, UnityEngine.Color.red);
                                    return;
                                }
                                AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames)).Add(itemID);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added Item: " + command[4] + " to BlockList: " + command[3], UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone blockList addItem <equip|build> <blockList> <itemID>", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "removeitem")
                {
                    if (command[2].ToLower() == "equip")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList removeItem equip <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        int itemID;
                        if (!int.TryParse(command[4], out itemID))
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList removeItem equip <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var equipBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames)
                        {
                            if (equipBlockNames.ToLower() == command[3].ToLower())
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.BlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames)).Contains(itemID))
                                {
                                    AdvancedZones.Instance.Configuration.Instance.BlockedEquip.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedEquipListNames.IndexOf(equipBlockNames)).Remove(itemID);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed Item: " + command[4] + " from BlockList: " + command[3], UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The BlockList: " + command[3] + " does not contain " + itemID, UnityEngine.Color.red);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else if (command[2].ToLower() == "build")
                    {
                        if (command.Length < 4)
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList removeItem build <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        int itemID;
                        if (!int.TryParse(command[4], out itemID))
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone blockList removeItem build <blockList> <itemID>", UnityEngine.Color.red);
                            return;
                        }
                        foreach (var buildBlockNames in AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames)
                        {
                            if (buildBlockNames.ToLower() == command[3].ToLower())
                            {
                                if (AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames)).Contains(itemID))
                                {
                                    AdvancedZones.Instance.Configuration.Instance.BlockedBuildables.ElementAt(AdvancedZones.Instance.Configuration.Instance.BlockedBuildablesListNames.IndexOf(buildBlockNames)).Remove(itemID);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed Item: " + command[4] + " from BlockList: " + command[3], UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The BlockList: " + command[3] + " does not contain " + itemID, UnityEngine.Color.red);
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The blockList: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone blockList removeItem <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone blockList <add|remove|list|addItem|removeItem> <equip|build> <blocked> <itemID>", UnityEngine.Color.red);
                    return;
                }
            }
            else if (command[0].ToLower() == "wiki")
            {
                player.Player.channel.send("askBrowserRequest", player.CSteamID, ESteamPacket.UPDATE_RELIABLE_BUFFER, "Need help? Take a look at the AdvancedZones wiki", "https://github.com/Game4Freak/AdvancedZones/wiki");
            }
            else if (command[0].ToLower() == "help")
            {
                UnturnedChat.Say(caller, "These are all commands of the AdvancedZones-Plugin", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "Check out the AdvancedZones wiki for more information with /zone wiki", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(1) /zone help", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(2) /zone wiki", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(3) /zone add <zone|node|flag|block|group|message> <zonename> <flag|equip|build|enter|leave> <blockList|message|add|remove> <group>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(4) /zone remove <zone|node|flag|block|group|message> <zonename> <node|flag|equip|build|enter|leave> <blockList|messageNum|add|remove> <group>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(5) /zone replace <zone|node> <zonename> <newzonename|node>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(6) /zone list <zone|zones|nodes|flags|blocklists|groups|messages> <zonename>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(7) /zone flags", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(8) /zone blockList <add|remove|list|addItem|removeItem> <equip|build> <blockList> <itemID>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(9) /zone inzone", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(10) /zone getpos <playername>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(11) /zone tp node <zonename> <node>", UnityEngine.Color.cyan);
                return;
            } else
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
        }
    }
}
