using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CybersecurityBotGUI
{
    public partial class QuizForm : Form
    {
        private class QuizQuestion
        {
            public string Question { get; set; }
            public string[] Options { get; set; }
            public int CorrectIndex { get; set; }
            public string Explanation { get; set; }
        }

        private List<QuizQuestion> questions = new List<QuizQuestion>();
        private int currentQuestionIndex = 0;
        private int score = 0;
        private bool answerChecked = false;

        public QuizForm()
        {
            InitializeComponent();
            LoadQuestions();
            DisplayQuestion();
        }

        private void LoadQuestions()
        {
            questions.Add(new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new[] { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                CorrectIndex = 2,
                Explanation = "Reporting phishing emails helps prevent scams."
            });

            questions.Add(new QuizQuestion
            {
                Question = "True or False: You should use the same password for all accounts.",
                Options = new[] { "True", "False", "", "" },
                CorrectIndex = 1,
                Explanation = "Using unique passwords helps protect each account individually."
            });

            questions.Add(new QuizQuestion
            {
                Question = "What is two-factor authentication (2FA)?",
                Options = new[] {
        "A password that changes daily",
        "A second method to verify identity after entering a password",
        "A backup email address",
        "An antivirus feature"
    },
                CorrectIndex = 1,
                Explanation = "2FA adds an extra layer of security by requiring something you know (password) and something you have (like a code from your phone)."
            });

            questions.Add(new QuizQuestion
            {
                Question = "What is a common sign of a phishing attempt?",
                Options = new[] {
        "Poor grammar or spelling in the email",
        "Official logos and branding",
        "Email from your friend",
        "A short subject line"
    },
                CorrectIndex = 0,
                Explanation = "Phishing emails often contain typos and bad grammar. Always check the sender and the tone of the message."
            });

            questions.Add(new QuizQuestion
            {
                Question = "True or False: Public Wi-Fi networks are always safe to use for online banking.",
                Options = new[] { "True", "False", "", "" },
                CorrectIndex = 1,
                Explanation = "Public Wi-Fi is insecure and can be monitored by attackers. Avoid sensitive actions like online banking on public networks."
            });

            questions.Add(new QuizQuestion
            {
                Question = "What should you do before clicking on a link in an email?",
                Options = new[] {
        "Click it to see where it goes",
        "Check the URL carefully and verify the sender",
        "Forward it to a friend",
        "Bookmark it"
    },
                CorrectIndex = 1,
                Explanation = "Always inspect the link and verify it's legitimate before clicking to avoid phishing traps."
            });

            questions.Add(new QuizQuestion
            {
                Question = "Which of the following is NOT a strong password?",
                Options = new[] {
        "12345678",
        "W!nt3r$2024",
        "H@ppyB!rthd@y#17",
        "9Lx#M22!w$Zp"
    },
                CorrectIndex = 0,
                Explanation = "Common passwords like '12345678' are easy to guess and highly insecure."
            });

            questions.Add(new QuizQuestion
            {
                Question = "What is social engineering in cybersecurity?",
                Options = new[] {
        "Engineering software for social media",
        "Manipulating people to give up confidential information",
        "Building secure bridges",
        "Protecting online games"
    },
                CorrectIndex = 1,
                Explanation = "Social engineering is when attackers trick or manipulate people into giving up sensitive information."
            });

            questions.Add(new QuizQuestion
            {
                Question = "What does HTTPS in a website URL indicate?",
                Options = new[] {
        "The site is hosted in another country",
        "The connection is encrypted and secure",
        "The site has been online for years",
        "The site is only accessible by Google"
    },
                CorrectIndex = 1,
                Explanation = "HTTPS uses encryption to protect your data as it travels between your browser and the website."
            });

            questions.Add(new QuizQuestion
            {
                Question = "Which is the best practice when creating passwords?",
                Options = new[] {
        "Use your birthdate",
        "Re-use old passwords",
        "Use a mix of letters, numbers, and symbols",
        "Use the same password for every site"
    },
                CorrectIndex = 2,
                Explanation = "Strong passwords should be complex and unique for every account."
            });

        }

        private void DisplayQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                EndQuiz();
                return;
            }

            var q = questions[currentQuestionIndex];
            lblQuestion.Text = q.Question;
            rbOption1.Text = q.Options[0];
            rbOption2.Text = q.Options[1];
            rbOption3.Text = q.Options[2];
            rbOption4.Text = q.Options[3];

            rbOption1.Checked = rbOption2.Checked = rbOption3.Checked = rbOption4.Checked = false;
            lblFeedback.Text = "";
            lblScore.Text = $"Score: {score}/{questions.Count}";

            EnableRadioButtons();
            btnNext.Text = "Check Answer";
            answerChecked = false;
        }

        private int GetSelectedOptionIndex()
        {
            if (rbOption1.Checked) return 0;
            if (rbOption2.Checked) return 1;
            if (rbOption3.Checked) return 2;
            if (rbOption4.Checked) return 3;
            return -1;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!answerChecked)
            {
                int selected = GetSelectedOptionIndex();
                if (selected == -1)
                {
                    MessageBox.Show("Please select an answer before proceeding.");
                    return;
                }

                var q = questions[currentQuestionIndex];

                if (selected == q.CorrectIndex)
                {
                    score++;
                    lblFeedback.Text = "✅ Correct! " + q.Explanation;
                }
                else
                {
                    lblFeedback.Text = "❌ Incorrect. " + q.Explanation;
                }

                lblScore.Text = $"Score: {score}/{questions.Count}";
                answerChecked = true;
                btnNext.Text = "Next Question";
                DisableRadioButtons();
            }
            else
            {
                currentQuestionIndex++;
                DisplayQuestion();
            }
        }

        private void DisableRadioButtons()
        {
            rbOption1.Enabled = false;
            rbOption2.Enabled = false;
            rbOption3.Enabled = false;
            rbOption4.Enabled = false;
        }

        private void EnableRadioButtons()
        {
            rbOption1.Enabled = true;
            rbOption2.Enabled = true;
            rbOption3.Enabled = true;
            rbOption4.Enabled = true;
        }

        private void EndQuiz()
        {
            MessageBox.Show($"Quiz Completed!\nYou scored {score} out of {questions.Count}.",
                            "Quiz Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close(); // or reset the quiz if you want
        }
    }
}
