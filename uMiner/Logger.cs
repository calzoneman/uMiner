/**
 * uMiner - A lightweight custom Minecraft Classic server written in C#
 * Copyright 2010 Calvin "calzoneman" Montgomery
 * 
 * Licensed under the Creative Commons Attribution-ShareAlike 3.0 Unported License
 * (see http://creativecommons.org/licenses/by-sa/3.0/, or LICENSE.txt for a full license
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace uMiner
{
    public class Logger
    {
        public string filename = "server.log";
        public List<string> buffer = new List<string>();

        public Logger()
        {
            log("Logger initialized", LogType.Info);
        }

        public void log(string data, LogType type)
        {
            string display = "[" + DateTime.Now.ToString() + "]" + "[" + type.ToString() + "] " + data;
            buffer.Add(display);
            logToConsole(data, type);
            try
            {
                StreamWriter fWriter = new StreamWriter(File.Open(filename, FileMode.Append));
                fWriter.WriteLine(display);
                fWriter.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("[" + DateTime.Now.ToString() + "]" + "[ERROR] Failed to write to logfile!");
                Console.WriteLine("Exception: " + e.ToString());
            }
            if (buffer.Count > 64)
            {
                buffer.Clear();
            }
        }

        public void log(string data)
        {
            log(data, LogType.Info);
        }

        public void log(Exception e)
        {
            string data = e.ToString();
            log(data, LogType.Error);
        }

        public void logToConsole(string data, LogType type)
        {
            Console.Write("[" + DateTime.Now.ToString() + "]");
            switch (type)
            {
                case LogType.Info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[" + type.ToString() + "] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[" + type.ToString() + "] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[" + type.ToString() + "] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                default:
                    Console.Write("[Unknown Type]");
                    break;
            }
            Console.WriteLine(data);
        }


        public enum LogType
        {
            Info,
            Warning,
            Error
        }
    }
}
