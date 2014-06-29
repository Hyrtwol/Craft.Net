using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Craft.Net.Anvil.Test
{
    internal class Labyrinth
    {
        struct Point
        {
            internal int X, Y;
        }
        private const int CellSize = 4;
        private readonly int CellSideCount, TotalCells;
        //private readonly int[,] _cells;
        private readonly Stack<Point> _moves;
        //private Point _pos;
        private readonly Random _random = new Random();

        // direction lookup table packed so first pair is (0,1)=down then (1,0)=right then (0,-1)=up and last (-1,0)=left by wrapping
        private readonly int[] _direction = new[] { 0, 1, 0, -1 };
        private readonly int[] _directionS = new[] { 1, 8, 4, 2 };
        private readonly int[] _directionE = new[] { 4, 2, 1, 8 };
        private int[,,] _tiles;
        public readonly int Dimension;

        public Labyrinth(int size)
        {
            CellSideCount = size;
            TotalCells = CellSideCount * CellSideCount;
            Dimension = CellSideCount * CellSize;
            //_cells = new int[CellSideCount, CellSideCount];
            //_pos = new Point {X = CellSideCount/2, Y = CellSideCount/2};
            _moves = new Stack<Point>(TotalCells);
            //_moves.Push(_pos);

            var tiles = new int[16, CellSize, CellSize];
            for (int tile = 1; tile < 16; tile++)
            {
                int b = tile & 1;
                int l = (tile >> 1) & 1;
                int t = (tile >> 2) & 1;
                int r = (tile >> 3) & 1;
                tiles[tile, 0, 1] = t;
                tiles[tile, 0, 2] = t;
                tiles[tile, 1, 0] = l;
                tiles[tile, 2, 0] = l;
                tiles[tile, 1, 3] = r;
                tiles[tile, 2, 3] = r;
                tiles[tile, 3, 1] = b;
                tiles[tile, 3, 2] = b;
                // center
                tiles[tile, 1, 1] = 1;
                tiles[tile, 1, 2] = 1;
                tiles[tile, 2, 1] = 1;
                tiles[tile, 2, 2] = 1;
            }
            _tiles = tiles;
        }


        public int[,] Generate()
        {
            Debug.Print("Generate {0}", TotalCells);
            _moves.Clear();
            var _pos = new Point { X = CellSideCount / 2, Y = CellSideCount / 2 };
            //Debug.Print("Push {0} {1}", _pos.X, _pos.Y);
            _moves.Push(_pos);
            var _cells = new int[CellSideCount, CellSideCount];
            int cellCounter = 0;
            while (cellCounter < TotalCells)
            {
                int x1, y1, x2 = 0, y2 = 0, dx = 0, dy = 0, d = 0;
                bool ok = false;
                do
                {
                    x1 = _pos.X;
                    y1 = _pos.Y;
                    d = _random.Next(4);
                    for (int i = 0; i < 4; i++)
                    {
                        dx = _direction[d++ & 3];
                        dy = _direction[d & 3];
                        x2 = x1 + dx;
                        y2 = y1 + dy;
                        if (x2 >= 0 && x2 < CellSideCount && y2 >= 0 && y2 < CellSideCount && _cells[x2, y2] == 0)
                        {
                            ok = true;
                            break;
                        }
                    }
                    if (!ok)
                    {
                        //Debug.Print("moves {0}", _moves.Count);
                        if (_moves.Count > 0)
                        {
                            _pos = _moves.Pop();
                            //Debug.Print("Pop {0} {1}", _pos.X, _pos.Y);
                        }
                        else
                        {
                            break;
                        }
                    }
                } while (!ok);

                if (ok)
                {
                    _pos.X = x2;
                    _pos.Y = y2;
                    _moves.Push(_pos);
                    d = (--d) & 3;
                    //int cm = _directionS[d] | _directionE[d];
                    _cells[x1, y1] |= _directionS[d];
                    _cells[x2, y2] |= _directionE[d];
                    //var pt1 = new Point(x1*CellSize + CellSizeHalf, y1*CellSize + CellSizeHalf);
                    //var pt2 = new Point(x2*CellSize + CellSizeHalf, y2*CellSize + CellSizeHalf);
                    //_pointers.Add(new Line(pt1, pt2));
                    cellCounter++;
                    //Debug.Print("Push {0} {1} --> {2}", _pos.X, _pos.Y, cellCounter);
                }
                else
                {
                    Debug.WriteLine("done!");
                    //timer1.Enabled = false;
                    break;
                }
            }

            //for (int y = 0; y < CellSideCount; y++)
            //{
            //    for (int x = 0; x < CellSideCount; x++)
            //    {
            //        var tileIdx = _cells[x, y];
            //        Debug.Write(string.Format("{0,2} ", tileIdx));
            //    }
            //    Debug.WriteLine("");
            //}
            //Debug.WriteLine("----------------------------------");
            var result = new int[Dimension, Dimension];
            for (int y = 0; y < CellSideCount; y++)
            {
                int oy = y*CellSize;
                for (int x = 0; x < CellSideCount; x++)
                {
                    int ox = x * CellSize;
                    var tileIdx = _cells[x,y];
                    for (int ix = 0; ix < CellSize; ix++)
                    {
                        for (int iy = 0; iy < CellSize; iy++)
                        {
                            result[ox + ix, oy + iy] = _tiles[tileIdx, iy, ix];
                        }
                    }
                }
            }
            return result;
        }
    }
}