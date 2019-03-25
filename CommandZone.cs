using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Core;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            get { return "<add|remove|list|inzone|getpos|tp|flags> <zone|node|flag|zones|nodes|flags|playername> <zonename> <node|flag>"; }
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
            if (command.Length != 1 && command.Length != 2 && command.Length != 3 && command.Length != 4)
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
            // ADD
            else if (command[0].ToLower() == "add")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add <zone|node|flag> <zonename> <node|flag>", UnityEngine.Color.red);
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
                        if (z == zoneName)
                        {
                            isValid = false;
                        }
                    }
                    if (isValid)
                    {
                        AdvancedZones.Instance.Configuration.Instance.ZoneNames.Add(zoneName);
                        AdvancedZones.Instance.Configuration.Instance.ZoneNodes.Add(new List<float[]>());
                        AdvancedZones.Instance.Configuration.Instance.ZoneFlags.Add(new List<int>());
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
                        if (z == command[2])
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
                            if (z == command[2])
                            {
                                AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x).Add(flagNum);
                                AdvancedZones.Instance.Configuration.Save();
                                UnturnedChat.Say(caller, "Added flag " + Zone.flagTypes[flagNum] + " to zone: " + z, UnityEngine.Color.cyan);
                                // TODO PLAYER DAMAGE
                                if (Zone.noPlayerDamage == flagNum)
                                {
                                    UnturnedChat.Say(caller, "The flag " + Zone.flagTypes[Zone.noPlayerDamage] + " is work in progress");
                                }
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
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone add <zone|node|flag> <zonename> <node|flag>", UnityEngine.Color.red);
                    return;
                }
            }
            // REMOVE
            else if (command[0].ToLower() == "remove")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag> <zonename> <node|flag>", UnityEngine.Color.red);
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
                        if (z == command[2])
                        {
                            AdvancedZones.Instance.Configuration.Instance.ZoneNames.Remove(z);
                            AdvancedZones.Instance.Configuration.Instance.ZoneNodes.Remove(AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x));
                            AdvancedZones.Instance.Configuration.Instance.ZoneFlags.Remove(AdvancedZones.Instance.Configuration.Instance.ZoneFlags.ElementAt(x));
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
                            if (z == command[2])
                            {
                                if (nodeNum < AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Count())
                                {
                                    AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).Remove(AdvancedZones.Instance.Configuration.Instance.ZoneNodes.ElementAt(x).ElementAt(nodeNum));
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
                            if (z == command[2])
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
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone remove <zone|node|flag> <zonename> <node|flag>", UnityEngine.Color.red);
                    return;
                }
            }
            // LIST
            else if (command[0].ToLower() == "list")
            {
                if (command.Length < 2)
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags> <zonename>", UnityEngine.Color.red);
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
                        if (z.getName() == command[2])
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
                        if (z.getName() == command[2])
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
                        if (z.getName() == command[2])
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
                else
                {
                    UnturnedChat.Say(caller, "Invalid! Try /zone list <zone|zones|nodes|flags> <zonename>", UnityEngine.Color.red);
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
                            if (z == command[2])
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
                    //TODO PLAYER DAMAGE
                    if (Zone.noPlayerDamage == i)
                    {
                        message += Zone.flagTypes[i] + " (WIP! " + Zone.flagDescs[i] + "), ";
                    } else
                    {
                        message += Zone.flagTypes[i] + " (" + Zone.flagDescs[i] + "), ";
                    }
                }
                UnturnedChat.Say(caller, message , UnityEngine.Color.cyan);
                return;
            }
            else if (command[0].ToLower() == "help")
            {
                UnturnedChat.Say(caller, "These are all commands of the AdvancedZones-Plugin", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(1) /zone help", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(2) /zone add <zone|node|flag> <zonename> <flag>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(3) /zone remove <zone|node|flag> <zonename> <node|flag>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(4) /zone list <zone|zones|nodes|flags> <zonename>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(5) /zone flags", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(6) /zone inzone", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(7) /zone getpos <playername>", UnityEngine.Color.cyan);
                UnturnedChat.Say(caller, "(8) /zone tp node <zonename> <node>", UnityEngine.Color.cyan);
                return;
            } else
            {
                UnturnedChat.Say(caller, "Invalid! Try /zone help or /zone " + Syntax, UnityEngine.Color.red);
                return;
            }
        }
    }
}
