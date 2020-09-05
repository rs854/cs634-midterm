using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cs634_midterm
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] items = File.ReadAllLines("items.txt");
            string[] transactions = File.ReadAllLines("transactions\\1.txt");
            List<ItemSet> startingItemSets = items.Select(item => new ItemSet()
            {
                items = new List<string>() { item }
            }).ToList();
            // Calculate support of starting items
            startingItemSets = GetFrequentItemSets(startingItemSets, transactions, 0);

            double minSupport = 0.15;
            List<ItemSet> frequentItemSets = new List<ItemSet>();

            frequentItemSets = GetFrequentItemSets(startingItemSets, transactions, minSupport);
            // PrintItemSets(frequentItemSets);
            var joined = JoinItemSets(frequentItemSets);
            

            while (frequentItemSets.Count() > 0)
            {
                frequentItemSets = GetFrequentItemSets(joined, transactions, minSupport);
                PrintAssociationRules(frequentItemSets, startingItemSets);
                // PrintItemSets(frequentItemSets);
                joined = JoinItemSets(frequentItemSets);
            }
        }

        private static void PrintAssociationRules(IEnumerable<ItemSet> joined, IEnumerable<ItemSet> startingItemSets)
        {
            foreach (var itemSet in joined)
            {
                var firstItem = itemSet.items.First();
                var restOfItems = itemSet.items.Skip(1).ToList();
                var supportOfFirstItem = startingItemSets.Where(itemSet => itemSet.items.Contains(firstItem))
                                                            .Select(itemSet => itemSet.support)
                                                            .FirstOrDefault();
                var confidence = itemSet.support / supportOfFirstItem;
                System.Console.WriteLine($"{firstItem} -> {String.Join(",", restOfItems)}\t{itemSet.support}\t{confidence}");
            }
        }

        private static void PrintItemSets(List<ItemSet> frequentItemSets)
        {
            if (frequentItemSets.Count() > 0)
            {
                foreach (var itemSet in frequentItemSets)
                {
                    System.Console.WriteLine($"{String.Join(",", itemSet.items)}: {itemSet.support}");
                }

                System.Console.WriteLine("===================");
            }
        }

        private static List<ItemSet> GetFrequentItemSets(IEnumerable<ItemSet> itemSets, string[] transactions, double minSupport)
        {
            double numTransactions = transactions.Count();

            return itemSets.Select(itemSet => new ItemSet()
            {
                support = transactions.Where(t => !itemSet.items
                                                    .Except(t.Split(","))
                                                    .Any())
                                                    .Count() / numTransactions,
                items = itemSet.items,
            }).Where(itemSet => itemSet.support >= minSupport).ToList();
        }

        static IEnumerable<ItemSet> JoinItemSets(IEnumerable<ItemSet> itemSets)
        {
            List<ItemSet> joinedItemSets = new List<ItemSet>();
            List<string> items = itemSets.SelectMany(itemSet => itemSet.items).Distinct().ToList();

            foreach (var itemSet in itemSets)
            {
                foreach (var item in items)
                {
                    // string a = itemSet.items.First();
                    // string b = itemSet2.items.First();
                    if (!itemSet.items.Contains(item))
                    {
                        var joinedItems = new List<string>();
                        foreach (var i in itemSet.items)
                        {
                            joinedItems.Add(i);
                        }
                        joinedItems.Add(item);

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
    }

    class ItemSet
    {
        public double support { get; set; }
        public IEnumerable<string> items { get; set; }
    }
}
