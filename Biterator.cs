using System;
using System.Collections.Generic;

public static class BiteratorHelper
{
    public static int DetermineBitsUInt(uint number)
    {
        int bits = 1;
        while (number >= (1 << bits))
        {
            bits++;
            if (bits >= sizeof(uint) * 8)
                break;
        }

        return bits;
    }

    public static string ByteToString(byte b)
    {
        string str = "";
        for (int i = 0; i < 8; i++)
        {
            str = ((b >> i) & 1).ToString() + str;
        }
        return str;
    }
}

public class Biterator
{
    private int currentByte = 0;
    private int currentBit = 0;
    private byte[] bytes;

    //for debugging
    private List<int> bitCounts;

    public Biterator(int numBytes)
    {
        bytes = new byte[numBytes];

        bitCounts = new List<int>();
    }

    public Biterator(byte[] data)
    {
        bytes = data;
    }

    public byte[] GetBytes()
    {
        return bytes;
    }

    public List<int> GetBitCounts()
    {
        return bitCounts;
    }

    /// <summary>
    /// Reset the biterator and initialize the length of the new byte array
    /// </summary>
    /// <param name="numBytes"></param>
    public void Reset(int numBytes)
    {
        currentBit = 0;
        currentByte = 0;

        bytes = new byte[numBytes];
        bitCounts.Clear();
    }

    /// <summary>
    /// Reset the biterator and initialize it with a byte array
    /// </summary>
    /// <param name="data"></param>
    public void Reset(byte[] data)
    {
        currentBit = 0;
        currentByte = 0;

        bytes = data;
        bitCounts.Clear();
    }

    void PushBits(int numBits, uint bits)
    {
        int bitsRead = 0;
        while (bitsRead < numBits)
        {
            bytes[currentByte] |= (byte)((bits >> bitsRead) << currentBit);

            int bitsLeftInByte = 8 - currentBit;
            int potentialBitsRead = numBits - bitsRead;

            if (bitsLeftInByte > potentialBitsRead)
            {
                bitsRead += potentialBitsRead;
                currentBit += potentialBitsRead;
            }
            else
            {
                bitsRead += bitsLeftInByte;

                currentBit = 0;
                currentByte++;
            }
        }
    }

    int PopBits(int numBits)
    {
        int returnVal = 0;

        int bitsRead = 0;
        while (bitsRead < numBits)
        {
            byte b = bytes[currentByte];

            int bitsLeftInByte = 8 - currentBit;
            int potentialBitsRead = numBits - bitsRead;

            int bitsToRead = bitsLeftInByte > potentialBitsRead ? potentialBitsRead : bitsLeftInByte;
            int mask = 0;
            for (int i = 0; i < bitsToRead; i++)
                mask |= 1 << i;

            byte test = (byte)((b >> currentBit) & mask);
            returnVal |= (int)(test << bitsRead);

            bitsRead += bitsToRead;
            currentBit += bitsToRead;
            if (currentBit >= 8)
            {
                currentBit = 0;
                currentByte++;
            }
        }

        return returnVal;
    }

    /// <summary>
    /// Compresses a 32-bit unsigned integer
    /// </summary>
    /// <param name="val">The value to compress</param>
    /// <param name="numBits">The number of bits to represent the value. Range [1, 32]</param>
    public void PushUInt(uint val, int numBits = 32)
    {
        AddCount(numBits);

        uint mask = 0;
        for (int i = 0; i < numBits; i++)
            mask |= (uint)(1 << i);

		uint adjustedVal = val & mask;
		PushBits(numBits, adjustedVal);
    }

    /// <summary>
    /// Compresses a 32-bit signed integer
    /// </summary>
    /// <param name="val"> The value to compress</param>
    /// <param name="numBits">The number of bits to represent the value. At least 1 bit is used for the sign. Range [2, 32]</param>
    public void PushInt(int val, int numBits = 32)
    {
        AddCount(numBits);

        int adjustedNumBits = numBits - 1; //to account for signed bit

        int mask = 0;
        for (int i = 0; i < numBits; i++)
            mask |= 1 << i;

        bool negative = false;
        if (val < 0)
        {
            val = -val;
            negative = true;
        }

        int adjustedVal = val & mask;

        PushBits(adjustedNumBits, (uint)adjustedVal);

        byte sign = (byte)(negative ? 0x01 : 0x00);
        bytes[currentByte] |= (byte)(sign << currentBit);

        currentBit++;
        if (currentBit >= 8)
        {
            currentBit = 0;
            currentByte++;
        }
    }

