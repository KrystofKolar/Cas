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
        public abstract Effect Effect { get; }

        public InputBase Input;

        protected RenderTarget2D BuildRenderTarget2D(Point size)
            => new RenderTarget2D(Input.graphicsDevice,
                                  width: size.X,
                                  height: size.Y,
                                  mipMap: false,
                                  Input.graphicsDevice.PresentationParameters.BackBufferFormat,
                                  Input.graphicsDevice.PresentationParameters.DepthStencilFormat,
                                  Input.graphicsDevice.PresentationParameters.MultiSampleCount,
                                  RenderTargetUsage.DiscardContents);

        public abstract void Prepare();

        public abstract void Calc(Texture2D tex);

        public virtual RenderTarget2D Result 
        { 
            get;
            protected set;
        }

        #region dispose
        protected bool _disposed = false;

        public virtual void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
                if (Result != null)
                    Result.Dispose();

            _disposed = true;
        }
        #endregion

    }
}