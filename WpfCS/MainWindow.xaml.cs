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

namespace WpfCS
{
    using WpfContLib;
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int COLODA_MAX_CARDS_COUNT = 11;
        private Client clientMain;
        private Coloda coloda;
        private Dictionary<string, ViewColoda>  dictionaryStopka;

        private ViewColoda dealerColoda;
        private ViewColoda playerColoda;

        private Label myScoreLabel;
        private Label dealerScoreLabel;
        private Label gameStatusLabel;
        private Label moneyLabel;

        private Blackjack blackjack;

        public MainWindow()
        {
            InitializeComponent();
            dictionaryStopka = new Dictionary<string, ViewColoda>();
            UpdateColoda();

            dealerColoda = MakeStopka(0, 0, COLODA_MAX_CARDS_COUNT, RuleDisabled, StateContainer.веером, Orientation.Horizontal);
            playerColoda = MakeStopka(0, 1, COLODA_MAX_CARDS_COUNT, RuleDisabled, StateContainer.веером, Orientation.Horizontal);

            clientMain = new Client("Blackjack", new ViewColoda[] { dealerColoda }, 10, 10, new ViewColoda[] { playerColoda }, 10, 500);

            //var c = (Canvas)clientMain.Content;
            //c.Children.Add(hitButton);

            var hitButton = MakeButton("Hit", HitButton_Click);
            clientMain.ButtonsPanel.Children.Add(hitButton);
            var standButton = MakeButton("Stand", StandButton_Click);
            clientMain.ButtonsPanel.Children.Add(standButton);
            myScoreLabel = MakeLabel("My score: 0", 100);
            clientMain.ButtonsPanel.Children.Add(myScoreLabel);
            dealerScoreLabel = MakeLabel("Dealer score: 0", 100);
            clientMain.ButtonsPanel.Children.Add(dealerScoreLabel);
            moneyLabel = MakeLabel("$0", 100);
            clientMain.ButtonsPanel.Children.Add(moneyLabel);

            clientMain.ButtonsPanelTop = 400;
            clientMain.ButtonsPanelLeft = 10;

            gameStatusLabel = MakeLabel("", 400, 100);
            gameStatusLabel.FontSize = 36;

            clientMain.TextPanel.Children.Add(gameStatusLabel);
            clientMain.TextPanelTop = clientMain.Height / 2 - 50;
            clientMain.TextPanelLeft = 10;

            clientMain.Background = new SolidColorBrush(Colors.Green);
            clientMain.ButtonsPanel.Background = new SolidColorBrush(Colors.White);
            clientMain.TextPanel.Background = new SolidColorBrush(Colors.White);
            clientMain.Show();

            MyScore = 0;
            DealerScore = 0;

            GameStatusText = "";

            clientMain.KeyDown += Key_Down;

            clientMain.Closed += (sender, e) => Close();
            Hide();

            blackjack = new Blackjack();
            StartGame();
        }
        private void UpdateColoda()
        {
            coloda = new Coloda(uriCards.Count() - 2);
            coloda.Tasovat();
        }
        private void StartGame()
        {
            dealerColoda.ClearCards();
            dealerColoda.AddCard(1);
            playerColoda.ClearCards();
            playerColoda.AddCard(1);

            UpdateColoda();
            blackjack.Bet();

            var firstCard = coloda.GetCard(1).First();
            blackjack.CurrentGotCard(new BlackjackCard(firstCard)); // dealer
            dealerColoda.AddCard(firstCard);
            blackjack.Stand();

            foreach (var card in coloda.GetCard(2)) // player
            {
                blackjack.CurrentGotCard(new BlackjackCard(card));
                playerColoda.AddCard(card);
            }
            ShowInfo();
            if (blackjack.MyScore == 21)
            {
                StandButton_Click(null, null);
            }
        }
        private void ShowInfo()
        {
            MyScore = blackjack.MyScore;
            DealerScore = blackjack.DealerScore;

            Money = "$" + blackjack.money;

            switch (blackjack.GameStatus)
            {
                case GameStatus.WIN:
                    GameStatusText = "Victory";
                    break;
                case GameStatus.LOSE:
                    GameStatusText = "Lose";
                    break;
                case GameStatus.PUSH:
                    GameStatusText = "Push";
                    break;
                default:
                    GameStatusText = "";
                    break;
            }
        }

