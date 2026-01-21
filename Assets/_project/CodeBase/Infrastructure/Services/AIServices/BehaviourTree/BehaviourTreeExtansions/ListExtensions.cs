using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.Services.AIServices.BehaviourTree.BehaviourTreeExtansions
{
    public static class ListExtensions
    {
        static Random rng;

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (rng == null) rng = new Random();
            int count = list.Count;
            while (count > 1)
            {
                --count;
                int index = rng.Next(count + 1);
                (list[index], list[count]) = (list[index], list[count]);
            }

            return list;
        }
    }
}
