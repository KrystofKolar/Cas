using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public enum eEffect
    {
        Org,
        Blur,
        Bloom,
        Pixelate,

        Last,
    }

    public class CwaEffectBridge : IDisposable
    {
        protected bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        virtual protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _EffectBase.Dispose();
            }

            _disposed = true;
        }

        public CwaEffectInput Input;

        protected CwaEffectImpl _EffectBase;

        public eEffect Request
        {
            get
            {
                    return _EffectBase.Effect;
            }

            set
            {
                if (value == Request)
                    return;

                switch (value)
                {
                    case eEffect.Pixelate:
                        _EffectBase = new CwaEffectImplPixelate();
                        break;

                    case eEffect.Bloom:
                        _EffectBase = new CwaEffectImplBloom();
                        break;

                    case eEffect.Blur:
                        _EffectBase = new CwaEffectImplBlur();
                        break;

                    case eEffect.Org:
                    default:
                        _EffectBase = new CwaEffectImplOriginal();
                        break;
                }

                _EffectBase.Input = Input;
            }
        }

        public CwaEffectBridge()
        {
            Input = new CwaEffectInput();
            _EffectBase = new CwaEffectImplOriginal();
        }

        public virtual void Prepare()
        {
            _EffectBase.Prepare();
        }

        public virtual void Calc(Texture2D tex)
        {
            _EffectBase.Calc(tex);
        }

        public Texture2D Result { get { return _EffectBase.Result; } }
    }

}
