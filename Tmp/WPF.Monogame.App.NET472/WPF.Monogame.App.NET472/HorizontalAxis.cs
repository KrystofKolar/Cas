using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WPF.Monogame.App.NET472
{
    public class HorizontalAxis : FrameworkElement
    {
        private Pen mainPen = new Pen(Brushes.Black, 1.0);
        private Pen mainPenRed = new Pen(Brushes.Red, 1.0);
        private double startPoint = 0.0;
        private double endPoint = 600.0;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double dbi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            double s = startPoint * dbi;
            double e = endPoint * dbi;

            //Draw horizontal line from startPoint to endPoint
            drawingContext.DrawLine(mainPen, new Point(s, ActualHeight / 2),
                                             new Point(e, ActualHeight / 2));

            drawingContext.DrawLine(mainPenRed, new Point(0, 0), new Point(ActualWidth, 0));
            drawingContext.DrawLine(mainPenRed, new Point(ActualWidth, 0 ), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawLine(mainPenRed, new Point(ActualWidth, ActualHeight), new Point(0, ActualHeight));
            drawingContext.DrawLine(mainPenRed, new Point(0, ActualHeight), new Point(0, 0));
            // Draw ticks and text
            for (double i = 0.0; i <= e; i++)
            {
                if (i % 50 == 0)
                {
                    // Draw vertical ticks on the horizontal line drawn above.
                    // They are spaced apart by 50 pixels.
                    drawingContext.DrawLine(mainPen, new Point(i, ActualHeight / 2),
                                                     new Point(i, ActualHeight / 1.25));
                    /*
                    // Draw text below every tick
                    FormattedText ft = new FormattedText(
                       (i).ToString(CultureInfo.CurrentCulture),
                                    CultureInfo.CurrentCulture,
                                    FlowDirection.LeftToRight,
                                    new Typeface(new FontFamily("Segoe UI"),
                                        FontStyles.Normal,
                                        FontWeights.Normal,
                                        FontStretches.Normal),
                                    12,
                                    Brushes.Black,
                                    null,
                                    TextFormattingMode.Display);
                    */


                    FormattedText ft2 = new FormattedText(
                        (i).ToString(CultureInfo.CurrentCulture),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                        new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        12,
                        Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    
                    drawingContext.DrawText(ft2, new Point(i, ActualHeight ));

                }

                FormattedText ft96 = new FormattedText(
                    "ABC",
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Verdana"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    12,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                Point pt = new Point(ActualWidth / 2, ActualHeight / 2);
                drawingContext.DrawText(ft96, pt);
            }
        }
    }
}
