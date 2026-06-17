using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityGreenScreenMod : Mod
    {
        /// <summary>
        /// Keybind that toggles all in-game UI on/off. Default: F10.
        /// </summary>
        public static ModKeybind HideUIKeybind { get; private set; } = null!;

        /// <summary>Whether the UI is currently hidden.</summary>
        public static bool UIHidden { get; set; } = false;

        /// <summary>Returns the configured background colour (fallback: spring green).</summary>
        public static Color BgColor
        {
            get
            {
                var cfg = ModContent.GetInstance<NamelessDeityGreenScreenConfig>();
                return cfg?.BackgroundColor ?? new Color(0, 255, 127);
            }
        }

        public override void Load()
        {
            HideUIKeybind = KeybindLoader.RegisterKeybind(this, "ToggleUI", Keys.F10.ToString());

            On_NPC.AI              += FreezeNamelessDeityAI;
            On_Main.DrawBackground += OverrideDrawBackground;
            On_Main.DrawSurfaceBG  += OverrideDrawSurfaceBG;
            On_Main.DrawInterface  += HideInterfaceWhenToggled;
        }

        public override void Unload()
        {
            On_NPC.AI              -= FreezeNamelessDeityAI;
            On_Main.DrawBackground -= OverrideDrawBackground;
            On_Main.DrawSurfaceBG  -= OverrideDrawSurfaceBG;
            On_Main.DrawInterface  -= HideInterfaceWhenToggled;

            UIHidden = false;
            NamelessDeityHelper.ResetCache();
        }

        // ── AI freeze ────────────────────────────────────────────────────────

        private static void FreezeNamelessDeityAI(On_NPC.orig_AI orig, NPC self)
        {
            if (!NamelessDeityHelper.IsNamelessDeity(self))
            {
                orig(self);
                return;
            }

            if (NamelessDeityHelper.IsBossInIntroCutscene(self))
            {
                orig(self);
                return;
            }

            self.velocity       = Vector2.Zero;
            self.noGravity      = true;
            self.noTileCollide  = true;
            self.life           = self.lifeMax;
            self.immortal       = true;
            self.dontTakeDamage = true;
            self.Opacity        = 1f;
            self.frameCounter++;

            NamelessDeityHelper.TickFrozenAnimations(self);
        }

        // ── background ───────────────────────────────────────────────────────

        private static void OverrideDrawBackground(On_Main.orig_DrawBackground orig, Main self)
        {
            if (!NamelessDeityHelper.NamelessDeityIsAlive()) { orig(self); return; }
            Main.graphics.GraphicsDevice.Clear(BgColor);
        }

        private static void OverrideDrawSurfaceBG(On_Main.orig_DrawSurfaceBG orig, Main self)
        {
            if (!NamelessDeityHelper.NamelessDeityIsAlive()) { orig(self); return; }
            Main.graphics.GraphicsDevice.Clear(BgColor);
        }

        // ── UI toggle ────────────────────────────────────────────────────────

        private static void HideInterfaceWhenToggled(
            On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            if (UIHidden) return;
            orig(self, gameTime);
        }
    }
}
