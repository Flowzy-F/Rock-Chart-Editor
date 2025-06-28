using Godot;
using static NoteInfo;
public partial class CurveDrawer : ColorRect
{
    public EasingType easingType = EasingType.Linear;
    int numPoints = 50;
    public override void _Ready()
    {
    }
    public override void _Draw()
    {
        var points = new Vector2[50];
        float width = (Size*Scale).X;
        float height = (Size * Scale).Y;

        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / (numPoints-1);
            float y = 1 - CalculateEase(t, easingType); 
            points[i] = new Vector2(t * width, y * height);
        }

        DrawPolyline(points, Colors.Red, 1.0f);
    }
    private float CalculateEase(float t, EasingType type)
    {
        t = Mathf.Clamp(t, 0f, 1f);

        switch (type)
        {
            // Linear
            case EasingType.Linear:
                return t;

            // Sine
            case EasingType.InSine:
                return 1 - Mathf.Cos(t * Mathf.Pi / 2);
            case EasingType.OutSine:
                return Mathf.Sin(t * Mathf.Pi / 2);
            case EasingType.InOutSine:
                return -(Mathf.Cos(Mathf.Pi * t) - 1) / 2;

            // Quad
            case EasingType.InQuad:
                return t * t;
            case EasingType.OutQuad:
                return 1 - (1 - t) * (1 - t);
            case EasingType.InOutQuad:
                return t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            case EasingType.InCubic:
                return t * t * t;
            case EasingType.OutCubic:
                return 1 - Mathf.Pow(1 - t, 3);
            case EasingType.InOutCubic:
                return t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;

            case EasingType.InQuart:
                return t * t * t * t;
            case EasingType.OutQuart:
                return 1 - Mathf.Pow(1 - t, 4);
            case EasingType.InOutQuart:
                return t < 0.5 ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;

            case EasingType.InQuint:
                return t * t * t * t * t;
            case EasingType.OutQuint:
                return 1 - Mathf.Pow(1 - t, 5);
            case EasingType.InOutQuint:
                return t < 0.5 ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;

            // Expo 
            case EasingType.InExpo:
                return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
            case EasingType.OutExpo:
                return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
            case EasingType.InOutExpo:
                if (t == 0) return 0;
                if (t == 1) return 1;
                return t < 0.5
                    ? Mathf.Pow(2, 20 * t - 10) / 2
                    : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;

            // Circ
            case EasingType.InCirc:
                return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
            case EasingType.OutCirc:
                return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
            case EasingType.InOutCirc:
                return t < 0.5
                    ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2
                    : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;

            // Elastic
            case EasingType.InElastic:
                const float c4 = (2 * Mathf.Pi) / 3;
                if (t == 0) return 0;
                if (t == 1) return 1;
                return -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * c4);

            case EasingType.OutElastic:
                const float c5 = (2 * Mathf.Pi) / 3;
                if (t == 0) return 0;
                if (t == 1) return 1;
                return Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c5) + 1;

            case EasingType.InOutElastic:
                const float c6 = (2 * Mathf.Pi) / 4.5f;
                if (t == 0) return 0;
                if (t == 1) return 1;
                return t < 0.5
                    ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * c6)) / 2
                    : Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * c6) / 2 + 1;

            // Back
            case EasingType.InBack:
                const float c1 = 1.70158f;
                return t * t * ((c1 + 1) * t - c1);

            case EasingType.OutBack:
                const float c2 = 1.70158f;
                return 1 + (--t) * t * ((c2 + 1) * t + c2);

            case EasingType.InOutBack:
                const float c3 = 1.70158f * 1.525f;
                return t < 0.5
                    ? (Mathf.Pow(2 * t, 2) * ((c3 + 1) * 2 * t - c3)) / 2
                    : (Mathf.Pow(2 * t - 2, 2) * ((c3 + 1) * (t * 2 - 2) + c3) + 2) / 2;

            // Bounce
            case EasingType.InBounce:
                return 1 - OutBounce(1 - t);

            case EasingType.OutBounce:
                return OutBounce(t);

            case EasingType.InOutBounce:
                return t < 0.5
                    ? (1 - OutBounce(1 - 2 * t)) / 2
                    : (1 + OutBounce(2 * t - 1)) / 2;

            // Flash
            case EasingType.Flash:
                return (Mathf.Sin(t * Mathf.Pi * 10) > 0) ? 1 : 0;

            case EasingType.InFlash:
                return t * (Mathf.Sin(t * Mathf.Pi * 8) * 0.5f + 0.5f);

            case EasingType.OutFlash:
                return (1 - t) * (Mathf.Sin(t * Mathf.Pi * 8) * 0.5f + 0.5f) + t;

            case EasingType.InOutFlash:
                float pulse = Mathf.Sin(t * Mathf.Pi * 8) * 0.5f + 0.5f;
                return t < 0.5
                    ? t * pulse * 2
                    : 1 - (1 - t) * pulse * 2;

            default:
                return t; // Return Linear
        }
    }

    private float OutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
            return n1 * t * t;
        else if (t < 2 / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        else if (t < 2.5f / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        else
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

}
