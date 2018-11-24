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

using MyHSV;
using System.Reflection;

namespace WPF色一覧
{
    class MyItem
    {
        public string Name;//色名
        public Color Color;
        public HSV HSV;
        public double Y;//YUV形式のY、輝度

        public MyItem(string name, Color color, HSV hSV, double y)
        {
            Name = name;
            Color = color;
            HSV = hSV;
            Y = y;
        }
    }



    ////複数キーで昇順ソート
    ////OrderBy().ThenBy().ThenBy().ThenBy().…
    ////降順ソート
    ////OrderByDescending().ThenByDescending().ThenByDescending().ThenByDescending().…
    ////ソート実行の順番は後ろからされるので
    ////ThenBy()のあとにOrderBy()が実行される、わかりにくい

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //色表示に使うTextBlockのリスト
        private List<TextBlock> MyListTextBlock = new List<TextBlock>();
        //並べ替えするテーブル
        List<MyItem> MyTable = new List<MyItem>();

        public MainWindow()
        {
            InitializeComponent();
            Title = this.ToString();

            MyInitialize();

            ButtonHue.Click += ButtonHue_Click;
            ButtonSaturation.Click += ButtonSaturation_Click;
            ButtonValue.Click += ButtonValue_Click;
            ButtonBrightness.Click += ButtonBrightness_Click;
            ButtonName.Click += ButtonName_Click;
            CheckBoxVisible.Click += CheckBoxVisible_Click;
        }

        private void CheckBoxVisible_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBoxVisible.IsChecked == true)
            {
                for (int i = 0; i < MyListTextBlock.Count; i++)
                {
                    MyListTextBlock[i].Text = MyTable[i].Name;
                }
            }
            else
            {
                for (int i = 0; i < MyListTextBlock.Count; i++)
                {
                    MyListTextBlock[i].Text = "";
                }
            }
        }

        private void ButtonName_Click(object sender, RoutedEventArgs e)
        {
            MyTable = MyTable.OrderBy(a => a.Name).ToList();
            ChangeTextBlockColor();
        }

        private void ButtonBrightness_Click(object sender, RoutedEventArgs e)
        {
            MyTable = MyTable.OrderByDescending(a => a.Y).ToList();
            ChangeTextBlockColor();
        }

        private void ButtonValue_Click(object sender, RoutedEventArgs e)
        {
            MyTable = MyTable.OrderByDescending(a => a.HSV.Value).ToList();
            ChangeTextBlockColor();
        }

        private void ButtonSaturation_Click(object sender, RoutedEventArgs e)
        {
            MyTable = MyTable.OrderBy(a => a.HSV.Saturation).ToList();
            ChangeTextBlockColor();
        }

        private void ButtonHue_Click(object sender, RoutedEventArgs e)
        {
            MyTable = MyTable.OrderBy(a => a.HSV.Hue).ToList();            
            ChangeTextBlockColor();
        }




        private void MyInitialize()
        {
            //TypeクラスのGetPropertyメソッドを使ってColorsクラスの色取得
            PropertyInfo[] infos = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < infos.Length; i++)
            {
                PropertyInfo info = infos[i];
                Color color = (Color)info.GetValue(null);
                //HSV hSV = HSV.Color2HSV_ConicalModel(color);//円錐モデル
                HSV hSV = HSV.Color2HSV(color);//円柱モデル
                string name = info.Name;
                double y = GetBrightness(color);
                MyTable.Add(new MyItem(name, color, hSV, y));
                var textBlock = new TextBlock()
                {
                    Margin = new Thickness(1),
                    Text = name,
                    Width = 120,
                    Tag = i
                };
                MyWrapPanel.Children.Add(textBlock);
                MyListTextBlock.Add(textBlock);
                textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
                textBlock.MouseEnter += TextBlock_MouseEnter;
            }
            ChangeTextBlockColor();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            var tb = (TextBlock)sender;
            var i = (int)tb.Tag;
            TextBlockBackAndForeColor.Foreground = new SolidColorBrush(MyTable[i].Color);
            TextBlockBackAndForeColor.Text = MyTable[i].Name;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBlock = (TextBlock)sender;
            var b = (SolidColorBrush)textBlock.Background;
            int i = (int)textBlock.Tag;
            MyItem item = MyTable[i];
            TextBlockBackAndForeColor.Background = b;
            TextBlockHexadecimal.Text = b.Color.ToString();
            TextBlockColor.Background = b;
            TextBlockColorName.Text = item.Name;
            TextBlockRGB.Text = $"R:{b.Color.R.ToString()} G:{b.Color.G.ToString()} B:{b.Color.B.ToString()}";
            TextBlockHue.Text = $"色相：{item.HSV.Hue:000.00}°";
            TextBlockSaturation.Text = $"彩度：{item.HSV.Saturation:000.00%}";
            TextBlockValue.Text = $"明度：{item.HSV.Value:000.00%}";
            TextBlockbrightness.Text = $"輝度：{item.Y:000.00}";
        }

        //色一覧textblockの色の変更        
        private void ChangeTextBlockColor()
        {
            for (int i = 0; i < MyTable.Count; i++)
            {
                TextBlock tb = MyListTextBlock[i];
                MyItem item = MyTable[i];
                tb.Background = new SolidColorBrush(item.Color);
                if (CheckBoxVisible.IsChecked == true)
                {
                    tb.Text = item.Name;
                }
                //色の輝度が200未満なら文字色を白にする、以上なら灰色
                if (item.Y < 200)
                {
                    tb.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    tb.Foreground = new SolidColorBrush(Colors.Gray);
                }

            }
        }

        //輝度
        private double GetBrightness(Color col)
        {
            double y = 0.299 * col.R + 0.587 * col.G + 0.11 * col.B;
            return y;
        }
    }
}
