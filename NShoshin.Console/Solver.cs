using System;
using System.Globalization;
using System.Linq;

namespace NShoshin.Console
{
	public class Solver
	{
		public event EventHandler Reduced;

		public void Solve(Puzzle puzzle)
		{
			bool success;
			var count = puzzle.Cells.Count(c => c.PossibleAnswers.Count > 1);

			do
			{
				ApplyReductions(puzzle);

				var newCount = puzzle.Cells.Count(c => c.PossibleAnswers.Count > 1);

				success = count != newCount;

				count = newCount;
				
				OnReduced(new EventArgs());
			}
			while (success && count > 0);
		}

		private static void ApplyReductions(Puzzle puzzle)
		{
			var answeredCells = puzzle.Cells.Where(c => c.PossibleAnswers.Count == 1).ToArray();

			foreach (var cell in answeredCells)
			{
				var answer = cell.PossibleAnswers[0];

				var otherCells = cell.OtherColumnCells.Union(cell.OtherGroupCells).Union(cell.OtherRowCells);
				
				foreach (var otherCell in otherCells)
				{
					otherCell.RemoveAnwer(answer);
				}
			}

			var cellSets = puzzle.Rows.Union(puzzle.Columns).Union(puzzle.Groups);

			foreach (var cellSet in cellSets)
			{
				SetOnlyPossibleCell(cellSet);
				ReduceUsingMatchingCells(cellSet);
			}

			//ReduceUsingPerGroupColumnsOrRows(puzzle);
			ReduceUsingsGroupColumns(puzzle);
			//ReduceUsingsGroupRows(puzzle);
		}

		private static void SetOnlyPossibleCell(Cell[] cellSet)
		{
			foreach (var number in Validations.AllNumbers)
			{
				var possibleCells = cellSet.Where(c => c.PossibleAnswers.Contains(number)).ToArray();

				if (possibleCells.Length == 1)
				{
					possibleCells[0].SetAnswer(number);
				}
			}
		}

		// In a colunm, row, or group, if a set of numbers it wholly consumed by a group of matching cells, i.e. the number of cells 
		// matches the number of possible answers per cell, then those answers cannot be used by a less restrictive cell in any of
		// the matching cells' cellsets
		private static void ReduceUsingMatchingCells(Cell[] cellSet)
		{
			var cellGroups = cellSet
				.Where(c => c.PossibleAnswers.Count > 1)
				.GroupBy(c => c.PossibleAnswerHash)
				.Where(g => g.First().PossibleAnswers.Count == g.Count());

			foreach (var cellGroup in cellGroups)
			{
				var answers = cellGroup.First().PossibleAnswers;
				var groupCells = string.Join(", ", cellGroup.Select(c => string.Format("({0}, {1})", c.Column, c.Row)));

				foreach (var cell in cellGroup)
				{
					var cellsToReduce = cell.OtherColumnCells
						.Union(cell.OtherRowCells)
						.Union(cell.OtherGroupCells)
						.Except(cellGroup)
						.Select(c => new { c.Row, c.Column, c.PossibleAnswers, AnswersToRemove = c.PossibleAnswers.Where(answers.Contains).ToArray() })
						.Where(c => c.AnswersToRemove.Length > 0);

					foreach (var reduceCell in cellsToReduce)
					{
						var toRemoveString = string.Join(", ", reduceCell.AnswersToRemove.Select(a => ((int)a + 1).ToString(CultureInfo.InvariantCulture)));
						System.Console.WriteLine(string.Format("Captive group {3} removes [{0}] from cell ({1}, {2})", toRemoveString, reduceCell.Column, reduceCell.Row, groupCells));
						cell.PossibleAnswers.RemoveAll(reduceCell.AnswersToRemove.Contains);
					}
				}
			}
		}

		// If 2 groups have cells that would cross reduce, then those answers cannot be used by any other cells.
		private static void ReduceUsingsGroupRows(Puzzle puzzle)
		{
			var groupRows = puzzle.Groups.GroupBy(g => g.First().Row).ToArray();

			foreach (var groupRow in groupRows)
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

		private static void ReduceUsingsGroupColumns(Puzzle puzzle)
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


		// If a number is only available in a singe row or column within a group, it must be used in that group.
		private static void ReduceUsingPerGroupColumnsOrRows(Puzzle puzzle)
		{
			foreach (var cellGroup in puzzle.Groups)
			{
				foreach (var number in Validations.AllNumbers)
				{
					var thisNumber = number;
					var matchingCells = cellGroup.Where(c => c.PossibleAnswers.Contains(thisNumber)).ToArray();
					if (matchingCells.Length == 1)
					{
						continue;
					}

					var rowsUsed = matchingCells.Select(c => c.Row).ToArray();
					var colsUsed = matchingCells.Select(c => c.Column).ToArray();

					if (rowsUsed.Length == 1)
					{
						foreach (var cell in puzzle.Rows[rowsUsed[0]].Except(matchingCells))
						{
							cell.RemoveAnwer(number);
						}
					}

					if (colsUsed.Length == 1)
					{
						foreach (var cell in puzzle.Columns[colsUsed[0]].Except(matchingCells))
						{
							cell.RemoveAnwer(number);
						}
					}
				}
			}
		}
	
		private void OnReduced(EventArgs e)
		{
			var handler = Reduced;
			if (handler != null)
			{
				handler(this, e);
			}
		}
	}
}