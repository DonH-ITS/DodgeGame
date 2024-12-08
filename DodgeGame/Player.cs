using System.ComponentModel;

namespace DodgeGame
{
    public class Player : INotifyPropertyChanged
    {        
        //Fields
        Task<bool>? animation = null;
        private double size;
        private Grid box;
        private System.Timers.Timer timer;
        private double maxHeight, maxWidth;
        private bool overedge = false;
        private int life;
        private int hits;

        //Properties and associated fields
        private Rect pos;
        public Rect Position
        {
            get
            {
                return pos;
            }
        }

        private bool allowmoves = false;
        public bool Allowmoves
        {
            get => allowmoves;
            set
            {
                allowmoves = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableButton));
            }
        }
        public bool EnableButton
        {
            get
            {
                return !allowmoves;
            }
        }

        // Commands
        public Command MoveCommand { get; }

        // Constructor
        public Player(double size, AbsoluteLayout mainl, double maxh, double maxw) {
            MoveCommand = new Command<string>(async (dir) => await MovePlayer(dir));

            box = new Grid()
             {
                 WidthRequest = size,
                 HeightRequest = size,
                 BackgroundColor = Colors.Red
             };

            maxHeight = maxh;
            maxWidth = maxw;
            this.size = size;
            mainl.Add(box);
            pos = new Rect(0, 0, size, size);
            timer = new System.Timers.Timer
            {
                Interval = 20
            };
            timer.Elapsed += (s, e) =>
            {
                Update();
            };
            life = 3;
            
            Allowmoves = false;
            StartPlayer();    
        }

        public async void StartPlayer() { 
            if (!allowmoves) {
                box.TranslationX = (maxWidth - size) / 2;
                box.TranslationY = (maxHeight - size) / 2;
                pos.X = box.TranslationX;
                pos.Y = box.TranslationY;
                hits = 0;
                Allowmoves = true;
                timer.Start();
            }
        }

        public async void GotHit() {
            hits++;
            if (hits < life) {
                //do something if hit, change the colour of the character?
            }
            else if(hits >= life) {
                // Game Over
                timer.Stop();
                box.CancelAnimations();
                Allowmoves = false;
                pos.Y = -size - 1;
                pos.X = -size - 1;
            }
        }

        // Every 20ms check the current position of the box and update the pos Rectangle. 
        // If it has gone over the edge, cancel the animation and signal the animation to fix the translation.
        // Do not modify box.TranslationX here as it is in a different thread than the UI
        private void Update() {
            if (box.TranslationY < 0 || box.TranslationX < 0 || box.TranslationY > maxHeight - size || box.TranslationX > maxWidth - size) {
                if (animation != null) {
                    overedge = true;
                    box.CancelAnimations(); 
                }
            }
            pos.Y = box.TranslationY;
            pos.X = box.TranslationX;
        }

        private async Task MovePlayer(string dir) {
            if (allowmoves) {
                double x = box.TranslationX;
                double y = box.TranslationY;
                switch (dir) {
                    case "0":
                        x -= size;
                        break;
                    case "1":
                        y -= size;
                        break;
                    case "2":
                        y += size;
                        break;
                    case "3":
                        x += size;
                        break;
                }
                if (animation != null) {
                    box.CancelAnimations();
                }

                animation = box.TranslateTo(x, y, 250);

                bool wascancelled = await animation;
                // if wascancelled is true, that means that box.CancelAnimations was called at some stage
                // We only care when box.CancelAnimations was called by the update, ie overedge is also true
                // We ensure the box does not go over the edge by doing this
                if (wascancelled && overedge) {
                    if (box.TranslationY < 0) {
                        box.TranslationY = 0;
                    }
                    else if (box.TranslationY > maxHeight - size) {
                        box.TranslationY = maxHeight - size;
                    }
                    if (box.TranslationX < 0) {
                        box.TranslationX = 0;
                    }
                    else if (box.TranslationX > maxWidth - size) {
                        box.TranslationX = maxWidth - size;
                    }
                    overedge = false;
                }
                animation = null;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
