namespace DodgeGame
{
    public class Enemy
    {
        private static double size;
        private static double height;
        private static double width;
        private static Player? playerref;
        private static AbsoluteLayout? mainL;
        private static IDispatcher? mainDispatch;
        private static readonly Random random = new();


        public event EventHandler? RemoveEnemy;
        private BoxView box;
        private readonly bool top;
        private System.Timers.Timer timer;
        private Task<bool>? animation;

        // These fields are shared with all Enemy objects so they should be static
        // The mainLayout is needed to know where to add the enemy
        // Dispatcher is needed to sometimes draw on the Main UI Thread
        // A Player object reference is needed to find out if enemy collides with player
        public static void SetStaticProperties(double s, Player player, AbsoluteLayout view, IDispatcher dispatcher) {
            size = s; playerref = player; mainL = view; mainDispatch = dispatcher;
            height = mainL.Height;
            width = mainL.Width;
        }

        // If the Page is disappearing, set the mainL to null, this stops some crashes on closing
        public static void RemoveMainLayoutRef() {
            mainL = null;
        }

        public Enemy() {
            // Our timer means the update is checked every 20ms for every enemy object
            timer = new System.Timers.Timer
            {
                Interval = 20
            };

            // Starting Position, at top or from right
            top = random.Next(2) == 0;
            box = new BoxView()
            {
                WidthRequest = size,
                HeightRequest = size,
                Color = Colors.Blue,
            };
            
            if (mainL == null)
                return;
            mainL.Add(box);
            int x, y = 0;
            if (top) {
                x = random.Next((int)(width - size + 1));
                y = 0;
            }
            else {
                x = (int)(width - size);
                y = random.Next((int)(height - size + 1));
            }
          
            timer.Elapsed += (s, e) =>
            {
                Update();
            };
            // Insert the box at position 0,0 and then translate it to the position we want
            //MainL.SetLayoutBounds(box, new Rect(0, 0, size, size));
            box.TranslationX = x;
            box.TranslationY = y;
            
            // Use Task so StartMove is on a different thread, not locking up the UI
            Task.Run(() => StartMove());
        }

        // Every 20ms check if an enemy intersects with the player character
        private void Update() {
            // The current position of the enemy (the animation changes its position
            Rect rect = new Rect(box.TranslationX, box.TranslationY, size, size);            
        }

        public async Task StartMove() {
            // Give it a random delay before actually moving
            int randDelay = random.Next(200, 1000);
            await Task.Delay(randDelay);

            // Make the box have a random speed for its animation
            uint randSpeed = (uint)random.Next(1700, 5000);
            if (top) {
                animation = box.TranslateTo(box.TranslationX, height, randSpeed);
            }
            else {
                animation = box.TranslateTo(-size, box.TranslationY, randSpeed);
            }
            timer.Start();
            await animation;
            // If the box has gone off the screen, need to remove it
            if (box.TranslationX <= 0 || box.TranslationY >= height) {
                // Remove the box from the layout
                FullyRemoveEnemy();
            }
        }

        public void FullyRemoveEnemy() {
            // First delete the box from the mainlayout
            if (mainDispatch != null) {
                mainDispatch.Dispatch(() =>
                {
                    if (mainL != null)
                        mainL.Remove(box);
                });
            }
            // Remove the timer's resources
            timer.Stop();
            timer.Dispose();
            // Invoke an event so main thread will set the object to null for garbage collection
            RemoveEnemy?.Invoke(this, null);
        }
    }
}
