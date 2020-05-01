using Microsoft.Xna.Framework.Graphics;
using System;

namespace CwaEffects
{
    public enum Effect
    {
        Blur,
        Bloom,
        Pixelate,
    }

    public class EffectBridge : IDisposable
    {
        public bool EnableCalc = true;

        public InputBase Input
        {
            get => _EffectImpl.Input;
            set => _EffectImpl.Input = value;
        }

        protected CwaEffectImpl _EffectImpl;

        public Effect Request
        {
            get => _EffectImpl.Effect;

            set
            {
                switch (value)
                {
                    case Effect.Blur:
                        _EffectImpl = new CwaEffectImplBlur();
                        break;

                    case Effect.Bloom:
                        _EffectImpl = new CwaEffectImplBloom();
                        break;

                    case Effect.Pixelate:
                        _EffectImpl = new CwaEffectImplPixelate();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public virtual void Prepare()
            => _EffectImpl.Prepare();

        public virtual void Calc(Texture2D tex)
        {
            if (EnableCalc)
                _EffectImpl.Calc(tex);
        }

        public Texture2D Result
            => _EffectImpl.Result;

        #region dispose
        protected bool _disposed = false;

        public void Dispose()
            => Dispose(true);

        virtual protected void Dispose(bool manual)
        {
            if (_disposed)
                return;

            if (manual)
                if (_EffectImpl != null)
                    _EffectImpl.Dispose();

            _disposed = true;
        }
        #endregion

    }
}
