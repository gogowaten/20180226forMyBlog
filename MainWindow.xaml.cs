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
using System.IO;


namespace _20180224_単純減色
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;


            ButtonOrigin.Click += ButtonOrigin_Click;

            ButtonTest1.Click += ButtonTest1_Click;
            ButtonTest2.Click += ButtonTest2_Click;
            ButtonTest3.Click += ButtonTest3_Click;

            ButtonTest11.Click += ButtonTest1x_Click;
            ButtonTest12.Click += ButtonTest1x_Click;
            ButtonTest13.Click += ButtonTest1x_Click;
            ButtonTest14.Click += ButtonTest1x_Click;
            ButtonTest15.Click += ButtonTest1x_Click;
            ButtonTest16.Click += ButtonTest1x_Click;
            ButtonTest17.Click += ButtonTest1x_Click;

            ButtonTest21.Click += ButtonTest2x_Click;
            ButtonTest22.Click += ButtonTest2x_Click;
            ButtonTest23.Click += ButtonTest2x_Click;
            ButtonTest24.Click += ButtonTest2x_Click;
            ButtonTest25.Click += ButtonTest2x_Click;
            ButtonTest26.Click += ButtonTest2x_Click;
            ButtonTest27.Click += ButtonTest2x_Click;

            ButtonTest31.Click += ButtonTest2x_Click;
            ButtonTest32.Click += ButtonTest2x_Click;
            ButtonTest33.Click += ButtonTest2x_Click;
            ButtonTest34.Click += ButtonTest2x_Click;
            ButtonTest35.Click += ButtonTest2x_Click;
            ButtonTest36.Click += ButtonTest2x_Click;
            ButtonTest37.Click += ButtonTest2x_Click;

            //AddButton();


        }

        //buttonのcontentのtextをintに変換
        private int GetButtonContentInteger(Button button)
        {
            string str = button.Content.ToString();
            return int.Parse(str);
        }
        private void ButtonTest2x_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            int division = GetButtonContentInteger((Button)sender);
            MyImage.Source = GensyokuNumeric2Table(OriginBitmap, division);
            TextBlockColorCount.Text = Math.Pow(division, 3).ToString();
        }

        private void ButtonTest1x_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            int division = GetButtonContentInteger((Button)sender);
            MyImage.Source = GensyokuNumeric(OriginBitmap, division);
            TextBlockColorCount.Text = Math.Pow(division, 3).ToString();
        }

        private void ButtonTest3_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = Gensyoku4division(OriginBitmap);
        }

        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = Gensyoku3division(OriginBitmap);
        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = Gensyoku2division(OriginBitmap);
        }





        //0か255の2階調、2^3=は全8色
        private BitmapSource Gensyoku2division(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        if (pixles[p + i] < 128)
                        {
                            pixles[p + i] = 0;
                        }
                        else
                        {
                            pixles[p + i] = 255;
                        }
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }


        //ポスタリゼーション
        //0，127，255の三段階、RGB3色の3段階で3^3＝の全27色
        private BitmapSource Gensyoku3division(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        if (pixles[p + i] < 85.3)
                        //if (pixles[p + i] < 85)
                        {
                            pixles[p + i] = 0;
                        }
                        else if (pixles[p + i] < 171)
                        {
                            pixles[p + i] = 127;
                        }
                        else
                        {
                            pixles[p + i] = 255;
                        }
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }

        //ポスタリゼーション
        //4段階、
        private BitmapSource Gensyoku4division(BitmapSource source)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        if (pixles[p + i] < 63.8)
                        {
                            pixles[p + i] = 0;
                        }
                        else if (pixles[p + i] < 128)
                        {
                            pixles[p + i] = 85;
                        }
                        else if (pixles[p + i] < 191)
                        {
                            pixles[p + i] = 170;
                        }

                        else
                        {
                            pixles[p + i] = 255;
                        }
                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }

        ////指定の色がどの範囲にあたるのか調べて対応する色を返す
        //private byte GetTest(byte color)
        //{
        //    int division = (int)NumericScrollBar.Value;
        //    double frequency = 255f / division;
        //    int neko = 0;
        //    for (int i = 0; i < division; ++i)
        //    {
        //        if (frequency * i <= color && color <= frequency * (i + 1))
        //        {
        //            neko = i;
        //            break;
        //        }
        //    }
        //    byte newColor = (byte)(255 / (division - 1) * neko);
        //    return newColor;
        //}
        //指定の色がどの範囲にあたるのか調べて対応する色を返す2


        //private byte GetCovertedColor(byte color, int division)
        //{
        //    float frequency = 256f / division;//ここは256であっている、255ではない
        //    int neko = division - 1;
        //    byte v = 255;
        //    for (int i = 1; i < division; ++i)
        //    {
        //        if (color < frequency * (i))
        //        {
        //            //neko = i;
        //            //break;
        //            var aa = 255f / (division - 1);
        //            //return (byte)Math.Round((aa * (i - 1)),MidpointRounding.AwayFromZero);
        //            //return (byte)Math.Floor((aa * (i - 1)));
        //            //return (byte)Math.Ceiling((aa * (i - 1)));
        //            return (byte)(aa * (i - 1));
        //        }
        //    }
        //    frequency = 255f / (division - 1);
        //    byte inu = (byte)(frequency * neko);
        //    var cc = (byte)(frequency * (neko + 1));
        //    //return inu;
        //    return v;
        //}

        private byte GetCovertedColor(byte color, int division)
        {
            float frequency = 256f / division;//ここは256であっている、255ではない            
            byte newColor = 255;
            for (int i = 1; i < division; ++i)
            {
                if (color < frequency * (i))
                {
                    var v = 255f / (division - 1);
                    return (byte)(v * (i - 1));
                }
            }
            return newColor;
        }


        //ポスタリゼーション
        //ピクセルの色ごとに毎回判定する方法、遅い
        private BitmapSource GensyokuNumeric(BitmapSource source, int division)
        {
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    for (int i = 0; i < 3; ++i)
                    {
                        //var neko = GetTest(pixles[p + i]);
                        //pixles[p + i] = GetTest(pixles[p + i]);
                        //var neko = GetCovertedColor(pixles[p + i], division);
                        pixles[p + i] = GetCovertedColor(pixles[p + i], division);

                    }
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }


        //        ポスタリゼーション（階調変更）
        //http://www.sm.rim.or.jp/~shishido/post.html
        //対応表を作成しておいて、それに当てはめて判定、速い
        private BitmapSource GensyokuNumeric2Table(BitmapSource source, int division)
        {
            //int division = (int)NumericScrollBar.Value;//分割数
            //変換対応表取得
            byte[] converter = GetConverterArray(division);

            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixles = new byte[h * stride];
            wb.CopyPixels(pixles, stride, 0);
            long p = 0;

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 4);
                    //対応表に当てはめて色変換
                    pixles[p + 2] = converter[pixles[p + 2]];
                    pixles[p + 1] = converter[pixles[p + 1]];
                    pixles[p + 0] = converter[pixles[p + 0]];
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixles, stride, 0);
            return wb;
        }


        /// <summary>
        /// 変換対応表作成
        /// </summary>
        /// <param name="division">分割数(階調数)</param>
        /// <returns></returns>
        private byte[] GetConverterArray(int division)
        {
            //範囲
            //1範囲の大きさ、周波数、256より255かなあと思ったけどやっぱり256でいいみたい
            float frequency = 256f / division;
            //3分割なら0，85.3333359，170.666672，256になる
            float[] range = new float[division + 1];
            for (int i = 0; i < range.Length; ++i)
            {
                range[i] = i * frequency;
            }

            //指定する値
            //こっちは255であっている、実際に指定する値は上限が255だからこれでいい
            frequency = 255f / (division - 1);
            //3分割なら0，127，255になる
            byte[] color = new byte[division];
            for (int i = 0; i < color.Length; ++i)
            {
                color[i] = (byte)(i * frequency);
            }

            //元の256階調全てに対する変換結果の配列作成、対応表
            //配列のindexが元の色の強さでvalueが変換後の色の強さになる
            byte[] converter = new byte[256];
            int j = 0;
            for (int i = 0; i < 256; ++i)
            {
                //不等号はイコール付き
                if (i >= range[j + 1])
                {
                    j++;
                }
                //3分割なら0から85が0、86から170までが127、171から255が255になる
                converter[i] = color[j];
            }
            return converter;
        }

        private void ButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        //画像ファイルドロップ時
        //PixelFormat.Pbgr32に変換してBitmapSource取得
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
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
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
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
