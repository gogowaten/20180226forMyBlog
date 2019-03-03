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

using System.Windows.Threading;

//C# - 【C#】画面上のクリックされた地点のRGB値を取得したい｜teratail
//https://teratail.com/questions/25722

//Graphics.CopyFromScreen Method(System.Drawing) | Microsoft Docs
//https://docs.microsoft.com/ja-jp/dotnet/api/system.drawing.graphics.copyfromscreen?view=netframework-4.7.2
//タイマにより一定時間間隔で処理を行うには？（WPFタイマ編）：.NET TIPS - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1812/12/news014.html
//WPFでスクリーンショットの取得 - SourceChord
//http://sourcechord.hatenablog.com/entry/20131013/1381691785
//仮想キーの状態
//http://wisdom.sakura.ne.jp/system/winapi/win32/win32.html

//System.DrawingとSystem.Windows.Formsを参照に追加する必要がある

//System.Drawingは画面全体をキャプチャするために
//System.Windows.Formsはマウスカーソルの画面上での座標の取得のため
//一定時間ごとにマウスカーソル位置を取得、その座標の1ピクセルをキャプチャして、その色を取得

    //このアプリの記事
//WPF？画面上のどこでもマウスカーソル下の色を取得(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15890527.html

namespace _20190302_アプリの外でもカーソル下の色取得
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //クリックされているか判定用
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern short GetKeyState(int nVirtkey);

        public MainWindow()
        {
            InitializeComponent();

            //タイトルバーにアプリの名前（アセンブリ名）表示
            var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Title = info.ProductName;

            //タイマーの設定
            var timer = new DispatcherTimer(DispatcherPriority.Normal);
            //timer.Interval = new TimeSpan(100000);//ナノ秒
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);//10ミリ秒毎（0.01秒毎）
            timer.Start();
            timer.Tick += Timer_Tick;
        }


        //タイマーのTickイベント時
        private void Timer_Tick(object sender, EventArgs e)
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;//マウスカーソル位置取得
            Color c = GetPixelColor(p.X, p.Y);//マウスカーソル位置の色取得
            var b = new SolidColorBrush(c);
            MyTextBlockColor.Background = b;
            MyTextBlockColor.Text = c.ToString();
            MyTextBlockCursorLocation.Text = $"マウスの位置 = {System.Windows.Forms.Cursor.Position}";

            if (IsClickDown())
            {
                MyTextBlockGetColor.Background = b;
            }
        }

        //クリック判定
        private bool IsClickDown()
        {
            //マウス左ボタン(0x01)の状態、押されていたらマイナス値(-127)、なかったら0
            return GetKeyState(0x01) < 0;
        }

        //指定座標の1ピクセルの色を返す
        private Color GetPixelColor(int x, int y)
        {
            //1x1サイズのBitmap作成
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(
                1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    //画面全体をキャプチャして指定座標の1ピクセルだけBitmapにコピー
                    bmpGraphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));
                    //ピクセルの色取得
                    System.Drawing.Color color = bitmap.GetPixel(0, 0);
                    //WPF用にSystem.Windows.Media.Colorに変換して返す
                    return Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
        }

        //マウスカーソル座標の1ピクセルの色を返す
        private Color GetPixelColor2()
        {
            //1x1サイズのBitmap作成
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(
                1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(bitmap))
                {
                    //マウスカーソル座標取得
                    var p = System.Windows.Forms.Cursor.Position;
                    //画面全体をキャプチャして指定座標の1ピクセルだけBitmapにコピー
                    bmpGraphics.CopyFromScreen(p.X, p.Y, 0, 0, new System.Drawing.Size(1, 1));
                    //ピクセルの色取得
                    System.Drawing.Color color = bitmap.GetPixel(0, 0);
                    //WPF用にSystem.Windows.Media.Colorに変換して返す
                    return Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
        }
    }
}
