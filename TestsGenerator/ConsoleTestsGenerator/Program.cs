using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGenerator;

namespace ConsoleTestsGenerator
{
    class Program
    {
        private static int MinArgsAmount = 5;
        private static string InvalidArgumentsAmmount = "Ammount of arguments should be at least " + MinArgsAmount.ToString()
            + ". There are: lists of paths to files for tests generating, path to folder with generated files," 
            + " restrictions on the conveyor (number of files to read, number of files to write, number of tasks at pool thread).";

        static void Main(string[] args)
        {
            if (args.Length < MinArgsAmount)
            {
                throw new ArgumentException(InvalidArgumentsAmmount);
            }

            try
            {
                // input files
                List<string> paths = new List<string>();
                for (int i = 0; i < args.Length - MinArgsAmount + 1; i++)
                {
                    if (File.Exists(args[i]))
                        paths.Add(args[i]);
                    else
                        Console.WriteLine("File not found: " + args[i]);
                }
                if (paths.Count == 0)
                {
                    Console.WriteLine("Ammount of input files should not be 0.");
                }
                else {

                    //output directory
                    string outputDirectory = args[args.Length - MinArgsAmount + 1];
                    if (!Directory.Exists(outputDirectory))
                    {
                        Console.WriteLine("Directory not found: " + outputDirectory);
                    }
                    else
                    {

                        //restrictions on the conveyor
                        int numberOfFilesToRead = Convert.ToInt32(args[args.Length - 3]);
                        int numberOfFilesToWrite = Convert.ToInt32(args[args.Length - 2]);
                        int maxTasksAmmount = Convert.ToInt32(args[args.Length - 1]);

                        Generator generator = new Generator(outputDirectory, numberOfFilesToRead, numberOfFilesToWrite, maxTasksAmmount);
                        generator.Generate(paths).Wait();

                        Console.WriteLine("Files generated.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
