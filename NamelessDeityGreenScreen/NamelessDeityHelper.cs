using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public static class NamelessDeityHelper
    {
        private const string WotGModName = "NoxusBoss";
        private const string NamelessDeityTypeName = "NamelessDeityBoss";

        private static int? _cachedType = null;

        public static void ResetCache() => _cachedType = null;

        public static int GetNamelessDeityType()
        {
            if (_cachedType.HasValue)
                return _cachedType.Value;

            if (ModLoader.TryGetMod(WotGModName, out Mod wotg))
            {
                if (wotg.TryFind<ModNPC>(NamelessDeityTypeName, out ModNPC deity))
                {
                    _cachedType = deity.Type;
                    return _cachedType.Value;
                }
            }

            _cachedType = -1;
            return -1;
        }

        public static bool IsNamelessDeity(NPC npc)
        {
            int t = GetNamelessDeityType();
            return t != -1 && npc.type == t;
        }

        public static bool NamelessDeityIsAlive()
        {
            int t = GetNamelessDeityType();
            return t != -1 && NPC.AnyNPCs(t);
        }
    }
}
