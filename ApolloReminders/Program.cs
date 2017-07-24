using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bcd;

namespace ApolloReminders
{
    class Program
    {
        static void Main(string[] args)
        {
            var cc = new bcd.ColoredConsole();
            cc.DrawTopLine();
            cc.Write("Apollo Reminders", textPosition: ColoredConsole.TextPosition.Center, textStyle: ColoredConsole.TextStyle.SpacedCaps);
            cc.DrawSeparator(ColoredConsole.LineStyle.Double, ColoredConsole.LineStyle.Double);
            // get all reminders that need to run today            
            cc.Write("[A] Get Today's Reminders",foreColor: ConsoleColor.Cyan);
            // get the count and display
            cc.Write("A total of 4 reminders found", textPosition: ColoredConsole.TextPosition.Left, tabStop: 1, foreColor: ConsoleColor.Gray);
            cc.DrawSeparator(ColoredConsole.LineStyle.Double, ColoredConsole.LineStyle.Single);
            // loop through each reminder
            cc.Write("[B] Run each reminder", foreColor: ConsoleColor.Cyan);
            // foreach
            // display remindeer name and schedule time
            var reminderName = "First Reminder"; var nextReminderDate = "2017-Jul-25 0800 hrs";
            cc.Write($"01. {reminderName} : {nextReminderDate}", tabStop: 1, foreColor: ConsoleColor.Green);
            // run the associated procedure to get instance details
            cc.Write($"Found 2 instances", tabStop: 2, foreColor: ConsoleColor.Gray);
            // foreach instance send reminder
            var requestNo = "2017051400003";
            cc.Write($"{requestNo} - Reminder Sent",tabStop: 3, foreColor: ConsoleColor.Red);
            requestNo = "2017062300016";
            cc.Write($"{requestNo} - Reminder Sent",tabStop: 3, foreColor: ConsoleColor.Red);
            reminderName = "Second Reminder"; nextReminderDate = "2017-Jul-25 0800 hrs";
            cc.Write($"01. {reminderName} : {nextReminderDate}", tabStop: 1, foreColor: ConsoleColor.Green);
            // run the associated procedure to get instance details
            cc.Write($"Found 2 instances", tabStop: 2, foreColor: ConsoleColor.Gray);
            // foreach instance send reminder
            requestNo = "2017051400003";
            cc.Write($"{requestNo} - Reminder Sent", tabStop: 3, foreColor: ConsoleColor.Red);
            requestNo = "2017062300016";
            cc.Write($"{requestNo} - Reminder Sent", tabStop: 3, foreColor: ConsoleColor.Red);

            cc.DrawSeparator(ColoredConsole.LineStyle.Double, ColoredConsole.LineStyle.Single);
            cc.Write("End", textPosition: ColoredConsole.TextPosition.Center);
            cc.DrawBottomLine();
            Console.ReadLine();
        }
    }
}
