using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public class CwaEffectImplBloom : CwaEffectImpl
    {
        public override eEffect Effect
            => eEffect.Bloom;

        RenderTarget2D _Half;
        RenderTarget2D _Quarter;
        RenderTarget2D _Quarter2;

        // settings
        static readonly float _BloomThreshold = 0.25f;
        static readonly float _BloomIntensity = 2.9f;
        static readonly int _BlurPasses = 2;

        // result = source - destination
        static BlendState BlendStateExtractBrightColors =
            new BlendState
            {
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,

                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.Subtract,
                AlphaBlendFunction = BlendFunction.Subtract,
            };

        // result = source + destination
        static BlendState BlendStateAdditiveBlur = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,

            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
        };

        // result = source + (destination * (1 - source))
        static BlendState BlendStateCombineFinalResult = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,

            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaDestinationBlend = Blend.InverseSourceColor,
        };

        protected override void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
            {
                if (_Half != null)
                    _Half.Dispose();

                if (_Quarter != null)
                    _Quarter.Dispose();

                if (_Quarter2 != null)
                    _Quarter2.Dispose();
            }

            _disposed = true;

            base.Dispose(manual);
        }

        protected override void Init()
        {
            PresentationParameters pp = Input.gd.PresentationParameters;

            Input ta = (Input)Input;

            int w = ta.ptBounds.X;
            int h = ta.ptBounds.Y;

            Result = GetRenderTarget2D(new Point(w, h));

            _Half = new RenderTarget2D(Input.gd, w/2, h/2, false, pp.BackBufferFormat, DepthFormat.None);

            _Quarter = new RenderTarget2D(Input.gd, w/4, h/4, false, pp.BackBufferFormat, DepthFormat.None);

            _Quarter2 = new RenderTarget2D(Input.gd, w/4, h/4, false, pp.BackBufferFormat, DepthFormat.None);
        }

        public override void Calc(Texture2D tex)
        {
            // Shrink to half size.
            Input.gd.SetRenderTarget(_Half);
            Input.sb.Begin(0, BlendState.Opaque);
            Input.sb.Draw(tex, new Rectangle(0, 0, _Half.Width, _Half.Height), Color.White);
            Input.sb.End();

            // Shrink again to quarter size
            // at the same time applying the threshold subtraction.
            Input.gd.SetRenderTarget(_Quarter);
            Input.gd.Clear(new Color(_BloomThreshold, _BloomThreshold, _BloomThreshold));
            Input.sb.Begin(0, BlendStateExtractBrightColors);
            Input.sb.Draw(_Half, new Rectangle(0, 0, _Quarter.Width, _Quarter.Height), Color.White);
            Input.sb.End();

            for (int i = 0; i < _BlurPasses; i++)
            {
                Input.gd.SetRenderTarget(_Quarter2);
                Input.gd.Clear(Color.Black);

                int w = _Quarter.Width;
                int h = _Quarter.Height;

                float brightness = 0.25f;

                // On the first pass, scale brightness to restore full range after the threshold subtraction.
                if (i == 0)
                {
                    brightness /= (1 - _BloomThreshold);
                }

                // On the final pass, apply tweakable intensity adjustment.
                if (i == _BlurPasses - 1)
                {
                    brightness *= _BloomIntensity;
                }

                Color tint = new Color(brightness, brightness, brightness);

                Input.sb.Begin(0, BlendStateAdditiveBlur);

                int j = i;
                Vector2 v = new Vector2(0.5f, 0.5f) * .3f; //todo

                Input.sb.Draw(_Quarter, v, new Rectangle(j + 1, j + 1, w, h), tint);
                Input.sb.Draw(_Quarter, v, new Rectangle(-j, j + 1, w, h), tint);
                Input.sb.Draw(_Quarter, v, new Rectangle(j + 1, -j, w, h), tint);
                Input.sb.Draw(_Quarter, v, new Rectangle(-j, -j, w, h), tint);

                Input.sb.End();
            }

            // Combine the original scene and bloom images.
            Input.gd.SetRenderTarget(Result);
            Input.sb.Begin(0, BlendState.Opaque);
            Input.sb.Draw(tex, new Rectangle(0, 0, Result.Width, Result.Height), Color.White);
            Input.sb.End();

            Input.sb.Begin(0, BlendStateCombineFinalResult);
            Input.sb.Draw(_Quarter, new Rectangle(0, 0, Result.Width, Result.Height), Color.White);
            Input.sb.End();

            Input.gd.SetRenderTarget(null);
        }
    }
}
