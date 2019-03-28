using Cas;
using CasGfxItem;

using Cwa.Cas;

using CwaNotes;
using CwaScreenSystem;
using CwaSplineCubic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cwa
{
    public class CwaNoteStatsImage : IDisposable
    {
        #region dispose
        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool manual)
        {
            if (_disposed)
            {
                return;
            }

            if (manual)
            {
                if (_rt != null)
                    _rt.Dispose();

                if (_basicEffect != null)
                    _basicEffect.Dispose();

                if (_canvas != null)
                    _canvas.Dispose();
            }

            _disposed = true;
        }
        #endregion

        CwaScreensManager _mgr;

        // Using an own SpriteBatch, because a BackgroundWorker
        // will call this class and 
        // the wrong call order SpriteBatch.Begin/End can occur.
        SpriteBatch _sb;

        // Summary:
        //     Enable image update/calculate
        public bool Enabled
        {
            get;
            set;
        }

        // Summary:
        //     Image was enabled and calculated. Ready to be shown.
        public bool Ready { get; private set; }

        RenderTarget2D _rt;

        // Summary:
        //     Image center
        public Vector2 center;

        // Summary:
        //     Image position
        public Vector2 pos { get; set; }

        Texture2D _canvas;

        Color[] _pixels;

        List<int> _years;

        List<Vector2> _posYears;

        int yearprev = -1;

        Vector2 posNotes;
        Vector2 posLnZero; // zero positio

        Dictionary<String, CwaNote> _Notes; // extern, throws "InvalidOperation_EnumFailedVersion" in foreach, care when multi threading
        Dictionary<String, float> _NotesXValues;


        Matrix _worldMatrix;
        Matrix _viewMatrix;
        Matrix _projectionMatrix;

        BasicEffect _basicEffect;

        int _yearRendered;

        VertexPositionColor[] _pointList;
        short[] _lineListIndices;

        List<TexXyz> _txlistNotes;
        List<Vector2> _posListNotes;

        TexXyz[] _txYearsDif;
        Color _clrYearDif = Color.White * .7F;

        TexXyz[] _txlogScale;

        const int ROUND = 3;

        const int IDX_MID = 10; // index position of zero, keep sync with logs2
        const int IDX_LAST = NUMS - 1;
        const int NUMS = 2 * IDX_MID + 1; // # of items in the scale

        const float _scale = .7F;
        static Vector2 _vScale = new Vector2(_scale, _scale);

        Texture2D _texCircle;
        const int _SCALENUM = 800 / NUMS; //todo magic value is the screenwidth

        Color _clrGlobal = Color.RoyalBlue * 1F;
        Color _clrYearNotes = Color.White * .5F;

        public CwaNoteStatsImage()
        {
            _yearRendered = -1;
            _NotesXValues = new Dictionary<String, float>();

            _posYears = new List<Vector2>();
            _years = new List<int>();

            _txlistNotes = new List<TexXyz>();
            _posListNotes = new List<Vector2>();

            Enabled = false;
            Ready = false;
        }

#if DEBUG
        void Initialize()
        {
            int n = 300;
            int height = 100;
            int minY = 100;

            _pointList = new VertexPositionColor[n];

            for (int i = 0; i < n; i++)
            {
                _pointList[i] = new VertexPositionColor()
                    {
                        Position = new Vector3(i,
                                               (float)(Math.Sin((i / 15.0)) * height / 2.0 + height / 2.0 + minY),
                                               0),
                        Color = Color.Blue
                    };
            }

            //links the points into a list
            _lineListIndices = new short[(n * 2) - 2];
            for (int i = 0; i < n - 1; i++)
            {
                _lineListIndices[i * 2] = (short)i;
                _lineListIndices[i * 2 + 1] = (short)(i + 1);
            }

            _worldMatrix = Matrix.Identity;

            _viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), // cameraPosition
                                             Vector3.Zero,                   // cameraTarget
                                             Vector3.Up);                    // cameraUpVector

            _projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0,                                                // left
                (float)_mgr.GraphicsDevice.Viewport.Width,        // right
                (float)_mgr.GraphicsDevice.Viewport.Height,       // bottom
                0,                                                // top
                1.0f,                                             // zNearPlane
                1000.0f);                                         // zFarPlane

            _basicEffect = new BasicEffect(_mgr.GraphicsDevice);
            _basicEffect.World = _worldMatrix;
            _basicEffect.View = _viewMatrix;
            _basicEffect.Projection = _projectionMatrix;

            _basicEffect.VertexColorEnabled = true;
        }
