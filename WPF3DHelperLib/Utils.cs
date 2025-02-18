using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class Utils
  {
    public static Vector3D getFrom2Points(Point3D pnt1, Point3D pnt2)
    {
      Vector3D ret = new Vector3D(pnt2.X - pnt1.X, pnt2.Y - pnt1.Y, pnt2.Z - pnt1.Z);

      ret.Normalize();
      return ret;
    }
    public static void DrawCoordSystem(Model3DGroup modelGroup)
    {
      double defAxisWidth = 0.5;
      var axisMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkBlue));

      double axisLen = 500;

      MeshGeometry3D axisX = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), axisLen, defAxisWidth, defAxisWidth);
      GeometryModel3D axisXModel = new GeometryModel3D(axisX, axisMaterial);
      modelGroup.Children.Add(axisXModel);

      MeshGeometry3D axisY = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), defAxisWidth, axisLen, defAxisWidth);
      GeometryModel3D axisYModel = new GeometryModel3D(axisY, axisMaterial);
      modelGroup.Children.Add(axisYModel);

      MeshGeometry3D axisZ = Geometries.CreateParallelepiped(new Point3D(0, 0, 0), defAxisWidth, defAxisWidth, axisLen);
      GeometryModel3D axisZModel = new GeometryModel3D(axisZ, axisMaterial);
      modelGroup.Children.Add(axisZModel);
    }
  }
}
