using PolarCurvesDemo.Models;

namespace PolarCurvesDemo.Services;

public sealed record PolarPoint(double Theta, double Radius, double X, double Y);

public sealed class PolarCurveService
{
    public static readonly IReadOnlyList<CurveDefinition> Curves = new List<CurveDefinition>
{
new()
{
Name = "Коло",
Formula = "r = a",
Description = "Коло з центром у полюсі та радіусом a.",
RadiusFunction = (theta, a, b, petals) => a,
DefaultA = 2,
DefaultB = 1,
DefaultPetals = 5,
DefaultAngleLimit = 2 * Math.PI,
RecommendedScale = 1
},
new()
{
Name = "Кардіоїда",
Formula = "r = a(1 + cos θ)",
Description = "Крива у формі серця; утворюється як епіциклоїда з одним циклом.",
RadiusFunction = (theta, a, b, petals) => a * (1 + Math.Cos(theta)),
DefaultA = 2,
DefaultB = 1,
DefaultPetals = 5,
DefaultAngleLimit = 2 * Math.PI,
RecommendedScale = 1
},
new()
{
Name = "Лімніската",
Formula = "r² = a² cos 2θ",
Description = "Крива, що має форму вісімки та симетрична відносно полюса.",
RadiusFunction = (theta, a, b, petals) =>
{
double value = a * a * Math.Cos(2 * theta);

            return value >= 0 ? Math.Sqrt(value) : double.NaN;
        },
        DefaultA = 3,
        DefaultB = 1,
        DefaultPetals = 5,
        DefaultAngleLimit = 2 * Math.PI,
        RecommendedScale = 1
    },
    new()
    {
        Name = "Троянда",
        Formula = "r = a cos(kθ)",
        Description = "Крива з k пелюстками, якщо k непарне, або з 2k пелюстками, якщо k парне.",
        RadiusFunction = (theta, a, b, petals) => a * Math.Cos(petals * theta),
        DefaultA = 3,
        DefaultB = 1,
        DefaultPetals = 5,
        DefaultAngleLimit = 2 * Math.PI,
        RecommendedScale = 1
    },
    new()
    {
        Name = "Спіраль Архімеда",
        Formula = "r = aθ",
        Description = "Відстань від полюса зростає пропорційно куту повороту.",
        RadiusFunction = (theta, a, b, petals) => a * theta,
        DefaultA = 0.5,
        DefaultB = 1,
        DefaultPetals = 5,
        DefaultAngleLimit = 6 * Math.PI,
        RecommendedScale = 0.6
    },
    new()
    {
        Name = "Равлик Паскаля",
        Formula = "r = b + a cos θ",
        Description = "Узагальнення кардіоїди; форма залежить від співвідношення a та b.",
        RadiusFunction = (theta, a, b, petals) => b + a * Math.Cos(theta),
        DefaultA = 2,
        DefaultB = 3,
        DefaultPetals = 5,
        DefaultAngleLimit = 2 * Math.PI,
        RecommendedScale = 1
    },
    new()
    {
        Name = "Лемніската Бернуллі",
        Formula = "r² = a² cos 2θ / (1 + sin²θ)",
        Description = "Крива у формі вісімки з характерною асимптотичною поведінкою.",
        RadiusFunction = (theta, a, b, petals) =>
        {
            double denominator = 1 + Math.Pow(Math.Sin(theta), 2);
            double value = a * a * Math.Cos(2 * theta) / denominator;

            return value >= 0 ? Math.Sqrt(value) : double.NaN;
        },
        DefaultA = 3,
        DefaultB = 1,
        DefaultPetals = 5,
        DefaultAngleLimit = 2 * Math.PI,
        RecommendedScale = 1
    },
    new()
    {
        Name = "Крива Ліссажу у полярній формі",
        Formula = "r = a sin(kθ)",
        Description = "Періодична крива, що демонструє вплив коефіцієнта k на кількість пелюсток.",
        RadiusFunction = (theta, a, b, petals) => a * Math.Sin(petals * theta),
        DefaultA = 3,
        DefaultB = 1,
        DefaultPetals = 7,
        DefaultAngleLimit = 2 * Math.PI,
        RecommendedScale = 1
    }
};

    public List<PolarPoint> BuildPoints(
        CurveDefinition curve,
        double a,
        double b,
        int petals,
        double angleLimit,
        int pointCount)
    {
        var points = new List<PolarPoint>(pointCount + 1);
        double step = angleLimit / pointCount;

        for (int i = 0; i <= pointCount; i++)
        {
            double theta = i * step;
            double radius = curve.RadiusFunction(theta, a, b, petals);

            if (double.IsNaN(radius) || double.IsInfinity(radius))
            {
                continue;
            }

            double x = radius * Math.Cos(theta);
            double y = radius * Math.Sin(theta);

            points.Add(new PolarPoint(theta, radius, x, y));
        }

        return points;
    }

    public double GetMaxRadius(IEnumerable<PolarPoint> points)
    {
        double max = 0;

        foreach (PolarPoint point in points)
        {
            max = Math.Max(max, Math.Abs(point.Radius));
        }

        return max == 0 ? 1 : max;
    }
}

