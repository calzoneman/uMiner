/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace uMiner
{
    /**
     * A Class to manage packet data.
     * Provides methods for appending data of various types
     */
    public class Packet
    {
        public byte[] raw;
        public int currentIndex;

        public Packet(int size)
        {
            this.raw = new byte[size];
            this.currentIndex = 0;
        }

        public Packet(byte[] data)
        {
            this.raw = data;
            this.currentIndex = data.Length;
        }

        public void Append(int data)
        {
            BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data)).CopyTo(raw, currentIndex);
            currentIndex += 4;
        }

        public void Append(uint data)
        {
            BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data)).CopyTo(raw, currentIndex);
            currentIndex += 4;
        }

        public void Append(short data)
        {
            BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data)).CopyTo(raw, currentIndex);
            currentIndex += 2;
        }

        public void Append(byte data)
        {
            raw[currentIndex] = data;
            currentIndex++;
        }

        public void Append(string data)
        {
            Encoding.UTF8.GetBytes(data.PadRight(64).Substring(0, 64)).CopyTo(raw, currentIndex);
            currentIndex += 64;
        }

        public void Append(byte[] data)
        {
            data.CopyTo(raw, currentIndex);
            currentIndex += data.Length;
        }
    }

    public enum ClientPacket //Client packet opcodes
    {
        Login = 0,
        Blockchange = 5,
        MoveRotate = 8,
        Message = 13
    }

    public enum ServerPacket
    {
        Login = 0,
        Ping = 1,
        MapBegin = 2,
        MapChunk = 3,
        MapFinal = 4,
        Blockchange = 6,
        SpawnEntity = 7,
        MoveRotate = 8,
        PlayerDie = 12,
        Message = 13,
        Kick = 14,
        RankUpdate = 15
    }


}
