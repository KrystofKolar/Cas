using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        public Point Pixels { get; set; }
    }

    public class InputEffectBlur : InputBase
    {
        public Point PixelsBlurry;
    }

    public class InputEffectBloom : InputBase
    {

        public float BloomThreshold;
        public float BloomIntensity;
        public int BlurPasses;


    }
}
