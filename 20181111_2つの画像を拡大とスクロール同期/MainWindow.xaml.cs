using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

//WPF、スクロールバーの同期、2つの画像を並べて拡大して見比べたい、ScrollViewer(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15754500.html

namespace _20181111_2つの画像を拡大とスクロール同期
{
    public partial class MainWindow : Window
    {
        ScaleTransform MyScale;//拡大率用

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
            //filePath1 = @"D:\ブログ用\チェック用2\NEC_5898_2018_10_28_午後わてん.jpg";
            //filePath2 = @"D:\ブログ用\チェック用2\NEC_5898_2018_10_28_午後わてん.jpg";
            //filePath2 = @"D:\ブログ用\チェック用2\NEC_5899_2018_10_28_午後わてん_4色誤差拡散.png";
            //filePath2 = @"D:\ブログ用\チェック用2\NEC_5899_2018_10_28_午後わてん_256色.png";
            //filePath2 = @"D:\ブログ用\チェック用2\NEC_5899_2018_10_28_午後わてん_256色誤差拡散.png";

            //Imageに画像表示
            MyImage1.Source = new BitmapImage(new Uri(filePath1));
            MyImage2.Source = new BitmapImage(new Uri(filePath2));


            SliderScale.ValueChanged += SliderScale_ValueChanged;
            MyScroll1.ScrollChanged += MyScroll1_ScrollChanged;
            MyScroll2.ScrollChanged += MyScroll2_ScrollChanged;

            //拡大倍率用
            MyScale = new ScaleTransform();
            MyImage1.RenderTransform = MyScale;
            MyImage2.RenderTransform = MyScale;
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

            //ScrollViewer同士のスクロール具合のbindingはエラーになる
            //var b = new Binding();
            //b.Source = MyScroll1;
            ////b.Mode = BindingMode.TwoWay;
            //b.Path = new PropertyPath(ScrollViewer.VerticalOffsetProperty);
            //MyText.SetBinding(TextBlock.TextProperty, b);
            ////MyScroll2.SetBinding(ScrollViewer.VerticalOffsetProperty, b);
            
        }


        //        UWPのScrollViewerでスクロール位置の同期を行うメモ
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
            //Imageのサイズを直接変更すると表示されなくなるので使わない
            //MyImage1.Height = bounds.Height;
            //MyImage1.Width = bounds.Width;

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