#endif

        void LoadContentScale()
        {
            _txlogScale = new TexXyz[NUMS];

            for (int i = 0; i < NUMS; ++i)
            {
                _txlogScale[i] = new TexXyz();
                _txlogScale[i].LoadContent_CAS_SizeM_UnsignedX(_mgr.GraphicsDevice, _sb, _mgr.Content);
            }
        }

        // Build textures around a center date
        void CalculateBuildScaleTextures(DateTime dtCenter)
        {
            _txlogScale[IDX_MID].Update(dtCenter.Year);

            int[] logs2 = { 512, 256, 128, 64, 32, 16, 8, 4, 2, 1 }; // 10 pcs //todo magic

            int idx = 0;
            bool consume;
            DateTime dt;

            for (int i = 0; i < IDX_MID; ++i)
            {
                // update TexXy the lower and upper

                dt = dtCenter.AddYears(-logs2[i]);
                _txlogScale[i].Update(dt.Year);
                consume = _txlogScale[i].UpdateConsume;

                dt = dtCenter.AddYears(logs2[i]);
                _txlogScale[IDX_LAST - i].Update(dt.Year);
                consume = _txlogScale[idx].UpdateConsume;
            }
        }

        void DisposeNums() //todo not used
        {
            if (_txlogScale != null)
            {
                for (int i = 0; i < _txlogScale.Length; ++i)
                {
                    if (_txlogScale[i] != null)
                        _txlogScale[i].Dispose();
                }
            }

            if (_txYearsDif != null)
            {
                for (int i = 0; i < _txYearsDif.Length; ++i)
                {
                    if (_txYearsDif[i] != null)
                        _txYearsDif[i].Dispose();
                }
            }

            if (_txlistNotes != null)
            {
                foreach (var item in _txlistNotes)
                {
                    if (item != null)
                        item.Dispose();
                }
            }
        }

        void CalculateAndDrawLines()
        {
            float[] xs = new float[_NotesXValues.Count]; // list of x values
            Dictionary<float, String> ids = new Dictionary<float, String>(); // map x to id

            Vector2 vLastLeft = new Vector2(-100, 0);
            Vector2 vLastRight = vLastLeft;

            _posYears.Clear();
            _years.Clear();

            _txlistNotes.Clear();
            _posListNotes.Clear();

            // build: xs to hold x values
            //        dIds to hold x->id
            int m = 0;

            foreach (var n in _NotesXValues)
            {
                float x = (float)Math.Round(n.Value, ROUND);

                xs[m++] = x; // note x values

                if (ids.ContainsKey(x) == false)
                {
                    ids.Add(x, n.Key); // note id foreach x value
                }
            }

            Array.Sort(xs);

            // now we've the x values and we know which x value is which note...

            for (int j = 0; j < xs.Length -1; ++j) // draw from left to right - xs is sorted
            {
                float left, right;

                left = xs[j];
                right = xs[j + 1];

                float width = right - left;
                float h = posLnZero.Y - 25;

                if (width > 22)
                #region draw parametric line
                {
                    // draw parametric line

                    // Ranges for line
                    float half = left + width / 2;
                    float[] ixs = { left, half, right };
                    float[] iys = { posLnZero.Y, h, posLnZero.Y };

                    float[] xout, yout;

                    int nOutputPoints = (int)Math.Max(10, Math.Round(width / 4, 0));

                    CubicSpline.FitParametric(ixs, iys, nOutputPoints, out xout, out yout);

                    for (int k = 0; k < xout.Length - 1; ++k)
                    {
                        Vector2 pt1 = new Vector2(xout[k], yout[k]);
                        Vector2 pt2 = new Vector2(xout[k + 1], yout[k + 1]);

                        DrawLine(pt1, pt2, _clrGlobal);
                    }

                    // draw years

                    Vector2 pt = new Vector2(left + width / 2, posLnZero.Y - 25 - 14); //todo magic

                    _posYears.Add(pt);

                    // The current left, right x-values

                    float a = left; 
                    float b = right;

                    if (ids.ContainsKey(a) && ids.ContainsKey(b))
                    {
                        //String[] id = { ids[a], ids[b] }; // note ids

                        TimeSpan ts;
                        DateTime[] dts = { _Notes[ids[a]].dtUnified, _Notes[ids[b]].dtUnified };

                        if (dts[0] > dts[1])
                            ts = dts[0] - dts[1];
                        else
                            ts = dts[1] - dts[0];

                        DateTime dt = DateTime.MinValue + ts;
                        _years.Add(Math.Max(1, dt.Year - 1));

                        DateTime dtLeft = dts[0];
                        DateTime dtRight = dts[1];

                        if (dtLeft.Year != yearprev)
                        {
                            TexXyz texLeft = new TexXyz();
                            texLeft.LoadContent_CAS_SizeM_UnsignedX(_mgr.GraphicsDevice, _sb, _mgr.Content);
                            texLeft.Update(dtLeft.Year);

                            _txlistNotes.Add(texLeft);
                            _posListNotes.Add(new Vector2(left, posNotes.Y));
                        }

                        TexXyz texRight = new TexXyz();
                        texRight.LoadContent_CAS_SizeM_UnsignedX(_mgr.GraphicsDevice, _sb, _mgr.Content);
                        texRight.Update(dtRight.Year);

                        _txlistNotes.Add(texRight);
                        _posListNotes.Add(new Vector2(right, posNotes.Y));

                        yearprev = dtRight.Year;
                    }
#if DEBUG
                    else
                    {
                        Debug.WriteLine("Internal error CwaNoteStatsImage CalculateDrawLines missing key");
                    }
#endif

                } // if draw
                #endregion
                else
                {
                    // curve is too small, so just draw a small line
                    DrawLine(new Vector2(left, posLnZero.Y), new Vector2(right, posLnZero.Y), _clrGlobal);
                }
            }

            for (int i = 1; i < _posListNotes.Count; ++i)
            {
                float w = _posListNotes[i].X - _posListNotes[i - 1].X;

                if (w < 30)
                {
                    Vector2 left = _posListNotes[i];
                    left.X += 7;
                    _posListNotes[i] = left;

                    Vector2 right = _posListNotes[i - 1];
                    right.X -= 7;
                    _posListNotes[i-1] = right;
                }
            }

            // calculate the years difference textures

            _txYearsDif = new TexXyz[_years.Count];

            for (int w = 0; w < _years.Count; ++w)
            {
                if (_years[w] > 0)
                {
                    _txYearsDif[w] = new TexXyz();
                    _txYearsDif[w].LoadContent_CAS_SizeM_UnsignedX(_mgr.GraphicsDevice, _sb, _mgr.Content);
                    _txYearsDif[w].Update(_years[w]);
                    bool updated = _txYearsDif[w].UpdateConsume;
                }
            }
        }

        // Calc x values of all notes
        void CalculateNotesXValues(DateTime dtZero)
        {
            _NotesXValues.Clear();

            DateTime dt;
            TimeSpan ts;
            bool isPast;

            foreach (var kvp in _Notes)
            {
                dt = kvp.Value.dtUnified;

                if (dt > dtZero)
                {
                    ts = dt - dtZero;
                    isPast = false;
                }
                else
                {
                    ts = dtZero - dt;
                    isPast = true;
                }

                double years = ts.TotalDays / (int)CwaCommon.eDatetime.eDaysPerYear;
                years = (double)Math.Round(years, ROUND);

                if (years != 0)
                {
                    double posLnd = Math.Log(years, 2);

                    if (posLnd < 1)// log < 1
                    {
                        double su = _SCALENUM * .1F;
                        years *= 10;

                        years = (years + 1) * su;
                    }
                    else
                    {
                        years = (posLnd + 1) * _SCALENUM;
                    }

                    if (isPast)
                    {
                        years *= -1;
                    }
                }

                _NotesXValues[kvp.Key] = (float)Math.Round(posLnZero.X + (float)years, ROUND);
            }
        }

        // draw texture at positions in _NotesLog
        void DrawNotes()
        {
            foreach (var item in _NotesXValues)
            {
                float x = item.Value;

                _sb.Draw(_texCircle,
                                      new Vector2(x, posLnZero.Y),
                                      null,
                                      _clrGlobal,
                                      0F,
                                      CwaCommon.CwaMathHelper.PointToVector2(_texCircle.Bounds.Center),
                                      .3F,
                                      SpriteEffects.None,
                                      0F);
            }
        }

        // linear name position, the value itself is log.nat.
        void DrawScale()
        {
            Texture2D texLeft;
            Texture2D texRight;

            const float rot = -MathHelper.PiOver2; //ccw
            Color clrScale = Color.White * .5F;

            // draw the center digit
            texLeft = _txlogScale[IDX_MID].Texture;
            _sb.Draw(texLeft,
                     posNotes,
                     null,
                     Color.Red * .5F,
                     rot,
                     CwaCommon.CwaMathHelper.PointToVector2(texLeft.Bounds.Center),
                     _vScale,
                     SpriteEffects.None,
                     0F);

            //draw left, then right digits
            int leftright = 1;

            for (int i = IDX_MID -1; i >= 0 ; --i)
            {
                texLeft = _txlogScale[i].Texture;

                int idx = IDX_MID * 2 -i;
                texRight = _txlogScale[idx].Texture;

                _sb.Draw(texLeft,
                         posNotes - new Vector2(leftright * _SCALENUM, 0),
                         null,
                         clrScale,
                         rot,
                         CwaCommon.CwaMathHelper.PointToVector2(texLeft.Bounds.Center),
                         _vScale,
                         SpriteEffects.None,
                         0F);

                _sb.Draw(texRight,
                         posNotes + new Vector2(leftright * _SCALENUM, 0),
                         null,
                         clrScale,
                         rot,
                         CwaCommon.CwaMathHelper.PointToVector2(texRight.Bounds.Center),
                         _vScale,
                         SpriteEffects.None,
                         0F);

                leftright++;
            }

        }

        // draw line between endpoint
        void DrawLine(Vector2 a, Vector2 b, Color clr, float r = 1F)
        {
            bool canvasNeedsUpdate = false;

            int yMinUpdate = Int32.MaxValue;
            int yMaxUpdate = 0;

            // get the y min/max values
            int yMin = (int)(Math.Min(a.Y, b.Y) - r - 1);
            int yMax = (int)(Math.Max(a.Y, b.Y) + r + 1);

            // make sure min/max values are inside canvas
            yMin = Math.Max(0, Math.Min(_canvas.Height, yMin));
            yMax = Math.Max(0, Math.Min(_canvas.Height, yMax));

            List<float> xCollection = new List<float>();
            RoundedCappedLine line = new RoundedCappedLine(a, b, r);

            for (int y = yMin; y < yMax; ++y)
            {
                xCollection.Clear();
                line.GetAllX(y, xCollection);

                if (xCollection.Count == 2)
                {
                    int xMin = (int)(Math.Min(xCollection[0], xCollection[1]) + .5F);
                    int xMax = (int)(Math.Max(xCollection[0], xCollection[1]) + .5F);

                    // make sure x values are inside canvas and not below "0"
                    xMin = Math.Max(0, Math.Min(_canvas.Width, xMin));
                    xMax = Math.Max(0, Math.Min(_canvas.Width, xMax));

                    for (int x = xMin; x < xMax; x++)
                    {
                        _pixels[y * _canvas.Width + x] = clr;
                    }

                    yMinUpdate = Math.Min(yMinUpdate, yMin);
                    yMaxUpdate = Math.Max(yMaxUpdate, yMax);

                    if (!canvasNeedsUpdate)
                    {
                        canvasNeedsUpdate = true;
                    }
                }
            }

            if (canvasNeedsUpdate)
            {
                int height = yMaxUpdate - yMinUpdate;
                Rectangle rect = new Rectangle(0, yMinUpdate, _canvas.Width, height);
                _canvas.SetData<Color>(0, rect, _pixels,
                                      yMinUpdate * _canvas.Width, height * _canvas.Width);
            }
        }

        // draw difference between years
        void DrawYearsDiff()
        {
            int i = 0;

            foreach(var item in _txYearsDif)
            {
                _sb.Draw(item.Texture,
                                      _posYears[i++],
                                      null,
                                      _clrYearDif,
                                      0f,
                                      CwaCommon.CwaMathHelper.PointToVector2(item.Texture.Bounds.Center),
                                      _vScale,
                                      SpriteEffects.None,
                                      0f);
            }
        }

        // draw years of note
        void DrawYearsNote()
        {
            int i = 0;

            foreach (var item in _txlistNotes)
            {
                _sb.Draw(item.Texture,
                                      _posListNotes[i++],
                                      null,
                                      _clrYearNotes,
                                      -MathHelper.PiOver2,
                                      CwaCommon.CwaMathHelper.PointToVector2(item.Texture.Bounds.Center),
                                      _vScale,
                                      SpriteEffects.None,
                                      0f);
            }
        }

        public void LoadContent(CwaScreensManager mgr)
        {
            _mgr = mgr;
            _sb = new SpriteBatch(_mgr.GraphicsDevice);
            _rt = new RenderTarget2D(_mgr.GraphicsDevice, 
                                     _mgr.GraphicsDevice.Viewport.Width,
                                     _mgr.GraphicsDevice.Viewport.Height / 2);

            center = CwaCommon.CwaMathHelper.PointToVector2(_rt.Bounds.Center);

            posNotes = new Vector2(pos.X, pos.Y / 2);
            posLnZero = posNotes + new Vector2(0, -50); // little bit below todo magic

            LoadContentScale();

            _texCircle = _mgr.Content.Load<Texture2D>("Background/Mark/PointFrontS");
        }

        // more a initialise of the canvas, but for the sake of naming conventions
        void CalculateCanvas()
        {
            if (_canvas == null)
            {
                _canvas = new Texture2D(_mgr.GraphicsDevice, _rt.Width, _rt.Height);
            }

            if (_pixels == null)
            {
                _pixels = new Color[_canvas.Width * _canvas.Height];
            }

            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = Color.Transparent;
            }
        }

        public void EventOccured_StatsRecalculated(Dictionary<string, CwaNote> Notes, DateTime dt, bool forceUpdate) 
        {
            Debug.WriteLine("CwaNoteStatsImage::EventOccured_StatsRecalculated called");
            _Notes = Notes;

            Ready = false;
            Update(dt, forceUpdate);
        }

        // Enabled overrides forceUpdate !
        private void Update(DateTime dtMiddle, bool forceUpdate)
        {
            if (!Enabled)
            {
                return;
            }

            if (forceUpdate == false && _yearRendered == dtMiddle.Year)
            {
                return;
            }

            _yearRendered = dtMiddle.Year;

            if (_yearRendered < 500) //todo
            {
                dtMiddle = new DateTime(500, dtMiddle.Month, dtMiddle.Day, dtMiddle.Hour, dtMiddle.Minute, dtMiddle.Second);
                Debug.WriteLine("not supported lower limit in CwaNoteStatsImage");
            }

                CalculateBuildScaleTextures(dtMiddle);

                CalculateNotesXValues(dtMiddle);

                CalculateCanvas();

                CalculateAndDrawLines();

                _mgr.GraphicsDevice.SetRenderTarget(_rt);
                _mgr.GraphicsDevice.Clear(Color.Transparent);

                #region old
                if (false) //todo draw pointlist
                {
                    foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        _mgr.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                            PrimitiveType.LineList,
                            _pointList,
                            0,
                            _pointList.Length,
                            _lineListIndices,
                            0,
                            _pointList.Length - 1
                        );
                    }
                }
                #endregion

                _sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                    _sb.Draw(_canvas, Vector2.Zero, Color.White);
                    DrawScale();
                    DrawNotes();
                    DrawYearsDiff();
                    DrawYearsNote();

                _sb.End();
                _mgr.GraphicsDevice.SetRenderTarget(null);

                Ready = true;
        }

        public Texture2D Result
        {
            get { return _rt; }
        }
    }
}
