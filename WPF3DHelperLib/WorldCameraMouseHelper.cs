using MML;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  /// <summary>
  /// Helper class for interactive 3D camera control using mouse and keyboard input.
  /// </summary>
  /// <remarks>
  /// <para>Provides the following camera controls:</para>
  /// <list type="bullet">
  ///   <item><description>Left mouse button drag: Pan camera parallel to view plane</description></item>
  ///   <item><description>Right mouse button drag: Orbit camera around look-at point</description></item>
  ///   <item><description>Mouse wheel: Zoom in/out toward look-at point</description></item>
  ///   <item><description>W/S keys: Move forward/backward</description></item>
  ///   <item><description>A/D keys: Strafe left/right</description></item>
  ///   <item><description>Q/E keys: Move up/down</description></item>
  ///   <item><description>R key: Reset camera to initial position</description></item>
  /// </list>
  /// </remarks>
  public class WorldCameraMouseHelper
  {
    /// <summary>The perspective camera being controlled.</summary>
    public PerspectiveCamera _myCamera = new PerspectiveCamera();

    /// <summary>Current camera position in world coordinates.</summary>
    public Point3D _cameraPos = new Point3D(180, 80, 150);

    /// <summary>The point the camera is looking at.</summary>
    public Point3D _lookToPos = new Point3D(0, 0, 0);

    /// <summary>Camera position when right mouse button was pressed (for orbit calculation).</summary>
    public Point3D _startCameraPosRButtonClick;

    // Initial camera state for reset
    private Point3D _initialCameraPos;
    private Point3D _initialLookToPos;

    // Mouse button states
    private bool _bLButtonDown = false;
    private bool _bRightButtonDown = false;

    // Mouse positions for drag calculations
    private Point _lastMousePos;
    private Point _startMouseRButtonClick;

    // Camera constraints
    private const double MinZoomDistance = 10.0;
    private const double MaxZoomDistance = 5000.0;
    private const double MinTheta = 0.05;  // Prevent gimbal lock at poles
    private const double MaxTheta = Math.PI - 0.05;

    /// <summary>
    /// Gets or sets the movement speed for keyboard navigation.
    /// </summary>
    public double KeyboardMoveSpeed { get; set; } = 20.0;

    /// <summary>
    /// Gets or sets the mouse sensitivity for panning.
    /// </summary>
    public double PanSensitivity { get; set; } = 100.0;

    /// <summary>
    /// Gets or sets the mouse sensitivity for orbiting.
    /// </summary>
    public double OrbitSensitivity { get; set; } = 5.0;

    /// <summary>
    /// Gets or sets the zoom speed multiplier.
    /// </summary>
    public double ZoomSpeed { get; set; } = 10.0;

    /// <summary>
    /// Initializes the camera at the specified position, looking at the origin.
    /// </summary>
    /// <param name="inCameraPos">The initial camera position.</param>
    public void InitCamera(Point3D inCameraPos)
    {
      _cameraPos = inCameraPos;
      _initialCameraPos = inCameraPos;
      _initialLookToPos = _lookToPos;

      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirection();
      _myCamera.UpDirection = new Vector3D(0, 0, 1);
      _myCamera.FieldOfView = 60;
    }

    /// <summary>
    /// Sets the point the camera should look at.
    /// </summary>
    /// <param name="inLookAtPnt">The look-at point in world coordinates.</param>
    public void InitLookAtPoint(Point3D inLookAtPnt)
    {
      _lookToPos = inLookAtPnt;
      _initialLookToPos = inLookAtPnt;
      _myCamera.LookDirection = GetLookDirection();
    }

    /// <summary>
    /// Resets the camera to its initial position and orientation.
    /// </summary>
    public void ResetCamera()
    {
      _cameraPos = _initialCameraPos;
      _lookToPos = _initialLookToPos;
      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirection();
    }

    /// <summary>
    /// Calculates the look direction vector from camera to look-at point.
    /// </summary>
    private Vector3D GetLookDirection()
    {
      return Utils.getFrom2Points(_cameraPos, _lookToPos);
    }

    /// <summary>
    /// Gets the distance from camera to look-at point.
    /// </summary>
    private double GetCameraDistance()
    {
      Vector3D diff = _cameraPos - _lookToPos;
      return diff.Length;
    }

    /// <summary>
    /// Adds default lighting to a 3D model group.
    /// </summary>
    /// <param name="model3DGroup">The model group to add lights to.</param>
    public void InitLights(Model3DGroup model3DGroup)
    {
      AmbientLight ambLight = new AmbientLight { Color = System.Windows.Media.Colors.Gray };
      model3DGroup.Children.Add(ambLight);

      DirectionalLight myDirectionalLight = new DirectionalLight
      {
        Color = System.Windows.Media.Colors.LimeGreen,
        Direction = new Vector3D(-0.61, -0.5, -0.61)
      };
      model3DGroup.Children.Add(myDirectionalLight);

      DirectionalLight myDirectionalLight2 = new DirectionalLight
      {
        Color = System.Windows.Media.Colors.White,
        Direction = new Vector3D(0.31, 0.2, -0.61)
      };
      model3DGroup.Children.Add(myDirectionalLight2);
    }

    #region Mouse Event Handlers

    /// <summary>
    /// Handles left mouse button press to start panning.
    /// </summary>
    public void Window_MouseLeftButtonDown(Point mousePos)
    {
      _bLButtonDown = true;
      _lastMousePos = mousePos;
    }

    /// <summary>
    /// Handles left mouse button release to stop panning.
    /// </summary>
    public void Window_MouseLeftButtonUp()
    {
      _bLButtonDown = false;
    }

    /// <summary>
    /// Handles right mouse button press to start orbiting.
    /// </summary>
    public void Window_MouseRightButtonDown(Point mousePos)
    {
      _bRightButtonDown = true;
      _startMouseRButtonClick = mousePos;
      _startCameraPosRButtonClick = _cameraPos;
    }

    /// <summary>
    /// Handles right mouse button release to stop orbiting.
    /// </summary>
    public void Window_MouseRightButtonUp()
    {
      _bRightButtonDown = false;
    }

    /// <summary>
    /// Handles mouse movement for panning and orbiting.
    /// </summary>
    public void Window_MouseMove(Viewport3D myViewport3D, Point mousePos, object sender, MouseEventArgs e)
    {
      if (_bLButtonDown)
      {
        HandlePanning(mousePos);
        myViewport3D.InvalidateVisual();
      }

      if (_bRightButtonDown)
      {
        HandleOrbiting(mousePos);
        myViewport3D.InvalidateVisual();
      }

      _lastMousePos = mousePos;
    }

    /// <summary>
    /// Handles panning (left mouse button drag).
    /// </summary>
    private void HandlePanning(Point mousePos)
    {
      var diffX = (mousePos.X - _lastMousePos.X) / PanSensitivity;
      var diffY = (mousePos.Y - _lastMousePos.Y) / PanSensitivity;

      // Calculate camera orientation for proper panning direction
      double alpha = Math.Atan2(_cameraPos.Y - _lookToPos.Y, _cameraPos.X - _lookToPos.X);

      // Calculate pan offset in world coordinates
      var dx = diffX * Math.Sin(alpha) + diffY * Math.Cos(alpha);
      var dy = diffX * Math.Cos(alpha) - diffY * Math.Sin(alpha);

      // Move both camera and look-at point to pan
      _cameraPos = new Point3D(_cameraPos.X + dx, _cameraPos.Y - dy, _cameraPos.Z);
      _lookToPos = new Point3D(_lookToPos.X + dx, _lookToPos.Y - dy, _lookToPos.Z);

      _myCamera.Position = _cameraPos;
    }

    /// <summary>
    /// Handles orbiting (right mouse button drag).
    /// </summary>
    private void HandleOrbiting(Point mousePos)
    {
      double diffX = mousePos.X - _startMouseRButtonClick.X;
      double diffY = mousePos.Y - _startMouseRButtonClick.Y;

      // Convert pixel movement to rotation angles
      double angleX = diffX / OrbitSensitivity * Math.PI / 180.0;
      double angleY = diffY / OrbitSensitivity * Math.PI / 180.0;

      // Transform camera position relative to look-at point
      Point3D cam = _startCameraPosRButtonClick;
      cam.X -= _lookToPos.X;
      cam.Y -= _lookToPos.Y;

      // Convert to spherical coordinates for rotation
      Point3Cartesian camPnt = new Point3Cartesian(cam.X, cam.Y, cam.Z);
      Point3Spherical spherical = camPnt.GetSpherical();

      // Apply rotation with clamping
      spherical.Phi += angleX;
      spherical.Theta = Math.Clamp(spherical.Theta + angleY, MinTheta, MaxTheta);

      // Convert back to Cartesian
      Point3Cartesian newCamPos = spherical.GetCartesian();

      // Translate back from origin
      _cameraPos = new Point3D(
        newCamPos.X + _lookToPos.X,
        newCamPos.Y + _lookToPos.Y,
        newCamPos.Z
      );

      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirection();
    }

    /// <summary>
    /// Handles mouse wheel for zooming.
    /// </summary>
    public void Window_MouseWheel(Viewport3D myViewport3D, object sender, MouseWheelEventArgs e)
    {
      Vector3D dir = GetLookDirection();
      dir.Normalize();

      double currentDistance = GetCameraDistance();
      double zoomAmount = e.Delta / ZoomSpeed;

      // Calculate new distance with bounds checking
      double newDistance = currentDistance - zoomAmount;
      if (newDistance < MinZoomDistance || newDistance > MaxZoomDistance)
        return;

      _cameraPos = _cameraPos + zoomAmount * dir;
      _myCamera.Position = _cameraPos;

      myViewport3D.InvalidateVisual();
    }

    #endregion

    #region Keyboard Event Handlers

    /// <summary>
    /// Handles keyboard input for camera movement.
    /// </summary>
    /// <param name="myViewport3D">The viewport to refresh after movement.</param>
    /// <param name="e">The key event arguments.</param>
    /// <remarks>
    /// Supported keys:
    /// W - Move forward, S - Move backward,
    /// A - Strafe left, D - Strafe right,
    /// Q - Move down, E - Move up,
    /// R - Reset camera
    /// </remarks>
    public void Window_KeyDown(Viewport3D myViewport3D, KeyEventArgs e)
    {
      double step = KeyboardMoveSpeed;

      // Calculate movement vectors
      Vector3D forward = _myCamera.LookDirection;
      forward.Normalize();

      Vector3D up = new Vector3D(0, 0, 1);
      Vector3D right = Vector3D.CrossProduct(forward, up);
      right.Normalize();

      Vector3D moveDirection = new Vector3D(0, 0, 0);

      switch (e.Key)
      {
        case Key.W: // Forward
          moveDirection = forward * step;
          break;
        case Key.S: // Backward
          moveDirection = -forward * step;
          break;
        case Key.A: // Strafe left
          moveDirection = -right * step;
          break;
        case Key.D: // Strafe right
          moveDirection = right * step;
          break;
        case Key.Q: // Move down
          moveDirection = -up * step;
          break;
        case Key.E: // Move up
          moveDirection = up * step;
          break;
        case Key.R: // Reset camera
          ResetCamera();
          myViewport3D.InvalidateVisual();
          return;
        default:
          return;
      }

      // Update camera position AND the stored position
      _cameraPos = new Point3D(
        _cameraPos.X + moveDirection.X,
        _cameraPos.Y + moveDirection.Y,
        _cameraPos.Z + moveDirection.Z
      );

      // Also move look-at point to maintain look direction
      _lookToPos = new Point3D(
        _lookToPos.X + moveDirection.X,
        _lookToPos.Y + moveDirection.Y,
        _lookToPos.Z + moveDirection.Z
      );

      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirection();

      myViewport3D.InvalidateVisual();
      e.Handled = true;
    }

    /// <summary>
    /// Legacy overload for backward compatibility (without viewport refresh).
    /// </summary>
    [Obsolete("Use Window_KeyDown(Viewport3D, KeyEventArgs) instead for proper view refresh.")]
    public void Window_KeyDown(object sender, KeyEventArgs e)
    {
      // This overload doesn't refresh the viewport - kept for compatibility
      Debug.WriteLine("Warning: Using legacy Window_KeyDown without viewport refresh");
    }

    #endregion
  }
}
