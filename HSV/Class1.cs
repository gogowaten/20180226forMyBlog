using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
//参照したところ
//明度と彩度の求め方(プログラム ) - Color Model：色をプログラムするブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/pspevolution7/17679694.html

namespace MyHSV
{
    /// <summary>
    /// Color(RGB)と円柱モデルHSVの相互変換
    /// </summary>
    public class HSV
    {
        public double Hue;          //0.0fから360.0f
        public double Saturation;   //0.0fから1.0f
        public double Value;        //0.0fから1.0f

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hue">0.0fから360.0f</param>
        /// <param name="saturation">0.0fから1.0f</param>
        /// <param name="value">0.0fから1.0f</param>
        public HSV(double hue = 0f, double saturation = 0f, double value = 0f)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        /// <summary>
        /// Color(RGB)をHSV(円柱モデル)に変換、Hの値は0fから360f、SとVは0fから1f
        /// </summary>
        /// <param name="color"></param>
        /// <returns>HSV</returns>
        public static HSV Color2HSV(Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            byte Max = Math.Max(R, Math.Max(G, B));
            byte Min = Math.Min(R, Math.Min(G, B));
            if (Max == 0) { return new HSV(360f, 0f, 0f); }

            double chroma = Max - Min;
            double h = 0;
            double s = chroma / Max;
            double v = Max / 255f;

            if (Max == Min) { h = 360f; }
            else if (Max == R)
            {
                h = 60f * (G - B) / chroma;
                if (h < 0) { h += 360f; }
            }
            else if (Max == G)
            {
                h = 60f * (B - R) / chroma + 120f;
            }
            else if (Max == B)
            {
                h = 60f * (R - G) / chroma + 240f;
            }
            else { h = 360f; }

            return new HSV(h, s, v);
        }

        /// <summary>
        /// RGBをHSV(円柱モデル)に変換、RGBそれぞれの値を指定する
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns>HSV</returns>
        public static HSV Color2HSV(byte r, byte g, byte b)
        {
            return Color2HSV(Color.FromRgb(r, g, b));
        }


//        プログラミング 第6弾(プログラム ) - Color Model：色をプログラムするブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/pspevolution7/17682985.html

        /// <summary>
        /// Color(RGB)をHSV(円錐モデル)に変換、Hの値は0fから360f、SとVは0fから1f
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static HSV Color2HSV_ConicalModel(Color color)
        {
            byte R = color.R;
            byte G = color.G;
            byte B = color.B;
            byte Max = Math.Max(R, Math.Max(G, B));
            byte Min = Math.Min(R, Math.Min(G, B));
            if (Max == 0) { return new HSV(360f, 0f, 0f); }

            double chroma = Max - Min;
            double h = 0;
            double s = chroma / 255f;//円錐モデル
            double v = Max / 255f;

            if (Max == Min) { h = 360f; }
            else if (Max == R)
            {
                h = 60f * (G - B) / chroma;
                if (h < 0) { h += 360f; }
            }
            else if (Max == G)
            {
                h = 60f * (B - R) / chroma + 120f;
            }
            else if (Max == B)
            {
                h = 60f * (R - G) / chroma + 240f;
            }
            else { h = 360f; }

            return new HSV(h, s, v);
        }

        /// <summary>
        /// RGBをHSV(円錐モデル)に変換、RGBそれぞれの値を指定する
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static HSV Color2HSV_ConicalModel(byte r, byte g, byte b)
        {
            return Color2HSV_ConicalModel(Color.FromRgb(r, g, b));
        }



        /// <summary>
        /// HSV(円柱モデル)をColorに変換
        /// </summary>
        /// <param name="hsv"></param>
        /// <returns>Color</returns>
        public static Color HSV2Color(HSV hsv)
        {
            double h = (hsv.Hue % 360f) / 60f;
            double s = hsv.Saturation, v = hsv.Value;
            double r = v, g = v, b = v;

            if (v == 0) { return Color.FromRgb(0, 0, 0); }

            int i = (int)Math.Floor(h);
            double d = h - i;
            if (h < 1)
            {
                g *= 1f - s * (1f - d);
                b *= 1f - s;
            }
            else if (h < 2)
            {
                r *= 1f - s * d;
                b *= 1f - s;
            }
            else if (h < 3)
            {
                r *= 1f - s;
                b *= 1f - s * (1f - d);
            }
            else if (h < 4)
            {
                r *= 1f - s;
                g *= 1f - s * d;
            }
            else if (h < 5)
            {
                r *= 1f - s * (1f - d);
                g *= 1f - s;
            }
            else// if (h < 6)
            {
                g *= 1f - s;
                b *= 1f - s * d;
            }

            //return Color.FromScRgb(1f,(float)r,(float)g,(float)b);
            return Color.FromRgb(
                (byte)Math.Round(r * 255f, MidpointRounding.AwayFromZero),
                (byte)Math.Round(g * 255f, MidpointRounding.AwayFromZero),
                (byte)Math.Round(b * 255f, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// HSV(円柱モデル)をColorに変換、h,s,vそれぞれの値を指定する
        /// </summary>
        /// <param name="h">0fから360fの間で指定</param>
        /// <param name="s">0fから1fの間で指定</param>
        /// <param name="v">0fから1fの間で指定</param>
        /// <returns>Color</returns>
        public static Color HSV2Color(double h, double s, double v)
        {
            return HSV2Color(new HSV(h, s, v));
        }

        /// <summary>
        /// 円錐モデルのHSVをColorに変換
        /// </summary>
        /// <param name="hsv">円錐モデルのHSV</param>
        /// <returns></returns>
        public static Color HSV_ConicalModel2Color(HSV hsv)
        {
            double Max = hsv.Value * 255f;
            double Min = (hsv.Value - hsv.Saturation) * 255f;
            double d = Max - Min;

            double h = hsv.Hue;
            double r = 0, g = 0, b = 0;

            if (h < 60)
            {
                r = Max;
                g = Min + d * h / 60f;
                b = Min;
            }
            else if (h < 120)
            {
                r = Min + d * (120f - h) / 60f;
                g = Max;
                b = Min;
            }
            else if (h < 180)
            {
                r = Min;
                g = Max;
                b = Min + d * (h - 120f) / 60f;
            }
            else if (h < 240)
            {
                r = Min;
                g = Min + d * (240f - h) / 60f;
                b = Max;
            }
            else if (h < 300)
            {
                r = Min + d * (h - 240f) / 60f;
                g = Min;
                b = Max;
            }
            else
            {
                r = Max;
                g = Min;
                b = Min + d * (360f - h) / 60f;
            }
            return Color.FromRgb(
                (byte)Math.Round(r, MidpointRounding.AwayFromZero),
                (byte)Math.Round(g, MidpointRounding.AwayFromZero),
                (byte)Math.Round(b, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// 円錐モデルのHSVをColorに変換
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Color HSV_ConicalModel2Color(double h,double s,double v)
        {
            return HSV_ConicalModel2Color(new HSV(h, s, v));
        }


        public override string ToString()
        {
            //return base.ToString();
            return $"{Hue}, {Saturation}, {Value}";
        }
        public string ToString100()
        {
            return $"{Hue:000.00}, {Saturation * 100:000.00}, {Value * 100:000.00}";
        }
    }
}
