using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    class CwaEffectImplBlur : CwaEffectImpl
    {
        public override eEffect Effect { get { return eEffect.Blur; } }
        // blur
        RenderTarget2D _BlurTarget;

        RenderTarget2D[] _BlurTargets;
        RenderTarget2D[] _BlurResults;

        // blur settings

        // denominator/nenner of blurtargets and other usage
        int[] _BlurDenominators = { 2, 4, 8, 16, 32 }; //todo

        protected override void Dispose(bool dispose)
        {
            if (_disposed)
                return;

            if (dispose)
            {
                foreach (var v in _BlurTargets)
                    v.Dispose();

                foreach (var v in _BlurResults)
                    v.Dispose();

                if (Result != null)
                    Result.Dispose();
            }

            _disposed = true;
        }

        protected override void Init()
        {
            _BlurTarget = new RenderTarget2D(Input.gd, Input.ptBounds.X / 32, Input.ptBounds.Y / 32, false,
                                             Input.gd.PresentationParameters.BackBufferFormat, DepthFormat.None);

            Result = new RenderTarget2D(Input.gd, Input.ptBounds.X, Input.ptBounds.Y, false,
                                             Input.gd.PresentationParameters.BackBufferFormat,
                                             Input.gd.PresentationParameters.DepthStencilFormat,
                                             Input.gd.PresentationParameters.MultiSampleCount,
                                             RenderTargetUsage.DiscardContents);

            _BlurTargets = new RenderTarget2D[_BlurDenominators.Length];
            _BlurResults = new RenderTarget2D[_BlurDenominators.Length];

            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                _BlurTargets[i] = new RenderTarget2D(Input.gd, Input.ptBounds.X / _BlurDenominators[i], Input.ptBounds.Y / _BlurDenominators[i], false,
                                            Input.gd.PresentationParameters.BackBufferFormat, DepthFormat.None);

                _BlurResults[i] = new RenderTarget2D(Input.gd, Input.ptBounds.X, Input.ptBounds.Y, false,
                                            Input.gd.PresentationParameters.BackBufferFormat,
                                            Input.gd.PresentationParameters.DepthStencilFormat,
                                            Input.gd.PresentationParameters.MultiSampleCount,
                                            RenderTargetUsage.DiscardContents);
            }
        }

        void CalcBlur(Texture2D tex, RenderTarget2D blurTarget, RenderTarget2D blurResult) //const
        {
            // small
            Input.gd.SetRenderTarget(blurTarget);

            Input.sb.Begin(0, BlendState.AlphaBlend);
            Input.sb.Draw(tex, new Rectangle(0, 0, blurTarget.Width, blurTarget.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);

            // resize to large, simulate blur
            Input.gd.SetRenderTarget(blurResult);
            Input.sb.Begin(0, BlendState.AlphaBlend);
            Input.sb.Draw(blurTarget, new Rectangle(0, 0, blurResult.Width, blurResult.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);
        }

        void CalcBlurs(Texture2D tex) //todo
        {
            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                CalcBlur(tex, _BlurTargets[i], _BlurResults[i]);
            }
        }

        public override void Calc(Texture2D tex)
        {
            CalcBlur(tex, _BlurTarget, Result);
        }
    }
}
