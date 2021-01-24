using System;
using System.Collections.Generic;
using System.Linq;

namespace Spigot.Demo.Models
{
    public class ComplexModelWithChildren
    {
        private static Random rando = new Random(DateTime.Now.Millisecond);
        public DateTimeOffset DateTimeOffset { get; set; }

        public List<ComplexChild> ComplexChildren { get; set; }

        public static ComplexModelWithChildren Random() =>
            new ComplexModelWithChildren
            {
                DateTimeOffset = DateTimeOffset.Now.AddHours(rando.Next(-1000, 1000)),
                ComplexChildren = Enumerable.Range(0, rando.Next(20)).Select(x => ComplexChild.Random()).ToList()
            };

        public override string ToString()
        {
            return $"DateTimeOffset={DateTimeOffset} and {ComplexChildren?.Count} children";
        }
    }
}