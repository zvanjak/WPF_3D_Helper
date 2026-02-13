using MML;
using MML.Base;
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
  /// Implements space-game style controls with dual mouse modes:
  /// - Left mouse drag: Orbit around a virtual center point (great for examining objects)
  /// - Right mouse drag: Free-look rotation (look around from current position)
  /// - WASD: Move camera position relative to view direction
  /// - Mouse wheel: Change field of view for zoom/magnification
  /// </summary>
  /// <remarks>
  /// <para>Camera controls:</para>
  /// <list type="bullet">
  ///   <item><description>Left mouse button drag: Orbit around virtual center point (examine mode)</description></item>
  ///   <item><description>Right mouse button drag: Rotate view direction (free-look mode)</description></item>
  ///   <item><description>W/S keys: Move forward/backward along view direction</description></item>
  ///   <item><description>A/D keys: Strafe left/right perpendicular to view direction</description></item>
  ///   <item><description>Q/E keys: Move down/up in world space</description></item>
  ///   <item><description>Mouse wheel: Zoom by changing field of view (magnification)</description></item>
  ///   <item><description>R key: Reset camera to initial position and orientation</description></item>
  ///   <item><description>F key: Reset field of view to default (60°)</description></item>
  /// </list>
  /// </remarks>
  public class WorldCameraMouseHelper
  {
    /// <summary>The perspective camera being controlled.</summary>
    public PerspectiveCamera _myCamera = new PerspectiveCamera();

    /// <summary>Current camera position in world coordinates.</summary>
    public Point3D _cameraPos = new Point3D(180, 80, 150);

    /// <summary>The point the camera is looking at (used for initial setup and reset).</summary>
    public Point3D _lookToPos = new Point3D(0, 0, 0);

    /// <summary>Camera position when mouse button was pressed (for orbit calculation).</summary>
    public Point3D _startCameraPosRButtonClick;

    // Camera orientation (yaw and pitch in radians)
    private double _yaw = 0;    // Horizontal rotation (around Z axis)
    private double _pitch = 0;  // Vertical rotation (looking up/down)

    // Initial camera state for reset
    private Point3D _initialCameraPos;
    private Point3D _initialLookToPos;
    private double _initialYaw;
    private double _initialPitch;
    private double _initialFov = 60.0;

    // Mouse button states
    private bool _bLButtonDown = false;
    private bool _bRightButtonDown = false;

    // Mouse positions for drag calculations
    private Point _lastMousePos;
    private Point _startMouseLButtonClick;
    private Point _startMouseRButtonClick;
    private double _startYaw;
    private double _startPitch;

    // Orbit mode state
    private Point3D _orbitCenterPoint;      // The point we orbit around
    private Point3D _startCameraPosLButtonClick;
    private double _orbitDistance;          // Distance from camera to orbit center

    // Camera constraints
    private const double MinFov = 10.0;   // Maximum zoom in
    private const double MaxFov = 120.0;  // Maximum zoom out
    private const double DefaultFov = 60.0;
    private const double MinPitch = -Math.PI / 2 + 0.1;  // Prevent looking straight down
    private const double MaxPitch = Math.PI / 2 - 0.1;   // Prevent looking straight up

    /// <summary>
    /// Gets or sets the default distance for the virtual orbit center point.
    /// When orbiting, the camera rotates around a point this far along the view direction.
    /// </summary>
    public double OrbitDistance { get; set; } = 200.0;

    /// <summary>
    /// Gets or sets the movement speed for keyboard navigation.
    /// </summary>
    public double KeyboardMoveSpeed { get; set; } = 20.0;

    /// <summary>
    /// Gets or sets the mouse sensitivity for orbit rotation (left mouse drag).
    /// Higher values = slower rotation.
    /// </summary>
    public double OrbitSensitivity { get; set; } = 300.0;

    /// <summary>
    /// Gets or sets the mouse sensitivity for free-look rotation (right mouse drag).
    /// Higher values = slower rotation.
    /// </summary>
    public double LookSensitivity { get; set; } = 300.0;

    /// <summary>
    /// Gets or sets the zoom speed multiplier for FOV changes.
    /// </summary>
    public double ZoomSpeed { get; set; } = 0.5;

    /// <summary>
    /// Gets the current field of view in degrees.
    /// </summary>
    public double FieldOfView => _myCamera.FieldOfView;

    /// <summary>
    /// Initializes the camera at the specified position, looking at the origin.
    /// </summary>
    /// <param name="inCameraPos">The initial camera position.</param>
    public void InitCamera(Point3D inCameraPos)
    {
      _cameraPos = inCameraPos;
      _initialCameraPos = inCameraPos;
      _initialLookToPos = _lookToPos;

      // Calculate initial yaw and pitch from camera position to look-at point
      CalculateYawPitchFromLookAt();
      _initialYaw = _yaw;
      _initialPitch = _pitch;

      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirectionFromAngles();
      _myCamera.UpDirection = new Vector3D(0, 0, 1);
      _myCamera.FieldOfView = _initialFov;
    }

    /// <summary>
    /// Sets the point the camera should look at.
    /// </summary>
    /// <param name="inLookAtPnt">The look-at point in world coordinates.</param>
    public void InitLookAtPoint(Point3D inLookAtPnt)
    {
      _lookToPos = inLookAtPnt;
      _initialLookToPos = inLookAtPnt;
      CalculateYawPitchFromLookAt();
      _initialYaw = _yaw;
      _initialPitch = _pitch;
      _myCamera.LookDirection = GetLookDirectionFromAngles();

      // Set orbit distance based on initial look-at point
      Vector3D toTarget = new Vector3D(
        _lookToPos.X - _cameraPos.X,
        _lookToPos.Y - _cameraPos.Y,
        _lookToPos.Z - _cameraPos.Z);
      OrbitDistance = toTarget.Length;
    }

    /// <summary>
    /// Resets the camera to its initial position, orientation, and field of view.
    /// </summary>
    public void ResetCamera()
    {
      _cameraPos = _initialCameraPos;
      _lookToPos = _initialLookToPos;
      _yaw = _initialYaw;
      _pitch = _initialPitch;
      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirectionFromAngles();
      _myCamera.FieldOfView = _initialFov;
    }

    /// <summary>
    /// Resets only the field of view to default value.
    /// </summary>
    public void ResetFieldOfView()
    {
      _myCamera.FieldOfView = DefaultFov;
    }

    /// <summary>
    /// Calculates yaw and pitch angles from camera position to look-at point.
    /// </summary>
    private void CalculateYawPitchFromLookAt()
    {
      Vector3D dir = new Vector3D(
        _lookToPos.X - _cameraPos.X,
        _lookToPos.Y - _cameraPos.Y,
        _lookToPos.Z - _cameraPos.Z);

      double horizontalDist = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);

      _yaw = Math.Atan2(dir.Y, dir.X);
      _pitch = Math.Atan2(dir.Z, horizontalDist);
    }

    /// <summary>
    /// Calculates the look direction vector from current yaw and pitch angles.
    /// </summary>
    private Vector3D GetLookDirectionFromAngles()
    {
      double cosPitch = Math.Cos(_pitch);
      return new Vector3D(
        Math.Cos(_yaw) * cosPitch,
        Math.Sin(_yaw) * cosPitch,
        Math.Sin(_pitch));
    }

    /// <summary>
    /// Gets the virtual orbit center point at the current orbit distance along view direction.
    /// </summary>
    private Point3D GetVirtualOrbitCenter()
    {
      Vector3D lookDir = GetLookDirectionFromAngles();
      return new Point3D(
        _cameraPos.X + lookDir.X * OrbitDistance,
        _cameraPos.Y + lookDir.Y * OrbitDistance,
        _cameraPos.Z + lookDir.Z * OrbitDistance);
    }

    /// <summary>
    /// Adds default lighting to a 3D model group.
    /// </summary>
    /// <param name="model3DGroup">The model group to add lights to.</param>
    public void InitLights(Model3DGroup model3DGroup)
    {
      LightingSetup.AddThreePointLighting(model3DGroup);
    }

    #region Mouse Event Handlers

    /// <summary>
    /// Handles left mouse button press to start orbit mode.
    /// </summary>
    public void Window_MouseLeftButtonDown(Point mousePos)
    {
      _bLButtonDown = true;
      _startMouseLButtonClick = mousePos;
      _startCameraPosLButtonClick = _cameraPos;
      
      // Calculate the virtual orbit center at the moment of click
      _orbitCenterPoint = GetVirtualOrbitCenter();
      _orbitDistance = OrbitDistance;
      
      // Store starting angles
      _startYaw = _yaw;
      _startPitch = _pitch;
    }

    /// <summary>
    /// Handles left mouse button release to stop orbit mode.
    /// </summary>
    public void Window_MouseLeftButtonUp()
    {
      _bLButtonDown = false;
    }

    /// <summary>
    /// Handles right mouse button press to start free-look mode.
    /// </summary>
    public void Window_MouseRightButtonDown(Point mousePos)
    {
      _bRightButtonDown = true;
      _startMouseRButtonClick = mousePos;
      _startCameraPosRButtonClick = _cameraPos;
      _startYaw = _yaw;
      _startPitch = _pitch;
    }

    /// <summary>
    /// Handles right mouse button release to stop free-look mode.
    /// </summary>
    public void Window_MouseRightButtonUp()
    {
      _bRightButtonDown = false;
    }

    /// <summary>
    /// Handles mouse movement for orbit and free-look modes.
    /// </summary>
    public void Window_MouseMove(Viewport3D myViewport3D, Point mousePos, object sender, MouseEventArgs e)
    {
      if (_bLButtonDown)
      {
        HandleOrbiting(mousePos);
        myViewport3D.InvalidateVisual();
      }

      if (_bRightButtonDown)
      {
        HandleFreeLook(mousePos);
        myViewport3D.InvalidateVisual();
      }

      _lastMousePos = mousePos;
    }

    /// <summary>
    /// Handles orbiting (left mouse button drag).
    /// Rotates camera around the virtual orbit center point.
    /// </summary>
    private void HandleOrbiting(Point mousePos)
    {
      double diffX = mousePos.X - _startMouseLButtonClick.X;
      double diffY = mousePos.Y - _startMouseLButtonClick.Y;

      // Convert pixel movement to rotation angles
      double angleX = -diffX / OrbitSensitivity;  // Negative for natural "grab" feeling
      double angleY = -diffY / OrbitSensitivity;

      // Calculate new camera position by rotating around orbit center
      // We use spherical coordinates relative to the orbit center

      // Get vector from orbit center to starting camera position
      Vector3D toCamera = new Vector3D(
        _startCameraPosLButtonClick.X - _orbitCenterPoint.X,
        _startCameraPosLButtonClick.Y - _orbitCenterPoint.Y,
        _startCameraPosLButtonClick.Z - _orbitCenterPoint.Z);

      // Convert to spherical coordinates
      double r = toCamera.Length;
      double horizontalDist = Math.Sqrt(toCamera.X * toCamera.X + toCamera.Y * toCamera.Y);
      double phi = Math.Atan2(toCamera.Y, toCamera.X);      // Azimuthal angle
      double theta = Math.Atan2(horizontalDist, toCamera.Z); // Polar angle from Z axis

      // Apply rotation
      phi += angleX;
      theta = Math.Clamp(theta + angleY, 0.1, Math.PI - 0.1);

      // Convert back to Cartesian
      double sinTheta = Math.Sin(theta);
      Vector3D newToCamera = new Vector3D(
        r * sinTheta * Math.Cos(phi),
        r * sinTheta * Math.Sin(phi),
        r * Math.Cos(theta));

      // Calculate new camera position
      _cameraPos = new Point3D(
        _orbitCenterPoint.X + newToCamera.X,
        _orbitCenterPoint.Y + newToCamera.Y,
        _orbitCenterPoint.Z + newToCamera.Z);

      // Update yaw and pitch to look at the orbit center
      Vector3D lookDir = new Vector3D(
        _orbitCenterPoint.X - _cameraPos.X,
        _orbitCenterPoint.Y - _cameraPos.Y,
        _orbitCenterPoint.Z - _cameraPos.Z);

      double lookHorizontalDist = Math.Sqrt(lookDir.X * lookDir.X + lookDir.Y * lookDir.Y);
      _yaw = Math.Atan2(lookDir.Y, lookDir.X);
      _pitch = Math.Atan2(lookDir.Z, lookHorizontalDist);

      // Update camera
      _myCamera.Position = _cameraPos;
      _myCamera.LookDirection = GetLookDirectionFromAngles();
    }

    /// <summary>
    /// Handles free-look view rotation (right mouse button drag).
    /// Rotates the camera's view direction without moving position.
    /// </summary>
    private void HandleFreeLook(Point mousePos)
    {
      double diffX = mousePos.X - _startMouseRButtonClick.X;
      double diffY = mousePos.Y - _startMouseRButtonClick.Y;

      // Convert pixel movement to rotation angles
      // Negative diffX for natural "grab and drag" feeling
      _yaw = _startYaw - diffX / LookSensitivity;
      _pitch = Math.Clamp(_startPitch - diffY / LookSensitivity, MinPitch, MaxPitch);

      // Update camera look direction
      _myCamera.LookDirection = GetLookDirectionFromAngles();
    }

    /// <summary>
    /// Handles mouse wheel for zooming via field of view change.
    /// </summary>
    /// <remarks>
    /// Unlike position-based zoom, FOV zoom creates a magnification effect
    /// where the camera stays in place but the view narrows/widens.
    /// </remarks>
    public void Window_MouseWheel(Viewport3D myViewport3D, object sender, MouseWheelEventArgs e)
    {
      // Calculate new FOV (negative delta = zoom in = smaller FOV)
      double fovChange = -e.Delta * ZoomSpeed / 120.0;  // 120 = standard wheel delta
      double newFov = Math.Clamp(_myCamera.FieldOfView + fovChange, MinFov, MaxFov);

      _myCamera.FieldOfView = newFov;

      myViewport3D.InvalidateVisual();
    }

    /// <summary>
    /// Alternative zoom method that moves camera position instead of changing FOV.
    /// </summary>
    /// <param name="myViewport3D">The viewport to refresh.</param>
    /// <param name="e">Mouse wheel event arguments.</param>
    public void Window_MouseWheelPositionZoom(Viewport3D myViewport3D, MouseWheelEventArgs e)
    {
      Vector3D dir = GetLookDirectionFromAngles();
      double zoomAmount = e.Delta / 10.0;

      _cameraPos = new Point3D(
        _cameraPos.X + zoomAmount * dir.X,
        _cameraPos.Y + zoomAmount * dir.Y,
        _cameraPos.Z + zoomAmount * dir.Z);

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
    /// W - Move forward (along view direction),
    /// S - Move backward,
    /// A - Strafe left,
    /// D - Strafe right,
    /// Q - Move down,
    /// E - Move up,
    /// R - Reset camera position, orientation, and FOV,
    /// F - Reset FOV only
    /// </remarks>
    public void Window_KeyDown(Viewport3D myViewport3D, KeyEventArgs e)
    {
      double step = KeyboardMoveSpeed;

      // Calculate movement vectors based on current view direction
      Vector3D forward = GetLookDirectionFromAngles();

      // For horizontal movement, use only the horizontal component of forward
      Vector3D forwardHorizontal = new Vector3D(forward.X, forward.Y, 0);
      if (forwardHorizontal.Length > 0.001)
        forwardHorizontal.Normalize();
      else
        forwardHorizontal = new Vector3D(1, 0, 0);

      Vector3D worldUp = new Vector3D(0, 0, 1);
      Vector3D right = Vector3D.CrossProduct(forwardHorizontal, worldUp);
      right.Normalize();

      Vector3D moveDirection = new Vector3D(0, 0, 0);

      switch (e.Key)
      {
        case Key.W: // Forward (along view direction, including vertical component)
          moveDirection = forward * step;
          break;
        case Key.S: // Backward
          moveDirection = -forward * step;
          break;
        case Key.A: // Strafe left (horizontal only)
          moveDirection = -right * step;
          break;
        case Key.D: // Strafe right (horizontal only)
          moveDirection = right * step;
          break;
        case Key.Q: // Move down (world space)
          moveDirection = -worldUp * step;
          break;
        case Key.E: // Move up (world space)
          moveDirection = worldUp * step;
          break;
        case Key.R: // Reset camera completely
          ResetCamera();
          myViewport3D.InvalidateVisual();
          e.Handled = true;
          return;
        case Key.F: // Reset FOV only
          ResetFieldOfView();
          myViewport3D.InvalidateVisual();
          e.Handled = true;
          return;
        default:
          return;
      }

      // Update camera position
      _cameraPos = new Point3D(
        _cameraPos.X + moveDirection.X,
        _cameraPos.Y + moveDirection.Y,
        _cameraPos.Z + moveDirection.Z);

      _myCamera.Position = _cameraPos;

      myViewport3D.InvalidateVisual();
      e.Handled = true;
    }

    /// <summary>
    /// Legacy overload for backward compatibility (without viewport refresh).
    /// </summary>
    [Obsolete("Use Window_KeyDown(Viewport3D, KeyEventArgs) instead for proper view refresh.")]
    public void Window_KeyDown(object sender, KeyEventArgs e)
    {
      Debug.WriteLine("Warning: Using legacy Window_KeyDown without viewport refresh");
    }

    #endregion
  }
}
