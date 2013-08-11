using System.Linq;

namespace NShoshin.Console
{
    public class GuessingSolver : Solver
    {
        public override void Solve(Puzzle puzzle)
        {
            base.Solve(puzzle);

            if (puzzle.HasErrors || puzzle.IsSolved) return;

            WriteLine("Not solved without guessing");

            // Need to start guessing
            var cellToGuess= puzzle.Cells.OrderBy(c => c.PossibleAnswers.Count)
                                   .First(c => c.PossibleAnswers.Count > 1);

            WriteLine(string.Format("Guessing on {0} with [{1}]", cellToGuess.Coordinates, string.Join(", ", cellToGuess.PossibleAnswers)));

            var guessPuzzles = cellToGuess.PossibleAnswers
                                          .Select(p => CloneAndGuess(puzzle, cellToGuess, p));

            foreach (var guessPuzzle in guessPuzzles.AsParallel())
            {
                Solve(guessPuzzle);

                if (guessPuzzle.IsSolved && !guessPuzzle.HasErrors)
                {
                    puzzle.CopyCellsFrom(guessPuzzle);
                }
            }
        }

        private Puzzle CloneAndGuess(Puzzle puzzle, Cell cellToGuess, Number guess)
        {
            var clone = new Puzzle(puzzle);
            
            clone.Columns[cellToGuess.Column][cellToGuess.Row].SetAnswer(guess);

            RemoveAnsweredFromOtherPossibles(clone);
            
            return clone;
        }
    }
}