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
using System.IO;

//指定色で減色＋誤差拡散、減色結果を他のアプリと比較してみた(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15405037.html

namespace _20180308_減色_メディアンカット法と誤差拡散
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;
        Border[] MyBorderPalette;
        List<Color> MyPalette;
        const int MAX_PALETTE_COLOR_COUNT = 20;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            this.AllowDrop = true;
            this.Drop += MainWindow_Drop;

            Button1.Click += Button1_Click;
            Button2.Click += Button2_Click;
            ButtonOrigin.Click += ButtonOrigin_Click;
            ButtonReduceColor.Click += ButtonReduceColor_Click;
            ButtonGosakakusan.Click += ButtonGosakakusan_Click;
            ButtonGosakakusan2.Click += ButtonGosakakusan2_Click;
            ButtonGosakakusan3Limit0_255.Click += ButtonGosakakusan3Limit0_255_Click;
            ButtonGosakakusanNoErrorStack.Click += ButtonGosakakusanNoErrorStack_Click;
            ButtonGosakakusanNoErrorStackLimit0_255.Click += ButtonGosakakusanNoErrorStackLimit0_255_Click;
            ButtonTest1.Click += ButtonTest1_Click;
            ButtonTest2.Click += ButtonTest2_Click;
            ButtonPixelFormatIndexed1.Click += ButtonPixelFormatIndexed1_Click;
            ButtonPixelFormatIndexed2.Click += ButtonPixelFormatIndexed2_Click;
            ButtonPixelFormatIndexed4.Click += ButtonPixelFormatIndexed4_Click;
            ButtonGetImageColors.Click += ButtonGetImageColors_Click;
            AddBorders();//パレットの色表示用のBorder作成
            MyPalette = new List<Color>();


        }

        private void ButtonTest2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            //MyImage.Source = ReduceColor指定色誤差拡散で減色RGB平均分散(OriginBitmap, MyPalette, false);
            MyImage.Source = ReduceColor指定色誤差拡散で減色LimitPalette(OriginBitmap, MyPalette, false);
        }

        private void ButtonTest1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            //MyImage.Source = ReduceColor指定色誤差拡散で減色RGB平均分散(OriginBitmap, MyPalette, true);
            MyImage.Source = ReduceColor指定色誤差拡散で減色LimitPalette(OriginBitmap, MyPalette, true);
        }

        private BitmapSource ReduceColor指定色誤差拡散で減色LimitPalette(BitmapSource source, List<Color> palette, bool errorStack)
        {
            if (palette.Count == 0) { return source; }
            //パレットのRGBの下限上限
            byte[] minRGB = new byte[3];
            byte[] maxRGB = new byte[3];
            for (int i = 0; i < minRGB.Length; ++i) { minRGB[i] = 255; maxRGB[i] = 0; }
            for (int j = 0; j < palette.Count; ++j)
            {
                if (palette[j].R < minRGB[0]) { minRGB[0] = palette[j].R; }
                if (palette[j].G < minRGB[1]) { minRGB[1] = palette[j].G; }
                if (palette[j].B < minRGB[2]) { minRGB[2] = palette[j].B; }
                if (palette[j].R > maxRGB[0]) { maxRGB[0] = palette[j].R; }
                if (palette[j].G > maxRGB[1]) { maxRGB[1] = palette[j].G; }
                if (palette[j].B > maxRGB[2]) { maxRGB[2] = palette[j].B; }
            }

            //WriteableBitmapクラスのCopyPixelsを使って画像の色情報を配列に複製
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            //誤差計算のために小数点を使うのでfloat型の配列に複製したのを使う
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }

            long p = 0, pp = 0;//注目するピクセルの配列のアドレス
            float gosa = 0, pGosa = 0;//誤差、＋誤差
            Color myColor;//パレットから選んだ色
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);//注目するピクセルのアドレス
                    //誤差拡散した色に一番近いパレットの色を取得する
                    //誤差拡散後の色（小数点RGB）
                    double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };
                    //一番近い色取得
                    myColor = GetColorNearPlette(iRGB, palette);
                    //パレットのRGB
                    byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };

                    //RGBごとに誤差拡散、0以下の場合は0、255以上の場合は255に丸める（制限する）
                    for (int i = 0; i < 3; ++i)//RGBの3ループ
                    {
                        gosa = (errorStack) ? (float)(iPixels[p + i] - pRGB[i]) / 16f : (float)(pixels[p + i] - pRGB[i]) / 16f;
                        //右下ピクセルへ誤差拡散
                        pp = p + i + 3;//右下ピクセルアドレス
                        if (pp < pixels.Length && x != w - 1)//右ピクセル
                        {
                            //誤差拡散後の値が0以下なら0、255以上なら255に丸める
                            pGosa = iPixels[pp] + (gosa * 7f);//誤差拡散先の値に誤差を足す                            
                            //iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            iPixels[pp] = (pGosa < minRGB[i]) ? minRGB[i] : (pGosa > maxRGB[i]) ? maxRGB[i] : pGosa;                            
                        }

                        if (y < h - 1)//注目するピクセルが最下段じゃないなら
                        {
                            //真下ピクセルへ誤差拡散
                            pp = p + stride + i;//真下ピクセルアドレス
                            pGosa = iPixels[pp] + (gosa * 5f);
                            //iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            iPixels[pp] = (pGosa < minRGB[i]) ? minRGB[i] : (pGosa > maxRGB[i]) ? maxRGB[i] : pGosa;
                            //左下ピクセルへ誤差拡散
                            if (x != 0)
                            {
                                pp = p + stride + i - 3;//左下ピクセルアドレス
                                pGosa = iPixels[pp] + (gosa * 3f);
                                //iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                                iPixels[pp] = (pGosa < minRGB[i]) ? minRGB[i] : (pGosa > maxRGB[i]) ? maxRGB[i] : pGosa;
                            }
                            //右下ピクセルへ誤差拡散
                            if (x < w - 1)
                            {
                                pp = p + stride + i + 3;//右下ピクセルアドレス
                                pGosa = iPixels[pp] + (gosa * 1f);
                                //iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                                iPixels[pp] = (pGosa < minRGB[i]) ? minRGB[i] : (pGosa > maxRGB[i]) ? maxRGB[i] : pGosa;
                            }
                        }
                    }
                    //色変更
                    pixels[p + 0] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p + 2] = myColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }



        //表示中の画像の色を取得、パレットにコピー
        private void GetUsedColor(BitmapSource source)
        {
            var cb = new FormatConvertedBitmap(source, PixelFormats.Rgb24, source.Palette, 0);
            var wb = new WriteableBitmap(cb);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;// (w * wb.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            int[] colors = new int[256 * 256 * 256];//1700万色
            int numColor;
            //RGBをintに変換してint配列に入れる
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    var r = pixels[p];
                    var g = pixels[p + 1];
                    var b = pixels[p + 2];
                    numColor = pixels[p] + pixels[p + 1] * 256 + pixels[p + 2] * 256 * 256;
                    colors[numColor]++;
                    var nekoR = numColor % 256;
                    var nekoG = numColor / 256 % 256;
                    var nekoB = numColor / 256 / 256;
                }
            }
            //数値が入っているindexだけをlistに追加
            List<int> listColor = new List<int>(256 * 256 * 256);
            for (int i = 0; i < colors.Length; ++i)
            {
                if (colors[i] != 0) { listColor.Add(i); }
            }
            //MessageBox.Show(listColor.Count.ToString());
            //パレット初期化
            PalettePanColorDel();
            MyPalette.Clear();
            NumericScrollBar.Value = listColor.Count;
            TextBlockUsedColorCount.Text = "使用色数：" + listColor.Count.ToString();
            //listの値からRGBに戻してパレットに追加表示
            Color myColor;
            int iColor, rr, gg, bb;
            for (int i = 0; i < listColor.Count && i < MyBorderPalette.Length; ++i)
            {
                iColor = listColor[i];
                rr = iColor % 256;
                gg = iColor / 256 % 256;
                bb = iColor / 256 / 256;
                myColor = Color.FromRgb((byte)rr, (byte)gg, (byte)bb);
                MyBorderPalette[i].Background = new SolidColorBrush(myColor);
                MyPalette.Add(myColor);
            }
        }

        //失敗、誤差をRGBの合計を3で割ったものにしてみた
        private BitmapSource ReduceColor指定色誤差拡散で減色RGB平均分散(BitmapSource source, List<Color> palette, bool errorStack)
        {
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0;
            float gosa = 0;
            Color myColor;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    //誤差を足した色に一番近い色のパレットを取得
                    double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };//誤差拡散後の小数点RGB                    
                    myColor = GetColorNearPlette(iRGB, palette);//誤差拡散後の色に近いパレットの色
                    byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };//パレットのRGB
                    //RGB合計を3で割ったものを誤差にする
                    gosa = (errorStack) ?
                        (((iPixels[p] + iPixels[p + 1] + iPixels[p + 2]) - (myColor.R + myColor.G + myColor.B)) / 3f) / 16f :
                        (((pixels[p] + pixels[p + 1] + pixels[p + 2]) - (myColor.R + myColor.G + myColor.B)) / 3f) / 16f;
                    //誤差拡散
                    for (int i = 0; i < 3; ++i)
                    {
                        //gosa = (float)(pixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)
                        //gosa = (float)(iPixels[p + i] - pRGB[i]) / 16f;//誤差蓄積型(元の色-パレットの色)
                        //誤差を蓄積するか1ピクセルごとにリセットするか
                        //gosa = (errorStack) ? (float)(iPixels[p + i] - pRGB[i]) / 16f : (float)(pixels[p + i] - pRGB[i]) / 16f;
                        if (p + i + 3 < pixels.Length && x != w - 1)
                        {
                            iPixels[p + i + 3] += gosa * 7f;
                        }
                        if (y < h - 1)
                        {
                            iPixels[p + stride + i] += gosa * 5f;
                            if (x != 0)
                            {
                                iPixels[p + stride - 3] += gosa * 3f;
                            }
                            if (x < w - 1)
                            {
                                iPixels[p + stride + 3] += gosa * 1f;
                            }
                        }
                    }
                    //色変更
                    pixels[p + 0] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p + 2] = myColor.B;
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        //reverce
        private BitmapSource ReduceColor指定色誤差拡散で減色2(BitmapSource source, List<Color> palette)
        {
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0;
            float gosa = 0;
            Color myColor;
            for (int y = 0; y < h; ++y)
            {
                if (y % 2 == 0)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        p = y * stride + (x * 3);
                        double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };//誤差拡散後の小数点RGB
                        myColor = GetColorNearPlette(iRGB, palette);//誤差拡散後の色に近いパレットの色
                        byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };//パレットのRGB
                        //誤差拡散
                        for (int i = 0; i < 3; ++i)
                        {
                            gosa = (float)(pixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)

                            if (p + i + 3 < pixels.Length && x < w - 1)
                            {
                                iPixels[p + i + 3] += (gosa * 7f);//右隣
                            }
                            if (y < h - 1)
                            {
                                iPixels[p + i + stride] += (gosa * 5f);
                                if (x != 0)
                                {
                                    iPixels[p + i + stride - 3] += (gosa * 3f);//左下
                                }
                                if (x < w - 1)
                                {
                                    iPixels[p + i + stride + 3] += (gosa * 1f);//右下
                                }
                            }
                        }
                        //色変更
                        pixels[p + 0] = myColor.R;
                        pixels[p + 1] = myColor.G;
                        pixels[p + 2] = myColor.B;
                    }
                }
                else
                {
                    //left
                    for (int x = w - 1; x > 0; --x)
                    {
                        p = y * stride + (x * 3);
                        double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };//誤差拡散後の小数点RGB
                        myColor = GetColorNearPlette(iRGB, palette);//誤差拡散後の色に近いパレットの色
                        byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };//パレットのRGB
                        //誤差拡散
                        for (int i = 0; i < 3; ++i)
                        {
                            gosa = (float)(pixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)

                            if (x != 0)
                            {
                                iPixels[p + i - 3] += (gosa * 7f);//左
                            }
                            if (y < h - 1)
                            {
                                iPixels[p + i + stride] += (gosa * 5f);//真下
                                if (x < w - 1)
                                {
                                    iPixels[p + i + stride + 3] += (gosa * 3f);//右下
                                }
                                if (x != 0)
                                {
                                    iPixels[p + i + stride - 3] += (gosa * 1f);//左下
                                }
                            }
                        }
                        //色変更
                        pixels[p + 0] = myColor.R;
                        pixels[p + 1] = myColor.G;
                        pixels[p + 2] = myColor.B;
                    }
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        private BitmapSource ReduceColor指定色誤差拡散で減色(BitmapSource source, List<Color> palette, bool errorStack)
        {
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }
            long p = 0;
            float gosa = 0;
            Color myColor;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    //誤差を足した色に一番近い色のパレットを取得
                    double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };//誤差拡散後の小数点RGB                    
                    myColor = GetColorNearPlette(iRGB, palette);//誤差拡散後の色に近いパレットの色
                    byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };//パレットのRGB
                    //誤差拡散
                    for (int i = 0; i < 3; ++i)
                    {
                        //gosa = (float)(pixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)
                        //gosa = (float)(iPixels[p + i] - pRGB[i]) / 16f;//誤差蓄積型(元の色-パレットの色)
                        //誤差を蓄積するか1ピクセルごとにリセットするか
                        gosa = (errorStack) ? (float)(iPixels[p + i] - pRGB[i]) / 16f : (float)(pixels[p + i] - pRGB[i]) / 16f;
                        if (p + i + 3 < pixels.Length && x != w - 1)
                        {
                            iPixels[p + i + 3] += gosa * 7f;
                        }
                        if (y < h - 1)
                        {
                            iPixels[p + stride + i] += gosa * 5f;
                            if (x != 0)
                            {
                                iPixels[p + stride - 3] += gosa * 3f;
                            }
                            if (x < w - 1)
                            {
                                iPixels[p + stride + 3] += gosa * 1f;
                            }
                        }
                    }
                    //色変更
                    pixels[p + 0] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p + 2] = myColor.B;
                }
            }

            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        /// <summary>
        /// 誤差拡散で減色、誤差蓄積は0～255に制限
        /// </summary>
        /// <param name="source">PixelFormatはRgb24限定</param>
        /// <param name="palette">List<Color>型</param>
        /// <param name="errorStack">Trueで誤差蓄積なのでたくさん拡散する、falseでなしは拡散少なめ。Trueのほうがいいかも</param>
        /// <returns>PixelFormatはRgb</returns>
        private BitmapSource ReduceColor指定色誤差拡散で減色Limit0_255(BitmapSource source, List<Color> palette, bool errorStack)
        {
            if (palette.Count == 0) { return source; }
            //WriteableBitmapクラスのCopyPixelsを使って画像の色情報を配列に複製
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            //誤差計算のために小数点を使うのでfloat型の配列に複製したのを使う
            float[] iPixels = new float[pixels.Length];
            for (int i = 0; i < iPixels.Length; ++i)
            {
                iPixels[i] = pixels[i];
            }

            long p = 0, pp = 0;//注目するピクセルの配列のアドレス
            float gosa = 0, pGosa = 0;//誤差、＋誤差
            Color myColor;//パレットから選んだ色
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);//注目するピクセルのアドレス
                    //誤差拡散した色に一番近いパレットの色を取得する
                    //誤差拡散後の色（小数点RGB）
                    double[] iRGB = new double[] { iPixels[p + 0], iPixels[p + 1], iPixels[p + 2] };
                    //一番近い色取得
                    myColor = GetColorNearPlette(iRGB, palette);
                    //パレットのRGB
                    byte[] pRGB = new byte[] { myColor.R, myColor.G, myColor.B };

                    //RGBごとに誤差拡散、0以下の場合は0、255以上の場合は255に丸める（制限する）
                    for (int i = 0; i < 3; ++i)//RGBの3ループ
                    {
                        //gosa = (float)(pixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)
                        //gosa = (float)(iPixels[p + i] - pRGB[i]) / 16f;//誤差(元の色-パレットの色)
                        gosa = (errorStack) ? (float)(iPixels[p + i] - pRGB[i]) / 16f : (float)(pixels[p + i] - pRGB[i]) / 16f;
                        //右下ピクセルへ誤差拡散
                        pp = p + i + 3;//右下ピクセルアドレス
                        if (pp < pixels.Length && x != w - 1)//右ピクセル
                        {
                            //誤差拡散後の値が0以下なら0、255以上なら255に丸める
                            pGosa = iPixels[pp] + (gosa * 7f);//誤差拡散先の値に誤差を足す                            
                            iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            //↑の1行は↓の3行と同じ処理
                            //if (pGosa < 0) { iPixels[pp] = 0; }
                            //else if (pGosa > 255) { iPixels[pp] = 255; }
                            //else { iPixels[pp] = pGosa; }
                        }

                        if (y < h - 1)//注目するピクセルが最下段じゃないなら
                        {
                            //真下ピクセルへ誤差拡散
                            pp = p + stride + i;//真下ピクセルアドレス
                            pGosa = iPixels[pp] + (gosa * 5f);
                            iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            //左下ピクセルへ誤差拡散
                            if (x != 0)
                            {
                                pp = p + stride + i - 3;//左下ピクセルアドレス
                                pGosa = iPixels[pp] + (gosa * 3f);
                                iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            }
                            //右下ピクセルへ誤差拡散
                            if (x < w - 1)
                            {
                                pp = p + stride + i + 3;//右下ピクセルアドレス
                                pGosa = iPixels[pp] + (gosa * 1f);
                                iPixels[pp] = (pGosa < 0) ? 0 : (pGosa > 255) ? 255 : pGosa;
                            }
                        }
                    }
                    //色変更
                    pixels[p + 0] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p + 2] = myColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        //一番近いパレット色取得、RGB距離、未使用
        private Color GetColorNearPlette(Color color, List<Color> palette)
        {
            double min = 0;
            double distance = 0;
            int pIndex = 0;
            min = GetColorDistance(color, palette[0]);
            for (int i = 1; i < palette.Count; ++i)
            {
                distance = GetColorDistance(color, palette[i]);
                if (min > distance)
                {
                    min = distance;
                    pIndex = i;
                }
            }
            return palette[pIndex];
        }

        //一番近いパレット色取得、小数点RGB用、RGB距離
        private Color GetColorNearPlette(double[] rgb, List<Color> palette)
        {
            double min = 0;
            double distance = 0;
            int pIndex = 0;
            min = GetColorDistanceDouble(rgb, palette[0]);
            for (int i = 1; i < palette.Count; ++i)
            {
                distance = GetColorDistanceDouble(rgb, palette[i]);
                if (min > distance)
                {
                    min = distance;
                    pIndex = i;
                }
            }
            return palette[pIndex];
        }

        //パレットの色で減色
        private BitmapSource ReduceColor指定色で減色(BitmapSource source, List<Color> palette)
        {
            if (palette.Count == 0) { return source; }
            var wb = new WriteableBitmap(source);
            int h = wb.PixelHeight;
            int w = wb.PixelWidth;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[h * stride];
            wb.CopyPixels(pixels, stride, 0);
            long p = 0;
            int pIndex = 0;
            double min, distance;
            Color myColor;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    p = y * stride + (x * 3);
                    myColor = Color.FromRgb(pixels[p + 0], pixels[p + 1], pixels[p + 2]);
                    min = GetColorDistance(myColor, palette[0]);
                    pIndex = 0;
                    for (int i = 0; i < palette.Count; ++i)
                    {
                        distance = GetColorDistance(myColor, palette[i]);
                        if (min > distance)
                        {
                            min = distance;
                            pIndex = i;
                        }
                    }
                    myColor = palette[pIndex];
                    pixels[p] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p + 2] = myColor.B;
                }
            }
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }

        //Cubeを指定個数になるまで分割、ピクセル数が多いCubeを優先して分割
        private List<Cube> SplitCubeByColorsCount(int split, List<Cube> listCube)
        {
            int loopCount = 1;
            while (split > loopCount)
            {
                int max = 0, index = 0;
                for (int i = 0; i < listCube.Count; ++i)
                {
                    if (max < listCube[i].ListColors.Count)
                    {
                        max = listCube[i].ListColors.Count;
                        index = i;
                    }
                }
                listCube.AddRange(listCube[index].Split());
                listCube.RemoveAt(index);
                loopCount++;
            }
            return listCube;
        }

        //Cubeを指定個数になるまで分割、長辺が最大のCubeを優先
        private List<Cube> SplitCubeByLongSide(int split, List<Cube> listCube)
        {
            int loopCount = 1;
            while (split > loopCount)
            {
                int max = 0, index = 0;
                for (int i = 0; i < listCube.Count; ++i)
                {
                    if (max < listCube[i].LengthMax)
                    {
                        max = listCube[i].LengthMax;
                        index = i;
                    }
                }
                listCube.AddRange(listCube[index].Split());
                listCube.RemoveAt(index);
                loopCount++;
            }
            return listCube;
        }

        //距離
        private double GetColorDistance(Color c1, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(c1.R - c2.R, 2) +
                Math.Pow(c1.G - c2.G, 2) +
                Math.Pow(c1.B - c2.B, 2));
        }

        //距離、小数点RGB
        private double GetColorDistanceDouble(double[] rgb, Color c2)
        {
            return Math.Sqrt(
                Math.Pow(rgb[0] - c2.R, 2) +
                Math.Pow(rgb[1] - c2.G, 2) +
                Math.Pow(rgb[2] - c2.B, 2));
        }

        //パレットの色表示を初期化
        private void PalettePanColorDel()
        {
            for (int i = 0; i < MAX_PALETTE_COLOR_COUNT; ++i)
            {
                MyBorderPalette[i].Background = null;
            }
        }

        //パレットの色表示用のBorder作成
        private void AddBorders()
        {
            NumericScrollBar.Maximum = MAX_PALETTE_COLOR_COUNT;
            MyBorderPalette = new Border[MAX_PALETTE_COLOR_COUNT];
            Border border;
            for (int i = 0; i < MyBorderPalette.Length; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    Margin = new Thickness(1f)
                };
                MyBorderPalette[i] = border;
                MyWrapPanel.Children.Add(border);
            }
        }


        #region イベント



        //表示中の画像の色を取得、パレットにコピー
        private void ButtonGetImageColors_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            GetUsedColor((BitmapSource)MyImage.Source);
        }

        private void ButtonGosakakusanNoErrorStackLimit0_255_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色誤差拡散で減色Limit0_255(OriginBitmap, MyPalette, false);
        }

        private void ButtonGosakakusanNoErrorStack_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色誤差拡散で減色(OriginBitmap, MyPalette, false);
        }
        private void ButtonPixelFormatIndexed4_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var bitmap = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Indexed4, null, 0);
            MyImage.Source = bitmap;
        }

        private void ButtonPixelFormatIndexed2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var bitmap = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Indexed2, null, 0);
            MyImage.Source = bitmap;
        }

        private void ButtonGosakakusan3Limit0_255_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色誤差拡散で減色Limit0_255(OriginBitmap, MyPalette, true);
        }

        private void ButtonGosakakusan2_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色誤差拡散で減色2(OriginBitmap, MyPalette);
        }

        private void ButtonPixelFormatIndexed1_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            var bitmap = new FormatConvertedBitmap(OriginBitmap, PixelFormats.Indexed1, null, 0);
            MyImage.Source = bitmap;
        }
        private void ButtonGosakakusan_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色誤差拡散で減色(OriginBitmap, MyPalette, true);

        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            //パレットの色表示を初期化
            PalettePanColorDel();
            if (OriginBitmap == null) { return; }
            MyPalette.Clear();
            List<Cube> listCube = new List<Cube> { new Cube(OriginBitmap) };
            //長辺が最大のCubeを優先分割
            listCube = SplitCubeByLongSide((int)NumericScrollBar.Value, listCube);
            for (int i = 0; i < listCube.Count; ++i)
            {
                MyPalette.Add(listCube[i].GetAverageColor());
                MyBorderPalette[i].Background = new SolidColorBrush(MyPalette[i]);
            }
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //パレットの色表示を初期化
            PalettePanColorDel();
            if (OriginBitmap == null) { return; }
            MyPalette.Clear();
            List<Cube> listCube = new List<Cube> { new Cube(OriginBitmap) };
            //ピクセル数が多いCubeを優先分割
            listCube = SplitCubeByColorsCount((int)NumericScrollBar.Value, listCube);
            for (int i = 0; i < listCube.Count; ++i)
            {
                MyPalette.Add(listCube[i].GetAverageColor());
                MyBorderPalette[i].Background = new SolidColorBrush(MyPalette[i]);
            }
        }

        private void ButtonReduceColor_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色で減色(OriginBitmap, MyPalette);
        }

        private void ButtonOrigin_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = OriginBitmap;
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) == false) { return; }
            string[] filePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            OriginBitmap = GetBitmapSourceWithChangePixelFormat2(filePath[0], PixelFormats.Rgb24, 96, 96);

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

        #endregion

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
            catch (Exception) { }
            return source;
        }
    }
}



