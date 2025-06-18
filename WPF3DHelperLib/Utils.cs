using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace WPF3DHelperLib
{
  public class CoordSystemParams
  {
    public double _xMin;
    public double _xMax;
    public double _yMin;
    public double _yMax;
    public int _numPoints;

    public double _windowWidth = 1000;
    public double _windowHeight = 800;
    public double _centerX = 100;
    public double _centerY = 400;
    public double _scaleX = 40;
    public double _scaleY = 40;
  }

  public class Utils
  {
    public static Vector3D getFrom2Points(Point3D pnt1, Point3D pnt2)
    {
      Vector3D ret = new Vector3D(pnt2.X - pnt1.X, pnt2.Y - pnt1.Y, pnt2.Z - pnt1.Z);

      ret.Normalize();
      return ret;
    }
    public static void DrawCoordSystem(Model3DGroup modelGroup, double axisWidth, double axisLen)
    {
      var axisMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkBlue));

      MeshGeometry3D axisX = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), axisLen, axisWidth, axisWidth);
      GeometryModel3D axisXModel = new GeometryModel3D(axisX, axisMaterial);
      modelGroup.Children.Add(axisXModel);

      MeshGeometry3D axisY = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), axisWidth, axisLen, axisWidth);
      GeometryModel3D axisYModel = new GeometryModel3D(axisY, axisMaterial);
      modelGroup.Children.Add(axisYModel);

      MeshGeometry3D axisZ = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), axisWidth, axisWidth, axisLen);
      GeometryModel3D axisZModel = new GeometryModel3D(axisZ, axisMaterial);
      modelGroup.Children.Add(axisZModel);
    }
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

      int numXTicks = 10; //(int)((xMax - xMin) * coordSysParams._scaleX);
      double dx = coordSysParams._xMax;
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

      for (int i = -8; i <= 8; i++)
      {
        Line xTick = new Line();
        xTick.Stroke = Brushes.Black;
        xTick.X1 = coordSysParams._centerX - 2;
        xTick.Y1 = coordSysParams._centerY - i * coordSysParams._scaleY;
        xTick.X2 = coordSysParams._centerX + 2;
        xTick.Y2 = coordSysParams._centerY - i * coordSysParams._scaleY;
        mainCanvas.Children.Add(xTick);
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
