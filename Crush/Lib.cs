using System;
using System.Collections.Generic;
using System.Text;

namespace Crush
{
    public interface IState : ICloneable, IEquatable<IState>
    {
        ConsoleColor GetColor();
        String GetChar();

    }

    public class Tuple<T1, T2, T3, T4>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        } 
    }
    public class State : IState
    {
        private ConsoleColor _color;
        private char _state;
        private bool _isSelected;

        public ConsoleColor GetColor()
        {
            return _color;
        }
        public String GetChar()
        {
            return _state.ToString();
        }

        public object Clone()
        {
            return new State(_state, _color);
        }
        public State(char state, ConsoleColor color)
        {
            _state = state;
            _color = color;
        }
        public bool Equals(IState other)
        {
            if (!(other is State))
                return false;
            return _state == (other as State)._state;
        }

        public override string ToString()
        {
            return _state.ToString() + " ";
        }
    }


    public class Board
    {
        private Random _random;
        private int _width;
        private int _height;
        private List<IState> _states;
        private IState[,] _boardMatrix;

        public void Gravity()
        {
            for (int i = 0; i < _width; i++)
                GravityColumn(i);
        }

        private void GravityColumn(int col)
        {
            int lowIndex = _height - 1;
            int highIndex = _height - 2;

            while (lowIndex >= 0 && highIndex >= 0)
            {
                if (_boardMatrix[lowIndex, col] != null)
                {
                    lowIndex--;
                    if (highIndex == lowIndex)
                        highIndex--;
                }
                else if (_boardMatrix[highIndex, col] == null)
                {
                    highIndex--;
                }
                else
                {

                    _boardMatrix[lowIndex, col] = _boardMatrix[highIndex, col];
                    _boardMatrix[highIndex, col] = null;
                    lowIndex--;
                    highIndex--;
                }
            }

        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int k = 0; k < _height; k++)
            {
                for (int i = 0; i < _width; i++)
                    sb.Append(_boardMatrix[k, i] == null ? "  " : _boardMatrix[k, i].ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private int? row;
        private int? col;
        public void Select(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public void Unselect()
        {
            row = null;
            col = null;
        }
        public void PrintBoard()
        {
            for (int k = 0; k < _height; k++)
            {
                for (int i = 0; i < _width; i++)
                    if (_boardMatrix[k, i] == null)
                    {
                        Console.Write("  ");
                    }
                    else
                    {
                        var back = Console.BackgroundColor;
                        if (row == k && col == i)
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                        }
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = _boardMatrix[k, i].GetColor();
                        Console.Write(_boardMatrix[k, i].GetChar() + " ");
                        Console.ForegroundColor = color;
                        Console.BackgroundColor = back;
                    }
                Console.WriteLine();
            }
        }

        public Board(int h, int w, List<IState> states)
        {
            _random = new Random((int)DateTime.Now.Ticks);
            _width = w;
            _height = h;
            _boardMatrix = new IState[h, w];

            _states = new List<IState>(states);
        }

        public void InitializeBoard()
        {
            for (int k = 0; k < _height; k++)
                for (int i = 0; i < _width; i++)
                    InitializePosition(k, i);
        }

        public void InitializePosition(int row, int col)
        {
            var allowedStates = FindAllowedStatesForInit(row, col);
            if (allowedStates.Count == 0)
                throw new Exception("Not possible to initialize the matrix with so few states");
            _boardMatrix[row, col] = allowedStates[_random.Next(allowedStates.Count)];
        }


        private IState Offset(int row, int col, int voffset, int hoffset)
        {
            var nrow = row + voffset;
            var ncol = col + hoffset;
            if (nrow < 0 || ncol < 0 || nrow >= _height || ncol >= _width)
                return null;
            return _boardMatrix[nrow, ncol];
        }

        private IState VerticalOffset(int row, int col, int offset)
        {
            return Offset(row, col, offset, 0);
        }

        private IState HorizontalOffset(int row, int col, int offset)
        {
            return Offset(row, col, 0, offset);
        }

        public List<IState> FindAllowedStatesForInit(int row, int col)
        {
            var retVal = new List<IState>(_states);

            var above_one = VerticalOffset(row, col, -1);
            var above_two = VerticalOffset(row, col, -2);
            var left_one = HorizontalOffset(row, col, -1);
            var left_two = HorizontalOffset(row, col, -2);

            if (above_one != null && above_two != null && above_one.Equals(above_two))
                retVal.Remove(above_one);
            if (left_one != null && left_two != null && left_one.Equals(left_two))
                retVal.Remove(left_one);
            return retVal;
        }

        public bool Swap(int row1, int col1, int row2, int col2)
        {
            if (_boardMatrix[row1, col1] == null || _boardMatrix[row2, col2] == null)
                return false;
            if (Math.Abs(row2 - row1) == 1 ^ Math.Abs(col2 - col1) == 1)
            {
                var temp = _boardMatrix[row1, col1];
                _boardMatrix[row1, col1] = _boardMatrix[row2, col2];
                _boardMatrix[row2, col2] = temp;
                return true;
            }
            return false;
        }

        public void DeleteFromBoard(List<Tuple<int, int, int, int>> pairs)
        {
            foreach (var t in pairs)
            {
                for (int i = t.Item1; i <= t.Item3; i++)
                    for (int k = t.Item2; k <= t.Item4; k++)
                        _boardMatrix[i, k] = null;
            }
        }
        public void CheckBoard(out List<Tuple<int, int, int, int>> pairs)
        {
            pairs = new List<Tuple<int, int, int, int>>();
            int startrow, startcol = 0;
            IState prev = null;

            // Check Rows
            for (int k = 0; k < _height; k++)
            {
                startrow = k;
                startcol = 0;
                prev = null;
                for (int i = 0; i < _width; i++)
                {
                    if (prev == null || !prev.Equals(_boardMatrix[k, i]))
                    {
                        if (i - startcol > 2)
                        {
                            pairs.Add(new Tuple<int, int, int, int>(startrow, startcol, k, i - 1));
                        }
                        startcol = i;
                    }
                    prev = _boardMatrix[k, i];
                }
                if (_width - startcol > 2)
                {
                    pairs.Add(new Tuple<int, int, int, int>(startrow, startcol, k, _width - 1));
                }
            }

            // Check Columns
            for (int i = 0; i < _width; i++)
            {
                startrow = 0;
                startcol = i;
                prev = null;

                for (int k = 0; k < _height; k++)
                {
                    if (prev == null || !prev.Equals(_boardMatrix[k, i]))
                    {
                        if (k - startrow > 2)
                        {
                            pairs.Add(new Tuple<int, int, int, int>(startrow, startcol, k - 1, i));
                        }
                        startrow = k;
                    }
                    prev = _boardMatrix[k, i];
                }
                if (_height - startrow > 2)
                {
                    pairs.Add(new Tuple<int, int, int, int>(startrow, startcol, _height - 1, i));
                }
            }

        }
    }
}
