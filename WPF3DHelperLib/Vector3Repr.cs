using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MML;

namespace MML_VectorField3D_Visualizer
{
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
