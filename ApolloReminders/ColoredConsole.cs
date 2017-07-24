using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bcd
{
    public class ColoredConsole
    {
        static char DBL_TL = '╔';
        static char DBL_TR = '╗';
        static char DBL_LR = '║';
        static char DBL_LJ = '╠';
        static char DBL_RJ = '╣';
        static char DBL_TB = '═';
        static char DBL_TJ = '╦';
        static char DBL_BJ = '╩';
        static char DBL_CJ = '╬';
        static char DBL_BL = '╚';
        static char DBL_BR = '╝';
        //
        static char SGL_TL = '┌';   //218
        static char SGL_TR = '┐';
        static char SGL_LR = '│';
        static char SGL_LJ = '├';
        static char SGL_RJ = '┤';
        static char SGL_TB = '─';   //196
        static char SGL_TJ = '┬';
        static char SGL_BJ = '┴';
        static char SGL_CJ = '┼';   //197
        static char SGL_BL = '└';
        static char SGL_BR = '┘';   //217
        //
        static char MIX_DTSJ = '╤'; //209
        static char MIX_DBSJ = '╧'; //207
        static char MIX_DLSJ = '╟'; //199
        static char MIX_DRSJ = '╢'; //182
        static char MIX_STDJ = '╥'; //210
        static char MIX_SBDJ = '╨'; //208
        static char MIX_SLDJ = '╞'; //198
        static char MIX_SRDJ = '╡'; //181

        public int ConsoleWidth { get; set; }
        public LineStyle ConsoleLineStyle { get; set; }
        public TextPosition ConsoleTextPosition { get; set; }
        public TextStyle ConsoleTextStyle { get; set; }
        public ConsoleColor ConsoleBackColor { get; set; }
        public ConsoleColor ConsoleForeColor { get; set; }
        public ConsoleColor ConsoleLineColor { get; set; }

        private int AvailableWidth { get; set; }

        #region ***** CONSTRUCTORS *****

        public ColoredConsole(int consoleWidth = 80,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backColor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            this.ConsoleWidth = consoleWidth;
            this.ConsoleLineStyle = lineStyle;
            this.ConsoleTextPosition = textPosition;
            this.ConsoleTextStyle = textStyle;
            this.ConsoleBackColor = ConsoleColor.Black;
            this.ConsoleForeColor = ConsoleColor.White;
            this.ConsoleLineColor = ConsoleColor.Yellow;
            //
            this.AvailableWidth = this.ConsoleWidth - 4;
        }

        #endregion

        # region ***** PUBLIC METHODS *****

        public void DrawTopLine()
        {
            drawTopLine(ConsoleLineStyle);
        }

        public void DrawTopLine(LineStyle lineStyle)
        {
            drawTopLine(lineStyle);
        }

        public void DrawBottomLine()
        {
            drawBottomLine(ConsoleLineStyle);
        }

        public void DrawBottomLine(LineStyle lineStyle)
        {
            drawBottomLine(lineStyle);
        }

        public void DrawSeparator(LineStyle verticalLineStyle, LineStyle horizongalLineStyle)
        {
            if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Single)
                drawVSHSLine();
            else if (verticalLineStyle == LineStyle.Double && horizongalLineStyle == LineStyle.Double)
                drawVDHDLine();
            else if (verticalLineStyle == LineStyle.Single && horizongalLineStyle == LineStyle.Double)
                drawVSHDLine();
            else
                drawVDHSLine();
        }

        public void Write(string message,
            LineStyle lineStyle = LineStyle.Double,
            TextPosition textPosition = TextPosition.Left,
            int tabStop = 0,
            TextStyle textStyle = TextStyle.None,
            ConsoleColor backcolor = ConsoleColor.Black,
            ConsoleColor foreColor = ConsoleColor.White,
            ConsoleColor lineColor = ConsoleColor.Yellow)
        {
            writeLine(message, lineStyle, textPosition, tabStop, textStyle, backcolor, foreColor, lineColor);
        }

        #endregion
        //
        #region ***** PRIVATE METHODS *****

        private void drawTopLine(LineStyle ls)
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            switch (ls)
            {
                case LineStyle.Single:
                    Console.WriteLine($"{SGL_TL}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_TR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_TL}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_TR}");
                    break;
            }
            Console.ResetColor();
        }

        private void drawBottomLine(LineStyle ls)
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            switch (ls)
            {
                case LineStyle.Single:
                    Console.WriteLine($"{SGL_BL}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_BR}");
                    break;
                case LineStyle.Double:
                default:
                    Console.WriteLine($"{DBL_BL}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_BR}");
                    break;
            }
            Console.ResetColor();
        }

        private void drawVDHSLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{MIX_DLSJ}{new string(SGL_TB, ConsoleWidth - 2)}{MIX_DRSJ}");
            Console.ResetColor();
        }

        private void drawVSHDLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{MIX_SLDJ}{new string(DBL_TB, ConsoleWidth - 2)}{MIX_SRDJ}");
            Console.ResetColor();
        }

        private void drawVDHDLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{DBL_LJ}{new string(DBL_TB, ConsoleWidth - 2)}{DBL_RJ}");
            Console.ResetColor();
        }

        private void drawVSHSLine()
        {
            Console.BackgroundColor = ConsoleBackColor;
            Console.ForegroundColor = ConsoleLineColor;
            Console.WriteLine($"{SGL_LJ}{new string(SGL_TB, ConsoleWidth - 2)}{SGL_RJ}");
            Console.ResetColor();
        }

        private void writeLine(string msg,
            LineStyle ls = LineStyle.Double,
            TextPosition tp = TextPosition.Left,
            int tab = 0,
            TextStyle ts = TextStyle.None,
            ConsoleColor bc = ConsoleColor.Black,
            ConsoleColor fc = ConsoleColor.White,
            ConsoleColor lc = ConsoleColor.Yellow)
        {
            msg = msg.Trim();
            char lr;
            if (msg.Length <= AvailableWidth)
            {
                Console.BackgroundColor = bc;
                Console.ForegroundColor = lc;
                if (ls == LineStyle.Double) lr = DBL_LR; else lr = SGL_LR;
                Console.Write($"{lr} ");
                Console.ForegroundColor = fc;
                Console.Write(formatMessage(msg, tp, tab, ts));
                Console.ForegroundColor = lc;
                Console.WriteLine($" {lr}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("....................");
            }
            //
        }

        private string formatMessage(string msg, TextPosition tp, int tab, TextStyle ts)
        {
            switch (ts)
            {
                case TextStyle.Spaced:
                    return padString(msg.Aggregate(string.Empty, (c, i) => c + i + ' '), tp, tab);
                case TextStyle.Caps:
                    return padString(msg.ToUpper(), tp, tab);
                case TextStyle.SpacedCaps:
                    return padString(msg.Aggregate(string.Empty, (c, i) => c + i + ' ').ToUpper(), tp, tab);
                case TextStyle.None:
                default:
                    return padString(msg, tp, tab);
            }
        }

        private string padString(string str, TextPosition tp, int tab)
        {
            // pad the string 
            var padding = AvailableWidth - str.Length;
            switch (tp)
            {
                case TextPosition.Center:
                    return str.PadLeft((padding) / 2 + str.Length).PadRight(AvailableWidth);
                case TextPosition.Right:
                    return str.PadLeft(AvailableWidth);
                case TextPosition.Left:
                default:
                    str = str.PadLeft(str.Length + tab * 4);
                    return str.PadRight(AvailableWidth);
            }
        }

        #endregion

        #region ***** PUBLIC ENUMS *****
        public enum LineStyle
        {
            Single = 1,
            Double = 2
        }

        public enum TextPosition
        {
            Left = 1,
            Center = 2,
            Right = 3
        }

        public enum TextStyle
        {
            Spaced = 1,
            Caps = 2,
            SpacedCaps = 3,
            None = 0
        }

        #endregion
    }
}
