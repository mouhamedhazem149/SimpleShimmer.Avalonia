using Avalonia.Xaml.Interactivity;

namespace SimpleShimmer;

public sealed class ShimmeringBehavior : Behavior<Control>
{
    private ShimmeringHelper? _shimmeringHelper;
    private IDisposable? _isActiveListener;
    private IDisposable? _colorListener;
    private IDisposable? _brushListener;
    private IDisposable? _durationListener;

    protected override void OnAttached()
    {
        base.OnAttached();
        _shimmeringHelper = new(AssociatedObject!)
        {
            IsActive = IsActive,
            Color = Color,
            Duration = Duration
        };
        
        _isActiveListener = this.GetPropertyChangedObservable(IsActiveProperty).Subscribe(OnIsActiveChanged);
        _colorListener = this.GetPropertyChangedObservable(ColorProperty).Subscribe(OnColorChanged);
        _brushListener = this.GetPropertyChangedObservable(BrushProperty).Subscribe(OnBrushChanged);
        _durationListener = this.GetPropertyChangedObservable(DurationProperty).Subscribe(OnDurationChanged);
    }

    protected override void OnDetaching()
    {
        _shimmeringHelper = null;
        
        _isActiveListener?.Dispose();
        _colorListener?.Dispose();
        _brushListener?.Dispose();
        _durationListener?.Dispose();

        base.OnDetaching();
    }

    #region IsActive
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<ShimmeringBehavior, bool>(nameof(IsActive));

    private void OnIsActiveChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool _isActive || _shimmeringHelper is null)
        {
            return;
        }

        _shimmeringHelper.IsActive = _isActive;
    }
    #endregion

    #region Color
    public Color? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly StyledProperty<Color?> ColorProperty = AvaloniaProperty.Register<ShimmeringBehavior, Color?>(nameof(Color));

    private void OnColorChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_shimmeringHelper is null)
        {
            return;
        }

        _shimmeringHelper.Color = e.NewValue as Color?;
    }
    #endregion

    #region Brush
    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public static readonly StyledProperty<IBrush?> BrushProperty = AvaloniaProperty.Register<ShimmeringBehavior, IBrush?>(nameof(Brush));

    private void OnBrushChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_shimmeringHelper is null)
        {
            return;
        }

        _shimmeringHelper.CustomBrush = e.NewValue as Brush;
    }
    #endregion

    #region Duration
    public TimeSpan Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<ShimmeringBehavior, TimeSpan>(
        nameof(Duration),
        TimeSpan.FromSeconds(1));

    private void OnDurationChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (_shimmeringHelper is null || e.NewValue is not TimeSpan newDuration)
        {
            return;
        }

        _shimmeringHelper.Duration = newDuration;
    }
    #endregion
}