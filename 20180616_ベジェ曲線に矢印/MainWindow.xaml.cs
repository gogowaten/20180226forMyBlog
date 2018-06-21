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
//WPFで矢印曲線、ベジェ曲線(Path)と矢印(Polygon)を組み合わせて表現、PolyBezierSegment(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15559602.html

namespace _20180616_ベジェ曲線に矢印
{
    public partial class MainWindow : Window
    {
        PolyBezierSegment MySegment;
        Point OffsetFinePoint;//矢印の先端と左上との差
        Point ContactPoint; //線の終端にする矢印の後ろ座標

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();
            MyCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
            this.Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //矢印の先端をマウスカーソル位置に合わせるときのオフセット座標を設定
            OffsetFinePoint = new Point(ArrowHead.ActualWidth / 2, 0);
            //OffsetFinePoint = new Point(ArrowHead.RenderedGeometry.Bounds.Width / 2, 0);

            //矢印を回転するときの中心になる座標の指定
            //矢印の先端を中心にしたい、先端は左右だと中間なのでxは0.5、上下は上端にあるので0.0を指定
            ArrowHead.RenderTransformOrigin = new Point(0.5, 0.0);

            //線の終端にする座標は矢印の後ろ座標
            //-2は位置調整、0だと矢印と曲線の間に隙間ができる
            //マイナスを大きくすると矢印と線の重なりが大きくなる
            ContactPoint = new Point(ArrowHead.ActualWidth / 2, ArrowHead.ActualHeight - 2);
        }

        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MySegment.Points.Clear();
        }

        //Path.Dataを作成
        private void MyInitialize()
        {
            MySegment = new PolyBezierSegment();
            var pf = new PathFigure();
            pf.Segments.Add(MySegment);
            var pg = new PathGeometry();
            pg.Figures.Add(pf);
            MyPath.Data = pg;

        }

        //マウス移動時、終端のアンカーポイント移動とその他の制御点の位置調整
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            PointCollection ps = MySegment.Points;
            Point mouseP = e.GetPosition(MyCanvas);//マウスカーソル位置

            if (ps.Count > 5)
            {

                //終端のアンカーの座標は今のカーソル座標
                ps[ps.Count - 1] = mouseP;

                Point preAP = ps[ps.Count - 4];//一個前のアンカーポイント
                Point prepreAP;//二個前のアンカーポイント
                if (ps.Count < 7)
                {
                    var pg = (PathGeometry)MyPath.Data;
                    var pathFigureCollection = pg.Figures;
                    prepreAP = pathFigureCollection[0].StartPoint;
                }
                else
                {
                    prepreAP = ps[ps.Count - 7];
                }

                //終端アンカーと二個前のアンカーとの距離の1 / 4
                double xDiff = (mouseP.X - prepreAP.X) / 4.0;
                double yDiff = (mouseP.Y - prepreAP.Y) / 4.0;
                //一個前のアンカーポイントの制御点座標
                ps[ps.Count - 5] = new Point(preAP.X - xDiff, preAP.Y - yDiff);//終端から遠いほう
                ps[ps.Count - 3] = new Point(preAP.X + xDiff, preAP.Y + yDiff);//終端から近いほう

                //矢印の回転角度                
                //角度はマウスカーソル座標と一個前のアンカーの手前側の制御点との直線の角度
                double angle = Math.Atan2(mouseP.Y - ps[ps.Count - 3].Y, mouseP.X - ps[ps.Count - 3].X);
                angle = angle / Math.PI * 180;//ラジアンから度数へ変換
                angle += 90;//調整、元の矢印は上向きだけど0度は右向きだから
                //矢印回転
                ArrowHead.RenderTransform = new RotateTransform(angle);
                MyLabel.Content = "Angle = " + angle.ToString();
                //矢印の位置、先端をマウスカーソルに合わせる
                mouseP.Offset(-OffsetFinePoint.X, -OffsetFinePoint.Y);
                Canvas.SetLeft(ArrowHead, mouseP.X);
                Canvas.SetTop(ArrowHead, mouseP.Y);

                //終端座標決定
                //矢印の後ろ(接続座標)をベジェ曲線の終端にする
                //接続座標はTransformToVisualで計算
                GeneralTransform gt = ArrowHead.TransformToVisual(MyCanvas);
                Point lastAnc = gt.Transform(ContactPoint);
                ps[ps.Count - 1] = lastAnc;

                //終端(アンカー)と一個前のアンカーの距離の1/4
                xDiff = (lastAnc.X - ps[ps.Count - 3].X) / 4.0;
                yDiff = (lastAnc.Y - ps[ps.Count - 3].Y) / 4.0;
                //終端の制御点座標                
                Point lastControlP = new Point(lastAnc.X - xDiff, lastAnc.Y - yDiff);
                ps[ps.Count - 2] = lastControlP;

            }
            else if (ps.Count > 0)
            {
                //終端はカーソル位置
                ps[ps.Count - 1] = e.GetPosition(MyCanvas);//カーソル位置に
                //矢印の位置調整
                mouseP.Offset(-OffsetFinePoint.X, -OffsetFinePoint.Y);
                Canvas.SetLeft(ArrowHead, mouseP.X);
                Canvas.SetTop(ArrowHead, mouseP.Y);
            }

        }

        //マウスクリック時、アンカーポイントと制御点2つを追加
        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(MyCanvas);
            //最初のクリック時だけはStartPointを指定する
            if (MySegment.Points.Count == 0)
            {
                var pathGeometry = (PathGeometry)MyPath.Data;
                var pathFigureCollection = pathGeometry.Figures;
                //始点追加
                pathFigureCollection[0].StartPoint = p;
                //制御点2つとアンカーポイントの合計3つ追加
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
            }
            else
            {
                //一個前になるアンカー座標をクリック座標にする
                MySegment.Points[MySegment.Points.Count - 1] = p;
                //制御点2つとアンカーポイントの合計3つ追加
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
                MySegment.Points.Add(p);
            }

        }
    }
}
