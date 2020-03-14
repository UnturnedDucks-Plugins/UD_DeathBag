using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Unturned;
using Rocket.Core.Plugins;
using Rocket.API.Collections;
using Rocket.API;
using Rocket.Core.Permissions;
using Rocket.Core;
using System.Collections.Generic;
using Rocket.API.Serialisation;
using Rocket.Core.Logging;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using System.Linq;
using Rocket.Unturned.Events;
using static Rocket.Unturned.Events.UnturnedPlayerEvents;
using SDG.Unturned;
using static Rocket.Core.Steam.Profile;

namespace EFG.Duty
{
    public class Duty : RocketPlugin<DutyConfiguration>
    {
        public static Duty Instance;
        public List<DutyGroups> ValidGroups;

        protected override void Load()
        {
            Instance = this;

            ValidGroups = Configuration.Instance.Groups;

            Logger.LogWarning("Loading event \"Player Connected\"...");
            U.Events.OnPlayerConnected += PlayerConnected;
            Logger.LogWarning("Loading event \"Player Disconnected\"...");
            U.Events.OnPlayerDisconnected += PlayerDisconnected;

            Logger.LogWarning("");
            Logger.LogWarning("Duty has been successfully loaded!");
        }

        protected override void Unload()
        {
            Instance = null;

            Logger.LogWarning("Unloading on player connect event...");
            U.Events.OnPlayerConnected -= PlayerConnected;
            Logger.LogWarning("Unloading on player disconnect event...");
            U.Events.OnPlayerConnected -= PlayerDisconnected;

            Logger.LogWarning("");
            Logger.LogWarning("Duty has been unloaded!");
        }

        public void DoDuty(UnturnedPlayer caller)
        {
            if (caller.IsAdmin)
            {
                caller.Admin(false);
                caller.Features.GodMode = false;
                caller.Features.VanishMode = false;
                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("off_duty_message", caller.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
            }
            else
            {
                caller.Admin(true);
                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("on_duty_message", caller.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
            }
        }

        public void DoDuty(UnturnedPlayer Player, DutyGroups Group)
        {
            RocketPermissionsGroup Target = R.Permissions.GetGroup(Group.GroupID);
            if (Target.Members.Contains(Player.CSteamID.ToString()))
            {
                OnPlayerUpdateHealth -= EquippedItemHealth;
                OnPlayerUpdatePosition -= EquippedItemPosition;

                R.Permissions.RemovePlayerFromGroup(Target.Id, Player);
                R.Permissions.Reload();
                Player.Features.GodMode = false;
                Player.Features.VanishMode = false;
                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("off_duty_message", Player.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
            }
            else
            {
                OnPlayerUpdateHealth += EquippedItemHealth;
                OnPlayerUpdatePosition += EquippedItemPosition;

                R.Permissions.AddPlayerToGroup(Group.GroupID, Player);
                R.Permissions.Reload();
                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("on_duty_message", Player.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
            }
        }

        public void CDuty(UnturnedPlayer cplayer, IRocketPlayer caller)
        {
            if (!Configuration.Instance.AllowDutyCheck)
            {
                UnturnedChat.Say(caller, Translate("error_unable_checkduty"));
                return;
            }
            else if (cplayer != null)
            {
                foreach (DutyGroups Group in ValidGroups)
                {
                    RocketPermissionsGroup Target = R.Permissions.GetGroup(Group.GroupID);
                    R.Permissions.Reload();
                    if (Target != null && Target.Members.Contains(cplayer.CSteamID.ToString()))
                    {
                        if (caller is ConsolePlayer)
                        {
                            UnturnedChat.Say(Instance.Translate("check_on_duty_message", "Console", cplayer.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                        }
                        else if (caller is UnturnedPlayer)
                        {
                            UnturnedChat.Say(Instance.Translate("check_on_duty_message", caller.DisplayName, cplayer.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                        }
                        return;
                    }
                }
            }

            if (caller is ConsolePlayer)
            {
                UnturnedChat.Say(Instance.Translate("check_off_duty_message", "Console", cplayer.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.green));
            }
            else if (caller is UnturnedPlayer)
            {
                UnturnedChat.Say(Instance.Translate("check_off_duty_message", caller.DisplayName, cplayer.DisplayName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.green));
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList {
                    {"admin_login_message", "{0} has logged on and is now on duty."},
                    {"admin_logoff_message", "{0} has logged off and is now off duty."},
                    {"on_duty_message", "{0} is now on duty."},
                    {"off_duty_message", "{0} is now off duty."},
                    {"check_on_duty_message", "{0} has confirmed that {1} is on duty."},
                    {"check_off_duty_message", "{0} has confirmed that {1} is not on duty."},
                    {"not_enough_permissions", "You do not have the correct permissions to use duty."},
                    {"error_unable_checkduty", "Unable To Check Duty. Configuration Is Set To Be Disabled."},
                    {"error_cplayer_null", "Player is not online or his name is invalid." },
                    {"error_dc_usage", "No argument was specified. Please use \"dc <playername>\" to check on a player." }
                };

            }
        }

        void PlayerConnected(UnturnedPlayer player)
        {
            if (player.IsAdmin)
            {
                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("admin_login_message", player.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                return;
            }

            foreach (DutyGroups Group in ValidGroups)
            {
                RocketPermissionsGroup Target = R.Permissions.GetGroup(Group.GroupID);
                if (Target.Members.Contains(player.CSteamID.ToString()))
                {
                    if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("admin_login_message", player.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                    return;
                }
            }
        }

        void PlayerDisconnected(UnturnedPlayer player)
        {
            if (player.IsAdmin)
            {
                if (Configuration.Instance.RemoveDutyOnLogout)
                {
                    player.Admin(false);
                    player.Features.GodMode = false;
                    player.Features.VanishMode = false;
                }

                if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("admin_logoff_message", player.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                return;
            }

            foreach (DutyGroups Group in ValidGroups)
            {
                RocketPermissionsGroup Target = R.Permissions.GetGroup(Group.GroupID);
                if (Target.Members.Contains(player.CSteamID.ToString()))
                {
                    if (Configuration.Instance.RemoveDutyOnLogout)
                    {
                        player.Features.GodMode = false;
                        player.Features.VanishMode = false;
                        R.Permissions.RemovePlayerFromGroup(Target.Id, player);
                        R.Permissions.Reload();
                    }

                    if (Configuration.Instance.EnableServerAnnouncer) UnturnedChat.Say(Instance.Translate("admin_logoff_message", player.CharacterName), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColor, Color.red));
                    return;
                }
            }
        }

        public void EquippedItemHealth(UnturnedPlayer player, byte health)
        {
            EquippedItem(player);
        }
        public void EquippedItemPosition(UnturnedPlayer player, Vector3 position)
        {
            EquippedItem(player);
        }

        public void EquippedItem(UnturnedPlayer player)
        {
            foreach (DutyGroups group in ValidGroups)
            {
                RocketPermissionsGroup Target = R.Permissions.GetGroup(group.GroupID);
                if (Target != null && Target.Members.Contains(player.CSteamID.ToString()))
                {
                    if (player?.Player?.equipment?.useable == null)
                        return;

                    if (!player.Player.equipment.isEquipped)
                        return;

                    if (player.Player.equipment.useable is UseableGun ||
                        player.Player.equipment.useable is UseableMelee
                        || player.Player.equipment.useable is UseableThrowable || player.Player.equipment.asset.id == 1194)
                    {
                        player.Player.equipment.dequip();
                        UnturnedChat.Say(player, "You cannot use weapons while on duty.", Color.red);
                    }

                }
            }
        }
    }
}
