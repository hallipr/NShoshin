using System.Linq;

namespace NShoshin.Console
{
	public class Puzzle
	{
		private readonly Cell[] _cells;

		public Puzzle()
		{
			_cells = Enumerable.Range(0, 9)
				.SelectMany(r => Enumerable.Range(0, 9)
				                 	.Select(c => new Cell(r, c))
				)
				.ToArray();

			Rows = _cells.GroupBy(c => c.Row, (row, cells) => cells.ToArray())
				.ToArray();

			Columns = _cells.GroupBy(c => c.Column, (column, cells) => cells.ToArray())
				.ToArray();

			Groups = _cells.GroupBy(c => c.Group, (group, cells) => cells.ToArray())
				.ToArray();
		}

		public Cell[][] Rows { get; private set; }
		public Cell[][] Columns { get; private set; }
		public Cell[][] Groups { get; private set; }

		public bool IsSolved
		{ 
			get
			{
				return !HasErrors && _cells.All(c => c.Answer.HasValue);
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