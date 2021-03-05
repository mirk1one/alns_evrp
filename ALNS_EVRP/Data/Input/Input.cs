using System.Collections.Generic;
using System.Text;

namespace ALNS_EVRP.Data
{
    public class Input
    {
        /// <summary>
        /// Nodes of graph
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// Speeds of electric vehicles
        /// </summary>
        public List<Speed> Speeds { get; set; }

        /// <summary>
        /// Battery capacity
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Vehicle load capacity
        /// </summary>
        public float C { get; set; }

        /// <summary>
        /// Battery consumption rate for power kW/km*kg
        /// </summary>
        public float Lcr { get; set; }

        /// <summary>
        /// Battery recharging rate min/kW
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// Number of vehicles
        /// </summary>
        public int NumV { get; set; }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine("Nodes");
            int i = 0;
            Nodes.ForEach(n => strBuilder.AppendLine($"Node {i++} -> {n}"));
            i = 0;
            Speeds.ForEach(s => strBuilder.AppendLine($"Speed {i++} -> {s}"));
            strBuilder.AppendLine($"Q = {Q}");
            strBuilder.AppendLine($"C = {C}");
            strBuilder.AppendLine($"lcr = {Lcr}");
            strBuilder.AppendLine($"g = {G}");
            strBuilder.AppendLine($"NumV = {NumV}");
            return strBuilder.ToString();
        }
    }
}
