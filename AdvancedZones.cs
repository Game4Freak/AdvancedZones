﻿using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Game4Freak.AdvancedZones
{
    public class AdvancedZones : RocketPlugin<AdvancedZonesConfiguration>
    {
        public static AdvancedZones Inst;
        public static AdvancedZonesConfiguration Conf;
        public const string VERSION = "1.0.0";
        public string newVersion = null;
        private int frame = 10;
        private Dictionary<string, Vector3> lastPosition;
        private bool notifyUpdate = false;
        // Events
        public delegate void onZoneLeaveHandler(UnturnedPlayer player, Zone zone, Vector3 lastPos);
        public static event onZoneLeaveHandler onZoneLeave;

        public delegate void onZoneEnterHandler(UnturnedPlayer player, Zone zone, Vector3 lastPos);
        public static event onZoneEnterHandler onZoneEnter;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"noBuild","You are not allowed to build {1} in the Zone {0}"},
                    {"noDamage","You can't damage Structures in the Zone {0}"},
                    {"noVehicleDamage","You can't damage Vehicles in the Zone {0}"},
                    {"noPvP","You can't damage other Players in the Zone {0}"},
                    {"noAnimalDamage","You can't damage Animals in the Zone {0}"},
                    {"noZombieDamage","You can't damage Zombies in the Zone {0}"},
                    {"noTireDamage","You can't damage Vehicle Tires in the Zone {0}"},
                    {"noVehicleCarjack","You are not allowed to Carjack in the Zone {0}"},
                    {"noVehicleSiphoning","You are not allowed to siphone Vehicles in the Zone {0}"},
                    {"noLockpick","You are not allowed to lockpick Vehicles in the Zone {0}"},
                    {"noItemEquip","You are not allowed to use Item {1} in the Zone {0}"},
                    {"noEnter","You are not allowed to enter the Zone {0}"},
                    {"noLeave","You are not allowed to leave the Zone {0}"},
                };
            }
        }

        /**
         * TODO:
         * Translations
         * 
         * NOTES:
         * "IGNORE" in Buildables List --> all other listed ids - ignored ids
         * "ALL" in Buildables List --> all barricades + all structures
         * permissions with zonenames for custom override (eg. advancedzones.override.build.testZone for building in the zone testZone)
         * DEBUG: UnturnedChat.Say("");
         * 
         * IMPORTANT:
         * check on version > 0.7.0.0 if prior version was 0.7.0.0 or later for right xml upgrade
         * ctrl + f and search for IMPORTANT
         **/

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            Logger.Log("AdvancedZones v" + VERSION);

            WebClient client = new WebClient();
            try
            {
                newVersion = client.DownloadString("http://pastebin.com/raw/CnLNQehG");
            }
            catch (WebException e)
            {
                Logger.Log("No connection to version-check");
            }
            if (newVersion != null)
            {
                if (compareVersion(newVersion, VERSION))
                {
                    Logger.Log("A new AdvancedZones version (" + newVersion + ") is available !!!");
                    notifyUpdate = true;
                }
            }

            // Update config
            if (compareVersion(VERSION, Conf.version))
            {
                updateConfig();
                Conf.version = VERSION;
                Configuration.Save();
            }

            // Init
            onZoneLeave += onZoneLeft;
            onZoneEnter += onZoneEntered;

            lastPosition = new Dictionary<string, Vector3>();
            foreach (var splayer in Provider.clients)
            {
                onPlayerConnection(UnturnedPlayer.FromSteamPlayer(splayer));
            }
            
            // Enter / Leave
            U.Events.OnPlayerConnected += onPlayerConnection;
            U.Events.OnPlayerDisconnected += onPlayerDisconnection;
            // Block Damage
            BarricadeManager.onDamageBarricadeRequested += onBarricadeDamage;
            StructureManager.onDamageStructureRequested += onStructureDamage;
            VehicleManager.onVehicleLockpicked += onVehicleLockpick;
            VehicleManager.onDamageVehicleRequested += onVehicleDamage;
            DamageTool.damagePlayerRequested += onPlayerDamage;
            VehicleManager.onDamageTireRequested += onTireDamage;
            VehicleManager.onVehicleCarjacked += onVehicleCarjack;
            DamageTool.damageAnimalRequested += onAnimalDamage;
            DamageTool.damageZombieRequested += onZombieDamage;
            // Block Steal
            VehicleManager.onSiphonVehicleRequested += onVehicleSiphoning;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested += onBarricadeDeploy;
            StructureManager.onDeployStructureRequested += onStructureDepoly;
        }

        protected override void Unload()
        {
            lastPosition.Clear();

            // Init
            onZoneLeave -= onZoneLeft;
            onZoneEnter -= onZoneEntered;
            // Enter / Leave
            U.Events.OnPlayerConnected -= onPlayerConnection;
            U.Events.OnPlayerDisconnected -= onPlayerDisconnection;
            // Block Damage
            BarricadeManager.onDamageBarricadeRequested -= onBarricadeDamage;
            StructureManager.onDamageStructureRequested -= onStructureDamage;
            VehicleManager.onVehicleLockpicked -= onVehicleLockpick;
            VehicleManager.onDamageVehicleRequested -= onVehicleDamage;
            DamageTool.damagePlayerRequested -= onPlayerDamage;
            VehicleManager.onDamageTireRequested -= onTireDamage;
            VehicleManager.onVehicleCarjacked -= onVehicleCarjack;
            DamageTool.damageAnimalRequested -= onAnimalDamage;
            DamageTool.damageZombieRequested -= onZombieDamage;
            // Block Steal
            VehicleManager.onSiphonVehicleRequested -= onVehicleSiphoning;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested -= onBarricadeDeploy;
            StructureManager.onDeployStructureRequested -= onStructureDepoly;
        }

        private void updateConfig()
        {
            // Convert config to new config style IMPORTANT: remove upper part and clearing lists for the next update
            if (compareVersion("0.7.0.0", Conf.version))
            {
                Logger.Log("Converting old Xml layout into the new one");

                for (int x = 0; x < Conf.BlockedBuildablesListNames.Count; x++)
                {
                    Conf.BuildBlocklists.Add(new BuildBlocklist(Conf.BlockedBuildablesListNames.ElementAt(x)));
                    foreach (var itemID in Conf.BlockedBuildables.ElementAt(x))
                    {
                        Conf.BuildBlocklists.ElementAt(x).addItem(itemID);
                    }
                }

                for (int x = 0; x < Conf.BlockedEquipListNames.Count; x++)
                {
                    Conf.EquipBlocklists.Add(new EquipBlocklist(Conf.BlockedEquipListNames.ElementAt(x)));
                    foreach (var itemID in Conf.BlockedEquip.ElementAt(x))
                    {
                        Conf.EquipBlocklists.ElementAt(x).addItem(itemID);
                    }
                }

                Configuration.Save();

                for (int x = 0; x < Conf.ZoneNames.Count; x++)
                {
                    Zone temp = new Zone(Conf.ZoneNames.ElementAt(x));
                    foreach (var n in Conf.ZoneNodes.ElementAt(x))
                    {
                        temp.addNode(new Node(n[0], n[1], n[2]));
                    }
                    foreach (var f in Conf.ZoneFlags.ElementAt(x))
                    {
                        temp.addFlag(Zone.flagTypes[f]);
                    }
                    foreach (var bE in Conf.ZoneBlockedEquip.ElementAt(x))
                    {
                        temp.addEquipBlocklist(bE);
                    }
                    foreach (var bB in Conf.ZoneBlockedBuildables.ElementAt(x))
                    {
                        temp.addBuildBlocklist(bB);
                    }
                    foreach (var eAG in Conf.ZoneEnterAddGroups.ElementAt(x))
                    {
                        temp.addEnterAddGroup(eAG);
                    }
                    foreach (var eRG in Conf.ZoneEnterRemoveGroups.ElementAt(x))
                    {
                        temp.addEnterRemoveGroup(eRG);
                    }
                    foreach (var lAG in Conf.ZoneLeaveAddGroups.ElementAt(x))
                    {
                        temp.addLeaveAddGroup(lAG);
                    }
                    foreach (var lRG in Conf.ZoneLeaveRemoveGroups.ElementAt(x))
                    {
                        temp.addLeaveRemoveGroup(lRG);
                    }
                    foreach (var eM in Conf.ZoneEnterMessages.ElementAt(x))
                    {
                        temp.addEnterMessage(eM);
                    }
                    foreach (var lM in Conf.ZoneLeaveMessages.ElementAt(x))
                    {
                        temp.addLeaveMessage(lM);
                    }
                    Conf.Zones.Add(temp);
                    x++;
                }
            }
        }

        private void Update()
        {
            // TODO: set with command
            frame++;
            if (frame % 10 != 0) return;

            foreach (var splayer in Provider.clients)
            {
                Vector3 lastPos;
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(splayer);
                if (player == null) return;
                // Enter / Leave region
                if (!lastPosition.TryGetValue(player.Id, out lastPos))
                {
                    onPlayerConnection(player);
                }
                if (!lastPos.Equals(player.Position))
                {
                    List<string> lastZoneNames = new List<string>();
                    foreach (var zone in getPositionZones(lastPos))
                    {
                        lastZoneNames.Add(zone.getName());
                    }
                    List<string> currentZoneNames = new List<string>();
                    foreach (var zone in getPositionZones(player.Position))
                    {
                        currentZoneNames.Add(zone.getName());
                    }
                    foreach (var zoneName in lastZoneNames.Except(currentZoneNames))
                    {
                        // Leaving
                        onZoneLeave(player, getZoneByName(zoneName), lastPos);
                    }
                    foreach (var zoneName in currentZoneNames.Except(lastZoneNames))
                    {
                        // Entering
                        onZoneEnter(player, getZoneByName(zoneName), lastPos);
                    }
                }
                lastPosition[player.Id] = player.Position;

                // Player Equip
                if (player?.Player?.equipment != null && player.Player.equipment.isSelected && playerInZoneType(player, Zone.flagTypes[Zone.noItemEquip]))
                {
                    onPlayerEquiped(player.Player, player.Player.equipment);
                }
            }

            // infiniteGenerator flag
            InteractableGenerator[] generators = FindObjectsOfType<InteractableGenerator>();
            foreach (var generator in generators)
            {
                if (transformInZoneType(generator.transform, Zone.flagTypes[Zone.infiniteGenerator]))
                {
                    if (generator.fuel < generator.capacity - 10)
                    {
                        BarricadeManager.sendFuel(generator.transform, generator.capacity);
                    }
                }
            }

            // noZombie flag
            if (ZombieManager.regions != null)
            {
                foreach (ZombieRegion t in ZombieManager.regions.Where(t => t.zombies != null))
                {
                    foreach (var zombie in t.zombies.Where(z => z != null && z.transform?.position != null))
                    {
                        if (zombie.isDead) continue;
                        if (!transformInZoneType(zombie.transform, Zone.flagTypes[Zone.noZombie])) continue;
                        zombie.gear = 0;
                        zombie.isDead = true;
                        ZombieManager.sendZombieDead(zombie, new Vector3(0, 0, 0));
                    }
                }
            }
        }

        private void onZoneLeft(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            if (zone.hasFlag(Zone.flagTypes[Zone.noLeave]))
            {
                if (!player.HasPermission("advancedzones.override.noleave") && !player.HasPermission("advancedzones.override.noleave." + zone.getName().ToLower()))
                {
                    InteractableVehicle vehicle = player?.Player?.movement?.getVehicle();
                    if (vehicle != null)
                    {
                        vehicle.forceRemovePlayer(out byte seat, player.CSteamID, out Vector3 point, out byte angle);
                    }
                    player.Player.teleportToLocationUnsafe(new Vector3(lastPos.x, lastPos.y - 0.5f, lastPos.z), player.Rotation);
                    if (Conf.NotifyNoLeave) UnturnedChat.Say(player, Translate("noLeave", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                    return;
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.leaveMessage]))
            {
                foreach (var leaveMessage in zone.getLeaveMessages())
                {
                    UnturnedChat.Say(player, leaveMessage, UnityEngine.Color.green);
                }               
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.leaveRemoveGroup]))
            {
                foreach (var group in zone.getLeaveRemoveGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.leaveAddGroup]))
            {
                foreach (var group in zone.getLeaveAddGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.leaveAddEffect]))
            {
                foreach (var effect in zone.getLeaveAddEffects())
                {
                    player.TriggerEffect(effect);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.leaveRemoveEffect]))
            {
                foreach (var effect in zone.getLeaveRemoveEffects())
                {
                    EffectManager.askEffectClearByID(effect, player.CSteamID);
                }
            }
        }

        private void onZoneEntered(UnturnedPlayer player, Zone zone, Vector3 lastPos)
        {
            
            if (zone.hasFlag(Zone.flagTypes[Zone.noEnter]))
            {
                if (!player.HasPermission("advancedzones.override.noenter") && !player.HasPermission("advancedzones.override.noenter." + zone.getName().ToLower()))
                {
                    InteractableVehicle vehicle = player?.Player?.movement?.getVehicle();
                    if (vehicle != null)
                    {
                        vehicle.forceRemovePlayer(out byte seat, player.CSteamID, out Vector3 point, out byte angle);
                    }
                    player.Player.teleportToLocationUnsafe(new Vector3(lastPos.x, lastPos.y - 0.5f, lastPos.z), player.Rotation);
                    
                    if (Conf.NotifyNoEnter) UnturnedChat.Say(player, Translate("noEnter", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        
                    return;
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.enterMessage]))
            {
                foreach (var enterMessage in zone.getEnterMessages())
                {
                    UnturnedChat.Say(player, enterMessage, UnityEngine.Color.green);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.enterRemoveGroup]))
            {
                foreach (var group in zone.getEnterRemoveGroups())
                {
                    R.Permissions.RemovePlayerFromGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.enterAddGroup]))
            {
                foreach (var group in zone.getEnterAddGroups())
                {
                    R.Permissions.AddPlayerToGroup(group, player);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.enterAddEffect]))
            {
                foreach (var effect in zone.getEnterAddEffects())
                {
                    player.TriggerEffect(effect);
                }
            }
            if (zone.hasFlag(Zone.flagTypes[Zone.enterRemoveEffect]))
            {
                foreach (var effect in zone.getEnterRemoveEffects())
                {
                    EffectManager.askEffectClearByID(effect, player.CSteamID);
                }
            }
        }

        private void onPlayerDisconnection(UnturnedPlayer player)
        {
            lastPosition.Remove(player.Id);
            foreach (var zone in getPositionZones(player.Position))
            {
                onZoneLeave(player, zone, player.Position);
            }
        }

        private void onPlayerConnection(UnturnedPlayer player)
        {
            lastPosition.Add(player.Id, player.Position);
            foreach (var zone in getPositionZones(player.Position))
            {
                onZoneEnter(player, zone, player.Position);
            }
            if (player.HasPermission("advancedzones") && notifyUpdate)
            {
                UnturnedChat.Say(player, "A new AdvancedZones version (" + newVersion + ") is available !!! Yours is: " + VERSION, Color.red);
                notifyUpdate = false;
            }
        }

        private void onVehicleCarjack(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow, ref Vector3 force, ref Vector3 torque)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleCarjack]) && !player.HasPermission("advancedzones.override.carjack"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleCarjack]) && !player.HasPermission(("advancedzones.override.carjack." + zone.getName()).ToLower()))
                    {
                        if(Conf.NotifyCarjack)
                            UnturnedChat.Say(player, Translate("noVehicleCarjack", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        allow = false;
                        return;
                    }
                }
            }
        }

        private void onVehicleSiphoning(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleSiphoning]) && !player.HasPermission("advancedzones.override.siphoning"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleSiphoning]) && !player.HasPermission(("advancedzones.override.siphoning." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifySiphoning)
                            UnturnedChat.Say(player, Translate("noVehicleSiphoning", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }

        private void onZombieDamage(ref DamageZombieParameters parameters, ref bool canDamage)
        {
            if (transformInZoneType(parameters.zombie.transform, Zone.flagTypes[Zone.noZombieDamage]))
            {
                UnturnedPlayer player = null;
                if (parameters.instigator is CSteamID)
                    player = UnturnedPlayer.FromCSteamID((CSteamID)parameters.instigator);
                if (parameters.instigator is Player)
                    player = UnturnedPlayer.FromPlayer((Player)parameters.instigator);

                if (player != null)
                {
                    List<Zone> currentZones = getPositionZones(parameters.zombie.transform.position);
                    foreach (var zone in currentZones)
                    {
                        if (zone.hasFlag(Zone.flagTypes[Zone.noZombieDamage]) && !player.HasPermission(("advancedzones.override.zombiedamage." + zone.getName()).ToLower()))
                        {
                            if (Conf.NotifyDamageZombie)
                                UnturnedChat.Say(player, Translate("noZombieDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        }

                    }
                }
                canDamage = false;
            }
        }

        private void onAnimalDamage(ref DamageAnimalParameters parameters, ref bool canDamage)
        {
            if (transformInZoneType(parameters.animal.transform, Zone.flagTypes[Zone.noAnimalDamage]))
            {
                UnturnedPlayer player = null;
                if (parameters.instigator is CSteamID)
                    player = UnturnedPlayer.FromCSteamID((CSteamID)parameters.instigator);
                if (parameters.instigator is Player)
                    player = UnturnedPlayer.FromPlayer((Player)parameters.instigator);

                if (player != null)
                {
                    List<Zone> currentZones = getPositionZones(parameters.animal.transform.position);
                    foreach (var zone in currentZones)
                    {
                        if (zone.hasFlag(Zone.flagTypes[Zone.noAnimalDamage]) && !player.HasPermission(("advancedzones.override.animaldamage." + zone.getName()).ToLower()))
                        {
                            if (Conf.NotifyDamageAnimal)
                                UnturnedChat.Say(player, Translate("noAnimalDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        }
                
                    }
                }
                canDamage = false;
            }
        }

        private void onTireDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noTireDamage]) && !player.HasPermission("advancedzones.override.tiredamage"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noTireDamage]) && !player.HasPermission(("advancedzones.override.tiredamage." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifyDamageTire)
                            UnturnedChat.Say(player, Translate("noTireDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }

        private void onPlayerEquiped(Player player, PlayerEquipment equipment)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);
            if (!uPlayer.HasPermission("advancedzones.override.equip"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noItemEquip]))
                    {
                        if (!uPlayer.HasPermission(("advancedzones.override.equip." + zone.getName()).ToLower()))
                        {
                            List<EquipBlocklist> currentEquipBlocklists = getEquipBlocklists(zone.getEquipBlocklists());
                            List<EquipBlocklist> currentIgnoredEquipBlocklists = new List<EquipBlocklist>();
                            for (int i = 0; i < currentEquipBlocklists.Count; i++)
                            {
                                if (currentEquipBlocklists.ElementAt(i).name.ToLower().Contains("ignore"))
                                {
                                    currentIgnoredEquipBlocklists.Add(currentEquipBlocklists.ElementAt(i));
                                    currentEquipBlocklists.Remove(currentEquipBlocklists.ElementAt(i));
                                }
                            }

                            if (currentIgnoredEquipBlocklists.Count > 0)
                            {
                                foreach (var ignoredBlocklist in currentIgnoredEquipBlocklists)
                                {
                                    if (ignoredBlocklist.hasItem(equipment.asset.id))
                                        return;
                                }
                            }
                            string itemName = "Invalid Equipment";
                            if (equipment?.asset != null)
                            {
                                itemName = equipment.asset.name;
                            }
                            foreach (var blocklist in currentEquipBlocklists)
                            {
                                if (blocklist.name == "ALL")
                                {
                                    if (Conf.NotifyItemEquip)
                                        UnturnedChat.Say(uPlayer, Translate("noItemEquip", zone.name, itemName), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    equipment.dequip();
                                    return;
                                }
                            }

                            foreach (var blocklist in currentEquipBlocklists)
                            {
                                if (blocklist.hasItem(equipment.asset.id))
                                {
                                    if (Conf.NotifyItemEquip)
                                        UnturnedChat.Say(uPlayer, Translate("noItemEquip", zone.name, itemName), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    equipment.dequip();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onPlayerDamage(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(parameters.player);
            UnturnedPlayer oponent = UnturnedPlayer.FromCSteamID(parameters.killer);
            if (parameters.cause == EDeathCause.BLEEDING || parameters.cause == EDeathCause.BONES || parameters.cause == EDeathCause.BREATH || parameters.cause == EDeathCause.BURNING || parameters.cause == EDeathCause.FOOD || parameters.cause == EDeathCause.FREEZING 
                || parameters.cause == EDeathCause.INFECTION || parameters.cause == EDeathCause.ARENA || parameters.cause == EDeathCause.KILL || parameters.cause == EDeathCause.SUICIDE || parameters.cause == EDeathCause.WATER)
            {
                return;
            }
            if (parameters.cause == EDeathCause.LANDMINE || parameters.cause == EDeathCause.SHRED || parameters.cause == EDeathCause.SENTRY || parameters.cause == EDeathCause.VEHICLE || parameters.cause == EDeathCause.ROADKILL || parameters.cause == EDeathCause.ACID || parameters.cause == EDeathCause.BOULDER)
            {
                if (playerInZoneType(player, Zone.flagTypes[Zone.noPlayerDamage]))
                {
                    if (parameters.cause == EDeathCause.VEHICLE)
                    {
                        if (player.IsInVehicle)
                            return;
                    }
                    shouldAllow = false;
                    return;
                }
                else if (playerInZoneType(player, Zone.flagTypes[Zone.noPvP]) && (parameters.cause != EDeathCause.ACID && parameters.cause != EDeathCause.BOULDER))
                {
                    if (parameters.cause == EDeathCause.VEHICLE)
                    {
                        if (player.IsInVehicle)
                            return;
                    }
                    shouldAllow = false;
                    return;
                }
                else
                {
                    return;
                }
            } 
            if ((oponent == null || oponent.Player == null) && playerInZoneType(player, Zone.flagTypes[Zone.noPlayerDamage]))
            {
                if (parameters.cause == EDeathCause.ZOMBIE)
                {
                    player.Infection = 0;
                }
                shouldAllow = false;
                return;
            }
            else if (oponent == null || oponent.Player == null)
            {
                return;
            }
            if (((playerInZoneType(player, Zone.flagTypes[Zone.noPlayerDamage]) || playerInZoneType(oponent, Zone.flagTypes[Zone.noPlayerDamage]))
                 && !oponent.HasPermission("advancedzones.override.playerdamage")) || ((playerInZoneType(player, Zone.flagTypes[Zone.noPvP]) ||
                 playerInZoneType(oponent, Zone.flagTypes[Zone.noPvP])) && !oponent.HasPermission("advancedzones.override.pvp")))
            {
                List<Zone> currentZones = getPositionZones(parameters.player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noPlayerDamage]) && !oponent.HasPermission(("advancedzones.override.pvp." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifyDamagePlayer)
                            UnturnedChat.Say(oponent, Translate("noPvP", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        shouldAllow = false;
                        return;
                    }
                    else if (zone.hasFlag(Zone.flagTypes[Zone.noPvP]) && !oponent.HasPermission(("advancedzones.override.pvp." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifyDamagePlayer)
                            UnturnedChat.Say(oponent, Translate("noPvP", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }

        private void onStructureDepoly(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
            if (player == null) return;
            if (!player.HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBuild]))
                    {
                        if (!player.HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
                        {
                            List<BuildBlocklist> currentBuildBlocklists = getBuildBlocklists(zone.getBuildBlocklists());
                            List<BuildBlocklist> currentIgnoredBuildBlocklists = new List<BuildBlocklist>();
                            for (int i = 0; i < currentBuildBlocklists.Count; i++)
                            {
                                if (currentBuildBlocklists.ElementAt(i).name.ToLower().Contains("ignore"))
                                {
                                    currentIgnoredBuildBlocklists.Add(currentBuildBlocklists.ElementAt(i));
                                    currentBuildBlocklists.Remove(currentBuildBlocklists.ElementAt(i));
                                }
                            }

                            if (currentIgnoredBuildBlocklists.Count > 0)
                            {
                                foreach (var ignoredBlocklist in currentIgnoredBuildBlocklists)
                                {
                                    if (ignoredBlocklist.hasItem(asset.id))
                                        return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.name == "ALL")
                                {
                                    if (Conf.NotifyBuild)
                                        UnturnedChat.Say(player, Translate("noBuild", zone.name, asset.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    shouldAllow = false;
                                    return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.hasItem(asset.id))
                                {
                                    if (Conf.NotifyBuild)
                                        UnturnedChat.Say(player, Translate("noBuild", zone.name, asset.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    shouldAllow = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onBarricadeDeploy(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(owner));
            if (player == null) return;
            if (!player.HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBuild]))
                    {
                        if (!player.HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
                        {
                            List<BuildBlocklist> currentBuildBlocklists = getBuildBlocklists(zone.getBuildBlocklists());
                            List<BuildBlocklist> currentIgnoredBuildBlocklists = new List<BuildBlocklist>();
                            for (int i = 0; i < currentBuildBlocklists.Count; i++)
                            {
                                if (currentBuildBlocklists.ElementAt(i).name.ToLower().Contains("ignore"))
                                {
                                    currentIgnoredBuildBlocklists.Add(currentBuildBlocklists.ElementAt(i));
                                    currentBuildBlocklists.Remove(currentBuildBlocklists.ElementAt(i));
                                }
                            }

                            if (currentIgnoredBuildBlocklists.Count > 0)
                            {
                                foreach (var ignoredBlocklist in currentIgnoredBuildBlocklists)
                                {
                                    if (ignoredBlocklist.hasItem(asset.id))
                                        return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.name == "ALL")
                                {
                                    if (Conf.NotifyBuild)
                                        UnturnedChat.Say(player, Translate("noBuild", zone.name, asset.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    shouldAllow = false;
                                    return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.hasItem(asset.id))
                                {
                                    if (Conf.NotifyBuild)
                                        UnturnedChat.Say(player, Translate("noBuild", zone.name, asset.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    shouldAllow = false;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onVehicleLockpick(InteractableVehicle vehicle, Player instigatingPlayer, ref bool allow)
        {
            if (instigatingPlayer == null || vehicle?.transform == null) return;
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);

            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noLockpick]) && !player.HasPermission("advancedzones.override.lockpick"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noLockpick]) && !player.HasPermission(("advancedzones.override.lockpick." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifyLockpick)
                            UnturnedChat.Say(player, Translate("noLockpick", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        allow = false;
                    }
                }
            }
            else
            {
                allow = true;
            }
        }

        private void onVehicleDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, ref ushort pendingTotalDamage, ref bool canRepair, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
            if (player == null || vehicle?.transform == null) return;

            if ((transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleDamage]) && !player.HasPermission("advancedzones.override.vehicledamage")) && pendingTotalDamage > 0)
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleDamage]) && !player.HasPermission(("advancedzones.override.vehicledamage." + zone.getName()).ToLower()))
                    {
                        if (Conf.NotifyDamageVehicle)
                            UnturnedChat.Say(player, Translate("noVehicleDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                        shouldAllow = false;
                    }
                }
            }
            else
            {
                shouldAllow = true;
            }
        }

        private void onStructureDamage(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            
            if (transformInZoneType(structureTransform, Zone.flagTypes[Zone.noDamage]))
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                if (player != null)
                {
                    if (!player.HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        List<Zone> currentZones = getPositionZones(structureTransform.transform.position);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.flagTypes[Zone.noDamage]) && !player.HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                            {
                                if (Conf.NotifyDamageBuild)
                                    UnturnedChat.Say(player, Translate("noDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                shouldAllow = false;
                            }
                        }
                    }
                }
                else if (damageOrigin.ToString() == "Bullet_Explosion"
                || damageOrigin.ToString() == "Charge_Explosion"
                || damageOrigin.ToString() == "Food_Explosion"
                || damageOrigin.ToString() == "Rocket_Explosion"
                || damageOrigin.ToString() == "Sentry"
                || damageOrigin.ToString() == "Trap_Explosion"
                || damageOrigin.ToString() == "Vehicle_Explosion"
                || damageOrigin.ToString() == "Zombie_Swipe")
                {
                    shouldAllow = false;
                }
            }
        }

        private void onBarricadeDamage(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(barricadeTransform, Zone.flagTypes[Zone.noDamage]))
            {
                UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                if (player != null)
                {
                    if (!player.HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        if (barricadeTransform.name.ToString() != "1102"
                                && barricadeTransform.name.ToString() != "1101"
                                && barricadeTransform.name.ToString() != "1393"
                                && barricadeTransform.name.ToString() != "1241")
                        {
                            List<Zone> currentZones = getPositionZones(barricadeTransform.transform.position);
                            foreach (var zone in currentZones)
                            {
                                if (zone.hasFlag(Zone.flagTypes[Zone.noDamage]) && !player.HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                                {
                                    if (Conf.NotifyDamageBuild)
                                        UnturnedChat.Say(player, Translate("noDamage", zone.name), UnturnedChat.GetColorFromName(Conf.NotificationColor, Color.red));
                                    shouldAllow = false;
                                }
                            }
                        }
                    }
                }
                else if ((damageOrigin.ToString() == "Bullet_Explosion"
                    || (damageOrigin.ToString() == "Charge_Explosion")
                    || damageOrigin.ToString() == "Food_Explosion"
                    || damageOrigin.ToString() == "Rocket_Explosion"
                    || damageOrigin.ToString() == "Sentry"
                    || damageOrigin.ToString() == "Trap_Explosion"
                    || damageOrigin.ToString() == "Vehicle_Explosion"
                    || damageOrigin.ToString() == "Zombie_Swipe") &&
                    (barricadeTransform.name.ToString() != "1102"
                    && barricadeTransform.name.ToString() != "1101"
                    && barricadeTransform.name.ToString() != "1393"
                    && barricadeTransform.name.ToString() != "1241"))
                {
                    shouldAllow = false;
                }
            }
        }

        private bool compareVersion(string version1, string version2)
        {
            return int.Parse(version1.Replace(".", "")) > int.Parse(version2.Replace(".", ""));
        }

        public bool playerInAZone(UnturnedPlayer player)
        {
            return getPositionZones(player.Position).Count > 0;
        }

        public bool playerInZoneType(UnturnedPlayer player, string type)
        {
            if (player == null) return false;

            List<Zone> zones = getPositionZones(player.Position);
            if (zones.Count == 0)
            {
                return false;
            } else
            {
                foreach (var z in zones)
                {
                    if (z.hasFlag(type))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public List<Zone> getPositionZones(Vector3 position)
        {
            List<Zone> zones = new List<Zone>();
            foreach (var z in Conf.Zones)
            {
               if(isPositionInZone(position, z)) zones.Add(z);
            }
            return zones;
        }

        public bool isPositionInZone(Vector3 position, Zone zone)
        {
            if (zone.isReady())
            {
                float playerX = position.x;
                float playerZ = position.z;
                float playerY = position.y;

                HeightNode[] heightNodes = zone.GetHeightNodes();

                Node[] zoneNodes = zone.getNodes();

                int j = zoneNodes.Length - 1;
                bool oddNodes = false;

                for (int i = 0; i < zoneNodes.Length; i++)
                {
                    if ((zoneNodes[i].z < playerZ && zoneNodes[j].z >= playerZ
                         || zoneNodes[j].z < playerZ && zoneNodes[i].z >= playerZ)
                         && (zoneNodes[i].x <= playerX || zoneNodes[j].x <= playerX))
                    {
                        if (zoneNodes[i].x + (playerZ - zoneNodes[i].z) / (zoneNodes[j].z - zoneNodes[i].z) * (zoneNodes[j].x - zoneNodes[i].x) < playerX)
                        {
                            oddNodes = !oddNodes;
                        }
                    }
                    j = i;
                }
                if (oddNodes)
                {
                    if (heightNodes[0] != null && heightNodes[1] != null)
                    {
                        if (heightNodes[0].isUpper)
                        {
                            if (playerY < heightNodes[0].y && playerY > heightNodes[1].y)
                            {
                                return true;
                            }
                        }
                    }
                    else if (heightNodes[0] != null)
                    {
                        if (heightNodes[0].isUpper && playerY < heightNodes[0].y)
                            return true;
                        else if (!heightNodes[0].isUpper && playerY > heightNodes[0].y)
                            return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool transformInZoneType(Transform transform, string type)
        {

            if (getPositionZones(transform.position).Count == 0)
            {
                return false;
            }
            else
            {
                foreach (var z in getPositionZones(transform.position))
                {
                    if (z.hasFlag(type))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public Zone getZoneByName(string zoneName)
        {
            foreach (var z in Conf.Zones)
            {
                if (z.getName().ToLower() == zoneName.ToLower())
                {
                    return z;
                }
            }
            return null;
        }

        public List<BuildBlocklist> getBuildBlocklists(List<string> blocklistNames)
        {
            List<BuildBlocklist> temp = new List<BuildBlocklist>();
            foreach (var blocklistName in blocklistNames)
            {
                foreach (var blocklist in Conf.BuildBlocklists)
                {
                    if (blocklist.name == blocklistName)
                    {
                        temp.Add(blocklist);
                        break;
                    }
                }
            }
            return temp;
        }

        public List<EquipBlocklist> getEquipBlocklists(List<string> blocklistNames)
        {
            List<EquipBlocklist> temp = new List<EquipBlocklist>();
            foreach (var blocklistName in blocklistNames)
            {
                foreach (var blocklist in Conf.EquipBlocklists)
                {
                    if (blocklist.name == blocklistName)
                    {
                        temp.Add(blocklist);
                        break;
                    }
                }
            }
            return temp;
        }

        public void addCustomFlag(string flagName, int flagID, string flagDescription)
        {
            foreach (var customFlag in Conf.CustomFlags)
            {
                if (customFlag.name == flagName)
                    return;
            }
            Conf.CustomFlags.Add(new CustomFlag(flagName, flagID, flagDescription));
            Configuration.Save();
            return;
        }

        public void removeCustomFlag(string flagName)
        {
            foreach (var customFlag in Conf.CustomFlags.ToList())
            {
                if (customFlag.name == flagName)
                {
                    Conf.CustomFlags.Remove(customFlag);
                    Configuration.Save();
                }
            }
        }

        public List<CustomFlag> GetCustomFlags()
        {
            return Conf.CustomFlags;
        }
    }
}