using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public class JoinResult<K, T> 
        where K:class 
        where T:class
    {
        public JoinResult(K l, T r)
        {
            Left = l;
            Right = r;
        }
        public K Left { get; private set; }
        public T Right { get; private set; }
    }

    public enum JoinOrder
    {
        Natural,
        MatchFirst,
        FailFirst
    }

    public static class JoinUtil
    {
        public static IEnumerable<T> DoSkip<T>(this IEnumerable<T> coll,int count)
        {
            foreach (var item in coll)
            {
                if (count < 0)
                {
                    yield return item;
                }
                count--;
            }
        }

        public static IEnumerable<T> DoTake<T>(this IEnumerable<T> coll,int count)
        {
            foreach (var item in coll)
            {
                if (count >= 0)
                {
                    yield return item;
                }
                else {
                    yield break;
                }
                count--;
            }
        }

        public static IEnumerable<JoinResult<K, T>> InnerJoin<K, T, D>(this IEnumerable<K> left, IEnumerable<T> right, Func<K, D> lkey, Func<T, D> rkey) 
            where K:class 
            where T:class
        {
            var rightItems = right.ToDictionary<T, D>(rkey);

            foreach (var leftItem in left)
            {
                if (rightItems.ContainsKey(lkey(leftItem)))
                {
                    yield return new JoinResult<K, T>(leftItem, rightItems[lkey(leftItem)]);
                }
            }
        }

        public static IEnumerable<JoinResult<K, T>> LeftJoin<K, T, D>(this IEnumerable<K> left, IEnumerable<T> right, Func<K, D> lkey, Func<T, D> rkey,JoinOrder order = JoinOrder.Natural)
            where K : class
            where T : class
        {
            var rightItems = right.ToDictionary<T, D>(rkey);
            var others = new List<JoinResult<K, T>>();

            switch (order)
            {
                case (JoinOrder.FailFirst):
                    foreach (var leftItem in left)
                    {
                        if (rightItems.ContainsKey(lkey(leftItem)))
                        {
                            others.Add(new JoinResult<K, T>(leftItem, rightItems[lkey(leftItem)]));
                        }
                        else
                        {
                            yield return new JoinResult<K, T>(leftItem, null);
                        }
                    }
                    foreach (var other in others)
                    {
                        yield return other;
                    }
                    break;
                case (JoinOrder.MatchFirst):
                    foreach (var leftItem in left)
                    {
                        if (rightItems.ContainsKey(lkey(leftItem)))
                        {
                            yield return new JoinResult<K, T>(leftItem, rightItems[lkey(leftItem)]);
                        }
                        else
                        {
                            others.Add(new JoinResult<K, T>(leftItem, null));
                        }
                    }
                    foreach (var other in others)
                    {
                        yield return other;
                    }
                    break;
                case(JoinOrder.Natural):
                    foreach (var leftItem in left)
                    {
                        if (rightItems.ContainsKey(lkey(leftItem)))
                        {
                            yield return new JoinResult<K, T>(leftItem, rightItems[lkey(leftItem)]);
                        }
                        else
                        {
                            yield return new JoinResult<K, T>(leftItem, null);
                        }
                    }
                    break;

            }

            
        }
    }
}
