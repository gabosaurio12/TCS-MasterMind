using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MasterMind_Client.Controls
{
    /// <summary>
    /// Dibuja un sprite 9-slice (marco metálico + relleno).
    /// WPF .NET Framework no tiene 9-slice nativo y los assets del proyecto se usan
    /// en tamaños muy distintos, por eso existe este control.
    /// Cada región es un recorte del sprite estirado a su celda, así el marco nunca
    /// se deforma por mucho que el control cambie de tamaño.
    /// El contenido va encima: este control solo dibuja el marco.
    ///
    /// Se declara en código y no en XAML a propósito: un tipo definido por XAML al que
    /// apunta un ResourceDictionary del mismo proyecto depende del orden de
    /// MarkupCompilePass1/2 y falla con "el nombre X no existe en el espacio de
    /// nombres". Como tipo CLR se resuelve siempre.
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

        private double spriteWidth;
        private double spriteHeight;
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
        }

        /// <summary>
        /// Las filas y columnas Auto las dimensionan las celdas de esquina y borde,
        /// que llevan su grosor a pelo; la del centro se queda con lo que sobra.
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
            ((NineSlice)sender).UpdateRegions();
        }

        private void UpdateRegions()
        {
            ClearRegions();

            this.spriteWidth = Source == null ? 0 : Source.Width;
            this.spriteHeight = Source == null ? 0 : Source.Height;
            if (this.spriteWidth <= 0 || this.spriteHeight <= 0)
            {
                return;
            }

            ReadSlice();
            ApplyRegionSizes();
            ApplyRegionFills();
        }

        /// <summary>
        /// Un slice mayor que la mitad del sprite dejaría una región central de tamaño
        /// negativo, así que se recorta contra el propio sprite.
        /// </summary>
        private void ReadSlice()
        {
            var maxWidth = this.spriteWidth / 2;
            var maxHeight = this.spriteHeight / 2;
            this.left = Math.Min(Slice.Left, maxWidth);
            this.right = Math.Min(Slice.Right, maxWidth);
            this.top = Math.Min(Slice.Top, maxHeight);
            this.bottom = Math.Min(Slice.Bottom, maxHeight);
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

        private void ApplyRegionFills()
        {
            var middleWidth = this.spriteWidth - this.left - this.right;
            var middleHeight = this.spriteHeight - this.top - this.bottom;
            var endWidth = this.spriteWidth - this.right;
            var endHeight = this.spriteHeight - this.bottom;

            this.topLeftRect.Fill = BuildRegionBrush(new Rect(0, 0, this.left, this.top));
            this.topRect.Fill = BuildRegionBrush(new Rect(this.left, 0, middleWidth, this.top));
            this.topRightRect.Fill = BuildRegionBrush(new Rect(endWidth, 0, this.right, this.top));
            this.leftRect.Fill = BuildRegionBrush(new Rect(0, this.top, this.left, middleHeight));
            this.fillRect.Fill = BuildRegionBrush(new Rect(this.left, this.top, middleWidth, middleHeight));
            this.rightRect.Fill = BuildRegionBrush(new Rect(endWidth, this.top, this.right, middleHeight));
            this.bottomLeftRect.Fill = BuildRegionBrush(new Rect(0, endHeight, this.left, this.bottom));
            this.bottomRect.Fill = BuildRegionBrush(new Rect(this.left, endHeight, middleWidth, this.bottom));
            this.bottomRightRect.Fill = BuildRegionBrush(new Rect(endWidth, endHeight, this.right, this.bottom));
        }

        private void ClearRegions()
        {
            foreach (Rectangle region in this.layout.Children)
            {
                region.Fill = null;
            }
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
                    region.X / this.spriteWidth,
                    region.Y / this.spriteHeight,
                    region.Width / this.spriteWidth,
                    region.Height / this.spriteHeight),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
                Viewport = new Rect(0, 0, 1, 1)
            };
        }
    }
}
