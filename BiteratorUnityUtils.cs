using UnityEngine;
using System.Collections.Generic;

public static class BiteratorUnityUtils
{
    public static void DebugPrintBits(Biterator biterator, string[] colors = null)
    {
        string str = "";

        int bitCountIndex = 0;
        int count = 0;

        int colorIndex = 0;
        if(colors != null && colors.Length > 0)
            str = "</color>" + str;

        byte[] bytes = biterator.GetBytes();
        List<int> bitCounts = biterator.GetBitCounts();

        for (int b = 0; b < bytes.Length; b++)
        {
            for (int i = 0; i < 8; i++)
            {
                int val = (bytes[b] >> i) & 1;
                str = val.ToString() + str;

                count++;
                if (bitCountIndex < bitCounts.Count && count >= bitCounts[bitCountIndex])
                {
                    count = 0;
                    bitCountIndex++;

                    if (colors != null && colors.Length > 0)
                    {
                        str = string.Format("<color={0}>", colors[colorIndex]) + str;
                        str = "</color>" + str;

                        colorIndex = (colorIndex + 1) % colors.Length;
                    }
                }

            }
        }

        if(colors != null && colors.Length > 0)
            str = "<color=gray>" + str;

        Debug.Log(str);
    }
}