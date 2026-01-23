using MML;
using MML.Base;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  /// <summary>
  /// Provides factory methods for creating 3D mesh geometries.
  /// </summary>
  public static class Geometries
  {
    #region Box/Cube Methods

    /// <summary>
    /// Creates a cube mesh from a CubeModel.
    /// </summary>
    public static MeshGeometry3D CreateCube(CubeModel cubeModel)
    {
      return CreateBox(cubeModel.Center, cubeModel.SideLength, cubeModel.SideLength, cubeModel.SideLength);
    }

    /// <summary>
    /// Creates a cube mesh centered at the origin.
    /// </summary>
    /// <param name="length">The side length of the cube.</param>
    public static MeshGeometry3D CreateCube(double length)
    {
      return CreateBox(new Point3D(0, 0, 0), length, length, length);
    }

    /// <summary>
    /// Creates a cube mesh centered at the specified point.
    /// </summary>
    /// <param name="center">The center point of the cube.</param>
    /// <param name="length">The side length of the cube.</param>
    public static MeshGeometry3D CreateCube(Point3D center, double length)
    {
      return CreateBox(center, length, length, length);
    }

    /// <summary>
    /// Creates a rectangular box (parallelepiped) mesh centered at the specified point.
    /// </summary>
    /// <param name="center">The center point of the box.</param>
    /// <param name="lengthX">The length along the X axis.</param>
    /// <param name="lengthY">The length along the Y axis.</param>
    /// <param name="lengthZ">The length along the Z axis.</param>
    public static MeshGeometry3D CreateBox(Point3D center, double lengthX, double lengthY, double lengthZ)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      double hx = lengthX / 2;
      double hy = lengthY / 2;
      double hz = lengthZ / 2;

      // Define 8 vertices of the box
      Point3D p0 = new Point3D(center.X - hx, center.Y + hy, center.Z + hz);
      Point3D p1 = new Point3D(center.X + hx, center.Y + hy, center.Z + hz);
      Point3D p2 = new Point3D(center.X + hx, center.Y + hy, center.Z - hz);
      Point3D p3 = new Point3D(center.X - hx, center.Y + hy, center.Z - hz);
      Point3D p4 = new Point3D(center.X - hx, center.Y - hy, center.Z + hz);
      Point3D p5 = new Point3D(center.X + hx, center.Y - hy, center.Z + hz);
      Point3D p6 = new Point3D(center.X + hx, center.Y - hy, center.Z - hz);
      Point3D p7 = new Point3D(center.X - hx, center.Y - hy, center.Z - hz);

      mesh.Positions.Add(p0);
      mesh.Positions.Add(p1);
      mesh.Positions.Add(p2);
      mesh.Positions.Add(p3);
      mesh.Positions.Add(p4);
      mesh.Positions.Add(p5);
      mesh.Positions.Add(p6);
      mesh.Positions.Add(p7);

      // Top face (+Y)
      AddQuad(mesh, 0, 1, 2, 3);

      // Bottom face (-Y)
      AddQuad(mesh, 4, 6, 5, 4);
      AddQuad(mesh, 4, 7, 6, 4);

      // Front face (+Z)
      AddQuad(mesh, 0, 5, 1, 0);
      AddQuad(mesh, 0, 4, 5, 0);

      // Right face (+X)
      AddQuad(mesh, 1, 5, 6, 1);
      AddQuad(mesh, 1, 6, 2, 1);

      // Back face (-Z)
      AddQuad(mesh, 2, 6, 7, 2);
      AddQuad(mesh, 2, 7, 3, 2);

      // Left face (-X)
      AddQuad(mesh, 3, 7, 4, 3);
      AddQuad(mesh, 3, 4, 0, 3);

      return mesh;
    }

    /// <summary>
    /// Creates a rectangular box (parallelepiped) mesh centered at the specified point.
    /// Alias for CreateBox for backward compatibility.
    /// </summary>
    public static MeshGeometry3D CreateParallelepiped(Point3D center, double lengthX, double lengthY, double lengthZ)
    {
      return CreateBox(center, lengthX, lengthY, lengthZ);
    }

    /// <summary>
    /// Helper method to add a quad (two triangles) to a mesh.
    /// </summary>
    private static void AddQuad(MeshGeometry3D mesh, int i0, int i1, int i2, int i3)
    {
      mesh.TriangleIndices.Add(i0);
      mesh.TriangleIndices.Add(i1);
      mesh.TriangleIndices.Add(i2);

      mesh.TriangleIndices.Add(i0);
      mesh.TriangleIndices.Add(i2);
      mesh.TriangleIndices.Add(i3);
    }

    #endregion

    #region Cylinder and Arrow Methods

    /// <summary>
    /// Creates a cylinder mesh along the Z axis.
    /// </summary>
    /// <param name="baseRadius">The radius of the cylinder.</param>
    /// <param name="height">The height of the cylinder.</param>
    /// <param name="numBaseDivs">Number of divisions around the circumference.</param>
    /// <param name="numHeightDivs">Number of divisions along the height.</param>
    public static MeshGeometry3D CreateCylinder(double baseRadius, double height, int numBaseDivs, int numHeightDivs)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      // Add base ring points
      for (int i = 0; i < numBaseDivs; i++)
      {
        double angle = 2 * Math.PI * i / numBaseDivs;
        mesh.Positions.Add(new Point3D(baseRadius * Math.Cos(angle), baseRadius * Math.Sin(angle), 0));
      }

      // Add height segment rings and triangles
      double segmentHeight = height / numHeightDivs;
      for (int h = 1; h <= numHeightDivs; h++)
      {
        // Add ring points at this height
        for (int i = 0; i < numBaseDivs; i++)
        {
          double angle = 2 * Math.PI * i / numBaseDivs;
          mesh.Positions.Add(new Point3D(baseRadius * Math.Cos(angle), baseRadius * Math.Sin(angle), h * segmentHeight));
        }

        // Add triangles connecting this ring to previous
        AddCylinderRingTriangles(mesh, h - 1, h, numBaseDivs);
      }

      // Add base and top cap center points
      int baseCenterInd = mesh.Positions.Count;
      mesh.Positions.Add(new Point3D(0, 0, 0));
      int topCenterInd = mesh.Positions.Count;
      mesh.Positions.Add(new Point3D(0, 0, height));

      // Add cap triangles
      AddCapTriangles(mesh, 0, baseCenterInd, numBaseDivs, false);
      AddCapTriangles(mesh, numHeightDivs * numBaseDivs, topCenterInd, numBaseDivs, true);

      return mesh;
    }

    /// <summary>
    /// Creates an arrow mesh (cylinder with cone head) along the Z axis.
    /// </summary>
    public static MeshGeometry3D CreateVectorArrow(double baseRadius, double height, int numBaseDivs, int numHeightDivs, 
                                                    double topAdditionalRadius, double topAdditionalHeight)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      double totalHeight = height + topAdditionalHeight;
      double dZ = totalHeight / 2;

      // Add base ring points (offset so arrow is centered)
      for (int i = 0; i < numBaseDivs; i++)
      {
        double angle = 2 * Math.PI * i / numBaseDivs;
        mesh.Positions.Add(new Point3D(baseRadius * Math.Cos(angle), baseRadius * Math.Sin(angle), -dZ));
      }

      // Add shaft rings
      double segmentHeight = height / numHeightDivs;
      for (int h = 1; h <= numHeightDivs; h++)
      {
        for (int i = 0; i < numBaseDivs; i++)
        {
          double angle = 2 * Math.PI * i / numBaseDivs;
          mesh.Positions.Add(new Point3D(baseRadius * Math.Cos(angle), baseRadius * Math.Sin(angle), h * segmentHeight - dZ));
        }

        AddCylinderRingTriangles(mesh, h - 1, h, numBaseDivs);
      }

      // Add base cap
      int baseCenterInd = mesh.Positions.Count;
      mesh.Positions.Add(new Point3D(0, 0, -dZ));
      AddCapTriangles(mesh, 0, baseCenterInd, numBaseDivs, false);

      // Add arrow tip point
      int topCenterInd = mesh.Positions.Count;
      mesh.Positions.Add(new Point3D(0, 0, height + topAdditionalHeight - dZ));

      // Add arrow head base ring (wider than shaft)
      int arrowBaseStart = mesh.Positions.Count;
      for (int i = 0; i < numBaseDivs; i++)
      {
        double angle = 2 * Math.PI * i / numBaseDivs;
        double r = baseRadius + topAdditionalRadius;
        mesh.Positions.Add(new Point3D(r * Math.Cos(angle), r * Math.Sin(angle), height - dZ));
      }

      // Add arrow head cone triangles (to tip)
      for (int i = 0; i < numBaseDivs; i++)
      {
        mesh.TriangleIndices.Add(arrowBaseStart + i);
        mesh.TriangleIndices.Add(arrowBaseStart + (i + 1) % numBaseDivs);
        mesh.TriangleIndices.Add(topCenterInd);
      }

      // Add arrow head base triangles (connecting to shaft)
      int shaftTopStart = numHeightDivs * numBaseDivs;
      for (int i = 0; i < numBaseDivs; i++)
      {
        mesh.TriangleIndices.Add(arrowBaseStart + i);
        mesh.TriangleIndices.Add(shaftTopStart + i);
        mesh.TriangleIndices.Add(arrowBaseStart + (i + 1) % numBaseDivs);

        mesh.TriangleIndices.Add(arrowBaseStart + (i + 1) % numBaseDivs);
        mesh.TriangleIndices.Add(shaftTopStart + i);
        mesh.TriangleIndices.Add(shaftTopStart + (i + 1) % numBaseDivs);
      }

      return mesh;
    }

    /// <summary>
    /// Adds triangles connecting two cylinder rings.
    /// </summary>
    private static void AddCylinderRingTriangles(MeshGeometry3D mesh, int ring1, int ring2, int numDivs)
    {
      for (int i = 0; i < numDivs; i++)
      {
        int i1 = ring1 * numDivs + i;
        int i2 = ring1 * numDivs + (i + 1) % numDivs;
        int i3 = ring2 * numDivs + i;
        int i4 = ring2 * numDivs + (i + 1) % numDivs;

        mesh.TriangleIndices.Add(i1);
        mesh.TriangleIndices.Add(i2);
        mesh.TriangleIndices.Add(i3);

        mesh.TriangleIndices.Add(i2);
        mesh.TriangleIndices.Add(i4);
        mesh.TriangleIndices.Add(i3);
      }
    }

    /// <summary>
    /// Adds cap triangles for cylinder/cone.
    /// </summary>
    private static void AddCapTriangles(MeshGeometry3D mesh, int ringStart, int centerIndex, int numDivs, bool isTop)
    {
      for (int i = 0; i < numDivs; i++)
      {
        if (isTop)
        {
          mesh.TriangleIndices.Add(ringStart + i);
          mesh.TriangleIndices.Add(ringStart + (i + 1) % numDivs);
          mesh.TriangleIndices.Add(centerIndex);
        }
        else
        {
          mesh.TriangleIndices.Add(ringStart + i);
          mesh.TriangleIndices.Add(centerIndex);
          mesh.TriangleIndices.Add(ringStart + (i + 1) % numDivs);
        }
      }
    }

    #endregion

    #region Sphere Methods

    /// <summary>
    /// Creates a sphere mesh.
    /// </summary>
    /// <param name="center">The center of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="latitudeDivisions">Number of divisions around the equator.</param>
    /// <param name="longitudeDivisions">Number of divisions from pole to pole.</param>
    public static MeshGeometry3D CreateSphere(Point3D center, double radius, int latitudeDivisions = 10, int longitudeDivisions = 10)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      int N = latitudeDivisions;
      int M = longitudeDivisions;

      double angleLatStep = 2 * Math.PI / N;
      double angleLonStep = Math.PI / M;

      // North pole
      mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z + radius));
      int northPoleInd = 0;

      // Add latitude rings
      for (int j = 1; j < M; j++)
      {
        double angleLon = angleLonStep * j;
        double ringRadius = radius * Math.Sin(angleLon);
        double z = radius * Math.Cos(angleLon);

        for (int i = 0; i < N; i++)
        {
          double angleLat = angleLatStep * i;
          mesh.Positions.Add(new Point3D(
            center.X + ringRadius * Math.Cos(angleLat),
            center.Y + ringRadius * Math.Sin(angleLat),
            center.Z + z));
        }
      }

      // South pole
      int southPoleInd = mesh.Positions.Count;
      mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z - radius));

      // North pole triangles
      for (int i = 0; i < N; i++)
      {
        mesh.TriangleIndices.Add(northPoleInd);
        mesh.TriangleIndices.Add(1 + i);
        mesh.TriangleIndices.Add(1 + (i + 1) % N);
      }

      // Middle strip triangles
      for (int j = 1; j < M - 1; j++)
      {
        int upperStart = 1 + N * (j - 1);
        int lowerStart = 1 + N * j;

        for (int i = 0; i < N; i++)
        {
          int nextI = (i + 1) % N;

          mesh.TriangleIndices.Add(upperStart + i);
          mesh.TriangleIndices.Add(lowerStart + i);
          mesh.TriangleIndices.Add(lowerStart + nextI);

          mesh.TriangleIndices.Add(upperStart + i);
          mesh.TriangleIndices.Add(lowerStart + nextI);
          mesh.TriangleIndices.Add(upperStart + nextI);
        }
      }

      // South pole triangles
      int lastRingStart = 1 + N * (M - 2);
      for (int i = 0; i < N; i++)
      {
        mesh.TriangleIndices.Add(southPoleInd);
        mesh.TriangleIndices.Add(lastRingStart + (i + 1) % N);
        mesh.TriangleIndices.Add(lastRingStart + i);
      }

      return mesh;
    }

    #endregion

    #region Special Shapes

    /// <summary>
    /// Creates a double cone (bicone) mesh - two cones meeting at their bases.
    /// </summary>
    public static MeshGeometry3D CreateDoubleCone(Point3D center, double radius, double height)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      int N = 20;
      double angleStep = 2 * Math.PI / N;

      // Top point
      mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z + height));
      int northPoleInd = 0;

      // Equator ring
      for (int i = 0; i < N; i++)
      {
        double angle = angleStep * i;
        mesh.Positions.Add(new Point3D(
          center.X + radius * Math.Cos(angle),
          center.Y + radius * Math.Sin(angle),
          center.Z));
      }

      // Bottom point
      int southPoleInd = N + 1;
      mesh.Positions.Add(new Point3D(center.X, center.Y, center.Z - height));

      // Top cone triangles
      for (int i = 0; i < N; i++)
      {
        mesh.TriangleIndices.Add(northPoleInd);
        mesh.TriangleIndices.Add(1 + i);
        mesh.TriangleIndices.Add(1 + (i + 1) % N);
      }

      // Bottom cone triangles
      for (int i = 0; i < N; i++)
      {
        mesh.TriangleIndices.Add(southPoleInd);
        mesh.TriangleIndices.Add(1 + (i + 1) % N);
        mesh.TriangleIndices.Add(1 + i);
      }

      return mesh;
    }

    /// <summary>
    /// Creates a double cone mesh. Alias for backward compatibility.
    /// </summary>
    public static MeshGeometry3D CreateDvostrukiStozac(Point3D center, double radius, double height)
    {
      return CreateDoubleCone(center, radius, height);
    }

    #endregion

    #region Surface and Plane Methods

    /// <summary>
    /// Creates a surface mesh from matrix data.
    /// </summary>
    public static MeshGeometry3D CreateSurface(Matrix data, double xMin, double xMax, double yMin, double yMax, double scaleX, double scaleY)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      int numRows = data.Rows;
      int numCols = data.Cols;
      double dz = 0.01;

      // Add top surface points
      for (int i = 0; i < numRows; i++)
      {
        for (int j = 0; j < numCols; j++)
        {
          double x = scaleX * (xMin + i * (xMax - xMin) / numRows);
          double y = scaleY * (yMin + j * (yMax - yMin) / numCols);
          double z = data.ElemAt(i, j);
          mesh.Positions.Add(new Point3D(x, y, z + dz));
        }
      }

      // Add bottom surface points
      for (int i = 0; i < numRows; i++)
      {
        for (int j = 0; j < numCols; j++)
        {
          double x = scaleX * (xMin + i * (xMax - xMin) / numRows);
          double y = scaleY * (yMin + j * (yMax - yMin) / numCols);
          double z = data.ElemAt(i, j);
          mesh.Positions.Add(new Point3D(x, y, z - dz));
        }
      }

      // Add top surface triangles
      for (int i = 0; i < numRows - 1; i++)
      {
        for (int j = 0; j < numCols - 1; j++)
        {
          int idx = i * numCols + j;
          mesh.TriangleIndices.Add(idx);
          mesh.TriangleIndices.Add(idx + numCols);
          mesh.TriangleIndices.Add(idx + 1);

          mesh.TriangleIndices.Add(idx + 1);
          mesh.TriangleIndices.Add(idx + numCols);
          mesh.TriangleIndices.Add(idx + numCols + 1);
        }
      }

      // Add bottom surface triangles
      int offset = numRows * numCols;
      for (int i = 0; i < numRows - 1; i++)
      {
        for (int j = 0; j < numCols - 1; j++)
        {
          int idx = offset + i * numCols + j;
          mesh.TriangleIndices.Add(idx);
          mesh.TriangleIndices.Add(idx + 1);
          mesh.TriangleIndices.Add(idx + numCols);

          mesh.TriangleIndices.Add(idx + 1);
          mesh.TriangleIndices.Add(idx + numCols + 1);
          mesh.TriangleIndices.Add(idx + numCols);
        }
      }

      return mesh;
    }

    /// <summary>
    /// Creates a simple plane mesh.
    /// </summary>
    public static MeshGeometry3D CreatePlane(double size = 20)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      double half = size / 2;
      mesh.Positions.Add(new Point3D(-half, -half, 0));
      mesh.Positions.Add(new Point3D(-half, half, 0));
      mesh.Positions.Add(new Point3D(half, half, 0));
      mesh.Positions.Add(new Point3D(half, -half, 0));

      mesh.TriangleIndices.Add(0);
      mesh.TriangleIndices.Add(2);
      mesh.TriangleIndices.Add(1);

      mesh.TriangleIndices.Add(0);
      mesh.TriangleIndices.Add(3);
      mesh.TriangleIndices.Add(2);

      return mesh;
    }

    #endregion

    #region Line and Tube Methods

    /// <summary>
    /// Creates a tubular mesh along a parametric curve.
    /// </summary>
    public static MeshGeometry3D CreateParametricCurveTube(double tStart, double tEnd, int numSegments, 
                                                           double baseRadius, int numBaseDivs, 
                                                           Func<double, Vector3Cartesian> curveFunc)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();

      double dT = (tEnd - tStart) / numSegments;
      Vector3Cartesian startPnt = curveFunc(tStart);
      Vector3Cartesian nextPnt = curveFunc(tStart + dT);

      var (n1, n2, n3) = ComputeFrenetFrame(startPnt, nextPnt);
      AddTubeRing(mesh, startPnt, n1, n2, n3, baseRadius, numBaseDivs);

      for (int h = 1; h <= numSegments; h++)
      {
        double T = tStart + (tEnd - tStart) * h / numSegments;
        Vector3Cartesian currentPnt = curveFunc(T);
        nextPnt = curveFunc(T + dT);

        (n1, n2, n3) = ComputeFrenetFrame(currentPnt, nextPnt, n1);
        AddTubeRing(mesh, currentPnt, n1, n2, n3, baseRadius, numBaseDivs);
        AddTubeSegmentTriangles(mesh, h - 1, h, numBaseDivs);
      }

      return mesh;
    }

    /// <summary>
    /// Creates a simple line (tube) between two points.
    /// </summary>
    public static MeshGeometry3D CreateSimpleLine(Vector3Cartesian point1, Vector3Cartesian point2, double baseRadius, int numBaseDivs)
    {
      return CreatePolyLine(new List<Vector3Cartesian> { point1, point2 }, baseRadius, numBaseDivs);
    }

    /// <summary>
    /// Creates a polyline (connected tubes) through a series of points.
    /// </summary>
    public static MeshGeometry3D CreatePolyLine(List<Vector3Cartesian> points, double baseRadius, int numBaseDivs)
    {
      MeshGeometry3D mesh = new MeshGeometry3D();
      int numSegments = points.Count - 1;

      if (numSegments < 1) return mesh;

      Vector3Cartesian startPnt = points[0];
      Vector3Cartesian nextPnt = points[1];

      var (n1, n2, n3) = ComputeFrenetFrame(startPnt, nextPnt);
      AddTubeRing(mesh, startPnt, n1, n2, n3, baseRadius, numBaseDivs);

      for (int h = 1; h <= numSegments; h++)
      {
        Vector3Cartesian currentPnt = points[h];

        if (h < numSegments)
        {
          nextPnt = points[h + 1];
          (n1, n2, n3) = ComputeFrenetFrame(currentPnt, nextPnt, n1);
        }

        AddTubeRing(mesh, currentPnt, n1, n2, n3, baseRadius, numBaseDivs);
        AddTubeSegmentTriangles(mesh, h - 1, h, numBaseDivs);
      }

      return mesh;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Computes a Frenet frame (tangent and two perpendicular vectors) for tube generation.
    /// </summary>
    private static (Vector3Cartesian n1, Vector3Cartesian n2, Vector3Cartesian n3) ComputeFrenetFrame(
        Vector3Cartesian current, Vector3Cartesian next, Vector3Cartesian previousN1)
    {
      var tangent = next - current;
      if (tangent.Norm() < 1e-10)
        tangent = previousN1;

      Vector3Cartesian n1 = tangent / tangent.Norm();
      var (v2, v3) = GetPerpendicularVectors(tangent);

      // Orthonormalize using Gram-Schmidt
      Vector3Cartesian cn2 = v2 - Vector3Cartesian.ScalProd(v2, n1) * n1;
      Vector3Cartesian n2 = cn2 / cn2.Norm();
      Vector3Cartesian cn3 = v3 - Vector3Cartesian.ScalProd(v3, n1) * n1 - Vector3Cartesian.ScalProd(v3, n2) * n2;
      Vector3Cartesian n3 = cn3 / cn3.Norm();

      return (n1, n2, n3);
    }

    /// <summary>
    /// Computes a Frenet frame for initial point (no previous direction).
    /// </summary>
    private static (Vector3Cartesian n1, Vector3Cartesian n2, Vector3Cartesian n3) ComputeFrenetFrame(
        Vector3Cartesian current, Vector3Cartesian next)
    {
      var tangent = next - current;
      Vector3Cartesian n1 = tangent / tangent.Norm();
      var (v2, v3) = GetPerpendicularVectors(tangent);

      // Orthonormalize using Gram-Schmidt
      Vector3Cartesian cn2 = v2 - Vector3Cartesian.ScalProd(v2, n1) * n1;
      Vector3Cartesian n2 = cn2 / cn2.Norm();
      Vector3Cartesian cn3 = v3 - Vector3Cartesian.ScalProd(v3, n1) * n1 - Vector3Cartesian.ScalProd(v3, n2) * n2;
      Vector3Cartesian n3 = cn3 / cn3.Norm();

      return (n1, n2, n3);
    }

    /// <summary>
    /// Gets two perpendicular vectors to the given direction.
    /// </summary>
    private static (Vector3Cartesian, Vector3Cartesian) GetPerpendicularVectors(Vector3Cartesian direction)
    {
      Vector3Cartesian v2, v3;

      if (Math.Abs(direction.X) > Math.Abs(direction.Y) && Math.Abs(direction.X) > Math.Abs(direction.Z))
      {
        v2 = direction.Y == 0 ? new Vector3Cartesian(0, 1, 0) : new Vector3Cartesian(0, Math.Sign(direction.Y), 0);
        v3 = direction.Z == 0 ? new Vector3Cartesian(0, 0, 1) : new Vector3Cartesian(0, 0, Math.Sign(direction.Z));
      }
      else if (Math.Abs(direction.Y) > Math.Abs(direction.X) && Math.Abs(direction.Y) > Math.Abs(direction.Z))
      {
        v2 = direction.X == 0 ? new Vector3Cartesian(1, 0, 0) : new Vector3Cartesian(Math.Sign(direction.X), 0, 0);
        v3 = direction.Z == 0 ? new Vector3Cartesian(0, 0, 1) : new Vector3Cartesian(0, 0, Math.Sign(direction.Z));
      }
      else
      {
        v2 = direction.X == 0 ? new Vector3Cartesian(1, 0, 0) : new Vector3Cartesian(Math.Sign(direction.X), 0, 0);
        v3 = direction.Y == 0 ? new Vector3Cartesian(0, 1, 0) : new Vector3Cartesian(0, Math.Sign(direction.Y), 0);
      }

      return (v2, v3);
    }

    /// <summary>
    /// Adds a ring of vertices for tube generation.
    /// </summary>
    private static void AddTubeRing(MeshGeometry3D mesh, Vector3Cartesian center,
                                     Vector3Cartesian n1, Vector3Cartesian n2, Vector3Cartesian n3,
                                     double radius, int numDivs)
    {
      Vector3Cartesian w1 = new Vector3Cartesian(n1.X, n2.X, n3.X);
      Vector3Cartesian w2 = new Vector3Cartesian(n1.Y, n2.Y, n3.Y);
      Vector3Cartesian w3 = new Vector3Cartesian(n1.Z, n2.Z, n3.Z);

      for (int i = 0; i < numDivs; i++)
      {
        double angle = 2 * Math.PI * i / numDivs;
        Vector3Cartesian localVec = new Vector3Cartesian(radius * Math.Cos(angle), radius * Math.Sin(angle), 0);

        double newx = Vector3Cartesian.ScalProd(localVec, w2);
        double newy = Vector3Cartesian.ScalProd(localVec, w3);
        double newz = Vector3Cartesian.ScalProd(localVec, w1);

        mesh.Positions.Add(new Point3D(center.X + newx, center.Y + newy, center.Z + newz));
      }
    }

    /// <summary>
    /// Adds triangles connecting two tube rings.
    /// </summary>
    private static void AddTubeSegmentTriangles(MeshGeometry3D mesh, int ring1, int ring2, int numDivs)
    {
      for (int i = 0; i < numDivs; i++)
      {
        int i1 = ring1 * numDivs + i;
        int i2 = ring1 * numDivs + (i + 1) % numDivs;
        int i3 = ring2 * numDivs + i;
        int i4 = ring2 * numDivs + (i + 1) % numDivs;

        mesh.TriangleIndices.Add(i1);
        mesh.TriangleIndices.Add(i2);
        mesh.TriangleIndices.Add(i3);

        mesh.TriangleIndices.Add(i2);
        mesh.TriangleIndices.Add(i4);
        mesh.TriangleIndices.Add(i3);
      }
    }

    #endregion
  }
}
