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
        private static int XB, YB, XR=0, YR=0, XRMapped, YRMapped, XBMapped, YBMapped, ZR, ZB;
        public static double[] ObjectFrame = {0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1};
        private double Rxcoord, Rycoord, Bxcoord, Bycoord; //stores coordinate info
        private double[] RedPoint = new double[3], BluePoint = new double[3];
        private int i = 1, j = 0, k = 0, m = 0;
        public static double anglex = 0, angle1=0, angle2 = 0, angle3 = 0;

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
                Imageprocessing.OutputScreen(Outputstream, ObjectFrame);
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

                double vertF = 571.401, horzF = 557.274; //Focal lengths

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
                        if (h % 640 != 0)
                        {
                            XRMapped = h % 640;
                        }

                        YRMapped = (h - XR) / 640;

                        //Red coordinates
                        ZR = this.depthPixels[(640 - XRMapped) + (YRMapped * 640)].Depth;

                        Rxcoord = (ZR * (320 - XRMapped)) / horzF;
                        Rycoord = (ZR * (240 - YRMapped)) / vertF;

                        RCoordX.Content = Math.Round(Rxcoord);
                        RCoordY.Content = Math.Round(Rycoord);
                        RCoordZ.Content = ZR;
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
                        if (h % 640 != 0)
                        {
                            XBMapped = h % 640;
                        }
                        YBMapped = (h - XB) / 640;

                        //Red coordinates
                        ZB = this.depthPixels[(640 - XBMapped) + (YBMapped * 640)].Depth;

                        Bxcoord = (ZB * (320 - XBMapped)) / horzF;
                        Bycoord = (ZB * (240 - YBMapped)) / vertF;

                        BCoordX.Content = Math.Round(Bxcoord);
                        BCoordY.Content = Math.Round(Bycoord);
                        BCoordZ.Content = ZB;
                        break;
                    }
                }

                //Set stream to image
                Depthstream.Source = bmap;

                //Add points to imageviews for debugging
                Canvas1.Children.Clear();
                Canvas2.Children.Clear();

                System.Windows.Shapes.Ellipse DepthPointRed = CreateEllipse.CircleRed();
                System.Windows.Shapes.Ellipse DepthPointBlue = CreateEllipse.CircleBlue();
                System.Windows.Shapes.Ellipse ColorPointRed = CreateEllipse.CircleRed();
                System.Windows.Shapes.Ellipse ColorPointBlue = CreateEllipse.CircleBlue();

                Canvas2.Children.Add(ColorPointRed);
                Canvas2.Children.Add(ColorPointBlue);

                Canvas1.Children.Add(DepthPointRed);
                Canvas1.Children.Add(DepthPointBlue);

                DepthPointRed.SetValue(Canvas.LeftProperty, (depthFrame.Width - XRMapped -3)*.6);
                DepthPointRed.SetValue(Canvas.TopProperty, (YRMapped-3) *.6);

                DepthPointBlue.SetValue(Canvas.LeftProperty, (depthFrame.Width - XBMapped-3) * .6);
                DepthPointBlue.SetValue(Canvas.TopProperty, (YBMapped-3) * .6);

                ColorPointRed.SetValue(Canvas.LeftProperty, (depthFrame.Width - XR-3) * .6);
                ColorPointRed.SetValue(Canvas.TopProperty, (YR-3) * .6);

                ColorPointBlue.SetValue(Canvas.LeftProperty, (depthFrame.Width - XB-3) * .6);
                ColorPointBlue.SetValue(Canvas.TopProperty, (YB-3) * .6);

                //Cleanup
                depthFrame.Dispose();
                CoordinateFrameCalc();
            }
        }

        private void CoordinateFrameCalc()
        {
            RedPoint[0] = Rxcoord;
            RedPoint[1] = Rycoord;
            RedPoint[2] = ZR;

            BluePoint[0] = Bxcoord;
            BluePoint[1] = Bycoord;
            BluePoint[2] = ZB;

            ObjectFrame[3] = BluePoint[0] + ((RedPoint[0] - BluePoint[0]) / 2);
            ObjectFrame[7] = BluePoint[1] + ((RedPoint[1] - BluePoint[1]) / 2);
            ObjectFrame[11] = BluePoint[2] + ((RedPoint[2] - BluePoint[2]) / 2);

            m = 0;
            foreach (double element in RedPoint)
            {
                RedPoint[m] = BluePoint[m] - RedPoint[m];
                m++;
            }

            m = 0;
            RedPoint[1] = 0;
            double mag = Math.Sqrt(RedPoint[0] * RedPoint[0] + RedPoint[1] * RedPoint[1] + RedPoint[2] * RedPoint[2]);

            foreach (double element in RedPoint)
            {
                RedPoint[m] = RedPoint[m] / mag;
                m++;
            }
            m = 0;

            ObjectFrame[0] = RedPoint[0];
            ObjectFrame[4] = RedPoint[1];
            ObjectFrame[8] = RedPoint[2];

            BluePoint[0] = RedPoint[2] * -1;
            BluePoint[1] = 0;
            BluePoint[2] = RedPoint[0] * 1;

            ObjectFrame[2] = BluePoint[0];
            ObjectFrame[6] = BluePoint[1];
            ObjectFrame[10] = BluePoint[2];

            // Displays the coordinates frame to the debug interface
            if (ObjectFrame[3] != 0 && ObjectFrame[11] != 0) {
                _0.Content = Math.Round(ObjectFrame[0], 2);
                _1.Content = Math.Round(ObjectFrame[1], 2);
                _2.Content = Math.Round(ObjectFrame[2], 2);
                _3.Content = Math.Round(ObjectFrame[3], 2);
                _4.Content = Math.Round(ObjectFrame[4], 2);
                _5.Content = Math.Round(ObjectFrame[5], 2);
                _6.Content = Math.Round(ObjectFrame[6], 2);
                _7.Content = Math.Round(ObjectFrame[7], 2);
                _8.Content = Math.Round(ObjectFrame[8], 2);
                _9.Content = Math.Round(ObjectFrame[9], 2);
                _10.Content = Math.Round(ObjectFrame[10], 2);
                _11.Content = Math.Round(ObjectFrame[11], 2);
                _12.Content = Math.Round(ObjectFrame[12], 2);
                _13.Content = Math.Round(ObjectFrame[13], 2);
                _14.Content = Math.Round(ObjectFrame[14], 2);
                _15.Content = Math.Round(ObjectFrame[15], 2);

                anglex = Math.Atan(ObjectFrame[8] / ObjectFrame[0]);
                angle.Content = String.Format("{0} Degrees", Math.Round(anglex * (180 / Math.PI)));
            }

            // Runs when no pallet is found in the scene
            else { 
                angle.Content = "Pallet Not Found";
                _0.Content = 0;
                _1.Content = 0;
                _2.Content = 0;
                _3.Content = 0;
                _4.Content = 0;
                _5.Content = 0;
                _6.Content = 0;
                _7.Content = 0;
                _8.Content = 0;
                _9.Content = 0;
                _10.Content = 0;
                _11.Content = 0;
                _12.Content = 0;
                _13.Content = 0;
                _14.Content = 0;
                _15.Content = 0;
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
