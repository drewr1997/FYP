using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

public class Imageprocessing
{ 
    public static int XRed, YRed, XBlue, YBlue;

    //Red Segmentation
    public static BitmapSource Procred(BitmapSource Image)
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
            Image<Hsv, Byte> processedred = new Image<Hsv, Byte>(myBmpred);                   //Casts bitmap to image<Hsv, byte>

            //Main processing
            CvInvoke.Flip(processedred, processedred, Emgu.CV.CvEnum.FlipType.Horizontal);    //Flips the image in the horizontal
            Image<Gray, Byte> Thrred;                                                     //Creates two Grayscale images that will be used when segmenting
            Thrred = processedred.InRange(new Hsv(170, 120, 70), new Hsv(180, 255, 255));    //Handles second range for RED

            //Handles noise and cleans image
            Mat kernel = Mat.Ones(3, 3, Emgu.CV.CvEnum.DepthType.Cv32F, 1);             //Creates 3x3 kernelred for use as kernelred
            CvInvoke.MorphologyEx(Thrred, Thrred, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(Thrred, Thrred, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Extracts only RED parts from orignal image
            Mat Mask;                                                                  //Creates Mat for converting mask to Mat
            Mask = Thrred.Mat;                                                           //Casts mask to Mat
            Image<Hsv, byte> Redisolated = new Image<Hsv, byte>(processedred.Width, processedred.Height);    //Creates Image<Hsv,byte> for final processedred image

            //CvInvoke.BitwiseAnd(processedred, processedred, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double largestareared = 0;
            int largestcontourindexred = 0, Xr,Yr;
            MCvPoint2D64f CenterRed;
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
                    //bounding_rect=boundingRect(contours[i]);                          // Find the bounding rectangle for biggest contour
                }
            }

            CvInvoke.DrawContours(ContourdrawnRed, ContoursRed, largestcontourindexred, new MCvScalar(255, 255, 255), 10, Emgu.CV.CvEnum.LineType.Filled, HierarchyRed, 0); //Draws biggest contour on blank image
            Moments moments = CvInvoke.Moments(ContourdrawnRed, true);                     //Gets the moments of the dranw contour
            CenterRed = moments.GravityCenter;                                             //converts the moment to a center

            if (moments.M00 != 0)
            {
                CenterRed = moments.GravityCenter;                                             //converts the moment to a center
                Xr = Convert.ToInt32(CenterRed.X);                                              //Converts X to integer
                XRed = Xr;
                Yr = Convert.ToInt32(CenterRed.Y);                                              //Converts Y to integer
                YRed = Yr;
                Debug.WriteLine("X - {0}, Y - {1}", Xr, Yr);                                //Prints centre co-ords to console
                CvInvoke.Circle(Output, new System.Drawing.Point(Xr, Yr), 10, new MCvScalar(0, 255, 255), -1);
            }
            else
            {
                Debug.WriteLine("No RED detected");
            }

            //Cleanup
            Mask.Dispose();
            Thrred.Dispose();
            Streamred.Dispose();
            myBmpred.Dispose();

            //Blue
            //Converts to image<>
            MemoryStream StreamBlue = new MemoryStream();
            BitmapEncoder encodedBlue = new BmpBitmapEncoder();
            encodedBlue.Frames.Add(BitmapFrame.Create(Image));
            encodedBlue.Save(StreamBlue);
            System.Drawing.Bitmap myBmpBlue = new System.Drawing.Bitmap(StreamBlue);            //Casts image to bitmap
            Image<Hsv, Byte> processedBlue = new Image<Hsv, Byte>(myBmpBlue);                   //Casts bitmap to image<Hsv, byte>

            //Main processing
            CvInvoke.Flip(processedBlue, processedBlue, Emgu.CV.CvEnum.FlipType.Horizontal);    //Flips the image in the horizontal
            Image<Gray, Byte> ThrBlue;                                                     //Creates two Grayscale images that will be used when segmenting
            ThrBlue = processedBlue.InRange(new Hsv(20,200,120), new Hsv(27, 255, 200));    //Handles second range for RED

            //Handles noise and cleans image
            CvInvoke.MorphologyEx(ThrBlue, ThrBlue, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(ThrBlue, ThrBlue, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Extracts only RED parts from orignal image
            Mat Mask1;                                                                  //Creates Mat for converting mask to Mat
            Mask1 = ThrBlue.Mat;                                                           //Casts mask to Mat
            Image<Hsv, byte> Bluleisolated = new Image<Hsv, byte>(processedBlue.Width, processedBlue.Height);    //Creates Image<Hsv,byte> for final processedred image

            //CvInvoke.BitwiseAnd(processedred, processedred, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double LargestAreaBlue = 0;
            int LargestContourIndexBlue = 0, Xb, Yb;
            MCvPoint2D64f CenterBlue = new MCvPoint2D64f(0,0);
            Image<Gray, Byte> ContourDrawnBlue = new Image<Gray, Byte>(processedBlue.Width, processedBlue.Height);
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
                    //bounding_rect=boundingRect(contours[i]);                          // Find the bounding rectangle for biggest contour
                }
            }

            CvInvoke.DrawContours(ContourDrawnBlue, ContoursBlue, LargestContourIndexBlue, new MCvScalar(255, 255, 255), 10, Emgu.CV.CvEnum.LineType.Filled, HierarchyBlue, 0); //Draws biggest contour on blank image
            //return BitmapSourceConvert.ToBitmapSource(Contourdrawn);
            MomentsBlue = CvInvoke.Moments(ContourDrawnBlue, true);                     //Gets the moments of the drawn contour

            if (MomentsBlue.M00 != 0)
            {
                CenterBlue = MomentsBlue.GravityCenter;                                             //converts the moment to a center
                Xb = Convert.ToInt32(CenterBlue.X);                                              //Converts to integer
                XBlue = Xb;
                Yb = Convert.ToInt32(CenterBlue.Y);
                YBlue = Yb;
                //Debug.WriteLine("X - {0}, Y - {1}", X, Y);                                //Prints centre co-ords to console
                CvInvoke.Circle(Output, new System.Drawing.Point(Xb, Yb), 10, new MCvScalar(125, 255, 255), -1);
            }
            else
            {
                Debug.WriteLine("No BLUE detected");
            }

            //Cleanup
            Mask1.Dispose();
            ThrBlue.Dispose();
            StreamBlue.Dispose();
            myBmpBlue.Dispose();

            return BitmapSourceConvert.ToBitmapSource1(Output);                          //Returns processedred image
        }
        else {return null;}
    }
}
