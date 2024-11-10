namespace SimpleShimmer;

public sealed class ShimmeringHelper
{
    private readonly Control? _associatedObject;

    private CompositionCustomVisual? _maskBrush;
    private BrushVisual? _gradientVisual;

    private Vector3DKeyFrameAnimation? _animation;
    private LinearEasing? _animationEasing;
    private Compositor? compositor;

    public ShimmeringHelper(Control associatedObject)
    {
        _associatedObject = associatedObject;

        _colorSourceProperty = GetColorSourceProperty();

        if (_associatedObject.IsLoaded)
        {
            OnLoaded(null, null);
        }

        _associatedObject.Loaded += OnLoaded;
        _associatedObject.SizeChanged += OnSizeChanged;
        _associatedObject.Unloaded += OnUnLoaded;
    }

    ~ShimmeringHelper()
    {
        if (_associatedObject is null)
        {
            return;
        }
        
        _associatedObject.Loaded -= OnLoaded;
        _associatedObject.SizeChanged -= OnSizeChanged;
        _associatedObject.Unloaded -= OnUnLoaded;
        OnUnLoaded(null, null);
    }

    private bool _resourcesInitialized;

    private void InitializeResources()
    {
        if (_resourcesInitialized || _associatedObject is null)
        {
            return;
        }

        compositor = ElementComposition.GetElementVisual(_associatedObject)?.Compositor;

        if (compositor is null)
        {
            return;
        }
        
        CreateGradientVisual();
        
        UpdateMaskBrush(compositor);

        UpdateAnimation();

        _resourcesInitialized = true;
    }

    private void CreateGradientVisual()
    {
        _gradientVisual = new();

        var color = Color;

        if (color is not null)
        {
            _gradientVisual.UpdateGradient(color.Value);
            return;
        }

        var brush = CustomBrush;

        if (brush is not null)
        {
            _gradientVisual.UpdateBrush(brush);
            return;
        }

        AddColorListeners();

        color = CalculateAlternativeColor(_associatedObject!);
        _gradientVisual.UpdateGradient(color.Value);
    }

    private void UpdateMaskBrush(Compositor _compositor)
    {
        ArgumentNullException.ThrowIfNull(_gradientVisual);

        _maskBrush = _compositor.CreateCustomVisual(_gradientVisual);
        _maskBrush.ClipToBounds = true;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs? e)
    {
        InitializeResources();
    }

    private void OnUnLoaded(object? sender, RoutedEventArgs? e)
    {
        DisposeResources();
    }

    private void DisposeResources()
    {
        if (!_resourcesInitialized)
        {
            return;
        }

        StopAnimation();

        _gradientVisual = null;

        _maskBrush = null;

        _animation = null;

        _animationEasing = null;
        compositor = null;
        
        _resourcesInitialized = false;
    }

    private void OnSizeChanged(object? sender, RoutedEventArgs e)
    {
        if (CheckResourcesInitialization())
        {
            UpdateAnimation();
        }
    }

    private bool CheckResourcesInitialization()
    {
        if (_resourcesInitialized)
        {
            return true;
        }

        if (_associatedObject?.IsLoaded != true)
        {
            return false;
        }

        InitializeResources();
        return true;
    }

    private void UpdateAnimation()
    {
        StopAnimation();

        HandleCornerRadiusClip(_associatedObject);

        var width = _associatedObject.Bounds.Width;

        var minOffsetX = -width / 2;
        var maxOffsetX = width * 2;

        var OffsetY = 0d;

        if (_associatedObject is TextBlock textBlock)
        {
            var textLayoutWidth = textBlock.TextLayout.Width;

            _maskBrush.Size = new(textLayoutWidth, textBlock.TextLayout.Height);
            var offset = textBlock.TextLayout.HitTestTextPosition(0);

            minOffsetX = offset.X - textLayoutWidth / 2;
            maxOffsetX = offset.X + textLayoutWidth * 2;

            OffsetY = offset.Y;
        }
        else
        {
            _maskBrush.Size = new(width, _associatedObject.Bounds.Height);
        }

        UpdateAnimationKeyFrames(minOffsetX, OffsetY, maxOffsetX);

        if (IsActive)
        {
            StartAnimation();
        }
    }

