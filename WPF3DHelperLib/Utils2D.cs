using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF3DHelperLib
{
  #region Axis Tick Classes

  /// <summary>
  /// Represents a single tick mark on a coordinate axis.
  /// </summary>
  public class AxisTick
  {
    /// <summary>Gets or sets the numerical value at this tick position.</summary>
    public double Value { get; set; }

    /// <summary>Gets or sets the formatted text label to display at this tick.</summary>
    public string Label { get; set; } = "";

    /// <summary>Gets or sets whether this is a major tick (typically longer with a label).</summary>
    public bool IsMajor { get; set; } = true;
  }

  /// <summary>
  /// Contains comprehensive tick information for rendering an axis.
  /// </summary>
  public class AxisTickInfo
  {
    /// <summary>Gets or sets the nice rounded minimum value for the axis.</summary>
    public double Min { get; set; }

    /// <summary>Gets or sets the nice rounded maximum value for the axis.</summary>
    public double Max { get; set; }

    /// <summary>Gets or sets the spacing between consecutive ticks.</summary>
    public double TickSpacing { get; set; }

    /// <summary>Gets or sets the list of tick marks for the axis.</summary>
    public List<AxisTick> Ticks { get; set; } = new List<AxisTick>();

    /// <summary>Gets or sets the number of decimal places for tick labels.</summary>
    public int DecimalPlaces { get; set; }

    /// <summary>Gets or sets whether tick labels should use scientific notation.</summary>
    public bool UseScientificNotation { get; set; }
  }

  #endregion

  #region Axis Tick Calculator

  /// <summary>
  /// Calculates optimal axis tick positions with "nice" rounded values.
  /// </summary>
  public static class AxisTickCalculator
  {
    private static readonly double[] NiceNumbers = { 1.0, 2.0, 2.5, 5.0, 10.0 };

    /// <summary>
    /// Calculates nice tick values for an axis based on data range.
    /// </summary>
    public static AxisTickInfo CalculateTicks(double dataMin, double dataMax, int targetTickCount = 8)
    {
      // Handle edge cases
      if (double.IsNaN(dataMin) || double.IsNaN(dataMax) ||
          double.IsInfinity(dataMin) || double.IsInfinity(dataMax))
      {
        return CreateDefaultTicks(-10, 10, targetTickCount);
      }

      // Handle case where min equals max
      if (Math.Abs(dataMax - dataMin) < double.Epsilon)
      {
        double padding = Math.Abs(dataMin) * 0.1;
        if (padding < double.Epsilon) padding = 1.0;
        dataMin -= padding;
        dataMax += padding;
      }

      double range = dataMax - dataMin;
      double roughTickSpacing = range / (targetTickCount - 1);
      double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughTickSpacing)));
      double normalizedSpacing = roughTickSpacing / magnitude;
      double niceSpacing = FindNiceNumber(normalizedSpacing);
      double tickSpacing = niceSpacing * magnitude;

      double niceMin = Math.Floor(dataMin / tickSpacing) * tickSpacing;
      double niceMax = Math.Ceiling(dataMax / tickSpacing) * tickSpacing;

      var result = new AxisTickInfo
      {
        DecimalPlaces = CalculateDecimalPlaces(tickSpacing),
        UseScientificNotation = ShouldUseScientificNotation(niceMin, niceMax, tickSpacing),
        Min = niceMin,
        Max = niceMax,
        TickSpacing = tickSpacing
      };
      result.Ticks = GenerateTicks(niceMin, niceMax, tickSpacing, result.DecimalPlaces, result.UseScientificNotation);

      return result;
    }

    /// <summary>
    /// Calculates ticks with additional padding around the data range.
    /// </summary>
    public static AxisTickInfo CalculateTicksWithPadding(double dataMin, double dataMax, int targetTickCount = 8, double paddingPercent = 5)
    {
      double range = dataMax - dataMin;
      double padding = range * paddingPercent / 100.0;
      return CalculateTicks(dataMin - padding, dataMax + padding, targetTickCount);
    }

    /// <summary>
    /// Calculates tick information for both X and Y axes simultaneously.
    /// </summary>
    public static (AxisTickInfo xTicks, AxisTickInfo yTicks) CalculateAxisTicks(
      double xMin, double xMax, double yMin, double yMax,
      int xTickCount = 10, int yTickCount = 8)
    {
      var xTicks = CalculateTicks(xMin, xMax, xTickCount);
      var yTicks = CalculateTicks(yMin, yMax, yTickCount);
      return (xTicks, yTicks);
    }

    private static double FindNiceNumber(double value)
    {
      foreach (double nice in NiceNumbers)
      {
        if (nice >= value * 0.9)
          return nice;
      }
      return NiceNumbers[NiceNumbers.Length - 1];
    }

    private static int CalculateDecimalPlaces(double tickSpacing)
    {
      if (tickSpacing >= 1.0) return 0;
      double log = Math.Log10(tickSpacing);
      int decimals = (int)Math.Ceiling(-log);
      return Math.Max(0, Math.Min(decimals, 10));
    }

    private static bool ShouldUseScientificNotation(double min, double max, double tickSpacing)
    {
      double maxAbs = Math.Max(Math.Abs(min), Math.Abs(max));
      return maxAbs >= 100000 || (maxAbs > 0 && maxAbs < 0.01);
    }

    private static List<AxisTick> GenerateTicks(double min, double max, double spacing, int decimalPlaces, bool useScientific)
    {
      var ticks = new List<AxisTick>();
      double epsilon = spacing * 1e-10;

      for (double value = min; value <= max + epsilon; value += spacing)
      {
        double roundedValue = Math.Round(value / spacing) * spacing;
        if (Math.Abs(roundedValue) < spacing * 1e-10)
          roundedValue = 0;

        ticks.Add(new AxisTick
        {
          Value = roundedValue,
          Label = FormatTickLabel(roundedValue, decimalPlaces, useScientific),
          IsMajor = true
        });
      }

      return ticks;
    }

    private static string FormatTickLabel(double value, int decimalPlaces, bool useScientific)
    {
      if (Math.Abs(value) < 1e-15) return "0";
      if (useScientific) return value.ToString("E2");
      if (decimalPlaces == 0) return ((long)Math.Round(value)).ToString();
      return value.ToString($"F{decimalPlaces}");
    }

    private static AxisTickInfo CreateDefaultTicks(double min, double max, int tickCount)
    {
      var result = new AxisTickInfo
      {
        Min = min,
        Max = max,
        TickSpacing = (max - min) / (tickCount - 1),
        DecimalPlaces = 0,
        UseScientificNotation = false
      };

      for (int i = 0; i < tickCount; i++)
      {
        double value = min + i * result.TickSpacing;
        result.Ticks.Add(new AxisTick { Value = value, Label = value.ToString("F0"), IsMajor = true });
      }

      return result;
    }
  }

  #endregion

  #region Coordinate Transform

  /// <summary>
  /// Provides coordinate transformation utilities between world and screen coordinates.
  /// </summary>
  public static class CoordTransform
  {
    /// <summary>
    /// Transforms a point from world coordinates to screen coordinates.
    /// </summary>
    public static Point WorldToScreen(double worldX, double worldY, CoordSystemParams coordParams)
    {
      double screenX = coordParams._centerX + worldX * coordParams._scaleX;
      double screenY = coordParams._centerY - worldY * coordParams._scaleY;
      return new Point(screenX, screenY);
    }

    /// <summary>
    /// Transforms a point from screen coordinates to world coordinates.
    /// </summary>
    public static Point ScreenToWorld(double screenX, double screenY, CoordSystemParams coordParams)
    {
      double worldX = (screenX - coordParams._centerX) / coordParams._scaleX;
      double worldY = (coordParams._centerY - screenY) / coordParams._scaleY;
      return new Point(worldX, worldY);
    }

    /// <summary>
    /// Transforms only the X coordinate from world to screen space.
    /// </summary>
    public static double WorldToScreenX(double worldX, CoordSystemParams coordParams)
    {
      return coordParams._centerX + worldX * coordParams._scaleX;
    }

    /// <summary>
    /// Transforms only the Y coordinate from world to screen space.
    /// </summary>
    public static double WorldToScreenY(double worldY, CoordSystemParams coordParams)
    {
      return coordParams._centerY - worldY * coordParams._scaleY;
    }
  }

  #endregion

  #region Coordinate System Style

  /// <summary>
  /// Configuration class for coordinate system visual styling.
  /// </summary>
  public class CoordSystemStyle
  {
    /// <summary>Gets or sets the brush used for the main axis lines.</summary>
    public Brush AxisColor { get; set; } = Brushes.Black;

    /// <summary>Gets or sets the brush used for grid lines.</summary>
    public Brush GridColor { get; set; } = Brushes.LightGray;

    /// <summary>Gets or sets the brush used for tick marks.</summary>
    public Brush TickColor { get; set; } = Brushes.Black;

    /// <summary>Gets or sets the brush used for axis labels.</summary>
    public Brush LabelColor { get; set; } = Brushes.Black;

    /// <summary>Gets or sets the thickness of axis lines in pixels.</summary>
    public double AxisThickness { get; set; } = 1.5;

    /// <summary>Gets or sets the thickness of grid lines in pixels.</summary>
    public double GridThickness { get; set; } = 0.5;

    /// <summary>Gets or sets the length of tick marks in pixels.</summary>
    public double TickLength { get; set; } = 5;

    /// <summary>Gets or sets the font size for axis labels.</summary>
    public double LabelFontSize { get; set; } = 11;

    /// <summary>Gets or sets the font family for axis labels.</summary>
    public FontFamily LabelFontFamily { get; set; } = new FontFamily("Segoe UI");

    /// <summary>Gets or sets whether to display the grid lines.</summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>Gets or sets whether to display numerical labels on the axes.</summary>
    public bool ShowAxisLabels { get; set; } = true;

    /// <summary>Gets or sets the horizontal offset for Y-axis labels from the axis.</summary>
    public double LabelOffsetX { get; set; } = 5;

    /// <summary>Gets or sets the vertical offset for X-axis labels from the axis.</summary>
    public double LabelOffsetY { get; set; } = 5;
  }

  #endregion

  #region Coordinate System Renderer

  /// <summary>
  /// Renders a complete 2D coordinate system with axes, grid lines, ticks, and labels.
  /// </summary>
  public static class CoordSystemRenderer
  {
    private static readonly CoordSystemStyle DefaultStyle = new CoordSystemStyle();

    /// <summary>
    /// Draws a complete coordinate system on the specified canvas.
    /// </summary>
    public static void Draw(Canvas canvas, CoordSystemParams coordParams,
      double dataXMin, double dataXMax, double dataYMin, double dataYMax,
      CoordSystemStyle? style = null)
    {
      style ??= DefaultStyle;

      var (xTickInfo, yTickInfo) = AxisTickCalculator.CalculateAxisTicks(
        dataXMin, dataXMax, dataYMin, dataYMax, 10, 8);

      if (style.ShowGrid)
      {
        DrawGrid(canvas, coordParams, xTickInfo, yTickInfo, style);
      }

      DrawAxes(canvas, coordParams, xTickInfo, yTickInfo, style);
      DrawXAxisTicks(canvas, coordParams, xTickInfo, yTickInfo, style);
      DrawYAxisTicks(canvas, coordParams, xTickInfo, yTickInfo, style);
    }

    /// <summary>
    /// Updates coordinate system parameters with nice rounded bounds and calculates scales.
    /// </summary>
    public static void UpdateParamsWithNiceBounds(CoordSystemParams coordParams,
      double dataXMin, double dataXMax, double dataYMin, double dataYMax,
      bool preserveAspectRatio = false)
    {
      var (xTicks, yTicks) = AxisTickCalculator.CalculateAxisTicks(
        dataXMin, dataXMax, dataYMin, dataYMax);

      coordParams._xMin = xTicks.Min;
      coordParams._xMax = xTicks.Max;
      coordParams._yMin = yTicks.Min;
      coordParams._yMax = yTicks.Max;

      double xRange = coordParams._xMax - coordParams._xMin;
      double yRange = coordParams._yMax - coordParams._yMin;
      double midPointX = (coordParams._xMin + coordParams._xMax) / 2;
      double midPointY = (coordParams._yMin + coordParams._yMax) / 2;

      double availableWidth = coordParams._windowWidth * 0.9;
      double availableHeight = coordParams._windowHeight * 0.9;

      if (preserveAspectRatio)
      {
        double scaleX = availableWidth / xRange;
        double scaleY = availableHeight / yRange;
        double uniformScale = Math.Min(scaleX, scaleY);
        coordParams._scaleX = uniformScale;
        coordParams._scaleY = uniformScale;
      }
      else
      {
        coordParams._scaleX = availableWidth / xRange;
        coordParams._scaleY = availableHeight / yRange;
      }

      coordParams._centerX = coordParams._windowWidth / 2 - midPointX * coordParams._scaleX;
      coordParams._centerY = coordParams._windowHeight / 2 + midPointY * coordParams._scaleY;
    }

    #region Private Drawing Methods

    private static void DrawGrid(Canvas canvas, CoordSystemParams coordParams,
      AxisTickInfo xTicks, AxisTickInfo yTicks, CoordSystemStyle style)
    {
      // Vertical grid lines (at X tick positions)
      foreach (var tick in xTicks.Ticks)
      {
        Point top = CoordTransform.WorldToScreen(tick.Value, yTicks.Max, coordParams);
        Point bottom = CoordTransform.WorldToScreen(tick.Value, yTicks.Min, coordParams);

        canvas.Children.Add(new Line
        {
          X1 = top.X,
          Y1 = top.Y,
          X2 = bottom.X,
          Y2 = bottom.Y,
          Stroke = style.GridColor,
          StrokeThickness = style.GridThickness
        });
      }

      // Horizontal grid lines (at Y tick positions)
      foreach (var tick in yTicks.Ticks)
      {
        Point left = CoordTransform.WorldToScreen(xTicks.Min, tick.Value, coordParams);
        Point right = CoordTransform.WorldToScreen(xTicks.Max, tick.Value, coordParams);

        canvas.Children.Add(new Line
        {
          X1 = left.X,
          Y1 = left.Y,
          X2 = right.X,
          Y2 = right.Y,
          Stroke = style.GridColor,
          StrokeThickness = style.GridThickness
        });
      }
    }

    private static void DrawAxes(Canvas canvas, CoordSystemParams coordParams,
      AxisTickInfo xTicks, AxisTickInfo yTicks, CoordSystemStyle style)
    {
      // Determine X-axis Y position (at y=0 if visible, else at edge)
      double xAxisY = 0;
      if (yTicks.Min > 0) xAxisY = yTicks.Min;
      else if (yTicks.Max < 0) xAxisY = yTicks.Max;

      // Determine Y-axis X position (at x=0 if visible, else at edge)
      double yAxisX = 0;
      if (xTicks.Min > 0) yAxisX = xTicks.Min;
      else if (xTicks.Max < 0) yAxisX = xTicks.Max;

      // Draw X-axis
      Point xAxisStart = CoordTransform.WorldToScreen(xTicks.Min, xAxisY, coordParams);
      Point xAxisEnd = CoordTransform.WorldToScreen(xTicks.Max, xAxisY, coordParams);

      canvas.Children.Add(new Line
      {
        X1 = xAxisStart.X,
        Y1 = xAxisStart.Y,
        X2 = xAxisEnd.X,
        Y2 = xAxisEnd.Y,
        Stroke = style.AxisColor,
        StrokeThickness = style.AxisThickness
      });

      // Draw Y-axis
      Point yAxisStart = CoordTransform.WorldToScreen(yAxisX, yTicks.Min, coordParams);
      Point yAxisEnd = CoordTransform.WorldToScreen(yAxisX, yTicks.Max, coordParams);

      canvas.Children.Add(new Line
      {
        X1 = yAxisStart.X,
        Y1 = yAxisStart.Y,
        X2 = yAxisEnd.X,
        Y2 = yAxisEnd.Y,
        Stroke = style.AxisColor,
        StrokeThickness = style.AxisThickness
      });
    }

    private static void DrawXAxisTicks(Canvas canvas, CoordSystemParams coordParams,
      AxisTickInfo xTicks, AxisTickInfo yTicks, CoordSystemStyle style)
    {
      double tickY = 0;
      if (yTicks.Min > 0) tickY = yTicks.Min;
      else if (yTicks.Max < 0) tickY = yTicks.Max;

      bool labelsBelow = tickY >= 0 || yTicks.Max < 0;

      foreach (var tick in xTicks.Ticks)
      {
        Point tickPos = CoordTransform.WorldToScreen(tick.Value, tickY, coordParams);

        // Draw tick mark
        canvas.Children.Add(new Line
        {
          X1 = tickPos.X,
          Y1 = tickPos.Y - style.TickLength / 2,
          X2 = tickPos.X,
          Y2 = tickPos.Y + style.TickLength / 2,
          Stroke = style.TickColor,
          StrokeThickness = 1
        });

        // Draw label
        if (style.ShowAxisLabels)
        {
          var label = new TextBlock
          {
            Text = tick.Label,
            Foreground = style.LabelColor,
            FontSize = style.LabelFontSize,
            FontFamily = style.LabelFontFamily
          };

          label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
          double labelX = tickPos.X - label.DesiredSize.Width / 2;
          double labelY = labelsBelow ? tickPos.Y + style.LabelOffsetY : tickPos.Y - label.DesiredSize.Height - style.LabelOffsetY;

          canvas.Children.Add(label);
          Canvas.SetLeft(label, labelX);
          Canvas.SetTop(label, labelY);
        }
      }
    }

    private static void DrawYAxisTicks(Canvas canvas, CoordSystemParams coordParams,
      AxisTickInfo xTicks, AxisTickInfo yTicks, CoordSystemStyle style)
    {
      double tickX = 0;
      if (xTicks.Min > 0) tickX = xTicks.Min;
      else if (xTicks.Max < 0) tickX = xTicks.Max;

      bool labelsLeft = tickX <= 0 || xTicks.Min > 0;

      foreach (var tick in yTicks.Ticks)
      {
        Point tickPos = CoordTransform.WorldToScreen(tickX, tick.Value, coordParams);

        // Draw tick mark
        canvas.Children.Add(new Line
        {
          X1 = tickPos.X - style.TickLength / 2,
          Y1 = tickPos.Y,
          X2 = tickPos.X + style.TickLength / 2,
          Y2 = tickPos.Y,
          Stroke = style.TickColor,
          StrokeThickness = 1
        });

        // Draw label
        if (style.ShowAxisLabels)
        {
          var label = new TextBlock
          {
            Text = tick.Label,
            Foreground = style.LabelColor,
            FontSize = style.LabelFontSize,
            FontFamily = style.LabelFontFamily
          };

          label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
          double labelX = labelsLeft ? tickPos.X - label.DesiredSize.Width - style.LabelOffsetX : tickPos.X + style.LabelOffsetX;
          double labelY = tickPos.Y - label.DesiredSize.Height / 2;

          canvas.Children.Add(label);
          Canvas.SetLeft(label, labelX);
          Canvas.SetTop(label, labelY);
        }
      }
    }

    #endregion
  }

  #endregion

  #region Legacy Utils2D (for backward compatibility)

  /// <summary>
  /// Legacy 2D drawing utilities. Use CoordSystemRenderer instead.
  /// </summary>
  [Obsolete("Use CoordSystemRenderer.Draw() instead")]
  public static class Utils2D
  {
    /// <summary>
    /// Draws a point on the canvas.
    /// </summary>
    public static void DrawPoint(Canvas mainCanvas, CoordSystemParams coordSysParams, double x, double y, Color inColor)
    {
      var point = CoordTransform.WorldToScreen(x, y, coordSysParams);

      Ellipse circle = new Ellipse
      {
        Width = 5,
        Height = 5,
        Fill = new SolidColorBrush(inColor)
      };
      mainCanvas.Children.Add(circle);
      Canvas.SetLeft(circle, point.X - 2.5);
      Canvas.SetTop(circle, point.Y - 2.5);
    }
  }

  #endregion
}
