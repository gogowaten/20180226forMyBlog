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

namespace _20181104_マウスで引いた線をベジェ曲線4
{
    //方向線の長さの決め方の種類
    public enum DistanceType
    {
        Zero0距離,
        Average平均,
        Separate別々,
        Shorter短いほう,
        FrontBack前後間
    }
    public enum RadianType
    {
        Parallel平行,
        RightAngleOfCenter中間の直角
    }
    /// <summary>
    /// WPF、マウスドラッグ移動でなめらかな曲線を描画、PolyBezierSegment ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
    ///https://blogs.yahoo.co.jp/gogowaten/15753225.html
    /// </summary>
    public partial class MainWindow : Window
    {
        bool IsDrawing = false;//マウスで描画中フラグ
        Polyline TempPolyline;//マウスで描画中のPolyline
        //ベジェ曲線の管理用
        List<MyBezier> MyListBezierPaths = new List<MyBezier>();


        public MainWindow()
        {
            InitializeComponent();
            this.Title = this.ToString();

            //マウスイベント系
            MyCanvas.MouseMove += MyCanvas_MouseMove;
            MyCanvas.MouseRightButtonDown += MyCanvas_MouseRightButtonDown;
            MyCanvas.PreviewMouseRightButtonUp += MyCanvas_PreviewMouseRightButtonUp;

            //その他イベント
            //ButtonTest.Click += ButtonTest_Click;
            ButtonToClipboard.Click += ButtonToClipboard_Click; ;
            ButtonClear.Click += ButtonClear_Click;

            SliderInterval.ValueChanged += SliderInterval_ValueChanged;
            SliderCurve.ValueChanged += SliderCurve_ValueChanged;

            CheckBoxVisible.Checked += CheckBoxVisible_Checked;
            CheckBoxVisible.Unchecked += CheckBoxVisible_Unchecked;

            ComboBoxDistanceType.SelectionChanged += ComboBoxDistanceType_SelectionChanged;
            ComboBoxRadianType.SelectionChanged += ComboBoxRadianType_SelectionChanged;

            InitializeComboBox();//ComboBox初期化

            ButtonTest.Click += ButtonTest_Click;
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            MyListBezierPaths[0].test();
        }

        private void ComboBoxRadianType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyListBezierPaths.Count < 1) { return; }

