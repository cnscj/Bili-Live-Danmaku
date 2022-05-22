using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PackTool
{
    /// <summary>
    /// format the dec integer to a hex stream, for sending msg (pack之后是一个16进制串，需要把一个整数转换成一个4字节小端，即低位存储在低字节的16进制字符串)
    /// </summary>
    /// <param name="size"> dec integer </param>
    /// <returns> hex stream </returns>
    public static string Dec2HexStream(int size)
    {
        string raw = "00000000";
        string hex = Convert.ToString(size, 16);
        //Console.WriteLine("转换字符串 " + hex + " 长度 " + hex.Length);
        int hexLen = hex.Length;
        if (hexLen > 8)
            return "\x00\x00\x00\x00";
        else
        {
            // 100 -> 00 01 00 00 -> 00 00 01 00
            hex = raw.Substring(0, 8 - hexLen) + hex;
        }
        //Console.WriteLine("加0字符串 " + hex);

        string res = "";
        for (int i = 3; i >= 0; i--)
        {
            res += hex.Substring(i * 2, 2);
        }
        hex = res;

        //Console.WriteLine("反转字符串 " + hex);
        res = "";
        for (int i = 0; i < 4; i++)
        {
            res += "\\x" + hex.Substring(i * 2, 2);
        }
        //Console.WriteLine("结果字符串 " + res);
        return Regex.Unescape(res);
    }

    /// <summary>
    /// for recving msg and parsering the size of data  ((对应的将4字节小端存储的16进制字符串转换成为一个整数)
    /// </summary>
    /// <param name="s"> hex stream </param>
    /// <returns>dec integer</returns>
    public static int HexStream2Dec(string s)
    {
        // 00 01 00 00
        // 00 00 01 00 
        // \x01\x00\x00\x00 小端
        UInt32 res = 0;
        //Console.WriteLine("处理字符串 " + s);
        string tmp = "";
        for (int i = 3; i >= 0; i--)
        {
            UInt32 m = Convert.ToUInt32(s[i]);
            string hexs = Convert.ToString(m, 16);
            tmp += hexs;
        }
        //Console.WriteLine("还原字符串 " + tmp + " 长度 " + (tmp.Length).ToString());
        res = Convert.ToUInt32(tmp, 16);
        return (int)res;
    }

    /// <summary>
    /// 封包头
    /// </summary>
    /// <param name="packHead"></param>
    /// <returns></returns>
    public static byte[] PackHeadBytes(short packHead)
    {
        return BitConverter.GetBytes(packHead);
    }

    /// <summary>
    /// 封包(包含包头和包体)
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
    /// 拆包(返回short包头,out出包体)
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
    /// 拆包头(参数只有包头)
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
