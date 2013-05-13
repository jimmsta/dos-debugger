using System.Windows.Input;

namespace WpfDebugger
{
    static class DebuggerCommands
    {
        public static readonly RoutedCommand OpenDisassembly = new RoutedCommand();
        public static readonly RoutedCommand OpenNewDisassembly = new RoutedCommand();
        public static readonly RoutedCommand OpenHexView = new RoutedCommand();
        public static readonly RoutedCommand OpenNewHexView = new RoutedCommand();
    }
}
