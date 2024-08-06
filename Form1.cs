using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Chicken
{
    public partial class Form1 : Form
    {
        int chickenSpeed = 0,
            leftMostChicken = 0,// Index of the leftmost chicken
            counter = 0,
            dt = 1,   // Delta time or time step for game updates
            live = 3, // Number of lives the player has
            score = 0;


        List<Piece> _bullets = new List<Piece>();//رصاصة List to store the bullets fired by the player's rocket
        Bitmap _mainChickenImage = Properties.Resources.bossRed;

        // List to store individual frames of the chicken animation
        List<Bitmap> _chickenFrames = new List<Bitmap>();
        // 2D array to store the chickens in a grid (3 rows X  8 columns)
        Piece[,] _chickens = new Piece[3, 8];
        int[] topChicken = new int[3]; // Array to store the top position of each column of chickens

        // Hearts
        List<Piece> _liveHearts = new List<Piece>();

        //eggs
        Bitmap _mainBrokenEgg = Properties.Resources.eggBreak;
        List<Bitmap> _brokenEggFrames = new List<Bitmap>();
        List<Piece> _eggs = new List<Piece>();
        Piece _rocket;


        Random rand = new Random();
        public Form1()
        {
            InitializeComponent();
            Initial();
        }
        //timer controlling chicken movement
        private void tmrchickens_Tick(object sender, EventArgs e)
        {
            // Check if the leftmost chicken has reached the right or left boundary of the window
            if (leftMostChicken + 800 > Width || leftMostChicken < 0)
                // Reverse the direction of the chickens' movement
                chickenSpeed = -chickenSpeed;
            // Update the position of the leftmost chicken
            leftMostChicken += chickenSpeed;

            // Loop over 3 rows of chickens
            for (int i = 0; i < 3; i++)
                // Loop over 8 columns of chickens
                for (int j = 0; j < 8; j++)
                {
                    // If the chicken at the current position is null, skip to the next iteration
                    if (_chickens[i, j] == null)
                        continue;

                    // Update the image of the chicken to the current frame (for animation)
                    _chickens[i, j].Image = _chickenFrames[counter];

                    // Move the chicken horizontally based on the current speed
                    _chickens[i, j].Left += chickenSpeed;
                }

            // Update the counter for the chicken animation frames
            counter = counter + dt;// plus time step = 1  <=== global

            // Reverse the direction of the frame animation if the counter reaches the last or first frame
            dt = counter == 9 ? -1 : (counter == 0 ? 1 : dt);
        }
        // Method to launch a random egg from a random available chicken
        private void launchRandomEgg()
        {
            // Create a list to hold chickens that can drop an egg
            List<Piece> availablesChickens = new List<Piece>();
            // Loop over 3 rows of chickens
            for (int i = 0; i < 3; i++)
                // Loop over 8 columns of chickens
                for (int j = 0; j < 8; j++)
                    // If the chicken at the current position is not null, add it to the list
                    if (_chickens[i, j] != null)
                        availablesChickens.Add(_chickens[i, j]);

            // Select a random chicken from the available chickens
            Piece chicken = availablesChickens[rand.Next() % availablesChickens.Count];

            // Create a new egg piece
            Piece egg = new Piece(10, 10);
            // Set the egg image
            egg.Image = Properties.Resources.egg;

            // Position the egg at the center bottom of the selected chicken
            egg.Left = chicken.Left + chicken.Width / 2 - egg.Width / 2;
            egg.Top = chicken.Top + chicken.Height;

            // Add the egg to the list of eggs and to the form's controls
            _eggs.Add(egg);
            Controls.Add(egg);
        }
        private void decreaseLive()
        {
            live -= 1;
            _liveHearts[live].Image = Properties.Resources.heart;
            if (live < 1) endGame(Properties.Resources.d_heart);
        }
        private void cls(object sender, EventArgs e)
        {
            Close();
        }
        private void endGame(Bitmap img)
        {
            tmrEggs.Stop(); 
            tmrBullets.Stop(); 
            tmrchickens.Stop();
            Controls.Clear();
            Piece pic = new Piece(100, 100);
            pic.Click += cls;
            pic.Image = img;
            pic.Left = Width / 2 - pic.Width / 2;
            pic.Top = Height / 2 - pic.Height / 2;
            Controls.Add(pic);
        }

        private void tmrEggs_Tick(object sender, EventArgs e)
        {// Randomly decide whether to launch a new egg. 1 in 200 chance.
            if (rand.Next(200) == 5)
                launchRandomEgg();

            // Iterate through each egg in the _eggs list.
            for (int i = 0; i < _eggs.Count; i++)
            {
                // Move the egg downward by increasing its Top property by eggDownSpeed.
                _eggs[i].Top += _eggs[i].eggDownSpeed;

                // Check if the egg intersects with the rocket.
                if (_rocket.Bounds.IntersectsWith(_eggs[i].Bounds))
                {
                    // If the egg hits the rocket, remove the egg from the screen and the list.
                    Controls.Remove(_eggs[i]);
                    _eggs.RemoveAt(i);

                    // Decrease the player's life.
                    decreaseLive();
                    // Break out of the loop after handling the collision.
                    break;
                }

                // Check if the egg has reached the bottom of the screen.
                if (_eggs[i].Top >= Height - (_eggs[i].Height + 20))
                {
                    // Stop the egg's downward movement.
                    _eggs[i].eggDownSpeed = 0;

                    // Animate the egg breaking.
                    // If the eggLandCount divided by 2 is less than the number of broken egg frames,
                    if (_eggs[i].eggLandCount / 2 < _brokenEggFrames.Count)
                    {
                        // Update the egg's image to the next broken egg frame.
                        _eggs[i].Image = _brokenEggFrames[_eggs[i].eggLandCount / 2];
                        // Increment the eggLandCount.
                        _eggs[i].eggLandCount += 1;
                    }
                    else
                    {
                        // If all frames have been shown, remove the egg from the screen and the list.
                        Controls.Remove(_eggs[i]);
                        _eggs.RemoveAt(i);
                    }
                }



            }
        }
        private void collision()
        {
            for (int i = 0; i < topChicken.Length; i++)
            {
                // Initialize variables for binary search
                int lo = 0, hi = _bullets.Count - 1, md, ans = -1;
                // Perform binary search to find the first bullet that could hit the current row of chickens

                while (lo <= hi)
                {
                    md = lo + (hi - lo) / 2;
                    if (_bullets[md].Top >= topChicken[i])
                    {
                        hi = md - 1;
                        ans = md;
                    }
                    else lo = md + 1;
                }

                // Check if a bullet is found within the vertical range of the chickens

                if (ans != -1 && _bullets[ans].Top >= topChicken[i]
                   && _bullets[ans].Top <= topChicken[i] + _chickenFrames[0].Height)
                {           
                    // Calculate the column of the chicken hit by the bullet
                    int j = (_bullets[ans].Left + 9 - leftMostChicken) / 100;
                    // Check if the calculated column is valid and there is a chicken at that position
                    if (j >= 0 && j < 8 && _chickens[i, j] != null
                        && _bullets[ans].Bounds
                        .IntersectsWith(_chickens[i, j].Bounds))
                    {// Remove the bullet from the screen and the list
                        Controls.Remove(_bullets[ans]);
                        _bullets.RemoveAt(ans);
                        // Remove the chicken from the screen and the array

                        Controls.Remove(_chickens[i, j]);
                        _chickens[i, j] = null;
                        score += 10;
                        lblScore.Text = "Score: " + score.ToString();
                    }
                }
            }
        }
        private void tmrBullets_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < _bullets.Count; i++)
                _bullets[i].Top -= 3;

            collision();
            if (score == 240) 
                endGame(Properties.Resources.win);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                launchBullet();

        }
        private void launchBullet()
        {
            Piece bullet = new Piece(30, 30);
            bullet.Left = _rocket.Left + _rocket.Width / 2 - bullet.Width / 2;
            bullet.Top = _rocket.Top - bullet.Height;
            bullet.Image = Properties.Resources.b2;//
            _bullets.Add(bullet);
            Controls.Add(bullet);
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    _rocket.Left -= 10;
                    break;
                case Keys.Right:
                    _rocket.Left += 10;
                    break;
                case Keys.Up:
                    _rocket.Top -= 10;
                    break;
                case Keys.Down:
                    _rocket.Top += 10;
                    break;
            }

        }

        private void Initial()
            {
                _rocket = new Piece(100, 100);// create new rocket
                                              // Set the rocket's position to be centered horizontally at the bottom of the screen
                _rocket.Left = Width / 2 - _rocket.Width / 2;
                _rocket.Top = Height - _rocket.Height;
                _rocket.Image = Properties.Resources.rocket;
                Controls.Add(_rocket);
                divideImageIntoFrames(_mainChickenImage, _chickenFrames, 10);
                createChickens();
                createHearts();
                divideImageIntoFrames(_mainBrokenEgg, _brokenEggFrames, 8);

            }
            private void createHearts()
            {
                Bitmap heart = Properties.Resources.heart;
                for (int i = 0; i < 3; i++)
                {
                    _liveHearts.Add(new Piece(25, 25));
                    _liveHearts[i].Image = heart;
                    _liveHearts[i].Left = Width - (3 - i) * 45;
                    Controls.Add(_liveHearts[i]);
                }
            }
        private void divideImageIntoFrames(Bitmap original, List<Bitmap> res, int n)
            {
                // Calculate the width of each frame
                int w = original.Width / n;
                // Get the height of the original image (each frame has the same height)
                int h = original.Height;
                // Loop to create each frame
                for (int i = 0; i < n; i++)
                {
                    // Calculate the starting position of the current frame in the original image
                    int s = i * w;

                    // Create a new Bitmap for the current frame with the calculated width and height
                    Bitmap piece = new Bitmap(w, h);

                    // Nested loop to copy each pixel from the original image to the new frame
                    for (int j = 0; j < h; j++) // Loop over each row (height)
                        for (int k = 0; k < w; k++) // Loop over each column (width)
                                                    // Copy the pixel from the original image to the new frame
                            piece.SetPixel(k, j, original.GetPixel(k + s, j));

                    // Add the created frame to the result list
                    res.Add(piece);

                }
            }
        private void createChickens()
            {
                Bitmap img = _chickenFrames[0];
                for (int i = 0; i < 3; i++)
                {
                    // Calculate the top position for the current row of chickens
                    topChicken[i] = i * 100 + img.Height; // Each row is spaced 100 pixels apart + the height of the chicken image

                    for (int j = 0; j < 8; j++)
                    {
                        Piece chicken = new Piece(img.Width, img.Height);
                        // Set the image of the chicken to the current frame
                        chicken.Image = img;

                        // Calculate and set the left position of the chicken in the current column
                        chicken.Left = j * 100; // Each column is spaced 100 pixels apart

                        // Set the top position of the chicken in the current row
                        chicken.Top = i * 100 + img.Height;
                        _chickens[i, j] = chicken;
                        Controls.Add(chicken);
                    }
                }
            }
       private void Form1_Load(object sender, EventArgs e)
            {

            }

        
    
    
    
    
    }
  



}


