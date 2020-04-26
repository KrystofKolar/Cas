using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Kawase blur filter (see http://developer.amd.com/media/gpu_assets/Oat-ScenePostprocessing.pdf)

namespace CwaEffects
{
    public enum eEffect
    {
        Undef,
        None,

        Blur,
        Bloom,
        Pixelate
    }

    public class CwaEffectBox : IDisposable
    {
        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (_disposed)
                return;

            if (dispose)
            {
                if (_PixelateResultSmall != null)
                    _PixelateResultSmall.Dispose();

                if (_PixelateResult != null)
                    _PixelateResult.Dispose();


                if (_BloomTargetHalfSize != null)
                    _BloomTargetHalfSize.Dispose();

                if (_BloomTargetQuarterSize != null)
                    _BloomTargetQuarterSize.Dispose();

                if (_BloomTargetQuarterSize2 != null)
                    _BloomTargetQuarterSize2.Dispose();

                if (_BloomResult != null)
                    _BloomResult.Dispose();



                if (_BlurResult != null)
                    _BlurResult.Dispose();

                if (_BlurTarget != null)
                    _BlurTarget.Dispose();

                for (int i = 0; i < _BlurResults.Length; ++i)
                {
                    _BlurResults[i].Dispose();
                }

                for (int i = 0; i < _BlurTargets.Length; ++i)
                {
                    _BlurTargets[i].Dispose();
                }
            }

            _disposed = true;
        }

        public CwaEffectBox()
        {
            Input = new InputEffectBox();
            _EffectPrepared = eEffect.Undef;
        }

        public InputEffectBox Input;

        Texture2D _Result;

        // bloom
        RenderTarget2D _BloomTargetHalfSize;
        RenderTarget2D _BloomTargetQuarterSize;
        RenderTarget2D _BloomTargetQuarterSize2;

        RenderTarget2D _BloomResult;

        // bloom settings
        const float _BloomThreshold = 0.25f;
        const float _BloomIntensity = 2.9f;
        const int _BlurPasses = 2;

        // blur
        RenderTarget2D _BlurTarget;
        RenderTarget2D _BlurResult;

        RenderTarget2D[] _BlurTargets;
        RenderTarget2D[] _BlurResults;

        // blur settings

        // denominator/nenner of blurtargets and other usage
        int[] _BlurDenominators = { 2, 4, 8, 16, 32 }; 

        // pixelate
        RenderTarget2D _PixelateResult;
        RenderTarget2D _PixelateResultSmall;

        // pixelate settings
        Point _PixelateResultSizeSmall = Point.Zero; //320, 200); //(10*7, 10*5);
        Point _PixelateResultSize = Point.Zero;

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

        static BlendState BlendstateTest =
            new BlendState
            { 
                /*
                // BlendState.AlphaBlend

                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,

                ColorDestinationBlend = Blend.InverseSourceAlpha,

                AlphaDestinationBlend = Blend.InverseSourceAlpha,
                */


                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,

                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,

                ColorBlendFunction = BlendFunction.ReverseSubtract,
                AlphaBlendFunction = BlendFunction.ReverseSubtract,
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

        public delegate void DelegateCalculator(Texture2D tex);

        public DelegateCalculator Calc;

        eEffect _EffectPrepared = eEffect.Undef;

        public void PrepareEffect()
        {
            if (Input.eEffect == _EffectPrepared)
                return;

            switch (Input.eEffect)
            {
                case eEffect.Bloom:
                    InitBloom();
                    Calc = CalcBloom;
                    _Result = _BloomResult;
                    break;

                case eEffect.Blur:
                    InitBlur();
                    Calc = CalcBlur;
                    _Result = _BlurResult;
                    break;

                case eEffect.Pixelate:
                    InitPixelate();
                    Calc = CalcPixelate;
                    _Result = _PixelateResult;
                    break;

                default:
                    break;
            }

            _EffectPrepared = Input.eEffect;
        }

        public Texture2D Result
        {
            get
            {
                return _Result;
            }
        }



        void InitBloom()
        {
            PresentationParameters pp = Input.gd.PresentationParameters;

            int w = Input.ptBounds.X;
            int h = Input.ptBounds.Y;

            _BloomResult = new RenderTarget2D(Input.gd, w, h, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);

            _BloomTargetHalfSize = new RenderTarget2D(Input.gd, w / 2, h / 2, false, pp.BackBufferFormat, DepthFormat.None);

            _BloomTargetQuarterSize = new RenderTarget2D(Input.gd, w / 4, h / 4, false, pp.BackBufferFormat, DepthFormat.None);

            _BloomTargetQuarterSize2 = new RenderTarget2D(Input.gd, w / 4, h / 4, false, pp.BackBufferFormat, DepthFormat.None);
        }

        void CalcBloom(Texture2D tex)
        {
            // Shrink to half size.
            Input.gd.SetRenderTarget(_BloomTargetHalfSize);
            CwaCommon.Texture.DrawSprite(Input.sb, tex, _BloomTargetHalfSize.Width, _BloomTargetHalfSize.Height, BlendState.Opaque);

            // Shrink again to quarter size
            // at the same time applying the threshold subtraction.
            Input.gd.SetRenderTarget(_BloomTargetQuarterSize);
            Input.gd.Clear(new Color(_BloomThreshold, _BloomThreshold, _BloomThreshold));
            CwaCommon.Texture.DrawSprite(Input.sb, _BloomTargetHalfSize, _BloomTargetQuarterSize.Width, _BloomTargetQuarterSize.Height, BlendStateExtractBrightColors);

            for (int i = 0; i < _BlurPasses; i++)
            {
                Input.gd.SetRenderTarget(_BloomTargetQuarterSize2);
                Input.gd.Clear(Color.Black);

                int w = _BloomTargetQuarterSize.Width;
                int h = _BloomTargetQuarterSize.Height;

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

                Input.sb.Draw(_BloomTargetQuarterSize, v, new Rectangle(j + 1, j + 1, w, h), tint);
                Input.sb.Draw(_BloomTargetQuarterSize, v, new Rectangle(-j, j + 1, w, h), tint);
                Input.sb.Draw(_BloomTargetQuarterSize, v, new Rectangle(j + 1, -j, w, h), tint);
                Input.sb.Draw(_BloomTargetQuarterSize, v, new Rectangle(-j, -j, w, h), tint);

                Input.sb.End();

                CwaCommon.Texture.Swap(ref _BloomTargetQuarterSize, ref _BloomTargetQuarterSize2);
            }

            // Combine the original scene and bloom images.
            Input.gd.SetRenderTarget(_BloomResult);

            CwaCommon.Texture.DrawSprite(Input.sb, tex, _BloomResult.Width, _BloomResult.Height, BlendState.Opaque);
            CwaCommon.Texture.DrawSprite(Input.sb, _BloomTargetQuarterSize, _BloomResult.Width, _BloomResult.Height, BlendStateCombineFinalResult);
            //DrawSprite(quarterSize2, result.Width, result.Height, BlendState.AlphaBlend);

            Input.gd.SetRenderTarget(null);
        }



        void InitBlur()
        {
            _BlurTarget = new RenderTarget2D(Input.gd, Input.ptBounds.X / 32, Input.ptBounds.Y / 32, false,
                                             Input.gd.PresentationParameters.BackBufferFormat, DepthFormat.None);

            _BlurResult = new RenderTarget2D(Input.gd, Input.ptBounds.X, Input.ptBounds.Y, false,
                                             Input.gd.PresentationParameters.BackBufferFormat,
                                             Input.gd.PresentationParameters.DepthStencilFormat,
                                             Input.gd.PresentationParameters.MultiSampleCount,
                                             RenderTargetUsage.DiscardContents);
        }

        void InitBlurs()
        {
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

        void CalcBlur(Texture2D tex)
        {
            CalcBlur(tex, _BlurTarget, _BlurResult);
        }

        void CalcBlur(Texture2D tex, RenderTarget2D blurTarget, RenderTarget2D blurResult) /*const*/
        {
            // small
            Input.gd.SetRenderTarget(blurTarget);
            CwaCommon.Texture.DrawSprite(Input.sb, tex, blurTarget.Width, blurTarget.Height, BlendState.AlphaBlend);
            Input.gd.SetRenderTarget(null);

            // resize to large, simulate blur
            Input.gd.SetRenderTarget(blurResult);
            CwaCommon.Texture.DrawSprite(Input.sb, blurTarget, blurResult.Width, blurResult.Height, BlendState.AlphaBlend);
            Input.gd.SetRenderTarget(null);
        }

        void CalcBlurs(Texture2D tex)
        {
            for (int i = 0; i < _BlurDenominators.Length; ++i)
            {
                CalcBlur(tex, _BlurTargets[i], _BlurResults[i]);
            }
        }



        void InitPixelate()
        {
            InitPixelateResult();
            InitPixelateResultSmall();
        }

        void InitPixelateResult()
        {
            if (Input.ptBounds == _PixelateResultSize)
                return;

            _PixelateResultSize = Input.ptBounds;

            _PixelateResult = new RenderTarget2D(Input.gd, _PixelateResultSize.X, _PixelateResultSize.Y, false,
                                                 Input.gd.PresentationParameters.BackBufferFormat,
                                                 Input.gd.PresentationParameters.DepthStencilFormat,
                                                 Input.gd.PresentationParameters.MultiSampleCount,
                                                 RenderTargetUsage.DiscardContents);
        }

        void InitPixelateResultSmall()
        {
            if (_PixelateResultSizeSmall == Input.ptBoundsNext)
                return;

            _PixelateResultSizeSmall = Input.ptBoundsNext;

            _PixelateResultSmall = new RenderTarget2D(Input.gd, 
                                                      _PixelateResultSizeSmall.X,
                                                      _PixelateResultSizeSmall.Y);
        }

        void CalcPixelate(Texture2D tex)
        {
            Input.gd.SetRenderTarget(_PixelateResultSmall);
            Input.gd.Clear(Color.Transparent);
            Input.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.sb.Draw(tex, new Rectangle(0, 0, _PixelateResultSmall.Width, _PixelateResultSmall.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);

            Input.gd.SetRenderTarget(_PixelateResult);
            Input.gd.Clear(Color.Transparent);
            Input.sb.Begin(SpriteSortMode.Deferred,
                           BlendState.AlphaBlend,
                           SamplerState.PointClamp, // here happens the magic in the SamplerState.PointClamp
                           null,
                           null);
            Input.sb.Draw(_PixelateResultSmall, new Rectangle(0, 0, _PixelateResult.Width, _PixelateResult.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);
        }
    }
}