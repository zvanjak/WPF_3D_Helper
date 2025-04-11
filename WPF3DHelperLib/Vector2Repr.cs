using MML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF3DHelperLib
{
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
}
