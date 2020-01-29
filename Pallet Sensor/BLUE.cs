using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.IO;
using System.Windows.Media.Imaging;

public class BLUE
{
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
            Image<Gray, Byte> Thr1;                                              //Creates two Grayscale images that will be used when segmenting
            Thr1 = processed.InRange(new Hsv(100, 110, 70), new Hsv(120, 255, 150));       //Handles first range for RED

            //Handles noise and cleans image
            Mat kernel = Mat.Ones(9, 9, Emgu.CV.CvEnum.DepthType.Cv32F, 1);             //Creates 3x3 kernel for use as kernel
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));
            CvInvoke.MorphologyEx(Thr1, Thr1, Emgu.CV.CvEnum.MorphOp.Dilate, kernel, new System.Drawing.Point(0, 0), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(1));

            //Extracts only RED parts from orignal image
            Mat Mask;                                                                  //Creates Mat for converting mask to Mat
            Mask = Thr1.Mat;                                                           //Casts mask to Mat
            Image<Hsv, byte> Final = new Image<Hsv, byte>(processed.Width, processed.Height);    //Creates Image<Hsv,byte> for final processed image
            CvInvoke.BitwiseAnd(processed, processed, Final, Mask);                     //ANDS mask with orignal image to retain only portions that are RED

            //Cleanup
            Mask.Dispose();
            Thr1.Dispose();
            Stream.Dispose();
            myBmp.Dispose();

            return null;
            /*return BitmapSourceConvert.ToBitmapSource(Final);  */                        //Returns processed image
        }
        else { return null; }
    }
}
