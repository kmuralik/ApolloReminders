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
            cc.Write("Reminders to Run:",foreColor: ConsoleColor.Cyan);
            cc.Write("4 reminders found", textPosition: ColoredConsole.TextPosition.Right, foreColor: ConsoleColor.Gray);
            cc.DrawSeparator(ColoredConsole.LineStyle.Double, ColoredConsole.LineStyle.Single);
            cc.Write("End", textPosition: ColoredConsole.TextPosition.Center);
            cc.DrawBottomLine();
            Console.ReadLine();
        }
    }
}
