using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NamelessDeityGreenScreen
{
    public class NamelessDeityGreenScreenConfig : ModConfig
    {
        // ClientSide – каждый игрок выбирает свой цвет.
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("ChromaKey")]
        [Label("Background Color")]
        [Tooltip("Solid color rendered behind the boss.\nChange to match your chroma-key workflow.\nDefault: Spring Green (0, 255, 127 / #00FF7F)")]
        public Color BackgroundColor { get; set; } = new Color(0, 255, 127);
    }
}
