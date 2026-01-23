using MML;
using MML.Base;
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

  public class Vector2Repr
  {
    public Vector2Cartesian Pos;
    public Vector2Cartesian Vec;

    public Vector2Repr(Vector2Cartesian pos, Vector2Cartesian vec)
    {
      Pos = pos;
      Vec = vec;
    }
  }

  /// <summary>
  /// Represents a 3D vector at a specific position in space.
  /// Used for vector field visualization.
  /// </summary>
  public class Vector3Repr
  {
    public Vector3Cartesian Pos;
    public Vector3Cartesian Vec;

    public Vector3Repr(Vector3Cartesian pos, Vector3Cartesian vec)
    {
      Pos = pos;
      Vec = vec;
    }
  }
}
