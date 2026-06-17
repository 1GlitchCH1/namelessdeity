using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityFreezeNPC : GlobalNPC
    {
        public override bool PreAI(NPC npc)
        {
            if (!NamelessDeityHelper.IsNamelessDeity(npc))
                return true;

            // Let the intro cutscene run normally so all parts initialise.
            if (NamelessDeityHelper.IsBossInIntroCutscene(npc))
                return true;

            // ── Frozen mode ────────────────────────────────────────────────
            // Keep the boss locked in place and at full health.
            npc.velocity  = Vector2.Zero;
            npc.life      = npc.lifeMax;
            npc.immortal  = true;
            npc.dontTakeDamage = true;
            npc.Opacity   = 1f;
            npc.frameCounter++;

            // Drive all animations that normally run inside AI().
            NamelessDeityHelper.TickFrozenAnimations(npc);

            return false; // skip boss AI entirely
        }
    }
}
