using System;
using System.Collections.Generic;
using System.Linq;

namespace NShoshin.Console
{
	public static class Validations
	{
		public static readonly Number[] AllNumbers = (Number[])Enum.GetValues(typeof(Number));
		
		public static bool HasDuplicates(IEnumerable<Cell> cells)
		{
			return cells.Where(c => c.PossibleAnswers.Count == 1)
				.GroupBy(c => c.PossibleAnswers[0])
				.Any(g => g.Count() > 1);
		}
	}
}