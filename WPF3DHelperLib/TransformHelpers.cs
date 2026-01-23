using System;
using System.Windows.Media.Media3D;
using MML;
using MML.Base;

namespace WPF3DHelperLib
{
  /// <summary>
  /// Provides helper methods for 3D transformations.
  /// </summary>
  public static class TransformHelpers
  {
    /// <summary>
    /// Creates a rotation transform that rotates from one direction to another.
    /// </summary>
    /// <param name="fromDirection">The source direction (will be normalized).</param>
    /// <param name="toDirection">The target direction (will be normalized).</param>
    /// <returns>A rotation transform, or identity if directions are parallel.</returns>
    public static Transform3D CreateRotationFromDirections(Vector3D fromDirection, Vector3D toDirection)
    {
      fromDirection.Normalize();
      toDirection.Normalize();

      Vector3D cross = Vector3D.CrossProduct(fromDirection, toDirection);
      double dot = Vector3D.DotProduct(fromDirection, toDirection);
      double angle = Math.Acos(Math.Clamp(dot, -1.0, 1.0)) * 180.0 / Math.PI;

      if (cross.Length < 1e-10)
      {
        // Vectors are parallel
        if (dot > 0)
        {
          // Same direction - no rotation needed
          return Transform3D.Identity;
        }
        else
        {
          // Opposite direction - rotate 180 degrees around any perpendicular axis
          Vector3D perpendicular = GetPerpendicularVector(fromDirection);
          return new RotateTransform3D(new AxisAngleRotation3D(perpendicular, 180));
        }
      }

      cross.Normalize();
      return new RotateTransform3D(new AxisAngleRotation3D(cross, angle));
    }

    /// <summary>
    /// Creates a rotation transform that rotates from the +Z axis to the specified direction.
    /// Useful for orienting objects that are created along the Z axis.
    /// </summary>
    /// <param name="targetDirection">The target direction.</param>
    /// <returns>A rotation transform.</returns>
    public static Transform3D CreateRotationFromZAxis(Vector3D targetDirection)
    {
      return CreateRotationFromDirections(new Vector3D(0, 0, 1), targetDirection);
    }

    /// <summary>
    /// Creates a combined rotation and translation transform.
    /// First rotates from +Z to the target direction, then translates to position.
    /// </summary>
    /// <param name="position">The target position.</param>
    /// <param name="direction">The target direction.</param>
    /// <returns>A combined transform.</returns>
    public static Transform3D CreateRotateAndTranslate(Point3D position, Vector3D direction)
    {
      var group = new Transform3DGroup();
      group.Children.Add(CreateRotationFromZAxis(direction));
      group.Children.Add(new TranslateTransform3D(position.X, position.Y, position.Z));
      return group;
    }

    /// <summary>
    /// Creates a transform to position and orient an object along a line segment.
    /// The object is assumed to be created along the Z axis with base at origin.
    /// </summary>
    /// <param name="startPoint">The start point of the line.</param>
    /// <param name="endPoint">The end point of the line.</param>
    /// <returns>A transform that positions and orients the object along the line.</returns>
    public static Transform3D CreateLineTransform(Point3D startPoint, Point3D endPoint)
    {
      Vector3D direction = endPoint - startPoint;
      if (direction.Length < 1e-10)
        return new TranslateTransform3D(startPoint.X, startPoint.Y, startPoint.Z);

      return CreateRotateAndTranslate(startPoint, direction);
    }

    /// <summary>
    /// Creates a transform to position and orient an object along a line segment,
    /// using Vector3Cartesian points.
    /// </summary>
    public static Transform3D CreateLineTransform(Vector3Cartesian startPoint, Vector3Cartesian endPoint)
    {
      return CreateLineTransform(
        new Point3D(startPoint.X, startPoint.Y, startPoint.Z),
        new Point3D(endPoint.X, endPoint.Y, endPoint.Z));
    }

    /// <summary>
    /// Gets a vector perpendicular to the given vector.
    /// </summary>
    /// <param name="vector">The input vector.</param>
    /// <returns>A perpendicular vector.</returns>
    public static Vector3D GetPerpendicularVector(Vector3D vector)
    {
      vector.Normalize();

      // Find the component with smallest absolute value and create perpendicular
      if (Math.Abs(vector.X) <= Math.Abs(vector.Y) && Math.Abs(vector.X) <= Math.Abs(vector.Z))
      {
        return new Vector3D(0, -vector.Z, vector.Y);
      }
      else if (Math.Abs(vector.Y) <= Math.Abs(vector.X) && Math.Abs(vector.Y) <= Math.Abs(vector.Z))
      {
        return new Vector3D(-vector.Z, 0, vector.X);
      }
      else
      {
        return new Vector3D(-vector.Y, vector.X, 0);
      }
    }

    /// <summary>
    /// Gets two perpendicular vectors that form an orthonormal basis with the given direction.
    /// </summary>
    /// <param name="direction">The primary direction (will be normalized).</param>
    /// <returns>Two perpendicular vectors.</returns>
    public static (Vector3D, Vector3D) GetOrthonormalBasis(Vector3D direction)
    {
      direction.Normalize();
      Vector3D perp1 = GetPerpendicularVector(direction);
      perp1.Normalize();
      Vector3D perp2 = Vector3D.CrossProduct(direction, perp1);
      perp2.Normalize();
      return (perp1, perp2);
    }

    /// <summary>
    /// Creates a scale transform.
    /// </summary>
    public static Transform3D CreateScale(double scaleX, double scaleY, double scaleZ)
    {
      return new ScaleTransform3D(scaleX, scaleY, scaleZ);
    }

    /// <summary>
    /// Creates a uniform scale transform.
    /// </summary>
    public static Transform3D CreateScale(double scale)
    {
      return new ScaleTransform3D(scale, scale, scale);
    }

    /// <summary>
    /// Creates a translation transform.
    /// </summary>
    public static Transform3D CreateTranslation(Point3D position)
    {
      return new TranslateTransform3D(position.X, position.Y, position.Z);
    }

    /// <summary>
    /// Creates a translation transform.
    /// </summary>
    public static Transform3D CreateTranslation(double x, double y, double z)
    {
      return new TranslateTransform3D(x, y, z);
    }

    /// <summary>
    /// Creates a rotation transform around the X axis.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    public static Transform3D CreateRotationX(double angleDegrees)
    {
      return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), angleDegrees));
    }

    /// <summary>
    /// Creates a rotation transform around the Y axis.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    public static Transform3D CreateRotationY(double angleDegrees)
    {
      return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), angleDegrees));
    }

    /// <summary>
    /// Creates a rotation transform around the Z axis.
    /// </summary>
    /// <param name="angleDegrees">The rotation angle in degrees.</param>
    public static Transform3D CreateRotationZ(double angleDegrees)
    {
      return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), angleDegrees));
    }

    /// <summary>
    /// Combines multiple transforms into a single transform group.
    /// </summary>
    public static Transform3D Combine(params Transform3D[] transforms)
    {
      var group = new Transform3DGroup();
      foreach (var transform in transforms)
      {
        group.Children.Add(transform);
      }
      return group;
    }
  }
}
