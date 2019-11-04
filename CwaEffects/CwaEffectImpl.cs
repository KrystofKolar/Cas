using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CwaEffects
{

    public abstract class CwaEffectImpl : IDisposable
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




}