using System;

namespace NShoshin.Console
{
    public interface ISolver
    {
        event EventHandler Reduced;
        void Solve(Puzzle puzzle);
    }
}