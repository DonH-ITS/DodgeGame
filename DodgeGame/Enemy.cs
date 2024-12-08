
using System.Runtime.CompilerServices;
using System.Timers;

namespace DodgeGame
{
    public class Enemy
    {
        private static double size;
        private static double height;
        private static double width;
        private static Player playerref;
        private static AbsoluteLayout mainL;
        private static IDispatcher mainDispatch;
        private static Random random;


        public event EventHandler RemoveEnemy;
        private BoxView box;
        private readonly bool top;
        private System.Timers.Timer timer;
        private Task<bool> animation;

        public static void SetStaticProperties(double s, Player player, AbsoluteLayout view, IDispatcher dispatcher) {
            size = s; playerref = player; mainL = view; mainDispatch = dispatcher;
            height = mainL.Height;
            width = mainL.Width;
            random = new Random();
        }

        public static void RemoveMainLayoutRef() {
            mainL = null;
        }

        public Enemy() {
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
            timer = new System.Timers.Timer
            {
                Interval = 20
            };
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

        private void Update() {
            // See if the box has collided with the player
            Rect rect = new Rect(box.TranslationX, box.TranslationY, size, size);            
        }

        public async Task StartMove() {
            int randDelay = random.Next(200, 1000);
            await Task.Delay(randDelay);
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
            mainDispatch.Dispatch(() => { 
                if(mainL != null)
                    mainL.Remove(box); 
            });
            // Remove the timer's resources
            timer.Stop();
            timer.Dispose();
            // Invoke an event so main thread will set the object to null for garbage collection
            RemoveEnemy?.Invoke(this, null);
        }
    }
}
