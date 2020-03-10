using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Drawing;

public class Imageprocessing
{ 
    public static int XRed, YRed, XBlue, YBlue;
    private static Rectangle boundingrectBlue, boundingrectred;
    private static Image<Hsv, Byte> processedred;                   //Casts bitmap to image<Hsv, byte>

    //Red Segmentation
    public static void Proc(BitmapSource Image, Canvas Canvas3, System.Windows.Controls.Image outputimage)
    {
        if (Image != null)
        {
            //Red Processing
            //Converts to image<>
            MemoryStream Streamred = new MemoryStream();
            BitmapEncoder encodedred = new BmpBitmapEncoder();
            encodedred.Frames.Add(BitmapFrame.Create(Image));
            encodedred.Save(Streamred);
            System.Drawing.Bitmap myBmpred = new System.Drawing.Bitmap(Streamred);            //Casts image to bitmap
            processedred = new Image<Hsv, Byte>(myBmpred);                   //Casts bitmap to image<Hsv, byte>

            //Main processing
            CvInvoke.Flip(processedred, processedred, Emgu.CV.CvEnum.FlipType.Horizontal);    //Flips the image in the horizontal
            Image<Gray, Byte> Thrred;                                                     //Creates two Grayscale images that will be used when segmenting
            Thrred = processedred.InRange(new Hsv(170, 120, 70), new Hsv(180, 255, 255));    //Handles second range for RED

            //Handles noise and cleans image
            Mat kernel = Mat.Ones(3, 3, Emgu.CV.CvEnum.DepthType.Cv32F, 1);             //Creates 3x3 kernelred for use as kernelred
            CvInvoke.MorphologyEx(Thrred, Thrred, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(Thrred, Thrred, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            // Used to display red parts of original image
                    //Extracts only RED parts from orignal image
                    //Mat Mask;                                                                    //Creates Mat for converting mask to Mat
                    //Mask = Thrred.Mat;                                                           //Casts mask to Mat
                    //Image<Hsv, byte> Redisolated = new Image<Hsv, byte>(processedred.Width, processedred.Height);    //Creates Image<Hsv,byte> for final processedred image
                    //CvInvoke.BitwiseAnd(processedred, processedred, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double largestareared = 0;
            int largestcontourindexred = 0;
            Image<Hsv,Byte> Output = new Image<Hsv, Byte>(processedred.Width,processedred.Height);
            Image<Gray, Byte> ContourdrawnRed = new Image<Gray, Byte>(processedred.Width, processedred.Height);
            VectorOfVectorOfPoint ContoursRed = new VectorOfVectorOfPoint();
            Mat HierarchyRed = new Mat();

            //Processing
            CvInvoke.FindContours(Thrred, ContoursRed, HierarchyRed, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);    //Finds contours in image

            //Iterates through each contour
            for (int i = 0; i < ContoursRed.Size; i++)
            {
                double a = CvInvoke.ContourArea(ContoursRed[i], false);                    //  Find the area of contour
                if (a > largestareared)
                {
                    largestareared = a;
                    largestcontourindexred = i;                                            //Stores the index of largest contour
                    boundingrectred = CvInvoke.BoundingRectangle(ContoursRed[i]);          // Creates bounding rectangle for biggest contour
                }
            }

            //Compute centre of rectangle
            XRed = boundingrectred.X + (boundingrectred.Width / 2);
            YRed = boundingrectred.Y + (boundingrectred.Height / 2);

            //Old Method used for overlay
                    //CvInvoke.DrawContours(processedred, ContoursRed, largestcontourindexred, new MCvScalar(255, 255, 255), 10, Emgu.CV.CvEnum.LineType.Filled, HierarchyRed, 0); //Draws biggest contour on blank image
                    //processedred.Draw(boundingrectred,new Hsv(255,255,255), 3);
                    //CvInvoke.Circle(processedred, new System.Drawing.Point(640-XRed, YRed), 4, new MCvScalar(255),2, Emgu.CV.CvEnum.LineType.Filled);
                    //outputimage.Source = BitmapSourceConvert.ToBitmapSource1(processedred1);

            //Cleanup
            //Mask.Dispose();
            Thrred.Dispose();
            Streamred.Dispose();
            myBmpred.Dispose();

            //Blue
            Image<Gray, Byte> ThrBlue;                                                     //Creates two Grayscale images that will be used when segmenting
            ThrBlue = processedred.InRange(new Hsv(90,120,80), new Hsv(120, 255, 180));    //Handles second range for RED

            //Handles noise and cleans image
            CvInvoke.MorphologyEx(ThrBlue, ThrBlue, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(ThrBlue, ThrBlue, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Used to display blue parts of original image
                    //Extracts only RED parts from orignal image
                    //Mat Mask1;                                                                  //Creates Mat for converting mask to Mat
                    //Mask1 = ThrBlue.Mat;                                                           //Casts mask to Mat
                    //Image<Hsv, byte> Bluleisolated = new Image<Hsv, byte>(processedred.Width, processedred.Height);    //Creates Image<Hsv,byte> for final processedred image
                    //CvInvoke.BitwiseAnd(processedred, processedred, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double LargestAreaBlue = 0;
            int LargestContourIndexBlue = 0;
            MCvPoint2D64f CenterBlue = new MCvPoint2D64f(0,0);
            Image<Gray, Byte> ContourDrawnBlue = new Image<Gray, Byte>(processedred.Width, processedred.Height);
            VectorOfVectorOfPoint ContoursBlue = new VectorOfVectorOfPoint();
            Moments MomentsBlue = new Moments();
            Mat HierarchyBlue = new Mat();
             
            //Processing
            CvInvoke.FindContours(ThrBlue, ContoursBlue, HierarchyBlue, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);    //Finds contours in image

            //Iterates through each contour
            for (int i = 0; i < ContoursBlue.Size; i++)
            {
                double a = CvInvoke.ContourArea(ContoursBlue[i], false);                    //  Find the area of contour
                if (a > LargestAreaBlue)
                {
                    LargestAreaBlue = a;
                    LargestContourIndexBlue = i;                                            //Stores the index of largest contour
                    boundingrectBlue = CvInvoke.BoundingRectangle(ContoursBlue[LargestContourIndexBlue]);    // Creates bounding rectangle for biggest contour
                }
            }

            //Compute center of rectangle
            XBlue = boundingrectBlue.X + boundingrectBlue.Width / 2;
            YBlue = boundingrectBlue.Y + boundingrectBlue.Height / 2;

            //Cleanup
            //Mask1.Dispose();
            ThrBlue.Dispose();

            //Add point to images
            Canvas3.Children.Clear();

            System.Windows.Shapes.Ellipse PointRed = CreateEllipse.CircleRed();
            System.Windows.Shapes.Ellipse PointBlue = CreateEllipse.CircleBlue();

            Canvas3.Children.Add(PointRed);
            Canvas3.Children.Add(PointBlue);

            PointRed.SetValue(Canvas.LeftProperty, (640 - XRed) * .6);      //0.6 used as the stream sizes are 0.6 times the actual resolution
            PointRed.SetValue(Canvas.TopProperty, YRed * .6);

            PointBlue.SetValue(Canvas.LeftProperty, (640 - XBlue) * .6);
            PointBlue.SetValue(Canvas.TopProperty, YBlue * .6);

            return;
        }
        else {return;}
    }

    //Create output screen
    public static void OutputScreen(BitmapSource Image, System.Windows.Controls.Image outputimage, double angle, double angle1, double angle2)
    {
        if (Image != null)
        {
            int length = 50;
            int r = 100;
            double opposite = Math.Sin(angle)* length;

            if (opposite != null)
                {

                int X, Y, X1, Y1;

                // Horizontal Indicator

                if (angle != 0 && angle > 0)
                {
                    X = Convert.ToInt32(320 - (200 * ((Math.Abs(angle)-90)/35)));
                }
                else if (angle != 0 && angle < 0)
                {
                    X = Convert.ToInt32(320 + (200 * ((Math.Abs(angle) - 90) / 35)));
                }
                else
                {
                    X = 320;
                }

                Rectangle horz = new Rectangle(120, 20, 400, 60);
                processedred.Draw(horz, new Hsv(255, 255, 255), 5);
                CvInvoke.Line(processedred, new System.Drawing.Point(320, 20), new System.Drawing.Point(320, 80), new MCvScalar(255, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
                CvInvoke.Line(processedred, new System.Drawing.Point(X, 20), new System.Drawing.Point(X, 80), new MCvScalar(140, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);

                // Vertical Indicator

                if (angle1 != 0 && angle1 > 0)
                {
                    Y = Convert.ToInt32(240 + (200 * ((Math.Abs(angle1)) / 35)));
                }
                else if (angle1 != 0 && angle1 < 0)
                {
                    Y = Convert.ToInt32(240 - (200 * ((Math.Abs(angle1)) / 35)));
                }
                else
                {
                    Y = 240;
                }

                Rectangle vert = new Rectangle(20, 40, 60, 400);
                processedred.Draw(vert, new Hsv(255, 255, 255), 5);
                CvInvoke.Line(processedred, new System.Drawing.Point(20, 240), new System.Drawing.Point(80, 240), new MCvScalar(255, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
                CvInvoke.Line(processedred, new System.Drawing.Point(20, Y), new System.Drawing.Point(80, Y), new MCvScalar(140, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);

                //Angle indicator
           
                 //X1 = Convert.ToInt32(Math.Cos(angle2) * r);
                 //Y1 = Convert.ToInt32(Math.Sin(angle2) * r);

                 //CvInvoke.Line(processedred, new System.Drawing.Point(320 + X1, 240 + Y1), new System.Drawing.Point(320 - X1, 240 - Y1), new MCvScalar(140, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
                 //CvInvoke.Line(processedred, new System.Drawing.Point(420, 240), new System.Drawing.Point(220, 240), new MCvScalar(255, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);

                //Setting output source
                outputimage.Source = BitmapSourceConvert.ToBitmapSource1(processedred);
            }
        }
    }
}
