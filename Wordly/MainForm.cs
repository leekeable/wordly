namespace Wordly
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        private TextBox currentTextBox;
        private string word = string.Empty;
        private int guesses = 0;
        
        // hack to allow the backspace button to clear the text box
        private bool clearTextBox = false;

        //private TextBox[] Word1, Word2, Word3, Word4, Word5;
        //private List<TextBox[]> Guesses;

        private readonly string[] words;
        public MainForm()
        {
            InitializeComponent();

            //Word1 = new TextBox[] { txtWord1Letter1, txtWord1Letter2, txtWord1Letter3, txtWord1Letter4, txtWord1Letter5 };
            //Word2 = new TextBox[] { txtWord2Letter1, txtWord2Letter2, txtWord2Letter3, txtWord2Letter4, txtWord2Letter5 };
            //Word3 = new TextBox[] { txtWord3Letter1, txtWord3Letter2, txtWord3Letter3, txtWord3Letter4, txtWord3Letter5 };
            //Word4 = new TextBox[] { txtWord4Letter1, txtWord4Letter2, txtWord4Letter3, txtWord4Letter4, txtWord4Letter5 };
            //Word5 = new TextBox[] { txtWord5Letter1, txtWord5Letter2, txtWord5Letter3, txtWord5Letter4, txtWord5Letter5 };

            //Guesses = new List<TextBox[]> { Word1, Word2, Word3, Word4, Word5 };

            string dictionaryJson = ReadDictionary();//File.ReadAllText("dictionary.json");

            var jarray = (JArray)JsonConvert.DeserializeObject(dictionaryJson);
            words = jarray.ToObject<string[]>();
            
            var random = new Random();
            word = words[random.Next(words.Length-1)];
            //this.Text = word;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.currentTextBox = txtWord1Letter1;
        }

        private void btnLetter_Click(object sender, EventArgs e)
        {
            this.currentTextBox.Text = ((Button)sender).Text;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            clearTextBox = true;
            this.currentTextBox.Text = string.Empty;
            this.SelectNextControl(this.currentTextBox, false, true, false, false);
        }

        private void txtGuess_Enter(object sender, EventArgs e)
        {
            this.currentTextBox = (TextBox)sender;
            if (clearTextBox == true)
            {
                this.currentTextBox.Text = string.Empty;
                clearTextBox = false;   
            }
        }

        private void txtGuess_TextChanged(object sender, EventArgs e)
        {
            var s = (TextBox)sender;
            s.Text = s.Text.ToUpper();
            if (s.Text.Length == s.MaxLength)
            {
                this.SelectNextControl(s, true, true, false, false);
            }
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            if (!ValidWord())
            {
                MessageBox.Show(this, "Invalid word", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (var i = 0; i <= 4; i++)
            {
                var guess = Controls.OfType<TextBox>()
                    .Where(t => t.TabIndex == (guesses * 5) + i)
                    .Select(t => t)
                    .First();
                var letter = word.Substring(i, 1).ToLower();
                var button = Controls.OfType<Button>()
                    .Where(t => t.Text.ToLower() == guess.Text.ToLower())
                    .Select(t => t)
                    .First();

                // right letter right position
                if (guess.Text.ToLower() == letter)
                {
                    guess.BackColor = Color.LightGreen;
                    guess.ForeColor = Color.White;
                    button.BackColor = Color.LightGreen;
                }
                // right letter wrong position
                else if (word.Contains(guess.Text.ToLower()))
                {
                    guess.BackColor = Color.Orange;
                    guess.ForeColor = Color.Black;
                    button.BackColor = Color.Orange;
                }
                else
                {
                    guess.BackColor = Color.DarkViolet;
                    guess.ForeColor = Color.White;
                    button.BackColor = Color.DarkViolet;
                }
                guess.Enabled = false;
                Thread.Sleep(500);
                Application.DoEvents();
            }

            // disable all textboxes in this guess
            EnableGuessBoxes(false, guesses);
            guesses++;
            EnableGuessBoxes(true, guesses);

            // enmable all textboxes for the next guess
            Debug.WriteLine(guesses);
        }

        private void txtGuess_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox currentContainer = ((TextBox)sender);
            if (e.KeyCode == Keys.Back)
            {
                clearTextBox = true;
                this.SelectNextControl(currentContainer, false, true, false, false);
            };
        }

        private void EnableGuessBoxes(bool enable, int guessCount)
        {
            for (int i = 0; i <= 4; i++)
            {
                var guess = Controls.OfType<TextBox>()
                    .Where(t => t.TabIndex == (guessCount * 5) + i)
                    .Select(t => t)
                    .First();
                guess.Enabled = enable;
            }
        }

        private bool ValidWord()
        {
            var guessedWord = string.Empty;

            // build the word from the textboxes for this guess
            for (var i = 0; i <= 4; i++)
            {
                var guess = Controls.OfType<TextBox>()
                    .Where(t => t.TabIndex == (guesses * 5) + i)
                    .Select(t => t)
                    .First();
                guessedWord += guess.Text.ToLower();
            }

            // if its not a valid word, clear the textboxes and move focus to the first one for this guess
            if (!words.Contains(guessedWord))
            {
                for (var i = 0; i <= 4; i++)
                {
                    var guess = Controls.OfType<TextBox>()
                        .Where(t => t.TabIndex == (guesses * 5) + i)
                        .Select(t => t)
                        .First();
                    guess.Text = string.Empty;
                }
                var firstletter = Controls.OfType<TextBox>()
                    .Where(t => t.TabIndex == (guesses * 5))
                    .Select(t => t)
                    .First();
                firstletter.Focus();

                return false;
            }

            // its a valid word
            return true;
        }
        private string ReadDictionary()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Wordly.dictionary.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}
