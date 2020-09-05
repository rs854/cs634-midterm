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
            double numItems = items.Count();
            string[] transactions = File.ReadAllLines("transactions\\1.txt");

            double minSupport = 0.2;

            List<ItemSet> frequentItemSets = items.Select(item => new ItemSet()
            {
                support = transactions.Where(t => t.Contains(item)).Count() / numItems,
                items = new List<string>() { item },
            })
            .Where(itemSet => itemSet.support >= minSupport)
            .ToList();

            var joined = JoinItemSets(frequentItemSets);


            foreach (var itemSet in joined)
            {
                System.Console.WriteLine($"{String.Join(",", itemSet.items)}: {itemSet.support}");
            }
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
                        foreach (var i in items)
                        {
                            joinedItems.Add(i);
                        }
                        joinedItems.Add(item);

                        if (joinedItemSets.Select(jis => jis.items.Except(joinedItems).Any()).All(o => o))
                        {
                            joinedItemSets.Add(new ItemSet()
                            {
                                items = items,
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
        public double confidence { get; set; }
        public IEnumerable<string> items { get; set; }
    }
}
