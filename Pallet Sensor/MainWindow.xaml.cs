using Microsoft.Kinect;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;

// Author: Andrew Ross
// Student ID: 16511676

namespace Pallet_Sensor
{
    public partial class MainWindow : Window
    {
        // Declaring Variables
        private KinectSensor ksensor;                   //Declares kinect
        private WriteableBitmap colorBmap;              //Declares place to store color bitmap
        private DepthImagePixel[] depthPixels;          //Declares place to store depth data
        private byte[] colorPixels;                     //Declares place to store color data
        private static int XB, YB, XR, YR;
        private int i = 1;

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
            BitmapSource red, blue;

            //Each function executed every other time
            if (i == 1)
            {
                red = Imageprocessing.Procred(bmap);
                XB = Imageprocessing.XBlue;
                YB = Imageprocessing.YBlue;
                XR = Imageprocessing.XRed;
                YR = Imageprocessing.YRed;
                Processedstream.Source = red;
                i = 0;
            }
            else {
                blue = Imageprocessing.Procblue(bmap);
                Outputstream.Source = blue;
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

                //Red coordinates
                int Rzcoord = (ushort)pixelData[640-XR + YR * depthFrame.Width]; 
                Rzcoord = Rzcoord >> 3;
                Rxcoord = (Rzcoord * (320 - XR)) / horzF;
                Rycoord = (Rzcoord * (240 - YR)) / vertF;

                RCoordX.Content = Math.Round(Rxcoord);
                RCoordY.Content = Math.Round(Rycoord);
                RCoordZ.Content = Rzcoord;

                //Blue coordinates
                int Bzcoord = (ushort)pixelData[640-XB + YB * depthFrame.Width];
                Bzcoord = Bzcoord >> 3;
                Bxcoord = (Bzcoord * (320 - XR)) / horzF;
                Bycoord = (Bzcoord * (240 - YR)) / vertF;

                BCoordX.Content = Math.Round(Rxcoord);
                BCoordY.Content = Math.Round(Rycoord);
                BCoordZ.Content = Rzcoord;

                //Set stream to image
                Depthstream.Source = bmap;

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
