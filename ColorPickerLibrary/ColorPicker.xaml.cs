using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyHSV;
using System.Windows.Controls.Primitives;
using IntegerUpDown;


namespace ColorPickerLibrary
{
    /// <summary>
    /// ColorPicker.xaml の相互作用ロジック
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        private int ImageSize = 100;//100以外にすると表示が崩れる
        private bool IsHsvChanging = false;
        private bool IsRgbChanging = false;
        private double ThumbSize = 20;
        private Point PointDiff;//SV画像のクリック位置とThumbの位置の差を記録

        #region 依存関係プロパティ

        public Color PickupColor
        {
            get { return (Color)GetValue(PickupColorProperty); }
            set { SetValue(PickupColorProperty, value); }
        }
        public static readonly DependencyProperty PickupColorProperty =
            DependencyProperty.Register(nameof(PickupColor), typeof(Color), typeof(ColorPicker));

        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }
        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register(nameof(R), typeof(byte), typeof(ColorPicker));

        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }
        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register(nameof(G), typeof(byte), typeof(ColorPicker));

        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }
        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register(nameof(B), typeof(byte), typeof(ColorPicker));

        public byte A
        {
            get { return (byte)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }
        public static readonly DependencyProperty AProperty =
            DependencyProperty.Register(nameof(A), typeof(byte), typeof(ColorPicker));


        #endregion


        public ColorPicker()
        {
            InitializeComponent();

            MySetBinting();
            MySetEvents();
            InitializeThumb();
            MyInitialize();

            R = 255;
            G = 0;
            B = 0;
            A = 255;
            //ButtonTest.Click += ButtonTest_Click;
        }


        //デバッグ用
        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            var r = R;
            var color = PickupColor;
            var hv = SliderHue.Value;
            //BorderPickupColorSample.Background = new SolidColorBrush(Colors.LemonChiffon);
        }



        private void MySetEvents()
        {
            UpDownR.MyValueChanged += UpDownRGB_MyValueChanged;
            UpDownG.MyValueChanged += UpDownRGB_MyValueChanged;
            UpDownB.MyValueChanged += UpDownRGB_MyValueChanged;
            UpDownH.MyValueChanged += UpDownHSV_MyValueChanged;
            UpDownS.MyValueChanged += UpDownHSV_MyValueChanged;
            UpDownV.MyValueChanged += UpDownHSV_MyValueChanged;
            ImageSV.MouseLeftButtonDown += ImageSV_MouseLeftButtonDown;
            SliderHue.MouseWheel += SliderHue_MouseWheel;
        }

        private void SliderHue_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) { UpDownH.Value--; }
            else { UpDownH.Value++; }
        }

        //rgb変更時
        private void UpDownRGB_MyValueChanged(object sender, MyValueChangedEventArgs e)
        {
            IsRgbChanging = true;
            if (IsHsvChanging != true)
            {
                var iHsv = HSV.Color2HSV(R, G, B);
                if (UpDownH.Value != (int)iHsv.Hue)
                {
                    UpDownH.Value = (int)iHsv.Hue;
                    SetImageSV(iHsv.Hue);
                }
                UpDownS.Value = (int)(iHsv.Saturation * ImageSize);
                UpDownV.Value = (int)(iHsv.Value * ImageSize);


                Canvas.SetLeft(ThumbPicker, UpDownS.Value - ThumbSize / 2f);
                Canvas.SetTop(ThumbPicker, UpDownV.Value - ThumbSize / 2f);
            }
            IsRgbChanging = false;
        }

        //HSV変更時
        private void UpDownHSV_MyValueChanged(object sender, MyValueChangedEventArgs e)
        {

            IsHsvChanging = true;
            if (IsRgbChanging != true)
            {
                IntegerUpDown.IntegerUpDown ud = (IntegerUpDown.IntegerUpDown)sender;
                if (ud == UpDownH) { SetImageSV(UpDownH.Value); }

                var s = UpDownS.Value / (double)ImageSize;
                var v = UpDownV.Value / (double)ImageSize;
                Canvas.SetLeft(ThumbPicker, s * ImageSize - ThumbSize / 2f);
                Canvas.SetTop(ThumbPicker, v * ImageSize - ThumbSize / 2f);

                var col = HSV.HSV2Color(UpDownH.Value, s, v);
                R = col.R;
                G = col.G;
                B = col.B;

            }
            IsHsvChanging = false;
        }

        private void MySetBinting()
        {


            Binding br = new Binding();
            br.Source = this;
            br.Path = new PropertyPath(RProperty);
            //br.Converter = new ConverterByte2Integer();//要らない
            br.Mode = BindingMode.TwoWay;//必須、ないとバインディングが消滅する
            UpDownR.SetBinding(IntegerUpDown.IntegerUpDown.ValueProperty, br);

            Binding bg = new Binding();
            bg.Source = this;
            bg.Path = new PropertyPath(GProperty);
            //bg.Converter = new ConverterByte2Integer();
            bg.Mode = BindingMode.TwoWay;
            UpDownG.SetBinding(IntegerUpDown.IntegerUpDown.ValueProperty, bg);

            Binding bb = new Binding();
            bb.Source = this;
            bb.Path = new PropertyPath(BProperty);
            //bb.Converter = new ConverterByte2Integer();
            bb.Mode = BindingMode.TwoWay;
            UpDownB.SetBinding(IntegerUpDown.IntegerUpDown.ValueProperty, bb);

            Binding ba = new Binding();
            ba.Source = this;
            ba.Path = new PropertyPath(AProperty);
            ba.Mode = BindingMode.TwoWay;
            UpDownA.SetBinding(IntegerUpDown.IntegerUpDown.ValueProperty, ba);

            MultiBinding mb = new MultiBinding();
            mb.Bindings.Add(br);
            mb.Bindings.Add(bg);
            mb.Bindings.Add(bb);
            mb.Bindings.Add(ba);
            mb.Mode = BindingMode.TwoWay;
            mb.Converter = new ConverterRGB2Color();
            BindingOperations.SetBinding(this, PickupColorProperty, mb);

            Binding bColor = new Binding();
            bColor.Source = this;
            bColor.Path = new PropertyPath(PickupColorProperty);
            bColor.Mode = BindingMode.TwoWay;
            bColor.Converter = new ConverterColor2SolidBrush();
            BorderPickupColorSample.SetBinding(Border.BackgroundProperty, bColor);


            Binding bSlider = new Binding();
            bSlider.Source = UpDownH;
            bSlider.Path = new PropertyPath(IntegerUpDown.IntegerUpDown.ValueProperty);
            bSlider.Mode = BindingMode.TwoWay;
            SliderHue.SetBinding(Slider.ValueProperty, bSlider);

        }


        private void MyInitialize()
        {
            MyGrid.Margin = new Thickness(ThumbSize / 2, ThumbSize / 2, 0, 0);
            ImageHue.Source = GetImageHue(ImageSize / 2, ImageSize);
            ImageHue.Width = ImageHue.Source.Width;

            SliderHue.RenderTransform = new RotateTransform(180);
            SliderHue.RenderTransformOrigin = new Point(0.5, 0.5);
            //SliderHue.Opacity = 0;

            SetImageSV(0);
            SetImageAlphaSource();
            //BorderPickupColorSample.Visibility = Visibility.Hidden;


            UpDownR.LargeChange = 10;
            UpDownG.LargeChange = 10;
            UpDownB.LargeChange = 10;
            UpDownA.LargeChange = 10;
            UpDownH.LargeChange = 10;
            UpDownS.LargeChange = 10;
            UpDownV.LargeChange = 10;
        }



        private void ImageSV_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image i = (Image)sender;
            Point p = e.GetPosition(i);
            //SV画像のクリック位置とThumbの位置の差を記録
            PointDiff = new Point(p.X - UpDownS.Value, p.Y - UpDownV.Value);
            UpDownS.Value = (int)p.X;
            UpDownV.Value = (int)p.Y;

            //            単体テストコードでコントロールのイベントを発生させる - ABCの海岸で
            //http://d.hatena.ne.jp/abcneet/20110620/1308551640
            //Thumbにクリックイベント発生させてそのままThumbのドラッグ移動開始させる
            ThumbPicker.RaiseEvent(e);
        }

        private void InitializeThumb()
        {
            ThumbPicker.Width = ThumbSize;//20
            ThumbPicker.Height = ThumbSize;
            ControlTemplate template = new ControlTemplate(typeof(Thumb));
            template.VisualTree = new FrameworkElementFactory(typeof(Grid), "tempGrid");
            ThumbPicker.Template = template;
            ThumbPicker.ApplyTemplate();

            Grid myGrid = (Grid)ThumbPicker.Template.FindName("tempGrid", ThumbPicker);
            var eBlack = new Ellipse();
            eBlack.Width = ThumbSize;//20
            eBlack.Height = ThumbSize;
            eBlack.Stroke = new SolidColorBrush(Colors.Black);
            eBlack.Fill = new SolidColorBrush(Colors.Transparent);
            var eWhite = new Ellipse();
            eWhite.Width = ThumbSize - 2;//18
            eWhite.Height = ThumbSize - 2;
            eWhite.Stroke = new SolidColorBrush(Colors.White);

            myGrid.Children.Add(eBlack);
            myGrid.Children.Add(eWhite);
            //myGrid.Background = new SolidColorBrush(Colors.Transparent);

            ThumbPicker.DragDelta += ThumbPicker_DragDelta;
            ThumbPicker.PreviewMouseLeftButtonDown += ThumbPicker_PreviewMouseLeftButtonDown;
        }

        private void ThumbPicker_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //SV画像のクリック位置とThumbの位置の差を初期化
            PointDiff = new Point();
        }

        //Thumbドラッグ移動
        private void ThumbPicker_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb t = (Thumb)sender;
            var nx = Canvas.GetLeft(t) + e.HorizontalChange;
            var ny = Canvas.GetTop(t) + e.VerticalChange;
            //SV画像のクリック位置とThumbの位置の差を加味する
            nx += PointDiff.X;
            ny += PointDiff.Y;
            //移動制限、SV画像の範囲内にする
            var lowerLimit = -ThumbSize / 2f;
            var upperLimit = ImageSize + lowerLimit;
            if (nx < lowerLimit) { nx = lowerLimit; }
            else if (nx > upperLimit) { nx = upperLimit; }
            if (ny < lowerLimit) { ny = lowerLimit; }
            else if (ny > upperLimit) { ny = upperLimit; }
            //セット
            Canvas.SetLeft(t, nx);
            Canvas.SetTop(t, ny);
            //彩度、明度
            UpDownS.Value = (int)(nx - lowerLimit);
            UpDownV.Value = (int)(ny - lowerLimit);
        }

        private BitmapSource GetImageHue(int w, int h)
        {
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    Color hue = HSV.HSV2Color(y / (float)h * 360f, 1f, 1f);
                    pixels[p] = hue.R;
                    pixels[p + 1] = hue.G;
                    pixels[p + 2] = hue.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }
        private void SetImageSV(double hue)
        {
            var wb = new WriteableBitmap(this.ImageSize, ImageSize, 96, 96, PixelFormats.Rgb24, null);
            int stride = wb.BackBufferStride;
            var pixels = new byte[ImageSize * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Parallel.For(0, ImageSize, y =>
            {
                ParallelImageSV(p, y, stride, pixels, hue, this.ImageSize, ImageSize);
            });

            wb.WritePixels(new Int32Rect(0, 0, this.ImageSize, ImageSize), pixels, stride, 0);
            ImageSV.Source = wb;
        }
        private void ParallelImageSV(int p, int y, int stride, byte[] pixels, double hue, int w, int h)
        {
            for (int x = 0; x < w; ++x)
            {
                p = y * stride + (x * 3);
                Color svColor = HSV.HSV2Color(hue, x / (double)w, y / (double)h);
                pixels[p] = svColor.R;
                pixels[p + 1] = svColor.G;
                pixels[p + 2] = svColor.B;
            }
        }

        //市松模様画像
        private void SetImageAlphaSource()
        {
            int w = (int)ImageAlpha.Width;
            int h = (int)ImageAlpha.Height;
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr24, null);
            int stride = wb.BackBufferStride;
            var pixels = new byte[ImageSize * stride];
            wb.CopyPixels(pixels, stride, 0);

            byte color = 255;
            int x, y;
            for (int i = 0; i < pixels.Length; i += 3)
            {
                x = (i / (10 * 3)) % 2;
                y = (i / 10 / stride) % 2;
                if ((x == 0 && y == 0) || (x == 1 && y == 1))
                {
                    color = 255;
                }
                else
                {
                    color = 200;
                }
                pixels[i] = color;
                pixels[i + 1] = color;
                pixels[i + 2] = color;
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            ImageAlpha.Source = wb;
        }
    }




    internal class ConverterRGB2Color : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte r = (byte)values[0];
            byte g = (byte)values[1];
            byte b = (byte)values[2];
            byte a = (byte)values[3];
            return Color.FromArgb(a, r, g, b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return new object[] { c.R, c.G, c.B, c.A };
        }
    }

    //要らないみたい
    internal class ConverterByte2Integer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte b = (byte)value;
            return (int)b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)value;
            return (byte)i;
        }
    }

    internal class ConverterColor2SolidBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color;
        }
    }
}
