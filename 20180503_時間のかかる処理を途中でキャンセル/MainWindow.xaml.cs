using System;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
//処理中に進捗率表示とキャンセルボタンで中止はasync、await、Task.Run、Progress、CancellationTokenSource(ソフトウェア ) - 午後わてんのブログ - Yahoo!ブログ
//https://blogs.yahoo.co.jp/gogowaten/15489172.html

//時間のかかる処理をする時に
//進捗率を表示
//途中で処理のキャンセルをできるようにする

//時間のかかる処理は別スレッドで実行する
//それにはasyncとawait、System.ThreadingクラスのTask.Runを使う
//これで処理中にアプリが固まることがなくなる

//表示更新、進捗率表示はProgressクラスを使う
//別スレッドから直接UIスレッドの表示を更新することはできないので
//別スレッドにする処理に表示更新メソッドをもたせたProgressオブジェクトを渡して
//ProgressのReportメソッドを実行して表示更新する
//受け取る側はProgressじゃなくてIProgressで受け取る

//キャンセル
//CancellationTokenSourceクラスのTokenを使う
//これを時間のかかる処理に渡しておく
//キャンセルボタンが押されたらキャンセルを発行
//時間のかかる処理中にはキャンセルが発行されたかどうかの判定をしておいて
//発行されたら処理停止処理

namespace _20180503_時間のかかる処理を途中でキャンセル
{
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancelTokensource;//キャンセル判定用

        public MainWindow()
        {
            InitializeComponent();
            MyButton実行.Click += MyButton実行_Click;
            Mybuttonキャンセル.Click += Mybuttonキャンセル_Click;
        }

        private void Mybuttonキャンセル_Click(object sender, RoutedEventArgs e)
        {
            if (cancelTokensource != null)
            {
                cancelTokensource.Cancel();//キャンセルを発行            
            }
        }

        //非同期メソッドにするのでvoidの前にasyncを付けている
        private async void MyButton実行_Click(object sender, RoutedEventArgs e)
        {
            MyButton実行.IsEnabled = false;
            MyStatusText.Text = "処理中…";
            MyProgressBar.Value = 0;

            //キャンセル用トークン作成
            cancelTokensource = new CancellationTokenSource();
            var cToken = cancelTokensource.Token;

            //Progress作成時に表示更新用のメソッドを指定する
            //これを時間のかかる処理に渡す
            var p = new Progress<int>(ShowProgress);
            //非同期で時間のかかる処理を実行、これが別スレッドで実行される            
            bool result = await Task.Run(() => DoWork時間かかるＹＯ(p, cToken));

            if (result == false)
            {
                MyStatusText.Text = "キャンセルされた";
            }
            else
            {
                MyStatusText.Text = "処理完了！";
            }
            
            MyButton実行.IsEnabled = true;
        }

        //5秒かかる処理
        //ProgressはIProgressで受け取る
        private bool DoWork時間かかるＹＯ(IProgress<int> p, CancellationToken cancelToken)
        {
            for (int i = 1; i <= 50; ++i)//0.1秒の50回ループ、合計5秒
            {
                //キャンセル判定
                if (cancelToken.IsCancellationRequested == true)
                {
                    return false;
                }

                Thread.Sleep(100);//0.1秒待機
                int percentage = i * 100 / 50;//進捗率                
                p.Report(percentage);//状況の報告
            }
            return true;
        }

        //表示更新用
        private void ShowProgress(int percent)
        {
            MyProgressBar.Value = percent;
            MyTextBlock.Text = percent.ToString() + "％完了";
        }

    }
}