        private int _myScore;
        public int MyScore
        {
            get
            {
                return _myScore;
            }
            set
            {
                myScoreLabel.Content = String.Format("My score: {0,-2}", value);
                _myScore = value;
            }
        }

        private int _dealerScore;
        public int DealerScore
        {
            get
            {
                return _dealerScore;
            }
            set
            {
                dealerScoreLabel.Content = String.Format("Dealer score: {0,-2}", value);
                _dealerScore = value;
            }
        }
        public string GameStatusText
        {
            get
            {
                return (string) gameStatusLabel.Content;
            }
            set
            {
                gameStatusLabel.Content = value;
            }
        }
        public string Money
        {
            get
            {
                return (string)moneyLabel.Content;
            }
            set
            {
                moneyLabel.Content = value;
            }
        }

        private bool RuleDisabled(ref bool podKartu, ColorKard otkudaColorKard, PriceKard otkudaPriceKard,
            int indexOfCardsOtkuda, int MaxCountOtkuda, ColorKard kudaColorKard, PriceKard kudaPriceKard,
            int indexOfCardsKuda, int MaxCountKuda)
        {
            return false;
        }

        private void StandButton_Click(object sender, RoutedEventArgs e)
        {
            if (blackjack.IsGameEnded())
            {
                StartGame();
                return;
            }
            blackjack.Stand();
            while (blackjack.GameStatus == GameStatus.DEALER)
            {
                var card = coloda.GetCard(1).First();
                blackjack.CurrentGotCard(new BlackjackCard(card));
                dealerColoda.AddCard(card);
            }
            blackjack.Stand();
            ShowInfo();
        }

