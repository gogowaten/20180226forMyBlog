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
//WPFで矢印線、直線(PolyLine)と矢印(Polygon)を組み合わせて表現(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15558066.html


namespace _20180615_直線に矢印2
{
    public partial class MainWindow : Window
    {
        Point OffsetFine矢印先端Point;
        double ArrowheadHeight矢印の高さ;

        public MainWindow()
        {
            InitializeComponent();
            
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
            this.Loaded += MainWindow_Loaded;            
        }

        //アプリ起動完了時、初期設定
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OffsetFine矢印先端Point = new Point(ArrowHead.ActualWidth / 2, 0);
            ArrowheadHeight矢印の高さ = ArrowHead.ActualHeight;
            //矢印の回転の中心は矢印先端にしたいので、横は真ん中、縦は上端に設定
            ArrowHead.RenderTransformOrigin = new Point(0.5, 0.0);
        }

        //右クリックでクリア
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MyPolyline.Points.Clear();
        }

        //左クリックで座標追加
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointCollection points = MyPolyline.Points;
            Point ima = e.GetPosition(MyCanvas);//マウスカーソル位置
            //最初のクリックのときだけ座標を2個追加する
            if (points.Count == 0)
            {
                points.Add(ima);
            }
            //一個前の頂点座標はクリック座標を指定            
            points[points.Count - 1] = ima;            
            points.Add(ima);//追加            
        }
        
        //マウス移動時は矢印の移動と回転、線分の終端座標を矢印の後ろへ
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointCollection points = MyPolyline.Points;
            Point ima = e.GetPosition(MyCanvas);//今の頂点、マウスカーソル位置
            if (points.Count != 0)
            {
                Point pre = points[points.Count - 2];//一個前の頂点
                //矢印の回転
                double angle = Math.Atan2(ima.Y - pre.Y, ima.X - pre.X);
                angle = angle / Math.PI * 180;
                angle += 90;
                ArrowHead.RenderTransform = new RotateTransform(angle);
                MyLabel.Content = "Angle = " + angle.ToString();

                //線分の終端座標を矢印の後ろにする
                points[points.Count - 1] = GetContactPoint接点(pre, ima);
                //points[points.Count - 1] = ima;
            }
            //マウスカーソル位置に矢印先端を移動
            ima.Offset(-OffsetFine矢印先端Point.X, -OffsetFine矢印先端Point.Y);
            Canvas.SetLeft(ArrowHead, ima.X);
            Canvas.SetTop(ArrowHead, ima.Y);
                        
            //Geometry rg = ArrowHead.RenderedGeometry;
            //var boundsRect = rg.Bounds;//penの太さを無視した描画サイズ、Transform前
            //var renderBountsRect = rg.GetRenderBounds(new Pen());//penの太さを考慮した描画サイズ
            //double areaGeometry = rg.GetArea();//意味がわからん
            //PathGeometry outlinePathGeometry = rg.GetOutlinedPathGeometry();
        }

        /// <summary>
        /// 矢印後ろと直線の接触座標
        /// </summary>
        /// <param name="pre">一個前の頂点</param>
        /// <param name="ima">マウスカーソル位置</param>
        /// <returns></returns>
        private Point GetContactPoint接点(Point pre, Point ima)
        {            
            double lastLineLength = Distance(ima, pre);//今と一個前との距離
            //距離から矢印の高さの差
            double diffLength = lastLineLength - ArrowheadHeight矢印の高さ + 1;
            double ratio = diffLength / lastLineLength;//比率
            double x = (ima.X - pre.X) * ratio;//今と一個前のx距離*比率
            double y = (ima.Y - pre.Y) * ratio;//y
            Point nPoint = new Point(pre.X + x, pre.Y + y);
            return nPoint;
        }

        //2点間の距離、ユークリッド距離
        private double Distance(Point p1, Point p2) =>
            Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }
}
