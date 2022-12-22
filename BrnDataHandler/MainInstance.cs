using BrnDataHandler.Commands;

namespace BrnDataHandler
{
    internal class MainInstance
    {
        // Switches
        public bool convertExtensions = false;
        public static bool keepExtensions = true;
        public static bool sortByType = false;
        public bool debug = false;

        public List<string> Directories = new();
        public List<string> Files = new();

        //delegate void Command();

        public void Initialize(string[] args)
        {
            if (args.Length > 0)
            {
                List<string> tokens = new List<string>();

                Command command = new Commands.Command_Null();

                foreach (var arg in args)
                {
                    Console.WriteLine($"Launched with argument {arg}");

                    if (Directory.Exists(arg))
                        Directories.Add(arg);

                    else if (File.Exists(arg))
                        Files.Add(arg);

                    else
                        tokens.Add(arg.ToLower());

                    // Really hacky system to do quick debugging.
                    // Gotta replace with a proper token parsing system.
                    foreach (string tk in tokens)
                    {
                        if (tk.StartsWith("--"))
                        {
                            switch (tk) // Switches
                            {
                                case "--sort-by-type":
                                    sortByType = true;
                                    break;
                                case "--convert-extensions":
                                    convertExtensions = true;
                                    keepExtensions = false;
                                    break;
                                case "--keep-original-extensions":
                                    keepExtensions = true;
                                    break;
                                case "--debug":
                                    debug = true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (tk) // Commands
                            {
                                case "recover":
                                    command = new Commands.Command_Recover();
                                    break;
                                case "convert-asset-endian":
                                    command = new Command_ConvertAssetEndian();
                                    break;
                                case "help":
                                    command = new Command_Help();
                                    break;
                            }
                        }
                    }

                    if (convertExtensions)
                        keepExtensions = false;
                }

                // Run delegate command
                command.Run();
            }
        }
    }
}