        private void HitButton_Click(object sender, RoutedEventArgs e)
        {
            if (blackjack.IsGameEnded())
            {
                StartGame();
                return;
            }
            var card = coloda.GetCard(1).First();
            blackjack.CurrentGotCard(new BlackjackCard(card));
            playerColoda.AddCard(card);
            ShowInfo();
            if (blackjack.MyScore == 21)
            {
                StandButton_Click(null, null);
            }
        }
        private void Key_Down(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    HitButton_Click(null, null);
                    break;
                case Key.Enter:
                    StandButton_Click(null, null);
                    break;
                case Key.Escape:
                    clientMain.Close();
                    break;
            }
        }

        private bool RuleIstochnik(ref bool podKartu, ColorKard otkudaColorKard, PriceKard otkudaPriceKard,
            int indexOfKardsOtkuda, int MaxCountOtkuda, ColorKard kudaColorKard, PriceKard kudaPriceKard,
            int indexOfKardsKuda, int MaxCountKuda)
        {
            if ((otkudaColorKard == ColorKard.кресты || otkudaColorKard == ColorKard.пики) &&
              (kudaColorKard == ColorKard.червы || kudaColorKard == ColorKard.буби))
                if((otkudaPriceKard-PriceKard.p0)+1==(kudaPriceKard-PriceKard.p0))
                return true;
            if ((otkudaColorKard == ColorKard.буби || otkudaColorKard == ColorKard.червы) &&
              (kudaColorKard == ColorKard.кресты || kudaColorKard == ColorKard.пики))
                if ((otkudaPriceKard - PriceKard.p0) + 1 == (kudaPriceKard - PriceKard.p0))
                    return true;
            return false;
        }
        private Button MakeButton(string text, RoutedEventHandler handler = null, double width = 50, double height = 25)
        {
            var button = new Button();
            button.Height = height;
            button.Width = width;
            button.Content = text;
            if (handler != null) {
                button.Click += handler;
            }
            return button;
        }
        private Label MakeLabel(string text, double width = 50, double height = 25)
        {
            var label = new Label();
            label.Content = text;
            label.Width = width;
            label.Height = height;
            return label;
        }
        private ViewColoda MakeStopka(int countCards, int numberSstopka, int MaxCard, BackRule backRule,
            StateContainer stateContainer = StateContainer.веером, Orientation orientation = Orientation.Vertical)
        {
            List<int> stopka = new List<int>();
            stopka.Add(1);
            if (countCards > 0)
            {
                stopka.AddRange(coloda.GetCard(countCards));
            }
            string name = "Stopka" + numberSstopka.ToString();
            ViewColoda vcStopka = new ViewColoda(uriCards, backRule, stopka, MaxCard, name, stateContainer, StateCard.рубашка_вниз,
                orientation);
            vcStopka.Name = name;
            vcStopka.SaveGlobalSender += SaveImage;
            vcStopka.DropGlobalSender += DropImage;
            dictionaryStopka.Add(name, vcStopka);
            return vcStopka;
        }
        private void SaveImage(string name, Image parm)
        {
            foreach (var item in dictionaryStopka)
            {
                if (item.Key != name)
                {
                    item.Value.GlobalSender = parm;
                }
            }
        }
        private void DropImage(string name, Image parm)
        {
            if (parm != null)
            {
                int indexOfUriCards = ((ForTag)parm.Tag).IndexOfUriCards;
                int indexOfCards = ((ForTag)parm.Tag).IndexOfCards;
                ViewColoda vcStopka = null;
                if (dictionaryStopka.TryGetValue(name, out vcStopka))
                    {
                    vcStopka.DeleteCard(indexOfCards);
                }
            }
        }
        private string[] uriCards = new string[]
    {
            @"\cards\Рубашка.jpg",    //0
            @"\cards\Нет60.jpg",        //1

            @"\cards\Б2.jpg",           //2
            @"\cards\Б3.jpg",           //3
            @"\cards\Б4.jpg",           //4
            @"\cards\Б5.jpg",           //5
            @"\cards\Б6.jpg",           //6
            @"\cards\Б7.jpg",           //7
            @"\cards\Б8.jpg",           //8
            @"\cards\Б9.jpg",           //9
            @"\cards\Б10.jpg",          //10
            @"\cards\БВ.jpg",           //11
            @"\cards\БД.jpg",           //12
            @"\cards\БК.jpg",           //13
            @"\cards\БТ.jpg",           //14
                              
            @"\cards\Ч2.jpg",           //15
            @"\cards\Ч3.jpg",           //16
            @"\cards\Ч4.jpg",           //17
            @"\cards\Ч5.jpg",           //18
            @"\cards\Ч6.jpg",           //19
            @"\cards\Ч7.jpg",           //20
            @"\cards\Ч8.jpg",           //21
            @"\cards\Ч9.jpg",           //22
            @"\cards\Ч10.jpg",          //23
            @"\cards\ЧВ.jpg",           //24
            @"\cards\ЧД.jpg",           //25
            @"\cards\ЧК.jpg",           //26
            @"\cards\ЧТ.jpg",           //27

            @"\cards\К2.jpg",           //28
            @"\cards\К3.jpg",           //29
            @"\cards\К4.jpg",           //30
            @"\cards\К5.jpg",           //31
            @"\cards\К6.jpg",           //32
            @"\cards\К7.jpg",           //33
            @"\cards\К8.jpg",           //34
            @"\cards\К9.jpg",           //35
            @"\cards\К10.jpg",          //36
            @"\cards\КВ.jpg",           //37
            @"\cards\КД.jpg",           //38
            @"\cards\КК.jpg",           //39
            @"\cards\КТ.jpg",           //40

            @"\cards\П2.jpg",           //41
            @"\cards\П3.jpg",           //42
            @"\cards\П4.jpg",           //43
            @"\cards\П5.jpg",           //44
            @"\cards\П6.jpg",           //45
            @"\cards\П7.jpg",           //46
            @"\cards\П8.jpg",           //47
            @"\cards\П9.jpg",           //48
            @"\cards\П10.jpg",          //49
            @"\cards\ПВ.jpg",           //50
            @"\cards\ПД.jpg",           //51
            @"\cards\ПК.jpg",           //52
            @"\cards\ПТ.jpg",           //53
    };
    }
}
