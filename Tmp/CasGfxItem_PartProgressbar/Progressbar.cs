
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using tRectInt = System.Tuple<Microsoft.Xna.Framework.Rectangle, int>;

namespace CasGfxItem
{
    public class CBar
    {
        public static readonly int pxmin = 10;

        public int p; // percent [0..100]
        public int px; // pixel
        public int pxRendered;
        public int pxSoundPlayed;

        private Point _bar;
        public Point bar
        {
            get => _bar;

            set 
            {
                _bar = value;
                width = value.Y - value.X;
            }
        }
        public int width { get; private set; }

        public List<Tuple<Rectangle,int>> parts = 
            new List<Tuple<Rectangle, int>>();
    }

    public class CResult
    {
        public RenderTarget2D Image
        {
            get;
            set;
        }

        public Vector2 Padding { get; set; }

        public Point Center
        {
            get => Image.Bounds.Center;
        }

        public float Scale { get; set; } = 1f;
    }

    public abstract class BarBase : IDisposable
    {
        protected GraphicsDevice _gd;
        protected SpriteBatch _sb;
        protected ContentManager _cm;

        public CResult Result = new CResult();
        public CBar Bar = new CBar();

        public delegate Color fnColorFloat(float levelAbsolute);
        public delegate bool fnBoolFloat(float loadstate);

        public event fnColorFloat EventProgressColor;
        public event fnBoolFloat EventSound;

        public Color ColorBar { get; set; } = Color.Blue;

        public enum eTex
        {
            back,
            left,
            mid,
            right
        }

        public Texture2D[] _texs = 
            new Texture2D[Enum.GetNames(typeof(eTex)).Count()];

        protected virtual RenderTargetUsage GetRenderTargetUsage()
            => RenderTargetUsage.DiscardContents;

        public virtual void LoadContent(
            GraphicsDevice gd,
            SpriteBatch sb,
            ContentManager cm,
            Point bar,
            string sback,
            string sleft,
            string smid,
            string sright)
        {
            _gd = gd;
            _sb = sb;
            _cm = cm;

            Bar.bar = bar;

            _texs[(int)eTex.back] = _cm.Load<Texture2D>(sback);

            _texs[(int)eTex.left] = _cm.Load<Texture2D>(sleft);
            _texs[(int)eTex.mid] = _cm.Load<Texture2D>(smid);
            _texs[(int)eTex.right] = _cm.Load<Texture2D>(sright);

            PresentationParameters pp = _gd.PresentationParameters;

            Result.Image = new RenderTarget2D(
                                        _gd,
                                        _texs[(int)eTex.back].Width,
                                        _texs[(int)eTex.back].Height,
                                        mipMap: true, //was false
                                        pp.BackBufferFormat,
                                        pp.DepthStencilFormat,
                                        pp.MultiSampleCount,
                                        GetRenderTargetUsage());
        }

        public virtual void Update(GameTime gt, int p)
        {
            if (p < 0 || p > 100)
                throw new NotSupportedException();

            if (Bar.p == p)
                return;

            int px = (int)(Bar.width * p / 100f);

            if (Math.Abs(px - Bar.pxRendered) < CBar.pxmin)
                return;

            // take care about the order !

            if (!_sb.BeginEnter())
                return;

            try
            {
                Bar.p = p;
                Bar.px = px;

                UpdateEventColor();

                UpdateSpriteBatch(gt);
            }
            finally
            {
                _sb.BeginLeave();
            }
            
            UpdateEventSound();
        }

        public abstract void UpdateSpriteBatch(GameTime gt);

        public abstract void UpdateSpriteBatchBar(GameTime gt);

        public virtual void UpdateEventSound()
        {
            if (EventSound == null)
                return;

            int dSound = Bar.px - Bar.pxSoundPlayed;
            int BarWidthOver10 = Bar.width / 10;
            if (dSound > BarWidthOver10)
            {
                EventSound(Bar.p);
                Bar.pxSoundPlayed = Bar.px;

                Debug.WriteLine($"EventSound {Bar.pxSoundPlayed}");
            }
        }

        public virtual void UpdateEventColor()
        {
            if (EventProgressColor == null)
                return;

            ColorBar = EventProgressColor(Bar.p);
        }

        protected bool _disposed = false;

        public virtual void Dispose()
            => Dispose(true);

        public virtual void Dispose(bool dispose)
        {
            if (_disposed)
                return;

            if (dispose)
            {
                if (Result.Image != null)
                    Result.Image.Dispose();

                if (_texs != null)
                    _texs.ToList().ForEach(t => t.Dispose());
            }

            _disposed = true;
        }

    }

