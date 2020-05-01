using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CwaEffects;
using System;
using System.Reflection;

namespace CwaEffectsTest
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Point halfScreen;

        EffectBridge _EffectBridge;
        Texture2D tex;

        KeyboardState prevStateKeyboard = Keyboard.GetState();
        MouseState prevmouseState = Mouse.GetState();
        private int prevmouseWheelState;
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
        {
            base.Initialize();

            prevmouseWheelState = Mouse.GetState().ScrollWheelValue;
        }

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
                            PixelsBlurry = new Point(80,48), // the higher, the more blurry
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
                                                   GraphicsDevice.PresentationParameters.BackBufferHeight),
                            BloomThreshold = 0.25f,
                            BloomIntensity = 1.0f,
                            BlurPasses = 5,
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
                        Pixels = new Point(80, 48)
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
            bool needCalc = false;
            bool? KeyInc = null;

            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            //if (Keyboard.GetState().IsKeyDown(Keys.Enter) && 
            //    !prevStateKeyboard.IsKeyDown(Keys.Enter))
            if (mouseState.RightButton == ButtonState.Pressed &&
                prevmouseState.RightButton == ButtonState.Released)
            {
                if (_EffectBridge.Request == CwaEffects.Effect.Pixelate)
                    CreateEffect(CwaEffects.Effect.Blur); // rewind
                else
                    CreateEffect(++_EffectBridge.Request);
            }


            //if (Keyboard.GetState().IsKeyDown(Keys.Up) &&
            //    !prevStateKeyboard.IsKeyDown(Keys.Up))
            if (mouseState.ScrollWheelValue > prevmouseWheelState)
            {
                KeyInc = true;
            }
            //else if (Keyboard.GetState().IsKeyDown(Keys.Down) &&
            //    !prevStateKeyboard.IsKeyDown(Keys.Down))
            else if (mouseState.ScrollWheelValue < prevmouseWheelState)
            {
                KeyInc = false;
            }

            if (KeyInc.HasValue)
            {
                switch (_EffectBridge.Request)
                {
                    case CwaEffects.Effect.Bloom:
                        {
                            //FieldInfo fieldinfo  = _EffectBridge.Input.GetType().GetField("BloomIntensity");
                            //var entry = (float)fieldinfo.GetValue(_EffectBridge.Input);
                            //entry += KeyInc.Value ? 0.1f : -0.1f;

                            InputEffectBloom InputBloom = _EffectBridge.Input as InputEffectBloom;
                            InputBloom.BloomIntensity += KeyInc.Value ? 0.1f : -0.1f;
                            //fptr = (_EffectBridge.Input as InputEffectBloom).BloomIntensity;
                            needCalc = true;
                        }
                        break;
                }
            }

            if (needCalc)
            {
                _EffectBridge.Prepare();
                _EffectBridge.Calc(tex);
            }

            prevmouseState = mouseState;
            prevmouseWheelState = mouseState.ScrollWheelValue; ;
            prevStateKeyboard = Keyboard.GetState();

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
