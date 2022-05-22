using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PackTool
{
    /// <summary>
    /// format the dec integer to a hex stream, for sending msg (pack֮����һ��16���ƴ�����Ҫ��һ������ת����һ��4�ֽ�С�ˣ�����λ�洢�ڵ��ֽڵ�16�����ַ���)
    /// </summary>
    /// <param name="size"> dec integer </param>
    /// <returns> hex stream </returns>
    public static string Dec2HexStream(int size)
    {
        string raw = "00000000";
        string hex = Convert.ToString(size, 16);
        //Console.WriteLine("ת���ַ��� " + hex + " ���� " + hex.Length);
        int hexLen = hex.Length;
        if (hexLen > 8)
            return "\x00\x00\x00\x00";
        else
        {
            // 100 -> 00 01 00 00 -> 00 00 01 00
            hex = raw.Substring(0, 8 - hexLen) + hex;
        }
        //Console.WriteLine("��0�ַ��� " + hex);

        string res = "";
        for (int i = 3; i >= 0; i--)
        {
            res += hex.Substring(i * 2, 2);
        }
        hex = res;

        //Console.WriteLine("��ת�ַ��� " + hex);
        res = "";
        for (int i = 0; i < 4; i++)
        {
            res += "\\x" + hex.Substring(i * 2, 2);
        }
        //Console.WriteLine("����ַ��� " + res);
        return Regex.Unescape(res);
    }

    /// <summary>
    /// for recving msg and parsering the size of data  ((��Ӧ�Ľ�4�ֽ�С�˴洢��16�����ַ���ת����Ϊһ������)
    /// </summary>
    /// <param name="s"> hex stream </param>
    /// <returns>dec integer</returns>
    public static int HexStream2Dec(string s)
    {
        // 00 01 00 00
        // 00 00 01 00 
        // \x01\x00\x00\x00 С��
        UInt32 res = 0;
        //Console.WriteLine("�����ַ��� " + s);
        string tmp = "";
        for (int i = 3; i >= 0; i--)
        {
            UInt32 m = Convert.ToUInt32(s[i]);
            string hexs = Convert.ToString(m, 16);
            tmp += hexs;
        }
        //Console.WriteLine("��ԭ�ַ��� " + tmp + " ���� " + (tmp.Length).ToString());
        res = Convert.ToUInt32(tmp, 16);
        return (int)res;
    }

    /// <summary>
    /// ���ͷ
    /// </summary>
    /// <param name="packHead"></param>
    /// <returns></returns>
    public static byte[] PackHeadBytes(short packHead)
    {
        return BitConverter.GetBytes(packHead);
    }

    /// <summary>
    /// ���(������ͷ�Ͱ���)
    /// </summary>
    /// <param name="packHead"></param>
    /// <param name="packBody"></param>
    /// <returns></returns>
    public static byte[] PackBytes(short packHead, byte[] packBody)
    {
        byte[] headBytes = BitConverter.GetBytes(packHead);
        byte[] packBytes = new byte[headBytes.Length + packBody.Length];
        for (int i = 0; i < packBytes.Length; i++)
        {
            if (i < headBytes.Length)
                packBytes[i] = headBytes[i];
            else
                packBytes[i] = packBody[i - 2];
        }
        return packBytes;
    }

    /// <summary>
    /// ���(����short��ͷ,out������)
    /// </summary>
    /// <param name="packBytes"></param>
    /// <param name="packBody"></param>
    /// <returns></returns>
    public static short UnPack(byte[] packBytes, out byte[] packBody)
    {
        byte[] packHead = new byte[2];
        packBody = new byte[packBytes.Length - packHead.Length];
        for (int i = 0; i < packBytes.Length; i++)
        {
            if (i < 2)
                packHead[i] = packBytes[i];
            else
                packBody[i - 2] = packBytes[i];
        }
        short packHeadShort = BitConverter.ToInt16(packHead, 0);
        return packHeadShort;
    }

    /// <summary>
    /// ���ͷ(����ֻ�а�ͷ)
    /// </summary>
    /// <param name="packBytes"></param>
    /// <returns></returns>
    public static short UnPackHead(byte[] packBytes)
    {
        byte[] packHead = new byte[2];
        short packHeadShort = BitConverter.ToInt16(packHead, 0);
        return packHeadShort;
    }


    public static short PackHeader(List<KeyValuePair<int,int>> headerList)
    {
        BitArray ba1 = new BitArray(16);

        return 0;

    }

    public static List<KeyValuePair<int, int>> UnpackHeader(short header)
    {
        return null;

    }

    public static byte[] BitArray2ByteArray(BitArray myBitArray)
    {
        byte[] mybyte = new byte[myBitArray.Length/8];
        myBitArray.CopyTo(mybyte, 0);

        return mybyte;
    }
}
