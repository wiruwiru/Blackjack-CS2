using Blackjack.Config;

namespace Blackjack.Utils;

public static class Debug
{
    public static BaseConfigs? Config { get; set; }

    public static void DebugMessage(string message)
    {
        if (Config?.EnableDebug != true) return;
        Console.WriteLine($"================================= [ Blackjack ] =================================");
        Console.WriteLine(message);
        Console.WriteLine("===================================================================================");
    }

    public static void DebugError(string error)
    {
        if (Config?.EnableDebug != true) return;
        Console.WriteLine($"================================= [ Blackjack - ERROR ] =================================");
        Console.WriteLine($"ERROR: {error}");
        Console.WriteLine("==========================================================================================");
    }

    public static void DebugWarning(string warning)
    {
        if (Config?.EnableDebug != true) return;
        Console.WriteLine($"================================= [ Blackjack - WARNING ] =================================");
        Console.WriteLine($"WARNING: {warning}");
        Console.WriteLine("============================================================================================");
    }
}