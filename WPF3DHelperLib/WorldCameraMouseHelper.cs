using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class WorldCameraMouseHelper
  {
    PerspectiveCamera myPCamera = new PerspectiveCamera();

    Point3D _cameraPos = new Point3D(180, 80, 150);
    Point3D _lookToPos = new Point3D(0, 0, 0);

    Point3D _startCameraPosRButtonClick;

    private bool _bLButtonDown = false;
    private bool _bRightButtonDown = false;

    private Point _lastMousePos;
    private Point _startMouseRButtonClick;
  }
}
