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
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
            // ADD
            else if (command[0].ToLower() == "add")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add /zone add <zone|node|flag|block|group|parameter|heightnode|effect> <zonename> <flag|equip|build|enter|leave|values|isupper> <blockList|add|remove|heightoffset> <group|effect>", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.Zones)
                    {
                        if (z.name.ToLower() == command[2].ToLower())
                        {
                            UnturnedChat.Say(caller, "This name already exists", UnityEngine.Color.red);
                            return;
                        }
                    }
                    AdvancedZones.Instance.Configuration.Instance.Zones.Add(new Zone(command[2]));
                    AdvancedZones.Instance.Configuration.Save();
                    UnturnedChat.Say(caller, "Added zone: " + command[2], UnityEngine.Color.cyan);
                    return;
                }
                else if (command[1].ToLower() == "node")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add node <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        currentZone.addNode(new Node(player.Position.x, player.Position.z, player.Position.y));
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Added node at x: " + player.Position.x + ", z: " + player.Position.z + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        return;
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
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        if (command[3].ToLower() == Zone.flagTypes[i].ToLower())
                        {
                            Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                            if (currentZone != null)
                            {
                                if (currentZone.hasFlag(Zone.flagTypes[i]))
                                {
                                    UnturnedChat.Say(caller, "The zone: " + command[2] + " already has the flag: " + Zone.flagTypes[i], UnityEngine.Color.red);
                                    return;
                                }
                                currentZone.addFlag(Zone.flagTypes[i]);
                                UnturnedChat.Say(caller, "Added flag " + Zone.flagTypes[i] + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                if (i == Zone.enterMessage && currentZone.getEnterMessages().Count == 0)
                                {
                                    currentZone.addEnterMessage("Now entering the zone: " + currentZone.getName());
                                    UnturnedChat.Say(caller, "Added default message on entering", UnityEngine.Color.cyan);
                                }
                                else if (i == Zone.leaveMessage && currentZone.getLeaveMessages().Count == 0)
                                {
                                    currentZone.addLeaveMessage("Now leaving the zone: " + currentZone.getName());
                                    UnturnedChat.Say(caller, "Added default message on leaving", UnityEngine.Color.cyan);
                                }
                                AdvancedZones.Instance.Configuration.Save();
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < AdvancedZones.Instance.Configuration.Instance.CustomFlags.Count; i++)
                    {
                        if (command[3].ToLower() == AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name.ToLower())
                        {
                            Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                            if (currentZone != null)
                            {
                                if (currentZone.hasFlag(AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name))
                                {
                                    UnturnedChat.Say(caller, "The zone: " + command[2] + " already has the flag: " + Zone.flagTypes[i], UnityEngine.Color.red);
                                    return;
                                }
                                currentZone.addFlag(AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name);
                                UnturnedChat.Say(caller, "Added flag " + AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                AdvancedZones.Instance.Configuration.Save();
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    string message = "Invalid! Try flags: ";
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        message = message + Zone.flagTypes[i] + ", ";
                    }
                    foreach (var customFlag in AdvancedZones.Instance.Configuration.Instance.CustomFlags)
                    {
                        message = message + customFlag.name + ", ";
                    }
                    UnturnedChat.Say(caller, message, UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "block")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "equip")
                        {
                            foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                            {
                                if (blockList.name.ToLower() == command[4].ToLower())
                                {
                                    foreach (var blockListName in currentZone.getEquipBlocklists())
                                    {
                                        if (blockListName.ToLower() == blockList.name.ToLower())
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the BlockList: " + blockList.name, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    currentZone.addEquipBlocklist(blockList.name);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added BlockList: " + blockList.name + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                            return;
                        }
                        else if (command[3].ToLower() == "build")
                        {
                            foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                            {
                                if (blockList.name.ToLower() == command[4].ToLower())
                                {
                                    foreach (var blocklistName in currentZone.getBuildBlocklists())
                                    {
                                        if (blocklistName.ToLower() == command[4].ToLower())
                                        {
                                            UnturnedChat.Say(caller, "Zone already got the BlockList: " + blockList.name, UnityEngine.Color.red);
                                            return;
                                        }
                                    }
                                    currentZone.addBuildBlocklist(blockList.name);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Added BlockList: " + blockList.name + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
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
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var enterAddGroup in currentZone.getEnterAddGroups())
                                {
                                    if (enterAddGroup.ToLower() == command[5])
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the group: " + enterAddGroup, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addEnterAddGroup(command[5]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + currentZone.getName() + " to add on entering", UnityEngine.Color.cyan);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var enterRemoveGroup in currentZone.getEnterRemoveGroups())
                                {
                                    if (enterRemoveGroup.ToLower() == command[5])
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the group: " + enterRemoveGroup, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addEnterRemoveGroup(command[5]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + currentZone.getName() + " to remove on entering", UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        else if (command[3].ToLower() == "leave")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var leaveAddGroup in currentZone.getLeaveAddGroups())
                                {
                                    if (leaveAddGroup.ToLower() == command[5])
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the group: " + leaveAddGroup, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addLeaveAddGroup(command[5]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + currentZone.getName() + " to add on leaveing", UnityEngine.Color.cyan);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var leaveRemoveGroup in currentZone.getLeaveRemoveGroups())
                                {
                                    if (leaveRemoveGroup.ToLower() == command[5])
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the group: " + leaveRemoveGroup, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addLeaveRemoveGroup(command[5]);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added group: " + command[5] + " to zone: " + currentZone.getName() + " to remove on leaveing", UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone add group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "message")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            currentZone.addEnterMessage(command[4]);
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Added message: " + command[4] + " to zone: " + currentZone.getName() + " on entering", UnityEngine.Color.cyan);
                            return;
                        }
                        else if (command[3].ToLower() == "leave")
                        {
                            currentZone.addLeaveMessage(command[4]);
                            AdvancedZones.Instance.Configuration.Save();
                            UnturnedChat.Say(caller, "Added message: " + command[4] + " to zone: " + currentZone.getName() + " on leaving", UnityEngine.Color.cyan);
                            return;
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone add message <zonename> <enter|leave> <message>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "parameter")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add parameter <zonename> <values>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        foreach (var parameter in currentZone.GetParameters())
                        {
                            if (command[3].ToLower() == parameter.name.ToLower())
                            {
                                UnturnedChat.Say(caller, "Zone already got the parameter: " + parameter.name, UnityEngine.Color.red);
                                return;
                            }
                        }
                        List<string> values = new List<string>();
                        for (int i = 4; i < command.Length; i++)
                            values.Add(command[i]);
                        currentZone.addParameter(command[3], values);
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Added parameter: " + command[3] + " to zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "heightnode")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add heightnode <zonename> <isupper> <heightoffset>", UnityEngine.Color.red);
                        return;
                    }
                    float heightOffset = 0;
                    if (command.Length == 5)
                    {
                        if (!float.TryParse(command[4], out heightOffset))
                        {
                            UnturnedChat.Say(caller, command[4] + " is not a number", UnityEngine.Color.red);
                            return;
                        }
                    }
                    bool isUpper;
                    if (!bool.TryParse(command[3], out isUpper))
                    {
                        UnturnedChat.Say(caller, command[4] + " is not true of false", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        currentZone.addHeightNode(new HeightNode(player.Position.x, player.Position.z, player.Position.y + heightOffset, isUpper));
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Set heightNode isUpper: " + isUpper + " on zone: " + currentZone.name, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "effect")
                {
                    if (command.Length < 6)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add effect <zonename> <enter|leave> <add|remove> <effect>", UnityEngine.Color.red);
                        return;
                    }
                    ushort id = 0;
                    if (!ushort.TryParse(command[5], out id))
                    {
                        UnturnedChat.Say(caller, "Invalid! " + command[5] + "is not a number", UnityEngine.Color.red);
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var enterAddEffect in currentZone.getEnterAddEffects())
                                {
                                    if (enterAddEffect == id)
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the effect: " + enterAddEffect, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addEnterAddEffect(id);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added effect: " + command[5] + " to zone: " + currentZone.getName() + " to add on entering", UnityEngine.Color.cyan);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var enterRemoveEffect in currentZone.getEnterRemoveEffects())
                                {
                                    if (enterRemoveEffect == id)
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the effect: " + enterRemoveEffect, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addEnterRemoveEffect(id);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added effect: " + command[5] + " to zone: " + currentZone.getName() + " to remove on entering", UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        else if (command[3].ToLower() == "leave")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var leaveAddEffect in currentZone.getLeaveAddEffects())
                                {
                                    if (leaveAddEffect == id)
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the effect: " + leaveAddEffect, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addLeaveAddEffect(id);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added effect: " + command[5] + " to zone: " + currentZone.getName() + " to add on leaveing", UnityEngine.Color.cyan);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var leaveRemoveEffect in currentZone.getLeaveRemoveEffects())
                                {
                                    if (leaveRemoveEffect == id)
                                    {
                                        UnturnedChat.Say(caller, "Zone already got the effect: " + leaveRemoveEffect, UnityEngine.Color.red);
                                        return;
                                    }
                                }
                                currentZone.addLeaveRemoveEffect(id);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added effect: " + command[5] + " to zone: " + currentZone.getName() + " to remove on leaveing", UnityEngine.Color.cyan);
                                return;
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone add effect <zonename> <enter|leave> <add|remove> <effect>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add /zone add <zone|node|flag|block|group|parameter|heightnode|effect> <zonename> <flag|equip|build|enter|leave|values|isupper> <blockList|add|remove|heightoffset> <group|effect>", UnityEngine.Color.red);
                    return;
                }
            }
            // REMOVE
            else if (command[0].ToLower() == "remove")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag|block|group|parameter|heightnode|effect> <zonename> <node|flag|equip|build|enter|leave|values|isupper> <blockList|add|remove> <group|effect>", UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        UnturnedChat.Say(caller, "Removed zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        AdvancedZones.Instance.Configuration.Instance.Zones.RemoveAt(AdvancedZones.Instance.Configuration.Instance.Zones.IndexOf(currentZone));
                        AdvancedZones.Instance.Configuration.Save();
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
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
                        Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                        if (currentZone != null)
                        {
                            if (nodeNum < currentZone.getNodes().Count())
                            {
                                currentZone.removeNode(nodeNum);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Removed node (" + nodeNum + ") from zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not has the node: (" + nodeNum + ")", UnityEngine.Color.red);
                            }
                            return;
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        }
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
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        if (command[3].ToLower() == Zone.flagTypes[i].ToLower())
                        {
                            Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                            if (currentZone != null)
                            {
                                if (currentZone.hasFlag(Zone.flagTypes[i]))
                                {
                                    currentZone.removeFlag(Zone.flagTypes[i]);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed flag " + Zone.flagTypes[i] + " from zone: " + currentZone.name, UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not has the flag: " + Zone.flagTypes[i], UnityEngine.Color.red);
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    for (int i = 0; i < AdvancedZones.Instance.Configuration.Instance.CustomFlags.Count; i++)
                    {
                        if (command[3].ToLower() == AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name.ToLower())
                        {
                            Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                            if (currentZone != null)
                            {
                                if (currentZone.hasFlag(AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name))
                                {
                                    currentZone.removeFlag(AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed flag " + AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name + " from zone: " + currentZone.name, UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not has the flag: " + AdvancedZones.Instance.Configuration.Instance.CustomFlags[i].name, UnityEngine.Color.red);
                                return;
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                                return;
                            }
                        }
                    }
                    string message = "Invalid! Try flags: ";
                    for (int i = 0; i < Zone.flagTypes.Length; i++)
                    {
                        message = message + Zone.flagTypes[i] + ", ";
                    }
                    foreach (var customFlag in AdvancedZones.Instance.Configuration.Instance.CustomFlags)
                    {
                        message = message + customFlag.name + ", ";
                    }
                    UnturnedChat.Say(caller, message, UnityEngine.Color.red);
                    return;
                }
                else if (command[1].ToLower() == "block")
                {
                    if (command.Length < 5)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone add block <zonename> <equip|build> <blockList>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "equip")
                        {
                            foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                            {
                                if (blockList.name.ToLower() == command[4].ToLower())
                                {
                                    foreach (var zoneBlockList in currentZone.getEquipBlocklists())
                                    {
                                        if (zoneBlockList.ToLower() == command[4].ToLower())
                                        {
                                            currentZone.removeEquipBlocklist(blockList.name);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed BlockList: " + blockList.name + " from zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the BlockList: " + blockList, UnityEngine.Color.red);
                                    return;
                                }
                            }
                            UnturnedChat.Say(caller, "The blockList: " + command[4] + " does not exist", UnityEngine.Color.red);
                            return;
                        }
                        else if (command[3].ToLower() == "build")
                        {
                            foreach (var blockList in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                            {
                                if (blockList.name.ToLower() == command[4])
                                {
                                    foreach (var zoneBlockList in currentZone.getBuildBlocklists())
                                    {
                                        if (zoneBlockList.ToLower() == command[4].ToLower())
                                        {
                                            currentZone.removeBuildBlocklist(blockList.name);
                                            AdvancedZones.Instance.Configuration.Save();
                                            UnturnedChat.Say(caller, "Removed BlockList: " + blockList.name + " from zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                            return;
                                        }
                                    }
                                    UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the BlockList: " + blockList, UnityEngine.Color.red);
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
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "group")
                {
                    if (command.Length < 6)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var enterAddGroup in currentZone.getEnterAddGroups())
                                {
                                    if (enterAddGroup.ToLower() == command[5].ToLower())
                                    {
                                        currentZone.removeEnterAddGroup(enterAddGroup);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed group: " + enterAddGroup + " from zone: " + currentZone.getName() + " from add on entering", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the group: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var enterRemoveGroup in currentZone.getEnterRemoveGroups())
                                {
                                    if (enterRemoveGroup.ToLower() == command[5].ToLower())
                                    {
                                        currentZone.removeEnterRemoveGroup(enterRemoveGroup);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed group: " + enterRemoveGroup + " from zone: " + currentZone.getName() + " from remove on entering", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the group: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                        }
                        else if (command[3].ToLower() == "leave")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var leaveAddGroup in currentZone.getLeaveAddGroups())
                                {
                                    if (leaveAddGroup.ToLower() == command[5].ToLower())
                                    {
                                        currentZone.removeLeaveAddGroup(leaveAddGroup);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed group: " + leaveAddGroup + " from zone: " + currentZone.getName() + " from add on leaveing", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the group: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var leaveRemoveGroup in currentZone.getLeaveRemoveGroups())
                                {
                                    if (leaveRemoveGroup.ToLower() == command[5].ToLower())
                                    {
                                        currentZone.removeLeaveRemoveGroup(leaveRemoveGroup);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed group: " + leaveRemoveGroup + " from zone: " + currentZone.getName() + " from remove on leaveing", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the group: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone remove group <zonename> <enter|leave> <add|remove> <group>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
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
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            if (currentZone.getEnterMessages().Count > messageIndex)
                            {
                                UnturnedChat.Say(caller, "Removed message: " + currentZone.getEnterMessages()[messageIndex] + " from zone: " + currentZone.getName() + " on entering", UnityEngine.Color.cyan);
                                currentZone.getEnterMessages().RemoveAt(messageIndex);
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
                            if (currentZone.getLeaveMessages().Count > messageIndex)
                            {
                                UnturnedChat.Say(caller, "Removed message: " + currentZone.getLeaveMessages()[messageIndex] + " from zone: " + currentZone.getName() + " on leaving", UnityEngine.Color.cyan);
                                currentZone.getLeaveMessages().RemoveAt(messageIndex);
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
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "parameter")
                {
                    if (command.Length < 4)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove parameter <zonename> <values>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        foreach (var parameter in currentZone.GetParameters())
                        {
                            if (command[3].ToLower() == parameter.name.ToLower())
                            {
                                UnturnedChat.Say(caller, "Removed parameter: " + parameter.name + " from zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                                currentZone.removeParameter(parameter.name);
                                AdvancedZones.Instance.Configuration.Save();
                                return;
                            }
                        }
                        UnturnedChat.Say(caller, "The parameter: " + command[3] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "heightnode")
                {
                    if (command.Length < 4)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove heightnode <zonename> <isUpper>", UnityEngine.Color.red);
                        return;
                    }
                    bool isUpper;
                    if (!bool.TryParse(command[3], out isUpper))
                    {
                        UnturnedChat.Say(caller, command[4] + " is not true of false", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        currentZone.removeHeightNode(isUpper);
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Removed heightNode isUpper: " + isUpper + " from zone: " + currentZone.name, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "effect")
                {
                    if (command.Length < 6)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone remove effect <zonename> <enter|leave> <add|remove> <effect>", UnityEngine.Color.red);
                        return;
                    }
                    ushort id = 0;
                    if (!ushort.TryParse(command[5], out id))
                    {
                        UnturnedChat.Say(caller, "Invalid! " + command[5] + "is not a number", UnityEngine.Color.red);
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (command[3].ToLower() == "enter")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var enterAddEffect in currentZone.getEnterAddEffects())
                                {
                                    if (enterAddEffect == id)
                                    {
                                        currentZone.removeEnterAddEffect(enterAddEffect);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed effect: " + enterAddEffect + " from zone: " + currentZone.getName() + " from add on entering", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the effect: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var enterRemoveEffect in currentZone.getEnterRemoveEffects())
                                {
                                    if (enterRemoveEffect == id)
                                    {
                                        currentZone.removeEnterRemoveEffect(enterRemoveEffect);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed effect: " + enterRemoveEffect + " from zone: " + currentZone.getName() + " from remove on entering", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the effect: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                        }
                        else if (command[3].ToLower() == "leave")
                        {
                            if (command[4].ToLower() == "add")
                            {
                                foreach (var leaveAddEffect in currentZone.getLeaveAddEffects())
                                {
                                    if (leaveAddEffect == id)
                                    {
                                        currentZone.removeLeaveAddEffect(leaveAddEffect);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed effect: " + leaveAddEffect + " from zone: " + currentZone.getName() + " from add on leaveing", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the effect: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                            else if (command[4].ToLower() == "remove")
                            {
                                foreach (var leaveRemoveEffect in currentZone.getLeaveRemoveEffects())
                                {
                                    if (leaveRemoveEffect == id)
                                    {
                                        currentZone.removeLeaveRemoveEffect(leaveRemoveEffect);
                                        AdvancedZones.Instance.Configuration.Save();
                                        UnturnedChat.Say(caller, "Removed effect: " + leaveRemoveEffect + " from zone: " + currentZone.getName() + " from remove on leaveing", UnityEngine.Color.cyan);
                                        return;
                                    }
                                }
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not have the effect: " + command[5], UnityEngine.Color.red);
                                return;
                            }
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone remove effect <zonename> <enter|leave> <add|remove> <effect>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag|block|group|parameter|heightnode|effect> <zonename> <node|flag|equip|build|enter|leave|values|isupper> <blockList|add|remove> <group|effect>", UnityEngine.Color.red);
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
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        UnturnedChat.Say(caller, "Renamed zone: " + currentZone.getName() + " to: " + command[3], UnityEngine.Color.cyan);
                        currentZone.name = command[3];
                        AdvancedZones.Instance.Configuration.Save();
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "node")
                {
                    int nodeNum = -1;
                    if (!int.TryParse(command[3], out nodeNum))
                    {
                        UnturnedChat.Say(caller, "Invalid! " + command[3] + " is not a number", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        if (currentZone.getNodes().Count() < nodeNum)
                        {
                            UnturnedChat.Say(caller, "The zone: " + command[2] + " does not have the node: " + nodeNum, UnityEngine.Color.red);
                            return;
                        }
                        currentZone.getNodes()[nodeNum] = new Node(player.Position.x, player.Position.z, player.Position.y);
                        AdvancedZones.Instance.Configuration.Save();
                        UnturnedChat.Say(caller, "Replaced node: " + nodeNum + " at x: " + currentZone.getNodes()[nodeNum].x + ", z: " + currentZone.getNodes()[nodeNum].z + " on zone: " + currentZone.getName()
                            + " with x: " + player.Position.x + ", z: " + player.Position.z, UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
            }
            // LIST
            else if (command[0].ToLower() == "list")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags|blocklists|groups|parameters|heightnodes|effects> <zonename>", UnityEngine.Color.red);
                    return;
                }
                if (command[1].ToLower() == "zone")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list zone <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "";
                        if (currentZone.isReady())
                        {
                            message = "Zone: " + currentZone.getName() + "(ready): Flags: ";
                        }
                        else
                        {
                            message = "Zone: " + currentZone.getName() + "(notReady): Flags: ";
                        }
                        foreach (var f in currentZone.getFlags())
                        {
                            message = message + f + ", ";
                        }
                        message = message.Substring(0, message.Length - 2) + "; Nodes: ";
                        for (int i = 0; i < currentZone.getNodes().Length; i++)
                        {
                            message = message + "(" + i + ") x:" + currentZone.getNodes()[i].x + " z: " + currentZone.getNodes()[i].z + ", ";
                        }
                        UnturnedChat.Say(caller, message.Substring(0, message.Length - 2) + ";", UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "zones")
                {
                    if (command.Length < 2)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list zones", UnityEngine.Color.red);
                        return;
                    }
                    string message = "Serverzones: ";
                    foreach (var z in AdvancedZones.Instance.Configuration.Instance.Zones)
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
                            message = message + f + ", ";
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
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "Nodes of zone: " + currentZone.getName() + ": ";
                        for (int i = 0; i < currentZone.getNodes().Length; i++)
                        {
                            message = message + "(" + i + ") x:" + currentZone.getNodes()[i].x + " z: " + currentZone.getNodes()[i].z + ", ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "flags")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list flags <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "Flags of zone: " + currentZone.getName() + ": ";
                        foreach (var f in currentZone.getFlags())
                        {
                            message = message + f + ", ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "blocklists")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list blocklists <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "BlockLists of zone: " + currentZone.getName() + ": Equip{";
                        foreach (var blocked in currentZone.getEquipBlocklists())
                        {
                            message = message + blocked + ", ";
                        }
                        message = message + "}, Build{";
                        foreach (var blocked in currentZone.getBuildBlocklists())
                        {
                            message = message + blocked + ", ";
                        }
                        UnturnedChat.Say(caller, message + "}", UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "groups")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list groups <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "Groups of zone: " + currentZone.getName() + ": Enter{Add{";
                        foreach (var enterAddGroup in currentZone.getEnterAddGroups())
                        {
                            message = message + enterAddGroup + ", ";
                        }
                        message = message + "}, Remove{";
                        foreach (var enterRemoveGroup in currentZone.getEnterRemoveGroups())
                        {
                            message = message + enterRemoveGroup + ", ";
                        }
                        message = message + "}}, Leave{Add{";
                        foreach (var leaveAddGroup in currentZone.getLeaveAddGroups())
                        {
                            message = message + leaveAddGroup + ", ";
                        }
                        message = message + "}, Remove{";
                        foreach (var leaveRemoveGroup in currentZone.getLeaveRemoveGroups())
                        {
                            message = message + leaveRemoveGroup + ", ";
                        }
                        UnturnedChat.Say(caller, message + "}}", UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "messages")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list messages <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        UnturnedChat.Say(caller, "Messages of zone: " + currentZone.getName() + " on entering:", UnityEngine.Color.cyan);
                        int x = 0;
                        foreach (var enterMessage in currentZone.getEnterMessages())
                        {
                            UnturnedChat.Say(caller, "(" + x + ") " + enterMessage, UnityEngine.Color.cyan);
                            x++;
                        }
                        UnturnedChat.Say(caller, "Messages of zone: " + currentZone.getName() + " on leaving:", UnityEngine.Color.cyan);
                        x = 0;
                        foreach (var leaveMessage in currentZone.getLeaveMessages())
                        {
                            UnturnedChat.Say(caller, "(" + x + ") " + leaveMessage, UnityEngine.Color.cyan);
                        }
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else if (command[1].ToLower() == "parameters")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list parameters <zonename> <values>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "Parameters of zone: " + currentZone.getName() + ": ";
                        foreach (var parameter in currentZone.GetParameters())
                        {
                            message = message + parameter.name + " {";
                            foreach (var values in parameter.values)
                            {
                                message = message + values + ", ";
                            }
                            message = message + "}, ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "heightnodes")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list heightnodes <zonename> <values>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "HeightNodes of zone: " + currentZone.getName() + ": ";
                        foreach (var heightNode in currentZone.GetHeightNodes())
                        {
                            message = message + "isUpper: " + heightNode.isUpper + " {x: " + heightNode.x + ", z: " + heightNode.z + ", y: " + heightNode.y + "}, ";
                        }
                        UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        return;
                    }
                }
                else if (command[1].ToLower() == "effects")
                {
                    if (command.Length < 3)
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone list effects <zonename>", UnityEngine.Color.red);
                        return;
                    }
                    Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                    if (currentZone != null)
                    {
                        string message = "Effects of zone: " + currentZone.getName() + ": Enter{Add{";
                        foreach (var enterAddEffect in currentZone.getEnterAddEffects())
                        {
                            message = message + enterAddEffect + ", ";
                        }
                        message = message + "}, Remove{";
                        foreach (var enterRemoveEffect in currentZone.getEnterRemoveEffects())
                        {
                            message = message + enterRemoveEffect + ", ";
                        }
                        message = message + "}}, Leave{Add{";
                        foreach (var leaveAddEffect in currentZone.getLeaveAddEffects())
                        {
                            message = message + leaveAddEffect + ", ";
                        }
                        message = message + "}, Remove{";
                        foreach (var leaveRemoveEffect in currentZone.getLeaveRemoveEffects())
                        {
                            message = message + leaveRemoveEffect + ", ";
                        }
                        UnturnedChat.Say(caller, message + "}}", UnityEngine.Color.cyan);
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags|blocklists|groups|parameters|heightnodes|effects> <zonename>", UnityEngine.Color.red);
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
                        message = message + f + ",";
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
                        Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                        if (currentZone != null)
                        {
                            if (nodeNum < currentZone.getNodes().Count())
                            {
                                player.Teleport(new Vector3(currentZone.getNodes()[nodeNum].x, currentZone.getNodes()[nodeNum].y, currentZone.getNodes()[nodeNum].z), 0);
                                UnturnedChat.Say(caller, "Teleported to node (" + nodeNum + ") from zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                            }
                            else
                            {
                                UnturnedChat.Say(caller, "The zone: " + currentZone.getName() + " does not has the node: (" + nodeNum + ")", UnityEngine.Color.red);
                            }
                            return;
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
                        }
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
                foreach (var customFlag in AdvancedZones.Instance.Configuration.Instance.CustomFlags)
                {
                    message += customFlag.name + " (" + customFlag.description + "), ";
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
                        foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                        {
                            if (equipBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "The blockList: " + command[3] + " already exists", UnityEngine.Color.red);
                                return;
                            }
                        }
                        AdvancedZones.Instance.Configuration.Instance.EquipBlocklists.Add(new EquipBlocklist(command[3]));
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
                        foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                        {
                            if (buildBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "The blockList: " + command[3] + " already exists", UnityEngine.Color.red);
                                return;
                            }
                        }
                        AdvancedZones.Instance.Configuration.Instance.BuildBlocklists.Add(new BuildBlocklist(command[3]));
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
                        foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                        {
                            if (equipBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "Removed BlockList: " + equipBlocklist.name, UnityEngine.Color.cyan);
                                AdvancedZones.Instance.Configuration.Instance.EquipBlocklists.Remove(equipBlocklist);
                                AdvancedZones.Instance.Configuration.Save();
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
                        foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                        {
                            if (buildBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                UnturnedChat.Say(caller, "Removed BlockList: " + buildBlocklist.name, UnityEngine.Color.cyan);
                                AdvancedZones.Instance.Configuration.Instance.BuildBlocklists.Remove(buildBlocklist);
                                AdvancedZones.Instance.Configuration.Save();
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
                            foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                            {
                                message += " " + equipBlocklist.name + ",";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        }
                        else
                        {
                            string message = "BlockList: ";
                            foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                            {
                                if (equipBlocklist.name.ToLower() == command[3].ToLower())
                                {
                                    message += equipBlocklist.name + " {IDs:";
                                    foreach (var itemId in equipBlocklist.itemIDs)
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
                            foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                            {
                                message += " " + buildBlocklist.name + ",";
                            }
                            UnturnedChat.Say(caller, message, UnityEngine.Color.cyan);
                        }
                        else
                        {
                            string message = "BlockList: ";
                            foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                            {
                                if (buildBlocklist.name.ToLower() == command[3].ToLower())
                                {
                                    message += buildBlocklist.name + " {IDs:";
                                    foreach (var itemId in buildBlocklist.itemIDs)
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
                        foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                        {
                            if (equipBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                if (equipBlocklist.hasItem(itemID))
                                {
                                    UnturnedChat.Say(caller, "The BlockList: " + equipBlocklist.name + " already contains " + itemID, UnityEngine.Color.red);
                                    return;
                                }
                                equipBlocklist.addItem(itemID);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added Item: " + command[4] + " to BlockList: " + equipBlocklist.name, UnityEngine.Color.cyan);
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
                        foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                        {
                            if (buildBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                if (buildBlocklist.hasItem(itemID))
                                {
                                    UnturnedChat.Say(caller, "The BlockList: " + buildBlocklist.name + " already contains " + itemID, UnityEngine.Color.red);
                                    return;
                                }
                                buildBlocklist.addItem(itemID);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added Item: " + command[4] + " to BlockList: " + buildBlocklist.name, UnityEngine.Color.cyan);
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
                        foreach (var equipBlocklist in AdvancedZones.Instance.Configuration.Instance.EquipBlocklists)
                        {
                            if (equipBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                if (equipBlocklist.hasItem(itemID))
                                {
                                    equipBlocklist.removeItem(itemID);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed Item: " + command[4] + " from BlockList: " + equipBlocklist.name, UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The BlockList: " + equipBlocklist.name + " does not contain " + itemID, UnityEngine.Color.red);
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
                        foreach (var buildBlocklist in AdvancedZones.Instance.Configuration.Instance.BuildBlocklists)
                        {
                            if (buildBlocklist.name.ToLower() == command[3].ToLower())
                            {
                                if (buildBlocklist.hasItem(itemID))
                                {
                                    buildBlocklist.removeItem(itemID);
                                    AdvancedZones.Instance.Configuration.Save();
                                    UnturnedChat.Say(caller, "Removed Item: " + command[4] + " from BlockList: " + buildBlocklist.name, UnityEngine.Color.cyan);
                                    return;
                                }
                                UnturnedChat.Say(caller, "The BlockList: " + buildBlocklist.name + " does not contain " + itemID, UnityEngine.Color.red);
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
            else if (command[0].ToLower() == "visualize" || command[0].ToLower() == "show")
            {
                if (command.Length < 4)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone visualize <nodes|border> <zonename> <on|off> <space>", UnityEngine.Color.red);
                    return;
                }
                float space = 5;
                if (command.Length > 4)
                {
                    float.TryParse(command[4], out space);
                }
                Zone currentZone = AdvancedZones.Instance.getZoneByName(command[2]);
                if (currentZone != null)
                {
                    if (command[1].ToLower() == "nodes")
                    {
                        if (command[3].ToLower() == "on")
                        {
                            foreach (var node in currentZone.getNodes())
                            {
                                StructureManager.dropStructure(new Structure(1212), new Vector3(node.x, node.y + (float)2.5, node.z), 0, (float)(System.Math.Atan(node.x / node.z) * (180 / System.Math.PI)), 0, ulong.Parse(player.CSteamID.ToString()), ulong.Parse(player.SteamGroupID.ToString()));
                                StructureManager.dropStructure(new Structure(1212), new Vector3(node.x, node.y + (float)7.5, node.z), 0, (float)(System.Math.Atan(node.x / node.z) * (180 / System.Math.PI)), 0, ulong.Parse(player.CSteamID.ToString()), ulong.Parse(player.SteamGroupID.ToString()));
                            }
                            UnturnedChat.Say(caller, "Enabled visualizing nodes of zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        }
                        else if (command[3].ToLower() == "off")
                        {
                            foreach (var node in currentZone.getNodes())
                            {
                                byte x;
                                byte y;
                                Regions.tryGetCoordinate(new Vector3(node.x, node.y, node.z), out x, out y);
                                List<RegionCoordinate> coordinates = new List<RegionCoordinate>() { new RegionCoordinate(x, y) };
                                List<Transform> transforms = new List<Transform>();
                                StructureManager.getStructuresInRadius(new Vector3(node.x, node.y + (float)2.5, node.z), 2, coordinates, transforms);
                                StructureManager.getStructuresInRadius(new Vector3(node.x, node.y + (float)7.5, node.z), 2, coordinates, transforms);
                                foreach (var transform in transforms)
                                {
                                    if (transform.position == new Vector3(node.x, node.y + (float)2.5, node.z) || transform.position == new Vector3(node.x, node.y + (float)7.5, node.z))
                                        StructureManager.damage(transform, transform.position, 800, 10, false);
                                }
                            }
                            UnturnedChat.Say(caller, "Disabled visualizing nodes of zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone visualize nodes <zonename> <on|off>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else if (command[1].ToLower() == "border")
                    {
                        if (command[3].ToLower() == "on")
                        {
                            List<Vector3> positions = new List<Vector3>();
                            for (int i = 0; i < currentZone.getNodes().Length; i++)
                            {
                                Node node = currentZone.getNodes()[i];
                                Node nextNode;
                                if (i == currentZone.getNodes().Length - 1)
                                    nextNode = currentZone.getNodes()[0];
                                else
                                    nextNode = currentZone.getNodes()[i + 1];
                                Vector3 direction = new Vector3(nextNode.x, nextNode.y, nextNode.z) - new Vector3(node.x, node.y, node.z);
                                float magnitude = direction.magnitude;
                                int x = 0;
                                while (magnitude > x * space)
                                {
                                    positions.Add(new Vector3(node.x, node.y, node.z) + direction.normalized * space * x);
                                    x++;
                                }
                            }
                            for (int i = 0; i < positions.Count; i++)
                            {
                                int y = i + 1;
                                if (i == positions.Count - 1)
                                    y = 0;
                                StructureManager.dropStructure(new Structure(1212), getGroundedPosition(positions[i], 2.5f), 0, (float)(System.Math.Atan((positions[i].x - positions[y].x) / (positions[i].z - positions[y].z)) * (180 / System.Math.PI)), 0, ulong.Parse(player.CSteamID.ToString()), ulong.Parse(player.SteamGroupID.ToString()));
                                StructureManager.dropStructure(new Structure(1212), getGroundedPosition(positions[i], 7.5f), 0, (float)(System.Math.Atan((positions[i].x - positions[y].x) / (positions[i].z - positions[y].z)) * (180 / System.Math.PI)), 0, ulong.Parse(player.CSteamID.ToString()), ulong.Parse(player.SteamGroupID.ToString()));
                            }
                            UnturnedChat.Say(caller, "Enabled visualizing border of zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        }
                        else if (command[3].ToLower() == "off")
                        {
                            List<Vector3> positions = new List<Vector3>();
                            for (int i = 0; i < currentZone.getNodes().Length; i++)
                            {
                                Node node = currentZone.getNodes()[i];
                                Node nextNode;
                                if (i == currentZone.getNodes().Length - 1)
                                    nextNode = currentZone.getNodes()[0];
                                else
                                    nextNode = currentZone.getNodes()[i + 1];
                                Vector3 direction = new Vector3(nextNode.x, nextNode.y, nextNode.z) - new Vector3(node.x, node.y, node.z);
                                float magnitude = direction.magnitude;
                                int x = 0;
                                while (magnitude > x * space)
                                {
                                    positions.Add(new Vector3(node.x, node.y, node.z) + direction.normalized * space * x);
                                    x++;
                                }
                            }
                            foreach (var position in positions)
                            {
                                byte x;
                                byte y;
                                Regions.tryGetCoordinate(new Vector3(position.x, position.y, position.z), out x, out y);
                                List<RegionCoordinate> coordinates = new List<RegionCoordinate>() { new RegionCoordinate(x, y) };
                                List<Transform> transforms = new List<Transform>();
                                StructureManager.getStructuresInRadius(getGroundedPosition(position, 2.5f), 2, coordinates, transforms);
                                StructureManager.getStructuresInRadius(getGroundedPosition(position, 7.5f), 2, coordinates, transforms);
                                foreach (var transform in transforms)
                                {
                                    if (transform.position == getGroundedPosition(position, 2.5f) || transform.position == getGroundedPosition(position, 7.5f))
                                        StructureManager.damage(transform, transform.position, 800, 10, false);
                                }
                            }
                            UnturnedChat.Say(caller, "Disabled visualizing border of zone: " + currentZone.getName(), UnityEngine.Color.cyan);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, "Invalid! Try /zone visualize border <zonename> <on|off> <space>", UnityEngine.Color.red);
                            return;
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, "Invalid! Try /zone visualize <nodes|border> <zonename> <on|off>", UnityEngine.Color.red);
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, "The zone: " + command[2] + " does not exist", UnityEngine.Color.red);
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
                UnturnedChat.Say(caller, "(3) /zone add <zone|node|flag|block|group|message|parameter|heightnode|effect> <zonename> <flag|equip|build|enter|leave|values|isupper> <blockList|message|add|remove|heightoffset> <group|effect>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(4) /zone remove <zone|node|flag|block|group|message|parameter|heightnode|effect> <zonename> <node|flag|equip|build|enter|leave|values|isupper> <blockList|messageNum|add|remove> <group|effect>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(5) /zone replace <zone|node> <zonename> <newzonename|node>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(6) /zone list <zone|zones|nodes|flags|blocklists|groups|messages|parameters|heightnodes|effects> <zonename>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(7) /zone flags", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(8) /zone blockList <add|remove|list|addItem|removeItem> <equip|build> <blockList> <itemID>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(9) /zone <visualize|show> <nodes|border> <zonename> <on|off> <space>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(10) /zone inzone", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(11) /zone getpos <playername>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(12) /zone tp node <zonename> <node>", UnityEngine.Color.cyan);
                return;
            } else
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
        }

        private Vector3 getGroundedPosition(Vector3 point, float offset)
        {
            return new Vector3(point.x, LevelGround.getHeight(point) + offset, point.z);
        }
    }
}
