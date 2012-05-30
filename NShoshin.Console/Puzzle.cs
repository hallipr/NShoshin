using System.Linq;

namespace NShoshin.Console
{
	public class Puzzle
	{
		public readonly Cell[] Cells;

		public Puzzle()
		{
			Cells = Enumerable.Range(0, 9)
				.SelectMany(r => Enumerable.Range(0, 9)
				    .Select(c => new Cell(this, r, c)))
				.ToArray();

			Rows = Cells.GroupBy(c => c.Row, (row, cells) => cells.ToArray())
				.ToArray();

			Columns = Cells.GroupBy(c => c.Column, (column, cells) => cells.ToArray())
				.ToArray();

			Groups = Cells.GroupBy(c => c.Group, (group, cells) => cells.ToArray())
				.ToArray();
		}

		public Cell[][] Rows { get; private set; }
		public Cell[][] Columns { get; private set; }
		public Cell[][] Groups { get; private set; }

		public bool IsSolved
		{ 
			get
			{
				return !HasErrors && Cells.All(c => c.PossibleAnswers.Count == 1);
			} 
		}

		public bool HasErrors
		{ 
			get
			{
				return Rows.Any(Validations.HasDuplicates) || 
				       Columns.Any(Validations.HasDuplicates) || 
				       Groups.Any(Validations.HasDuplicates);
			}
		}
	}
}