using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF3DHelperLib
{
  public class Utils2D
  {
    public static void DrawPoint(Canvas mainCanvas, CoordSystemParams coordSysParams, double x, double y, Color inColor)
    {
      Ellipse circle = new Ellipse();
      circle.Width = 5;
      circle.Height = 5;
      circle.Fill = new SolidColorBrush(inColor);
      mainCanvas.Children.Add(circle);
      Canvas.SetLeft(circle, coordSysParams._centerX + x * coordSysParams._scaleX - 2.5);
      Canvas.SetTop(circle, coordSysParams._centerY - y * coordSysParams._scaleY - 2.5);
    }

    public static void DrawCoordSystem(Canvas mainCanvas, CoordSystemParams coordSysParams, double xMin, double xMax, double yMin, double yMax)
    {
      Line xAxis = new Line();
      xAxis.Stroke = Brushes.Black;
      xAxis.X1 = 0;
      xAxis.Y1 = coordSysParams._centerY;
      xAxis.X2 = coordSysParams._windowWidth;
      xAxis.Y2 = coordSysParams._centerY;
      mainCanvas.Children.Add(xAxis);

      Line yAxis = new Line();
      yAxis.Stroke = Brushes.Black;
      yAxis.X1 = coordSysParams._centerX;
      yAxis.Y1 = 0;
      yAxis.X2 = coordSysParams._centerX;
      yAxis.Y2 = coordSysParams._windowHeight;
      mainCanvas.Children.Add(yAxis);

      int numXTicks = 10;
      double dx = (coordSysParams._xMax - coordSysParams._xMin) * coordSysParams._scaleX;
      int delta = (int)(dx / numXTicks);
      for (int i = 0; i <= numXTicks; i++)
      {
        Line xTick = new Line();
        xTick.Stroke = Brushes.Black;
        xTick.X1 = coordSysParams._centerX + i * delta;
        xTick.Y1 = coordSysParams._centerY - 2;
        xTick.X2 = coordSysParams._centerX + i * delta;
        xTick.Y2 = coordSysParams._centerY + 2;
        mainCanvas.Children.Add(xTick);
      }

      // based on yMax and yMin, we want to calculate optimal positions for ticks on y-axis
      // we want them to be rounded numbers, so we will use a simple approach
      double yRange = coordSysParams._yMax - coordSysParams._yMin;
      double yRangeAbs = Math.Abs(yRange);

      int logOrder = (int)Math.Floor(Math.Log10(yRangeAbs));

      int nextSigDigit = (int)Math.Ceiling(yRangeAbs / Math.Pow(10, logOrder));

      double nextRounded = Math.Ceiling(yRangeAbs / Math.Pow(10, logOrder)) * Math.Pow(10, logOrder);

      double percent = 100 * yRangeAbs / Math.Pow(10, logOrder + 1);
      if (percent < 50)
      {

      }

      // what we need is
      // ystart = Math.Floor(yMin / nextRounded) * nextRounded;
      // delta and num Ticks


      int numYTicks = 10;
      double dy = (coordSysParams._yMax - coordSysParams._yMin) * coordSysParams._scaleY;
      delta = (int)(dy / numYTicks);
      for (int i = 0; i <= numYTicks; i++)
      {
        Line yTick = new Line();
        yTick.Stroke = Brushes.Black;
        yTick.X1 = coordSysParams._centerX - 2;
        yTick.Y1 = coordSysParams._centerY - i * delta;
        yTick.X2 = coordSysParams._centerX + 2;
        yTick.Y2 = coordSysParams._centerY - i * delta;
        mainCanvas.Children.Add(yTick);
      }


      TextBlock xMinText = new TextBlock();
      xMinText.Text = xMin.ToString();
      mainCanvas.Children.Add(xMinText);
      Canvas.SetLeft(xMinText, coordSysParams._centerX + xMin * coordSysParams._scaleX - 2.5);
      Canvas.SetTop(xMinText, coordSysParams._centerY + 5);

      TextBlock xMaxText = new TextBlock();
      xMaxText.Text = xMax.ToString();
      mainCanvas.Children.Add(xMaxText);
      Canvas.SetLeft(xMaxText, coordSysParams._centerX + xMax * coordSysParams._scaleX - 20);
      Canvas.SetTop(xMaxText, coordSysParams._centerY + 5);

      TextBlock yMinText = new TextBlock();
      yMinText.Text = yMin.ToString();
      mainCanvas.Children.Add(yMinText);
      Canvas.SetLeft(yMinText, coordSysParams._centerX - 20);
      Canvas.SetTop(yMinText, coordSysParams._centerY - yMin * coordSysParams._scaleY - 2.5);

      TextBlock yMaxText = new TextBlock();
      yMaxText.Text = yMax.ToString();
      mainCanvas.Children.Add(yMaxText);
      Canvas.SetLeft(yMaxText, coordSysParams._centerX - 20);
      Canvas.SetTop(yMaxText, coordSysParams._centerY - yMax * coordSysParams._scaleY - 2.5);
    }

  }
}