    public class BarUp : BarBase
    {
        protected override RenderTargetUsage GetRenderTargetUsage()
            => RenderTargetUsage.PreserveContents;

        public override void LoadContent(GraphicsDevice gd, SpriteBatch sb, ContentManager cm, Point bar, string sback, string sleft, string smid, string sright)
        {
            base.LoadContent(gd, sb, cm, bar, sback, sleft, smid, sright);

            int posRight =
                _texs[(int)eTex.back].Width - _texs[(int)eTex.right].Width;

            if (_sb.BeginEnter())
            {
                try
                {
                    _gd.SetRenderTarget(Result.Image);
                    _gd.Clear(Color.Transparent);
                    _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    _sb.Draw(_texs[(int)eTex.back], Vector2.Zero, Color.White);
                    _sb.Draw(_texs[(int)eTex.left], Vector2.Zero, Color.White);
                    _sb.Draw(_texs[(int)eTex.right], new Vector2(posRight, 0), Color.White);

                    _sb.End();
                    _gd.SetRenderTarget(null);
                }
                finally
                {
                    _sb.BeginLeave();
                }
            }
        }

        public override void UpdateSpriteBatch(GameTime gt)
        {
            _gd.SetRenderTarget(Result.Image);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            UpdateSpriteBatchBar(gt);

            _sb.End();
            _gd.SetRenderTarget(null);
        }

        public override void UpdateSpriteBatchBar(GameTime gt)
        {
            if (Bar.pxRendered > Bar.px)
            {
                //Debug.Write("Bar is not allowed to get smaller");
                //throw new NotSupportedException();
                return; //todo specs
            }

            tRectInt tup = new tRectInt(new Rectangle(Bar.bar.X + Bar.pxRendered,
                                              0,
                                              Bar.px - Bar.pxRendered,
                                              Result.Image.Height),
                                0);

            Random R = new Random();

            Color color = new Color(R.Next(0, 256), R.Next(0, 256), R.Next(0, 256));

            _sb.Draw(_texs[(int)eTex.mid], tup.Item1, color);
            //_sb.Draw(_texs[(int)eTex.mid], 
            //         new Rectangle(30, 0, 380, 162), 
            //         Color.IndianRed * 0.5f);
            Bar.pxRendered = Bar.px;

            Bar.parts.Add(tup);

            if (Bar.parts.Count() > 10)
                Bar.parts.RemoveAt(0);
        }
    }

    public class BarUpDown : BarBase
    {
        public override void UpdateSpriteBatch(GameTime gt)
        {
            int posRight =
                _texs[(int)eTex.back].Width - _texs[(int)eTex.right].Width;

            _gd.SetRenderTarget(Result.Image);
            _gd.Clear(Color.Transparent);
            _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            _sb.Draw(_texs[(int)eTex.back], Vector2.Zero, Color.White);
            _sb.Draw(_texs[(int)eTex.left], Vector2.Zero, Color.White);

            UpdateSpriteBatchBar(gt);

            _sb.Draw(_texs[(int)eTex.right], new Vector2(posRight, 0), Color.White);

            _sb.End();
            _gd.SetRenderTarget(null);
        }

        public override void UpdateSpriteBatchBar(GameTime gt)
        {
            tRectInt tup = new tRectInt(new Rectangle(Bar.bar.X, 0,
                                              Bar.px, Result.Image.Height),
                                0);

            _sb.Draw(_texs[(int)eTex.mid], tup.Item1, ColorBar);

            Bar.pxRendered = Bar.px;

            Bar.parts.Add(tup);

            if (Bar.parts.Count() > 10)
                Bar.parts.RemoveAt(0);
        }
    }

    public class BarUpDownFader : BarUpDown
    {
        public override void UpdateSpriteBatchBar(GameTime gt)
        {
            tRectInt tup =
                new tRectInt(
                    new Rectangle(Bar.bar.X, 0,
                                  Bar.px, Result.Image.Height),
                    0);


            _sb.Draw(_texs[(int)eTex.mid], tup.Item1, ColorBar);

            Bar.pxRendered = Bar.px;

            Bar.parts.Add(tup);

            for (int i = Bar.parts.Count() - 1; i >= 0; --i)
            {
                tRectInt t = Bar.parts[i];

                if (Bar.px > t.Item1.Right)
                {
                    //hidden progressing
                }
                else
                {
                    //hidden getting smaller
                }
            }

            while (Bar.parts.Count() > 10)
                Bar.parts.RemoveAt(0);
        }
    }
}
