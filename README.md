# SimpleShimmer
 Shimmering animation helper for Avalonia

# Screenshots
![](https://github.com/mouhamedhazem149/SimpleShimmer.Avalonia/blob/main/images/PanelExample.gif)
![](https://github.com/mouhamedhazem149/SimpleShimmer.Avalonia/blob/main/images/ButtonExample.gif)
![](https://github.com/mouhamedhazem149/SimpleShimmer.Avalonia/blob/main/images/TextBlockExample.gif)

# How To Use

The package provides an attached property (e.g. ShimmerExtensions.IsActive) and a behavior (e.g. ShimmeringBehavior.IsActive) in namespace `SimpleShimmer` to help enable shimmering animation. In both cases you will find 4 properties:
   + **IsActive**: enables/ disables the animation.
   + **Duration**: controls duration of the animation (default is 1 second).
   + **Brush**: if you want to provide custom brush.
   + **Color**: you can use this to just provide a color that will be used to create a gradient animation based on this color.

**Note that, in case neither `Brush` or `Color` was provided, a default brush will be created based on background of the associated control (or foreground in case of TextBlock).
