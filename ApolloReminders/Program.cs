using System;
using System.Data;
using abcd = bcd.ColoredConsole;

namespace ApolloReminders
{
    class Program
    {
        static void Main(string[] args)
        {
            var cc = new abcd();
            var reminders = new Reminders();
            //
            cc.DrawBox("Apollo Reminders", abcd.LineStyle.Double, abcd.TextPosition.Center, 0, abcd.TextStyle.SpacedCaps, ConsoleColor.DarkBlue, ConsoleColor.Green, ConsoleColor.Yellow);

            cc.DrawTopLine();
            cc.Write("[A] Get Current Reminders", foreColor: ConsoleColor.Cyan);
            // get all reminders that need to run today
            var dtReminders = reminders.GetReminders();
            var todayCount = dtReminders.Rows.Count;
            // for day
            //cc.Write("[A] Get Today's Reminders", foreColor: ConsoleColor.Cyan);
            // for hour
            cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Double);
            // get the count and display
            cc.Write($"A total of {todayCount} reminders found", textPosition: abcd.TextPosition.Left, tabStop: 1, foreColor: ConsoleColor.Gray);
            cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Double);
            // loop through each reminder
            cc.Write("[B] Run each reminder", foreColor: ConsoleColor.Cyan);
            cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Double);
            // foreach
            var reminderCount = 1;
            foreach (DataRow rRow in dtReminders.Rows)
            {
                // display reminder details and schedule time 
                cc.Write($"{reminderCount++}. [{rRow["ReminderRunDate"].ToString()}] {rRow["ReminderName"].ToString()}", tabStop: 1, foreColor: ConsoleColor.Green);
                cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Single);
                // run the associated procedure to get instance details
                var dtInstances = reminders.GetInstances(rRow);
                cc.Write($"Found {dtInstances.Rows.Count} instances", tabStop: 2, foreColor: ConsoleColor.Gray);
                foreach (DataRow iRow in dtInstances.Rows)
                {
                    // foreach instance send reminder
                    var dataId = reminders.SendMail(iRow, int.Parse(rRow["RuleId"].ToString()));
                    if (dataId > 0)
                        cc.Write($"{iRow["request_no"].ToString()} - Reminder Sent. Ref No: {dataId}", tabStop: 3, foreColor: ConsoleColor.Red);

                }
                cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Single);
            }
            cc.Write("All reminders whether they are sent successfully or not will be available in 'ReminderData' table for reference along with the mail content.");
            cc.Write("Press 'Enter' to quit");
            cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Double);
            cc.Write("End", textPosition: abcd.TextPosition.Center);
            cc.DrawBottomLine();
            Console.ReadLine();
        }
    }
}
