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
using System.Text;

namespace uMiner
{
    public class Logger
    {
        public string filename = "server.log";
        public List<string> buffer = new List<string>();
        public bool logWithColor = false;

        public object mutex = new object();

        public Logger()
        {
            log("Logger initialized", LogType.Info);
        }

        public void log(string data, LogType type)
        {
            lock (mutex)
            {
                string display = "[" + DateTime.Now.ToString() + "]" + "[" + type.ToString() + "] " + StripColor(data);
                buffer.Add(display);
                logToConsole(data, type);
                try
                {
                    StreamWriter fWriter = new StreamWriter(File.Open(filename, FileMode.Append));
                    fWriter.WriteLine(display);
                    fWriter.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString() + "]" + "[ERROR] Failed to write to logfile!");
                    Console.WriteLine("Exception: " + e.ToString());
                }
                if (buffer.Count > 64)
                {
                    buffer.Clear();
                }
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
                case LogType.Chat:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[" + type.ToString() + "] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogType.CCmd:
                    Console.ForegroundColor = ConsoleColor.Cyan;
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
            if (logWithColor && "Chat|CCmd".Contains(type.ToString()))
            {
                bool defaultToYellow = false;
                if (data.Length > 8 && (data.Trim().Substring(0, 8).Equals("(Global)") || type.ToString().Equals("CCmd")))
                {
                    defaultToYellow = true;
                }
                WriteLineColor(data, defaultToYellow);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
            }
            else
            {
                Console.WriteLine(StripColor(data));
            }
        }

        public static void WriteLineColor(string line, bool defaultYellow)
        {
            int i = 0;
            Console.ForegroundColor = ConsoleColor.White;
            if (defaultYellow) { Console.ForegroundColor = ConsoleColor.Yellow; }
            while (i < line.Length)
            {

                if (line[i] == '&')
                {
                    int code = int.Parse(string.Empty + line[i + 1], System.Globalization.NumberStyles.HexNumber);
                    Console.BackgroundColor = ConsoleColor.Black;
                    switch (code)
                    {
                        case 0:
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Gray;
                            break;
                        case 1:
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            Console.BackgroundColor = ConsoleColor.Gray;
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 4:
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 5:
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 6:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 7:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 8:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 9:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 10:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 11:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 12:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 13:
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 14:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        case 15:
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;
                        default:
                            break;
                    }
                }

                if (!(line[i] == '&' || line[i] == 10 || (i - 1 >= 0 && line[i - 1] == '&')))
                {
                    Console.Write(line[i]);
                }
                if (line[i] == 10)
                {
                    Console.WriteLine();
                }
                i++;
            }
            Console.WriteLine();
        }

        public string StripColor(string input)
        {
            int i = 0;
            StringBuilder output = new StringBuilder();
            while (i < input.Length)
            {
                if (!(input[i] == '&' && i + 1 < input.Length && "0123456789abcdef".Contains(input[i + 1].ToString())) && !("0123456789abcdef".Contains(input[i].ToString()) && i - 1 >= 0 && input[i - 1] == '&'))
                {
                    output.Append(input[i]);
                }
                i++;
            }
            return output.ToString();
        }



        public enum LogType
        {
            Info,
            Warning,
            Error,
            Chat,
            CCmd
        }
    }
}
