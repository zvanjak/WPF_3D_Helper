using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPF3DHelperLib
{
  /// <summary>
  /// Provides lighting setup utilities for 3D scenes.
  /// </summary>
  public static class LightingSetup
  {
    /// <summary>
    /// Adds default three-point lighting to a model group.
    /// Provides balanced illumination suitable for most 3D visualizations.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    public static void AddThreePointLighting(Model3DGroup modelGroup)
    {
      // Ambient light provides base illumination for all surfaces
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(80, 80, 80)
      };
      modelGroup.Children.Add(ambientLight);

      // Main (key) light from upper-front-right - primary illumination
      var mainLight = new DirectionalLight
      {
        Color = Colors.White,
        Direction = new Vector3D(-1, -1, -1)
      };
      modelGroup.Children.Add(mainLight);

      // Fill light from opposite direction - reduces harsh shadows
      var fillLight = new DirectionalLight
      {
        Color = Color.FromRgb(180, 180, 180),
        Direction = new Vector3D(1, 0.5, 0.5)
      };
      modelGroup.Children.Add(fillLight);

      // Back light - highlights edges and adds depth
      var backLight = new DirectionalLight
      {
        Color = Color.FromRgb(100, 100, 100),
        Direction = new Vector3D(0, 1, -0.5)
      };
      modelGroup.Children.Add(backLight);
    }

    /// <summary>
    /// Adds simple ambient-only lighting. Good for flat-shaded objects.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    /// <param name="intensity">Light intensity from 0 to 255. Default is 150.</param>
    public static void AddAmbientOnly(Model3DGroup modelGroup, byte intensity = 150)
    {
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(intensity, intensity, intensity)
      };
      modelGroup.Children.Add(ambientLight);
    }

    /// <summary>
    /// Adds ambient lighting with a custom color.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    /// <param name="color">The ambient light color.</param>
    public static void AddAmbientOnly(Model3DGroup modelGroup, Color color)
    {
      var ambientLight = new AmbientLight { Color = color };
      modelGroup.Children.Add(ambientLight);
    }

    /// <summary>
    /// Adds a single directional light with optional ambient.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    /// <param name="direction">The light direction (points toward the light source).</param>
    /// <param name="color">The light color.</param>
    /// <param name="ambientIntensity">Ambient light intensity (0-255). Set to 0 for no ambient.</param>
    public static void AddDirectionalLight(Model3DGroup modelGroup, Vector3D direction, Color color, byte ambientIntensity = 60)
    {
      if (ambientIntensity > 0)
      {
        var ambientLight = new AmbientLight
        {
          Color = Color.FromRgb(ambientIntensity, ambientIntensity, ambientIntensity)
        };
        modelGroup.Children.Add(ambientLight);
      }

      var directionalLight = new DirectionalLight
      {
        Color = color,
        Direction = direction
      };
      modelGroup.Children.Add(directionalLight);
    }

    /// <summary>
    /// Adds bright studio-style lighting - good for showcasing objects.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    public static void AddStudioLighting(Model3DGroup modelGroup)
    {
      // Bright ambient
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(120, 120, 120)
      };
      modelGroup.Children.Add(ambientLight);

      // Strong key light from above-front
      var keyLight = new DirectionalLight
      {
        Color = Colors.White,
        Direction = new Vector3D(-0.5, -0.3, -1)
      };
      modelGroup.Children.Add(keyLight);

      // Soft fill from the side
      var fillLight = new DirectionalLight
      {
        Color = Color.FromRgb(200, 200, 200),
        Direction = new Vector3D(1, 0, -0.3)
      };
      modelGroup.Children.Add(fillLight);

      // Rim light from behind
      var rimLight = new DirectionalLight
      {
        Color = Color.FromRgb(150, 150, 150),
        Direction = new Vector3D(0, 1, 0.5)
      };
      modelGroup.Children.Add(rimLight);
    }

    /// <summary>
    /// Adds dramatic lighting with strong contrast - good for artistic effects.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    public static void AddDramaticLighting(Model3DGroup modelGroup)
    {
      // Very dim ambient
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(30, 30, 30)
      };
      modelGroup.Children.Add(ambientLight);

      // Strong single directional light
      var mainLight = new DirectionalLight
      {
        Color = Colors.White,
        Direction = new Vector3D(-1, -0.5, -0.5)
      };
      modelGroup.Children.Add(mainLight);
    }

    /// <summary>
    /// Adds soft, even lighting from multiple directions - good for scientific visualization.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    public static void AddSoftLighting(Model3DGroup modelGroup)
    {
      // Moderate ambient
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(100, 100, 100)
      };
      modelGroup.Children.Add(ambientLight);

      // Multiple soft lights from different angles
      var light1 = new DirectionalLight
      {
        Color = Color.FromRgb(140, 140, 140),
        Direction = new Vector3D(-1, -1, -1)
      };
      modelGroup.Children.Add(light1);

      var light2 = new DirectionalLight
      {
        Color = Color.FromRgb(140, 140, 140),
        Direction = new Vector3D(1, -1, -1)
      };
      modelGroup.Children.Add(light2);

      var light3 = new DirectionalLight
      {
        Color = Color.FromRgb(140, 140, 140),
        Direction = new Vector3D(0, 1, -1)
      };
      modelGroup.Children.Add(light3);
    }

    /// <summary>
    /// Adds top-down lighting - good for terrain and surface visualization.
    /// </summary>
    /// <param name="modelGroup">The model group to add lights to.</param>
    public static void AddTopDownLighting(Model3DGroup modelGroup)
    {
      var ambientLight = new AmbientLight
      {
        Color = Color.FromRgb(80, 80, 80)
      };
      modelGroup.Children.Add(ambientLight);

      // Strong light from directly above
      var topLight = new DirectionalLight
      {
        Color = Colors.White,
        Direction = new Vector3D(0, 0, -1)
      };
      modelGroup.Children.Add(topLight);

      // Subtle side light for depth
      var sideLight = new DirectionalLight
      {
        Color = Color.FromRgb(80, 80, 80),
        Direction = new Vector3D(-1, -1, 0)
      };
      modelGroup.Children.Add(sideLight);
    }
  }
}
