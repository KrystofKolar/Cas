using Cas;
using CasBase;
using Cwa.Cas;
using CwaScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cwa
{

// This is the "menu" with a create icon

class NoteMenuBlank : IDisposable
{
    bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposeManual)
    {
        if (_disposed)
        {
            return;
        }

        if (disposeManual)
        {
            // managed resources
            Result.Dispose();
            _Background.Dispose();
            _ButtonCreate.Dispose();
        }

        // unmanaged resources

        _disposed = true;
    }

    CwaScreensManager _sm;

    public RenderTarget2D Result { get; private set; }
    Texture2D _Background;
    Texture2D _ButtonCreate;

    Regions<casId> _Clickable;

    Vector2 _posCreate;
    Boundary _rCreate;

    bool _update;

    public NoteMenuBlank()
    {

    }

    public void LoadContent(CwaScreensManager ScreensManager)
    {
        _sm = ScreensManager;

        _Background =   _sm.Content.Load<Texture2D>("Background/Mark/FrameNoteMenuBlankBackground");
        _ButtonCreate = _sm.Content.Load<Texture2D>("Background/Mark/FrameNoteMenuBlankButtonCreate");

        Result = new RenderTarget2D(_sm.GraphicsDevice, _Background.Width, _Background.Height);

        _posCreate = Vector2.Zero;
        _rCreate = new Boundary(_posCreate, _ButtonCreate.Width, _ButtonCreate.Height);

        _Clickable = new Regions<casId>();
        _Clickable.Add(casId.markDetailEmpty_Create, _rCreate);

        _update = true;
    }

    public void Update(GameTime gt)
    {
        if (_update == false)
        {
            return;
        }

        _sm.GraphicsDevice.SetRenderTarget(Result);
        _sm.GraphicsDevice.Clear(Color.Transparent);
        _sm.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        _sm.SpriteBatch.Draw(_Background, Vector2.Zero, Color.AliceBlue);
        _sm.SpriteBatch.Draw(_ButtonCreate, _posCreate, Color.White);

        _sm.SpriteBatch.End();
        _sm.GraphicsDevice.SetRenderTarget(null);

        _update = false;
    }

    public casId Test(Vector2 vClick)
    {
        return _Clickable.Test(vClick);
    }

}
}
