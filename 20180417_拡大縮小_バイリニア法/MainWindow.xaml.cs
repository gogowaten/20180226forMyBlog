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
using System.Windows.Shapes;
//バイリニア法で画像の拡大縮小(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15464617.html

namespace _20180417_拡大縮小_バイリニア法
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();
            AllowDrop = true;
            Drop += MainWindow_Drop;
            MyButtonOrigin.Click += MyButtonOrigin_Click;
            MyButtonSave.Click += MyButtonSave_Click;
            MyButton1Bilinear2.Click += MyButton1_Click;
            MyButtonBilinear2Color.Click += MyButton2_Click;
            SliderXScale.MouseWheel += SliderRatio_MouseWheel;
            SliderYScale.MouseWheel += SliderRatio_MouseWheel;
            MyButton2x2.Click += MyButton2x2_Click;
        }

        private void MyButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            SaveImage((BitmapSource)MyImage.Source);
        }

        private void MyButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        private void MyButton2x2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = F4今表示している画像をニアレストネイバー法で2倍((BitmapSource)MyImage.Source);
        }

        private void SliderRatio_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider s = (Slider)sender;
            if (e.Delta > 0) { s.Value += s.SmallChange; }
            else { s.Value -= s.SmallChange; }
        }

        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = F2バイリニア法カラー2(OriginBitmap, SliderXScale.Value, SliderYScale.Value);
        }

        private void MyButton1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var b = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Gray8, null, 0);
            MyImage.Source = F1バイリニア法グレースケール(b, SliderXScale.Value, SliderYScale.Value);
        }




        //今表示している画像をニアレストネイバー法で2倍表示
        private BitmapSource F4今表示している画像をニアレストネイバー法で2倍(BitmapSource source)
        {
            if (source.PixelHeight > 1000)
            {
                if (MessageBox.Show("縦ピクセル2000以上の大きな画像になる、実行する?", "処理実行確認", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    return source;
                }
            }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            int nH = h * 2;
            int nW = w * 2;
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0, p = 0;
            int cc = source.Format.BitsPerPixel / 8;//8bitグレースケールなら1、32bppなら4
            for (int y = 0; y < nH; ++y)
            {
                for (int x = 0; x < nW; ++x)
                {
                    nP = y * nStride + (x * cc);
                    p = (y / 2) * stride + ((x / 2) * cc);
                    for (int i = 0; i < cc; ++i)
                    {
                        nPixels[nP + i] = pixels[p + i];
                    }

                }
            }

            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }
        //        画像処理 - HexeRein
        //http://www7a.biglobe.ne.jp/~fairytale/article/program/graphics.html#bi_linear


        //グレースケール限定,pixelformat.gray8専用
        private BitmapSource F1バイリニア法グレースケール(BitmapSource source, double xScale, double yScale)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            //変換後のサイズは四捨五入
            int nH = (int)Math.Round(h * yScale, MidpointRounding.AwayFromZero);
            int nW = (int)Math.Round(w * xScale, MidpointRounding.AwayFromZero);
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double rXScale = (w - 1) / (nW - 1.0f);
            double rYScale = (h - 1) / (nH - 1.0f);
            int x0, x1, y0, y1;
            double xx, yy;
            byte topLeft, topRight, botLeft, botRight;
            double topX, botX, newValue;
            for (int y = 0; y < nH; ++y)
            {
                motoY = y * rYScale;//元画像のy座標
                yy = motoY % 1;//y座標の小数点部分
                y0 = (int)(motoY - yy);//上のy座標
                y1 = y0 != h - 1 ? y0 + 1 : y0;//下のy座標、最下段じゃなければ一段下、最下段なら上と同じにしておく

                for (int x = 0; x < nW; ++x)
                {
                    motoX = x * rXScale;//元画像のx座標
                    xx = motoX % 1;//x座標の小数点部分
                    x0 = (int)(motoX - xx);//左のx座標（元画像）
                    x1 = x0 != w - 1 ? x0 + 1 : x0;//右のx座標、右端じゃなければ右隣、右端なら左と同じにしておく

                    topLeft = pixels[y0 * stride + (x0)];//左上の値（元画像）
                    topRight = pixels[y0 * stride + (x1)];//右上
                    botLeft = pixels[y1 * stride + (x0)];//左下
                    botRight = pixels[y1 * stride + (x1)];//右下

                    topX = topLeft * (1 - xx) + topRight * xx;//上の補間した値
                    botX = botLeft * (1 - xx) + botRight * xx;//下の補間した値

                    newValue = topX * (1 - yy) + botX * yy;//上下左右で補間した値
                    newValue = Math.Round(newValue, MidpointRounding.AwayFromZero);//四捨五入

                    nP = y * nStride + (x * 1);
                    nPixels[nP] = (byte)newValue;
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }


        //pixelformat.pbgra32限定
        private BitmapSource F2バイリニア法カラー2(BitmapSource source, double xScale, double yScale)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            var pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            //変換後のサイズは四捨五入
            int nH = (int)Math.Round(h * yScale, MidpointRounding.AwayFromZero);
            int nW = (int)Math.Round(w * xScale, MidpointRounding.AwayFromZero);
            var nWb = new WriteableBitmap(nW, nH, 96, 96, source.Format, source.Palette);
            int nStride = nWb.BackBufferStride;
            var nPixels = new byte[nH * nStride];
            long nP = 0;
            double motoX, motoY;
            double rXScale = (w - 1) / (nW - 1.0f);
            double rYScale = (h - 1) / (nH - 1.0f);
            int x0, x1, y0, y1;
            double xx, yy;
            byte topLeft, topRight, botLeft, botRight;
            double topX, botX, newValue;
            for (int y = 0; y < nH; ++y)
            {
                motoY = y * rYScale;
                yy = motoY % 1;//y座標の小数点部分
                y0 = (int)(motoY - yy);//上のy座標
                y1 = y0 != h - 1 ? y0 + 1 : y0;//下のy座標、最下段じゃなければ一段下、最下段なら上と同じにしておく
                for (int x = 0; x < nW; ++x)
                {
                    //元画像の座標                    
                    motoX = x * rXScale;
                    xx = motoX % 1;//x座標の小数点部分
                    x0 = (int)(motoX - xx);//左のx座標（元画像）
                    x1 = x0 != w - 1 ? x0 + 1 : x0;//右のx座標、右端じゃなければ右隣、右端なら左と同じにしておく

                    for (int i = 0; i < 4; ++i)
                    {
                        topLeft = pixels[y0 * stride + (x0 * 4) + i];//左上の値（元画像）
                        topRight = pixels[y0 * stride + (x1 * 4) + i];//右上
                        botLeft = pixels[y1 * stride + (x0 * 4) + i];//左下
                        botRight = pixels[y1 * stride + (x1 * 4) + i];//右下

                        topX = topLeft * (1 - xx) + topRight * xx;//上の補間した値
                        botX = botLeft * (1 - xx) + botRight * xx;//下の補間した値

                        newValue = topX * (1 - yy) + botX * yy;//上下左右で補間した値
                        newValue = Math.Round(newValue, MidpointRounding.AwayFromZero);//四捨五入

                        nP = y * nStride + (x * 4 + i);
                        nPixels[nP] = (byte)newValue;
                    }
                }
            }
            nWb.WritePixels(new Int32Rect(0, 0, nW, nH), nPixels, nStride, 0);
            return nWb;
        }


        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            //OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Gray8, 96, 96);
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Pbgra32, 96, 96);
            if (OriginBitmap == null)
            {
                MessageBox.Show("not Image");
            }
            else
            {
                MyImage.Source = OriginBitmap;
                ImageFileFullPath = filePath[0];
            }
        }

        private void SaveImage(BitmapSource source)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "*.png|*.png|*.bmp|*.bmp|*.tiff|*.tiff|*.jpg|*.jpg";
            saveFileDialog.AddExtension = true;
            saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(ImageFileFullPath) + "_";
            saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(ImageFileFullPath);
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
                else if (saveFileDialog.FilterIndex == 4)
                {
                    var je = new JpegBitmapEncoder();
                    je.QualityLevel = 96;
                    encoder = je;
                }

                encoder.Frames.Add(BitmapFrame.Create(source));
                using (var fs = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    encoder.Save(fs);
                }
            }
        }



        /// <summary>
        ///  ファイルパスとPixelFormatを指定してBitmapSourceを取得、dpiの変更は任意
        /// </summary>
        /// <param name="filePath">画像ファイルのフルパス</param>
        /// <param name="pixelFormat">PixelFormatsの中からどれかを指定</param>
        /// <param name="dpiX">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <param name="dpiY">無指定なら画像ファイルで指定されているdpiになる</param>
        /// <returns></returns>
        private BitmapSource GetBitmapSourceWithChangePixelFormat2(
            string filePath, PixelFormat pixelFormat, double dpiX = 0, double dpiY = 0)
        {
            BitmapSource source = null;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var bf = BitmapFrame.Create(fs);
                    var convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                    int w = convertedBitmap.PixelWidth;
                    int h = convertedBitmap.PixelHeight;
                    int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                    byte[] pixels = new byte[h * stride];
                    convertedBitmap.CopyPixels(pixels, stride, 0);
                    //dpi指定がなければ元の画像と同じdpiにする
                    if (dpiX == 0) { dpiX = bf.DpiX; }
                    if (dpiY == 0) { dpiY = bf.DpiY; }
                    //dpiを指定してBitmapSource作成
                    source = BitmapSource.Create(
                        w, h, dpiX, dpiY,
                        convertedBitmap.Format,
                        convertedBitmap.Palette, pixels, stride);
                };
            }
            catch (Exception)
            {

            }

            return source;
        }
    }
}