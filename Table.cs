using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cs634_midterm
{
    public class Table
    {
        public List<List<string>> Rows { get; set; }

        private List<int> columnWidths;
        private List<string> headers;

        public Table(List<string> headers)
        {
            this.headers = headers;
            this.Rows = new List<List<string>>();
        }

        public void Print()
        {
            columnWidths = new List<int>();

            for (int i = 0; i < headers.Count(); i++)
            {
                int maxLength = Math.Max(MaxRowLength(i), headers.ElementAt(i).Length);
                columnWidths.Add(maxLength + 2);
            }

            PrintHeaders();
            PrintRows();
        }

        private int MaxRowLength(int i)
        {
            if (Rows.Count() > 0)
            {
                return Rows.Select(r => r.ElementAt(i).Length).Max();
            }
            else
            {
                return 0;
            }
        }

        private void PrintRows()
        {
            foreach (var row in Rows)
            {
                PrintRow(row);
            }
            PrintLine();
        }

        private void PrintHeaders()
        {
            PrintLine();
            PrintRow(headers, true);
            PrintLine();
        }

        private void PrintRow(IEnumerable<string> row, bool centered = false)
        {
            int column = 0;
            foreach (var columnWidth in columnWidths)
            {
                string element = row.ElementAt(column);
                int numSpaces = columnWidth - element.Length - 1;
                Console.Write("| ");

                if (centered)
                {
                    int numStartSpaces = numSpaces / 2;
                    int numEndSpaces = numSpaces - numStartSpaces;
                    PrintSpaces(numStartSpaces);
                    Console.Write(element);
                    PrintSpaces(numEndSpaces);
                }
                else
                {
                    Console.Write(element);
                    PrintSpaces(numSpaces);
                }
                column = column + 1;
            }
            Console.WriteLine("|");
        }

        private static void PrintSpaces(int numSpaces)
        {
            string startSpaces = "";
            for (int i = 0; i < numSpaces; i++)
            {
                startSpaces += " ";
            }
            Console.Write(startSpaces);
        }

        private void PrintLine(bool bold = false)
        {
            foreach (var columnWidth in columnWidths)
            {
                Console.Write("+");
                for (int i = 0; i < columnWidth; i++)
                {

                    Console.Write(bold ? "=" : "-");
                }
            }
            Console.WriteLine("+");
        }
    }
}