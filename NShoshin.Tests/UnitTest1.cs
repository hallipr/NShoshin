using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NShoshin.Console;

namespace NShoshin.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void Solver_should_solve_Puzzles()
		{
			var solver = new Solver();
			var puzzle = GetHardTestPuzzle();

			solver.Solve(puzzle);

			Assert.IsTrue(puzzle.IsSolved);
		}

		[TestMethod]
		public void IsSolved_on_Puzzle_should_return_false_if_there_are_empty_cells()
		{
			var puzzle = new Puzzle();

			Assert.IsFalse(puzzle.IsSolved);
		}
		
		[TestMethod]
		public void IsSolved_on_Puzzle_should_return_false_if_there_are_errors()
		{
			var puzzle = new Puzzle();

			puzzle.Rows[0][2].PossibleAnswers.RemoveAll(n => n != Number.One);
			puzzle.Rows[0][5].PossibleAnswers.RemoveAll(n => n != Number.One);

			Assert.IsFalse(puzzle.IsSolved);
		}

		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_row()
		{
			var puzzle = new Puzzle();

			puzzle.Rows[0][3].PossibleAnswers.RemoveAll(n => n != Number.One);
			puzzle.Rows[0][5].PossibleAnswers.RemoveAll(n => n != Number.One);
		
			Assert.IsTrue(puzzle.HasErrors);
		}
		
		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_column()
		{
			var puzzle = new Puzzle();

			puzzle.Columns[0][3].PossibleAnswers.RemoveAll(n => n != Number.One);
			puzzle.Columns[0][7].PossibleAnswers.RemoveAll(n => n != Number.One);

			Assert.IsTrue(puzzle.HasErrors);
		}

		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_group()
		{
			var puzzle = new Puzzle();

			puzzle.Groups[0][1].PossibleAnswers.RemoveAll(n => n != Number.One);
			puzzle.Groups[0][5].PossibleAnswers.RemoveAll(n => n != Number.One);

			Assert.IsTrue(puzzle.HasErrors);
		}

		public Puzzle GetEasyTestPuzzle()
		{
			const string PuzzleString = @"
				18......9
				.3....4.5
				579.1.3.. 
				..38...7.
				498.2.153
				.1...46..
				..6.4.829
				8.7....3.
				.2.....46";

			return ParsePuzzleString(PuzzleString);
		}

		public Puzzle GetHardTestPuzzle()
		{
			const string PuzzleString = @"
				6...7.3..
				.....3.87
				9.....2.5
				.....49.1
				.9.6.1.2.
				7.53.....
				1.7.....9
				23.1.....
				..4.3...2";

			return ParsePuzzleString(PuzzleString);
		}

		private Puzzle ParsePuzzleString(string puzzleString)
		{
			var puzzle = new Puzzle();

			puzzleString = puzzleString.Trim().Replace("\r", string.Empty).Replace(" ", string.Empty);

			var rowIndex = 0;

			foreach (var row in puzzleString.Split('\n').Select(r => r.Trim()))
			{
				var columnIndex = 0;

				foreach (var column in row.Select(c => GetNumber(char.ToString(c))))
				{
					if (column.HasValue)
					{
						puzzle.Rows[rowIndex][columnIndex].PossibleAnswers.RemoveAll(n => n != column.Value);
					}

					columnIndex++;
				}

				rowIndex++;
			}

			return puzzle;
		}

		private Number? GetNumber(string number)
		{
			switch (number)
			{
				case "1": return Number.One;
				case "2": return Number.Two;
				case "3": return Number.Three;
				case "4": return Number.Four;
				case "5": return Number.Five;
				case "6": return Number.Six;
				case "7": return Number.Seven;
				case "8": return Number.Eight;
				case "9": return Number.Nine;
				default: return null;
			}
		}
	}
}
