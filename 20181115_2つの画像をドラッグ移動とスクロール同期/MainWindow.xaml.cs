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
//WPF、ScrollViewerの中の要素をマウスドラッグ移動しているように見せかける(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15755956.html

namespace _20181115_2つの画像をドラッグ移動とスクロール同期
{
    public partial class MainWindow : Window
    {
        ScaleTransform MyScale;//拡大率用
        Point MyPoint;// マウスクリックの位置用

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();

            Loaded += MainWindow_Loaded;
            MyInitialize();
        }

        //初期処理
        private void MyInitialize()
        {
            //表示する画像ファイルのパス
            string filePath1 = @"D:\ブログ用\チェック用2\NEC_5899_2018_10_28_午後わてん_.jpg";
            string filePath2 = @"D:\ブログ用\チェック用2\NEC_5899_2018_10_28_午後わてん_16color.png";

            //Imageに画像表示
            MyImage1.Source = new BitmapImage(new Uri(filePath1));
            MyImage2.Source = new BitmapImage(new Uri(filePath2));


            SliderScale.ValueChanged += SliderScale_ValueChanged;
            MyScroll1.ScrollChanged += MyScroll1_ScrollChanged;
            MyScroll2.ScrollChanged += MyScroll2_ScrollChanged;
            MyImage1.MouseRightButtonDown += MyImage1_MouseRightButtonDown;
            MyImage1.MouseRightButtonUp += MyImage1_MouseRightButtonUp;
            MyImage1.MouseMove += MyImage1_MouseMove;
            MyImage2.MouseRightButtonDown += MyImage2_MouseRightButtonDown;
            MyImage2.MouseRightButtonUp += MyImage2_MouseRightButtonUp;
            MyImage2.MouseMove += MyImage2_MouseMove;
            //拡大倍率用
            MyScale = new ScaleTransform();
            MyImage1.RenderTransform = MyScale;
            MyImage2.RenderTransform = MyScale;
        }

        private void MyImage2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MyImage2.Cursor = Cursors.ScrollAll;
                var mouseP = e.GetPosition(this);             
                var xd = MyPoint.X - mouseP.X;
                var yd = MyPoint.Y - mouseP.Y;
                xd += MyScroll2.HorizontalOffset;
                yd += MyScroll2.VerticalOffset;                
                MyScroll2.ScrollToHorizontalOffset(xd);
                MyScroll2.ScrollToVerticalOffset(yd);
                MyPoint = mouseP;
            }
        }

        private void MyImage2_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MyImage2.ReleaseMouseCapture();
            MyImage2.Cursor = Cursors.Arrow;
        }

        private void MyImage2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);            
            MyImage2.CaptureMouse();
        }

        private void MyImage1_MouseMove(object sender, MouseEventArgs e)
        {
                //マウスの移動距離だけスクロールさせる
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MyImage1.Cursor = Cursors.ScrollAll;
                //今のマウスの座標
                var mouseP = e.GetPosition(this);
                //マウスの移動距離＝直前の座標と今の座標の差
                var xd = MyPoint.X - mouseP.X;
                var yd = MyPoint.Y - mouseP.Y;
                //xd *= 2;//2倍速
                //yd *= 2;
                //移動距離＋今のスクロール位置
                xd += MyScroll1.HorizontalOffset;
                yd += MyScroll1.VerticalOffset;
                //スクロール位置の指定
                MyScroll1.ScrollToHorizontalOffset(xd);
                MyScroll1.ScrollToVerticalOffset(yd);

                MyPoint = mouseP;//直前の座標を今の座標に変更
            }
        }

        private void MyImage1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage1.ReleaseMouseCapture();
            MyImage1.Cursor = Cursors.Arrow;
        }

        private void MyImage1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPoint = e.GetPosition(this);
            //マウスがScrollViewer外になってもドラッグ移動を有効にしたいときだけ必要
            MyImage1.CaptureMouse();
        }

        //アプリ起動完了時に表示された画像サイズを取得してCanvasサイズに指定する
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvasサイズを画像のサイズにして
            //ScrollViewerによるスクロールバーを正しく表示する
            MyCanvas1.Width = MyImage1.ActualWidth;
            MyCanvas1.Height = MyImage1.ActualHeight;
            MyCanvas2.Width = MyImage2.ActualWidth;
            MyCanvas2.Height = MyImage2.ActualHeight;
            //画像の拡大方法の指定、無指定なら線形補間
            RenderOptions.SetBitmapScalingMode(MyImage1, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetBitmapScalingMode(MyImage2, BitmapScalingMode.NearestNeighbor);

        }


        //		UWPのScrollViewerでスクロール位置の同期を行うメモ
        //http://studio-geek.com/archives/857
        private void MyScroll2_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //値が双方で異なるときだけ更新
            if (e.VerticalOffset != MyScroll1.VerticalOffset)
            {
                MyScroll1.ScrollToVerticalOffset(e.VerticalOffset);
            }
            if (e.HorizontalOffset != MyScroll1.HorizontalOffset)
            {
                MyScroll1.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void MyScroll1_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset != MyScroll2.VerticalOffset)
            {
                MyScroll2.ScrollToVerticalOffset(e.VerticalOffset);
            }
            if (e.HorizontalOffset != MyScroll2.HorizontalOffset)
            {
                MyScroll2.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        //拡大倍率変更時はImageを乗せているCanvasのサイズを変更する
        private void SliderScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //ScaleTransformの拡大倍率変更
            MyScale.ScaleX = e.NewValue;
            MyScale.ScaleY = e.NewValue;
            //拡大後Imageのサイズを取得
            var bounds = MyScale.TransformBounds(new Rect(MyImage1.RenderSize));

            //Imageが乗っかっているCanvasのサイズを変更すると
            //正しく表示され、スクロールバーも期待通りになる
            MyCanvas1.Height = bounds.Height;
            MyCanvas1.Width = bounds.Width;

            //Image2も同様
            bounds = MyScale.TransformBounds(new Rect(MyImage2.RenderSize));
            MyCanvas2.Height = bounds.Height;
            MyCanvas2.Width = bounds.Width;

        }

    }
}
