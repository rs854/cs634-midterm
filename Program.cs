using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cs634_midterm
{
    static class Program
    {
        static void Main(string[] args)
        {
            string[] items = File.ReadAllLines("items.txt");
            string[] transactions = File.ReadAllLines("transactions\\2.txt");
            List<ItemSet> startingItemSets = items.Select(item => new ItemSet()
            {
                items = new List<string>() { item }
            }).ToList();
            // Calculate support of starting items
            startingItemSets = GetFrequentItemSets(startingItemSets, transactions, 0);

            // Get user input parameters
            // Console.WriteLine("Enter min support:");
            // double minSupport = Convert.ToDouble(Console.ReadLine());
            // Console.WriteLine("Enter min confidence:");
            // double minConfidence = Convert.ToDouble(Console.ReadLine());
            double minSupport = 0.4;
            double minConfidence = 0.8;

            List<ItemSet> frequentItemSets = new List<ItemSet>();

            Console.WriteLine("Iteration 1");
            frequentItemSets = GetFrequentItemSets(startingItemSets, transactions, minSupport);
            PrintItemSets(frequentItemSets);
            var joined = JoinItemSets(frequentItemSets);


            while (frequentItemSets.Count() > 0)
            {
                frequentItemSets = GetFrequentItemSets(joined, transactions, minSupport);
                PrintItemSets(frequentItemSets);
                PrintAssociationRules(frequentItemSets, transactions, minConfidence);
                joined = JoinItemSets(frequentItemSets);
            }
        }

        private static void PrintAssociationRules(IEnumerable<ItemSet> joined, string[] transactions, double minConfidence)
        {
            if (joined.Count() > 0)
            {
                Table t = new Table(new List<string>() { "Association Rule", "Support", "Confidence" });

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
                            if (confidence >= minConfidence)
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

        private static void PrintItemSets(List<ItemSet> frequentItemSets)
        {
            if (frequentItemSets.Count() > 0)
            {

                Table t = new Table(new List<string>() { "Frequent Items", "Support" });

                foreach (var itemSet in frequentItemSets)
                {
                    t.Rows.Add(new List<string>() { itemSet.ToString(), itemSet.support.ToString("0.000") });
                }

                t.Print();
                Console.WriteLine("");
            }
        }

        private static List<ItemSet> GetFrequentItemSets(IEnumerable<ItemSet> itemSets, string[] transactions, double minSupport)
        {
            IEnumerable<ItemSet> itemSetsWithSupport = GetItemSetsSupport(itemSets, transactions);

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
                        // Tentitively add the new item to the existing items
                        var joinedItems = new List<string>();
                        foreach (var i in itemSet.items)
                        {
                            joinedItems.Add(i);
                        }
                        joinedItems.Add(item);

                        // Check that the new group doesn't already exist in a different order
                        if (joinedItemSets.Select(jis => jis.items.Except(joinedItems).Any()).All(o => o))
                        {
                            joinedItemSets.Add(new ItemSet()
                            {
                                items = joinedItems,
                            });
                        }
                    }
                }
            }

            return joinedItemSets;
        }

        // static bool ItemsAreSame(IEnumerable<string> a, IEnumerable<string> b)
        // {

        // }

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



    class ItemSet
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
    }
}
