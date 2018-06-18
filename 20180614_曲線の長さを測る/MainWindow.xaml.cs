using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
//曲線のPathをGetFlattenedPathGeometryで近似直線Pathに変換して
//それをGetPointAtFractionLengthで分割した頂点同士の距離を合計する

//    ベジェ曲線の長さ測定できた、C#とWPF ( ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15554888.html

namespace _20180614_曲線の長さを測る
{
    public partial class MainWindow : Window
    {
        PathGeometry FlattenedPathGeometry;//曲線の近似直線に変換したPathGeometry
        List<Path> StepEllipseList = new List<Path>();//分割したところの○印
        List<Path> MyListLine = new List<Path>();//○印を結ぶ直線Path

        public MainWindow()
        {
            InitializeComponent();

            FlattenedPathGeometry = MyPath.Data.GetFlattenedPathGeometry();
            FlattendeLine.Data = FlattenedPathGeometry;

            SliderStep.ValueChanged += SliderStep_ValueChanged;
            SliderTolerance.ValueChanged += SliderTolerance_ValueChanged;
            SliderStep.MouseWheel += Slider_MouseWheel;
            SliderTolerance.MouseWheel += Slider_MouseWheel;

        }

        //マウスホイール回したとき
        private void Slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Slider slider = (Slider)sender;
            if (e.Delta > 0) { slider.Value += slider.SmallChange; }
            else { slider.Value -= slider.SmallChange; }
        }

        //公差変更時
        private void SliderTolerance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeFlattendePath();
            Measure();
        }

        //分割数変更時
        private void SliderStep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Measure();
        }

        //長さ測定
        private void Measure()
        {
            ClearStepEllipse();
            var pg = (PathGeometry)MyPath.Data;
            Point p1 = pg.Figures[0].StartPoint;
            Point p2;
            double pathLength = 0;//長さ測定用
            double step = SliderStep.Value;
            for (int i = 0; i < step + 1; i++)
            {
                //○印の作成と追加
                //第1引数は0から1を指定、0がPathの始点になり1は終点、0.5なら中間位置を取得できる
                FlattenedPathGeometry.GetPointAtFractionLength(i / step, out Point p, out Point pt);
                var ellipseGeo = new EllipseGeometry(p, 4, 4);
                var path = new Path();
                path.Stroke = Brushes.Blue;
                path.Data = ellipseGeo;
                MyGrid.Children.Add(path);
                StepEllipseList.Add(path);

                //長さ測定
                p2 = p;
                pathLength += Distance(p1, p2);
                //測定対象の2頂点を結ぶ直線Pathの作成と追加
                var lineG = new LineGeometry(p1, p2);
                p1 = p;
                path = new Path();
                path.Stroke = Brushes.Red;
                path.Data = lineG;
                MyGrid.Children.Add(path);
                MyListLine.Add(path);
            }
            //表示更新
            TextBlockMeasure.Text =
                "測定値 = " + Math.Round(pathLength, 1, MidpointRounding.AwayFromZero).ToString();
        }

        //2点間の距離、ユークリッド距離
        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        //表示をクリア
        private void ClearStepEllipse()
        {
            foreach (var item in StepEllipseList)
            {
                MyGrid.Children.Remove(item);
            }
            StepEllipseList.Clear();

            foreach (var item in MyListLine)
            {
                MyGrid.Children.Remove(item);
            }
            MyListLine.Clear();
        }

        //近似直線の更新
        private void ChangeFlattendePath()
        {
            //第1引数Toleranceは0以上を指定、0が一番曲線に近く正確だけど計算量が増える
            FlattenedPathGeometry =
                MyPath.Data.GetFlattenedPathGeometry(SliderTolerance.Value, ToleranceType.Absolute);
            FlattendeLine.Data = FlattenedPathGeometry;
                    
        }

    }
}
