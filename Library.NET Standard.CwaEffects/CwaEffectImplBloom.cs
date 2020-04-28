using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CwaEffects
{
    public class CwaEffectImplBloom : CwaEffectImpl
    {
        public override Effect Effect
            => Effect.Bloom;

        private RenderTarget2D _Half;
        private RenderTarget2D _Quarter;
        private RenderTarget2D _Eighth;

        // settings
        private static readonly float _BloomThreshold = 0.25f;
        private static readonly float _BloomIntensity = 2.9f;
        private static readonly int _BlurPasses = 2;

        // result = source - destination
        private  static BlendState BlendStateExtractBrightColors =
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
        private static BlendState BlendStateAdditiveBlur = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,

            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,

            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
        };

        // result = source + (destination * (1 - source))
        private static BlendState BlendStateCombineFinalResult = new BlendState
        {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,

            ColorDestinationBlend = Blend.InverseSourceColor,
            AlphaDestinationBlend = Blend.InverseSourceColor,
        };


        public override void Prepare()
        {
            PresentationParameters pp = Input.graphicsDevice.PresentationParameters;

            InputEffectBloom input = Input as InputEffectBloom;

            int w = input.ResultSize.X;
            int h = input.ResultSize.Y;

            Result = BuildRenderTarget2D(new Point(w, h));

            _Half = new RenderTarget2D(Input.graphicsDevice, 
                                       w/2, 
                                       h/2, 
                                       false, 
                                       pp.BackBufferFormat, 
                                       DepthFormat.None);

            _Quarter = new RenderTarget2D(Input.graphicsDevice, 
                                          w/4, 
                                          h/4, 
                                          false, 
                                          pp.BackBufferFormat, 
                                          DepthFormat.None);

            _Eighth = new RenderTarget2D(Input.graphicsDevice, 
                                         w/8,
                                         h/8, 
                                         false, 
                                         pp.BackBufferFormat, 
                                         DepthFormat.None);
        }

        public override void Calc(Texture2D tex)
        {
            // scale to half size.
            Input.graphicsDevice.SetRenderTarget(_Half);
            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            Input.spriteBatch.Draw(tex, 
                                   new Rectangle(
                                       0, 0, 
                                       _Half.Width, _Half.Height), 
                                   Color.White);
            Input.spriteBatch.End();

            // scale to quarter size
            Input.graphicsDevice.SetRenderTarget(_Quarter);
            Input.graphicsDevice.Clear(new Color(_BloomThreshold, _BloomThreshold, _BloomThreshold));
            Input.spriteBatch.Begin(0, BlendStateExtractBrightColors);
            Input.spriteBatch.Draw(_Half, new Rectangle(0, 0, _Quarter.Width, _Quarter.Height), Color.White);
            Input.spriteBatch.End();

            for (int i = 0; i < _BlurPasses; i++)
            {
                Input.graphicsDevice.SetRenderTarget(_Eighth);
                Input.graphicsDevice.Clear(Color.Black);

                int w = _Quarter.Width;
                int h = _Quarter.Height;

                float brightness = 0.25f;

                // On the first pass, scale brightness to restore full range after the threshold subtraction.
                if (i == 0)
                    brightness /= (1 - _BloomThreshold);

                // On the final pass, apply tweakable intensity adjustment.
                if (i == _BlurPasses - 1)
                    brightness *= _BloomIntensity;

                Color tint = new Color(brightness, brightness, brightness);

                Input.spriteBatch.Begin(SpriteSortMode.Deferred, 
                                        BlendStateAdditiveBlur);

                int j = i;
                Vector2 v = new Vector2(0.5f, 0.5f) * .3f; //todo

                Input.spriteBatch.Draw(_Quarter, v, new Rectangle(j + 1, j + 1, w, h), tint);
                Input.spriteBatch.Draw(_Quarter, v, new Rectangle(-j, j + 1, w, h), tint);
                Input.spriteBatch.Draw(_Quarter, v, new Rectangle(j + 1, -j, w, h), tint);
                Input.spriteBatch.Draw(_Quarter, v, new Rectangle(-j, -j, w, h), tint);

                Input.spriteBatch.End();
            }

            // Combine the original scene and bloom images.
            Input.graphicsDevice.SetRenderTarget(Result);

            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
            Input.spriteBatch.Draw(tex, 
                                   new Rectangle(0, 0, 
                                                 Result.Width, Result.Height), 
                                   Color.White);
            Input.spriteBatch.End();

            Input.spriteBatch.Begin(SpriteSortMode.Deferred, BlendStateCombineFinalResult);
            Input.spriteBatch.Draw(_Quarter, 
                                   new Rectangle(0, 0, 
                                                 Result.Width, Result.Height), 
                                   Color.White);
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
                if (_Half != null)
                    _Half.Dispose();

                if (_Quarter != null)
                    _Quarter.Dispose();

                if (_Eighth != null)
                    _Eighth.Dispose();
            }

            base.Dispose(manual);
        }
        #endregion

    }
}
