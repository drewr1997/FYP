using Microsoft.Kinect;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
using System.Windows.Controls;

// Author: Andrew Ross
// Student ID: 16511676

namespace Pallet_Sensor
{
    public partial class MainWindow : Window
    {
        // Declaring Variables
        public KinectSensor ksensor;                   //Declares kinect
        private WriteableBitmap colorBmap;              //Declares place to store color bitmap
        private DepthImagePixel[] depthPixels;          //Declares place to store depth data
        private byte[] colorPixels;                     //Declares place to store color data
        private static int XB, YB, XR=0, YR=0, XRMapped, YRMapped, XBMapped, YBMapped;
        private int i = 1, j=0, k=0;

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Up)
            {
                Tilt.TiltUp(ksensor);
            }
            if (e.Key == System.Windows.Input.Key.Down)
            {
                Tilt.TiltDown(ksensor);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        //Stream Click Event Handler
        private void Stream_Click(object sender, RoutedEventArgs e)
        {
            //Handling starting of service
            if (this.Stream.Content.ToString() == "Start")
            {
                foreach (var potentialSensor in KinectSensor.KinectSensors)
                {
                    //Checks for a connected Kinect
                    if (potentialSensor.Status == KinectStatus.Connected)
                    {
                        this.ksensor = potentialSensor;
                        break;
                    }
                }

                if (null != this.ksensor)
                {
                    // Turn on the depth stream
                    this.ksensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    // Allocate space to put the depth pixels
                    this.depthPixels = new DepthImagePixel[this.ksensor.DepthStream.FramePixelDataLength];

                    // This is the bitmap that will display
                    this.colorBmap = new WriteableBitmap(this.ksensor.DepthStream.FrameWidth, this.ksensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.Depthstream.Source = this.colorBmap;

                    // Event handler to be called whenever there is new depth frame data
                    this.ksensor.DepthFrameReady += this.Ksensor_DepthFrameReady;

                    // Turn on the color stream
                    this.ksensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                    // Allocate space to put the color pixels
                    this.colorPixels = new byte[this.ksensor.DepthStream.FramePixelDataLength * sizeof(int)];

                    // This is the bitmap that will display
                    this.colorBmap = new WriteableBitmap(this.ksensor.DepthStream.FrameWidth, this.ksensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                    // Set the image we display to point to the bitmap where we'll put the image data
                    this.Colorstream.Source = this.colorBmap;

                    // Event handler to be called whenever there is new depth frame data
                    this.ksensor.ColorFrameReady += this.Ksensor_ColorFrameReady;

                    // Start the sensor
                    try
                    {
                        this.ksensor.Start();
                        this.Stream.Content = ("Stop");
                    }
                    catch (IOException)
                    {
                        this.ksensor = null;
                    }
                }

                if (null == this.ksensor){}
            }

            //Handling stopping of service
            else if (this.Stream.Content.ToString() == "Stop")
            {
                if (this.ksensor != null)
                {
                    this.ksensor.Stop();
                    this.Stream.Content = ("Start");
                }
                else{}
            }
        }

        //Gets color frame from kinect
        private void Ksensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {            
            ColorImageFrame colorFrame = e.OpenColorImageFrame();
            BitmapSource bmap = ImageToBitmap(colorFrame);
            Colorstream.Source = bmap;

            //Each function executed every other time
            if (i == 1)
            {
                Imageprocessing.Proc(bmap, Canvas3, Outputstream);
                XB = Imageprocessing.XBlue;
                YB = Imageprocessing.YBlue;
                XR = Imageprocessing.XRed;
                YR = Imageprocessing.YRed;
                //Sets output screen
                Imageprocessing.OutputScreen(bmap, Outputstream);
                i = 0;
            }
            else
            {

                i = 1;
            }
        }

        //Converts image to bitmap
        BitmapSource ImageToBitmap(ColorImageFrame Image)
        {
            if (Image != null)
            {
                Image.CopyPixelDataTo(this.colorPixels);
                BitmapSource bmap = BitmapSource.Create(Image.Width, Image.Height, 10, 10, PixelFormats.Bgr32, null, this.colorPixels, Image.Width * Image.BytesPerPixel);
                Image.Dispose();
                return bmap;
            }
            else {
                return null;
            }
        }

        //Gets depth info from kinect and casts to a bitmap
        private void Ksensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            DepthImageFrame depthFrame = e.OpenDepthImageFrame();   //Puts Depthframe into Depthframe
            
            //Checks if there is a depthFrame
            if (depthFrame != null)
            {
                // Copy the pixel data from the image to a temporary array
                depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);

                // Get the min and max reliable depth for the current frame
                int minDepth = depthFrame.MinDepth;
                int maxDepth = depthFrame.MaxDepth;

                //Convert depth data to bitmapsource
                short[] pixelData = new short[depthFrame.PixelDataLength];
                depthFrame.CopyPixelDataTo(pixelData);
                
                BitmapSource bmap = BitmapSource.Create(
                    depthFrame.Width,
                    depthFrame.Height,
                    2, 2,
                    PixelFormats.Gray16, null,
                    pixelData,
                    depthFrame.Width * depthFrame.BytesPerPixel);

                double Rxcoord, Rycoord, Bxcoord, Bycoord; //stores coordinate info
                double vertF = 609.275495, horzF = 589.3666835; //Focal lengths

                ColorImagePoint[] color = new ColorImagePoint[depthFrame.PixelDataLength];
                ksensor.CoordinateMapper.MapDepthFrameToColorFrame(DepthImageFormat.Resolution640x480Fps30, this.depthPixels, ColorImageFormat.RgbResolution640x480Fps30, color);

                //Seraches mapped RED coordinates
                for (k = 0; k < 640; ++k)
                {
                    if (color[k].X == XR)
                    {
                        break;
                    }
                }
                for (int h = k; h < depthFrame.PixelDataLength; h += 640)
                {
                    if (color[h].Y == YR)
                    {
                        XRMapped = h % 640;
                        YRMapped = (h - XR) / 640;

                        //Red coordinates
                        int Rzcoord = this.depthPixels[(640 - XRMapped) + (YRMapped * 640)].Depth;

                        Rxcoord = (Rzcoord * (320 - XRMapped)) / horzF;
                        Rycoord = (Rzcoord * (240 - YRMapped)) / vertF;

                        RCoordX.Content = Math.Round(Rxcoord);
                        RCoordY.Content = Math.Round(Rycoord);
                        RCoordZ.Content = Rzcoord;
                        break;
                    }
                }

                //Searches mapped Blue coordinates
                for (j = 0; j < 640; ++j)
                {
                    if (color[j].X == XB)
                    {
                        break;
                    }
                }
                for (int h = j; h < depthFrame.PixelDataLength; h += 640)
                {
                    if (color[h].Y == YB)
                    {
                        XBMapped = h % 640;
                        YBMapped = (h - XB) / 640;

                        //Red coordinates
                        int Bzcoord = this.depthPixels[(640 - XBMapped) + (YBMapped * 640)].Depth;

                        Bxcoord = (Bzcoord * (320 - XBMapped)) / horzF;
                        Bycoord = (Bzcoord * (240 - YBMapped)) / vertF;

                        BCoordX.Content = Math.Round(Bxcoord);
                        BCoordY.Content = Math.Round(Bycoord);
                        BCoordZ.Content = Bzcoord;
                        break;
                    }
                }

                //Set stream to image
                Depthstream.Source = bmap;

                //Add points to imageviews for debugging
                Canvas1.Children.Clear();
                Canvas2.Children.Clear();

                System.Windows.Shapes.Ellipse DepthPointRed = CreateEllipse.CircleRed();
                System.Windows.Shapes.Ellipse DepthPointYellow = CreateEllipse.CircleYellow();
                System.Windows.Shapes.Ellipse ColorPointRed = CreateEllipse.CircleRed();
                System.Windows.Shapes.Ellipse ColorPointYellow = CreateEllipse.CircleYellow();

                Canvas2.Children.Add(ColorPointRed);
                Canvas2.Children.Add(ColorPointYellow);

                Canvas1.Children.Add(DepthPointRed);
                Canvas1.Children.Add(DepthPointYellow);

                DepthPointRed.SetValue(Canvas.LeftProperty, (depthFrame.Width - XRMapped -3)*.6);
                DepthPointRed.SetValue(Canvas.TopProperty, (YRMapped-3) *.6);

                DepthPointYellow.SetValue(Canvas.LeftProperty, (depthFrame.Width - XBMapped-3) * .6);
                DepthPointYellow.SetValue(Canvas.TopProperty, (YBMapped-3) * .6);

                ColorPointRed.SetValue(Canvas.LeftProperty, (depthFrame.Width - XR-3) * .6);
                ColorPointRed.SetValue(Canvas.TopProperty, (YR-3) * .6);

                ColorPointYellow.SetValue(Canvas.LeftProperty, (depthFrame.Width - XB-3) * .6);
                ColorPointYellow.SetValue(Canvas.TopProperty, (YB-3) * .6);

                //Cleanup
                depthFrame.Dispose();
            }
        }

        //Handles closing of the window
        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (ksensor != null)                    //Checks if there is a kinect connected
            {
                this.ksensor.Stop();                //Stops the kinect sensor
            }
            else{}
        }
    }
}
