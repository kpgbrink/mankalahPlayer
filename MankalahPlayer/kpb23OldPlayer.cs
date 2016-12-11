using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

/* Mankalah board representation
 * 
 *          12  11  10  9  8  7
 *  
 *        13                      6
 * 
 *           0   1  2   3  4   5
 */

namespace Mankalah
{
    // rename me
    public class kpb23OldPlayer: Player // class must be public
    {
        // Add stopwatch
        Stopwatch stopwatch;
        TimeSpan maxTime;
        int move = 0;
        public kpb23OldPlayer(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "kpb23OldPlayer", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            maxTime = TimeSpan.FromMilliseconds(maxTimePerMove - 50); // Have a safety time of 50 milliseconds
        }

        public override string gloat() { return "gg N00b"; }

        public override String getImage() { return "Kristofer Brink";}

        public int convert(int i, Board b) {
            if (b.whoseMove() == Position.Top) {
                return i  + 7;
            }
            return i;
        }

        public Board flipBoard(Board b) {
            // Turn the board into the same thing top or bottom. I will make the top seem like the bottom.
            Board newBoard = new Board(Position.Bottom);
            if (b.whoseMove() == Position.Top)
            {
                Array.Copy(b.board, 0, newBoard.board, 7, 7);
                Array.Copy(b.board, 7, newBoard.board, 0, 7);
            }
            else // b.whoseMove() == Position.Bottom
            {
                Array.Copy(b.board, 0, newBoard.board, 0, newBoard.board.Count());
            }
            return newBoard;
        }

        

        public override int chooseMove(Board b)
        {
            // Start stop watch
            stopwatch = Stopwatch.StartNew();
            Board newBoard = flipBoard(b);
            int moveChoose = 1;
            for (int i = 6; i < 1000; i++) {
                Console.WriteLine("Generation = " + i);
                try
                {
                    moveChoose = getMove(newBoard, i);
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerExceptions.Any(g => g is TaskCanceledException))
                    break;
                }
            }

            Console.WriteLine(stopwatch.Elapsed);
            return convert(moveChoose, b);
        }

        // Hueristics for evaluating the board
        public override int evaluate(Board b)
        {
            int value = 0;
            value += (b.stonesAt(6) - b.stonesAt(13)) * 1;
            //value += b.board.Take(6).Sum() - b.board.Skip(7).Take(6).Sum();
            return value;

        }

        public int getMove(Board board, int depth)
        {
            /*
            int max = int.MinValue;
            int position = 1;
            for (int i = 0; i <= 5; i++)
            {
                Board newB = new Board(board);
                if (!newB.legalMove(i)) // Skip illegal move
                    continue;
                newB.makeMove(i, false);
                int searchMax = DFS(newB, int.MinValue, int.MaxValue, depth);
                Console.WriteLine("SearchMax[" + i + "]: " + searchMax);
                if (max < searchMax)
                {
                    max = searchMax;
                    position = i;
                }
            }
            Console.WriteLine("MAX: " + max);
            return position;*/
            return Enumerable.Range(0, 6).AsParallel().OrderBy(i =>
            {
                Board newB = new Board(board);
                if (!newB.legalMove(i)) // Skip illegal move
                    return int.MinValue; // This is the worst
                newB.makeMove(i, false);
                int searchMax = DFS(newB, int.MinValue, int.MaxValue, depth);
                //Console.WriteLine("SearchMax[" + i + "]: " + searchMax);
                return searchMax;
            }).Last();
        }


        // Staged DFS
        // Tuple <Position, Value of Move>
        public int DFS(Board b, int alpha, int beta, int depthCountDown) {
            // Timer throw
            if (stopwatch.Elapsed > maxTime)
            {
                throw new TaskCanceledException();
            }


            // If gets to the depth then do the hueristics
            if (depthCountDown  <= 0 || b.gameOver()) {
                return evaluate(b);
            }

            // Try every possible move
            int start = convert(0, b);
            Board newB = new Board(b);
            int minORmax = (b.whoseMove() == Position.Bottom) ? alpha : beta;
            bool gameGoing = false;
            for (int i = start; i < start + 6; i++)
            {
                // Don't do illegal moves
                if (b.legalMove(i)) {
                    gameGoing = true;
                    newB.copy(b);
                    newB.makeMove(i, false);
                    minORmax = DFS(newB, alpha, beta, depthCountDown - 1);

                    
                    int newMinORmax = DFS(newB, alpha, beta, depthCountDown - 1);
                    //    max                                                      ||     min
                    if (minORmax < newMinORmax && b.whoseMove() == Position.Bottom || minORmax > newMinORmax && b.whoseMove() == Position.Top)
                    {
                        minORmax = newMinORmax;
                        if (b.whoseMove() == Position.Top)
                            beta = Math.Min(newMinORmax, beta);
                        else
                            alpha = Math.Max(newMinORmax, alpha);

                        // Alpha beta                                    max            ||              min
                        //if (minORmax > beta)
                            //return beta;
                        //if (minORmax < alpha)
                            //return alpha;
                        
                    }
                    


                }
            }

            // Throw exceptionn if too much
            return minORmax;
        }

        // adapt all code from your player class into this

    }
}