
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF.Monogame.App.NET472
{
    /// <summary>
    /// Interaction logic for WindowGfx.xaml
    /// </summary>
    public partial class WindowGfx : Window
    {
        public WindowGfx()
        {
            InitializeComponent();

            CombineImages();
        }

        private void CombineImages()
        {
            string[] path = new string[4];
            path[0] = "pack://application:,,,/Content/a.png";
            path[1] = "pack://application:,,,/Content/b.png";
            path[2] = "pack://application:,,,/Content/c.png";
            path[3] = "pack://application:,,,/Content/d.png";

            string pathTile ="combined.png";

            // Loads the images to tile (no need to specify PngBitmapDecoder, the correct decoder is automatically selected)
            BitmapFrame frame1 = BitmapDecoder.Create(new Uri(path[0]), 
                                                      BitmapCreateOptions.None, 
                                                      BitmapCacheOption.OnLoad).Frames.First();
            BitmapFrame frame2 = BitmapDecoder.Create(new Uri(path[1]), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
            BitmapFrame frame3 = BitmapDecoder.Create(new Uri(path[2]), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
            BitmapFrame frame4 = BitmapDecoder.Create(new Uri(path[3]), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();

            // image size
            int imageWidth = frame1.PixelWidth;
            int imageHeight = frame1.PixelHeight;

            // Draws the images into a DrawingVisual component
            // https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/using-drawingvisual-objects
            DrawingVisual drawingVisual = new DrawingVisual();

            // Describes visual content using draw, push, and pop commands.
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.drawingcontext?view=netframework-4.8
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(frame1, new Rect(0, 0, imageWidth, imageHeight));
                drawingContext.DrawImage(frame2, new Rect(imageWidth, 0, imageWidth, imageHeight));
                drawingContext.DrawImage(frame3, new Rect(0, imageHeight, imageWidth, imageHeight));
                drawingContext.DrawImage(frame4, new Rect(imageWidth, imageHeight, imageWidth, imageHeight));
            }

            // Converts the Visual (DrawingVisual) into a BitmapSource
            RenderTargetBitmap bmp = new RenderTargetBitmap(imageWidth * 2,
                                                            imageHeight * 2,
                                                            96,
                                                            96,
                                                            PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            // Creates a PngBitmapEncoder and adds the BitmapSource to the frames of the encoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            // Saves the image into a file using the encoder
            using (Stream stream = File.Create(pathTile))
                encoder.Save(stream);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            using (PdfDocument document = new PdfDocument())
            {
                //Add a page to the document
                PdfPage page = document.Pages.Add();

                //Create PDF graphics for a page
                PdfGraphics graphics = page.Graphics;

                //Set the standard font
                PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

                //Draw the text
                graphics.DrawString("Hello World!!!", font, PdfBrushes.Black, new System.Drawing.PointF(0, 0));

                //Save the document
                document.Save("Output.pdf");
            }
            
        }
    }
}
