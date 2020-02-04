using Microsoft.Kinect;
using System;

//Author Andrew Ross
//Tilts the kinect sensor about thhe X-Axis

public class Tilt
{
	public static void TiltUp(KinectSensor ksensor)
	{
        try
        {
            ksensor.ElevationAngle = ksensor.ElevationAngle + 5;  //Tilts the kinect up by 5 degrees
        }
        catch { }
        return;
	}
    public static void TiltDown(KinectSensor ksensor)
    {
        try
        {
            ksensor.ElevationAngle = ksensor.ElevationAngle - 5; //Tilts the kinect down by 5 degrees
        }
        catch { }
        return;
    }
}
