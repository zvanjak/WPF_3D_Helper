using MML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class Sphere
  {
    private Vector3D _pos = new Vector3D();
    public double _radius;

    private GeometryModel3D? _refGeomModel;

    public Vector3D Position
    {
      get => _pos;
      set => _pos = value;
    }
    public GeometryModel3D? RefGeomModel
    {
      get => _refGeomModel;
      set => _refGeomModel = value;
    }

    public double X
    {
      get => _pos.X;
      set => _pos.X = value;
    }

    public double Y
    {
      get => _pos.Y;
      set => _pos.Y = value;
    }
    public double Z
    {
      get => _pos.Z;
      set => _pos.Z = value;
    }
  }
}