    private void UpdateAnimationKeyFrames(double minOffsetX, double OffsetY, double maxOffsetX)
    {
        //TODO: animationFrames can't be updated?, just reset the whole thing
        _animation = compositor.CreateVector3DKeyFrameAnimation();
        _animationEasing = new();

        _animation.Duration = Duration;
        _animation.IterationBehavior = AnimationIterationBehavior.Forever;
        
        Vector3D startOffset = new(minOffsetX, OffsetY, 0);
        
        _animation.InsertKeyFrame(0f, new(minOffsetX, OffsetY, 0), _animationEasing);
        //u can just make it width, width * 2 (the difference between animation and actual _maskVisual Width) gives a pause between iterations
        _animation.InsertKeyFrame(1f, new(maxOffsetX, OffsetY, 0), _animationEasing);
    }

    private void StartAnimation()
    {
        ElementComposition.SetElementChildVisual(_associatedObject!, _maskBrush);
        _maskBrush?.StartAnimation("Offset", _animation!);
    }

    private void StopAnimation()
    {
        ArgumentNullException.ThrowIfNull(_maskBrush);
        _maskBrush.Offset = new Vector3D(-1 * _associatedObject.Bounds.Width, -_associatedObject.Bounds.Height,
            double.NegativeInfinity);
        ElementComposition.SetElementChildVisual(_associatedObject!, null);
    }

    #region IsActive

    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;

            if (!_resourcesInitialized)
            {
                return;
            }

