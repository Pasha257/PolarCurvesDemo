using PolarCurvesDemo.Controls;
using PolarCurvesDemo.Models;
using PolarCurvesDemo.Services;
using System.Windows.Forms;

namespace PolarCurvesDemo.Forms;

public sealed partial class MainForm : Form
{
    private readonly PolarCurveService _curveService = new();
    private readonly PolarChartControl _chart = new();
    private readonly ComboBox _curveComboBox = new();
    private readonly NumericUpDown _aNumeric = new();
    private readonly NumericUpDown _bNumeric = new();
    private readonly NumericUpDown _petalsNumeric = new();
    private readonly NumericUpDown _angleNumeric = new();
    private readonly NumericUpDown _pointsNumeric = new();
    private readonly NumericUpDown _scaleNumeric = new();
    private readonly NumericUpDown _gridStepNumeric = new();
    private readonly CheckBox _gridCheckBox = new();
    private readonly CheckBox _labelsCheckBox = new();
    private readonly CheckBox _axesCheckBox = new();
    private readonly Button _buildButton = new();
    private readonly Button _animateButton = new();
    private readonly Button _pauseButton = new();
    private readonly Button _resetButton = new();
    private readonly Button _saveButton = new();
    private readonly TrackBar _speedTrackBar = new();
    private readonly TextBox _formulaTextBox = new();
    private readonly TextBox _descriptionTextBox = new();
    private readonly TextBox _infoTextBox = new();
    private readonly ListBox _pointsListBox = new();
    private readonly Label _statusLabel = new();
    private readonly System.Windows.Forms.Timer _animationTimer = new();

    private CurveDefinition _selectedCurve;
    private List<PolarPoint> _currentPoints = new();
    private double _currentVisibleRadius = 5;
    private bool _isAnimating;
    private double _animationProgress;

    public MainForm()
    {
        Text = "Демонстраційно-навчальна програма «Криві у полярних координатах»";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1200, 720);
        Font = new Font("Segoe UI", 9.5F);

        _selectedCurve = PolarCurveService.Curves[0];

