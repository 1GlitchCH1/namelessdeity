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

            npc.velocity = Vector2.Zero;
            npc.life = npc.lifeMax;
            npc.immortal = true;
            npc.frameCounter++;
            // Without AI, Opacity stays at its initial spawn value (0).
            // Force it to 1 so WotG's render composite draws the boss visibly.
            npc.Opacity = 1f;

            return false; // skip all boss AI
        }
    }
}
