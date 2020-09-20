using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace cs634_midterm
{
  static class Program
  {
    static List<ItemSet> infrequentItemSets = new List<ItemSet>();

    static void Main(string[] args)
    {
      // Get user input parameters
      Console.WriteLine("Enter min support as a decimal (i.e. 0.3):");
      double minSupport = Convert.ToDouble(Console.ReadLine());

      Console.WriteLine("Enter min confidence as a decimal (i.e. 0.8):");
      double minConfidence = Convert.ToDouble(Console.ReadLine());

      Console.WriteLine("Select database (1, 2, 3, 4, or 5):");
      string[] transactions = File.ReadAllLines($"transactions\\{Console.ReadLine()}.txt");

      Console.WriteLine("Select algorithm (1 = Apriori, 2 = Brute Force):");
      int algorithm = Convert.ToInt32(Console.ReadLine());

      Stopwatch sw = new Stopwatch();
      sw.Start();
      if (algorithm == 1)
      {
        Apriori(transactions, minSupport, minConfidence);
        sw.Stop();
        double elapsed = (double)sw.ElapsedMilliseconds / 1000;
        Console.WriteLine($"Apriori algorithm completed in {elapsed} seconds");
      }
      else
      {
        BruteForce(transactions, minSupport, minConfidence);
        sw.Stop();
        double elapsed = (double)sw.ElapsedMilliseconds / 1000;
        Console.WriteLine($"Brute Force algorithm completed in {elapsed} seconds");
      }

    }

    private static void BruteForce(string[] transactions, double minSupport, double minConf)
    {
      int iteration = 0;
      string[] items = File.ReadAllLines("items.txt");

      List<ItemSet> startingItemSets = items.Select(item => new ItemSet()
      {
        items = new List<string>() { item }
      }).ToList();

      var joined = startingItemSets.ToList();
      var frequentItemSets = joined;

      while (frequentItemSets.Count() > 0)
      {
        iteration += 1;
        joined = JoinItemSets(joined).ToList();
        frequentItemSets = GetFrequentItemSets(joined, transactions, minSupport);
        PrintItemSets(frequentItemSets, $"Iteration #{iteration}");
        PrintAssociationRules(frequentItemSets, transactions, minConf, $"Iteration #{iteration}");
      }
    }

    private static void Apriori(string[] transactions, double minSupport, double minConfidence)
    {
      string[] items = File.ReadAllLines("items.txt");

      List<ItemSet> startingItemSets = items.Select(item => new ItemSet()
      {
        items = new List<string>() { item }
      }).ToList();
      // Calculate support of starting items
      startingItemSets = GetFrequentItemSets(startingItemSets, transactions, 0);

      List<ItemSet> frequentItemSets = new List<ItemSet>();

      int iter = 0;

      frequentItemSets = GetFrequentItemSets(startingItemSets, transactions, minSupport);
      PrintItemSets(frequentItemSets, $"Frequent Items");
      var joined = JoinItemSets(frequentItemSets);
      joined = PruneItemSets(joined);


      while (frequentItemSets.Count() > 0)
      {
        iter += 1;
        frequentItemSets = GetFrequentItemSets(joined, transactions, minSupport);
        PrintItemSets(frequentItemSets, $"Iteration #{iter}");
        PrintAssociationRules(frequentItemSets, transactions, minConfidence, $"Iteration #{iter}");
        joined = JoinItemSets(frequentItemSets);
        joined = PruneItemSets(joined);
      }
    }

    private static void PrintAssociationRules(IEnumerable<ItemSet> joined, string[] transactions, double minConf, string title)
    {
      if (joined.Count() > 0)
      {
        Table t = new Table(new List<string>() { "Association Rule", "Support", "Confidence" }, title);

        foreach (var itemSet in joined)
        {
          // Shift arrow to the right
          for (int firstGroupSize = 1; firstGroupSize < itemSet.items.Count(); firstGroupSize++)
          {
            // Shift elements to the left
            for (int j = 0; j < itemSet.items.Count(); j++)
            {
              itemSet.items = RotateItems(itemSet);

              // Split items at arrow position
              var firstGroupItems = itemSet.items.Take(firstGroupSize).ToList();
              var restOfItems = itemSet.items.Skip(firstGroupSize).ToList();

              var firstGroupItemSet = new List<ItemSet>() { new ItemSet() { items = firstGroupItems } };

              var supportOfFirstGroup = GetItemSetsSupport(firstGroupItemSet, transactions).First().support;

              var confidence = itemSet.support / supportOfFirstGroup;
              if (confidence >= minConf)
              {
                string rule = $"{firstGroupItems.ItemSetToString()} --> {restOfItems.ItemSetToString()}";

                t.Rows.Add(new List<string>() { rule, itemSet.support.ToString("0.000"), confidence.ToString("0.000") });
              }
            }
          }
        }

        t.Print();
        Console.WriteLine("");
      }
    }

    private static string[] RotateItems(ItemSet itemSet)
    {
      return itemSet.items.Skip(1).Concat(itemSet.items.Take(1)).ToArray();
    }

    private static void PrintItemSets(List<ItemSet> frequentItemSets, string title, bool includeSupport = true)
    {
      if (frequentItemSets.Count() > 0)
      {
        var headers = new List<string>() { "Frequent Items" };
        if (includeSupport)
        {
          headers.Add("Support");
        }
        Table t = new Table(headers, title);

        foreach (var itemSet in frequentItemSets)
        {
          var items = new List<string>() { itemSet.ToString() };
          if (includeSupport)
          {
            items.Add(itemSet.support.ToString("0.000"));
          }
          t.Rows.Add(items);
        }

        t.Print();
        Console.WriteLine("");
      }
    }

    private static List<ItemSet> GetFrequentItemSets(IEnumerable<ItemSet> itemSets, string[] transactions, double minSupport)
    {
      IEnumerable<ItemSet> itemSetsWithSupport = GetItemSetsSupport(itemSets, transactions);

      // Remeber infrequent item sets for future pruning
      infrequentItemSets.AddRange(itemSetsWithSupport.Where(itemSet => itemSet.support < minSupport));

      return itemSetsWithSupport.Where(itemSet => itemSet.support >= minSupport).ToList();
    }

    private static IEnumerable<ItemSet> GetItemSetsSupport(IEnumerable<ItemSet> itemSets, string[] transactions)
    {
      double numTransactions = transactions.Count();

      var itemSetsWithSupport = itemSets.Select(itemSet => new ItemSet()
      {
        support = transactions.Where(t => !itemSet.items
                          .Except(t.Split(","))
                          .Any())
                          .Count() / numTransactions,
        items = itemSet.items,
      });
      return itemSetsWithSupport;
    }

    static IEnumerable<ItemSet> JoinItemSets(IEnumerable<ItemSet> itemSets)
    {
      List<ItemSet> joinedItemSets = new List<ItemSet>();
      List<string> items = itemSets.SelectMany(itemSet => itemSet.items).Distinct().ToList();

      foreach (var itemSet in itemSets)
      {
        foreach (var item in items)
        {
          // Check that the item is not a duplicate
          if (!itemSet.items.Contains(item))
          {
            var joinedItems = new List<string>();
            foreach (var i in itemSet.items)
            {
              joinedItems.Add(i);
            }
            joinedItems.Add(item);

            joinedItemSets.Add(new ItemSet()
            {
              items = joinedItems,
            });
          }
        }
      }

      // Remove duplicates that are in a different order
      return joinedItemSets.Distinct();
    }

    static IEnumerable<ItemSet> PruneItemSets(IEnumerable<ItemSet> itemSets)
    {
      // Filter out item sets that are supersets of infrequent subsets
      return itemSets.Where(itemSet => !infrequentItemSets.Where(infreq => itemSet.Contains(infreq)).Any()).ToList();
    }

    public static string ItemSetToString(this List<string> items)
    {
      if (items != null && items.Count() > 0)
      {
        return "{ " + String.Join(", ", items) + " }";
      }
      else
      {
        return "{}";
      }
    }
  }

  // Data class that contains a collection of items
  class ItemSet : IEquatable<ItemSet>
  {
    public double support { get; set; }
    public IEnumerable<string> items { get; set; }

    public override string ToString()
    {
      if (items != null && items.Count() > 0)
      {
        return "{ " + String.Join(", ", items) + " }";
      }
      else
      {
        return base.ToString();
      }
    }

    public bool Equals(ItemSet other)
    {

      //Check whether the compared object is null.
      if (Object.ReferenceEquals(other, null)) return false;

      //Check whether the compared object references the same data.
      if (Object.ReferenceEquals(this, other)) return true;

      //Check whether the products' properties are equal.
      return this.GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
      return String.Join(",", this.items.OrderBy(x => x)).GetHashCode();
    }

    // Check if this item set is a superset of the other
    public bool Contains(ItemSet itemSet)
    {
      return !itemSet.items.Except(items).Any();
    }
  }

  // Helper class to print tables to the console
  public class Table
  {
    public string title { get; set; }
    public List<List<string>> Rows { get; set; }

    private List<int> columnWidths;
    private List<string> headers;

    public Table(List<string> headers, string title)
    {
      this.headers = headers;
      this.Rows = new List<List<string>>();
      this.title = title;
    }

    public void Print()
    {
      columnWidths = new List<int>();

      for (int i = 0; i < headers.Count(); i++)
      {
        int maxLength = Math.Max(MaxRowLength(i), headers.ElementAt(i).Length);
        columnWidths.Add(maxLength + 2);
      }

      PrintTitle();
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

    private void PrintTitle()
    {
      PrintLine(false, false);
      PrintCell(title, true, columnWidths.Sum() + columnWidths.Count() - 1);
      Console.WriteLine("|");
      PrintLine();
    }

    private void PrintHeaders()
    {
      PrintRow(headers, true);
      PrintLine();
    }

    private void PrintRow(IEnumerable<string> row, bool centered = false)
    {
      int column = 0;
      foreach (var columnWidth in columnWidths)
      {
        string element = row.ElementAt(column);
        PrintCell(element, centered, columnWidth);
        column = column + 1;
      }
      Console.WriteLine("|");
    }

    private static void PrintCell(string element, bool centered, int columnWidth)
    {
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

    private void PrintLine(bool bold = false, bool separators = true)
    {
      foreach (var columnWidth in columnWidths)
      {
        Console.Write(separators ? "+" : bold ? "=" : "-");
        for (int i = 0; i < columnWidth; i++)
        {

          Console.Write(bold ? "=" : "-");
        }
      }
      Console.WriteLine("+");
    }
  }
}
