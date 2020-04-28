using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CwaEffects
{
    public class CwaEffectImplBlur : CwaEffectImpl
    {
        public override Effect Effect
            => Effect.Blur;

        private RenderTarget2D _BlurTarget;
        private RenderTarget2D[] _BlurTargets;
        private RenderTarget2D[] _BlurResults;

        // denominator of blurtargets
        private int[] _BlurDenominators = {2, 4, 8, 16, 32};


        public override void Prepare()
        {
            InputEffectBlur input = Input as InputEffectBlur;

            _BlurTarget = new RenderTarget2D(input.graphicsDevice, 
                                             input.ResultSize.X / input.Denominator, 
                                             input.ResultSize.Y / input.Denominator, 
                                             mipMap: false,
                                             Input.graphicsDevice.PresentationParameters.BackBufferFormat, 
                                             DepthFormat.None);

            Result = BuildRenderTarget2D(input.ResultSize);

            //Result = new RenderTarget2D(Input.graphicsDevice, 
            //                            input.ResultSize.X, 
            //                            input.ResultSize.Y, 
            //                            mipMap: false,
            //                            Input.graphicsDevice.PresentationParameters.BackBufferFormat,
            //                            Input.graphicsDevice.PresentationParameters.DepthStencilFormat,
            //                            Input.graphicsDevice.PresentationParameters.MultiSampleCount,
            //                            RenderTargetUsage.DiscardContents);

            _BlurTargets = new RenderTarget2D[_BlurDenominators.Length];
            _BlurResults = new RenderTarget2D[_BlurDenominators.Length];

            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                _BlurTargets[i] = new RenderTarget2D(input.graphicsDevice, 
                                                     input.ResultSize.X / _BlurDenominators[i], 
                                                     input.ResultSize.Y / _BlurDenominators[i], 
                                                     false,
                                                     input.graphicsDevice.PresentationParameters.BackBufferFormat, 
                                                     DepthFormat.None);

                _BlurResults[i] = new RenderTarget2D(input.graphicsDevice, 
                                                     input.ResultSize.X, 
                                                     input.ResultSize.Y, 
                                                     false,
                                                     input.graphicsDevice.PresentationParameters.BackBufferFormat,
                                                     input.graphicsDevice.PresentationParameters.DepthStencilFormat,
                                                     input.graphicsDevice.PresentationParameters.MultiSampleCount,
                                                     RenderTargetUsage.DiscardContents);
            }
        }

        public override void Calc(Texture2D tex)
            => CalcBlur(tex, _BlurTarget, Result);

        private void CalcBlur(Texture2D tex, RenderTarget2D blurTarget, RenderTarget2D blurResult)
        {
            // scale down
            Input.graphicsDevice.SetRenderTarget(blurTarget);
            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.spriteBatch.Draw(
                tex, 
                new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), 
                Color.White);
            Input.spriteBatch.End();
            Input.graphicsDevice.SetRenderTarget(null);

            // scale up
            Input.graphicsDevice.SetRenderTarget(blurResult);
            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.spriteBatch.Draw(blurTarget, 
                                   new Rectangle(0, 0, blurResult.Width, blurResult.Height), 
                                   Color.White);
            Input.spriteBatch.End();
            Input.graphicsDevice.SetRenderTarget(null);
        }

        private void CalcBlurs(Texture2D tex)
        {
            for (int i = 0; i < _BlurDenominators.Length; ++i)
                CalcBlur(tex, _BlurTargets[i], _BlurResults[i]);
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
                if (_BlurTarget != null)
                    _BlurTarget.Dispose();

                foreach (var v in _BlurTargets)
                    v.Dispose();

                foreach (var v in _BlurResults)
                    v.Dispose();
            }

            base.Dispose(manual);
        }
        #endregion

    }
}
