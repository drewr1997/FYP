using System;
using System.Windows.Media;

//Author: Andrew Ross
//Crates circles with different colours but the same radii

public class CreateEllipse
{
	public static System.Windows.Shapes.Ellipse CircleRed()
	{
        System.Windows.Shapes.Ellipse Circle = new System.Windows.Shapes.Ellipse();
        Circle.Width = 6;
        Circle.Height = 6;
        Circle.Stroke = Brushes.Red;
        Circle.StrokeThickness = 2;
        return (Circle);
    }

    public static System.Windows.Shapes.Ellipse CircleYellow()
    {
        System.Windows.Shapes.Ellipse Circle = new System.Windows.Shapes.Ellipse();
        Circle.Width = 6;
        Circle.Height = 6;
        Circle.Stroke = Brushes.Yellow;
        Circle.StrokeThickness = 2;
        return (Circle);
    }
}
