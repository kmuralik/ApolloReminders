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
            // get all reminders that need to run today
            var dtReminders = reminders.GetReminders();
            var todayCount = dtReminders.Rows.Count;
            cc.Write("[A] Get Today's Reminders", foreColor: ConsoleColor.Cyan);
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
                    var status = reminders.SendMail(iRow);
                    cc.Write($"{iRow["RequestNo"].ToString()} - {iRow["LastReminderDate"].ToString()} Reminder Sent", tabStop: 3, foreColor: ConsoleColor.Red);

                }
                cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Single);
            }
            cc.Write($"This is a big message to test how lengthy messages will span itself into multiple lines. This sample should span into 2 lines.");
            cc.DrawSeparator(abcd.LineStyle.Double, abcd.LineStyle.Double);
            cc.Write("End", textPosition: abcd.TextPosition.Center);
            cc.DrawBottomLine();
            Console.ReadLine();
        }
    }
}
