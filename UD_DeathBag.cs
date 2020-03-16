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
                Logger.LogWarning("[DeathBag] Log: Author of Plugin: Duck");
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
            UnityEngine.Vector3 deathlocation = player.Position;

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
    }
}
