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

            double minSupport = 0.25;
            List<ItemSet> frequentItemSets = new List<ItemSet>();

            frequentItemSets = GetFrequentItemSets(startingItemSets, transactions, minSupport);
            PrintItemSets(frequentItemSets);
            var joined = JoinItemSets(frequentItemSets);


            while (frequentItemSets.Count() > 0)
            {
                frequentItemSets = GetFrequentItemSets(joined, transactions, minSupport);
                PrintAssociationRules(frequentItemSets, transactions);
                // PrintItemSets(frequentItemSets);
                joined = JoinItemSets(frequentItemSets);
            }
        }

        private static void PrintAssociationRules(IEnumerable<ItemSet> joined, string[] transactions)
        {
            foreach (var itemSet in joined)
            {
                int firstGroupSize = 1;

                while (firstGroupSize < itemSet.items.Count())
                {
                    var firstGroupItems = itemSet.items.Take(firstGroupSize).ToList();
                    var restOfItems = itemSet.items.Skip(firstGroupSize).ToList();

                    var firstGroupItemSet = new List<ItemSet>() { new ItemSet() { items = firstGroupItems } };

                    var supportOfFirstGroup = GetItemSetsSupport(firstGroupItemSet, transactions).First().support;

                    // var supportOfFirstGroup = startingItemSets.Where(itemSet => itemSet.items.Contains(firstItem))
                    //                                             .Select(itemSet => itemSet.support)
                    //                                             .FirstOrDefault();
                    var confidence = itemSet.support / supportOfFirstGroup;
                    System.Console.WriteLine($"{String.Join(",", firstGroupItems)} -> {String.Join(",", restOfItems)}\t{itemSet.support}\t{confidence}");

                    firstGroupSize = firstGroupSize + 1;
                }
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
    }

    class ItemSet
    {
        public double support { get; set; }
        public IEnumerable<string> items { get; set; }
    }
}
