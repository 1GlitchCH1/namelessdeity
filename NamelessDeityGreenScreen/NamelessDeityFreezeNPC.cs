using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityFreezeNPC : GlobalNPC
    {
        // Internal mod name for Wrath of the Gods and the Nameless Deity class name.
        private const string WotGModName = "NoxusBoss";
        private const string NamelessDeityTypeName = "NamelessDeityBoss";

        private static int? _cachedType = null;

        private static int GetNamelessDeityType()
        {
            if (_cachedType.HasValue)
                return _cachedType.Value;

            if (ModLoader.TryGetMod(WotGModName, out Mod calamity))
            {
                if (calamity.TryFind<ModNPC>(NamelessDeityTypeName, out ModNPC deity))
                {
                    _cachedType = deity.Type;
                    return _cachedType.Value;
                }
            }

            _cachedType = -1;
            return -1;
        }

        private bool IsNamelessDeity(NPC npc)
        {
            int t = GetNamelessDeityType();
            return t != -1 && npc.type == t;
        }

        // Stop ALL AI so the boss just floats in place.
        public override bool PreAI(NPC npc)
        {
            if (!IsNamelessDeity(npc))
                return true;

            // Kill velocity so it doesn't drift.
            npc.velocity = Vector2.Zero;

            // Keep the boss alive forever (don't let HP drop).
            npc.life = npc.lifeMax;
            npc.immortal = true;

            // Advance frame counter manually so the idle animation keeps playing.
            npc.frameCounter++;

            // Return false = skip the boss's own AI completely.
            return false;
        }

        // Draw a solid green rectangle BEHIND the NPC before it renders.
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!IsNamelessDeity(npc))
                return true;

            // Size of the green screen in pixels (world space mapped to screen).
            const int GreenScreenWidth = 1920;
            const int GreenScreenHeight = 1080;

            // Center the green screen on the NPC's screen position.
            Vector2 npcScreenPos = npc.Center - screenPos;
            Rectangle greenRect = new Rectangle(
                (int)(npcScreenPos.X - GreenScreenWidth / 2f),
                (int)(npcScreenPos.Y - GreenScreenHeight / 2f),
                GreenScreenWidth,
                GreenScreenHeight
            );

            spriteBatch.Draw(
                Terraria.GameContent.TextureAssets.MagicPixel.Value,
                greenRect,
                Color.Green
            );

            // Return true = still draw the NPC on top.
            return true;
        }
    }
}
