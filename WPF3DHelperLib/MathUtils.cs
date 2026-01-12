using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class MathUtils
  {
    public static Vector3D getFrom2Points(Point3D pnt1, Point3D pnt2)
    {
      Vector3D ret = new Vector3D(pnt2.X - pnt1.X, pnt2.Y - pnt1.Y, pnt2.Z - pnt1.Z);

      ret.Normalize();
      return ret;
    }
  }
}
