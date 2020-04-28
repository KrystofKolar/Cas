using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CwaEffects
{
    // fields and settings for an effect
    public class InputBase
    {
        public GraphicsDevice graphicsDevice;
        public SpriteBatch spriteBatch;

        public Point ResultSize { get; set; }
    }

    public class InputEffectPixelate : InputBase
    {
        public Point ScaleDownSize { get; set; }
    }

    public class InputEffectBlur : InputBase
    {
        public int Denominator; // parts resultsize
    }

    public class InputEffectBloom : InputBase
    {
        // params
    }
}
