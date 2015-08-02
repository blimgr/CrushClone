using System;
using System.Collections.Generic;
using System.Threading;

namespace Crush
{
    class Program
    {

        private static void PrintBoard(Board b, int posx, int posy, int? prevrow, int? prevcol)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            b.PrintBoard();
            Console.WriteLine();
            Console.Write("({0},{1})", posy, posx / 2);
            if (prevcol.HasValue)
            {
                Console.Write(" - Selected: ({0},{1})", prevrow, prevcol);
            }
        }
        static void Main(string[] args)
        {
            List<IState> states = new List<IState>();
            states.Add(new State('@' , ConsoleColor.Red));
            states.Add(new State('*', ConsoleColor.Green));
            states.Add(new State('#', ConsoleColor.Yellow));
            states.Add(new State('%', ConsoleColor.Blue));
            states.Add(new State('$', ConsoleColor.White));
            states.Add(new State('&', ConsoleColor.DarkMagenta));
            var height = 30;
            var width = 30;
            Board b = new Board(height, width, states);

            b.InitializeBoard();


            int posx = 0;
            int posy = 0;
            int? prevcol = null, prevrow = null;
            bool draw = true;
            while (true)
            {
                if (draw)
                    PrintBoard(b, posx, posy, prevrow, prevcol);
                draw = true;
                Console.SetCursorPosition(posx, posy);
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.RightArrow:
                        posx += 2;
                        if (posx > 2 * (width - 1))
                            posx = 2 * (width - 1);
                        draw = false;
                        continue;
                    case ConsoleKey.LeftArrow:
                        posx -= 2;
                        if (posx < 0)
                            posx = 0;
                        draw = false;
                        continue;
                    case ConsoleKey.DownArrow:
                        posy += 1;
                        if (posy > height - 1)
                            posy = height - 1;
                        draw = false;
                        continue;
                    case ConsoleKey.UpArrow:
                        posy -= 1;
                        if (posy < 0)
                            posy = 0;
                        draw = false;
                        continue;
                    case ConsoleKey.Enter:
                        if (prevcol == null)
                        {
                            prevcol = posx / 2;
                            prevrow = posy;
                            b.Select(prevrow.Value, prevcol.Value);
                        }
                        else
                        {
                            b.Swap(prevrow.Value, prevcol.Value, posy, posx / 2);
                            bool firsttime = true;
                            while (true)
                            {
                                var list = new List<Tuple<int, int, int, int>>();
                                b.CheckBoard(out list);
                                if (list.Count == 0)
                                {
                                    if (firsttime)
                                        b.Swap(prevrow.Value, prevcol.Value, posy, posx / 2);
                                    break;
                                }
                                else
                                {
                                    b.DeleteFromBoard(list);
                                    PrintBoard(b, posx, posy, prevrow, prevcol);
                                    Thread.Sleep(1000);
                                    b.Gravity();
                                }
                                firsttime = false;
                            }
                            b.Unselect();
                            prevcol = null;
                            prevrow = null;
                        }
                        continue;
                    case ConsoleKey.Q:
                        goto end;
                }

            }
        end:
            Console.ReadKey();
        }
    }
}
