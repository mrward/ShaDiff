using System;

namespace ShaDiff
{
    internal class Program
    {
        static string sourceDirectory;
        static string targetDirectory;
        static void Main(string[] args)
        {
            if (!ParseArgs(args))
            {
                return;
            }

            var checker = new ShaDiffCheck(sourceDirectory, targetDirectory);
            checker.Check();
        }

        static bool ParseArgs(string[] args)
        {
            if (args.Length < 2)
            {
                ShowHelp();
                return false;
            }

            sourceDirectory = args[0];
            targetDirectory = args[1];

            return true;
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: ShaDiff source-directory target-directory");
        }
    }
}