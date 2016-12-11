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
    public class kpb23Player : Player // class must be public
    {
        // Add stopwatch
        Stopwatch stopwatch;
        TimeSpan maxTime;
        int moveCount = 0;
        public kpb23Player(Position pos, int maxTimePerMove) // constructor must match this signature
            : base(pos, "kpb23Player", maxTimePerMove) // choose a string other than "MyPlayer"
        {
            maxTime = TimeSpan.FromMilliseconds(maxTimePerMove - 50); // Have a safety time of 50 milliseconds
        }

        public override string gloat() { return "gg N00b"; }

        public override String getImage() { return "https://scontent-atl3-1.xx.fbcdn.net/hphotos-xfp1/v/t1.0-9/12144931_10205171932885488_4643594610127344449_n.jpg?oh=eb6b4a166fe55b5020645b081b47b926&oe=56DB3C4E"; }

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

            int moveChoose = 0;

            // Use bonzo as backup if first search doesn't complete in time.
            for (int i = 5; i >= 0; i--)
                if (newBoard.stonesAt(i) == 6 - i) moveChoose = i;
            for (int i = 5; i >= 0; i--)
                if (newBoard.stonesAt(i) > 0) moveChoose = i;

            moveCount++;
            Console.WriteLine(moveCount);

            for (int i = 6; i < 1000; i++) {
                //Console.WriteLine("Generation = " + i);
                try
                {
                    Console.WriteLine("Depth: " + i);
                    moveChoose = getMove(newBoard, i, true);
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerExceptions.Any(g => g is TaskCanceledException))
                        break;
                    throw;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            //Console.WriteLine(stopwatch.Elapsed);
            return convert(moveChoose, b);
        }

        // Hueristics for evaluating the board
        public override int evaluate(Board b)
        {
            int value = 0;
            value += (b.stonesAt(6) - b.stonesAt(13)); // Amount in your base - opponent base
            //value -= b.board.Skip(7).Take(6).Sum(); // - opponent amount on their side
            return value;
        }

        public int getMove(Board board, int depth, bool parallel)
        {
            if (!parallel)
            {
                var boards = (from i in Enumerable.Range(0, depth + 1) select new Board(board)).ToArray();
                int max = int.MinValue;
                int position = 1;
                for (int i = 0; i <= 5; i++)
                {
                    var newB = boards[depth];
                    newB.copy(board);
                    if (!newB.legalMove(i)) // Skip illegal move
                        continue;
                    newB.makeMove(i, false);
                    int searchMax = DFS(boards, int.MinValue, int.MaxValue, depth);
                   // Console.WriteLine("SearchMax[" + i + "]: " + searchMax);
                    if (max < searchMax)
                    {
                        max = searchMax;
                        position = i;
                    }
                }
                //Console.WriteLine("MAX: " + max);
                return position;
            }else{
                return Enumerable.Range(0, 6).AsParallel().OrderBy(i =>
                {
                    var boards = (from j in Enumerable.Range(0, depth + 1) select new Board(board)).ToArray();
                    var newB = boards[depth];
                    newB.copy(board);
                    if (!newB.legalMove(i)) // Skip illegal move
                        return int.MinValue; // This is the worst
                    newB.makeMove(i, false);
                    int searchMax = DFS(boards, int.MinValue, int.MaxValue, depth);
                    //Console.WriteLine("SearchMax[" + i + "]: " + searchMax);
                    return searchMax;
                }).Last();
                
            }
        }

        // Staged DFS
        // Tuple <Position, Value of Move>
        public int DFS(Board[] boards, int alpha, int beta, int depthCountDown) {

            // Timer throw
            if (stopwatch.Elapsed > maxTime)
            {
                throw new TaskCanceledException();
            }
            var b = boards[depthCountDown];
            // If gets to the depth then do the hueristics or game over
            if (depthCountDown  <= 0 || b.gameOver()) {
                return evaluate(b);
            }
            var newB = boards[depthCountDown - 1];
            // Try every possible move
            int start = convert(0, b);
            int minORmax = (b.whoseMove() == Position.Bottom) ? alpha : beta;
            
            for (int i = start; i < start + 6; i++)
            {
                // Don't do illegal moves
                if (b.board[i] != 0)
                {
                    newB.copy(b);
                    newB.makeMove(i, false);
                    int newMinORmax = DFS(boards, alpha, beta, depthCountDown - 1); // Recursive call
                    //    max                                                      ||     min
                    if (minORmax < newMinORmax && b.whoseMove() == Position.Bottom || minORmax > newMinORmax && b.whoseMove() == Position.Top)
                    {
                        minORmax = newMinORmax;

                        if (b.whoseMove() == Position.Top)
                            beta = Math.Min(newMinORmax, beta);
                        else
                            alpha = Math.Max(newMinORmax, alpha);

                        // Alpha beta
                        if (minORmax > beta)
                        {
                            return beta;
                        }
                        if (minORmax < alpha)
                        {
                            return alpha;
                        }
                    }
                }
            }
            // move
            return minORmax;
        }

        // adapt all code from your player class into this

    }
}