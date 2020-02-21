﻿using Rocket.API;
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
        public static AdvancedZones Instance;
        public const string VERSION = "1.0.1";
        public string newVersion = null;
        private int frame;
        private Dictionary<string, Vector3> lastPosition;
        private bool notifyUpdate = false;
        // Events
        public delegate void onZoneLeaveHandler(UnturnedPlayer player, Zone zone, Vector3 lastPos);
        public static event onZoneLeaveHandler onZoneLeave;

        public delegate void onZoneEnterHandler(UnturnedPlayer player, Zone zone, Vector3 lastPos);
        public static event onZoneEnterHandler onZoneEnter;
       
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
            Instance = this;
            Logger.Log("AdvancedZones v" + VERSION);
            frame = Configuration.Instance.UpdateFrame;
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
            if (compareVersion(VERSION, Configuration.Instance.version))
            {
                updateConfig();
                Configuration.Instance.version = VERSION;
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
            ObjectManager.onDamageObjectRequested += onObjectDamage;
            ResourceManager.onDamageResourceRequested += onResourceDamage;
            VehicleManager.onDamageVehicleRequested += onVehicleDamage;
            DamageTool.damagePlayerRequested += onPlayerDamage;
            VehicleManager.onDamageTireRequested += onTireDamage;
            DamageTool.damageAnimalRequested += onAnimalDamage;
            DamageTool.damageZombieRequested += onZombieDamage;
            // Block Steal Gas
            VehicleManager.onSiphonVehicleRequested += onVehicleSiphoning;
            // Block Lockpick
            VehicleManager.onVehicleLockpicked += onVehicleLockpick;
            // Block Carjack
            VehicleManager.onVehicleCarjacked += onVehicleCarjack;
            // Block Exit Vehicle
            VehicleManager.onExitVehicleRequested += onExitVehicle;
            // Block Enter Vehicle
            VehicleManager.onEnterVehicleRequested += onEnterVehicle;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested += onBarricadeDeploy;
            StructureManager.onDeployStructureRequested += onStructureDeploy;
            // Block Swap Seat
            VehicleManager.onSwapSeatRequested += onSwapSeat;
            // Block Modify Sign
            BarricadeManager.onModifySignRequested += onChangeSign;
            // Block Harvest
            BarricadeManager.onHarvestPlantRequested += onHarvestPlant;
            // Block Salvage
            BarricadeManager.onSalvageBarricadeRequested += onBarricadeSalvage;
            StructureManager.onSalvageStructureRequested += onStructureSalvage;
            // Block Transform
            BarricadeManager.onTransformRequested += onBarricadeTransform;
            StructureManager.onTransformRequested += onStructureTransform;
            // Block Item Spawn
            ItemManager.onServerSpawningItemDrop += onItemSpawn;
            // Block Chat
            UnturnedPlayerEvents.OnPlayerChatted += onPlayerChat;
            // Block Take Item
            ItemManager.onTakeItemRequested += onTakeItem;
            // Block Crafting
            PlayerCrafting.onCraftBlueprintRequested += onPlayerCraft;
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
            ObjectManager.onDamageObjectRequested -= onObjectDamage;
            ResourceManager.onDamageResourceRequested -= onResourceDamage;
            VehicleManager.onDamageVehicleRequested -= onVehicleDamage;
            DamageTool.damagePlayerRequested -= onPlayerDamage;
            VehicleManager.onDamageTireRequested -= onTireDamage;
            DamageTool.damageAnimalRequested -= onAnimalDamage;
            DamageTool.damageZombieRequested -= onZombieDamage;
            // Block Steal Gas
            VehicleManager.onSiphonVehicleRequested -= onVehicleSiphoning;
            // Block Lockpick
            VehicleManager.onVehicleLockpicked -= onVehicleLockpick;
            // Block Carjack
            VehicleManager.onVehicleCarjacked -= onVehicleCarjack;
            // Block Exit Vehicle
            VehicleManager.onExitVehicleRequested -= onExitVehicle;
            // Block Enter Vehicle
            VehicleManager.onEnterVehicleRequested -= onEnterVehicle;
            // Block Buildable
            BarricadeManager.onDeployBarricadeRequested -= onBarricadeDeploy;
            StructureManager.onDeployStructureRequested -= onStructureDeploy;
            // Block Swap Seat
            VehicleManager.onSwapSeatRequested -= onSwapSeat;
            // Block Change Sign
            BarricadeManager.onModifySignRequested -= onChangeSign;
            // Block Harvest
            BarricadeManager.onHarvestPlantRequested -= onHarvestPlant;
            // Block Salvage
            BarricadeManager.onSalvageBarricadeRequested -= onBarricadeSalvage;
            StructureManager.onSalvageStructureRequested -= onStructureSalvage;
            // Block Transform
            BarricadeManager.onTransformRequested -= onBarricadeTransform;
            StructureManager.onTransformRequested -= onStructureTransform;
            // Block Item Spawn
            ItemManager.onServerSpawningItemDrop -= onItemSpawn;
            // Block Chat
            UnturnedPlayerEvents.OnPlayerChatted -= onPlayerChat;
            // Block Take Item
            ItemManager.onTakeItemRequested -= onTakeItem;
            // Block Crafting
            PlayerCrafting.onCraftBlueprintRequested -= onPlayerCraft;
        }

        private void updateConfig()
        {
            // Convert config to new config style IMPORTANT: remove upper part and clearing lists for the next update
            if (compareVersion("0.7.0.0", Configuration.Instance.version))
            {
                Logger.Log("Converting old Xml layout into the new one");

                for (int x = 0; x < Configuration.Instance.BlockedBuildablesListNames.Count; x++)
                {
                    Configuration.Instance.BuildBlocklists.Add(new BuildBlocklist(Configuration.Instance.BlockedBuildablesListNames.ElementAt(x)));
                    foreach (var itemID in Configuration.Instance.BlockedBuildables.ElementAt(x))
                    {
                        Configuration.Instance.BuildBlocklists.ElementAt(x).addItem(itemID);
                    }
                }

                for (int x = 0; x < Configuration.Instance.BlockedEquipListNames.Count; x++)
                {
                    Configuration.Instance.EquipBlocklists.Add(new EquipBlocklist(Configuration.Instance.BlockedEquipListNames.ElementAt(x)));
                    foreach (var itemID in Configuration.Instance.BlockedEquip.ElementAt(x))
                    {
                        Configuration.Instance.EquipBlocklists.ElementAt(x).addItem(itemID);
                    }
                }

                Configuration.Save();

                for (int x = 0; x < Configuration.Instance.ZoneNames.Count; x++)
                {
                    Zone temp = new Zone(Configuration.Instance.ZoneNames.ElementAt(x));
                    foreach (var n in Configuration.Instance.ZoneNodes.ElementAt(x))
                    {
                        temp.addNode(new Node(n[0], n[1], n[2]));
                    }
                    foreach (var f in Configuration.Instance.ZoneFlags.ElementAt(x))
                    {
                        temp.addFlag(Zone.flagTypes[f]);
                    }
                    foreach (var bE in Configuration.Instance.ZoneBlockedEquip.ElementAt(x))
                    {
                        temp.addEquipBlocklist(bE);
                    }
                    foreach (var bB in Configuration.Instance.ZoneBlockedBuildables.ElementAt(x))
                    {
                        temp.addBuildBlocklist(bB);
                    }
                    foreach (var eAG in Configuration.Instance.ZoneEnterAddGroups.ElementAt(x))
                    {
                        temp.addEnterAddGroup(eAG);
                    }
                    foreach (var eRG in Configuration.Instance.ZoneEnterRemoveGroups.ElementAt(x))
                    {
                        temp.addEnterRemoveGroup(eRG);
                    }
                    foreach (var lAG in Configuration.Instance.ZoneLeaveAddGroups.ElementAt(x))
                    {
                        temp.addLeaveAddGroup(lAG);
                    }
                    foreach (var lRG in Configuration.Instance.ZoneLeaveRemoveGroups.ElementAt(x))
                    {
                        temp.addLeaveRemoveGroup(lRG);
                    }
                    foreach (var eM in Configuration.Instance.ZoneEnterMessages.ElementAt(x))
                    {
                        temp.addEnterMessage(eM);
                    }
                    foreach (var lM in Configuration.Instance.ZoneLeaveMessages.ElementAt(x))
                    {
                        temp.addLeaveMessage(lM);
                    }
                    Configuration.Instance.Zones.Add(temp);
                    x++;
                }
            }
        }

        private void Update()
        {
            // TODO: set with command
            frame++;
            if (frame % Configuration.Instance.UpdateFrame != 0) return;

            foreach (var splayer in Provider.clients)
            {
                Vector3 lastPos;
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(splayer);
                // Enter / Leave region
                if (!lastPosition.ContainsKey(player.Id))
                {
                    onPlayerConnection(player);
                }
                else
                {
                    if (!lastPosition.TryGetValue(player.Id, out lastPos))
                    {
                        lastPos = player.Position;
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
                }

                // Player Equip
                if (player.Player.equipment.isSelected && playerInZoneType(player, Zone.flagTypes[Zone.noItemEquip]))
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
                    if (player.IsInVehicle)
                    {
                        player.CurrentVehicle.forceRemoveAllPlayers();
                    }
                    player.Teleport(new Vector3(lastPos.x, lastPos.y - 0.5f, lastPos.z), player.Rotation);
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
                    if (player.IsInVehicle)
                    {
                        player.CurrentVehicle.forceRemoveAllPlayers();
                    }
                    player.Teleport(new Vector3(lastPos.x, lastPos.y - 0.5f, lastPos.z), player.Rotation);
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
            // Block Drop Item
            player.Inventory.onDropItemRequested -= onDropItem;
            // Block Dequip
            player.Player.equipment.onDequipRequested -= onPlayerDequiped;

            lastPosition.Remove(player.Id);
            foreach (var zone in getPositionZones(player.Position))
            {
                onZoneLeave(player, zone, player.Position);
            }
        }

        private void onPlayerConnection(UnturnedPlayer player)
        {
            // Block Drop Item
            player.Inventory.onDropItemRequested += onDropItem;
            // Block Dequip
            player.Player.equipment.onDequipRequested += onPlayerDequiped;
            
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
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleCarjack]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission("advancedzones.override.carjack"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleCarjack]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission(("advancedzones.override.carjack." + zone.getName()).ToLower()))
                    {
                        allow = false;
                        return;
                    }
                }
            }
        }

        private void onVehicleSiphoning(InteractableVehicle vehicle, Player instigatingPlayer, ref bool shouldAllow, ref ushort desiredAmount)
        {
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleSiphoning]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission("advancedzones.override.siphoning"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleSiphoning]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission(("advancedzones.override.siphoning." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }

        private void onSwapSeat(Player player, InteractableVehicle vehicle, ref bool canSwap, byte fromIndex, ref byte toIndex)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.swapseat"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noSwapSeat]))
                    {
                        if (!UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.swapseat." + zone.getName()).ToLower()))
                        {
                            canSwap = false;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onExitVehicle(Player player, InteractableVehicle vehicle, ref bool canExit, ref Vector3 pendingLocation, ref float pendingYaw)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.exitvehicle"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noExitVehicle]))
                    {
                        if (!UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.exitvehicle." + zone.getName()).ToLower()))
                        {
                            canExit = false;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onEnterVehicle(Player player, InteractableVehicle vehicle, ref bool canEnter)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.entervehicle"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noExitVehicle]))
                    {
                        if (!UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.entervehicle." + zone.getName()).ToLower()))
                        {
                            canEnter = false;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void onDropItem(PlayerInventory inventory, Item item, ref bool canDrop)
        {
            if (!UnturnedPlayer.FromPlayer(inventory.player).HasPermission("advancedzones.override.dropitem"))
            {
                List<Zone> currentZones = getPositionZones(inventory.player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noDropItem]))
                    {
                        if (!UnturnedPlayer.FromPlayer(inventory.player).HasPermission(("advancedzones.override.dropitem." + zone.getName()).ToLower()))
                        {
                            canDrop = false;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }
        private void onZombieDamage(ref DamageZombieParameters parameters, ref bool canDamage)
        {
            if (transformInZoneType(parameters.zombie.transform, Zone.flagTypes[Zone.noZombieDamage]))
            {
                canDamage = false;
            }
        }

        private void onAnimalDamage(ref DamageAnimalParameters parameters, ref bool canDamage)
        {
            if (transformInZoneType(parameters.animal.transform, Zone.flagTypes[Zone.noAnimalDamage]))
            {
                canDamage = false;
            }
        }

        private void onTireDamage(CSteamID instigatorSteamID, InteractableVehicle vehicle, int tireIndex, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noTireDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.tiredamage"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noTireDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.tiredamage." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }
        private void onPlayerDequiped(PlayerEquipment equipment, ref bool canDequip)
        {
            if (!UnturnedPlayer.FromPlayer(equipment.player).HasPermission("advancedzones.override.dequip"))
            {
                List<Zone> currentZones = getPositionZones(equipment.player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noDequip]))
                    {
                        if (!UnturnedPlayer.FromPlayer(equipment.player).HasPermission(("advancedzones.override.dequip." + zone.getName()).ToLower()))
                        {
                            canDequip = false;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }
        
        private void onPlayerEquiped(Player player, PlayerEquipment equipment)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.equip"))
            {
                List<Zone> currentZones = getPositionZones(player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noItemEquip]))
                    {
                        if (!UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.equip." + zone.getName()).ToLower()))
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

                            foreach (var blocklist in currentEquipBlocklists)
                            {
                                if (blocklist.name == "ALL")
                                {
                                    equipment.dequip();
                                    return;
                                }
                            }

                            foreach (var blocklist in currentEquipBlocklists)
                            {
                                if (blocklist.hasItem(equipment.asset.id))
                                {
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
        
        private void onPlayerCraft(PlayerCrafting crafting, ref ushort itemID, ref byte blueprintIndex, ref bool canCraft)
        {
            if (!UnturnedPlayer.FromPlayer(crafting.player).HasPermission("advancedzones.override.craft"))
            {
                List<Zone> currentZones = getPositionZones(crafting.player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noCraft]))
                    {
                        if (!UnturnedPlayer.FromPlayer(crafting.player).HasPermission(("advancedzones.override.craft." + zone.getName()).ToLower()))
                        {
                            canCraft = false;
                            return;
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
            if (parameters.cause == EDeathCause.BLEEDING || parameters.cause == EDeathCause.BONES || parameters.cause == EDeathCause.BREATH || parameters.cause == EDeathCause.BURNING || parameters.cause == EDeathCause.FOOD || parameters.cause == EDeathCause.FREEZING 
                || parameters.cause == EDeathCause.INFECTION || parameters.cause == EDeathCause.ARENA || parameters.cause == EDeathCause.KILL || parameters.cause == EDeathCause.SUICIDE || parameters.cause == EDeathCause.WATER)
            {
                return;
            }
            if (parameters.cause == EDeathCause.LANDMINE || parameters.cause == EDeathCause.SHRED || parameters.cause == EDeathCause.SENTRY || parameters.cause == EDeathCause.VEHICLE || parameters.cause == EDeathCause.ROADKILL || parameters.cause == EDeathCause.ACID || parameters.cause == EDeathCause.BOULDER)
            {
                if (playerInZoneType(UnturnedPlayer.FromPlayer(parameters.player), Zone.flagTypes[Zone.noPlayerDamage]))
                {
                    if (parameters.cause == EDeathCause.VEHICLE)
                    {
                        if (UnturnedPlayer.FromPlayer(parameters.player).IsInVehicle)
                            return;
                    }
                    shouldAllow = false;
                    return;
                }
                else if (playerInZoneType(UnturnedPlayer.FromPlayer(parameters.player), Zone.flagTypes[Zone.noPvP]) && (parameters.cause != EDeathCause.ACID && parameters.cause != EDeathCause.BOULDER))
                {
                    if (parameters.cause == EDeathCause.VEHICLE)
                    {
                        if (UnturnedPlayer.FromPlayer(parameters.player).IsInVehicle)
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
            if ((UnturnedPlayer.FromCSteamID(parameters.killer) == null || UnturnedPlayer.FromCSteamID(parameters.killer).Player == null) && playerInZoneType(UnturnedPlayer.FromPlayer(parameters.player), Zone.flagTypes[Zone.noPlayerDamage]))
            {
                if (parameters.cause == EDeathCause.ZOMBIE)
                {
                    UnturnedPlayer.FromPlayer(parameters.player).Infection = 0;
                }
                shouldAllow = false;
                return;
            }
            else if (UnturnedPlayer.FromCSteamID(parameters.killer) == null || UnturnedPlayer.FromCSteamID(parameters.killer).Player == null)
            {
                return;
            }
            if (((playerInZoneType(UnturnedPlayer.FromPlayer(parameters.player), Zone.flagTypes[Zone.noPlayerDamage]) || playerInZoneType(UnturnedPlayer.FromCSteamID(parameters.killer), Zone.flagTypes[Zone.noPlayerDamage]))
                 && !UnturnedPlayer.FromCSteamID(parameters.killer).HasPermission("advancedzones.override.playerdamage")) || ((playerInZoneType(UnturnedPlayer.FromPlayer(parameters.player), Zone.flagTypes[Zone.noPvP]) ||
                 playerInZoneType(UnturnedPlayer.FromCSteamID(parameters.killer), Zone.flagTypes[Zone.noPvP])) && !UnturnedPlayer.FromCSteamID(parameters.killer).HasPermission("advancedzones.override.pvp")))
            {
                List<Zone> currentZones = getPositionZones(parameters.player.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noPlayerDamage]) && !UnturnedPlayer.FromCSteamID(parameters.killer).HasPermission(("advancedzones.override.pvp." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                        return;
                    }
                    else if (zone.hasFlag(Zone.flagTypes[Zone.noPvP]) && !UnturnedPlayer.FromCSteamID(parameters.killer).HasPermission(("advancedzones.override.pvp." + zone.getName()).ToLower()))
                    {
                        shouldAllow = false;
                        return;
                    }
                }
            }
        }

        private void onStructureDeploy(Structure structure, ItemStructureAsset asset, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBuild]))
                    {
                        if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
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
                                    shouldAllow = false;
                                    return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.hasItem(asset.id))
                                {
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
            if (UnturnedPlayer.FromCSteamID(new CSteamID(owner)) == null)
                return;
            if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission("advancedzones.override.build"))
            {
                List<Zone> currentZones = getPositionZones(point);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBuild]))
                    {
                        if (!UnturnedPlayer.FromCSteamID(new CSteamID(owner)).HasPermission(("advancedzones.override.build." + zone.getName()).ToLower()))
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
                                    shouldAllow = false;
                                    return;
                                }
                            }

                            foreach (var blocklist in currentBuildBlocklists)
                            {
                                if (blocklist.hasItem(asset.id))
                                {
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
            if (transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noLockpick]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission("advancedzones.override.lockpick"))
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noLockpick]) && !UnturnedPlayer.FromPlayer(instigatingPlayer).HasPermission(("advancedzones.override.lockpick." + zone.getName()).ToLower()))
                    {
                        allow = false; ;
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
            if ((transformInZoneType(vehicle.transform, Zone.flagTypes[Zone.noVehicleDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.vehicledamage")) && pendingTotalDamage > 0)
            {
                List<Zone> currentZones = getPositionZones(vehicle.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noVehicleDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.vehicledamage." + zone.getName()).ToLower()))
                    {
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
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        List<Zone> currentZones = getPositionZones(structureTransform.transform.position);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.flagTypes[Zone.noDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                            {
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
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.damage") && pendingTotalDamage > 0)
                    {
                        if (barricadeTransform.name.ToString() != "1102"
                                && barricadeTransform.name.ToString() != "1101"
                                && barricadeTransform.name.ToString() != "1393"
                                && barricadeTransform.name.ToString() != "1241")
                        {
                            List<Zone> currentZones = getPositionZones(barricadeTransform.transform.position);
                            foreach (var zone in currentZones)
                            {
                                if (zone.hasFlag(Zone.flagTypes[Zone.noDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.damage." + zone.getName()).ToLower()))
                                {
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
        
        private void onObjectDamage(CSteamID instigatorSteamID, Transform objectTransform, byte section, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(objectTransform, Zone.flagTypes[Zone.noObjectDamage]))
            {
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.objectdamage") && pendingTotalDamage > 0)
                    {
                        List<Zone> currentZones = getPositionZones(objectTransform.transform.position);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.flagTypes[Zone.noObjectDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.objectdamage." + zone.getName()).ToLower()))
                            {
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
        
        private void onResourceDamage(CSteamID instigatorSteamID, Transform resourceTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (transformInZoneType(resourceTransform, Zone.flagTypes[Zone.noResourceDamage]))
            {
                if (UnturnedPlayer.FromCSteamID(instigatorSteamID) != null)
                {
                    if (!UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.resourcedamage") && pendingTotalDamage > 0)
                    {
                        List<Zone> currentZones = getPositionZones(resourceTransform.transform.position);
                        foreach (var zone in currentZones)
                        {
                            if (zone.hasFlag(Zone.flagTypes[Zone.noResourceDamage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.resourcedamage." + zone.getName()).ToLower()))
                            {
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
        
        private void onChangeSign(CSteamID instigatorSteamID, InteractableSign sign, ref string text, ref bool canChange)
        {
            if (transformInZoneType(sign.transform, Zone.flagTypes[Zone.noChangeSign]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.changesign"))
            {
                List<Zone> currentZones = getPositionZones(sign.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noChangeSign]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.changesign." + zone.getName()).ToLower()))
                    {
                        canChange = false;
                    }
                }
            }
            else
            {
                return;
            }
        }
        
        private void onOpenStorage(CSteamID instigatorSteamID, InteractableStorage storage, ref bool canOpen)
        {
            if (transformInZoneType(storage.transform, Zone.flagTypes[Zone.noOpenStorage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission("advancedzones.override.storage"))
            {
                List<Zone> currentZones = getPositionZones(storage.transform.position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noOpenStorage]) && !UnturnedPlayer.FromCSteamID(instigatorSteamID).HasPermission(("advancedzones.override.storage." + zone.getName()).ToLower()))
                    {
                        canOpen = false;
                    }
                }
            }
            else
            {
                return;
            }
        }
        
        private void onHarvestPlant(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool canHarvest)
        {
            if (!UnturnedPlayer.FromCSteamID(steamID).HasPermission("advancedzones.override.harvest"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromCSteamID(steamID).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noHarvest]) && !UnturnedPlayer.FromCSteamID(steamID).HasPermission(("advancedzones.override.harvest." + zone.getName()).ToLower()))
                    {
                        canHarvest = false;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void onBarricadeSalvage(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool canSalvage)
        {
            if (!UnturnedPlayer.FromCSteamID(steamID).HasPermission("advancedzones.override.barricadesalvage"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromCSteamID(steamID).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBarricadeSalvage]) && !UnturnedPlayer.FromCSteamID(steamID).HasPermission(("advancedzones.override.barricadesalvage." + zone.getName()).ToLower()))
                    {
                        canSalvage = false;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void onStructureSalvage(CSteamID steamID, byte x, byte y, ushort index, ref bool canSalvage)
        {
            if (!UnturnedPlayer.FromCSteamID(steamID).HasPermission("advancedzones.override.structuresalvage"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromCSteamID(steamID).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noStructureSalvage]) && !UnturnedPlayer.FromCSteamID(steamID).HasPermission(("advancedzones.override.structuresalvage." + zone.getName()).ToLower()))
                    {
                        canSalvage = false;
                    }
                }
            }
            else
            {
                return;
            }
        }
        
        private void onBarricadeTransform(CSteamID steamID, byte x, byte y, ushort plant, uint instanceID, ref Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool canTransform)
        {
            if (!UnturnedPlayer.FromCSteamID(steamID).HasPermission("advancedzones.override.barricadetransform"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromCSteamID(steamID).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noBarricadeTransform]) && !UnturnedPlayer.FromCSteamID(steamID).HasPermission(("advancedzones.override.barricadetransform." + zone.getName()).ToLower()))
                    {
                        canTransform = false;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void onStructureTransform(CSteamID steamID, byte x, byte y, uint instanceID, ref Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool canTransform)
        {
            if (!UnturnedPlayer.FromCSteamID(steamID).HasPermission("advancedzones.override.structuretransform"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromCSteamID(steamID).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noStructureTransform]) && !UnturnedPlayer.FromCSteamID(steamID).HasPermission(("advancedzones.override.structuretransform." + zone.getName()).ToLower()))
                    {
                        canTransform = false;
                    }
                }
            }
            else
            {
                return;
            }
        }
       
        private void onItemSpawn(Item item, ref Vector3 location, ref bool allowSpawn)
        {
            List<Zone> currentZones = getPositionZones(location);
            foreach (var zone in currentZones)
            {
                if (zone.hasFlag(Zone.flagTypes[Zone.noItemSpawn]))
                {
                    allowSpawn = false;
                }
            }
        }
        
        private void onTakeItem(Player player, byte x, byte y, uint instanceID, byte to_x, byte to_y, byte to_rot, byte to_page, ItemData itemData, ref bool canTakeItem)
        {
            if (!UnturnedPlayer.FromPlayer(player).HasPermission("advancedzones.override.takeitem"))
            {
                List<Zone> currentZones = getPositionZones(UnturnedPlayer.FromPlayer(player).Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noTakeItem]) && !UnturnedPlayer.FromPlayer(player).HasPermission(("advancedzones.override.takeitem." + zone.getName()).ToLower()))
                    {
                        canTakeItem = false;
                    }
                }
            }
            else
            {
                return;
            }
        }

        private void onPlayerChat(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            if (!player.HasPermission("advancedzones.override.chat"))
            {
                List<Zone> currentZones = getPositionZones(player.Position);
                foreach (var zone in currentZones)
                {
                    if (zone.hasFlag(Zone.flagTypes[Zone.noChat]) && !player.HasPermission(("advancedzones.override.chat." + zone.getName()).ToLower()))
                    {
                        cancel = true;
                    }
                }
            }
            else
            {
                return;
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
            foreach (var z in Configuration.Instance.Zones)
            {
                if (z.isReady())
                {
                    float playerX = position.x;
                    float playerZ = position.z;
                    float playerY = position.y;

                    HeightNode[] heightNodes = z.GetHeightNodes();

                    Node[] zoneNodes = z.getNodes();

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
                                    zones.Add(z);
                                }
                            }
                        }
                        else if (heightNodes[0] != null)
                        {
                            if (heightNodes[0].isUpper && playerY < heightNodes[0].y)
                                zones.Add(z);
                            else if (!heightNodes[0].isUpper && playerY > heightNodes[0].y)
                                zones.Add(z);
                        }
                        else
                        {
                            zones.Add(z);
                        }
                    }
                }
            }
            return zones;
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
            foreach (var z in Configuration.Instance.Zones)
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
                foreach (var blocklist in Configuration.Instance.BuildBlocklists)
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
                foreach (var blocklist in Configuration.Instance.EquipBlocklists)
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
            foreach (var customFlag in Configuration.Instance.CustomFlags)
            {
                if (customFlag.name == flagName)
                    return;
            }
            Configuration.Instance.CustomFlags.Add(new CustomFlag(flagName, flagID, flagDescription));
            Configuration.Save();
            return;
        }

        public void removeCustomFlag(string flagName)
        {
            foreach (var customFlag in Configuration.Instance.CustomFlags.ToList())
            {
                if (customFlag.name == flagName)
                {
                    Configuration.Instance.CustomFlags.Remove(customFlag);
                    Configuration.Save();
                }
            }
        }

        public List<CustomFlag> GetCustomFlags()
        {
            return Configuration.Instance.CustomFlags;
        }
    }
}