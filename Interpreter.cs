using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace brainfuck
{
  class Interpeter
  {
    private bool _negativeCells;
    private bool _negativeCellValues;
    private int _bits;
    private bool _statistics;

    private int _maxCellValue;
    private char[] _legalChars = new char[] { '>', '<', '+', '-', '.', ',', '[', ']' };

    public Interpeter(bool negativeCells, bool negativeCellValues, int bits, bool statistics)
    {
      if (bits != 8 && bits != 16 && bits != 32)
      {
        throw new Exception("Bits can only be 8, 16 or 32");
      }
      _negativeCells = negativeCells;
      _negativeCellValues = negativeCellValues;
      _bits = bits;
      _statistics = statistics;
      _maxCellValue = (int)Math.Pow(2, _bits) - 1;
    }
    public string Clean(string inputCode)
    {
      string outputCode = "";

      for (int i = 0; i < inputCode.Length; i++)
      {
        if (_legalChars.Contains(inputCode[i]))
        {
          outputCode += inputCode[i];
        }
      }

      return outputCode;
    }
    public void Run(string code)
    {
      long steps = 0;
      long totalElapsedTime = 0;
      Dictionary<int, int> cells = new Dictionary<int, int>();
      cells.Add(0, 0);
      int pointer = 0;
      int i = 0;

      bool isDone = false;

      Stopwatch stopwatch = new Stopwatch();
      if (_statistics)
      {
        stopwatch.Start();
      }

      while (!isDone)
      {
        steps++;
        if (i == code.Length)
        {
          isDone = true;
          break;
        }
        char symbol = code[i];

        switch (symbol)
        {
          case '>':
            {
              pointer++;
              cells.TryAdd(pointer, 0);
              i++;
              break;
            }
          case '<':
            {
              if (pointer == 0 && !_negativeCells)
              {
                throw new Exception("Cells can't be negative (no cells to the left of the first one)");
              }
              pointer--;
              cells.TryAdd(pointer, 0);
              i++;
            }
            break;
          case '+':
            {
              int newValue;
              if (cells[pointer] == _maxCellValue && _negativeCellValues)
              {
                newValue = -_maxCellValue;
              }
              else if (cells[pointer] == _maxCellValue && !_negativeCellValues)
              {
                newValue = 0;
              }
              else
              {
                newValue = cells[pointer] + 1;
              }
              cells[pointer] = newValue;
              i++;
              break;
            }
          case '-':
            {
              int newValue;
              if (cells[pointer] == 0 && !_negativeCellValues)
              {
                newValue = _maxCellValue;
              }
              else if (cells[pointer] == -_maxCellValue && _negativeCellValues)
              {
                newValue = _maxCellValue;
              }
              else
              {
                newValue = cells[pointer] - 1;
              }
              cells[pointer] = newValue;
              i++;
              break;
            }
          case '.':
            {
              Console.Write((char)cells[pointer]);
              i++;
              break;
            }
          case ',':
            {
              Console.Write("\nUser input:");
              cells[pointer] = (int)Console.ReadKey().KeyChar;
              Console.Write("\n");
              i++;
              break;
            }
          case '[':
            {
              if (cells[pointer] == 0)
              {
                int brackets = 0;
                for (int j = i; j <= code.Length; j++)
                {
                  if (j == code.Length)
                  {
                    throw new Exception("No matching ] found");
                  }
                  if (code[j] == ']') brackets--;
                  if (code[j] == '[') brackets++;

                  if (brackets == 0)
                  {
                    i = j;
                    break;
                  }
                }
              }
              else
              {
                i++;
              }
              break;
            }
          case ']':
            {
              if (cells[pointer] != 0)
              {
                int brackets = 0;
                for (int j = i; j >= -1; j--)
                {
                  if (j == -1)
                  {
                    throw new Exception("No matching [ found");
                  }
                  if (code[j] == ']') brackets--;
                  if (code[j] == '[') brackets++;

                  if (brackets == 0)
                  {
                    i = j;
                    break;
                  }
                }
              }
              else
              {
                i++;
              }
              break;
            }
          default:
            {
              int skipTo = lowestIndexOf(_legalChars, code, i);
              if (skipTo == -1) isDone = true;
              else i = skipTo;
              break;
            }
        }
        if (steps % 10000000 == 0 && _statistics)
        {
          stopwatch.Stop();
          totalElapsedTime += stopwatch.ElapsedMilliseconds;
          Console.WriteLine("Millions of iterations:{0}; Time elapsed:{1}; Average time:{2}", steps / 10000000, stopwatch.ElapsedMilliseconds, totalElapsedTime / (steps / 10000000));
          stopwatch.Reset();
          stopwatch.Start();
        }
      }
    }
    private int lowestIndexOf(char[] searchFor, string text, int searchFrom)
    {
      int min = -1;
      foreach (char symbol in searchFor)
      {
        int index = text.IndexOf(symbol, searchFrom);
        if (index == -1) continue;
        if (min == -1 || index < min) min = index;
      }
      return min;
    }
    public void OptimizedRun(string inputCode)
    {
      string code = OptimizeCode(inputCode);

      Dictionary<int, int> cells = new Dictionary<int, int>();
      cells.Add(0, 0);

      int pointer = 0;
      int i = 0;
      bool isDone = false;

      int multiplier = 1;

      long steps = 0;
      long totalElapsedTime = 0;
      Stopwatch stopwatch = new Stopwatch();
      if (_statistics)
      {
        stopwatch.Start();
      }

      while (!isDone)
      {
        steps++;
        if (i == code.Length)
        {
          isDone = true;
          break;
        }
        char symbol = code[i];

        switch (symbol)
        {
          case '>':
            {
              pointer += multiplier;
              cells.TryAdd(pointer, 0);
              multiplier = 1;
              i++;
              break;
            }
          case '<':
            {
              pointer -= multiplier;
              if (pointer < 0 && !_negativeCells)
              {
                throw new Exception("Cells can't be negative (no cells to the left of the first one)");
              }
              cells.TryAdd(pointer, 0);
              multiplier = 1;
              i++;
            }
            break;
          case '+':
            {
              int newValue = cells[pointer] + multiplier;
              if (newValue > _maxCellValue && _negativeCellValues)
              {
                newValue = -_maxCellValue + ((newValue - 1) % _maxCellValue);
              }
              else if (newValue > _maxCellValue && !_negativeCellValues)
              {
                newValue = (newValue - 1) % _maxCellValue;
              }

              cells[pointer] = newValue;
              multiplier = 1;
              i++;
              break;
            }
          case '-':
            {
              int newValue = cells[pointer] - multiplier;
              if (newValue < 0 && !_negativeCellValues)
              {
                newValue = _maxCellValue - (-(newValue + 1) % _maxCellValue);
              }
              else if (cells[pointer] < -_maxCellValue && _negativeCellValues)
              {
                newValue = _maxCellValue - (-(newValue + 1) % _maxCellValue);
              }
              cells[pointer] = newValue;
              multiplier = 1;
              i++;
              break;
            }
          case '.':
            {
              string output = "";
              for (int j = 0; j < multiplier; j++)
              {
                output += (char)cells[pointer];
              }
              Console.Write(output);
              multiplier = 1;
              i++;
              break;
            }
          case ',':
            {
              Console.Write("\nUser input:");
              cells[pointer] = (int)Console.ReadKey().KeyChar;
              Console.Write("\n");
              multiplier = 1;
              i++;
              break;
            }
          case '[':
            {
              if (cells[pointer] == 0)
              {
                int brackets = 0;
                for (int j = i; j <= code.Length; j++)
                {
                  if (j == code.Length)
                  {
                    throw new Exception("No matching ] found");
                  }
                  if (code[j] == ']') brackets--;
                  if (code[j] == '[') brackets++;

                  if (brackets == 0)
                  {
                    i = j;
                    break;
                  }
                }
              }
              else
              {
                i++;
              }
              multiplier = 1;
              break;
            }
          case ']':
            {
              if (cells[pointer] != 0)
              {
                int brackets = 0;
                for (int j = i; j >= -1; j--)
                {
                  if (j == -1)
                  {
                    throw new Exception("No matching [ found");
                  }
                  if (code[j] == ']') brackets--;
                  if (code[j] == '[') brackets++;

                  if (brackets == 0)
                  {
                    i = j;
                    break;
                  }
                }
              }
              else
              {
                i++;
              }
              multiplier = 1;
              break;
            }
          default:
            {
              string multString = code[i].ToString();
              for (int j = i + 1; j < code.Length; j++)
              {
                if (Char.IsDigit(code[j])) multString += code[j];
                else
                {
                  i = j;
                  break;
                }
              }
              multiplier = Int32.Parse(multString);
              break;
            }
        }
        if (steps % 10000000 == 0 && _statistics)
        {
          stopwatch.Stop();
          totalElapsedTime += stopwatch.ElapsedMilliseconds;
          Console.WriteLine("Millions of iterations:{0}; Time elapsed:{1}; Average time:{2}", steps / 10000000, stopwatch.ElapsedMilliseconds, totalElapsedTime / (steps / 10000000));
          stopwatch.Reset();
          stopwatch.Start();
        }
      }
    }
    private string OptimizeCode(string inputCode)
    {
      string cleanCode = Clean(inputCode);

      string optimizedCode = "";

      char consecutiveType = ' ';
      int consecutiveCount = 0;

      for (int i = 0; i <= cleanCode.Length; i++)
      {
        if (i == cleanCode.Length)
        {
          if (consecutiveCount == 1) optimizedCode += $"{consecutiveType}";
          else optimizedCode += $"{consecutiveCount}{consecutiveType}";
          break;
        }
        if (consecutiveType != cleanCode[i] || cleanCode[i] == '[' || cleanCode[i] == ']')
        {
          if (consecutiveCount != 0)
          {
            if (consecutiveCount == 1) optimizedCode += $"{consecutiveType}";
            else optimizedCode += $"{consecutiveCount}{consecutiveType}";
          }
          consecutiveType = cleanCode[i];
          consecutiveCount = 1;
        }
        else
        {
          consecutiveCount++;
        }
      }

      return optimizedCode;
    }
  }
}