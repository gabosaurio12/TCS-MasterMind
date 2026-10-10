using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MasterMind_Client.Controls
{
    /// <summary>
    /// Dibuja un sprite 9-slice (marco metálico + relleno).
    /// WPF .NET Framework no tiene 9-slice nativo y los assets del proyecto se usan
    /// en tamaños muy distintos, por eso existe este control.
    /// Cada región es un recorte del sprite estirado a su celda, así el marco nunca
    /// se deforma por mucho que el control cambie de tamaño.
    /// El contenido va encima: este control solo dibuja el marco.
    /// </summary>
    public partial class NineSlice : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(ImageSource),
                typeof(NineSlice),
                new PropertyMetadata(null, OnSliceGeometryChanged));

        public static readonly DependencyProperty SliceProperty =
            DependencyProperty.Register(
                nameof(Slice),
                typeof(Thickness),
                typeof(NineSlice),
                new PropertyMetadata(new Thickness(24), OnSliceGeometryChanged));

        /// <summary>
        /// Sprite a dibujar. Se expone también como propiedad adjunta para que las
        /// plantillas de Button puedan elegir el color con un único Setter de estilo.
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

        /// <summary>Sprite que se dibuja de fondo.</summary>
        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>Grosor de cada borde, medido en píxeles ya escalados a pantalla.</summary>
        public Thickness Slice
        {
            get { return (Thickness)GetValue(SliceProperty); }
            set { SetValue(SliceProperty, value); }
        }

        public NineSlice()
        {
            InitializeComponent();
        }

        private static void OnSliceGeometryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NineSlice)sender).UpdateRegions();
        }

        private void UpdateRegions()
        {
            this.FillRect.Fill = null;
            this.TopLeftRect.Fill = null;
            this.TopRect.Fill = null;
            this.TopRightRect.Fill = null;
            this.LeftRect.Fill = null;
            this.RightRect.Fill = null;
            this.BottomLeftRect.Fill = null;
            this.BottomRect.Fill = null;
            this.BottomRightRect.Fill = null;

            var spriteWidth = Source == null ? 0 : Source.Width;
            var spriteHeight = Source == null ? 0 : Source.Height;
            if (spriteWidth <= 0 || spriteHeight <= 0)
            {
                return;
            }

            var left = Math.Min(Slice.Left, spriteWidth / 2);
            var right = Math.Min(Slice.Right, spriteWidth / 2);
            var top = Math.Min(Slice.Top, spriteHeight / 2);
            var bottom = Math.Min(Slice.Bottom, spriteHeight / 2);
            var middleWidth = spriteWidth - left - right;
            var middleHeight = spriteHeight - top - bottom;

            this.FillRect.Fill = BuildRegionBrush(new Rect(left, top, middleWidth, middleHeight));
            this.TopLeftRect.Fill = BuildRegionBrush(new Rect(0, 0, left, top));
            this.TopRightRect.Fill = BuildRegionBrush(new Rect(spriteWidth - right, 0, right, top));
            this.BottomLeftRect.Fill = BuildRegionBrush(new Rect(0, spriteHeight - bottom, left, bottom));
            this.BottomRightRect.Fill = BuildRegionBrush(
                new Rect(spriteWidth - right, spriteHeight - bottom, right, bottom));
            this.TopRect.Fill = BuildRegionBrush(new Rect(left, 0, middleWidth, top));
            this.BottomRect.Fill = BuildRegionBrush(new Rect(left, spriteHeight - bottom, middleWidth, bottom));
            this.LeftRect.Fill = BuildRegionBrush(new Rect(0, top, left, middleHeight));
            this.RightRect.Fill = BuildRegionBrush(new Rect(spriteWidth - right, top, right, middleHeight));
        }

        /// <summary>
        /// Recorta el sprite con Viewbox en fracciones del origen y lo estira a la celda.
        /// El Viewbox va en fracciones, y no en píxeles, para que el grosor de Slice no
        /// dependa de la resolución con la que se exportó el asset.
        /// </summary>
        private Brush BuildRegionBrush(Rect region)
        {
            return new ImageBrush
            {
                ImageSource = Source,
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                ViewboxUnits = BrushMappingMode.RelativeToSource,
                Viewbox = new Rect(
                    region.X / Source.Width,
                    region.Y / Source.Height,
                    region.Width / Source.Width,
                    region.Height / Source.Height),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewport = new Rect(0, 0, 1, 1)
            };
        }
    }
}