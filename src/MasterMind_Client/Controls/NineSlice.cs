using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MasterMind_Client.Controls
{
    /// <summary>
    /// Draws a 9-slice sprite (metal frame plus fill).
    /// WPF .NET Framework has no built-in 9-slice support and the project assets are
    /// used at very different sizes, hence this control. Every region is a crop of the
    /// sprite stretched to its cell, and the frame is sliced by a fraction of the sprite
    /// rather than a pixel size, so the frame scales with the control and its seams always
    /// land on the real edge of the frame. Content is drawn on top: this control only paints
    /// the frame.
    ///
    /// Declared in code rather than XAML on purpose: a XAML-defined type referenced by a
    /// ResourceDictionary in the same project depends on MarkupCompilePass1/2 ordering and
    /// fails with "the name X does not exist in the namespace". As a CLR type it always
    /// resolves.
    /// </summary>
    public class NineSlice : UserControl
    {
        private readonly Grid layout = new Grid();
        private readonly Rectangle topLeftRect = new Rectangle();
        private readonly Rectangle topRect = new Rectangle();
        private readonly Rectangle topRightRect = new Rectangle();
        private readonly Rectangle leftRect = new Rectangle();
        private readonly Rectangle fillRect = new Rectangle();
        private readonly Rectangle rightRect = new Rectangle();
        private readonly Rectangle bottomLeftRect = new Rectangle();
        private readonly Rectangle bottomRect = new Rectangle();
        private readonly Rectangle bottomRightRect = new Rectangle();

        private const double MaxRatio = 0.5;

        private double leftRatio;
        private double rightRatio;
        private double topRatio;
        private double bottomRatio;
        private double left;
        private double right;
        private double top;
        private double bottom;

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(ImageSource),
                typeof(NineSlice),
                new PropertyMetadata(null, OnSliceGeometryChanged));

        /// <summary>
        /// Slice taken by the frame on each edge, attached so the Button templates can
        /// TemplateBind it and every sprite variant can pick its own cut with a single
        /// style Setter, the same way <see cref="SliceImageProperty"/> picks the sprite.
        /// </summary>
        public static readonly DependencyProperty SliceRatioProperty =
            DependencyProperty.RegisterAttached(
                "SliceRatio",
                typeof(Thickness),
                typeof(NineSlice),
                new PropertyMetadata(new Thickness(0.08, 0.126, 0.08, 0.126), OnSliceGeometryChanged));

        public static Thickness GetSliceRatio(DependencyObject element)
        {
            return (Thickness)element.GetValue(SliceRatioProperty);
        }

        public static void SetSliceRatio(DependencyObject element, Thickness value)
        {
            element.SetValue(SliceRatioProperty, value);
        }

        /// <summary>
        /// Sprite to draw. Also exposed as an attached property so the Button templates
        /// can pick the color with a single style Setter.
        /// </summary>
        public static readonly DependencyProperty SliceImageProperty =
            DependencyProperty.RegisterAttached(
                "SliceImage",
                typeof(ImageSource),
                typeof(NineSlice),
                new PropertyMetadata(null));

        public static ImageSource GetSliceImage(DependencyObject element)
        {
            return (ImageSource)element.GetValue(SliceImageProperty);
        }

        public static void SetSliceImage(DependencyObject element, ImageSource value)
        {
            element.SetValue(SliceImageProperty, value);
        }

        /// <summary>Sprite painted in the background.</summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Share of the sprite taken by the frame on each edge, from 0 to 1. It is a fraction
        /// rather than a pixel size so the frame scales with the control and the seams always
        /// land on the real edge of the frame.
        /// </summary>
        public Thickness SliceRatio
        {
            get { return GetSliceRatio(this); }
            set { SetSliceRatio(this, value); }
        }

        public NineSlice()
        {
            ConfigureRowsAndColumns();
            Place(this.topLeftRect, 0, 0);
            Place(this.topRect, 0, 1);
            Place(this.topRightRect, 0, 2);
            Place(this.leftRect, 1, 0);
            Place(this.fillRect, 1, 1);
            Place(this.rightRect, 1, 2);
            Place(this.bottomLeftRect, 2, 0);
            Place(this.bottomRect, 2, 1);
            Place(this.bottomRightRect, 2, 2);

            Content = this.layout;
            ClipToBounds = true;
            SizeChanged += HandleSizeChanged;
        }

        /// <summary>
        /// The Auto rows and columns are sized by the corner and edge cells, which carry
        /// their thickness as explicit size; the center cell takes whatever is left.
        /// </summary>
        private void ConfigureRowsAndColumns()
        {
            this.layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            this.layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        private void Place(UIElement element, int row, int column)
        {
            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            this.layout.Children.Add(element);
        }

        private static void OnSliceGeometryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is NineSlice nineSlice)
            {
                nineSlice.UpdateRegions();
            }
        }

        /// <summary>
        /// The cells are sized in the control's own units, so the frame has to be laid out
        /// again whenever the control is resized. ActualWidth and ActualHeight are already
        /// up to date when SizeChanged is raised.
        /// </summary>
        private void HandleSizeChanged(object sender, SizeChangedEventArgs args)
        {
            UpdateRegions();
        }

        private void UpdateRegions()
        {
            ClearRegions();

            if (Source == null || ActualWidth <= 0 || ActualHeight <= 0)
            {
                return;
            }

            ReadSlice();
            ApplyRegionSizes();
            ApplyRegionFills();
        }

        /// <summary>
        /// A ratio above a half would leave a negative sized center region, so every side is
        /// clamped against a half of the sprite.
        /// </summary>
        private void ReadSlice()
        {
            this.leftRatio = Math.Min(SliceRatio.Left, MaxRatio);
            this.rightRatio = Math.Min(SliceRatio.Right, MaxRatio);
            this.topRatio = Math.Min(SliceRatio.Top, MaxRatio);
            this.bottomRatio = Math.Min(SliceRatio.Bottom, MaxRatio);

            this.left = this.leftRatio * ActualWidth;
            this.right = this.rightRatio * ActualWidth;
            this.top = this.topRatio * ActualHeight;
            this.bottom = this.bottomRatio * ActualHeight;
        }

        private void ApplyRegionSizes()
        {
            this.topLeftRect.Width = this.left;
            this.topLeftRect.Height = this.top;
            this.topRightRect.Width = this.right;
            this.topRightRect.Height = this.top;
            this.bottomLeftRect.Width = this.left;
            this.bottomLeftRect.Height = this.bottom;
            this.bottomRightRect.Width = this.right;
            this.bottomRightRect.Height = this.bottom;
            this.topRect.Height = this.top;
            this.bottomRect.Height = this.bottom;
            this.leftRect.Width = this.left;
            this.rightRect.Width = this.right;
        }

        /// <summary>
        /// Every cell crops the same source band and is drawn with the same fraction of the
        /// sprite, so the side cells and the center cell share one scale. Scaling the band by
        /// a fixed pixel size instead would split the frame between two different scales and
        /// leave the side plates visibly sticking out.
        /// </summary>
        private void ApplyRegionFills()
        {
            var middleWidthRatio = 1 - this.leftRatio - this.rightRatio;
            var middleHeightRatio = 1 - this.topRatio - this.bottomRatio;
            var endWidthRatio = 1 - this.rightRatio;
            var endHeightRatio = 1 - this.bottomRatio;

            this.topLeftRect.Fill = BuildRegionBrush(new Rect(0, 0, this.leftRatio, this.topRatio));
            this.topRect.Fill = BuildRegionBrush(new Rect(this.leftRatio, 0, middleWidthRatio, this.topRatio));
            this.topRightRect.Fill = BuildRegionBrush(new Rect(endWidthRatio, 0, this.rightRatio, this.topRatio));
            this.leftRect.Fill = BuildRegionBrush(new Rect(0, this.topRatio, this.leftRatio, middleHeightRatio));
            this.fillRect.Fill = BuildRegionBrush(
                new Rect(this.leftRatio, this.topRatio, middleWidthRatio, middleHeightRatio));
            this.rightRect.Fill = BuildRegionBrush(
                new Rect(endWidthRatio, this.topRatio, this.rightRatio, middleHeightRatio));
            this.bottomLeftRect.Fill = BuildRegionBrush(new Rect(0, endHeightRatio, this.leftRatio, this.bottomRatio));
            this.bottomRect.Fill = BuildRegionBrush(
                new Rect(this.leftRatio, endHeightRatio, middleWidthRatio, this.bottomRatio));
            this.bottomRightRect.Fill = BuildRegionBrush(
                new Rect(endWidthRatio, endHeightRatio, this.rightRatio, this.bottomRatio));
        }

        private void ClearRegions()
        {
            foreach (Rectangle region in this.layout.Children)
            {
                region.Fill = null;
            }
        }

        /// <summary>
        /// Crops the sprite with the Viewbox and stretches the crop to the cell. The Viewbox
        /// is already a fraction of the sprite, so the frame keeps its proportions whatever
        /// resolution the asset was exported at.
        /// </summary>
        private Brush BuildRegionBrush(Rect region)
        {
            return new ImageBrush
            {
                ImageSource = Source,
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewbox = region,
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewport = new Rect(0, 0, 1, 1)
            };
        }
    }
}
