using PolarCurvesDemo.Services;

namespace PolarCurvesDemo.Controls;

public sealed class PolarChartControl : Control
{
    private List<PolarPoint> _points = new();
    private double _visibleRadius = 5;
    private int _gridStep = 1;
    private bool _showGrid = true;
    private bool _showLabels = true;
    private bool _showAxes = true;

    public PolarChartControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetCurve(List<PolarPoint> points, double visibleRadius)
    {
        _points = points ?? new List<PolarPoint>();
        _visibleRadius = visibleRadius <= 0 ? 1 : visibleRadius;

        Invalidate();
    }

    public void SetGridSettings(
        bool showGrid,
        bool showLabels,
        bool showAxes,
        int gridStep)
    {
        _showGrid = showGrid;
        _showLabels = showLabels;
        _showAxes = showAxes;
        _gridStep = Math.Max(1, gridStep);

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(BackColor);

        int width = ClientSize.Width;
        int height = ClientSize.Height;

        if (width < 50 || height < 50)
        {
            return;
        }

        float centerX = width / 2f;
        float centerY = height / 2f;
        float available = Math.Min(width, height) / 2f - 30;
        float scale = available / (float)_visibleRadius;

        DrawGrid(graphics, centerX, centerY, scale);
        DrawAxes(graphics, centerX, centerY, available);
        DrawCurve(graphics, centerX, centerY, scale);
    }

    private void DrawGrid(Graphics graphics, float centerX, float centerY, float scale)
    {
        if (!_showGrid)
        {
            return;
        }

        using Pen gridPen = new(Color.FromArgb(225, 225, 225), 1);
        using Pen majorGridPen = new(Color.FromArgb(190, 190, 190), 1);
        using Font labelFont = new("Segoe UI", 8);
        using Brush labelBrush = new SolidBrush(Color.FromArgb(90, 90, 90));

        double maxRing = Math.Ceiling(_visibleRadius);

        for (double radius = _gridStep; radius <= maxRing; radius += _gridStep)
        {
            float screenRadius = (float)radius * scale;
            bool isMajor = Math.Abs(radius % (_gridStep * 5)) < 0.0001;

            graphics.DrawEllipse(
                isMajor ? majorGridPen : gridPen,
                centerX - screenRadius,
                centerY - screenRadius,
                screenRadius * 2,
                screenRadius * 2);

            if (_showLabels)
            {
                string text = radius.ToString("0.#");
                SizeF size = graphics.MeasureString(text, labelFont);

                graphics.DrawString(
                    text,
                    labelFont,
                    labelBrush,
                    centerX + screenRadius - size.Width / 2,
                    centerY - size.Height / 2);
            }
        }

        for (int angle = 0; angle < 360; angle += 15)
        {
            double radians = angle * Math.PI / 180;
            float x = centerX + (float)(Math.Cos(radians) * _visibleRadius * scale);
            float y = centerY - (float)(Math.Sin(radians) * _visibleRadius * scale);

            graphics.DrawLine(gridPen, centerX, centerY, x, y);
        }

        if (_showLabels)
        {
            string[] labels =
            {
            "0°",
            "45°",
            "90°",
            "135°",
            "180°",
            "225°",
            "270°",
            "315°"
        };

            for (int i = 0; i < labels.Length; i++)
            {
                double radians = i * Math.PI / 4;
                float x = centerX + (float)(Math.Cos(radians) * (_visibleRadius + 0.25) * scale);
                float y = centerY - (float)(Math.Sin(radians) * (_visibleRadius + 0.25) * scale);

                SizeF size = graphics.MeasureString(labels[i], labelFont);

                graphics.DrawString(
                    labels[i],
                    labelFont,
                    labelBrush,
                    x - size.Width / 2,
                    y - size.Height / 2);
            }
        }
    }

    private void DrawAxes(Graphics graphics, float centerX, float centerY, float available)
    {
        if (!_showAxes)
        {
            return;
        }
        using Pen axisPen = new(Color.FromArgb(70, 70, 70), 1.6f);
        using Font axisFont = new("Segoe UI", 9, FontStyle.Bold);
        using Brush axisBrush = new SolidBrush(Color.FromArgb(50, 50, 50));

        graphics.DrawLine(axisPen, centerX - available, centerY, centerX + available, centerY);
        graphics.DrawLine(axisPen, centerX, centerY - available, centerX, centerY + available);

        graphics.DrawString("X", axisFont, axisBrush, centerX + available + 4, centerY - 8);
        graphics.DrawString("Y", axisFont, axisBrush, centerX - 6, centerY - available - 16);
        graphics.DrawString("O", axisFont, axisBrush, centerX - 14, centerY + 2);
    }

    private void DrawCurve(Graphics graphics, float centerX, float centerY, float scale)
    {
        if (_points.Count == 0)
        {
            return;
        }

        var screenPoints = new PointF[_points.Count];

        for (int i = 0; i < _points.Count; i++)
        {
            PolarPoint point = _points[i];

            screenPoints[i] = new PointF(
                centerX + (float)point.X * scale,
                centerY - (float)point.Y * scale);
        }

        using Pen curvePen = new(Color.FromArgb(0, 102, 204), 2.4f);

        if (screenPoints.Length >= 2)
        {
            graphics.DrawLines(curvePen, screenPoints);
        }

        using SolidBrush pointBrush = new(Color.FromArgb(220, 30, 30));

        PointF last = screenPoints[^1];

        graphics.FillEllipse(
            pointBrush,
            last.X - 4,
            last.Y - 4,
            8,
            8);
    }
}