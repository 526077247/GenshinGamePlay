using System;
using System.Collections.Generic;

namespace Nebukam.Collections
{
    static public class Lists
    {

        #region Array

        /// <summary>
        /// Distribute a given amount into a given number of parts with random values
        /// equal to the provided amount when added up.
        /// i.e : 10/4 = 3, 1, 2, 4
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static int[] DistributeAmountRandom(int amount, int parts)
        {
            int[] result = new int[parts];

            for (int i = 0; i < parts; i++)
            {
                int part = (i == parts - 1) ? amount : (int)Nebukam.Common.Maths.Rand(amount, false);
                result[i] = part;
                amount = amount - part;
            }

            return result;
        }

        /// <summary>
        /// Distribute a given amount into a given number of parts, each time dividing 
        /// by two. 
        /// i.e : 10/4 = 5, 3, 1, 1
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static int[] DistributeAmountHalves(int amount, int parts)
        {
            int[] result = new int[parts];

            for (int i = 0; i < parts; i++)
            {
                int part = (i == parts - 1) ? amount : amount / 2;
                result[i] = part;
                amount = amount - part;
            }

            return result;
        }


        #endregion

        #region List

        /// <summary>
        /// Create a copy of a given list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> Copy<T>(List<T> list)
        {
            List<T> listCopy = new List<T>();
            foreach (T item in list)
                listCopy.Add(item);
            return listCopy;
        }

        /// <summary>
        /// Add content of a given source list at the end of the provided target list.
        /// Does not check for duplicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyTo<T>(List<T> source, List<T> target)
        {
            for (int i = 0; i < source.Count; i++)
                target.Add(source[i]);
        }

        /// <summary>
        /// Randomize the indexing of a list's items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Randomize<T>(IList<T> list)
        {

            List<T> listCopy = new List<T>();
            foreach (T item in list)
                listCopy.Add(item);

            int lCount = listCopy.Count;
            int i = 0;

            while (listCopy.Count != 0)
            {
                int rIndex = (int)Math.Floor(UnityEngine.Random.value * listCopy.Count);
                list[i] = listCopy[rIndex];
                listCopy.RemoveAt(rIndex);
                i++;
            }

        }

        /// <summary>
        /// Return an item at random index from a given list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomPick<T>(IList<T> list)
        {
            int range = list.Count - 1;

            if (range == 0)
                return default(T);

            return list[(int)Nebukam.Common.Maths.Rand(range, false)];
        }

        /// <summary>
        /// Return and remove and item at a random index from a given list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomExtract<T>(IList<T> list)
        {
            int range = list.Count - 1;

            if (range == 0)
                return default(T);

            int index = (int)Nebukam.Common.Maths.Rand(range, false);

            T result = list[index];
            list.RemoveAt(index);

            return result;
        }

        #endregion

    }

}