    /// <summary>
    /// Compresses a single-precision floating point number
    /// </summary>
    /// <param name="val">Value to compress</param>
    /// <param name="signed">True if the value needs to be signed</param>
    /// <param name="mantissa">The number of bits dedicated to the precision of the fractional portion of the floating point number. Range [1, 23]</param>
    public void PushFloat(float val, bool signed = true, int mantissa = 23)
    {
        int signSize = signed ? 1 : 0;
        int exponentSize = 8;

        int totalBits = signSize + exponentSize + mantissa;
        AddCount(totalBits);

        byte[] floatBytes = BitConverter.GetBytes(val);

        byte backBits = (byte)(floatBytes[2] & 0x7F);
        byte midBits = floatBytes[1];
        byte frontBits = floatBytes[0];

        int mantissaBits = 0;
        mantissaBits |= frontBits;
        mantissaBits |= midBits << 8;
        mantissaBits |= backBits << 16;
        mantissaBits >>= (23 - mantissa);

        PushBits(mantissa, (uint)mantissaBits);

        byte frontPart = (byte)((floatBytes[3] & 0x7F) << 1);
        byte endPart = (byte)((floatBytes[2] & 0x80) >> 7);
        byte exponent = (byte)(frontPart | endPart);

        PushBits(exponentSize, (uint)exponent);

        if (signed)
        {
            byte sign = (byte)(((floatBytes[3] & 0x80) == 0x80) ? 0x01 : 0x00);
            bytes[currentByte] |= (byte)(sign << currentBit);

            currentBit++;
            if (currentBit >= 8)
            {
                currentBit = 0;
                currentByte++;
            }
        }
    }

    /// <summary>
    /// Compresses a bool into a single bit
    /// </summary>
    /// <param name="val">The value to compress</param>
    public void PushBool(bool val)
    {
        AddCount(1);

        if (val)
            bytes[currentByte] |= (byte)(1 << currentBit);

        currentBit++;
        if (currentBit >= 8)
        {
            currentBit = 0;
            currentByte++;
        }
    }

    /// <summary>
    /// Restores a 32-bit unsigned integer.
    /// </summary>
    /// <param name="numBits">The assumed number of bits</param>
    /// <returns>The restored 32-bit unsigned integer</returns>
    public uint PopUInt(int numBits)
    {
		return (uint)PopBits(numBits);
    }

    /// <summary>
    /// Restores a 32-bit signed integer.
    /// </summary>
    /// <param name="numBits">The assumed number of bits</param>
    /// <returns>The restored 32-bit signed integer</returns>
    public int PopInt(int numBits)
    {
        int returnVal = 0;

        int adjustedNumBits = numBits - 1;
        returnVal = PopBits(adjustedNumBits);

        int negativeMask = 1 << currentBit;
        bool negative = (bytes[currentByte] & negativeMask) == negativeMask ? true : false;

        if (negative)
        {
            //Edge case for current implementation..
            //int.MinValue is all zeros with the signed bit 1
            if (returnVal == 0)
                returnVal = int.MinValue;
            else
                returnVal = (~returnVal + 1);
        }

        currentBit++;
        if (currentBit >= 8)
        {
            currentBit = 0;
            currentByte++;
        }

        return returnVal;
    }

    /// <summary>
    /// Restores a 32-bit single-precision floating point number
    /// </summary>
    /// <param name="signed">Was the value signed when pushed?</param>
    /// <param name="mantissa">The number of bits for the mantissa/significand portion of the value</param>
    /// <returns>The restored 32-bit single-precision floating point number</returns>
    public float PopFloat(bool signed = true, int mantissa = 23)
    {
        byte[] floatBytes = new byte[4];

        int exponentSize = 8;
        int mantissaPadding = 23 - mantissa;
        int readBitCount = exponentSize + mantissa;

        int bitsToConvert = PopBits(readBitCount);
        bitsToConvert <<= mantissaPadding;

        if (signed)
        {
            int mask = 1 << currentBit;
            bitsToConvert |= ((bytes[currentByte] & mask) == mask ? 1 : 0) << 31;

            currentBit++;
            if (currentBit >= 8)
            {
                currentBit = 0;
                currentByte++;
            }
        }

        floatBytes[0] = (byte)(bitsToConvert & 0x000000FF);
        floatBytes[1] = (byte)((bitsToConvert & 0x0000FF00) >> 8);
        floatBytes[2] = (byte)((bitsToConvert & 0x00FF0000) >> 16);
        floatBytes[3] = (byte)((bitsToConvert & 0xFF000000) >> 24);

        float returnVal = BitConverter.ToSingle(floatBytes, 0);
        return returnVal;
    }

    /// <summary>
    /// Restores a boolean value.
    /// </summary>
    /// <returns>The restored bool</returns>
    public bool PopBool()
    {
        int mask = 1 << currentBit;
        bool returnVal = (bytes[currentByte] & mask) == mask ? true : false;

        currentBit++;
        if (currentBit >= 8)
        {
            currentBit = 0;
            currentByte++;
        }

        return returnVal;
    }

    /// <summary>
    /// Stores the amount of bits an element uses when added to the biterator
    /// </summary>
    /// <param name="count"></param>
    private void AddCount(int count)
    {
        bitCounts.Add(count);
    }
}