            var cb = (ComboBox)sender;
            var rType = (RadianType)cb.SelectedItem;
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].RadianType = rType;
            }
        }

        private void ComboBoxDistanceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyListBezierPaths.Count < 1) { return; }

            var cb = (ComboBox)sender;
            var dType = (DistanceType)cb.SelectedItem;
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].DistanceType = dType;
            }
        }

        private void CheckBoxVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].DirectionLinePath.Visibility = Visibility.Hidden;
                MyListBezierPaths[i].VertexPath.Visibility = Visibility.Hidden;
            }
        }

        private void CheckBoxVisible_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].DirectionLinePath.Visibility = Visibility.Visible;
                MyListBezierPaths[i].VertexPath.Visibility = Visibility.Visible;
            }
        }

        private void SliderCurve_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].Curve = e.NewValue;
            }
        }

        private void SliderInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyListBezierPaths[i].Interval = (int)e.NewValue;
            }
        }



        #region ボタンイベント


        //画面クリア
        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < MyListBezierPaths.Count; i++)
            {
                MyCanvas.Children.Remove(MyListBezierPaths[i].BezierPath);
                MyCanvas.Children.Remove(MyListBezierPaths[i].VertexPath);
                MyCanvas.Children.Remove(MyListBezierPaths[i].DirectionLinePath);
            }
        }
        private void ButtonToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CanvasImageToClipboard(MyCanvas);
        }
        #endregion


        #region マウスイベント関連
        //マウス右クリックし終わったとき
        private void MyCanvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndDraw();//マウスで線を引いていたのを止める
        }

        //マウスで線を引いていたのを止める
        private void EndDraw()
        {
            IsDrawing = false;
            MyCanvas.ReleaseMouseCapture();//Canvasからマウス開放
            if (TempPolyline == null) { return; }
            //polyline
            if (TempPolyline.Points.Count > 1)
            {
                //PolylinePathからベジェ曲線Pathを作成してリストに追加
                Visibility visibility = (bool)(CheckBoxVisible.IsChecked) ? Visibility.Visible : Visibility.Hidden;
                var myBezier = new MyBezier(TempPolyline.Points, SliderCurve.Value, (int)SliderInterval.Value,
                    (DistanceType)ComboBoxDistanceType.SelectedItem,
                    (RadianType)ComboBoxRadianType.SelectedItem,
                    visibility);
                MyListBezierPaths.Add(myBezier);
                MyCanvas.Children.Add(myBezier.BezierPath);
                MyCanvas.Children.Add(myBezier.VertexPath);
                MyCanvas.Children.Add(myBezier.DirectionLinePath);

                MyCanvas.Children.Remove(TempPolyline);

            }
            TempPolyline = null;
        }

        //マウス右クリックしたとき
        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mp = e.GetPosition(MyCanvas);
            //マウスがCanvas外に出でも感知できるように
            MyCanvas.CaptureMouse();

            //polyline
            TempPolyline = InitializePolyline();
            TempPolyline.Points.Add(mp);
            MyCanvas.Children.Add(TempPolyline);

            IsDrawing = true;//描画フラグ

        }

        //マウス移動中
        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //アプリのウィンドウがアクティブじゃなくなったら描画終了処理
            if (this.IsActive == false && IsDrawing == true)
            {
                EndDraw();//マウスで線を引いていたのを止める
                return;
            }

            //ドラッグなら座標追加
            if (e.RightButton == MouseButtonState.Pressed & IsDrawing == true)
            {
                Point p = e.GetPosition(MyCanvas);
                //polyline
                TempPolyline.Points.Add(p);
            }
        }
        #endregion

        #region その他メソッド

        //Canvasを画像としてクリップボードにコピー
        private void CanvasImageToClipboard(Canvas canvas)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            Clipboard.SetImage(bitmap);
        }
        //Polyline初期化
        private Polyline InitializePolyline()
        {
            var poliline = new Polyline
            {
                Stroke = Brushes.Red,
                StrokeThickness = 10,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
            };
            return poliline;
        }

        //ComboBox初期化
        private void InitializeComboBox()
        {
            ComboBoxDistanceType.ItemsSource = Enum.GetValues(typeof(DistanceType));
            ComboBoxDistanceType.SelectedItem = DistanceType.Zero0距離;
            ComboBoxDistanceType.SelectedItem = DistanceType.Average平均;
            ComboBoxRadianType.ItemsSource = Enum.GetValues(typeof(RadianType));
            ComboBoxRadianType.SelectedItem = RadianType.Parallel平行;
        }
        #endregion

    }





    public class MyBezier
    {
        public MyBezier(PointCollection points, double curve, int interval,
            DistanceType distanceType, RadianType radianType, Visibility vertexAndDirectionVisible)
        {
            OriginAnchorPoint = points;
            AnchorPoints = ChoiceAnchorPoint(points, interval);
            Curve = curve;
            Interval = interval;
            DistanceType = distanceType;
            RadianType = radianType;

            //ベジェ曲線Path作成
            BezierPath = InitializeBezierPath();//Pathの初期化
            BezierPath.Data = MakeBezierPathGeometry(AnchorPoints);//PathのData作成
            ToCurve(BezierPath, curve, distanceType, radianType);//制御点座標決定
            //方向線Path作成
            DirectionLinePath = InitializeDirectionLinePath(vertexAndDirectionVisible);
            DirectionLinePath.Data = MakeDirectionLinePathGeometry(BezierPath);
            //頂点Path作成
            VertexPath = InitializeVertexPath(vertexAndDirectionVisible);
            VertexPath.Data = MakeVertexPathGeometry(BezierPath);

        }

        #region property
        public Path BezierPath { get; set; }
        public Path DirectionLinePath { get; set; }
        public Path VertexPath { get; set; }
        public PointCollection OriginAnchorPoint { get; private set; }//元のアンカー点
        public PointCollection AnchorPoints { get; private set; }//選択後のアンカー点

        private double _Curve;
        public double Curve
        {
            get { return _Curve; }
            set
            {
                _Curve = value;
                if (BezierPath != null)
                {
                    ToCurve(BezierPath, value, _DistanceType, _RadianType);//制御点座標決定
                    //DirectionLinePath.Data = MakeDirectionLinePathGeometry(BezierPath);
                    RenewDirectionLinePath(BezierPath, DirectionLinePath);
                    VertexPath.Data = MakeVertexPathGeometry(BezierPath);
                }
            }
        }

        private int _Interval;
        public int Interval
        {
            get { return _Interval; }
            set
            {
                _Interval = value;
                if (BezierPath != null)
                {
                    AnchorPoints = ChoiceAnchorPoint(OriginAnchorPoint, value);
                    BezierPath.Data = MakeBezierPathGeometry(AnchorPoints);
                    ToCurve(BezierPath, _Curve, _DistanceType, _RadianType);
                    DirectionLinePath.Data = MakeDirectionLinePathGeometry(BezierPath);
                    //RenewDirectionLinePath(BezierPath, DirectionLinePath);
                    VertexPath.Data = MakeVertexPathGeometry(BezierPath);
                }
            }
        }

        private DistanceType _DistanceType;
        public DistanceType DistanceType
        {
            get { return _DistanceType; }
            set
            {
                _DistanceType = value;
                if (BezierPath != null)
                {
                    ToCurve(BezierPath, _Curve, value, _RadianType);//制御点座標決定
                    //DirectionLinePath.Data = MakeDirectionLinePathGeometry(BezierPath);
                    RenewDirectionLinePath(BezierPath, DirectionLinePath);
                    VertexPath.Data = MakeVertexPathGeometry(BezierPath);
                }
            }
        }

        private RadianType _RadianType;
        public RadianType RadianType
        {
            get { return _RadianType; }
            set
            {
                _RadianType = value;
                if (BezierPath != null)
                {
                    ToCurve(BezierPath, _Curve, _DistanceType, value);//制御点座標決定
                    //DirectionLinePath.Data = MakeDirectionLinePathGeometry(BezierPath);
                    RenewDirectionLinePath(BezierPath, DirectionLinePath);
                    VertexPath.Data = MakeVertexPathGeometry(BezierPath);
                }
            }
        }


        #endregion

        //制御点座標を決めて曲線化
        private void ToCurve(Path bezierPath, double curve, DistanceType distanceType, RadianType radianType)
        {
            PointCollection segPoints = GetPolyBezierSegmentPoints(bezierPath);
            for (int i = 1; i < AnchorPoints.Count - 1; i++)
            {
                Point beginAnchor = AnchorPoints[i - 1];
                Point currentAnchor = AnchorPoints[i];
                Point endAnchor = AnchorPoints[i + 1];
                //方向線距離
                if (radianType == RadianType.RightAngleOfCenter中間の直角)
                {
                    double beginDistance = 0, endDistance = 0;
                    switch (distanceType)
                    {
                        case DistanceType.Zero0距離:
                            break;
                        case DistanceType.Average平均:
                            (beginDistance, endDistance) = DistanceAverage(beginAnchor, currentAnchor, endAnchor);
                            break;
                        case DistanceType.Separate別々:
                            (beginDistance, endDistance) = DistanceSeparate(beginAnchor, currentAnchor, endAnchor);
                            break;
                        case DistanceType.Shorter短いほう:
                            (beginDistance, endDistance) = DistanceShorter(beginAnchor, currentAnchor, endAnchor);
                            break;
                        case DistanceType.FrontBack前後間:
                            (beginDistance, endDistance) = DistanceFrontAndBackAnchor(beginAnchor, currentAnchor, endAnchor);
                            break;
                        default:
                            break;
                    }

                    //方向線弧度取得
                    (double bRadian, double eRadian) = GetRadianDirectionLine(beginAnchor, currentAnchor, endAnchor);
                    //(double bRadian, double eRadian) = SharpTest(beginAnchor, currentAnchor, endAnchor);
                    //始点側制御点座標
                    double xDiff = Math.Cos(bRadian) * beginDistance * curve;
                    double yDiff = Math.Sin(bRadian) * beginDistance * curve;
                    segPoints[i * 3 - 2] = new Point(currentAnchor.X + xDiff, currentAnchor.Y + yDiff);
                    //終点側制御点座標
                    xDiff = Math.Cos(eRadian) * endDistance * curve;
                    yDiff = Math.Sin(eRadian) * endDistance * curve;
                    segPoints[i * 3] = new Point(currentAnchor.X + xDiff, currentAnchor.Y + yDiff);
                }
                else if (radianType == RadianType.Parallel平行)
                {
                    Point bSide = currentAnchor;
                    Point eSide = currentAnchor;
                    switch (distanceType)
                    {
                        case DistanceType.Zero0距離:
                            break;
                        case DistanceType.Average平均:
                            (bSide, eSide) = ControlPointsAverage(beginAnchor, currentAnchor, endAnchor, curve);
                            break;
                        case DistanceType.Separate別々:
                            (bSide, eSide) = ControlPointsSeparate(beginAnchor, currentAnchor, endAnchor, curve);
                            break;
                        case DistanceType.Shorter短いほう:
                            (bSide, eSide) = ControlPointsShorter(beginAnchor, currentAnchor, endAnchor, curve);
                            break;
                        case DistanceType.FrontBack前後間:
                            (bSide, eSide) = ControlPointsFrontAndBack(beginAnchor, currentAnchor, endAnchor, curve);
                            break;
                        default:
                            break;
                    }
                    segPoints[i * 3 - 2] = bSide;
                    segPoints[i * 3] = eSide;
                }
            }

        }



        #region 平行
        /// <summary>
        /// 前後のアンカー点から現在のアンカー点の制御点座標を求める
        /// </summary>
        /// <param name="beginP">始点側アンカー点</param>
        /// <param name="currentP">現在アンカー点</param>
        /// <param name="endP">終点側アンカー点</param>
        /// <param name="curve">制御点距離、前後のアンカー点間距離をこの数値で割る、0.1~0.3が適当、0で直線、大きくすると曲がる</param>
        /// <returns>始点側制御点座標、終点側制御点座標</returns>
        private (Point begin, Point end) ControlPointsFrontAndBack(Point beginSide, Point current, Point endSide, double curve)
        {
            double xDiff = (endSide.X - beginSide.X) * curve;
            double yDiff = (endSide.Y - beginSide.Y) * curve;
            Point bPoint = new Point(current.X - xDiff, current.Y - yDiff);
            Point ePoint = new Point(current.X + xDiff, current.Y + yDiff);
            return (bPoint, ePoint);
        }

        //前後のアンカー点それぞれとの距離に比例
        private (Point begin, Point end) ControlPointsSeparate(Point beginSide, Point current, Point endSide, double curve)
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
        private (Point begin, Point end) ControlPointsShorter(Point beginSide, Point current, Point endSide, double curve)
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

        //これだけ間違っているかも
        //前後のアンカー点距離の平均
        private (Point begin, Point end) ControlPointsAverage(Point beginSide, Point current, Point endSide, double curve)
        {
            //距離
            double bDistance = GetDistance(beginSide, current);
            double eDistance = GetDistance(endSide, current);
            double average = (bDistance + eDistance) / 2.0;
            //割合
            double bRatio = bDistance / average;
            double eRatio = eDistance / average;

            double xDiff = (endSide.X - beginSide.X) * curve;
            double yDiff = (endSide.Y - beginSide.Y) * curve;
            Point bPoint = new Point(current.X - (xDiff * bRatio), current.Y - (yDiff * bRatio));
            Point ePoint = new Point(current.X + (xDiff * eRatio), current.Y + (yDiff * eRatio));
            return (bPoint, ePoint);
        }
        #endregion


        #region 中間の直角で使う
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
            //始点側角度＞終点側角度のときは始点側に90度を足して、終点側は90度引く
            //逆のときは足し引きも逆になる
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

        //未使用、鋭角
        private (double beginSide,double endSide) SharpTest(Point beginP,Point currentP,Point endP)
        {
            double sharpness = 4.0;//尖り具合、2.0以上を指定、小さい方がより尖る、3～4くらいがよく尖る
            //注目アンカーから始点側アンカーへの弧度取得
            double b1 = GetRadianFrom2Points(currentP, beginP);//mid->begin
            double b2 = b1;
            if (b1 < 0) { b2 = b1 + Math.PI * 2.0; }//マイナスだったらPI*2(円1周分)を足す
            double beginSideR = b1;//どちらを使うかまだわからないのでひとまず最初の方を入れておく

            //注目アンカーから終点側アンカーへの弧度取得
            var e1 = GetRadianFrom2Points(currentP, endP);//mid->end
            double e2 = e1;
            if (e1 < 0) { e2 = Math.PI * 2 + e1; }
            double endSideR = e1;

            //弧度差の選択
            double d1 = Math.Abs(b1 - e1);//普通の弧度差
            double d2 = Math.Abs(b2 - e2);//マイナスだった場合の弧度差
            double diffRadian = d1 / sharpness;//尖り具合
            //2つの弧度の差の小さかったほうを使う
            if (d1 > d2)
            {
                diffRadian = d2 / sharpness;
                beginSideR = b2;
                endSideR = e2;
            }

            //新しい弧度を求める
            double nBeginSideRadian, nEndSideRadian;
            //大きい方からは弧度差を引いて、小さい方には足す
            if (beginSideR > endSideR)
            {
                nBeginSideRadian = beginSideR - diffRadian;
                nEndSideRadian = endSideR + diffRadian;
            }
            else
            {
                nBeginSideRadian = beginSideR + diffRadian;
                nEndSideRadian = endSideR - diffRadian;
            }
            return (nBeginSideRadian, nEndSideRadian);
        }
        #endregion


        //指定間隔で選んだアンカー点を返す
        private PointCollection ChoiceAnchorPoint(PointCollection points, int interval)
        {
            var selectedPoints = new PointCollection();
            for (int i = 0; i < points.Count - 1; i += interval)
            {
                selectedPoints.Add(points[i]);
            }
            selectedPoints.Add(points[points.Count - 1]);//最後の一個は必ず入れる

            //選んだ頂点が3個以上あって、最後の頂点と最後から2番めが近いときは2番めを除去            
            if (selectedPoints.Count >= 3)
            {
                int mod = (points.Count - 2) % interval;
                if (interval / 2 > mod)
                {
                    selectedPoints.RemoveAt(selectedPoints.Count - 2);//除去
                }
            }
            return selectedPoints;
        }

        #region 表示更新
        //方向線表示用のPathの表示更新
        private void RenewDirectionLinePath(Path bezier, Path directionLine)
        {
            PointCollection beziPoint = GetPolyBezierSegmentPoints(bezier);
            var pg = (PathGeometry)directionLine.Data;
            var fig = pg.Figures;
            int count = 1;
            for (int i = 0; i < fig.Count; i += 2)
            {
                LineSegment ls;
                fig[i].StartPoint = beziPoint[count * 3 - 1];
                fig[i + 1].StartPoint = beziPoint[count * 3 - 1];

                ls = (LineSegment)fig[i].Segments[0];
                ls.Point = beziPoint[count * 3 - 2];
                ls = (LineSegment)fig[i + 1].Segments[0];
                ls.Point = beziPoint[count * 3];
                count++;
            }
        }


        #endregion


        #region PathGeometry(Path.Dara)作成
        //アンカー点となるPointCollectionからベジェ曲線のPathGeometryを作成
        private PathGeometry MakeBezierPathGeometry(PointCollection pc)
        {
            //PolyBezierSegment作成
            //PointCollectionをアンカー点に見立てて、その制御点を追加していく
            PolyBezierSegment seg = new PolyBezierSegment();
            seg.Points.Add(pc[0]);//始点制御点
            for (int i = 1; i < pc.Count - 1; i++)
            {
                seg.Points.Add(pc[i]);//制御点
                seg.Points.Add(pc[i]);//アンカー
                seg.Points.Add(pc[i]);//制御点
            }
            seg.Points.Add(pc[pc.Count - 1]);//終点制御点
            seg.Points.Add(pc[pc.Count - 1]);//終点アンカー

            //
            var fig = new PathFigure();
            fig.StartPoint = pc[0];
            fig.Segments.Add(seg);
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return pg;
        }


        //座標点表示用PathGeometry作成
        private PathGeometry MakeVertexPathGeometry(Path bezier)
        {
            PointCollection segmentPoint = GetPolyBezierSegmentPoints(bezier);
            var pg = new PathGeometry();
            pg.FillRule = FillRule.Nonzero;//塗り方指定、Path同士が重なったときにも塗りつぶす
            for (int i = 0; i < segmentPoint.Count; i++)
            {
                var ep = new EllipseGeometry(segmentPoint[i], 4, 4);
                pg.AddGeometry(ep);
            }
            return pg;
        }
        public void test()
        {
            var pg = (PathGeometry)VertexPath.Data;
            var fig = pg.Figures;
            int fc = fig.Count;
            var ff = fig[0];
            var ff1 = fig[1];
            var seg = ff.Segments[0];
            var seg2 = fig[1].Segments[0];


        }
        //方向線表示用PathGeometry作成
        private PathGeometry MakeDirectionLinePathGeometry(Path bezier)
        {
            PointCollection segmentPoint = GetPolyBezierSegmentPoints(bezier);
            var pg = new PathGeometry();
            for (int i = 2; i < segmentPoint.Count - 1; i += 3)
            {
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i - 1]));
                pg.AddGeometry(new LineGeometry(segmentPoint[i], segmentPoint[i + 1]));
            }
            return pg;
        }


        #endregion


        #region 初期化
        //ベジェ曲線初期設定
        private Path InitializeBezierPath()
        {
            var path = new Path
            {
                Stroke = Brushes.MediumPurple,
                StrokeThickness = 20,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
            };
            return path;
        }

        //頂点表示用Path初期化
        private Path InitializeVertexPath(Visibility visibility)
        {
            var path = new Path()
            {
                Data = new PathGeometry(),
                Stroke = Brushes.DeepPink,
                Fill = Brushes.Yellow,
                Visibility = visibility
            };
            return path;
        }
        //方向線表示用Path初期化
        private Path InitializeDirectionLinePath(Visibility visibility)
        {
            var path = new Path()
            {
                Data = new PathGeometry(),
                Stroke = Brushes.DeepPink,
                Visibility = visibility
            };

            return path;
        }
        #endregion


        #region その他メソッド
        //ベジェ曲線PathからSegmentのPointsを取得
        private PointCollection GetPolyBezierSegmentPoints(Path bezierPath)
        {
            var pg = (PathGeometry)bezierPath.Data;
            PathFigure fig = pg.Figures[0];
            var seg = (PolyBezierSegment)fig.Segments[0];
            return seg.Points;
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



        #endregion
    }
}
