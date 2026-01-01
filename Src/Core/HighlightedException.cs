using System.Drawing;

using Pastel;

namespace FinanceScraper.Core;

public class HighlightedException : Exception
{
    public HighlightedException(string message)
        : base(message.Pastel(Color.Red)) { }
}