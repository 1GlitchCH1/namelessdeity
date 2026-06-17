using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    // Backup position lock: runs AFTER all NPC updates so the boss cannot
    // visually drift once the intro cutscene has finished.
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

                // While the intro cutscene is playing, the boss moves around —
                // don't lock it yet.
                if (NamelessDeityHelper.IsBossInIntroCutscene(npc))
                {
                    _lockedCenter = null;
                    continue;
                }

                // First frame after the cutscene: record where the boss settled.
                if (_lockedCenter == null)
                    _lockedCenter = npc.Center;

                // Force position every frame.
                npc.Center   = _lockedCenter.Value;
                npc.velocity = Vector2.Zero;
            }
        }

        public override void PreUpdateNPCs()
        {
            if (!NamelessDeityHelper.NamelessDeityIsAlive())
                _lockedCenter = null;
        }
    }
}
