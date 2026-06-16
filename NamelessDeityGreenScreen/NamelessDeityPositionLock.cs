using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    // Backup position lock: runs AFTER all NPC updates (including any AI
    // that bypassed our On_NPC.AI hook), so the boss cannot visually drift.
    public class NamelessDeityPositionLock : ModSystem
    {
        private static Vector2? _lockedCenter = null;

        public override void PostUpdateNPCs()
        {
            int t = NamelessDeityHelper.GetNamelessDeityType();
            if (t == -1) return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.type != t) continue;

                // First time we see the boss: record its spawn position.
                if (_lockedCenter == null)
                    _lockedCenter = npc.Center;

                // Force position every frame — cannot be bypassed by any AI.
                npc.Center = _lockedCenter.Value;
                npc.velocity = Vector2.Zero;
            }
        }

        public override void PreUpdateNPCs()
        {
            // Release the lock when the boss despawns.
            if (!NamelessDeityHelper.NamelessDeityIsAlive())
                _lockedCenter = null;
        }
    }
}
