using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public class InputEffectBox
    {
        public GraphicsDevice gd;
        public SpriteBatch sb;

        public Point ptBounds;
        public Point ptBoundsNext;

        public eEffect eEffect = eEffect.None;
    }
}
