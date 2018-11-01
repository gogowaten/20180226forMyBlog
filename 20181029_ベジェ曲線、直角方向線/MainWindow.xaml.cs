using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

//WPF、ベジェ曲線、違和感なく滑らかになるような制御点座標はどこ？その3(終) (ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15735391.html

namespace _20181029_ベジェ曲線_直角方向線
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //方向線の長さの決め方の種類
        public enum DistanceType
        {
            None,
            Average,
            Separate,
            Shorter,
            FrontBack

        }

        private PointCollection AnchorPoints;//ベジェ曲線Pathのアンカー点
        private Path MyBezierPath = new Path();//ベジェ曲線Path
        private Path VertexPath = new Path();//頂点表示用
        private Path ControlLinePath = new Path();//確認用方向線表示用

        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();
            ButtonA0.Click += ButtonA0_Click;
            ButtonC1.Click += ButtonC1_Click;
            ButtonC2.Click += ButtonC2_Click;
            ButtonC3.Click += ButtonC3_Click;
            ButtonC4.Click += ButtonC4_Click;
            ButtonC5.Click += ButtonC5_Click;
            ButtonA1.Click += ButtonA1_Click;
            ButtonRandomPoint.Click += ButtonRandomPoint_Click;
            ButtonVisible.Click += ButtonVisible_Click;

            AnchorPoints = new PointCollection() { new Point(150, 250), new Point(100, 250), new Point(400, 350), new Point(200, 250) };
            AnchorPoints = new PointCollection() { new Point(100, 300), new Point(200, 400), new Point(400, 300) };
            AnchorPoints = new PointCollection() { new Point(150, 250), new Point(100, 300), new Point(400, 400) };
            //AnchorPoints = new PointCollection() { new Point(150, 100), new Point(200, 150), new Point(150, 200), new Point(100, 150), new Point(150, 100) };
            //AnchorPoints = new PointCollection() { new Point(100, 250), new Point(200, 250), new Point(350, 350), new Point(350, 250) };
            //AnchorPoints = new PointCollection() { new Point(100, 250), new Point(200, 250), new Point(350, 350) };
            InitializeBezierPath();//ベジェ曲線Path追加
            InitializeVertexPath();//確認用座標点表示
            InitializeLinePath();//確認用方向線表示

            MyCanvas.Children.Add(MyBezierPath);
            MyCanvas.Children.Add(VertexPath);
            MyCanvas.Children.Add(ControlLinePath);
        }

        private void ButtonC5_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeC(0.5, DistanceType.Shorter);
            InitializeVertexPath();
            InitializeLinePath();
        }

        private void ButtonC4_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeC(0.3, DistanceType.FrontBack);
            InitializeVertexPath();
            InitializeLinePath();
        }


        private void ButtonC3_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeC(0.3, DistanceType.Shorter);
            InitializeVertexPath();
            InitializeLinePath();
        }

        private void ButtonC2_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeC(0.3, DistanceType.Separate);
            InitializeVertexPath();
            InitializeLinePath();
        }

        private void ButtonC1_Click(object sender, RoutedEventArgs e)
        {

            ToCurveTypeC(0.3, DistanceType.Average);
            InitializeVertexPath();
            InitializeLinePath();
        }

        private void ButtonA1_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeA(0.3);
            InitializeVertexPath();
            InitializeLinePath();
        }

        private void ButtonA0_Click(object sender, RoutedEventArgs e)
        {
            ToCurveTypeC(0.3, DistanceType.None);
            InitializeVertexPath();
            InitializeLinePath();
        }

        //頂点と方向線の表示の有無切り替え
        private void ButtonVisible_Click(object sender, RoutedEventArgs e)
        {
            if (VertexPath.Visibility == Visibility.Visible)
            {
                VertexPath.Visibility = Visibility.Hidden;
                ControlLinePath.Visibility = Visibility.Hidden;
            }
            else
            {
                VertexPath.Visibility = Visibility.Visible;
                ControlLinePath.Visibility = Visibility.Visible;
            }
        }

        //ランダムアンカー
        private void ButtonRandomPoint_Click(object sender, RoutedEventArgs e)
        {
            AnchorPoints = RandomPoint(6);
            InitializeBezierPath();
            ToCurveTypeC(0.3, DistanceType.Average);
            InitializeVertexPath();
            InitializeLinePath();
        }



        //制御点座標を決めて曲線化
        private void ToCurveTypeC(double curve, DistanceType distanceType)
        {
            PointCollection segPoints = GetPolyBezierSegmentPoints(MyBezierPath);
            for (int i = 1; i < AnchorPoints.Count - 1; i++)
            {
                Point beginP = AnchorPoints[i - 1];
                Point currentP = AnchorPoints[i];
                Point endP = AnchorPoints[i + 1];
                //方向線距離
                double beginDistance = 0, endDistance = 0;
                switch (distanceType)
                {
                    case DistanceType.None:
                        beginDistance = 0;
                        endDistance = 0;
                        break;
                    case DistanceType.Average:
                        (beginDistance, endDistance) = DistanceAverage(beginP, currentP, endP);
                        break;
                    case DistanceType.Separate:
                        (beginDistance, endDistance) = DistanceSeparate(beginP, currentP, endP);
                        break;
                    case DistanceType.Shorter:
                        (beginDistance, endDistance) = DistanceShorter(beginP, currentP, endP);
                        break;
                    case DistanceType.FrontBack:
                        (beginDistance, endDistance) = DistanceFrontAndBackAnchor(beginP, currentP, endP);
                        break;
                    default:                        
                        break;
                }

                //方向線弧度取得
                (double bRadian, double eRadian) = GetRadianDirectionLine(beginP, currentP, endP);
                //始点側制御点座標
                double xDiff = Math.Cos(bRadian) * beginDistance * curve;
                double yDiff = Math.Sin(bRadian) * beginDistance * curve;
                segPoints[i * 3 - 2] = new Point(currentP.X + xDiff, currentP.Y + yDiff);
                //終点側制御点座標
                xDiff = Math.Cos(eRadian) * endDistance * curve;
                yDiff = Math.Sin(eRadian) * endDistance * curve;
                segPoints[i * 3] = new Point(currentP.X + xDiff, currentP.Y + yDiff);
            }

        }

        //アンカー点と制御点までの距離
        //前後のアンカー点の平均距離
        private (double begin, double end) DistanceAverage(Point beginP, Point currentP, Point endP)
        {
            double bSide = GetDistance(currentP, beginP);
            double eSide = GetDistance(currentP, endP);
            double average = (bSide + eSide) / 2.0;
            return (average, average);
        }
        //前後のアンカー点それぞれの距離
        private (double begin, double end) DistanceSeparate(Point beginP, Point currentP, Point endP)
        {
            double bSide = GetDistance(currentP, beginP);
            double eSide = GetDistance(currentP, endP);
            return (bSide, eSide);
        }
        //前後のアンカー点距離の短いほう
        private (double begin, double end) DistanceShorter(Point beginP, Point currentP, Point endP)
        {
            double bSide = GetDistance(currentP, beginP);
            double eSide = GetDistance(currentP, endP);
            double shorter = (bSide > eSide) ? eSide : bSide;
            return (shorter, shorter);
        }
        //前後のアンカー点間の距離
        private (double begin, double end) DistanceFrontAndBackAnchor(Point beginP, Point currentP, Point endP)
        {
            double distance = GetDistance(beginP, endP);
            return (distance, distance);
        }




        /// <summary>
        /// 現在アンカー点とその前後のアンカー点それぞれの中間弧度に直角な弧度を計算
        /// </summary>
        /// <param name="beginP">始点側アンカー点</param>
        /// <param name="currentP">現在アンカー点</param>
        /// <param name="endP">終点側アンカー点</param>
        /// <returns>始点側方向線弧度、終点側方向線弧度</returns>
        private (double beginSide, double endSide) GetRadianDirectionLine(Point beginP, Point currentP, Point endP)
        {
            //ラジアン(角度)
            double bRadian = GetRadianFrom2Points(currentP, beginP);//現在から始点側
            double eRadian = GetRadianFrom2Points(currentP, endP);//現在から終点側
            double midRadian = (bRadian + eRadian) / 2.0;//中間角度

            //中間角度に直角なのは90度を足した右回りと、90を引いた左回りがある
            //始点側、終点側角度を比較して大きい方が、右回りの方向線角度になる
            double bControlRadian, eControlRadian;
            if (bRadian > eRadian)
            {
                bControlRadian = midRadian + (Math.PI / 2.0);
                eControlRadian = midRadian - (Math.PI / 2.0);
            }
            else
            {
                bControlRadian = midRadian - (Math.PI / 2.0);
                eControlRadian = midRadian + (Math.PI / 2.0);
            }

            return (bControlRadian, eControlRadian);
        }

        //以前の方法
        private void ToCurveTypeA(double curve)
        {
            PointCollection segPoints = GetPolyBezierSegmentPoints(MyBezierPath);
            for (int i = 1; i < AnchorPoints.Count - 1; i++)
            {
                Point beginP = AnchorPoints[i - 1];
                Point currentP = AnchorPoints[i];
                Point endP = AnchorPoints[i + 1];

                double xDiff = (endP.X - beginP.X) * curve;
                double yDiff = (endP.Y - beginP.Y) * curve;
                segPoints[i * 3 - 2] = new Point(currentP.X - xDiff, currentP.Y - yDiff);
                segPoints[i * 3] = new Point(currentP.X + xDiff, currentP.Y + yDiff);
            }
        }





        //ベジェ曲線Pathを追加
        private void InitializeBezierPath()
        {
            MyBezierPath.Data = MakeBezierPahtGeometry(AnchorPoints);
            MyBezierPath.Stroke = Brushes.GreenYellow;
            MyBezierPath.StrokeThickness = 10;
            MyBezierPath.StrokeLineJoin = PenLineJoin.Bevel;

        }

        //確認用方向線表示
        private void InitializeLinePath()
        {
            PointCollection segmentPoint = GetPolyBezierSegmentPoints(MyBezierPath);
            var pg = new PathGeometry();
            for (int i = 2; i < segmentPoint.Count - 1; i += 3)
            {
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i - 1]));
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i + 1]));
            }
            ControlLinePath.Data = pg;
            ControlLinePath.Stroke = Brushes.DeepPink;
        }

        //確認用座標点表示
        private void InitializeVertexPath()
        {
            var ps = GetPolyBezierSegmentPoints(MyBezierPath);
            var pg = new PathGeometry();
            for (int i = 0; i < ps.Count; i++)
            {
                var ep = new EllipseGeometry(ps[i], 4, 4);
                pg.AddGeometry(ep);
            }

            VertexPath.Data = pg;
            VertexPath.Stroke = Brushes.DeepPink;
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

        //アンカー点からベジェ曲線用のPathGeometry作成
        private PathGeometry MakeBezierPahtGeometry(PointCollection points)
        {
            var fig = new PathFigure();
            fig.StartPoint = points[0];//始点
            fig.Segments.Add(MakePolyBezierSegment(points));//セグメント
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        //ランダムなPoint作成
        private PointCollection RandomPoint(int count)
        {
            var rand = new Random();
            var points = new PointCollection();
            for (int i = 0; i < count; i++)
            {
                points.Add(new Point(rand.Next(50, 450), rand.Next(250, 450)));
            }
            return points;
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

        //2点間距離を取得
        private double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2.0) + Math.Pow(p2.Y - p1.Y, 2.0));
        }
        //2点間線分のラジアン(弧度)を取得
        private double GetRadianFrom2Points(Point begin, Point end)
        {
            return Math.Atan2(end.Y - begin.Y, end.X - begin.X);
        }

    }
}
