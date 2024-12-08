namespace DodgeGame
{
    public partial class MainPage : ContentPage {
        double charHeight = 0;
        Random random = new Random();
        bool startGame = false;
        IDispatcherTimer timerDispatch;
        int timerdone = 0;
        Player player;
        

        public MainPage() {
            InitializeComponent();
            timerDispatch = Dispatcher.CreateTimer();
            timerDispatch.Tick += OnTimerTick;
        }

       

        private void OnTimerTick(object sender, EventArgs e) {
            timerDispatch.Stop();
            CreateEnemy();
            int mseconds = random.Next(500, 3000);
            timerDispatch.Interval = TimeSpan.FromMilliseconds(mseconds);
            timerDispatch.Start();
            ++timerdone;
        }

        private void CreateEnemy() {
            Enemy enemy = new Enemy();
            
            // Subscribe to the removeEnemy event. If this event is raised set the object to null
            enemy.RemoveEnemy += (s, data) =>
            {
                enemy = null;
            };

        }

        protected override void OnAppearing() {
            base.OnAppearing();

#if WINDOWS
            this.Window.X = 0;
            this.Window.Y = 0;
            this.Window.MaximumHeight = DeviceDisplay.Current.MainDisplayInfo.Height-50;
            this.Window.MinimumHeight = DeviceDisplay.Current.MainDisplayInfo.Height-50;
            this.Window.MaximumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            this.Window.MinimumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
#endif
        }

        protected override void OnSizeAllocated(double width, double height) {
            base.OnSizeAllocated(width, height);
            GameLayout.WidthRequest = width;
            GameLayout.HeightRequest = height - ControlsGrid.Height;
            double startheight = StartButton.Height;
            double startwidth = StartButton.Width;
            GameLayout.SetLayoutBounds(StartButton, new Rect(width/2-startwidth/2, (height-ControlsGrid.Height)/2-startheight/2, startwidth, startheight));
        }

        private void Start_Button_Clicked(object sender, EventArgs e) {
            if (!startGame) {
                charHeight = GameLayout.Height / 12.0;
                player = new Player(charHeight, GameLayout, GameLayout.Height, GameLayout.Width);
                Enemy.SetStaticProperties(charHeight, player, GameLayout, Dispatcher);
                BindingContext = player;

                startGame = true;
                int mseconds = random.Next(400, 3000);
                timerDispatch.Interval = TimeSpan.FromMilliseconds(mseconds);
                timerDispatch.Start();
            }           
        }

        protected override void OnDisappearing() {
            timerDispatch.Stop();
            Enemy.RemoveMainLayoutRef();
            base.OnDisappearing();
        }

    }

}
