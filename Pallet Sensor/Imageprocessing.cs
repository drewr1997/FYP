﻿using Emgu.CV;
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
            Thrred = processedred.InRange(new Hsv(165, 125, 120), new Hsv(180, 255, 255));    //Handles second range for RED

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
            Image<Hsv, Byte> Output = new Image<Hsv, Byte>(processedred.Width, processedred.Height);
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
            ThrBlue = processedred.InRange(new Hsv(85, 110, 80), new Hsv(135, 230, 220));    //Handles second range for Blue

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
            MCvPoint2D64f CenterBlue = new MCvPoint2D64f(0, 0);
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
        else { return; }
    }

    //Create output screen
    public static void OutputScreen(System.Windows.Controls.Image outputimage, double[] frame)
    {
        Image<Hsv, byte> blank = new Image<Hsv, byte>(processedred.Width, processedred.Height, new Hsv(100, 100, 100)); //Creates new blank image
        double angle = Math.Atan(frame[8] / frame[0]);                                         //Compute the horizontal angle betwwen pallet and Kinect
        string distance = "DISTANCE =";
        int xscaled, x2, x3, zscaled, y2, y3, yscaled, w = 150, i = 0;

        //Scaling pallet centre to fit the screenoi nij9h
        xscaled = Convert.ToInt32(320 - (frame[3] / 6000 * 640));
        yscaled = Convert.ToInt32(((frame[7] + 400) / 4096 * 150));
        zscaled = Convert.ToInt32(480 - (frame[11] / 6000 * 480));

        //Drawing the arrow
        CvInvoke.Line(blank, new System.Drawing.Point(320, 480), new System.Drawing.Point(320, 430), new MCvScalar(0, 0, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
        CvInvoke.Line(blank, new System.Drawing.Point(320, 430), new System.Drawing.Point(300, 450), new MCvScalar(0, 0, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
        CvInvoke.Line(blank, new System.Drawing.Point(320, 430), new System.Drawing.Point(340, 450), new MCvScalar(0, 0, 255), 5, Emgu.CV.CvEnum.LineType.Filled);

        //Drawing dashed centre line
        i = 0;
        while (i < 480)
        {
            CvInvoke.Line(blank, new System.Drawing.Point(320, i), new System.Drawing.Point(320, i + 20), new MCvScalar(0, 0, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
            i += 40;
        }

        i = 0;
        while (i < 200)
        {
            CvInvoke.Line(blank, new System.Drawing.Point(245, 480 - i), new System.Drawing.Point(245, 480 - i + 10), new MCvScalar(0, 0, 255), 3, Emgu.CV.CvEnum.LineType.Filled);
            CvInvoke.Line(blank, new System.Drawing.Point(395, 480 - i), new System.Drawing.Point(395, 480 - i + 10), new MCvScalar(0, 0, 255), 3, Emgu.CV.CvEnum.LineType.Filled);
            i += 20;
        }

        //Drawing vertical compass
        CvInvoke.Rectangle(blank, new Rectangle(90, 15, 30, 300), new MCvScalar(0, 0, 255), 5, Emgu.CV.CvEnum.LineType.Filled);
        CvInvoke.Line(blank, new System.Drawing.Point(15, 240), new System.Drawing.Point(45, 240), new MCvScalar(0, 0, 255), 3, Emgu.CV.CvEnum.LineType.Filled);
        CvInvoke.PutText(blank, "Height", new System.Drawing.Point(15, 60), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 255), 2, Emgu.CV.CvEnum.LineType.Filled);

        if (frame[3] != 0 && frame[11] != 0)
        {
            //Drawing circle on centre of pallet
            CvInvoke.Circle(blank, new System.Drawing.Point(xscaled, zscaled), 5, new MCvScalar(180, 255, 255), 2, Emgu.CV.CvEnum.LineType.Filled);

            //Computing end line coordinates
            x2 = Convert.ToInt32(xscaled - (w / 2) * Math.Cos(angle));
            y2 = Convert.ToInt32(zscaled - (w / 2) * Math.Sin(angle));
            x3 = Convert.ToInt32(xscaled + (w / 2) * Math.Cos(angle));
            y3 = Convert.ToInt32(zscaled + (w / 2) * Math.Sin(angle));

            //Drawing line for the pallet
            CvInvoke.Line(blank, new System.Drawing.Point(x2, y2), new System.Drawing.Point(x3, y3), new MCvScalar(180, 255, 255), 5, Emgu.CV.CvEnum.LineType.Filled);

            //Drawing of line on vertical compass
            CvInvoke.Line(blank, new System.Drawing.Point(15, 240 + yscaled), new System.Drawing.Point(45, 240 + yscaled), new MCvScalar(180, 255, 255), 4, Emgu.CV.CvEnum.LineType.Filled);
            distance = ($"Distance = {(frame[11] - 1200) / 1000}m");
            CvInvoke.PutText(blank, distance, new System.Drawing.Point(350, 30), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 255), 2, Emgu.CV.CvEnum.LineType.Filled);
        }
        else
        {
            //Drawing of line on vertical compass
            CvInvoke.Line(blank, new System.Drawing.Point(15, 240 - yscaled), new System.Drawing.Point(45, 240 - yscaled), new MCvScalar(180, 255, 255), 4, Emgu.CV.CvEnum.LineType.Filled);
            distance = ($"Distance = {(frame[11] - 1200) / 1000}m");
            CvInvoke.PutText(blank, "No Pallet Found", new System.Drawing.Point(350, 30), Emgu.CV.CvEnum.FontFace.HersheyPlain, 1.5, new MCvScalar(0, 255, 255), 2, Emgu.CV.CvEnum.LineType.Filled);
        }

        //Displaying the output image to user interface
        outputimage.Source = BitmapSourceConvert.ToBitmapSource1(blank);
    }
}