using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using WatiN.Core;

namespace NShoshin.Console
{
	public class WatinRunner
	{
		private IE _browser;
		private Puzzle _puzzle;

		private Dictionary<Tuple<int, int>, Element> _inputs;
		private Dictionary<Tuple<int, int>, Number?> _answers;

		public void Solve(string url)
		{
			_browser = new IE(url);
			_puzzle = ParsePuzzle();

            var solver = new GuessingSolver();

			// solver.Reduced += SolverOnReduced;

			solver.Solve(_puzzle);

			SolverOnReduced(null, null);

			if (_puzzle.IsSolved)
			{
				return;
			}

			if(_puzzle.HasErrors)
			{
				System.Console.WriteLine("Puzzle has Errors");
			}
		    var tempPath = Path.GetTempPath();

		    string tempFileName;
		    do
		    {
		        tempFileName = Path.Combine(tempPath, Guid.NewGuid().ToString().Remove(8) + ".html");
		    } while (File.Exists(tempFileName));

            File.WriteAllText(tempFileName, GetPuzzleHtml());
		    Process.Start(tempFileName);
		}

		public string GetPuzzleHtml()
		{
			var outString = new StringBuilder();

			outString.AppendLine("<html><body><table style=\"border-collapse: collapse; border-spacing: 0;\">");


			var rowNum = 0;
			foreach (var row in _puzzle.Rows)
			{
				outString.AppendLine("<tr>");
					
				var colNum = 0;
				foreach (var cell in row)
				{
					var style = "width: 3em; height: 3em; border: 1px solid black; margin: 0; padding: 3px; ";

					if(rowNum == 2 || rowNum == 5)
					{
						style += "border-bottom: 2px solid black;";
					}

					if(colNum == 2 || colNum == 5)
					{
						style += "border-right: 2px solid black;";
					}
					
					outString.Append(string.Format("<td style=\"{0}\">", style));

					foreach (var number in cell.PossibleAnswers)
					{
						outString.AppendLine(Convert.ToString((int)number));
					}

					outString.AppendLine("</td>");
					colNum++;
				}

				outString.AppendLine("</tr>");
				rowNum++;
			}

			outString.AppendLine("</table></body></html>");

			return outString.ToString();
		}

		private void SolverOnReduced(object sender, EventArgs eventArgs)
		{
			var answered = _puzzle.Cells.Where(c => c.PossibleAnswers.Count == 1);
			
			foreach (var cell in answered)
			{
				var answer = cell.PossibleAnswers[0];

				var id = Tuple.Create(cell.Column, cell.Row);

				if (_answers[id] == null)
				{
					_answers[id] = answer;
					_inputs[id].SetAttributeValue("Value", Convert.ToString((int)answer));
					_inputs[id].Blur();
				}
			}
		}

		private Puzzle ParsePuzzle()
		{
			_inputs = new Dictionary<Tuple<int, int>, Element>();
			_answers = new Dictionary<Tuple<int, int>, Number?>();

			var frame = _browser.Frames[0];
			var table = frame.Table(Find.ByClass("t"));
			var puzzle = new Puzzle();

			for (var row = 0; row < 9; row++)
			{
				for (var column = 0; column < 9; column++)
				{
					var id = string.Format("f{0}{1}", column, row);
					var input = table.Element(Find.ById(id));
					var number = GetNumber(input.GetAttributeValue("value"));

					var key = Tuple.Create(column, row);
					_inputs[key] = input;
					_answers[key] = number;
					
					if (number.HasValue)
					{
						puzzle.Rows[row][column].SetAnswer(number.Value);
					}
				}
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