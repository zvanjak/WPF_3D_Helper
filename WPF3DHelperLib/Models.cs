using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  public class WorldModel
  {
    public double OriginX { get; set; } = 0;
    public double OriginY { get; set; } = 0;
    public double OriginZ { get; set; } = 0;

    public List<CubeModelDTO> Cubes { get; set; } = new List<CubeModelDTO>();

    private List<CubeModel> listCubeModels { get; set; } = new List<CubeModel>();

    public void AddCube(CubeModel inCube)
    {
      if (inCube == null) 
        throw new ArgumentNullException(nameof(inCube), "Cube cannot be null.");

      listCubeModels.Add(inCube);

      CubeModelDTO dto = new CubeModelDTO(inCube);
      Cubes.Add(dto);
    }

    public void SaveToJSON(string fileName)
    {
      var options = new System.Text.Json.JsonSerializerOptions
      {
        WriteIndented = true,
        IncludeFields = true
      };
      string json = System.Text.Json.JsonSerializer.Serialize(this, options);
      System.IO.File.WriteAllText(fileName, json);
    }
    public static WorldModel LoadFromJSON(string fileName)
    {
      string json = System.IO.File.ReadAllText(fileName);
      var options = new System.Text.Json.JsonSerializerOptions
      {
        IncludeFields = true
      };
      var result = System.Text.Json.JsonSerializer.Deserialize<WorldModel>(json, options);
      if (result == null)
        throw new InvalidOperationException("Deserialization returned null.");

      // if we are good, we need to convert CubeModelDTO to CubeModel
      result.listCubeModels = result.Cubes.Select(dto => dto.ToCubeModel()).ToList();

      return result;
    }
  }

  public class CubeModel
  {
    public Point3D Center { get; set; }
    public double SideLength { get; set; }
    public Color Color { get; set; }

    public CubeModel()
    {
      Center = new Point3D(0, 0, 0);
      SideLength = 1;
      Color = Color.FromArgb(255, 255, 0, 0); // Default to red
    }
    public CubeModel(Point3D center, double sideLength, System.Windows.Media.Color color)
    {
      Center = center;
      SideLength = sideLength;
      Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
  }
  public class CubeModelDTO
  {
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double CenterZ { get; set; }
    public double SideLength { get; set; }
    public int ColorA { get; set; }
    public int ColorR { get; set; }
    public int ColorG { get; set; }
    public int ColorB { get; set; }

    public CubeModelDTO() { }

    public CubeModelDTO(CubeModel model)
    {
      CenterX = model.Center.X;
      CenterY = model.Center.Y;
      CenterZ = model.Center.Z;
      SideLength = model.SideLength;
      ColorA = model.Color.A;
      ColorR = model.Color.R;
      ColorG = model.Color.G;
      ColorB = model.Color.B;
    }
    public CubeModel ToCubeModel()
    {
      return new CubeModel
      {
        Center = new Point3D(CenterX, CenterY, CenterZ),
        SideLength = SideLength,
        Color = System.Drawing.Color.FromArgb(ColorA, ColorR, ColorG, ColorB)
      };
    }
  }

}
