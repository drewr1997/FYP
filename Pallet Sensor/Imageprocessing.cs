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
        //Checks to see if there is an image
        if (Image != null)
        {

            //Converts to image<>
            MemoryStream Stream = new MemoryStream();
            BitmapEncoder encoded = new BmpBitmapEncoder();
            encoded.Frames.Add(BitmapFrame.Create(Image));
            encoded.Save(Stream);
            System.Drawing.Bitmap myBmp = new System.Drawing.Bitmap(Stream);            //Casts image to bitmap
            Image<Hsv, Byte> processed = new Image<Hsv, Byte>(myBmp);                   //Casts bitmap to image<Hsv, byte>

            //Main processing
            CvInvoke.Flip(processed, processed, Emgu.CV.CvEnum.FlipType.Horizontal);    //Flips the image in the horizontal
            Image<Gray, Byte> Thr1;                                                     //Creates two Grayscale images that will be used when segmenting
            Thr1 = processed.InRange(new Hsv(170, 120, 70), new Hsv(180, 255, 255));    //Handles second range for RED

            //Handles noise and cleans image
            Mat kernel = Mat.Ones(3, 3, Emgu.CV.CvEnum.DepthType.Cv32F, 1);             //Creates 3x3 kernel for use as kernel
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Extracts only RED parts from orignal image
            Mat Mask;                                                                  //Creates Mat for converting mask to Mat
            Mask = Thr1.Mat;                                                           //Casts mask to Mat
            Image<Hsv, byte> Redisolated = new Image<Hsv, byte>(processed.Width, processed.Height);    //Creates Image<Hsv,byte> for final processed image

            //CvInvoke.BitwiseAnd(processed, processed, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double Largestarea = 0;
            int Largestcontourindex = 0, X=0,Y=0;
            MCvPoint2D64f Center;
            Image<Gray,Byte> Centroid = new Image<Gray, Byte>(processed.Width,processed.Height);
            Image<Gray, Byte> Contourdrawn = new Image<Gray, Byte>(processed.Width, processed.Height);
            VectorOfVectorOfPoint Contours = new VectorOfVectorOfPoint();
            Mat Hierarchy = new Mat();

            //Processing
            CvInvoke.FindContours(Thr1, Contours, Hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);    //Finds contours in image

            //Iterates through each contour
            for (int i = 0; i < Contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(Contours[i], false);                    //  Find the area of contour
                if (a > Largestarea)
                {
                    Largestarea = a;
                    Largestcontourindex = i;                                            //Stores the index of largest contour
                    //bounding_rect=boundingRect(contours[i]);                          // Find the bounding rectangle for biggest contour
                }
            }

            CvInvoke.DrawContours(Contourdrawn, Contours, Largestcontourindex, new MCvScalar(255, 255, 255), 10, Emgu.CV.CvEnum.LineType.Filled, Hierarchy, 0); //Draws biggest contour on blank image
            Moments moments = CvInvoke.Moments(Contourdrawn, true);                     //Gets the moments of the dranw contour
            Center = moments.GravityCenter;                                             //converts the moment to a center

            if (moments.M00 != 0)
            {
                Center = moments.GravityCenter;                                             //converts the moment to a center
                X = Convert.ToInt32(Center.X);                                              //Converts X to integer
                XRed = X;
                Y = Convert.ToInt32(Center.Y);                                              //Converts Y to integer
                YRed = Y;
                Debug.WriteLine("X - {0}, Y - {1}", X, Y);                                //Prints centre co-ords to console
                CvInvoke.Circle(Centroid, new System.Drawing.Point(X, Y), 10, new MCvScalar(255, 255, 255), -1);
            }
            else
            {
                Debug.WriteLine("No RED detected");
            }

            //Cleanup
            Mask.Dispose();
            Thr1.Dispose();
            Stream.Dispose();
            myBmp.Dispose();
            
            return BitmapSourceConvert.ToBitmapSource(Centroid);                          //Returns processed image
        }
        else {return null;}
    }

    //Blue Segmentation
    public static BitmapSource Procblue(BitmapSource Image)
    {
        //Checks to see if there is an image
        if (Image != null)
        {
            //Converts to image<>
            MemoryStream Stream = new MemoryStream();
            BitmapEncoder encoded = new BmpBitmapEncoder();
            encoded.Frames.Add(BitmapFrame.Create(Image));
            encoded.Save(Stream);
            System.Drawing.Bitmap myBmp = new System.Drawing.Bitmap(Stream);            //Casts image to bitmap
            Image<Hsv, Byte> processed = new Image<Hsv, Byte>(myBmp);                   //Casts bitmap to image<Hsv, byte>

            //Main processing
            CvInvoke.Flip(processed, processed, Emgu.CV.CvEnum.FlipType.Horizontal);    //Flips the image in the horizontal
            Image<Gray, Byte> Thr1;                                                     //Creates two Grayscale images that will be used when segmenting
            Thr1 = processed.InRange(new Hsv(100, 110, 70), new Hsv(120, 255, 150));    //Handles second range for RED

            //Handles noise and cleans image
            Mat kernel = Mat.Ones(3, 3, Emgu.CV.CvEnum.DepthType.Cv32F, 1);             //Creates 3x3 kernel for use as kernel
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Extracts only RED parts from orignal image
            Mat Mask;                                                                  //Creates Mat for converting mask to Mat
            Mask = Thr1.Mat;                                                           //Casts mask to Mat
            Image<Hsv, byte> Redisolated = new Image<Hsv, byte>(processed.Width, processed.Height);    //Creates Image<Hsv,byte> for final processed image

            //CvInvoke.BitwiseAnd(processed, processed, Redisolated, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Extracts biggest blob
            //Variables
            double Largestarea = 0;
            double depth;
            int Largestcontourindex = 0, X, Y;
            MCvPoint2D64f Center = new MCvPoint2D64f(0,0);
            Image<Gray, Byte> Centroid = new Image<Gray, Byte>(processed.Width, processed.Height);
            Image<Gray, Byte> Contourdrawn = new Image<Gray, Byte>(processed.Width, processed.Height);
            VectorOfVectorOfPoint Contours = new VectorOfVectorOfPoint();
            Moments moments = new Moments();
            Mat Hierarchy = new Mat();

             
            //Processing
            CvInvoke.FindContours(Thr1, Contours, Hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);    //Finds contours in image

            //Iterates through each contour
            for (int i = 0; i < Contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(Contours[i], false);                    //  Find the area of contour
                if (a > Largestarea)
                {
                    Largestarea = a;
                    Largestcontourindex = i;                                            //Stores the index of largest contour
                    //bounding_rect=boundingRect(contours[i]);                          // Find the bounding rectangle for biggest contour
                }
            }

            CvInvoke.DrawContours(Contourdrawn, Contours, Largestcontourindex, new MCvScalar(255, 255, 255), 10, Emgu.CV.CvEnum.LineType.Filled, Hierarchy, 0); //Draws biggest contour on blank image
            //return BitmapSourceConvert.ToBitmapSource(Contourdrawn);
            moments = CvInvoke.Moments(Contourdrawn, true);                     //Gets the moments of the drawn contour

            if (moments.M00 != 0)
            {
                Center = moments.GravityCenter;                                             //converts the moment to a center
                X = Convert.ToInt32(Center.X);                                              //Converts to integer
                XBlue = X;
                Y = Convert.ToInt32(Center.Y);
                YBlue = Y;
                //Debug.WriteLine("X - {0}, Y - {1}", X, Y);                                //Prints centre co-ords to console
                CvInvoke.Circle(Centroid, new System.Drawing.Point(X, Y), 10, new MCvScalar(255, 255, 255), -1);
            }
            else
            {
                Debug.WriteLine("No BLUE detected");
            }

            //Cleanup
            Mask.Dispose();
            Thr1.Dispose();
            Stream.Dispose();
            myBmp.Dispose();

            return BitmapSourceConvert.ToBitmapSource(Centroid);                          //Returns processed image
        }
        else {return null;}
    }
}
