using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;

namespace UD_DeathBag
{
    class UD_DeathBag : RocketPlugin<UD_DeathBagConfiguration>
    {
        public static UD_DeathBag Instance;

        protected override void Load()
        {
            UD_DeathBag.Instance = this;

            if (Instance.Configuration.Instance.Enabled)
            {
                UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
                Logger.LogWarning("[DeathBag] Plugin Dev: Duck");
                Logger.LogWarning("[DeathBag] If this plugin is running on a server unaffliated with BBGN, you are violating International Copyright Laws and will be punished accordingly");
                Logger.LogWarning("[DeathBag] Log: The plugin has been loaded");
            }
        }

        protected override void Unload()
        {
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;


            Logger.LogWarning("[DeathBag] Log: The plugin has been unloaded");
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            List<ItemJar> droppedInventory = new List<ItemJar>();
            UnityEngine.Vector3 deathlocation = player.Position + new UnityEngine.Vector3(0, (float)0.60, 0);

            moveInventoryItems(player, droppedInventory);
            if(Instance.Configuration.Instance.StoreClothes)
                removeClothing(player, droppedInventory);

            // use the specified unturned storage asset
            Barricade deathBag = new Barricade(Instance.Configuration.Instance.DeathBagId);
            UnityEngine.Transform barricadeTransform = new UnityEngine.GameObject().transform;
            barricadeTransform.localPosition = deathlocation;

            // create the physical bag to be dropped into the scene
            InteractableStorage deathBagStorage = BarricadeManager.dropBarricade(deathBag, barricadeTransform, deathlocation, 0, 0, 0, 0, 0).GetComponent<InteractableStorage>();
            foreach (ItemJar itemjar in droppedInventory)
            {
                // add the inventory into the bag
                deathBagStorage.items.tryAddItem(itemjar.item);
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                DamageTool.damage(deathBagStorage.transform, false, 100000, 1, out EPlayerKill kill);
            }, (float)Instance.Configuration.Instance.Delay);
        }

        private void moveInventoryItems(UnturnedPlayer player, List<ItemJar> droppedInventory)
        {
            for (byte page = 0; page < (PlayerInventory.PAGES - 1); page++)
            {
                byte itemcount = player.Inventory.getItemCount(page);
                if (itemcount > 0)
                {
                    for (byte p1 = 0; p1 < itemcount; p1++)
                    {
                        // store the dead player's inventory to put into another storage
                        droppedInventory.Add(player.Inventory.getItem(page, 0));
                        // remove the item from a player's inventory to avoid duplications
                        player.Inventory.removeItem(page, 0);
                    }
                }
            }
        }

        private void removeClothing(UnturnedPlayer player, List<ItemJar> droppedInventory)
        {

            byte[] EMPTY_BYTE_ARRAY = new byte[0];

            // Unequip & remove from inventory
            player.Player.clothing.askWearBackpack(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearGlasses(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearHat(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearPants(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearMask(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearShirt(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);

            player.Player.clothing.askWearVest(0, 0, EMPTY_BYTE_ARRAY, true);
            moveInventoryItems(player, droppedInventory);
        }
    }
}
