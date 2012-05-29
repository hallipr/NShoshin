namespace NShoshin.Console
{
	public class Cell
	{
		public readonly int Row;
		public readonly int Column;
		public readonly int Group;

		public Cell(int row, int column)
		{
			Row = row;
			Column = column;

			Group = column / 3 + row - (row % 3);
		}

		public Number? Answer { get; set; }
	}
}