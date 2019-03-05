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

using System.Collections.ObjectModel;//ObservableCollection用
using System.Globalization;//ValueConverterで使用


//WPF の ListBox に対する リストデータ の バインディング と 表示名 の 制御 - galife
//https://garafu.blogspot.com/2014/09/wpf-listbox.html
//WPF4.5入門 その56「コレクションのバインディング」 - かずきのBlog @hatena
//https://blog.okazuki.jp/entry/2014/10/29/220236
//第5回 WPFの「データ・バインディング」を理解する(2/3)：連載：WPF入門 - ＠IT
//https://www.atmarkit.co.jp/ait/articles/1010/08/news123_2.html


    //このアプリの記事
//WPFのListBoxでいろいろ、Binding、見た目の変更、横リスト(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15893148.html

namespace _20190304_ListBoxのDataTemplate
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<MyColorData> myData;//Bindingのソースになるデータ

        public MainWindow()
        {
            InitializeComponent();

            //データリスト作成
            //データを入れるリストには変更通知してくれるObservableCollectionを使うのがラク
            myData = new ObservableCollection<MyColorData>();
            myData.Add(new MyColorData { Color = Colors.BlanchedAlmond, Name = "BlanchedAlmond" });
            myData.Add(new MyColorData { Color = Colors.Orange, Name = "Orange" });
            myData.Add(new MyColorData { Color = Colors.Olive, Name = nameof(Colors.Olive) });
            myData.Add(new MyColorData { Color = Color.FromRgb(0xEB, 0x79, 0x88), Name = "紅梅" });
            myData.Add(new MyColorData { Color = Color.FromRgb(242, 216, 223), Name = "桜色" });

            //アプリ自体にデータをBinding、参照の追加、関連付け
            this.DataContext = myData;

            //ボタンクリック時の動作
            //MyButton1.Click += (s, e) => { myData[0].Color = Colors.Tomato; myData[0].Name = "Tomato"; };
            //↑だとBindingが外れてしまう
            //ボタンクリックでデータの変更
            MyButton1.Click += (s, e) => { myData[0] = new MyColorData { Color = Colors.Tomato, Name = "Tomato" }; };
            //↑ラムダ式↓今までの
            MyButton2.Click += MyButton2_Click;


            //Colorsクラスの色一覧表示、MyListBoxWPFColors用
            //TypeクラスのGetPropertyメソッドを使ってColorsクラスの色取得
            System.Reflection.PropertyInfo[] infos = typeof(Colors).GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            var wpfColors = new ObservableCollection<MyColorData>();//Bindingのソースになるデータ
            foreach (var item in infos)
            {
                wpfColors.Add(new MyColorData { Color = (Color)item.GetValue(null), Name = item.Name });
            }
            //関連付けはアプリ自体じゃなくて対象のListBoxにした
            MyListBoxWPFColors.DataContext = wpfColors;
        }


        private void MyButton2_Click(object sender, RoutedEventArgs e)
        {
            myData[0] = new MyColorData { Color = Colors.MediumSpringGreen, Name = nameof(Colors.MediumSpringGreen) };
            //↓だとBindingが外れてしまう
            //myData[0].Color = Colors.MediumSpringGreen;
            //myData[0].Name = nameof(Colors.MediumSpringGreen);
        }
    }



    public class MyColorData
    {
        public Color Color { get; set; }
        public string Name { get; set; }
    }



    //ColorをSolidColorBrushに変換
    public class MyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color c = (Color)value;
            return new SolidColorBrush(c);
        }

        //こっちは未使用
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
