using System;

namespace TaoTie
{
    public static class SortHelper
    {
        public static void Sort<T>(this T[] array, Func<T,T,int> compare) 
        {
	        try
	        {
		        if (array == null || array.Length < 2)
			        return;

		        Sort(array, 0, array.Length - 1, compare);
	        }
	        catch (Exception ex)
	        {
		        Log.Error(ex);
	        }
        }

        private static void Sort<T>(T[] array, int left, int right, Func<T,T,int> compare)
        {
            if (left >= right)
                return;

            // 小数组使用插入排序优化
            if (right - left + 1 < 16)
            {
                InsertionSort(array, left, right, compare);
                return;
            }

            // 三数取中选取基准
            int pivotIndex = MedianOfThree(array, left, right, compare);
            Swap(array, pivotIndex, right);

            int partitionIndex = Partition(array, left, right, compare);

            Sort(array, left, partitionIndex - 1, compare);
            Sort(array, partitionIndex + 1, right, compare);
        }

        // 三数取中：选择left, mid, right的中位数
        private static int MedianOfThree<T>(T[] array, int left, int right, Func<T, T, int> compare)
        {
            int mid = left + (right - left) / 2;

            if (compare(array[left], array[mid]) < 0)
                Swap(array, left, mid);
            if (compare(array[left], array[right]) < 0)
                Swap(array, left, right);
            if (compare(array[mid], array[right]) < 0)
                Swap(array, mid, right);

            return mid; // 现在mid是中位数
        }

        private static int Partition<T>(T[] array, int left, int right, Func<T,T,int> compare)
        {
            var pivot = array[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (compare(array[j], pivot) > 0)
                {
                    i++;
                    Swap(array, i, j);
                }
            }

            Swap(array, i + 1, right);
            return i + 1;
        }

        // 插入排序用于小数组
        private static void InsertionSort<T>(T[] array, int left, int right, Func<T,T,int> compare)
        {
            for (int i = left + 1; i <= right; i++)
            {
                var key = array[i];
                int j = i - 1;

                while (j >= left && compare(array[j], key) < 0)
                {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = key;
            }
        }

        private static void Swap<T>(T[] array, int i, int j)
        {
            (array[i], array[j]) = (array[j], array[i]);
        }

        public static int LongCompare(long a, long b)
        {
            var res = a - b;
            if (res < int.MinValue) return int.MinValue;
            if (res > int.MaxValue) return int.MaxValue;
            return (int) res;
        }
    }
}