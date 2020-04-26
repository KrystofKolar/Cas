﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CwaEffects
{
    // fields and settings for an effect
    public class InputBase
    {
        public GraphicsDevice gd;
        public SpriteBatch sb;
    }

    public class Input : InputBase
    {
        public Point ptBounds;
        public Point ptBoundsNext;
    }
}
