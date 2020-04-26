using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public enum eEffect
    {
        Blur,
        Bloom,
        Pixelate,
    }

    public class EffectBridge : IDisposable
    {
        public bool Enabled = false;

        public Input Input;

        protected bool _disposed = false;

        public void Dispose() 
            => Dispose(true);

        virtual protected void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
                _EffectImpl.Dispose();

            _disposed = true;
        }

        protected CwaEffectImpl _EffectImpl;

        public eEffect Request
        {
            get => _EffectImpl.Effect;

            set
            {
                Request = value;

                switch (Request)
                {
                    case eEffect.Pixelate:
                        _EffectImpl = new CwaEffectImplPixelate();
                        break;

                    case eEffect.Bloom:
                        _EffectImpl = new CwaEffectImplBloom();
                        break;

                    case eEffect.Blur:
                        _EffectImpl = new CwaEffectImplBlur();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public virtual void Prepare() 
            => _EffectImpl.Prepare();


        public virtual void Calc(Texture2D tex)
            => _EffectImpl.Calc(tex);

        public Texture2D Result
            => _EffectImpl.Result;
    }

}
