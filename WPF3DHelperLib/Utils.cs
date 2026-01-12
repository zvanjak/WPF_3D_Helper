using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

using MML;

namespace WPF3DHelperLib
{
  public class CoordSystemParams
  {
    public double _xMin;
    public double _xMax;
    public double _yMin;
    public double _yMax;
    public int _numPoints;

    public double _windowWidth = 1000;
    public double _windowHeight = 800;
    public double _centerX = 100;
    public double _centerY = 400;
    public double _scaleX = 40;
    public double _scaleY = 40;
  }

  public class Utils
  {
    /// <summary>
    /// Draws a 3D coordinate system with cylindrical axes and arrow tips.
    /// </summary>
    /// <param name="modelGroup">The model group to add the axes to.</param>
    /// <param name="axisRadius">The radius of the axis cylinders.</param>
    /// <param name="axisLen">The length of each axis (from -axisLen/2 to +axisLen/2).</param>
    public static void DrawCoordSystem(Model3DGroup modelGroup, double axisRadius, double axisLen)
    {
      // Use different colors for each axis for better visualization
      var xAxisMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed));
      var yAxisMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkGreen));
      var zAxisMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.DarkBlue));

      int numSegments = 12; // Number of segments around the cylinder circumference

      // Arrow parameters (50% smaller)
      double arrowHeadRadius = axisRadius * 2;
      double arrowHeadLength = axisLen * 0.03;

      // Half length for centering axes at origin
      double halfLen = axisLen / 2.0;

      // === Z Axis (already aligned with cylinder default orientation) ===
      MeshGeometry3D axisZ = Geometries.CreateCylinder(axisRadius, axisLen, numSegments, 1);
      GeometryModel3D axisZModel = new GeometryModel3D(axisZ, zAxisMaterial);
      // Translate so it's centered at origin (cylinder starts at z=0, we want it from -halfLen to +halfLen)
      axisZModel.Transform = new TranslateTransform3D(0, 0, -halfLen);
      modelGroup.Children.Add(axisZModel);

      // Z axis arrows (both ends)
      AddAxisArrow(modelGroup, new Point3D(0, 0, halfLen), new Vector3D(0, 0, 1), 
                   arrowHeadRadius, arrowHeadLength, zAxisMaterial);
      AddAxisArrow(modelGroup, new Point3D(0, 0, -halfLen), new Vector3D(0, 0, -1), 
                   arrowHeadRadius, arrowHeadLength, zAxisMaterial);

      // === X Axis (rotate Z-aligned cylinder to X direction) ===
      MeshGeometry3D axisX = Geometries.CreateCylinder(axisRadius, axisLen, numSegments, 1);
      GeometryModel3D axisXModel = new GeometryModel3D(axisX, xAxisMaterial);
      // Rotate 90 degrees around Y axis to align with X, then translate to center
      var xTransformGroup = new Transform3DGroup();
      xTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90)));
      xTransformGroup.Children.Add(new TranslateTransform3D(-halfLen, 0, 0));
      axisXModel.Transform = xTransformGroup;
      modelGroup.Children.Add(axisXModel);

      // X axis arrows (both ends)
      AddAxisArrow(modelGroup, new Point3D(halfLen, 0, 0), new Vector3D(1, 0, 0), 
                   arrowHeadRadius, arrowHeadLength, xAxisMaterial);
      AddAxisArrow(modelGroup, new Point3D(-halfLen, 0, 0), new Vector3D(-1, 0, 0), 
                   arrowHeadRadius, arrowHeadLength, xAxisMaterial);

      // === Y Axis (rotate Z-aligned cylinder to Y direction) ===
      MeshGeometry3D axisY = Geometries.CreateCylinder(axisRadius, axisLen, numSegments, 1);
      GeometryModel3D axisYModel = new GeometryModel3D(axisY, yAxisMaterial);
      // Rotate -90 degrees around X axis to align with Y, then translate to center
      var yTransformGroup = new Transform3DGroup();
      yTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), -90)));
      yTransformGroup.Children.Add(new TranslateTransform3D(0, -halfLen, 0));
      axisYModel.Transform = yTransformGroup;
      modelGroup.Children.Add(axisYModel);

      // Y axis arrows (both ends)
      AddAxisArrow(modelGroup, new Point3D(0, halfLen, 0), new Vector3D(0, 1, 0), 
                   arrowHeadRadius, arrowHeadLength, yAxisMaterial);
      AddAxisArrow(modelGroup, new Point3D(0, -halfLen, 0), new Vector3D(0, -1, 0), 
                   arrowHeadRadius, arrowHeadLength, yAxisMaterial);
    }

    /// <summary>
    /// Adds an arrow (cone) at the end of an axis.
    /// </summary>
    private static void AddAxisArrow(Model3DGroup modelGroup, Point3D position, Vector3D direction,
                                     double radius, double length, Material material)
    {
      // Create a cone using CreateVectorArrow with minimal base
      MeshGeometry3D arrow = Geometries.CreateVectorArrow(radius * 0.1, length * 0.1, 12, 1, radius, length);
      GeometryModel3D arrowModel = new GeometryModel3D(arrow, material);

      // The arrow is created pointing in +Z direction, centered at origin
      // We need to rotate it to match the axis direction and translate to position

      var transformGroup = new Transform3DGroup();

      // Determine rotation needed
      Vector3D defaultDir = new Vector3D(0, 0, 1);
      Vector3D targetDir = direction;
      targetDir.Normalize();

      Vector3D cross = Vector3D.CrossProduct(defaultDir, targetDir);
      double dot = Vector3D.DotProduct(defaultDir, targetDir);
      double angle = Math.Acos(Math.Clamp(dot, -1, 1)) * 180 / Math.PI;

      if (cross.Length > 1e-10)
      {
        cross.Normalize();
        transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(cross, angle)));
      }
      else if (dot < 0)
      {
        // 180 degree rotation needed
        transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 180)));
      }

      // Translate to position (arrow tip should be at position + direction * length/2)
      transformGroup.Children.Add(new TranslateTransform3D(position.X, position.Y, position.Z));

      arrowModel.Transform = transformGroup;
      modelGroup.Children.Add(arrowModel);
    }


    /// <summary>
    /// Exports a Viewport3D to an image file. Supports PNG, JPEG, and BMP formats.
    /// </summary>
    /// <param name="viewport">The Viewport3D to export.</param>
    /// <param name="filePath">The file path to save the image to.</param>
    /// <param name="customWidth">Optional custom width in pixels.</param>
    /// <param name="customHeight">Optional custom height in pixels.</param>
    /// <param name="dpi">DPI for the output image (default 96).</param>
    public static void ExportViewportToImage(Viewport3D viewport, string filePath, int? customWidth = null, int? customHeight = null, double dpi = 96)
    {
      if (viewport == null) throw new ArgumentNullException(nameof(viewport));

      // Use viewport actual size or custom size
      int pixelWidth = customWidth ?? (int)viewport.ActualWidth;
      int pixelHeight = customHeight ?? (int)viewport.ActualHeight;

      if (pixelWidth <= 0 || pixelHeight <= 0)
      {
        pixelWidth = 1920;
        pixelHeight = 1080;
      }

      // Create a DrawingVisual with background and viewport content
      // Do NOT call Measure/Arrange/UpdateLayout on the viewport - it disrupts its layout in parent
      var drawingVisual = new DrawingVisual();
      using (var dc = drawingVisual.RenderOpen())
      {
        // Draw white background
        dc.DrawRectangle(Brushes.White, null, new System.Windows.Rect(0, 0, pixelWidth, pixelHeight));

        // Paint the viewport using a VisualBrush
        var visualBrush = new VisualBrush(viewport)
        {
          Stretch = Stretch.Uniform
        };
        dc.DrawRectangle(visualBrush, null, new System.Windows.Rect(0, 0, pixelWidth, pixelHeight));
      }

      // Render to bitmap
      var renderBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
      renderBitmap.Render(drawingVisual);

      // Determine encoder based on file extension
      BitmapEncoder encoder;
      string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
      switch (extension)
      {
        case ".jpg":
        case ".jpeg":
          encoder = new JpegBitmapEncoder { QualityLevel = 95 };
          break;
        case ".bmp":
          encoder = new BmpBitmapEncoder();
          break;
        case ".png":
        default:
          encoder = new PngBitmapEncoder();
          break;
      }

      encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

      using (var stream = File.Create(filePath))
      {
        encoder.Save(stream);
      }
    }
  }
}
