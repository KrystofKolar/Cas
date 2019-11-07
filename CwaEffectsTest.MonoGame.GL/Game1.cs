using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CwaEffectsTest.MonoGame.GL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector2 measure;
        Vector2 c;
        CwaEffects.CwaEffectBridge _EffectBridge;
        Texture2D tex;
        KeyboardState prevState = Keyboard.GetState();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            c = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);

            Content.RootDirectory = "Content";
        }

        void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            c = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        void GraphicsDevice_DeviceReset(object sender, System.EventArgs e)
        {
            c = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
        }
  
        protected override void Initialize()
        {
            graphics.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("font"); ;
            tex = Content.Load <Texture2D>("test");

            _EffectBridge = new CwaEffects.CwaEffectBridge();

            EffectSettings();
            _EffectBridge.Prepare();
            _EffectBridge.Calc(tex);

            measure = font.MeasureString(_EffectBridge.Request.ToString());
        }

        void EffectSettings()
        {
            _EffectBridge.Request++;

            if (_EffectBridge.Request == CwaEffects.eEffect.Last)
                _EffectBridge.Request = CwaEffects.eEffect.Org;

            _EffectBridge.Input.gd = graphics.GraphicsDevice;
            _EffectBridge.Input.sb = spriteBatch;

            _EffectBridge.Input.ptBounds = new Point(graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                     graphics.GraphicsDevice.PresentationParameters.BackBufferHeight);

            switch (_EffectBridge.Request)
            {
                case CwaEffects.eEffect.Bloom:
                case CwaEffects.eEffect.Blur:
                    _EffectBridge.Input.ptBoundsNext = _EffectBridge.Input.ptBounds;
                    break;

                case CwaEffects.eEffect.Pixelate:
                    _EffectBridge.Input.ptBoundsNext = new Point(112, 70);
                    break;

                default:
                    break;
            }

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter))
            {
                EffectSettings();
                _EffectBridge.Prepare();
                _EffectBridge.Calc(tex);

                measure = font.MeasureString(_EffectBridge.Request.ToString());
            }

            prevState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            int cOver2 = (int)c.X / 2;

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(tex,
                             new Rectangle(0, 0, cOver2, (int)c.Y),
                             Color.White);
            spriteBatch.Draw(_EffectBridge.Result,
                             new Rectangle(cOver2, 0, cOver2, (int)c.Y),
                             Color.White);
            spriteBatch.DrawString(font,
                             _EffectBridge.Request.ToString() + " <ENTER> next",
                             new Vector2(cOver2, 10),
                             Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
