using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
//メディアンカット法で色の選択、減色してみた、難しい(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15400162.html

namespace _20180306_メディアンカット法でパレット作成して減色
{
    public partial class MainWindow : Window
    {
        BitmapSource OriginBitmap;
        string ImageFileFullPath;
        Border[] MyPalette;
        List<Color> palette;
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
            AddBorders();//パレットの色表示用のBorder作成
            palette = new List<Color>();
        }


        private BitmapSource ReduceColor指定色で減色(BitmapSource source, List<Color> palette)
        {
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
                    p = y * stride + (x * 4);
                    myColor = Color.FromRgb(pixels[p + 2], pixels[p + 1], pixels[p]);
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
                    pixels[p + 2] = myColor.R;
                    pixels[p + 1] = myColor.G;
                    pixels[p] = myColor.B;
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

        //パレットの色表示を初期化
        private void PalettePanColorDel()
        {
            for (int i = 0; i < MAX_PALETTE_COLOR_COUNT; ++i)
            {
                MyPalette[i].Background = null;
            }
        }

        //パレットの色表示用のBorder作成
        private void AddBorders()
        {
            NumericScrollBar.Maximum = MAX_PALETTE_COLOR_COUNT;
            MyPalette = new Border[MAX_PALETTE_COLOR_COUNT];
            Border border;
            for (int i = 0; i < MyPalette.Length; i++)
            {
                border = new Border()
                {
                    Width = 18,
                    Height = 18,
                    BorderBrush = new SolidColorBrush(Colors.AliceBlue),
                    BorderThickness = new Thickness(1f),
                    Margin = new Thickness(1f)
                };
                MyPalette[i] = border;
                MyWrapPanel.Children.Add(border);
            }
        }


        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            //パレットの色表示を初期化
            PalettePanColorDel();
            if (OriginBitmap == null) { return; }
            palette.Clear();
            List<Cube> listCube = new List<Cube> { new Cube(OriginBitmap) };
            //長辺が最大のCubeを優先分割
            listCube = SplitCubeByLongSide((int)NumericScrollBar.Value, listCube);
            for (int i = 0; i < listCube.Count; ++i)
            {
                palette.Add(listCube[i].GetAverageColor());
                MyPalette[i].Background = new SolidColorBrush(palette[i]);
            }
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //パレットの色表示を初期化
            PalettePanColorDel();
            if (OriginBitmap == null) { return; }
            palette.Clear();
            List<Cube> listCube = new List<Cube> { new Cube(OriginBitmap) };
            //ピクセル数が多いCubeを優先分割
            listCube = SplitCubeByColorsCount((int)NumericScrollBar.Value, listCube);
            for (int i = 0; i < listCube.Count; ++i)
            {
                palette.Add(listCube[i].GetAverageColor());
                MyPalette[i].Background = new SolidColorBrush(palette[i]);
            }
        }

        private void ButtonReduceColor_Click(object sender, RoutedEventArgs e)
        {
            if (OriginBitmap == null) { return; }
            MyImage.Source = ReduceColor指定色で減色(OriginBitmap, palette);
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
        Color cColor = color[0];
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


