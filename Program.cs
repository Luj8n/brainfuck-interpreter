using System;
using System.IO;

namespace brainfuck
{
  class Program
  {
    static void Main(string[] args)
    {
      try
      {
        string filePath = args[0];
        Console.WriteLine("Starting...");

        Interpeter interpeter = new Interpeter(true, false, 32, false);

        string inputCode = ReadCode(filePath);

        interpeter.OptimizedRun(inputCode);
        Console.WriteLine("\nDone!");
      }
      catch (IndexOutOfRangeException e)
      {
        Console.WriteLine("Error: No path to .bf file. Usage: dotnet run pathToFile.bf");
      }
      catch (FileNotFoundException e)
      {
        Console.WriteLine("Error: could not find the file");
      }
    }

    static string ReadCode(string path)
    {
      return File.ReadAllText(path);
    }
  }
}