public class Cube
{
    public byte MinRed;//最小R
    public byte MinGreen;
    public byte MinBlue;
    public byte MaxRed;//最大赤
    public byte MaxGreen;
    public byte MaxBlue;
    public List<Color> ListColors;//色リスト
    public int LengthMax;//Cubeの最大辺長
    public int LengthRed;//赤の辺長
    public int LengthGreen;
    public int LengthBlue;

    //BitmapSourceからCubeを作成
    public Cube(BitmapSource source)
    {
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
        var wb = new WriteableBitmap(bitmap);
        int h = wb.PixelHeight;
        int w = wb.PixelWidth;
        int stride = wb.BackBufferStride;
        byte[] pixels = new byte[h * stride];
        wb.CopyPixels(pixels, stride, 0);
        long p = 0;
        byte cR, cG, cB;
        byte lR = 255, lG = 255, lB = 255, hR = 0, hG = 0, hB = 0;
        ListColors = new List<Color>();
        for (int y = 0; y < h; ++y)
        {
            for (int x = 0; x < w; ++x)
            {
                p = y * stride + (x * 4);
                cR = pixels[p + 2]; cG = pixels[p + 1]; cB = pixels[p];
                ListColors.Add(Color.FromRgb(cR, cG, cB));
                if (lR > cR) { lR = cR; }
                if (lG > cG) { lG = cG; }
                if (lB > cB) { lB = cB; }
                if (hR < cR) { hR = cR; }
                if (hG < cG) { hG = cG; }
                if (hB < cB) { hB = cB; }
            }
        }
        MinRed = lR; MinGreen = lG; MinBlue = lB;
        MaxRed = hR; MaxGreen = hG; MaxBlue = hB;
        LengthRed = 1 + MaxRed - MinRed;
        LengthGreen = 1 + MaxGreen - MinGreen;
        LengthBlue = 1 + MaxBlue - MinBlue;
        LengthMax = Math.Max(LengthRed, Math.Max(LengthGreen, LengthBlue));
    }

