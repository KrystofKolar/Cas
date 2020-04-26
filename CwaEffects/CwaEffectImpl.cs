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

        public InputBase Input;

        protected bool _disposed = false;

        public virtual void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
            {
                if (Result != null)
                    Result.Dispose();
            }

            _disposed = true;
        }

        public virtual void Prepare()
            => Init();

        virtual protected void Init() { }

        virtual public void Calc(Texture2D tex) { }

        public RenderTarget2D Result { get; protected set; }

        // get a rendertarget with size in arg
        protected RenderTarget2D GetRenderTarget2D(Point sz)
        {
            return new RenderTarget2D(Input.gd,
                                      sz.X,
                                      sz.Y,
                                      mipMap: false,
                                      Input.gd.PresentationParameters.BackBufferFormat,
                                      Input.gd.PresentationParameters.DepthStencilFormat,
                                      Input.gd.PresentationParameters.MultiSampleCount,
                                      RenderTargetUsage.DiscardContents);
        }
    }
}