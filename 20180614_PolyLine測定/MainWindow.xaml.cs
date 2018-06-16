using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
//GetPointAtFractionLengthで分割した座標からのPathの長さ測定の確認(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15551929.html

namespace _20180614_PolyLine測定
{
    public partial class MainWindow : Window
    {
        List<Path> MyListEllipse = new List<Path>();//○印用
        List<Label> MyListLabel = new List<Label>();//○印の角度表示用
        List<Path> MyListLine = new List<Path>();//○印を結ぶ赤線用

        public MainWindow()
        {
            InitializeComponent();

            MyInitialize();
        }

        //分割数変更時
        private void SliderStep_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeStep();
        }

        //いろいろ初期化
        private void MyInitialize()
        {
            ChangeStep();
            SliderStep.ValueChanged += SliderStep_ValueChanged;

            //Pathの各頂点から実際の長さ測定
            double pathLength = 0;
            var pg = (PathGeometry)MyPath.Data;
            Point p1 = pg.Figures[0].StartPoint;
            var ls = (PolyLineSegment)pg.Figures[0].Segments[0];
            for (int i = 0; i < ls.Points.Count; i++)
            {
                var p2 = ls.Points[i];
                pathLength += Distance(p1, p2);
                p1 = p2;
            }
            TextBlockTrueLength.Text =
                "Actually Path Lenght = " +
                Math.Round(pathLength, MidpointRounding.AwayFromZero).ToString();//四捨五入
        }

        //表示をクリア
        private void ClearStepObject()
        {
            foreach (var item in MyListLabel)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListLabel.Clear();
            foreach (var item in MyListEllipse)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListEllipse.Clear();
            foreach (var item in MyListLine)
            {
                MyCanvas.Children.Remove(item);
            }
            MyListLine.Clear();
        }

        //Step(分割)数変更時とかに実行
        private void ChangeStep()
        {
            ClearStepObject();//表示をクリア

            double step = SliderStep.Value;
            var pg = (PathGeometry)MyPath.Data;
            Point p, pt;
            Point p1 = pg.Figures[0].StartPoint;
            Point p2 = p1;
            double pathLength = 0;
            for (int i = 0; i < step + 1; i++)
            {
                //GetPointAtFractionLengthの第1引数は計測する位置の割合を0から1で指定する
                //Path全体からの割合なので、中間を取得したいなら0.5を指定する
                //1個めのoutは指定した位置の座標が返ってくる
                //2個めのoutはタンジェント座標？が返ってくる、Math.Atan2に入れるとラジアンが得られる？
                pg.GetPointAtFractionLength(i / step, out p, out pt);
                //○印の作成
                var path = new Path();
                path.Stroke = Brushes.Blue;
                path.Data = new EllipseGeometry(p, 4, 4);
                MyCanvas.Children.Add(path);
                MyListEllipse.Add(path);

                //角度の計算と表示
                //GetPointAtFractionLengthで得られたタンジェントをMath.Atan2に入れると
                //ラジアンが得られるので、Math.PI*180で角度に変換
                var kakudo = Math.Atan2(pt.Y, pt.X) / Math.PI * 180;
                Label label = new Label();
                label.Content = kakudo;
                Canvas.SetTop(label, p.Y);
                Canvas.SetLeft(label, p.X);
                MyCanvas.Children.Add(label);
                MyListLabel.Add(label);

                //長さ測定
                p2 = p;
                pathLength += Distance(p1, p2);

                //測定対象の2頂点を結ぶ直線Pathの作成
                var lineG = new LineGeometry(p1, p2);
                p1 = p;
                path = new Path();
                path.Stroke = Brushes.Red;
                path.Data = lineG;
                MyCanvas.Children.Add(path);
                MyListLine.Add(path);

            }
            TextBlockPathLength.Text =
                "Path Length Measure = " +
                Math.Round(pathLength, MidpointRounding.AwayFromZero).ToString();//四捨五入
        }

        //2点間の距離、ユークリッド距離
        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }
    }
}
