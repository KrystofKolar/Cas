using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CwaEffects;
using System;

namespace CwaEffectsTest
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Point halfScreen;

        EffectBridge _EffectBridge;
        Texture2D tex;

        KeyboardState prevState = Keyboard.GetState();

        public Game1()
        {
            int scale = 2;
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 800 * scale;
            graphics.PreferredBackBufferHeight = 400 * scale;
            graphics.ApplyChanges();

            halfScreen = new Point(graphics.PreferredBackBufferWidth / 2,
                                   graphics.PreferredBackBufferHeight);

            IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
            => base.Initialize();

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tex = Content.Load <Texture2D>("test");

            CreateEffect(CwaEffects.Effect.Blur);
        }

        protected void CreateEffect(CwaEffects.Effect effect)
        {
            if (_EffectBridge == null)
                _EffectBridge = new EffectBridge();

            _EffectBridge.Request = effect;

            switch (_EffectBridge.Request)
            {
                case CwaEffects.Effect.Blur:
                    {
                        _EffectBridge.Input = new InputEffectBlur
                        {
                            graphicsDevice = GraphicsDevice,
                            spriteBatch = spriteBatch,
                            ResultSize = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                    GraphicsDevice.PresentationParameters.BackBufferHeight),
                            Denominator = 32,
                        };
                    }
                    break;

                case CwaEffects.Effect.Bloom:
                    {
                        _EffectBridge.Input = new InputEffectBloom
                        {
                            graphicsDevice = GraphicsDevice,
                            spriteBatch = spriteBatch,
                            ResultSize = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                   GraphicsDevice.PresentationParameters.BackBufferHeight)
                        };
                    }
                    break;

                case CwaEffects.Effect.Pixelate:
                    _EffectBridge.Input = new InputEffectPixelate
                    {
                        graphicsDevice = GraphicsDevice,
                        spriteBatch = spriteBatch,
                        ResultSize = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
                                                GraphicsDevice.PresentationParameters.BackBufferHeight),
                        ScaleDownSize = new Point(80, 48)
                    };
                    break;

                default:
                    throw new NotSupportedException();
            }

            _EffectBridge.Prepare();
            _EffectBridge.Calc(tex);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && 
                !prevState.IsKeyDown(Keys.Enter))
            {
                if (_EffectBridge.Request == CwaEffects.Effect.Pixelate)
                    CreateEffect(CwaEffects.Effect.Blur); // rewind
                else
                    CreateEffect(++_EffectBridge.Request);
            }

            prevState = Keyboard.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            spriteBatch.Draw(tex,
                             destinationRectangle: new Rectangle(Point.Zero, halfScreen),
                             sourceRectangle: null,
                             Color.White);

            spriteBatch.Draw(_EffectBridge.Result,
                             destinationRectangle: new Rectangle(new Point(halfScreen.X, 0), halfScreen),
                             sourceRectangle: null,
                             Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
