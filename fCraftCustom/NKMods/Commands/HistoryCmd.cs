using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;

namespace fCraftCustom.NKMods.Commands {
    class HistoryCmd {
        public static CommandDescriptor cdRevert = new CommandDescriptor {
            Name = "revert",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Ban },
            Usage = "/revert <player>",
            Help = "Revert block history for a player",
            Handler = Revert
        };

        static void Revert(Player player, Command cmd) {
            string name = cmd.Next();
            Player target = player;

            if (name == null) {
                player.Message("&cType in a player name, dummy");
                return;
            }
            PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);
            if (target == null) return;

            if (!cmd.IsConfirmed) {
                player.Confirm(cmd, "Revert all recorded history of {0}?", info.ClassyName);
                return;
            }

            int count = Helpers.History.RevertHistory(player.World, info, player);
            if (count < 0) {
                player.Message("&cError while reverting history");
            }
            else if (count == 0) {
                player.Message("&cNo history on this map from that player");
            }
        }

        public static CommandDescriptor cdHistory = new CommandDescriptor {
            Name = "history",
            Category = CommandCategory.Moderation,
            Aliases = new[] { "?", "a" },
            IsConsoleSafe = false,
            Permissions = new[] { Permission.ViewOthersInfo },
            Usage = "/history",
            Help = "Find who created a block.",
            Handler = History
        };

        static void History(Player player, Command cmd) {
            player.SelectionStart(1, HistoryCallback, null, cdHistory.Permissions);
            player.Message("Click the block that you would like to test");
        }


        internal static void HistoryCallback(Player player, Vector3I[] marks, object tag) {
            CheckHistory(player, marks[0]);
        }

        internal static void CheckHistory(Player player, Vector3I coord) {
            PlayerInfo target = Helpers.History.GetHistory(player.World, coord.X, coord.Y, coord.Z);
            if (target == null) {
                player.Message("&7History on this block is Unknown");
            } else {
                player.Message("&cThis block was placed by {0}", target.ClassyName);
            }
        }

        public static void OnPlayerPlacingBlock(object sender, PlayerPlacingBlockEventArgs e) {
            if (e.Context != BlockChangeContext.Manual) return;

            if (e.NewBlock != Block.GoldOre) return;
            if (e.Player.GetBind(Block.Aqua) != Block.Water) return;

            CheckHistory(e.Player, e.Coords);
            e.Result = CanPlaceResult.PluginDenied;
        }
    }
}
