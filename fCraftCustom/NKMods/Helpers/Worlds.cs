using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;
using fCraft.Events;

namespace fCraftCustom.NKMods.Helpers {
    class Worlds {
        public static void OnPlayerConnected(Object sender, PlayerConnectedEventArgs e) {
            if (!e.StartingWorld.IsFull) return;

            World found = FindWorldNum("Guest", true);
            if (found != null) {
                e.StartingWorld = found;
            }
        }
        public static void OnSearchingForWorld(Object sender, SearchingForWorldEventArgs e) {
            //if (!e.ToJoin) return;
            if (e.Matches.Count <= 1) return;
            World found = FindWorldNum(e.SearchTerm);
            if (found != null) {
                e.Matches.Clear();
                e.Matches.Add(found);
            }
        }
        private static World FindWorldNum(string request, bool ignorefull = false) {
            int max = 0;
            World found = null;

            World[] worldListCache = WorldManager.Worlds;
            foreach (World world in worldListCache) {
                if (ignorefull && world.IsFull) continue;
                if (!world.Name.ToLower().StartsWith(request.ToLower())) continue;
                String stripped = world.Name.ToLower().Replace(request.ToLower(), "");
                int number = 0;
                try {
                    number = Convert.ToInt32(stripped);
                }
                catch (Exception) {
                }

                if (number > max) {
                    max = number;
                    found = world;
                }
            }

            if (max > 0) return found;
            return null;
        }
    }
}
