
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CybersecurityBotGUI
{
    public partial class Form1 : Form
    {
        // Stores chatbot actions with timestamps
        private List<string> activityLog = new List<string>();
        private List<TaskItem> tasks = new List<TaskItem>();
        private TextBox txtUserInput;
        private Button btnSend;
        private RichTextBox rtbChat;
        private TextBox txtTaskTitle;
        private TextBox txtTaskDesc;
        private DateTimePicker dtpReminder;
        private ListBox lstTasks;
        private Button btnAddTask;
        private Button btnComplete;
        private Button btnQuiz;
        private Label label1;
        private Label label2;
        private Label label3;
        private ChatBot bot = new ChatBot();
        private TaskItem lastTaskAwaitingReminder = null;


        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string userInput = txtUserInput.Text.Trim();
            if (!string.IsNullOrEmpty(userInput))
            {
                rtbChat.AppendText("You: " + userInput + "\n");

                bool handledByNLP = ProcessNLPInput(userInput);

                if (!handledByNLP)
                {
                    string botResponse = bot.Respond(userInput);
                    rtbChat.AppendText("Bot: " + botResponse + "\n\n");
                }

                txtUserInput.Clear();
            }
        }

        private void LogActivity(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            activityLog.Add($"{timestamp}: {message}");

            // Keep only the latest 10 actions
            if (activityLog.Count > 10)
            {
                activityLog.RemoveAt(0);
            }
        }

        private DateTime? ParseDateFromInput(string input)
        {
            input = input.ToLower();

            if (input.Contains("tomorrow"))
            {
                return DateTime.Now.AddDays(1);
            }

            // Check for "in X days"
            var match = System.Text.RegularExpressions.Regex.Match(input, @"in (\d+) days?");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            {
                return DateTime.Now.AddDays(days);
            }

            // Try natural language parsing
            if (DateTime.TryParse(input, out DateTime parsedDate))
            {
                return parsedDate;
            }

            return null;
        }


        private void btnAddTask_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTaskTitle.Text)) return;

            var task = new TaskItem
            {
                Title = txtTaskTitle.Text.Trim(),
                Description = txtTaskDesc.Text.Trim(),
                Reminder = dtpReminder.Checked ? dtpReminder.Value : (DateTime?)null,
                IsCompleted = false
            };

            tasks.Add(task);
            lstTasks.Items.Add(task);

            rtbChat.AppendText($"ChatBot: Task added - \"{task.Title}\".\n");
            if (task.Reminder.HasValue)
            {
                rtbChat.AppendText($"ChatBot: Got it! I'll remind you on {task.Reminder.Value.ToShortDateString()}.\n");
            }

            txtTaskTitle.Clear();
            txtTaskDesc.Clear();
            dtpReminder.Value = DateTime.Now;
            dtpReminder.Checked = false;
            LogActivity($"Task added: '{task.Title}'");
            LogActivity($"Reminder set for task '{task.Title}' on {task.Reminder?.ToShortDateString()}");

        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem == null)
            {
                MessageBox.Show("Please select a task to complete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            
            TaskItem selectedTask = (TaskItem)lstTasks.SelectedItem;
            tasks.Remove(selectedTask);
            lstTasks.Items.Remove(selectedTask);

            rtbChat.AppendText($"Task completed: {selectedTask.Title}\n");
        }

        private bool ProcessNLPInput(string input)
        {


            
            string lower = input.ToLower();
            //Check if user is following up with reminder info
            if (lastTaskAwaitingReminder != null)
            {
                DateTime? parsed = ParseDateFromInput(input);
                if (parsed != null)
                {
                    lastTaskAwaitingReminder.Reminder = parsed;
                    rtbChat.AppendText($"Great! Reminder set for '{lastTaskAwaitingReminder.Title}' on {parsed?.ToShortDateString()}.\n");
                    LogActivity($"Reminder set for task '{lastTaskAwaitingReminder.Title}' on {parsed?.ToShortDateString()}");
                    lastTaskAwaitingReminder = null; // Clear after setting
                    return true;
                }
                else if (lower.Contains("no") || lower.Contains("not now"))
                {
                    rtbChat.AppendText("No problem! You can always set a reminder later.\n");
                    lastTaskAwaitingReminder = null;
                    return true;
                }
                else
                {
                    rtbChat.AppendText("Sorry, I couldn't understand the date. Try something like 'tomorrow' or 'in 3 days'.\n");
                    return true;
                }
            }

            if (input.ToLower().Contains("activity log") || input.ToLower().Contains("what have you done"))
            {
                rtbChat.AppendText("Here's a summary of recent actions:\n");

                foreach (var log in activityLog)
                {
                    rtbChat.AppendText("- " + log + "\n");
                }

                return true;
            }


            // Task/Reminder detection
            if ((lower.Contains("add") || lower.Contains("create") || lower.Contains("set")) &&
                (lower.Contains("task") || lower.Contains("reminder")))
            {
                // Extract title
                string title = System.Text.RegularExpressions.Regex.Replace(
    input.ToLower(),
    @"^(add|set|create)\s+(a\s+)?(task|reminder)?\s*","", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();


                if (string.IsNullOrWhiteSpace(title))
                {
                    rtbChat.AppendText("What would you like the task to be?\n");
                }
                else
                {
                    var task = new TaskItem
                    {
                        Title = title,
                        Description = "Auto-added via chat NLP",
                    };
                    //Parse reminder from input
                    DateTime? reminderDate = ParseDateFromInput(input);
                    if (reminderDate != null)
                    {
                        task.Reminder = reminderDate;
                        rtbChat.AppendText($"Reminder set for '{task.Title}' on {reminderDate?.ToShortDateString()}.\n");
                        LogActivity($"Task added: '{task.Title}' with reminder on {reminderDate?.ToShortDateString()}");
                    }
                    else
                    {
                        rtbChat.AppendText($"Task added: {task.Title}. Would you like to set a reminder?\n");
                        LogActivity($"Task added: '{task.Title}' without reminder");
                        lastTaskAwaitingReminder = task;
                    }
                    tasks.Add(task);
                    lstTasks.Items.Add(task);
                }

                return true;
            }



            // Quiz detection
            if (lower.Contains("quiz") || lower.Contains("test") || lower.Contains("cyber quiz"))
            {
                rtbChat.AppendText("Launching the cybersecurity quiz now...\n");
                QuizForm quiz = new QuizForm();
                quiz.ShowDialog();
                LogActivity("Quiz started.");

                return true;
            }

            // Password help 
            if (lower.Contains("password") && (lower.Contains("help") || lower.Contains("change") || lower.Contains("update")))
            {
                rtbChat.AppendText("It's important to use strong, unique passwords. Consider enabling 2FA too!\n");
                return true;
            }



           
            return false;
        }


        private void InitializeComponent()
        {
            this.txtUserInput = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.txtTaskTitle = new System.Windows.Forms.TextBox();
            this.txtTaskDesc = new System.Windows.Forms.TextBox();
            this.dtpReminder = new System.Windows.Forms.DateTimePicker();
            this.lstTasks = new System.Windows.Forms.ListBox();
            this.btnAddTask = new System.Windows.Forms.Button();
            this.btnComplete = new System.Windows.Forms.Button();
            this.btnQuiz = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtUserInput
            // 
            this.txtUserInput.AccessibleName = "txtUserInput";
            this.txtUserInput.Location = new System.Drawing.Point(108, 451);
            this.txtUserInput.Name = "txtUserInput";
            this.txtUserInput.Size = new System.Drawing.Size(374, 20);
            this.txtUserInput.TabIndex = 0;
            this.txtUserInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnSend
            // 
            this.btnSend.AccessibleName = "btnSend";
            this.btnSend.Location = new System.Drawing.Point(501, 449);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // rtbChat
            // 
            this.rtbChat.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.rtbChat.Location = new System.Drawing.Point(99, 319);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.ReadOnly = true;
            this.rtbChat.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbChat.Size = new System.Drawing.Size(477, 109);
            this.rtbChat.TabIndex = 2;
            this.rtbChat.Text = "";
            this.rtbChat.TextChanged += new System.EventHandler(this.rtbChat_TextChanged);
            // 
            // txtTaskTitle
            // 
            this.txtTaskTitle.AccessibleDescription = "";
            this.txtTaskTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTaskTitle.Location = new System.Drawing.Point(96, 126);
            this.txtTaskTitle.Name = "txtTaskTitle";
            this.txtTaskTitle.Size = new System.Drawing.Size(202, 20);
            this.txtTaskTitle.TabIndex = 3;
            this.txtTaskTitle.TextChanged += new System.EventHandler(this.txtTaskTitle_TextChanged);
            // 
            // txtTaskDesc
            // 
            this.txtTaskDesc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTaskDesc.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtTaskDesc.Location = new System.Drawing.Point(94, 167);
            this.txtTaskDesc.Name = "txtTaskDesc";
            this.txtTaskDesc.Size = new System.Drawing.Size(202, 20);
            this.txtTaskDesc.TabIndex = 4;
            this.txtTaskDesc.TextChanged += new System.EventHandler(this.txtTaskDesc_TextChanged);
            // 
            // dtpReminder
            // 
            this.dtpReminder.Location = new System.Drawing.Point(96, 211);
            this.dtpReminder.Name = "dtpReminder";
            this.dtpReminder.Size = new System.Drawing.Size(200, 20);
            this.dtpReminder.TabIndex = 5;
            // 
            // lstTasks
            // 
            this.lstTasks.FormattingEnabled = true;
            this.lstTasks.Location = new System.Drawing.Point(333, 110);
            this.lstTasks.Name = "lstTasks";
            this.lstTasks.Size = new System.Drawing.Size(298, 121);
            this.lstTasks.TabIndex = 6;
            this.lstTasks.SelectedIndexChanged += new System.EventHandler(this.lstTasks_SelectedIndexChanged);
            // 
            // btnAddTask
            // 
            this.btnAddTask.Location = new System.Drawing.Point(152, 250);
            this.btnAddTask.Name = "btnAddTask";
            this.btnAddTask.Size = new System.Drawing.Size(75, 23);
            this.btnAddTask.TabIndex = 7;
            this.btnAddTask.Text = "Add Task";
            this.btnAddTask.UseVisualStyleBackColor = true;
            this.btnAddTask.Click += new System.EventHandler(this.btnAddTask_Click);
            // 
            // btnComplete
            // 
            this.btnComplete.Location = new System.Drawing.Point(637, 123);
            this.btnComplete.Name = "btnComplete";
            this.btnComplete.Size = new System.Drawing.Size(75, 23);
            this.btnComplete.TabIndex = 8;
            this.btnComplete.Text = "Complete";
            this.btnComplete.UseVisualStyleBackColor = true;
            this.btnComplete.Click += new System.EventHandler(this.btnComplete_Click);
            // 
            // btnQuiz
            // 
            this.btnQuiz.Location = new System.Drawing.Point(637, 448);
            this.btnQuiz.Name = "btnQuiz";
            this.btnQuiz.Size = new System.Drawing.Size(75, 23);
            this.btnQuiz.TabIndex = 9;
            this.btnQuiz.Text = "Quiz";
            this.btnQuiz.UseVisualStyleBackColor = true;
            this.btnQuiz.Click += new System.EventHandler(this.btnQuiz_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("OCR A Extended", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(108, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(582, 49);
            this.label1.TabIndex = 10;
            this.label1.Text = "CyberScurity BOT GUI";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Task Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(96, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Task Description";
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(765, 526);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnQuiz);
            this.Controls.Add(this.btnComplete);
            this.Controls.Add(this.btnAddTask);
            this.Controls.Add(this.lstTasks);
            this.Controls.Add(this.dtpReminder);
            this.Controls.Add(this.txtTaskDesc);
            this.Controls.Add(this.txtTaskTitle);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtUserInput);
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtTaskTitle_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnQuiz_Click(object sender, EventArgs e)
        {
            QuizForm quiz = new QuizForm();
            quiz.ShowDialog();
            LogActivity("Quiz started.");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void txtTaskDesc_TextChanged(object sender, EventArgs e)
        {

        }

        private void rtbChat_TextChanged(object sender, EventArgs e)
        {

        }

        private void lstTasks_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
