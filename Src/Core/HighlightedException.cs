using System.Drawing;

using Pastel;

namespace FinanceNotifier.Src.Core;

public class HighlightedException : Exception
{
    public HighlightedException(string message)
        : base(message.Pastel(Color.Red)) { }
}