using System.Drawing;

using Pastel;

namespace FinanceNotifier.Core;

public class HighlightedException : Exception
{
    public HighlightedException(string message)
        : base(message.Pastel(Color.Red)) { }
}