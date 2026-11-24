namespace PolarCurvesDemo.Models;

public sealed class CurveDefinition
{
    public string Name { get; init; } = string.Empty;

    public string Formula { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Func<double, double, double, int, double> RadiusFunction { get; init; } =
        (_, _, _, _) => 0;

    public double DefaultA { get; init; } = 1;

    public double DefaultB { get; init; } = 1;

    public int DefaultPetals { get; init; } = 5;

    public double DefaultAngleLimit { get; init; } = 2 * Math.PI;

    public double RecommendedScale { get; init; } = 1;
}