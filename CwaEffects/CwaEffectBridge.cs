using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CwaEffects
{
    public enum eEffect
    {
        Undef,

        None, // original
        Blur,
        Bloom,
        Pixelate
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

        protected CwaEffectBase _EffectBase;

        public eEffect Request
        {
            get
            {
                if (_EffectBase != null)
                    return _EffectBase.Effect;
                else
                    return eEffect.Undef;
            }

            set
            {
                if (value != Request)
                {
                    switch (value)
                    {
                        case eEffect.Pixelate:
                            _EffectBase = new CwaEffectPixelate();
                            break;

                        case eEffect.None:
                        default:
                            _EffectBase = new CwaEffectNone();
                            break;
                    }

                    _EffectBase.Input = Input;
                }
            }
        }

        public CwaEffectBridge()
        {
            Input = new CwaEffectInput();
        }

        public void Prepare()
        {
            _EffectBase.Prepare();
        }

        public void Calc(Texture2D tex)
        {
            _EffectBase.Calc(tex);
        }

        public Texture2D Result { get { return _EffectBase.Result; } }
    }

}
