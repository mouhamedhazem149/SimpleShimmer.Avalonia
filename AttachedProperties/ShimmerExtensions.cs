namespace SimpleShimmer;

public class ShimmerExtensions
{
    static ShimmerExtensions()
    {
        IsActiveProperty.Changed.AddClassHandler<Control>(OnIsActiveChanged);
        ColorProperty.Changed.AddClassHandler<Control>(OnColorChanged);
        BrushProperty.Changed.AddClassHandler<Control>(OnBrushChanged);
        DurationProperty.Changed.AddClassHandler<Control>(OnDurationChanged);
    }

    #region IsActive

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.RegisterAttached<ShimmerExtensions, Control, bool>(
            "IsActive",
            defaultBindingMode: BindingMode.OneWay);

    public static bool GetIsActive(Control element) => element.GetValue(IsActiveProperty);

    public static void SetIsActive(Control element, bool value) => element.SetValue(IsActiveProperty, value);

    private static void OnIsActiveChanged(Control element, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not bool _isActive)
        {
            return;
        }

        var helper = GetShimmeringHelper(element);
        helper.IsActive = _isActive;
    }

    #endregion

    #region Color

    public static readonly StyledProperty<Color?> ColorProperty =
        AvaloniaProperty.RegisterAttached<ShimmerExtensions, Control, Color?>(
            "Color",
            defaultBindingMode: BindingMode.OneWay);

    public static Color? GetColor(Control element) => element.GetValue(ColorProperty);

    public static void SetColor(Control element, Color? value) => element.SetValue(ColorProperty, value);

    private static void OnColorChanged(Control element, AvaloniaPropertyChangedEventArgs args)
    {
        var helper = GetShimmeringHelper(element);
        helper.Color = args.NewValue as Color?;
    }

    #endregion

    #region IBrush

    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.RegisterAttached<ShimmerExtensions, Control, IBrush?>(
            "Brush",
            defaultBindingMode: BindingMode.OneWay);

    public static IBrush? GetBrush(Control element) => element.GetValue(BrushProperty);

    public static void SetBrush(Control element, IBrush? value) => element.SetValue(BrushProperty, value);

    private static void OnBrushChanged(Control element, AvaloniaPropertyChangedEventArgs args)
    {
        var helper = GetShimmeringHelper(element);
        helper.CustomBrush = args.NewValue as IBrush;
    }

    #endregion

    #region Duration

    public static readonly StyledProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<ShimmerExtensions, Control, TimeSpan>(
            "Duration",
            TimeSpan.FromSeconds(1),
            defaultBindingMode: BindingMode.OneWay);

    public static TimeSpan? GetDuration(Control element) => element.GetValue(DurationProperty);

    public static void SetDuration(Control element, TimeSpan value) => element.SetValue(DurationProperty, value);

    private static void OnDurationChanged(Control element, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is not TimeSpan _newDuration)
        {
            return;
        }

        var helper = GetShimmeringHelper(element);
        helper.Duration = _newDuration;
    }

    #endregion

    #region ShimmeringHelper

    private static readonly StyledProperty<ShimmeringHelper?> ShimmeringHelperProperty =
        AvaloniaProperty.RegisterAttached<ShimmerExtensions, Control, ShimmeringHelper?>(
            "ShimmeringHelper");

    private static ShimmeringHelper GetShimmeringHelper(Control element)
    {
        var _currentValue = element.GetValue(ShimmeringHelperProperty);

        if (_currentValue is not null)
        {
            return _currentValue;
        }

        _currentValue = new ShimmeringHelper(element);
        SetShimmeringHelper(element, _currentValue);

        return _currentValue;
    }

    private static void SetShimmeringHelper(Control element, ShimmeringHelper value)
    {
        element.SetValue(ShimmeringHelperProperty, value);
    }

    #endregion
}