using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public class CwaEffectImplNone : CwaEffectImpl
    {
        public override eEffect Effect { get { return eEffect.None; } }

        protected override void Dispose(bool dispose)
        {
            if (_disposed)
                return;

            if (dispose)
            {
                if (Result != null)
                    Result.Dispose();
            }

            _disposed = true;
        }

        protected override void Init()
        {
            Result = Helper_Get(Input.ptBounds);
        }

        public override void Calc(Texture2D tex)
        {
            // scale down
            Input.gd.SetRenderTarget(Result);
            Input.gd.Clear(Color.Transparent);
            Input.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.sb.Draw(tex, new Rectangle(0, 0, Result.Width, Result.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);
        }
    }

}
