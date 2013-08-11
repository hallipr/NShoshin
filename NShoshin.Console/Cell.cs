using System;
using System.Collections.Generic;
using System.Linq;

namespace NShoshin.Console
{
	public class Cell
	{
		private readonly Puzzle _puzzle;

		public Cell(Puzzle puzzle, int row, int column)
		{
			_puzzle = puzzle;

			Row = row;
			Column = column;

			PossibleAnswers = new List<Number>(Validations.AllNumbers);

			Group = row - (row % 3) + (column / 3);
		}

		public int Row { get; private set; }

		public int Column { get; private set; }

		public int Group { get; private set; }

		public IEnumerable<Cell> OtherColumnCells
		{
			get
			{
				var cells = _puzzle.Columns[Column];
				return cells.Except(new[] { this });
			}
		} 

		public IEnumerable<Cell> OtherRowCells
		{
			get
			{
				var cells = _puzzle.Rows[Row];
				return cells.Except(new[] { this });
			}
		} 

		public IEnumerable<Cell> OtherGroupCells
		{
			get
			{
				var cells = _puzzle.Groups[Group];
				return cells.Except(new[] { this });
			}
		} 

		public List<Number> PossibleAnswers { get; set; }

		public int PossibleAnswerHash
		{ 
			get
			{
				return PossibleAnswers.Sum(a => (int)Math.Pow(2, (int)a));
			}
		}

		public string Coordinates
		{ 
			get
			{
				return string.Format("({0}, {1})", Column + 1, Row + 1);
			} 
		}

		public void SetAnswer(Number number)
		{
			PossibleAnswers = new List<Number> { number };
		}

		public void RemoveAnwer(Number number)
		{
            PossibleAnswers.RemoveAll(n => n == number);
		}
	}
}