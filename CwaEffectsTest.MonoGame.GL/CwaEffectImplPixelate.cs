using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public class CwaEffectImplPixelate : CwaEffectImpl
    {
        public override Effect Effect 
            => Effect.Pixelate;

        private RenderTarget2D _rtSmall;

        public override void Prepare()
        {
            InputEffectPixelate input = Input as InputEffectPixelate;

            _rtSmall = BuildRenderTarget2D(input.Pixels);
            Result = BuildRenderTarget2D(input.ResultSize);
        }

        public override void Calc(Texture2D tex)
        {
            // scale down
            Input.graphicsDevice.SetRenderTarget(_rtSmall);
            Input.graphicsDevice.Clear(Color.Transparent);
            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.spriteBatch.Draw(tex, new Rectangle(0, 0, _rtSmall.Width, _rtSmall.Height), Color.White);
            Input.spriteBatch.End();
            Input.graphicsDevice.SetRenderTarget(null);

            // scale up
            Input.graphicsDevice.SetRenderTarget(Result);
            Input.graphicsDevice.Clear(Color.Transparent);
            Input.spriteBatch.Begin(SpriteSortMode.Deferred,
                           BlendState.AlphaBlend,
                           SamplerState.PointClamp,
                           null,
                           null);
            Input.spriteBatch.Draw(_rtSmall, new Rectangle(0, 0, Result.Width, Result.Height), Color.White);
            Input.spriteBatch.End();
            Input.graphicsDevice.SetRenderTarget(null);
        }

        #region dispose
        public override void Dispose()
            => Dispose(true);

        protected override void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
            {
                if (_rtSmall != null)
                    _rtSmall.Dispose();
            }

            base.Dispose(true);
        }
        #endregion

    }

}
