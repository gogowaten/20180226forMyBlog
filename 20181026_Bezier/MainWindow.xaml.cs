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

namespace _20181026_Bezier
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private PointCollection AnchorPoints;//ベジェ曲線Pathのアンカー点
        private Path MyBezierPath;//ベジェ曲線Path
        private Path VertexPath;//頂点表示用
        private Path ControlLinePath;//確認用方向線表示用

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            AnchorPoints = new PointCollection() { new Point(150, 250), new Point(100, 250), new Point(400, 350), new Point(200, 250) };
            AnchorPoints = new PointCollection() { new Point(150, 200), new Point(100, 250), new Point(400, 350) };
            //AnchorPoints = new PointCollection() { new Point(150, 100), new Point(200, 150), new Point(150, 200), new Point(100, 150), new Point(150, 100) };
            //AnchorPoints = new PointCollection() { new Point(100, 250), new Point(200, 250), new Point(350, 350), new Point(350, 250) };
            //AnchorPoints = new PointCollection() { new Point(100, 250), new Point(200, 250), new Point(350, 350) };
            AddBezierPath();//ベジェ曲線Path追加
            AddVertexPath();//確認用座標点表示
            AddLinePath();//確認用方向線表示
            ButtonRandomPoint.Click += ButtonRandomPoint_Click;
            ButtonA0.Click += ButtonA0_Click;
            ButtonA1.Click += ButtonA1_Click;
            ButtonA2.Click += ButtonA2_Click;
            ButtonA3.Click += ButtonA3_Click;
            ButtonB1.Click += ButtonB1_Click;
            ButtonB2.Click += ButtonB2_Click;
        }


        private void ButtonRandomPoint_Click(object sender, RoutedEventArgs e)
        {
            AnchorPoints = RandomPoint(5);
            MyCanvas.Children.Remove(MyBezierPath);
            AddBezierPath();
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }

        private void ButtonB2_Click(object sender, RoutedEventArgs e)
        {
            double distance = GetTotalDistance(AnchorPoints);
            distance /= AnchorPoints.Count - 1;
            distance *= 0.3;
            ToCurve(ControlPointsB1, MyBezierPath, AnchorPoints, distance);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }

        private void ButtonB1_Click(object sender, RoutedEventArgs e)
        {
            ToCurve(ControlPointsB1, MyBezierPath, AnchorPoints, 25.0);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }

        private void ButtonA3_Click(object sender, RoutedEventArgs e)
        {
            ToCurve(ControlPointsA3, MyBezierPath, AnchorPoints, 0.3);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }

        private void ButtonA2_Click(object sender, RoutedEventArgs e)
        {
            ToCurve(ControlPointsA2, MyBezierPath, AnchorPoints, 0.3);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }

        private void ButtonA1_Click(object sender, RoutedEventArgs e)
        {
            ToCurve(ControlPointsA1, MyBezierPath, AnchorPoints, 0.3);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
            AddLinePath();
        }
        private void ButtonA0_Click(object sender, RoutedEventArgs e)
        {
            ToCurve(ControlPointsA0, MyBezierPath, AnchorPoints, 0);
            MyCanvas.Children.Remove(VertexPath);
            AddVertexPath();
            MyCanvas.Children.Remove(ControlLinePath);
        }

        /// <summary>
        /// ベジェ曲線Pathの制御点座標を変化させて曲がり具合を変える
        /// </summary>
        /// <param name="func">制御点座標を求める関数を指定</param>
        /// <param name="bezier">ベジェ曲線Path</param>
        /// <param name="anchor">ベジェ曲線Pathの全アンカー点</param>
        /// <param name="curve">曲げ具合の指定</param>
        private void ToCurve(Func<Point, Point, Point, double, (Point, Point)> func, Path bezier, PointCollection anchor, double curve)
        {
            PointCollection segPoints = GetPolyBezierSegmentPoints(bezier);
            segPoints[0] = anchor[0];//始点アンカー点の制御点

            for (int i = 1; i < anchor.Count - 1; i++)
            {
                //現在アンカー点とその前後のアンカー点から現在のアンカー点の2つの制御点座標を求める
                (Point begin, Point end) = func(anchor[i - 1], anchor[i], anchor[i + 1], curve);
                segPoints[i * 3 - 2] = begin;//始点側制御点
                segPoints[i * 3] = end;//終点側制御点
            }

            //終点アンカー点の制御点
            segPoints[segPoints.Count - 1] = anchor[anchor.Count - 1];
        }



        #region 制御点座標を求める関数

        //直線化、制御点距離は0なので現在アンカー点と同じ座標
        private (Point begin, Point end) ControlPointsA0(Point beginSide, Point current, Point endSide, double curve)
        {
            return (current, current);
        }

        /// <summary>
        /// 前後のアンカー点から現在のアンカー点の制御点座標を求める
        /// </summary>
        /// <param name="beginP">始点側アンカー点</param>
        /// <param name="currentP">現在アンカー点</param>
        /// <param name="endP">終点側アンカー点</param>
        /// <param name="curve">制御点距離、前後のアンカー点間距離をこの数値で割る、0.3が適当、0で直線、大きくすると曲がる</param>
        /// <returns>始点側制御点座標、終点側制御点座標</returns>
        private (Point begin, Point end) ControlPointsA1(Point beginSide, Point current, Point endSide, double curve)
        {
            double xDiff = (endSide.X - beginSide.X) * curve;
            double yDiff = (endSide.Y - beginSide.Y) * curve;
            Point bPoint = new Point(current.X - xDiff, current.Y - yDiff);
            Point ePoint = new Point(current.X + xDiff, current.Y + yDiff);
            return (bPoint, ePoint);
        }

        //前後のアンカー点それぞれとの距離に比例
        private (Point begin, Point end) ControlPointsA2(Point beginSide, Point current, Point endSide, double curve)
        {
            //距離
            double bDistance = GetDistance(beginSide, current);
            double eDistance = GetDistance(endSide, current);
            //全体距離は始点側アンカー点と終点側アンカー点の距離
            double distance = GetDistance(beginSide, endSide);
            //全体距離で割って割合
            double bRatio = bDistance / distance;
            double eRatio = eDistance / distance;

            double xDiff = (endSide.X - beginSide.X) * curve;
            double yDiff = (endSide.Y - beginSide.Y) * curve;
            Point bPoint = new Point(current.X - (xDiff * bRatio), current.Y - (yDiff * bRatio));
            Point ePoint = new Point(current.X + (xDiff * eRatio), current.Y + (yDiff * eRatio));
            return (bPoint, ePoint);
        }

        //短い方に合わせる
        private (Point begin, Point end) ControlPointsA3(Point beginSide, Point current, Point endSide, double curve)
        {
            //距離
            double bDistance = GetDistance(beginSide, current);
            double eDistance = GetDistance(endSide, current);
            //全体距離は始点側アンカー点と終点側アンカー点の距離
            double distance = GetDistance(beginSide, endSide);
            //全体距離で割って割合
            double bRatio = bDistance / distance;
            double eRatio = eDistance / distance;
            //短い方の割合を使う
            double ratio = (bRatio > eRatio) ? eRatio : bRatio;

            double xDiff = (endSide.X - beginSide.X) * curve * ratio;
            double yDiff = (endSide.Y - beginSide.Y) * curve * ratio;
            Point bPoint = new Point(current.X - xDiff, current.Y - yDiff);
            Point ePoint = new Point(current.X + xDiff, current.Y + yDiff);
            return (bPoint, ePoint);
        }



        //指定した長さ
        private (Point begin, Point end) ControlPointsB1(Point beginSide, Point current, Point endSide, double distance)
        {
            double xDiff = (endSide.X - beginSide.X);
            double yDiff = (endSide.Y - beginSide.Y);
            double radian = GetRadianFrom2Points(new Point(), new Point(xDiff, yDiff));
            double cos = Math.Cos(radian) * distance;
            double sin = Math.Sin(radian) * distance;

            Point bPoint = new Point(current.X - cos, current.Y - sin);
            Point ePoint = new Point(current.X + cos, current.Y + sin);
            return (bPoint, ePoint);
        }

        #endregion




        private PointCollection RandomPoint(int count)
        {
            var rand = new Random();
            var points = new PointCollection();
            for (int i = 0; i < count; i++)
            {
                points.Add(new Point(rand.Next(50, 450), rand.Next(50, 450)));
            }
            return points;
        }

        //ベジェ曲線Pathを追加
        private void AddBezierPath()
        {
            MyBezierPath = MakeBezierPath(AnchorPoints);
            ToCurve(ControlPointsA1, MyBezierPath, AnchorPoints, 0.3);
            MyCanvas.Children.Add(MyBezierPath);
            MyBezierPath.Stroke = Brushes.Khaki;
            MyBezierPath.StrokeThickness = 10;
            MyBezierPath.StrokeLineJoin = PenLineJoin.Bevel;

        }

        //確認用方向線表示
        private void AddLinePath()
        {
            PointCollection segmentPoint = GetPolyBezierSegmentPoints(MyBezierPath);
            var pg = new PathGeometry();
            for (int i = 2; i < segmentPoint.Count - 1; i += 3)
            {
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i - 1]));
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i + 1]));
            }
            ControlLinePath = new Path();
            ControlLinePath.Data = pg;
            ControlLinePath.Stroke = Brushes.DeepPink;
            MyCanvas.Children.Add(ControlLinePath);
        }

        //確認用座標点表示
        private void AddVertexPath()
        {
            var ps = GetPolyBezierSegmentPoints(MyBezierPath);
            var pg = new PathGeometry();
            for (int i = 0; i < ps.Count; i++)
            {
                var ep = new EllipseGeometry(ps[i], 4, 4);
                pg.AddGeometry(ep);
            }
            VertexPath = new Path();
            VertexPath.Data = pg;
            VertexPath.Stroke = Brushes.DeepPink;

            MyCanvas.Children.Add(VertexPath);
        }


        //PointCollectionのPoint間の総距離を取得
        private double GetTotalDistance(PointCollection anchorPoints)
        {
            double distance = 0;
            for (int i = 0; i < anchorPoints.Count - 1; i++)
            {
                distance += GetDistance(anchorPoints[i], anchorPoints[i + 1]);
            }
            return distance;
        }


        //ベジェ曲線PathからSegmentのPointsを取得
        private PointCollection GetPolyBezierSegmentPoints(Path bezierPath)
        {
            var pg = (PathGeometry)bezierPath.Data;
            PathFigure fig = pg.Figures[0];
            var seg = (PolyBezierSegment)fig.Segments[0];
            return seg.Points;
        }

        //アンカー点からベジェ曲線用のセグメントを作成
        private PolyBezierSegment MakePolyBezierSegment(PointCollection anchorPoints)
        {
            var seg = new PolyBezierSegment();
            seg.Points.Add(anchorPoints[0]);//始点の制御点
            for (int i = 1; i < anchorPoints.Count - 1; i++)
            {
                seg.Points.Add(anchorPoints[i]);//制御点
                seg.Points.Add(anchorPoints[i]);//アンカー点
                seg.Points.Add(anchorPoints[i]);//制御点
            }
            seg.Points.Add(anchorPoints[anchorPoints.Count - 1]);//終点の制御点
            seg.Points.Add(anchorPoints[anchorPoints.Count - 1]);//終点
            return seg;
        }

        //アンカー点からベジェ曲線Pathを作成
        private Path MakeBezierPath(PointCollection points)
        {
            var fig = new PathFigure();
            fig.StartPoint = points[0];//始点
            fig.Segments.Add(MakePolyBezierSegment(points));//セグメント
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            var path = new Path();
            path.Data = pg;
            return path;
        }

        //2点間線分のラジアンを取得
        private double GetRadianFrom2Points(Point begin, Point end)
        {
            return Math.Atan2(end.Y - begin.Y, end.X - begin.X);
        }
        //2点間距離を取得
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2.0) + Math.Pow(p2.Y - p1.Y, 2.0));
        }

    }
}