    //ColorのリストからCube作成
    public Cube(List<Color> color)
    {
        //Color cColor = color[0];
        byte lR = 255, lG = 255, lB = 255, hR = 0, hG = 0, hB = 0;
        byte cR, cG, cB;
        ListColors = new List<Color>();
        foreach (Color item in color)
        {
            cR = item.R; cG = item.G; cB = item.B;
            ListColors.Add(Color.FromRgb(cR, cG, cB));
            if (lR > cR) { lR = cR; }
            if (lG > cG) { lG = cG; }
            if (lB > cB) { lB = cB; }
            if (hR < cR) { hR = cR; }
            if (hG < cG) { hG = cG; }
            if (hB < cB) { hB = cB; }
        }
        MinRed = lR; MinGreen = lG; MinBlue = lB;
        MaxRed = hR; MaxGreen = hG; MaxBlue = hB;
        LengthRed = 1 + MaxRed - MinRed;
        LengthGreen = 1 + MaxGreen - MinGreen;
        LengthBlue = 1 + MaxBlue - MinBlue;
        LengthMax = Math.Max(LengthRed, Math.Max(LengthGreen, LengthBlue));
    }

    //一番長い辺で2分割
    public List<Cube> Split()
    {
        List<Color> low = new List<Color>();
        List<Color> high = new List<Color>();
        float mid;
        if (LengthMax == LengthRed)
        {//Rの辺が最長の場合、R要素の中間で2分割
            mid = ((MinRed + MaxRed) / 2f);
            foreach (Color item in ListColors)
            {
                if (item.R < mid) { low.Add(item); }
                else { high.Add(item); }
            }
        }
        else if (LengthMax == LengthGreen)
        {
            mid = ((MinGreen + MaxGreen) / 2f);
            foreach (Color item in ListColors)
            {
                if (item.G < mid) { low.Add(item); }
                else { high.Add(item); }
            }
        }
        else
        {
            mid = ((MinBlue + MaxBlue) / 2f);
            foreach (Color item in ListColors)
            {
                if (item.B < mid) { low.Add(item); }
                else { high.Add(item); }
            }
        }
        return new List<Cube> { new Cube(low), new Cube(high) };
    }

    //平均色
    public Color GetAverageColor()
    {
        List<Color> colorList = ListColors;
        long r = 0, g = 0, b = 0;
        int cCount = colorList.Count;
        if (cCount == 0)
        {
            return Color.FromRgb(127, 127, 127);
        }
        for (int i = 0; i < cCount; ++i)
        {
            r += colorList[i].R;
            g += colorList[i].G;
            b += colorList[i].B;
        }
        return Color.FromRgb((byte)(r / cCount), (byte)(g / cCount), (byte)(b / cCount));
    }
}
