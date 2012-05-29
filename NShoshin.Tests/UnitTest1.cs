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
			var puzzle = new Puzzle();

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

			puzzle.Rows[0][2].Answer = Number.One;
			puzzle.Rows[0][5].Answer = Number.One;

			Assert.IsFalse(puzzle.IsSolved);
		}

		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_row()
		{
			var puzzle = new Puzzle();

			puzzle.Rows[0][3].Answer = Number.One;
			puzzle.Rows[0][5].Answer = Number.One;
		
			Assert.IsTrue(puzzle.HasErrors);
		}
		
		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_column()
		{
			var puzzle = new Puzzle();

			puzzle.Rows[2][0].Answer = Number.One;
			puzzle.Rows[4][0].Answer = Number.One;

			Assert.IsTrue(puzzle.HasErrors);
		}

		[TestMethod]
		public void HasErrors_on_Puzzle_should_return_true_if_a_number_is_duplicated_in_a_group()
		{
			var puzzle = new Puzzle();

			puzzle.Rows[0][2].Answer = Number.One;
			puzzle.Rows[2][1].Answer = Number.One;

			Assert.IsTrue(puzzle.HasErrors);
		}
	}
}
