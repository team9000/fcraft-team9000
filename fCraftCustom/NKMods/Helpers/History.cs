using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;

namespace fCraftCustom.NKMods.Helpers {
    class HistoryInfo {
        public int[] lastplayer;
        public Block[] prevblock;
        public Dictionary<int, List<int>> playersets;
        public HistoryInfo(int length) {
            lastplayer = new int[length];
            prevblock = new Block[length];
            playersets = new Dictionary<int, List<int>>();
        }
    }
    class History {
        public static Dictionary<string, HistoryInfo> historyinfos = new Dictionary<string, HistoryInfo>();


        static bool PrepareHistory(World world) {
            if (world == null || world.Map == null || world.Map.Blocks == null)
                return false;
            if (!ShouldKeepHistory(world))
                return false;

            int length = world.Map.Blocks.Length;

            if (!historyinfos.ContainsKey(world.Name) ||
                historyinfos[world.Name].lastplayer == null ||
                historyinfos[world.Name].lastplayer.Length != length) {

                historyinfos[world.Name] = new HistoryInfo(length);
            }
            return true;
        }
        static int PrepareHistory(World world, int x, int y, int h) {
            if (!PrepareHistory(world))
                return -1;
            if (!world.Map.InBounds(x, y, h))
                return -1;
            int index = world.Map.Index(x, y, h);
            if (index < 0 || index > historyinfos[world.Name].lastplayer.Length)
                return -1;
            return index;
        }
        public static PlayerInfo FindPlayerInfo(int id) {
            PlayerInfo[] cache = PlayerDB.PlayerInfoList;
            foreach (PlayerInfo info in cache) {
                if (info.ID == id) {
                    return info;
                }
            }

            return null;
        }
        public static PlayerInfo GetHistory(World world, int x, int y, int h) {
            int index = PrepareHistory(world, x, y, h);
            if (index < 0)
                return null;
            int historyid = historyinfos[world.Name].lastplayer[index];
            if (historyid <= 0) return null;
            return FindPlayerInfo(historyid);
        }
        static void SetHistory(World world, int x, int y, int h, Player player, Block block) {
            if (player == null)
                return;
            int index = PrepareHistory(world, x, y, h);
            if (index < 0)
                return;

            HistoryInfo historyinfo = historyinfos[world.Name];
            if (historyinfo.lastplayer[index] != player.Info.ID) {
                historyinfo.lastplayer[index] = player.Info.ID;
                historyinfo.prevblock[index] = block;
                List<int> playerset;
                if (!historyinfo.playersets.TryGetValue(player.Info.ID, out playerset)) {
                    playerset = new List<int>();
                    historyinfo.playersets.Add(player.Info.ID, playerset);
                }
                playerset.Add(index);
            }
        }
        static int[] UnIndex(Map map, int index)
        {
            int x = index % map.Width;
            index = (index - x) / map.Width;
            int y = index % map.Length;
            index = (index - y) / map.Length;
            int h = index;
            int[] coords = { x, y, h };
            return coords;
        }
        public static int RevertHistory(World world, PlayerInfo target, Player inform) {
            try {
                if (!PrepareHistory(world))
                    return -1;

                HistoryInfo historyinfo = historyinfos[world.Name];
                List<int> playerset;
                if (!historyinfo.playersets.TryGetValue(target.ID, out playerset))
                    return 0;

                int count = 0;
                foreach (int index in playerset) {
                    if (historyinfo.lastplayer[index] != target.ID)
                        continue;

                    int[] coords = UnIndex(world.Map, index);
                    // set origin player as null so the SetHistory for the update doesn't do anything
                    world.Map.QueueUpdate(
                        new BlockUpdate(null, (short)coords[0], (short)coords[1], (short)coords[2], historyinfo.prevblock[index]));
                    historyinfo.lastplayer[index] = 0;
                    historyinfo.prevblock[index] = 0;
                    count++;
                }

                if (inform != null && count > 0) {
                    inform.Message("&aReverted {0} changes from player {1} on world {2}",
                        count,
                        target.ClassyName,
                        world.ClassyName
                    );
                }

                return count;
            }
            catch (Exception) {
            }
            return 0;
        }

        public static bool ShouldKeepHistory(World world) {
            Rank coolrank = RankManager.FindRank("cool");
            if (
                (world.BuildSecurity.MinRank == RankManager.LowestRank) ||
                (coolrank != null && world.BuildSecurity.MinRank <= coolrank) ||
                world.Name.ToLower().StartsWith("spritebuilder")
            ) {
                return true;
            }
            return false;
        }

        public static void OnPlayerPlacedBlock(Object sender, PlayerPlacedBlockEventArgs e) {
            if (e.Context != BlockChangeContext.Manual && e.Context != BlockChangeContext.Replaced) return;

            try {
                SetHistory(e.Player.World, e.Coords.X, e.Coords.Y, e.Coords.Z, e.Player, e.OldBlock);
            } catch (Exception) {
            }
        }
    }
}
