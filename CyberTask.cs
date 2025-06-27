using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CybersecurityBotGUI
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Reminder { get; set; }
        public bool IsCompleted { get; set; }
        public override string ToString()
        {
            return $"{Title} - {Description}" + (Reminder.HasValue ? $" (Remind on {Reminder.Value.ToShortDateString()})" : "");
        }
    }
}
   