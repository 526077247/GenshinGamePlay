using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nebukam
{
    public static class CollectionExtensions
    {


        /// <summary>
        /// Return the last item of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Last<T>(this IList<T> @this)
        {
            int index = @this.Count - 1;
            if (index < 0) { return default(T); }

            return @this[index];
        }

        /// <summary>
        /// Return the first item of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T First<T>(this IList<T> @this)
        {
            if (@this.Count == 0) { return default(T); }
            return @this[0];
        }

        /// <summary>
        /// Return an item at random index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomPick<T>(this IList<T> @this)
        {
            return Nebukam.Collections.Lists.RandomPick(@this);
        }

        /// <summary>
        /// Return and remove and item at a random index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RandomExtract<T>(this IList<T> @this)
        {
            return Nebukam.Collections.Lists.RandomExtract(@this);
        }

        /// <summary>
        /// Randomize this list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Randomize<T>(this IList<T> @this)
        {
            Nebukam.Collections.Lists.Randomize(@this);
        }

        /// <summary>
        /// Removes and return the last item of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pop<T>(this IList<T> @this)
        {
            int index = @this.Count - 1;
            if (index < 0) { return default(T); }

            T result = @this[index];
            @this.RemoveAt(index);
            return result;
        }

        /// <summary>
        /// Removes and return the first item of the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Shift<T>(this IList<T> @this)
        {

            if (@this.Count == 0) { return default(T); }

            T result = @this[0];
            @this.RemoveAt(0);
            return result;
        }

        /// <summary>
        /// Only add an item if it isn't already in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddOnce<T>(this IList<T> @this, T item)
        {
            if (@this.IndexOf(item) != -1) { return item; }
            @this.Add(item);
            return item;
        }

        /// <summary>
        /// Only add an item if it isn't already in the list, with a boolean
        /// returning whether or not the iteam was already in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <param name="added"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddOnce<T>(this IList<T> @this, T item, out bool added)
        {
            if (@this.IndexOf(item) != -1)
            {
                added = false;
                return item;
            }

            added = true;
            @this.Add(item);
            return item;
        }

        /// <summary>
        /// Only add an item if it isn't already in the list, with a boolean
        /// returning whether or not the iteam was already in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <param name="added"></param>
        /// <returns>True if the item has been added, false if the item was already present in the collection</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddOnce<T>(this IList<T> @this, T item)
        {
            if (@this.IndexOf(item) != -1)
                return false;

            @this.Add(item);
            return true;
        }

        /// <summary>
        /// Only add an item if it isn't already in the list, with a boolean
        /// returning whether or not the iteam was already in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <param name="added"></param>
        /// <returns>True if the item has been added, false if the item was already present in the collection</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAddOnce<T>(this HashSet<T> @this, T item)
        {
            if (@this.Contains(item))
                return false;

            @this.Add(item);
            return true;
        }

        /// <summary>
        /// Removes an item from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <param name="removed"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remove<T>(this IList<T> @this, T item, out bool removed)
        {
            int index = @this.IndexOf(item);
            if (index == -1)
            {
                removed = false;
                return item;
            }

            removed = true;
            @this.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Removes an item from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <returns>True if the item has been removed, false if it wasn't in the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(this IList<T> @this, T item)
        {
            int index = @this.IndexOf(item);

            if (index == -1)
                return false;

            @this.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes an item from a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="item"></param>
        /// <returns>True if the item has been removed, false if it wasn't in the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(this HashSet<T> @this, T item)
        {
            if (!@this.Contains(item))
                return false;

            @this.Remove(item);
            return true;
        }

        /// <summary>
        /// Returns whether the list is empty or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this IList<T> @this)
        {
            return !(@this.Count != 0);
        }

    }
}
