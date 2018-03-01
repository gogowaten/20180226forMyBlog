using System;
using System.Collections.Generic;
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
using System.Windows.Controls.Primitives;
using System.IO;
using MyHSV;


namespace _2080226_HSVRect
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ButtonSaveImage.Click += ButtonSaveImage_Click;
            ScrollBarSizeWidth.ValueChanged += ScrollBarSize_ValueChanged;
            ScrollBarSizeHeight.ValueChanged += ScrollBarSize_ValueChanged;

            ScrollBarHue.ValueChanged += ScrollBarSize_ValueChanged;
            ScrollBarSaturation.ValueChanged += ScrollBarSaturation_ValueChanged;
            ScrollBarValue.ValueChanged += ScrollBarValue_ValueChanged;

            ScrollBarHue.MouseWheel += ScrollBarHue_MouseWheel;
            ScrollBarSaturation.MouseWheel += ScrollBarHue_MouseWheel;
            ScrollBarValue.MouseWheel += ScrollBarHue_MouseWheel;

            ScrollBarSizeWidth.MouseWheel += ScrollBarHue_MouseWheel;
            ScrollBarSizeHeight.MouseWheel += ScrollBarHue_MouseWheel;

            TextBoxHue.MouseWheel += TextBoxHue_MouseWheel;
            TextBoxSaturation.MouseWheel += TextBoxHue_MouseWheel;
            TextBoxValue.MouseWheel += TextBoxHue_MouseWheel;

            TextBoxSize.MouseWheel += TextBoxHue_MouseWheel;
            TextBoxSizeHeight.MouseWheel += TextBoxHue_MouseWheel;

            RadioButtonHue.Checked += RadioButtonHue_Checked;
            RadioButtonSaturaion.Checked += RadioButtonSaturaion_Checked;
            RadioButtonValue.Checked += RadioButtonValue_Checked;

            TextBoxHue.GotFocus += TextBlock_GotFocus;
            TextBoxSize.GotFocus += TextBlock_GotFocus;

            ButtonHueAdd30.Click += ButtonHueAdd30_Click;
            ButtonHueSub30.Click += ButtonHueSub30_Click;
            ButtonSaturationAdd.Click += ButtonSaturationAdd_Click;
            ButtonSaturationSub.Click += ButtonSaturationSub_Click;
            ButtonValueAdd.Click += ButtonValueAdd_Click;
            ButtonValueSub.Click += ButtonValueSub_Click;

            ButtonSizeWidth100.Click += ButtonSize100_Click;
            ButtonSizeWidth256.Click += ButtonSize256_Click;
            ButtonSizeWidth360.Click += ButtonSizeWidth360_Click;
            ButtonSizeHeight100.Click += ButtonSizeHeight100_Click;
            ButtonSizeHeight256.Click += ButtonSizeHeight256_Click;
            ButtonSizeHeight360.Click += ButtonSizeHeight360_Click;

            DrawHue();
            ComboBoxInitialize();
        }

        private void TextBoxHue_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox box = (TextBox)sender;
            Binding binding = BindingOperations.GetBinding(box, TextBox.TextProperty);
            var name = binding.ElementName;
            ScrollBar bar = (ScrollBar)this.FindName(name);
            if (e.Delta > 0)
            {
                bar.Value++;
            }
            else
            {
                bar.Value--;
            }
        }

        private void ScrollBarHue_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollBar bar = (ScrollBar)sender;
            if (e.Delta > 0)
            {
                bar.Value++;
            }
            else
            {
                bar.Value--;
            }
        }

        private void ComboBoxInitialize()
        {
            ComboBoxSavePixelFormat.Items.Add(PixelFormats.Bgr24);
            ComboBoxSavePixelFormat.Items.Add(PixelFormats.Pbgra32);
            ComboBoxSavePixelFormat.Items.Add(PixelFormats.Gray8);
            ComboBoxSavePixelFormat.SelectedIndex = 0;
        }

        private void DrawHue()
        {
            if (RadioButtonHue.IsChecked == true)
            {
                MyImage.Source = GetHueRect(ScrollBarHue.Value, (int)ScrollBarSizeWidth.Value);
            }
        }
        private void DrawSaturation()
        {
            if (RadioButtonSaturaion.IsChecked == true)
            {
                MyImage.Source = GetSaturationRect(
                    (int)ScrollBarSizeWidth.Value,
                    (int)ScrollBarSizeHeight.Value,
                    (float)ScrollBarSaturation.Value / 100f
                    );
            }
        }
        private void DrawValue()
        {
            if (RadioButtonValue.IsChecked == true)
            {
                MyImage.Source = GetValueRect(
                    (int)ScrollBarSizeWidth.Value,
                    (int)ScrollBarSizeHeight.Value,
                    (float)(ScrollBarValue.Value / 100f));
            }
        }

        #region イベント


        private void RadioButtonHue_Checked(object sender, RoutedEventArgs e) => DrawHue();

        private void RadioButtonValue_Checked(object sender, RoutedEventArgs e) => DrawValue();

        private void RadioButtonSaturaion_Checked(object sender, RoutedEventArgs e) => DrawSaturation();
        private void ScrollBarValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => DrawValue();

        private void ScrollBarSaturation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => DrawSaturation();

        private void ButtonSizeHeight360_Click(object sender, RoutedEventArgs e) => ScrollBarSizeHeight.Value = 360f;

        private void ButtonSizeHeight256_Click(object sender, RoutedEventArgs e) => ScrollBarSizeHeight.Value = 256f;

        private void ButtonSizeHeight100_Click(object sender, RoutedEventArgs e) => ScrollBarSizeHeight.Value = 100f;

        private void ButtonSizeWidth360_Click(object sender, RoutedEventArgs e) => ScrollBarSizeWidth.Value = 360f;

        private void ButtonSize256_Click(object sender, RoutedEventArgs e) => ScrollBarSizeWidth.Value = 256f;

        private void ButtonSize100_Click(object sender, RoutedEventArgs e) => ScrollBarSizeWidth.Value = 100f;
        private void ButtonValueSub_Click(object sender, RoutedEventArgs e) => ScrollBarValue.Value -= 10f;

        private void ButtonValueAdd_Click(object sender, RoutedEventArgs e) => ScrollBarValue.Value += 10f;

        private void ButtonSaturationSub_Click(object sender, RoutedEventArgs e) => ScrollBarSaturation.Value -= 10f;

        private void ButtonSaturationAdd_Click(object sender, RoutedEventArgs e) => ScrollBarSaturation.Value += 10f;
        private void ButtonHueSub30_Click(object sender, RoutedEventArgs e) => ScrollBarHue.Value -= 30f;

        private void ButtonHueAdd30_Click(object sender, RoutedEventArgs e) => ScrollBarHue.Value += 30f;

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            this.Dispatcher.InvokeAsync(() => { Task.Delay(10); box.SelectAll(); });
        }

        private void ScrollBarSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DrawHue();
            DrawSaturation();
            DrawValue();
        }

        private void ButtonSaveImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImage((BitmapSource)MyImage.Source);
        }
        #endregion


        //彩度を指定、0から1
        private BitmapSource GetSaturationRect(int width, int height, float saturation)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Color color;
            //高さが1のときは明度を1fいに固定したいので特別
            if (height == 1)
            {
                for (int x = 0; x < width; ++x)
                {
                    p = (x * 4);
                    color = HSV.HSV2Color(
                        (x / (width - 1f)) * 359f,
                        saturation,
                        1f);
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }
            else
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        p = y * stride + (x * 4);
                        color = HSV.HSV2Color(
                            (x / (width - 1f)) * 359f,
                            saturation,
                            y / (double)(height - 1f));
                        pixels[p + 3] = 255;
                        pixels[p + 2] = color.R;
                        pixels[p + 1] = color.G;
                        pixels[p + 0] = color.B;
                    }
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        //明度を指定、0から1
        private BitmapSource GetValueRect(int width, int height, float value)
        {
            var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Color color;
            //高さ1のときは彩度を1fに固定したいので特別
            if (height == 1)
            {
                for (int x = 0; x < width; ++x)
                {
                    p = (x * 4);
                    color = HSV.HSV2Color(
                        (x * 359f) / (width - 1f),
                        1f,
                        value);
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }
            else
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        p = y * stride + (x * 4);
                        color = HSV.HSV2Color(
                            (x * 359f) / (width - 1f),                            
                            y / (double)(height - 1f),
                            value);
                        pixels[p + 3] = 255;
                        pixels[p + 2] = color.R;
                        pixels[p + 1] = color.G;
                        pixels[p + 0] = color.B;
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }



        private BitmapSource GetHueRect(double hue, int size)
        {
            var wb = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Color color;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    color = HSV.HSV2Color(hue, (double)x / (size - 1f), (double)y / (size - 1f));
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
            return wb;
        }

        //明度が上下逆、こっちのほうが一般的みたい？
        private BitmapSource GetHueRectReverse(double hue, int size)
        {
            var wb = new WriteableBitmap(size, size, 96, 96, PixelFormats.Pbgra32, null);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int p = 0;
            Color color;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    color = HSV.HSV2Color(hue, (double)x / (size - 1f), 1f - ((double)y / (size - 1f)));
                    pixels[p + 3] = 255;
                    pixels[p + 2] = color.R;
                    pixels[p + 1] = color.G;
                    pixels[p + 0] = color.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, size, size), pixels, stride, 0);
            return wb;
        }




        private void SaveImage(BitmapSource source)
        {
            source = new FormatConvertedBitmap(source, (PixelFormat)ComboBoxSavePixelFormat.SelectedItem, null, 0);
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = "HSVRect";
            if (saveFileDialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                if (saveFileDialog.FilterIndex == 1)
                {
                    encoder = new PngBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 2)
                {
                    encoder = new BmpBitmapEncoder();
                }
                else if (saveFileDialog.FilterIndex == 3)
                {
                    encoder = new TiffBitmapEncoder();
                }
                encoder.Frames.Add(BitmapFrame.Create(source));

                using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }
    }
}
