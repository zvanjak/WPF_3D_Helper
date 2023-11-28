using MML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class WorldCameraMouseHelper
  {
    public PerspectiveCamera _myCamera = new PerspectiveCamera();

    public Point3D _cameraPos = new Point3D(180, 80, 150);
    public Point3D _lookToPos = new Point3D(0, 0, 0);

    public Point3D _startCameraPosRButtonClick;

    private bool _bLButtonDown = false;
    private bool _bRightButtonDown = false;

    private Point _lastMousePos;
    private Point _startMouseRButtonClick;


    public void InitCamera(Point3D inCameraPos)
    {
      _cameraPos = inCameraPos;

      // Defines the camera used to view the 3D object. In order to view the 3D object,
      // the camera must be positioned and pointed such that the object is within view
      // of the camera.
      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = Utils.getFrom2Points(_cameraPos, _lookToPos);
      _myCamera.UpDirection = new Vector3D(0, 0, 1);
      _myCamera.FieldOfView = 60;
    }

    public void InitLights(Model3DGroup model3DGroup)
    {
      // Define the lights cast in the scene. Without light, the 3D object cannot
      // be seen. Note: to illuminate an object from additional directions, create
      // additional lights.
      AmbientLight ambLight = new AmbientLight();
      ambLight.Color = Colors.Yellow;
      model3DGroup.Children.Add(ambLight);

      DirectionalLight myDirectionalLight = new DirectionalLight();
      myDirectionalLight.Color = Colors.White;
      myDirectionalLight.Direction = new Vector3D(-0.61, -0.5, -0.61);
      model3DGroup.Children.Add(myDirectionalLight);

      DirectionalLight myDirectionalLight2 = new DirectionalLight();
      myDirectionalLight2.Color = Colors.White;
      myDirectionalLight2.Direction = new Vector3D(0.31, 0.2, -0.61);
      model3DGroup.Children.Add(myDirectionalLight2);
    }

    public void Window_MouseLeftButtonDown(Point mousePos)
    {
      _bLButtonDown = true;
      _lastMousePos = mousePos;
    }

    public void Window_MouseLeftButtonUp()
    {
      _bLButtonDown = false;
    }

    public void Window_MouseRightButtonDown(Point mousePos)
    {
      _bRightButtonDown = true;
      _startMouseRButtonClick = mousePos;

      _startCameraPosRButtonClick = _cameraPos;
    }

    public void Window_MouseRightButtonUp()
    {
      _bRightButtonDown = false;
    }


    public void Window_MouseMove(Viewport3D myViewport3D, Point mousePos, object sender, MouseEventArgs e)
    {
      if (_bLButtonDown)
      {
        // morati ćemo uzeti u obzir i smjer gledanja na kraju!
        var newPos = mousePos;
        var diffX = (newPos.X - _lastMousePos.X) / 100.0;
        var diffY = (newPos.Y - _lastMousePos.Y) / 100.0;

        // uzeti smjer gledanja kao normalu, i kreirati horizontalni x-y koord sustav
        // koji određuje koliki su u stvari dx i dy
        _cameraPos = new Point3D(_cameraPos.X + diffX, _cameraPos.Y - diffY, _cameraPos.Z);
        _lookToPos = new Point3D(_lookToPos.X + diffX, _lookToPos.Y - diffY, _lookToPos.Z);

        _myCamera.Position = _cameraPos;

        myViewport3D.InvalidateVisual();
      }

      if (_bRightButtonDown)
      {
        // za početak, samo ćemo se micati lijevo desno
        double diffX = mousePos.X - _startMouseRButtonClick.X;
        double diffY = mousePos.Y - _startMouseRButtonClick.Y;

        Debug.WriteLine("Diff {0} - {1}", diffX, diffY);

        // znači, moramo zarotirati točku kamere, OKO točke gledanja
        double angleX = diffX / 5.0 * Math.PI / 180.0;
        double angleY = diffY / 5.0 * Math.PI / 180.0;

        Debug.WriteLine("Angle {0} - {1}", angleX, angleY);

        // treba oduzeti _lookAtPos, da translatiramo origin
        Point3D cam = _startCameraPosRButtonClick;
        cam.X -= _lookToPos.X;
        cam.Y -= _lookToPos.Y;

        // transformiramo u sferne koordinate
        Point3Cartesian camPnt = new Point3Cartesian(cam.X, cam.Y, cam.Z);
        Point3Spherical outPnt = camPnt.GetSpherical();

        //Debug.WriteLine("Polar {0}  Elevation {1}", polar, elevation);

        outPnt.Phi += angleX;
        outPnt.Theta += angleY;
        if (outPnt.Theta < 0.0)
          outPnt.Theta = 0.05;

        Point3Cartesian newCamPos = outPnt.GetCartesian();

        cam.X = newCamPos.X;
        cam.Y = newCamPos.Y;
        cam.Z = newCamPos.Z;

        cam.X += _lookToPos.X;
        cam.Y += _lookToPos.Y;

        _cameraPos = cam;

        _myCamera.Position = _cameraPos;

        Debug.WriteLine("{0}  Elevation - {1}", _cameraPos.ToString(), outPnt.Theta);

        // treba ažurirati i LookDirection!!!
        _myCamera.LookDirection = Utils.getFrom2Points(_cameraPos, _lookToPos);

        myViewport3D.InvalidateVisual();
      }
    }

    public void Window_MouseWheel(Viewport3D myViewport3D, object sender, MouseWheelEventArgs e)
    {
      // mijenjamo poziciju kamere da se ili približi ili udalji od točke u koju gledamo
      Vector3D dir = Utils.getFrom2Points(_cameraPos, _lookToPos);

      _cameraPos = _cameraPos + (e.Delta / 10.0) * dir;

      _myCamera.Position = _cameraPos;

      myViewport3D.InvalidateVisual();
    }
  }
}
