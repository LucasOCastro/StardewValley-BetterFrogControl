namespace StardewBetterFrog;

public static class CollectionExtension
{
    public static int IndexOf<T>(this T[] arr, T val)
    {
        for (int i = 0; i < arr.Length; i++)
            if ((arr[i] == null && val == null) || (val?.Equals(arr[i]) ?? arr[i]?.Equals(val) ?? false))
                return i;
        return -1;
    }
}