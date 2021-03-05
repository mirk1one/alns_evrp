using ALNS_EVRP.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ALNS_EVRP.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// From a file format for EVRP, read all input data
        /// More examples are in File directory under ECRP_Tests test project
        /// </summary>
        /// <param name="lines">All lines read from file</param>
        /// <returns>Class of input file read</returns>
        public static Input GetInputFromFileStrings(string[] lines)
        {
            int l = 0;
            Input input = new Input();
            string[] labels = null;
            string[] values = null;

            try
            {
                // Read nodes
                input.Nodes = new List<Node>();
                labels = lines[l].ToLower().Replace("\r", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                l++;
                while (!string.IsNullOrEmpty(lines[l]) && lines[l] != "\r")
                {
                    values = lines[l].Replace("\r", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Node node = new Node();
                    for (int i = 0; i < values.Length; i++)
                    {
                        string value = values[i];
                        switch (labels[i])
                        {
                            case "stringid":
                                node.ID = value;
                                break;
                            case "type":
                                node.Type = (TypeNode)char.Parse(value);
                                break;
                            case "x":
                                node.X = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "y":
                                node.Y = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "demand":
                                node.Demand = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "readytime":
                                node.ReadyTime = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "duedate":
                                node.DueDate = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                            case "servicetime":
                                node.ServiceTime = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    input.Nodes.Add(node);
                    l++;
                }

                l++;

                // Read speeds
                input.Speeds = new List<Speed>();
                labels = lines[l].ToLower().Replace("\r", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                l++;
                while (!string.IsNullOrEmpty(lines[l]) && lines[l] != "\r")
                {
                    values = lines[l].Replace("\r", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Speed speed = new Speed();
                    for (int i = 0; i < values.Length; i++)
                    {
                        string value = values[i];
                        switch (labels[i])
                        {
                            case "speed":
                                speed.Value = int.Parse(value);
                                break;
                            case "batteryconsumptionrateforkm":
                                speed.BatteryConsumptionRateForKm = float.Parse(value, CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    input.Speeds.Add(speed);
                    l++;
                }

                l++;
            }
            catch(FileLoadException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new FileLoadException($"Error problem loading data, labels: {labels} | values: {values}, message: {ex.Message}", ex);
            }

            try
            {
                // Read other parameters
                while (l < lines.Length && !string.IsNullOrEmpty(lines[l]))
                {
                    string[] inputLines = lines[l].Replace("\r", " ").Split(' ');
                    string label = inputLines[0].ToLower();
                    string value = inputLines[2];
                    switch(label)
                    {
                        case "q":
                            input.Q = float.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case "c":
                            input.C = float.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case "lcr":
                            input.Lcr = float.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case "g":
                            input.G = float.Parse(value, CultureInfo.InvariantCulture);
                            break;
                        case "numv":
                            input.NumV = int.Parse(value);
                            break;
                    }
                    l++;
                }
            }
            catch (FileLoadException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Error problem loading data line: {lines[l]}, message: {ex.Message}", ex);
            }

            return input;
        }
    }
}
