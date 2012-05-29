using System.Collections.Generic;
using System.Linq;

namespace NShoshin.Console
{
	public static class Validations
	{
		public static bool HasDuplicates(IEnumerable<Cell> cells)
		{

			return cells.Where(c => c.Answer.HasValue)
				.GroupBy(c => c.Answer)
				.Any(g => g.Count() > 1);
		}
	}
}