using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CwaEffects
{
    class CwaEffectImplBlur : CwaEffectImpl
    {
        public override eEffect Effect
            => eEffect.Blur;

        RenderTarget2D _BlurTarget;
        RenderTarget2D[] _BlurTargets;
        RenderTarget2D[] _BlurResults;

        // blur settings

        // denominator/nenner of blurtargets and other usage
        int[] _BlurDenominators = { 2, 4, 8, 16, 32 }; //todo

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

            _disposed = true;

            base.Dispose(manual);
        }

        protected override void Init()
        {
            Input ta = (Input)Input;

            _BlurTarget = new RenderTarget2D(ta.gd, ta.ptBounds.X / 32, ta.ptBounds.Y / 32, false,
                                             Input.gd.PresentationParameters.BackBufferFormat, DepthFormat.None);

            Result = new RenderTarget2D(Input.gd, ta.ptBounds.X, ta.ptBounds.Y, false,
                                             Input.gd.PresentationParameters.BackBufferFormat,
                                             Input.gd.PresentationParameters.DepthStencilFormat,
                                             Input.gd.PresentationParameters.MultiSampleCount,
                                             RenderTargetUsage.DiscardContents);

            _BlurTargets = new RenderTarget2D[_BlurDenominators.Length];
            _BlurResults = new RenderTarget2D[_BlurDenominators.Length];

            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                _BlurTargets[i] = new RenderTarget2D(ta.gd, ta.ptBounds.X / _BlurDenominators[i], ta.ptBounds.Y / _BlurDenominators[i], false,
                                            ta.gd.PresentationParameters.BackBufferFormat, DepthFormat.None);

                _BlurResults[i] = new RenderTarget2D(ta.gd, ta.ptBounds.X, ta.ptBounds.Y, false,
                                            ta.gd.PresentationParameters.BackBufferFormat,
                                            ta.gd.PresentationParameters.DepthStencilFormat,
                                            ta.gd.PresentationParameters.MultiSampleCount,
                                            RenderTargetUsage.DiscardContents);
            }
        }

        void CalcBlur(Texture2D tex, RenderTarget2D blurTarget, RenderTarget2D blurResult)
        {
            // small
            Input.gd.SetRenderTarget(blurTarget);
            Input.sb.Begin(0, BlendState.AlphaBlend);
            Input.sb.Draw(tex, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);

            // resize to large, gets blurry
            Input.gd.SetRenderTarget(blurResult);
            Input.sb.Begin(0, BlendState.AlphaBlend);
            Input.sb.Draw(blurTarget, new Rectangle(0, 0, blurResult.Width, blurResult.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);
        }

        void CalcBlurs(Texture2D tex)
        {
            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                CalcBlur(tex, _BlurTargets[i], _BlurResults[i]);
            }
        }

        public override void Calc(Texture2D tex)
            => CalcBlur(tex, _BlurTarget, Result);
    }
}
