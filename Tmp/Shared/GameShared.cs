//using Cwa;

using CasGfxItem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace Monogame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameShared : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Random R = new Random();
        BarUp _BarUp;
        BarUpDown _BarUpDown;
        BarUpDownFader _BarUpDownFader;
        TimeSpan updateProgressbar = new TimeSpan();
        bool _dirUp = true;

        BackgroundWorker _bw;
#if LINUX
        delegate void DlgtProgressUpdate(GameTime gametime, int pct);
         
        Queue<Tuple<DlgtProgressUpdate, int>> _QueueMethod = 
            new Queue<Tuple<DlgtProgressUpdate, int>>();
        internal static object ThreadLocker = new object();
#endif
        
        private void BackgroundWorkerBuild(ref BackgroundWorker bw, string name)
        {
            Thread.CurrentThread.Name = $"Backgroundworker {name}";

            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += (object sender, DoWorkEventArgs args) =>
            {
                try
                {
                    Action act = args.Argument as Action;
                    act.Invoke();

                    args.Result = $"bw {name} success";
                }
                catch(Exception e)
                {
                    args.Result = $"bw {name} exception";
                }
            };

            bw.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs args) =>
            {
                string str = (string)args.Result;
                //Debug.WriteLine(str);
            };
        }

        public GameShared()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;
            graphics.IsFullScreen = false;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft |
                                                    DisplayOrientation.LandscapeRight;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            Window.IsBorderless = false;
            Window.AllowAltF4 = true;
            Window.Title = "Title";
            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            ContentManager cm = new ContentManager(this.Services);

            //AudioManager.Initialize(this);
            //AudioManager.LoadSound("wav/testnew", eSound.background_sound.ToString(), AudioManager.VolumeLevels.LittleLessLess, true);
            //AudioManager.PlaySound(eSound.background_sound.ToString());

            font = Content.Load<SpriteFont>("testfont");

            InitBar<BarUp>(ref _BarUp, new Vector2(50,0), Color.Red);
            InitBar<BarUpDown>(ref _BarUpDown, new Vector2(50, 150), Color.Green);
            InitBar<BarUpDownFader>(ref _BarUpDownFader, new Vector2(50, 300), Color.Blue);

            BackgroundWorkerBuild(ref _bw, "2");
        }

        private void BackgroundWorkerUpdate()
        {
            ////////////////////////////
            // make big calculations here
            int pct = R.Next(0, 101);
            /////////////////////////////



            // Update has to be on the UI thread on some platforms like OpenGL
            // with DirectX it just works, the "normal" thread is enough.
            // the reason is that the Videomemory gets timing issues/
            // sync. problems, and thats not implented in each Monogame version
            // in the same way.

            // The OpenGL version is safe.

#if WINDOWS
            _LoadingProgressbar2.Update(pct);
#endif
#if LINUX
            lock (ThreadLocker)
            {
                // Put the render item in a queue for the UI thread
                _QueueMethod.Enqueue(
                    new Tuple<DlgtProgressUpdate, int>(_BarUpDown.Update, pct));
            }
#endif
        }

        private void InitBar<T>(ref T bar, Vector2 pos, Color color) where T : BarBase, new()
        {
            bar = new T();
            bar.LoadContent(GraphicsDevice,
                            spriteBatch,
                            Content,
                            new Point(30, 410),
                            "Progressbar/back3",
                            "Progressbar/back3_left",
                            "Progressbar/back3_mid",
                            "Progressbar/back3_right");

            bar.Result.Padding = pos;
            bar.Result.Scale = 1f;
            bar.ColorBar = color;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            updateProgressbar += gameTime.ElapsedGameTime;

            if (updateProgressbar > new TimeSpan(10 * 1000 * 100))
            {
                int d = R.Next(0, 11);

                int p = _BarUp.Bar.p + d;
                if (p >= 100)
                {
                    _dirUp = false;
                    p = 100;
                }
                else if (p <= 0)
                {
                    _dirUp = true;
                    p = 0;
                }

                if (!_dirUp)
                    d *= -1;

                p = _BarUp.Bar.p + d;
                if (p > 100)
                    p = 100;
                else if (p < 0)
                    p = 0;

                _BarUp.Update(gameTime, p);
                _BarUpDown.Update(gameTime, p);
                _BarUpDownFader.Update(gameTime, p);

                if (false && !_bw.IsBusy)
                    _bw.RunWorkerAsync(new Action(BackgroundWorkerUpdate));

                updateProgressbar = new TimeSpan();
            }

#if LINUX
            lock (ThreadLocker)
            {
                if (_QueueMethod.Count > 0)
                {
                    Tuple<DlgtProgressUpdate, int> tuple = _QueueMethod.Dequeue();

                    tuple.Item1.Invoke(gameTime, tuple.Item2);
                }
            }
#endif
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (spriteBatch.BeginEnter())
            {
                try
                {
                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Clear(Color.Gray);
                    spriteBatch.Begin();
#if WINDOWS
                    //spriteBatch.DrawString(font, "Monogame.DX", 200 * Vector2.One, Color.White);
#endif
#if LINUX
                    //spriteBatch.DrawString(font, "Monogame.GL", 200 * Vector2.One, Color.White);
#endif
                    spriteBatch.Draw(_BarUp.Result.Image,
                        _BarUp.Result.Padding,
                        null,
                        Color.White,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        _BarUp.Result.Scale,
                        SpriteEffects.None,
                        layerDepth: 0f);

                    spriteBatch.Draw(_BarUpDown.Result.Image,
                        _BarUpDown.Result.Padding,
                        null,
                        Color.White,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        _BarUpDown.Result.Scale,
                        SpriteEffects.None,
                        layerDepth: 0f);

                    spriteBatch.Draw(_BarUpDownFader.Result.Image,
                        _BarUpDownFader.Result.Padding,
                        null,
                        Color.White,
                        rotation: 0f,
                        origin: Vector2.Zero,
                        _BarUpDownFader.Result.Scale,
                        SpriteEffects.None,
                        layerDepth: 0f);

                    spriteBatch.End();
                }
                finally
                {
                    spriteBatch.BeginLeave();
                }
            }
            
            //base.Draw(gameTime);
        }
    }
}
