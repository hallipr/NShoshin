using System.Linq;

namespace NShoshin.Console
{
    public class AdvancedSolver : Solver
    {
        protected override void ApplyReductions(Puzzle puzzle)
        {
            base.ApplyReductions(puzzle);
            ReduceUsingRowsOfGroups(puzzle);
            ReduceUsingsColumnsOfGroups(puzzle);
        }
        // If 2 groups have cells that would cross reduce, then those answers cannot be used by any other cells.
        private static void ReduceUsingRowsOfGroups(Puzzle puzzle)
        {
            var groupRows = puzzle.Groups.GroupBy(g => g.First().Row).ToArray();

            foreach (var groupRow in groupRows)
            {
                var reducingGroups = groupRow.SelectMany(cells => cells)
                                             .GroupBy(c => c.PossibleAnswerHash)
                                             .Select(g => new { Cells = g.ToArray(), Answers = g.First().PossibleAnswers })
                                             .Where(g => g.Cells.Length > 1)  // There is a set
                                             .Where(g => g.Answers.Count == g.Cells.Length) // There is an excliving set
                                             .Where(g => g.Cells.GroupBy(c => c.Group).Count() == 2);  // The set spans two groups

                // The remaining group cannot have any of the set's answers in any row used by the set
                foreach (var reducingGroup in reducingGroups)
                {
                    var activeGroups = reducingGroup.Cells.Select(c => c.Group).Distinct().ToArray();
                    var affectedRows = reducingGroup.Cells.Select(c => c.Row).Distinct().ToArray();

                    var affectedCells = groupRow.SelectMany(g => g)
                                                .Where(c => activeGroups.Contains(c.Group))
                                                .Where(c => affectedRows.Contains(c.Row))
                                                .ToArray();

                    foreach (var affectedCell in affectedCells)
                    {
                        affectedCell.PossibleAnswers.RemoveAll(reducingGroup.Answers.Contains);
                    }
                }
            }
        }

        private static void ReduceUsingsColumnsOfGroups(Puzzle puzzle)
        {
            var groupColumns = puzzle.Groups.GroupBy(g => g.First().Column).ToArray();

            foreach (var groupRow in groupColumns)
            {
                var reducingGroups = groupRow.SelectMany(cells => cells)
                                             .GroupBy(c => c.PossibleAnswerHash)
                                             .Select(g => new { Cells = g.ToArray(), Answers = g.First().PossibleAnswers })
                                             .Where(g => g.Cells.Length > 1)  // There is an set
                                             .Where(g => g.Answers.Count == g.Cells.Length) // There is an excliving set
                                             .Where(g => g.Cells.GroupBy(c => c.Group).Count() == 2);  // The set spans two groups

                // The remaining group cannot have any of the set's answers in any row used by the set
                foreach (var reducingGroup in reducingGroups)
                {
                    var activeGroups = reducingGroup.Cells.Select(c => c.Group).Distinct().ToArray();
                    var affectedColumns = reducingGroup.Cells.Select(c => c.Column).Distinct().ToArray();

                    var affectedCells = groupRow.SelectMany(g => g)
                                                .Where(c => activeGroups.Contains(c.Group))
                                                .Where(c => affectedColumns.Contains(c.Column))
                                                .ToArray();

                    foreach (var affectedCell in affectedCells)
                    {
                        affectedCell.PossibleAnswers.RemoveAll(reducingGroup.Answers.Contains);
                    }
                }
            }
        }
    }
}