using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  class WorldModel
  {
    public double OriginX { get; set; } = 0;
    public double OriginY { get; set; } = 0;
    public double OriginZ { get; set; } = 0;

    public List<CubeModelDTO> Cubes { get; set; } = new List<CubeModelDTO>();

    private List<CubeModel> listCubes { get; set; } = new List<CubeModel>();

    void SaveToJSON(string fileName)
    {
      var options = new System.Text.Json.JsonSerializerOptions
      {
        WriteIndented = true,
        IncludeFields = true
      };
      string json = System.Text.Json.JsonSerializer.Serialize(this, options);
      System.IO.File.WriteAllText(fileName, json);
    }
  }

  public class CubeModel
  {
    public Point3D Center { get; set; }
    public double SideLength { get; set; }
    public Color Color { get; set; }
  }
  public class CubeModelDTO
  {
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double CenterZ { get; set; }
    public double SideLength { get; set; }
    public Color Color { get; set; }

    public static CubeModelDTO FromCubeModel(CubeModel model)
    {
      return new CubeModelDTO
      {
        CenterX = model.Center.X,
        CenterY = model.Center.Y,
        CenterZ = model.Center.Z,
        SideLength = model.SideLength,
        Color = Color.FromArgb(model.Color.A, model.Color.R, model.Color.G, model.Color.B)
      };
    }
    public CubeModel ToCubeModel()
    {
      return new CubeModel
      {
        Center = new Point3D(CenterX, CenterY, CenterZ),
        SideLength = SideLength,
        Color = Color.FromArgb(Color.A, Color.R, Color.G, Color.B)
      };
    }
  }

}