        InitializeUi();
        LoadCurves();
        ApplyCurveDefaults();
        BuildCurve();
    }

    private void InitializeUi()
    {
        var controlsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.FromArgb(245, 247, 250),
            Margin = new Padding(0)
        };

        var title = new Label
        {
            Text = "Криві у полярних координатах",
            Width = 300,
            Height = 64,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 45, 80),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 8)
        };

        var subtitle = new Label
        {
            Text = "Оберіть криву, задайте параметри та побудуйте графік",
            Width = 300,
            Height = 48,
            ForeColor = Color.FromArgb(90, 100, 115),
            Margin = new Padding(0, 0, 0, 12)
        };

        _curveComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _curveComboBox.SelectedIndexChanged += CurveComboBoxSelectedIndexChanged;

        ConfigureNumeric(_aNumeric, -20, 20, 0.1, 2);
        ConfigureNumeric(_bNumeric, -20, 20, 0.1, 2);
        ConfigureNumeric(_petalsNumeric, 1, 50, 1, 0);
        ConfigureNumeric(_angleNumeric, Math.PI, 20 * Math.PI, 0.5, 2);
        ConfigureNumeric(_pointsNumeric, 50, 10000, 100, 0);
        ConfigureNumeric(_scaleNumeric, 0.1, 10, 0.1, 1);
        ConfigureNumeric(_gridStepNumeric, 1, 20, 1, 0);

        _gridCheckBox.Text = "Показувати сітку";
        _gridCheckBox.Checked = true;
        _gridCheckBox.CheckedChanged += (_, _) => UpdateChartSettings();

        _labelsCheckBox.Text = "Показувати підписи";
        _labelsCheckBox.Checked = true;
        _labelsCheckBox.CheckedChanged += (_, _) => UpdateChartSettings();

        _axesCheckBox.Text = "Показувати осі";
        _axesCheckBox.Checked = true;
        _axesCheckBox.CheckedChanged += (_, _) => UpdateChartSettings();

        _gridStepNumeric.ValueChanged += (_, _) => UpdateChartSettings();

        _buildButton.Text = "Побудувати криву";
        _buildButton.BackColor = Color.FromArgb(0, 102, 204);
        _buildButton.ForeColor = Color.White;
        _buildButton.FlatStyle = FlatStyle.Flat;
        _buildButton.FlatAppearance.BorderSize = 0;
        _buildButton.Click += (_, _) => BuildCurve();

        _animateButton.Text = "Анімація побудови";
        _animateButton.BackColor = Color.FromArgb(35, 150, 110);
        _animateButton.ForeColor = Color.White;
        _animateButton.FlatStyle = FlatStyle.Flat;
        _animateButton.FlatAppearance.BorderSize = 0;
        _animateButton.Click += (_, _) => StartAnimation();

        _pauseButton.Text = "Пауза";
        _pauseButton.Enabled = false;
        _pauseButton.Click += (_, _) => TogglePause();

        _resetButton.Text = "Скинути параметри";
        _resetButton.Click += (_, _) =>
        {
            ApplyCurveDefaults();
            BuildCurve();
        };

        _saveButton.Text = "Зберегти графік у PNG";
        _saveButton.Click += SaveChartAsPng;

        _speedTrackBar.Minimum = 1;
        _speedTrackBar.Maximum = 20;
        _speedTrackBar.Value = 5;
        _speedTrackBar.Width = 300;
        _speedTrackBar.TickFrequency = 1;
        _speedTrackBar.ValueChanged += (_, _) => UpdateAnimationSpeed();

        _formulaTextBox.Multiline = true;
        _formulaTextBox.ReadOnly = true;
        _formulaTextBox.Height = 54;
        _formulaTextBox.Font = new Font("Consolas", 11, FontStyle.Bold);
        _formulaTextBox.BackColor = Color.FromArgb(235, 242, 255);

        _descriptionTextBox.Multiline = true;
        _descriptionTextBox.ReadOnly = true;
        _descriptionTextBox.Height = 90;
        _descriptionTextBox.ScrollBars = ScrollBars.Vertical;
        _descriptionTextBox.BackColor = Color.White;

        _infoTextBox.Multiline = true;
        _infoTextBox.ReadOnly = true;
        _infoTextBox.Height = 150;
        _infoTextBox.ScrollBars = ScrollBars.Vertical;
        _infoTextBox.BackColor = Color.White;

        _pointsListBox.Dock = DockStyle.Fill;
        _pointsListBox.Font = new Font("Consolas", 9);
        _pointsListBox.HorizontalScrollbar = true;
        _pointsListBox.IntegralHeight = false;

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.AutoEllipsis = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.ForeColor = Color.FromArgb(70, 80, 95);
        _statusLabel.Padding = new Padding(8, 0, 0, 0);
        _statusLabel.Margin = new Padding(0);

        _animationTimer.Interval = 40;
        _animationTimer.Tick += AnimationTimerTick;

        _chart.Dock = DockStyle.Fill;
        _chart.Margin = new Padding(8);

        foreach (Button button in new[]
        {
    _buildButton,
    _animateButton,
    _pauseButton,
    _resetButton,
    _saveButton
})
        {
            button.Width = 300;
            button.Height = 38;
            button.Margin = new Padding(0, 0, 0, 8);
        }

        foreach (CheckBox checkBox in new[]
        {
    _gridCheckBox,
    _labelsCheckBox,
    _axesCheckBox
})
        {
            checkBox.AutoSize = false;
            checkBox.Width = 300;
            checkBox.Height = 28;
            checkBox.Margin = new Padding(0, 0, 0, 4);
        }

        controlsPanel.Controls.Add(title);
        controlsPanel.Controls.Add(subtitle);
        controlsPanel.Controls.Add(CreateField("Тип кривої", _curveComboBox));
        controlsPanel.Controls.Add(CreateField("Параметр a", _aNumeric));
        controlsPanel.Controls.Add(CreateField("Параметр b", _bNumeric));
        controlsPanel.Controls.Add(CreateField("Параметр k", _petalsNumeric));
        controlsPanel.Controls.Add(CreateField("Максимальний кут, радіани", _angleNumeric));
        controlsPanel.Controls.Add(CreateField("Кількість інтервалів побудови", _pointsNumeric));
        controlsPanel.Controls.Add(CreateField("Масштаб", _scaleNumeric));
        controlsPanel.Controls.Add(CreateField("Крок сітки", _gridStepNumeric));
        controlsPanel.Controls.Add(_gridCheckBox);
        controlsPanel.Controls.Add(_labelsCheckBox);
        controlsPanel.Controls.Add(_axesCheckBox);
        controlsPanel.Controls.Add(_buildButton);
        controlsPanel.Controls.Add(_animateButton);
        controlsPanel.Controls.Add(_pauseButton);
        controlsPanel.Controls.Add(CreateField("Швидкість анімації", _speedTrackBar));
        controlsPanel.Controls.Add(_resetButton);
        controlsPanel.Controls.Add(_saveButton);
        controlsPanel.Controls.Add(CreateField("Формула", _formulaTextBox));
        controlsPanel.Controls.Add(CreateField("Опис", _descriptionTextBox));
        controlsPanel.Controls.Add(CreateField("Обчислені дані", _infoTextBox));

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Absolute, 360F));

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        layout.Controls.Add(controlsPanel, 0, 0);
        layout.Controls.Add(_chart, 1, 0);

        var pointsTitle = new Label
        {
            Text = "Точки кривої: θ, r, x, y",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Padding = new Padding(8, 0, 0, 0),
            Margin = new Padding(0)
        };

        var pointsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0),
            Padding = new Padding(4)
        };

        pointsPanel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        pointsPanel.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 28F));

        pointsPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        pointsPanel.Controls.Add(pointsTitle, 0, 0);
        pointsPanel.Controls.Add(_pointsListBox, 0, 1);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        mainLayout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100F));

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100F));

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 180F));

        mainLayout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 28F));

        mainLayout.Controls.Add(layout, 0, 0);
        mainLayout.Controls.Add(pointsPanel, 0, 1);
        mainLayout.Controls.Add(_statusLabel, 0, 2);

        Controls.Add(mainLayout);
    }

    private static FlowLayoutPanel CreateField(string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            Width = 300,
            Height = 22,
            TextAlign = ContentAlignment.MiddleLeft
        };

        control.Width = 300;

        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            Width = 310,
            Height = control.Height + 28,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };

        panel.Controls.Add(label);
        panel.Controls.Add(control);

        return panel;
    }

    private static void ConfigureNumeric(
    NumericUpDown numeric,
    double minimum,
    double maximum,
    double increment,
    int decimals)
    {
        numeric.Minimum = (decimal)minimum;
        numeric.Maximum = (decimal)maximum;
        numeric.Increment = (decimal)increment;
        numeric.DecimalPlaces = decimals;
        numeric.Width = 300;
    }

    private void CurveComboBoxSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_curveComboBox.SelectedIndex < 0)
        {
            return;
        }

        _selectedCurve = PolarCurveService.Curves[_curveComboBox.SelectedIndex];

        ApplyCurveDefaults();
        BuildCurve();
    }

    private void LoadCurves()
    {
        _curveComboBox.Items.Clear();

        foreach (CurveDefinition curve in PolarCurveService.Curves)
        {
            _curveComboBox.Items.Add(curve.Name);
        }

        _curveComboBox.SelectedIndex = 0;
    }

    private void ApplyCurveDefaults()
    {
        _aNumeric.Value = (decimal)_selectedCurve.DefaultA;
        _bNumeric.Value = (decimal)_selectedCurve.DefaultB;
        _petalsNumeric.Value = _selectedCurve.DefaultPetals;
        _angleNumeric.Value = (decimal)_selectedCurve.DefaultAngleLimit;
        _pointsNumeric.Value = 1000;
        _scaleNumeric.Value = (decimal)_selectedCurve.RecommendedScale;
        _gridStepNumeric.Value = 1;

        _formulaTextBox.Text = _selectedCurve.Formula;
        _descriptionTextBox.Text = _selectedCurve.Description;
    }

    private void BuildCurve()
    {
        double a = Convert.ToDouble(_aNumeric.Value);
        double b = Convert.ToDouble(_bNumeric.Value);
        int petals = Convert.ToInt32(_petalsNumeric.Value);
        double angleLimit = Convert.ToDouble(_angleNumeric.Value);
        int pointCount = Convert.ToInt32(_pointsNumeric.Value);
        double scale = Convert.ToDouble(_scaleNumeric.Value);

        _currentPoints = _curveService.BuildPoints(
            _selectedCurve,
            a,
            b,
            petals,
            angleLimit,
            pointCount);

        double maxRadius = _curveService.GetMaxRadius(_currentPoints);
        _currentVisibleRadius = Math.Max(0.5, maxRadius * scale);

        _animationProgress = 0;
        _isAnimating = false;

        _animationTimer.Stop();
        _pauseButton.Enabled = false;
        _pauseButton.Text = "Пауза";
        _animateButton.Text = "Анімація побудови";

        _chart.SetCurve(_currentPoints, _currentVisibleRadius);
        UpdateChartSettings();
        UpdateInfo(a, b, petals, angleLimit, pointCount, maxRadius);
        UpdatePointsList();

        _statusLabel.Text =
            $"Крива: {_selectedCurve.Name} | " +
            $"Точок: {_currentPoints.Count} | " +
            $"Максимальний радіус: {maxRadius:0.###}";
    }

    private void UpdateChartSettings()
    {
        _chart.SetGridSettings(
            _gridCheckBox.Checked,
            _labelsCheckBox.Checked,
            _axesCheckBox.Checked,
            Convert.ToInt32(_gridStepNumeric.Value));
    }

    private void UpdateInfo(
        double a,
        double b,
        int petals,
        double angleLimit,
        int pointCount,
        double maxRadius)
    {
        double degrees = angleLimit * 180 / Math.PI;

        _infoTextBox.Text =
            $"Тип: {_selectedCurve.Name}{Environment.NewLine}" +
            $"Параметри: a = {a:0.###}, b = {b:0.###}, k = {petals}{Environment.NewLine}" +
            $"Кут: 0 … {angleLimit:0.###} рад ({degrees:0.#}°){Environment.NewLine}" +
            $"Точок: {pointCount}{Environment.NewLine}" +
            $"Максимальний |r|: {maxRadius:0.###}{Environment.NewLine}" +
        $"Перетворення: x = r cos θ, y = r sin θ";
    }

    private void UpdatePointsList()
    {
        _pointsListBox.Items.Clear();

        int shown = Math.Min(30, _currentPoints.Count);

        for (int i = 0; i < shown; i++)
        {
            PolarPoint point = _currentPoints[i];

            _pointsListBox.Items.Add(
                $"θ = {point.Theta:0.000} рад | " +
                $"r = {point.Radius:0.000} | " +
                $"x = {point.X:0.000} | " +
                $"y = {point.Y:0.000}");
        }

        if (_currentPoints.Count > shown)
        {
            _pointsListBox.Items.Add($"… ще {_currentPoints.Count - shown} точок");
        }
    }

    private void StartAnimation()
    {
        if (_currentPoints.Count == 0)
        {
            return;
        }

        _animationProgress = 0;
        _isAnimating = true;

        _animateButton.Text = "Перезапустити";
        _pauseButton.Text = "Пауза";
        _pauseButton.Enabled = true;

        UpdateAnimationSpeed();
        _animationTimer.Start();
    }

    private void TogglePause()
    {
        if (!_isAnimating)
        {
            return;
        }

        if (_animationTimer.Enabled)
        {
            _animationTimer.Stop();
            _pauseButton.Text = "Продовжити";
        }
        else
        {
            _animationTimer.Start();
            _pauseButton.Text = "Пауза";
        }
    }

    private void UpdateAnimationSpeed()
    {
        int interval = 110 - _speedTrackBar.Value * 5;

        _animationTimer.Interval = Math.Max(1, interval);
    }

    private void AnimationTimerTick(object? sender, EventArgs e)
    {
        if (!_isAnimating || _currentPoints.Count == 0)
        {
            _animationTimer.Stop();
            return;
        }

        int totalPoints = _currentPoints.Count;
        double step = Math.Max(1, totalPoints / 120.0);

        _animationProgress += step;

        if (_animationProgress >= totalPoints)
        {
            _animationProgress = totalPoints;
            _animationTimer.Stop();

            _isAnimating = false;
            _pauseButton.Enabled = false;
            _pauseButton.Text = "Пауза";
        }

        int visiblePoints = (int)_animationProgress;

        if (visiblePoints < 2)
        {
            return;
        }

        _chart.SetCurve(
            _currentPoints.Take(visiblePoints).ToList(),
            _currentVisibleRadius);

        _statusLabel.Text =
            $"Анімація: {visiblePoints} із {totalPoints} точок " +
            $"({visiblePoints * 100.0 / totalPoints:0.0}%)";
    }

    private void SaveChartAsPng(object? sender, EventArgs e)
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "PNG зображення|*.png",
            FileName = $"{_selectedCurve.Name}.png"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        using Bitmap bitmap = new(_chart.Width, _chart.Height);

        _chart.DrawToBitmap(
            bitmap,
            new Rectangle(0, 0, bitmap.Width, bitmap.Height));

        bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);

        MessageBox.Show(
            "Графік успішно збережено.",
            "Збереження",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}