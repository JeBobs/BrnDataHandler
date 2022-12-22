namespace BrnDataHandler;

internal class Brn
{
    public static MainInstance C_MainInstance;
    public static DataHandler  C_DataHandler;

    public static readonly string debugPrefix = $"DEBUG:";

    static void Main(string[] args)
    {
        C_DataHandler  = new();
        C_MainInstance = new();

        C_MainInstance.Initialize(args);
    }
}