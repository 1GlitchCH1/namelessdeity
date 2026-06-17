using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace NamelessDeityGreenScreen
{
    /// <summary>
    /// ModSystem that:
    ///  1. Deactivates WotG's Nameless Deity sky filter every frame so it
    ///     does not draw the star background over our green screen.
    ///  2. Handles the F10 keybind that toggles the in-game UI.
    /// </summary>
    public class GreenScreenSystem : ModSystem
    {
        // Key names that WotG registers for its sky effects.
        // We deactivate all of them when the green screen is active.
        private static readonly string[] WotGSkyFilterKeys = new[]
        {
            "NoxusBoss:NamelessDeitySky",
            "NoxusBoss:NamelessDeityBackground",
            "NoxusBoss:NamelessDeityDimension",
        };

        public override void PostUpdateNPCs()
        {
            if (!NamelessDeityHelper.NamelessDeityIsAlive())
                return;

            // Deactivate WotG sky / background filters so they cannot paint
            // their star textures on top of our green GPU clear.
            foreach (string key in WotGSkyFilterKeys)
            {
                try
                {
                    if (Filters.Scene[key].IsActive())
                        Filters.Scene[key].Deactivate();
                }
                catch { /* filter may not exist in all WotG versions */ }
            }
        }

        public override void PostUpdateEverything()
        {
            // Handle the UI toggle keybind.
            if (NamelessDeityGreenScreenMod.HideUIKeybind.JustPressed)
                NamelessDeityGreenScreenMod.UIHidden = !NamelessDeityGreenScreenMod.UIHidden;
        }
    }
}
