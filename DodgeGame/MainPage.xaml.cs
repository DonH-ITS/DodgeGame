namespace DodgeGame
{
    public partial class MainPage : ContentPage {
        double charHeight = 0;
        Random random = new Random();
        bool startGame = false;
        IDispatcherTimer timerDispatch;
        Player? player;
        
        // Instantiate the timer and components
        public MainPage() {
            InitializeComponent();
            timerDispatch = Dispatcher.CreateTimer();
            timerDispatch.Tick += OnTimerTick;
        }

        // Everytime OnTimerTick is called, create a new enemy
        // The next enemy should have a random start time too
        private void OnTimerTick(object? sender, EventArgs e) {
            timerDispatch.Stop();
            CreateEnemy();
            int mseconds = random.Next(500, 3000);
            timerDispatch.Interval = TimeSpan.FromMilliseconds(mseconds);
            timerDispatch.Start();
        }

        // Create a new Enemy object, StartMove is async (in Enemy class) so 
        // multiple threading is fine
        private static void CreateEnemy() {
            Enemy? enemy = new Enemy();
            
            // Subscribe to the removeEnemy event. If this event is raised set the object to null
            enemy.RemoveEnemy += (s, data) =>
            {
                enemy = null;
            };

        }

        // OnAppearing is when we want to set the Window's height and width. Fixing them in Windows
        // In Android, this will not change
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

        // Figure out what size we can make the GameLayout and do that
        // We need room for the Arrow Keys
        // Put the StartButton in the centre of the screen
        protected override void OnSizeAllocated(double width, double height) {
            base.OnSizeAllocated(width, height);
            GameLayout.WidthRequest = width;
            GameLayout.HeightRequest = height - ControlsGrid.Height;
        }

        // Start Button means start the game
        // Figure out the height and width of the characters, let's make it a 1/12 the height
        // Create the player object, set the Enemy's Static properties
        // Set the bindingcontext and finally make the first timer for the first enemy be a random number
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

        // When someone closes the Window or the Page is disappearing
        // Stop the timer and set MainL to null to hopefully prevent some crashes
        // Enemies may spawn/animate on a different thread so this is needed
        protected override void OnDisappearing() {
            timerDispatch.Stop();
            Enemy.RemoveMainLayoutRef();
            base.OnDisappearing();
        }

    }

}
