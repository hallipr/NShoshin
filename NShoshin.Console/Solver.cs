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
				System.Console.WriteLine("--  Starting Reductions --");
				
				ApplyReductions(puzzle);

				var newCount = puzzle.Cells.Count(c => c.PossibleAnswers.Count > 1);

				success = count != newCount;

				count = newCount;
			}
			while (success && count > 0 && !puzzle.HasErrors);
		}

		private void ApplyReductions(Puzzle puzzle)
		{
			RemoveAnsweredFromOtherPossibles(puzzle);

			var cellSets = puzzle.Rows.Select(r => new { Name = "Row " + (r[0].Row + 1), Cells = r })
				.Union(puzzle.Columns.Select(r => new { Name = "Column " + (r[0].Column + 1), Cells = r }))
				.Union(puzzle.Groups.Select(r => new { Name = "Group " + (r[0].Group + 1), Cells = r }));

			foreach (var cellSet in cellSets)
			{
				SetOnlyPossibleCell(cellSet.Cells, cellSet.Name);
				ReduceUsingMatchingCells(cellSet.Cells, cellSet.Name);
			}

			ReduceUsingPerGroupColumnsOrRows(puzzle);
			// ReduceUsingsGroupColumns(puzzle);
			// ReduceUsingsGroupRows(puzzle);
		}

		private void RemoveAnsweredFromOtherPossibles(Puzzle puzzle)
		{
			bool success;
			var count = puzzle.Cells.Count(c => c.PossibleAnswers.Count > 1);

			do
			{
				var answeredCells = puzzle.Cells.Where(c => c.PossibleAnswers.Count == 1).ToArray();
				foreach (var cell in answeredCells)
				{
					var answer = cell.PossibleAnswers[0];

					var otherCells = cell.OtherColumnCells.Union(cell.OtherGroupCells).Union(cell.OtherRowCells)
						.Where(c => c.PossibleAnswers.Contains(answer))
						.ToArray();

					if (!otherCells.Any())
					{
						continue;
					}
					
					foreach (var otherCell in otherCells)
					{
						WriteLine(string.Format("{0} looses {1} by answer on {2}", otherCell.Coordinates, (int)answer, cell.Coordinates));
						otherCell.RemoveAnwer(answer);
						OnReduced(new EventArgs());
					}
				}

				var newCount = puzzle.Cells.Count(c => c.PossibleAnswers.Count > 1);

				success = count != newCount;

				count = newCount;

			}
			while (success && count > 0 && !puzzle.HasErrors);
		}

		private void SetOnlyPossibleCell(Cell[] cellSet, string name)
		{
			foreach (var number in Validations.AllNumbers)
			{
				var thisNumber = number;

				var possibleCells = cellSet.Where(c => c.PossibleAnswers.Contains(thisNumber)).ToArray();

				if (possibleCells.Length == 1 && possibleCells[0].PossibleAnswers.Count > 1)
				{
					var cell = possibleCells[0];
					WriteLine(string.Format("{0} set to {1} for only possible cell in {2}", cell.Coordinates, (int)thisNumber, name));
					cell.SetAnswer(thisNumber);
				}
			}
		}

		// In a colunm, row, or group, if a set of numbers it wholly consumed by a group of matching cells, i.e. the number of cells 
		// matches the number of possible answers per cell, then those answers cannot be used by a less restrictive cell in any of
		// the matching cells' cellsets
		private void ReduceUsingMatchingCells(Cell[] cellSet, string name)
		{
			var cellGroups = cellSet
				.Where(c => c.PossibleAnswers.Count > 1)
				.GroupBy(c => c.PossibleAnswerHash)
				.Where(g => g.First().PossibleAnswers.Count == g.Count());

			foreach (var cellGroup in cellGroups)
			{
				var answers = cellGroup.First().PossibleAnswers;
				var groupCells = string.Join(", ", cellGroup.Select(c => string.Format("({0}, {1})", c.Column + 1, c.Row + 1)));
				var answerStrings = string.Join(", ", answers.Select(a => ((int)a).ToString(CultureInfo.InvariantCulture)));
				
				var cellsToReduce = cellSet
					.Except(cellGroup)
					.Select(c => new { c.Coordinates, c.PossibleAnswers, AnswersToRemove = c.PossibleAnswers.Where(answers.Contains).ToArray() })
					.Where(c => c.AnswersToRemove.Length > 0);

				foreach (var reduceCell in cellsToReduce)
				{
					var toRemoveString = string.Join(", ", reduceCell.AnswersToRemove.Select(a => ((int)a).ToString(CultureInfo.InvariantCulture)));
						
					WriteLine(string.Format("{0} looses [{1}] by maching group {2} [{3}] in {4}", reduceCell.Coordinates, toRemoveString, groupCells, answerStrings, name));
						
					reduceCell.PossibleAnswers.RemoveAll(reduceCell.AnswersToRemove.Contains);

					OnReduced(null);
				}
			}
		}

		// If a number is only available in a singe row or column within a group, it must be used in that group.
		private void ReduceUsingPerGroupColumnsOrRows(Puzzle puzzle)
		{
			foreach (var cellGroup in puzzle.Groups)
			{
				foreach (var number in Validations.AllNumbers)
				{
					var thisNumber = number;
					var matchingCells = cellGroup.Where(c => c.PossibleAnswers.Contains(thisNumber)).ToArray();
					var matchingCellsString = string.Join(", ", matchingCells.Select(c => c.Coordinates));
					
					if (matchingCells.Length == 1)
					{
						continue;
					}

					var rowsUsed = matchingCells.Select(c => c.Row).ToArray();
					if (rowsUsed.Length == 1)
					{
						foreach (var cell in puzzle.Rows[rowsUsed[0]].Except(matchingCells))
						{
							WriteLine(string.Format("{0} looses {1} by required cells {2} in group {3}", cell.Coordinates, (int)thisNumber, matchingCellsString, cellGroup[0].Group + 1));
							cell.RemoveAnwer(number);
						}
					}

					var colsUsed = matchingCells.Select(c => c.Column).ToArray();
					if (colsUsed.Length == 1)
					{
						foreach (var cell in puzzle.Columns[colsUsed[0]].Except(matchingCells))
						{
							WriteLine(string.Format("{0} looses {1} by required cells {2} in group {3}", cell.Coordinates, (int)thisNumber, matchingCellsString, cellGroup[0].Group + 1));
							cell.RemoveAnwer(number);
						}
					}
				}
			}
		}

		// If 2 groups have cells that would cross reduce, then those answers cannot be used by any other cells.
		private void ReduceUsingsGroupRows(Puzzle puzzle)
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

		private void ReduceUsingsGroupColumns(Puzzle puzzle)
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

		private void WriteLine(string value)
		{
			System.Console.WriteLine(value);
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