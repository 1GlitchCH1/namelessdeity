using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityGreenScreen : Mod
    {
        public override void Load()
        {
            On_NPC.AI += FreezeNamelessDeity;
            On_Main.DrawBackground += OverrideDrawBackground;
        }

        public override void Unload()
        {
            On_NPC.AI -= FreezeNamelessDeity;
            On_Main.DrawBackground -= OverrideDrawBackground;
            NamelessDeityHelper.ResetCache();
        }

        private static void FreezeNamelessDeity(On_NPC.orig_AI orig, NPC self)
        {
            if (NamelessDeityHelper.IsNamelessDeity(self))
            {
                self.velocity = Vector2.Zero;
                self.noGravity = true;
                self.noTileCollide = true;
                self.life = self.lifeMax;
                self.immortal = true;
                self.frameCounter++;
                // Nameless Deity spawns with Opacity = 0 and fades in through AI.
                // We skip AI entirely, so Opacity would stay at 0 forever.
                // Force 1 so WotG's render composite actually draws the boss.
                self.Opacity = 1f;
                return;
            }
            orig(self);
        }

        private static void OverrideDrawBackground(On_Main.orig_DrawBackground orig, Main self)
        {
            if (!NamelessDeityHelper.NamelessDeityIsAlive())
            {
                orig(self);
                return;
            }

            // Boss is alive: replace the ENTIRE background with solid green.
            //
            // We intentionally do NOT call orig(self).
            // Calling orig would trigger WotG's DrawBackground hook, which draws its
            // star sky on top of our green — and WotG's hooks leave the SpriteBatch in
            // a state that conflicts with CalamityMod/Daybreak, causing the
            // "End called before Begin" crash.
            //
            // GraphicsDevice.Clear() is a raw GPU operation that requires no SpriteBatch
            // at all, so it is completely crash-safe regardless of batch state.
            // All subsequent draw phases (tiles, NPCs including the boss, player, etc.)
            // will render on top of this green layer.
            Main.graphics.GraphicsDevice.Clear(Color.Green);
        }
    }
}