            if (_isActive)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }
    }

    #endregion

    #region Color

    private Color? _color;

    public Color? Color
    {
        get => _color;
        set
        {
            if (_color == value)
            {
                return;
            }

            if (_color is null)
            {
                RemoveColorListeners();
            }

            _color = value;
            UpdateGradientColor(value);

            if (value is null)
            {
                AddColorListeners();
            }
        }
    }

    private IBrush? _customBrush;
    public IBrush? CustomBrush
    {
        get => _customBrush;
        set
        {
            if (Equals(_customBrush, value))
            {
                return;
            }

            if (_customBrush is null)
            {
                RemoveColorListeners();
            }

            _customBrush = value;
            UpdateGradientColor(value!);

            if (_customBrush is null)
            {
                AddColorListeners();
            }
        }
    }

    private void UpdateGradientColor(Color? value)
    {
        if (_associatedObject is null || _gradientVisual is null)
        {
            return;
        }

        var newColor = value ?? CalculateAlternativeColor(_associatedObject);

        _gradientVisual.UpdateGradient(newColor);
    }

    private void UpdateGradientColor(IBrush value)
    {
        _gradientVisual?.UpdateBrush(value);
    }

    private AvaloniaProperty<IBrush?>? GetColorSourceProperty() =>
        (_associatedObject switch
        {
            TemplatedControl => TemplatedControl.BackgroundProperty,
            Shape => Shape.FillProperty,
            TextBlock => TextBlock.ForegroundProperty,
            _ => null
        });

    private readonly AvaloniaProperty<IBrush?>? _colorSourceProperty;

    private void RemoveColorListeners()
    {
        if (_associatedObject is null)
        {
            return;
        }

        _associatedObject.PropertyChanged -= OnColorSourceChanged;
    }

    private void AddColorListeners()
    {
        if (_associatedObject is null)
        {
            return;
        }

        _associatedObject.PropertyChanged += OnColorSourceChanged;
    }

    private void OnColorSourceChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == _colorSourceProperty)
        {
            UpdateGradientColor(e.NewValue as Color?);
        }
    }

    static Color CalculateAlternativeColor(Control element)
    {
        var _color = element switch
        {
            TemplatedControl { Background: ISolidColorBrush brush } => brush.Color,
            Shape { Fill: ISolidColorBrush brush } => brush.Color,
            TextBlock { Foreground: ISolidColorBrush brush } => brush.Color,
            _ => Colors.DarkGray
        };

        return GetBrighterColor(_color, 1.3);
    }

    private static Color GetBrighterColor(Color origin, double change)
    {
        var originColor = System.Drawing.Color.FromArgb(origin.A, origin.R, origin.G, origin.B);

        var lum = originColor.GetBrightness() * change;
        var hue = originColor.GetHue();
        var sat = originColor.GetSaturation();
        return GetColorFromHSL(lum, hue, sat);
    }
    
    private static Color GetColorFromHSL(double lum, double hue, double sat, byte alpha = 0xFF)
    {
        var a = sat * Math.Min(lum, 1 - lum);
        return Avalonia.Media.Color.FromArgb(alpha, nFunk(0), nFunk(8), nFunk(4));

        byte nFunk(double n)
        {
            var k = (n + hue / 30) % 12;
            var factor = Math.Max(-1, Math.Min(Math.Min(k - 3, 9 - k), 1));
            return (byte)Math.Round((lum - a * factor) * 255);
        }
    }
    
    private static Color GetColorFromHSL2(double lum, double hue, double sat)
    {
        if (lum == 0)
        {
            return Avalonia.Media.Color.FromArgb(0xFF, 0, 0, 0);
        }

        if (sat == 0)
        {
            var _lum = (byte)(255 * lum);
            return Avalonia.Media.Color.FromArgb(0xFF, _lum, _lum, _lum);
        }

        var temp = lum < 0.5
            ? lum * (1.0 + sat)
            : (lum + sat - (lum * sat));

        var temp1 = 2.0 * lum - temp;

        var r = GetColorComponent(temp, temp1, hue + 1.0 / 3.0);
        var g = GetColorComponent(temp, temp1, hue);
        var b = GetColorComponent(temp, temp1, hue - 1.0 / 3.0);
        return Avalonia.Media.Color.FromArgb(0xFF, r, g, b);
    }

    private static byte GetColorComponent(double temp, double temp1, double adjustedHue)
    {
        if (adjustedHue < 0)
        {
            adjustedHue += 1;
        }

        if (adjustedHue > 1)
        {
            adjustedHue -= 1;
        }

        var component = adjustedHue switch
        {
            < 1 / 6d => temp1 + (temp - temp1) * 6.0 * adjustedHue,
            < 0.5 => temp,
            < 2 / 3d => temp1 + (temp - temp1) * (2.0 / 3.0 - adjustedHue) * 6.0,
            _ => temp1
        };

        return (byte)(component * 255);
    }

    #endregion

    #region Duration

    private TimeSpan _duration = TimeSpan.FromSeconds(1);

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            //timeSpan limitaion of animation Duration by winui
            if (_duration == value || value < TimeSpan.FromMilliseconds(1) || value > TimeSpan.FromDays(24))
            {
                return;
            }

            _duration = value;

            if (_animation is not null)
            {
                _animation.Duration = value;
            }
        }
    }

    #endregion

    private static void HandleCornerRadiusClip(Control element)
    {
        element.Clip = GetClipGeometryForElement(element);
        return;

        static Geometry? GetClipGeometryForElement(Control element) => element switch
        {
            TemplatedControl templatedControl => GetElementCornersClip(templatedControl),
            Shape shape => shape.DefiningGeometry,
            TextBlock textBlock => GetTextClipGeometry(textBlock,
                new(textBlock.TextLayout.HitTestTextPosition(0).X, textBlock.TextLayout.HitTestTextPosition(0).Y)),
            Panel panel => GetGeometryForPanel(panel),
            _ => null
        };

        static Geometry? GetVisualTransformedGeometryForElement(Control element)
        {
            var transform = element.GetVisualRoot() is Visual visual
                ? (element.TransformToVisual(visual) ?? Matrix.Identity)
                : Matrix.Identity;

            var transformedBounds = new Rect(element.Bounds.Size).TransformToAABB(transform);

            return element switch
            {
                Rectangle => new RectangleGeometry(transformedBounds),
                Ellipse => new EllipseGeometry(transformedBounds),
                Line line => new LineGeometry(transform.Transform(new Point(line.StartPoint.X, line.StartPoint.Y)),
                    transform.Transform(new Point(line.EndPoint.X, line.EndPoint.Y))),
                Avalonia.Controls.Shapes.Path path => path.Data?.Clone(),
                TextBlock textBlock => GetTextClipGeometry(textBlock,
                    new(textBlock.BaselineOffset + transformedBounds.X, transformedBounds.Y)),
                TemplatedControl templatedControl => GetVisualTransformedGeometryForTemplatedControl(templatedControl),
                _ => null
            };
        }

        static Geometry? GetVisualTransformedGeometryForTemplatedControl(TemplatedControl control)
        {
            var transform = control.GetVisualRoot() is Visual visual
                ? (control.TransformToVisual(visual) ?? Matrix.Identity)
                : Matrix.Identity;
            
            var transformedBounds = new Rect(control.Bounds.Size).TransformToAABB(transform);

            // Adjust based on the shape type
            return GetRoundedRectGeometry(transformedBounds.Size, control.CornerRadius, transformedBounds.X,
                transformedBounds.Y);
        }

        static Geometry? GetGeometryForPanel(Panel panel) => panel.Children
            .Select(GetVisualTransformedGeometryForElement)
            .Aggregate((geometry1, geometry2) =>
                new CombinedGeometry(
                    GeometryCombineMode.Union,
                    geometry1,
                    geometry2));

        static Geometry? GetTextClipGeometry(TextBlock textBlock, Point origin)
        {
            if (string.IsNullOrWhiteSpace(textBlock.Text))
            {
                return null;
            }

            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentCulture,
                textBlock.FlowDirection,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                textBlock.Foreground);

            return formattedText.BuildGeometry(origin);
        }
    }

    private static Geometry? GetElementCornersClip(TemplatedControl element)
    {
        return GetRoundedRectGeometry(element.Bounds.Size, element.CornerRadius, 0, 0);
    }

    private static Geometry? GetRoundedRectGeometry(Size size, CornerRadius cornerRadius, double xAdjustment,
        double yAdjustment)
    {
        var width = size.Width;
        var height = size.Height;

        if (width == 0 || height == 0) return null; // Avoid zero-sized elements.

        var topLeftRadius = cornerRadius.TopLeft;
        var topRightRadius = cornerRadius.TopRight;
        var bottomRightRadius = cornerRadius.BottomRight;
        var bottomLeftRadius = cornerRadius.BottomLeft;

        var geometry = new StreamGeometry();

        var pathBuilderContext = geometry.Open();
        try
        {
            pathBuilderContext.BeginFigure(GetPoint(topLeftRadius, 0), true);

            // Top edge
            pathBuilderContext.LineTo(GetPoint(width - topRightRadius, 0));
            pathBuilderContext.ArcTo(
                GetPoint(width, topRightRadius),
                new Size(topRightRadius, topRightRadius),
                0,
                false,
                SweepDirection.Clockwise);

            // Right edge
            pathBuilderContext.LineTo(GetPoint(width, height - bottomRightRadius));
            pathBuilderContext.ArcTo(
                GetPoint(width - bottomRightRadius, height),
                new Size(bottomRightRadius, bottomRightRadius),
                0,
                false,
                SweepDirection.Clockwise);

            // Bottom edge
            pathBuilderContext.LineTo(GetPoint(bottomLeftRadius, height));
            pathBuilderContext.ArcTo(GetPoint(0, height - bottomLeftRadius),
                new Size(bottomLeftRadius, bottomLeftRadius),
                0,
                false,
                SweepDirection.Clockwise);

            // Left edge
            pathBuilderContext.LineTo(GetPoint(0, topLeftRadius));
            pathBuilderContext.ArcTo(GetPoint(topLeftRadius, 0),
                new Size(topLeftRadius, topLeftRadius),
                0,
                false,
                SweepDirection.Clockwise);

            pathBuilderContext.EndFigure(true);
        }
        finally
        {
            pathBuilderContext.Dispose();
        }

        return geometry;
        Point GetPoint(double _x, double _y) => new Point(xAdjustment + _x, yAdjustment + _y);
    }

    private sealed class BrushVisual : CompositionCustomVisualHandler
    {
        private IImmutableBrush? _gradient;
        
        public void UpdateBrush(IBrush brush)
        {
            _gradient = brush.ToImmutable();
        }

        public void UpdateGradient(Color color)
        {
            var transparent = Avalonia.Media.Color.FromArgb(0, color.R, color.G, color.B);

            IReadOnlyList<ImmutableGradientStop> stops =
            [
                new(0, transparent),
                new(0.25, color),
                new(0.35, color),
                new(0.5, transparent),
            ];

            _gradient = new ImmutableLinearGradientBrush(stops,
                startPoint: new RelativePoint(0, 0, RelativeUnit.Relative),
                endPoint: new(1, 0, RelativeUnit.Relative));
        }

        public override void OnRender(ImmediateDrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(_gradient,
                null,
                new Rect(0, 0, EffectiveSize.X, EffectiveSize.Y));
        }
    }
}