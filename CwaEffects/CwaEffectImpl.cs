using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CwaEffects
{


    public abstract class CwaEffectBase : IDisposable
    {
        public abstract eEffect Effect { get; }

        public CwaEffectInput Input;

        protected bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool dispose);

        public void Prepare()
        {
            Init();
        }

        abstract protected void Init();
        abstract public void Calc(Texture2D tex);

        public RenderTarget2D Result { get; protected set; }

        // get a rendertarget with size in arg
        protected RenderTarget2D Helper_Get(Point sz)
        {
            return new RenderTarget2D(Input.gd, sz.X, sz.Y, false,
                                      Input.gd.PresentationParameters.BackBufferFormat,
                                      Input.gd.PresentationParameters.DepthStencilFormat,
                                      Input.gd.PresentationParameters.MultiSampleCount,
                                      RenderTargetUsage.DiscardContents);
        }
    }

    public class CwaEffectNone : CwaEffectBase
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

    public class CwaEffectPixelate : CwaEffectBase
    {
        public override eEffect Effect { get { return eEffect.Pixelate; } }

        RenderTarget2D _rtSmall;

        protected override void Dispose(bool dispose)
        {
            if (_disposed)
                return;

 	        if (dispose)
            {
                if (_rtSmall != null)
                    _rtSmall.Dispose();

                if (Result != null)
                    Result.Dispose();
            }

            _disposed = true;
        }

        protected override void Init()
        {
            _rtSmall = Helper_Get(Input.ptBoundsNext);
            Result = Helper_Get(Input.ptBounds);
        }

        public override void Calc(Texture2D tex)
        {
            // scale down
            Input.gd.SetRenderTarget(_rtSmall);
            Input.gd.Clear(Color.Transparent);
            Input.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Input.sb.Draw(tex, new Rectangle(0, 0, _rtSmall.Width, _rtSmall.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);

            // scale up
            Input.gd.SetRenderTarget(Result);
            Input.gd.Clear(Color.Transparent);
            Input.sb.Begin(SpriteSortMode.Deferred,
                           BlendState.AlphaBlend,
                           SamplerState.PointClamp, // magic
                           null,
                           null);
            Input.sb.Draw(_rtSmall, new Rectangle(0, 0, Result.Width, Result.Height), Color.White);
            Input.sb.End();
            Input.gd.SetRenderTarget(null);
        }
    }
}