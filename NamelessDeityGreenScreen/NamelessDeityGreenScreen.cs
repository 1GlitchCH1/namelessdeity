using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityGreenScreen : Mod
    {
        public override void Load()
        {
            // Skip boss AI at the MonoMod level — bypasses tModLoader hooks
            // and any IL-injected behavior from WotG.
            On_NPC.AI += FreezeNamelessDeity;

            // Draw green AFTER WotG's own DrawBackground hook renders its star sky,
            // so our green sits on top of WotG's background but under the boss NPC.
            On_Main.DrawBackground += DrawGreenAfterBackground;
        }

        public override void Unload()
        {
            On_NPC.AI -= FreezeNamelessDeity;
            On_Main.DrawBackground -= DrawGreenAfterBackground;
            NamelessDeityHelper.ResetCache();
        }

        private static void FreezeNamelessDeity(On_NPC.orig_AI orig, NPC self)
        {
            if (NamelessDeityHelper.IsNamelessDeity(self))
            {
                // Do NOT call orig — skip all AI including WotG's StateMachine.
                self.velocity = Vector2.Zero;
                self.noGravity = true;
                self.noTileCollide = true;
                self.life = self.lifeMax;
                self.immortal = true;
                self.frameCounter++;
                return;
            }
            orig(self);
        }

        private static void DrawGreenAfterBackground(On_Main.orig_DrawBackground orig, Main self)
        {
            // Call WotG's hook chain first so its star sky renders.
            orig(self);

            if (!NamelessDeityHelper.NamelessDeityIsAlive())
                return;

            // DrawBackground ends the SpriteBatch before returning.
            // Open a fresh batch so we can paint over WotG's background.
            // The boss NPC will render on top of this during the NPC draw phase.
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            Main.spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                Color.Green
            );

            Main.spriteBatch.End();
        }
    }
}
