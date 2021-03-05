using ALNS_EVRP.Data;
using ALNS_EVRP.Model;
using Aspose.Cells;
using System;
using System.Collections.Generic;

namespace ALNS_EVRP.Utils
{
    public static class ExcelUtils
    {
        public static void CreateSolutionExcel(Solution initSol, Solution bestSol, List<Solution> allSol)
        {
            // Instantiate a Workbook object that represents Excel file.
            Workbook wb = new Workbook();
            wb.Worksheets.Add();
            wb.Worksheets.Add();

            // Obtain the reference of the newly added worksheet by passing its sheet index.
            Worksheet worksheet = wb.Worksheets[0];
            worksheet.Name = "Init sol";

            // Write best solution on first tab
            WriteBestSolution(worksheet, initSol);

            // Obtain the reference of the newly added worksheet by passing its sheet index.
            Worksheet worksheet2 = wb.Worksheets[1];
            worksheet2.Name = "ALNS sol";

            // Write best solution on first tab
            WriteBestSolution(worksheet2, bestSol);

            // Obtain the reference of the newly added worksheet by passing its sheet index.
            Worksheet worksheet3 = wb.Worksheets[2];
            worksheet3.Name = "ALNS data";

            WriteALNSData(worksheet3, bestSol, allSol);

            // Save the Excel file.
            wb.Save($"ALNS_EVPR_Result_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.xlsx", SaveFormat.Xlsx);

        }

        private static void WriteBestSolution(Worksheet worksheet, Solution sol)
        {
            // Add dummy values to cells.
            CellsFactory cellsFactory = new CellsFactory();
            Style labelStyle = cellsFactory.CreateStyle();
            labelStyle.Pattern = BackgroundType.Solid;
            labelStyle.ForegroundColor = System.Drawing.Color.LightGray;
            labelStyle.Font.IsBold = true;
            worksheet.Cells["A1"].PutValue("Vehicle");
            worksheet.Cells["A1"].SetStyle(labelStyle);
            worksheet.Cells["B1"].PutValue("Orig");
            worksheet.Cells["B1"].SetStyle(labelStyle);
            worksheet.Cells["C1"].PutValue("TypeO");
            worksheet.Cells["C1"].SetStyle(labelStyle);
            worksheet.Cells["D1"].PutValue("Dest");
            worksheet.Cells["D1"].SetStyle(labelStyle);
            worksheet.Cells["E1"].PutValue("TypeD");
            worksheet.Cells["E1"].SetStyle(labelStyle);
            worksheet.Cells["F1"].PutValue("DemandD");
            worksheet.Cells["F1"].SetStyle(labelStyle);
            worksheet.Cells["G1"].PutValue("ServTime");
            worksheet.Cells["G1"].SetStyle(labelStyle);
            worksheet.Cells["H1"].PutValue("Dist");
            worksheet.Cells["H1"].SetStyle(labelStyle);
            worksheet.Cells["I1"].PutValue("Speed");
            worksheet.Cells["I1"].SetStyle(labelStyle);
            worksheet.Cells["J1"].PutValue("ReadyTime");
            worksheet.Cells["J1"].SetStyle(labelStyle);
            worksheet.Cells["K1"].PutValue("DueDate");
            worksheet.Cells["K1"].SetStyle(labelStyle);
            worksheet.Cells["L1"].PutValue("DepTime");
            worksheet.Cells["L1"].SetStyle(labelStyle);
            worksheet.Cells["M1"].PutValue("ArrTimeD");
            worksheet.Cells["M1"].SetStyle(labelStyle);
            worksheet.Cells["N1"].PutValue("DepBatt");
            worksheet.Cells["N1"].SetStyle(labelStyle);
            worksheet.Cells["O1"].PutValue("ArrBattD");
            worksheet.Cells["O1"].SetStyle(labelStyle);
            worksheet.Cells["P1"].PutValue("DepLoad");
            worksheet.Cells["P1"].SetStyle(labelStyle);
            worksheet.Cells["Q1"].PutValue("ArrLoadD");
            worksheet.Cells["Q1"].SetStyle(labelStyle);
            worksheet.Cells["R1"].PutValue("RechBatt");
            worksheet.Cells["R1"].SetStyle(labelStyle);

            int row = 2;
            for (int v = 0; v < sol.Routes.Count; v++)
            {
                for (int n = 1; n < sol.Routes[v].Nodes.Count; n++)
                {
                    Style valueStyle = cellsFactory.CreateStyle();
                    valueStyle.Pattern = BackgroundType.Solid;
                    valueStyle.ForegroundColor = GetColor(v);
                    worksheet.Cells[$"A{row}"].PutValue(v + 1);
                    worksheet.Cells[$"A{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"B{row}"].PutValue(sol.Routes[v].Nodes[n - 1].ID);
                    worksheet.Cells[$"B{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"C{row}"].PutValue(sol.Routes[v].Nodes[n - 1].Type.ToString());
                    worksheet.Cells[$"C{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"D{row}"].PutValue(sol.Routes[v].Nodes[n].ID);
                    worksheet.Cells[$"D{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"E{row}"].PutValue(sol.Routes[v].Nodes[n].Type.ToString());
                    worksheet.Cells[$"E{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"F{row}"].PutValue(sol.Routes[v].Nodes[n].Demand);
                    worksheet.Cells[$"F{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"G{row}"].PutValue(sol.Routes[v].Nodes[n].ServiceTime);
                    worksheet.Cells[$"G{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"H{row}"].PutValue(sol.Routes[v].Nodes[n].Dist);
                    worksheet.Cells[$"H{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"I{row}"].PutValue(sol.Routes[v].Nodes[n].SpeedArrive.Value);
                    worksheet.Cells[$"I{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"J{row}"].PutValue(sol.Routes[v].Nodes[n].ReadyTime);
                    worksheet.Cells[$"J{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"K{row}"].PutValue(sol.Routes[v].Nodes[n].DueDate);
                    worksheet.Cells[$"K{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"L{row}"].PutValue(sol.Routes[v].Nodes[n - 1].EndServiceTime);
                    worksheet.Cells[$"L{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"M{row}"].PutValue(sol.Routes[v].Nodes[n].Time);
                    worksheet.Cells[$"M{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"N{row}"].PutValue(sol.Routes[v].Nodes[n - 1].BatteryRemaining);
                    worksheet.Cells[$"N{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"O{row}"].PutValue(sol.Routes[v].Nodes[n].BatteryRemaining);
                    worksheet.Cells[$"O{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"P{row}"].PutValue(sol.Routes[v].Nodes[n - 1].Load);
                    worksheet.Cells[$"P{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"Q{row}"].PutValue(sol.Routes[v].Nodes[n].Load);
                    worksheet.Cells[$"Q{row}"].SetStyle(valueStyle);
                    worksheet.Cells[$"R{row}"].PutValue(sol.Routes[v].Nodes[n - 1].BatteryRecharged);
                    worksheet.Cells[$"R{row}"].SetStyle(valueStyle);
                    row++;
                }
            }
        }

        private static void WriteALNSData(Worksheet worksheet, Solution bestSol, List<Solution> allSol)
        {
            // Add dummy values to cells.
            CellsFactory cellsFactory = new CellsFactory();

            Style labelStyle = cellsFactory.CreateStyle();
            labelStyle.Pattern = BackgroundType.Solid;
            labelStyle.ForegroundColor = System.Drawing.Color.Cyan;
            labelStyle.Font.IsBold = true;

            Style bestSolStyle = cellsFactory.CreateStyle();
            bestSolStyle.Pattern = BackgroundType.Solid;
            bestSolStyle.ForegroundColor = System.Drawing.Color.LightGreen;

            Style localSolStyle = cellsFactory.CreateStyle();
            localSolStyle.Pattern = BackgroundType.Solid;
            localSolStyle.ForegroundColor = System.Drawing.Color.Orange;

            Style worseSolStyle = cellsFactory.CreateStyle();
            worseSolStyle.Pattern = BackgroundType.Solid;
            worseSolStyle.ForegroundColor = System.Drawing.Color.LightGray;

            Style raiseTempStyle = cellsFactory.CreateStyle();
            raiseTempStyle.Pattern = BackgroundType.Solid;
            raiseTempStyle.ForegroundColor = System.Drawing.Color.Red;

            Style bestCostStyle = cellsFactory.CreateStyle();
            bestCostStyle.Pattern = BackgroundType.Solid;
            bestCostStyle.ForegroundColor = System.Drawing.Color.GreenYellow;
            bestCostStyle.Font.IsBold = true;

            worksheet.Cells["A1"].PutValue("Dist weight");
            worksheet.Cells["A1"].SetStyle(labelStyle);
            worksheet.Cells["A2"].PutValue("Time weight");
            worksheet.Cells["A2"].SetStyle(labelStyle);
            worksheet.Cells["A3"].PutValue("Batt weight");
            worksheet.Cells["A3"].SetStyle(labelStyle);
            worksheet.Cells["A4"].PutValue("Req bank weight");
            worksheet.Cells["A4"].SetStyle(labelStyle);
            worksheet.Cells["A5"].PutValue("Time viol weight");
            worksheet.Cells["A5"].SetStyle(labelStyle);
            worksheet.Cells["A6"].PutValue("Batt viol weight");
            worksheet.Cells["A6"].SetStyle(labelStyle);
            worksheet.Cells["A7"].PutValue("Load viol weight");
            worksheet.Cells["A7"].SetStyle(labelStyle);
            worksheet.Cells["A8"].PutValue("Start temp");
            worksheet.Cells["A8"].SetStyle(labelStyle);
            worksheet.Cells["A9"].PutValue("End temp");
            worksheet.Cells["A9"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");
            worksheet.Cells["A10"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");
            worksheet.Cells["A10"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");
            worksheet.Cells["A10"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");
            worksheet.Cells["A10"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");
            worksheet.Cells["A10"].SetStyle(labelStyle);
            worksheet.Cells["A10"].PutValue("Tot iters");

            worksheet.Cells["B1"].PutValue(bestSol.Cost.Dist.Total);
            worksheet.Cells["B2"].PutValue(bestSol.Cost.Time.Total);
            worksheet.Cells["B3"].PutValue(bestSol.Cost.Battery.Total);
            worksheet.Cells["B4"].PutValue(bestSol.Cost.RequestBank.Total);
            worksheet.Cells["B5"].PutValue(bestSol.Cost.TimeViolation.Total);
            worksheet.Cells["B6"].PutValue(bestSol.Cost.BatteryViolation.Total);
            worksheet.Cells["B7"].PutValue(bestSol.Cost.LoadViolation.Total);
            worksheet.Cells["B8"].PutValue(allSol[0].Temperature);
            worksheet.Cells["B9"].PutValue(allSol[^1].Temperature);
            worksheet.Cells["B10"].PutValue(allSol[^1].Iteration + 1);

            worksheet.Cells["A14"].PutValue("Iter");
            worksheet.Cells["A14"].SetStyle(labelStyle);
            worksheet.Cells["B14"].PutValue("Temp");
            worksheet.Cells["B14"].SetStyle(labelStyle);
            worksheet.Cells["C14"].PutValue("Type");
            worksheet.Cells["C14"].SetStyle(labelStyle);
            worksheet.Cells["D14"].PutValue("Cost sol");
            worksheet.Cells["D14"].SetStyle(labelStyle);
            worksheet.Cells["E14"].PutValue("Cost dist");
            worksheet.Cells["E14"].SetStyle(labelStyle);
            worksheet.Cells["F14"].PutValue("Cost time");
            worksheet.Cells["F14"].SetStyle(labelStyle);
            worksheet.Cells["G14"].PutValue("Cost batt");
            worksheet.Cells["G14"].SetStyle(labelStyle);
            worksheet.Cells["H14"].PutValue("Cost cust not served");
            worksheet.Cells["H14"].SetStyle(labelStyle);
            worksheet.Cells["I14"].PutValue("Cost viol time");
            worksheet.Cells["I14"].SetStyle(labelStyle);
            worksheet.Cells["J14"].PutValue("Cost viol batt");
            worksheet.Cells["J14"].SetStyle(labelStyle);
            worksheet.Cells["K14"].PutValue("Cost viol load");
            worksheet.Cells["K14"].SetStyle(labelStyle);

            int row = 15;
            for (int v = 0; v < allSol.Count; v++)
            {
                worksheet.Cells[$"A{row}"].PutValue(allSol[v].Iteration);
                worksheet.Cells[$"B{row}"].PutValue(allSol[v].Temperature);
                worksheet.Cells[$"C{row}"].PutValue(allSol[v].Type.ToString());
                worksheet.Cells[$"D{row}"].PutValue(allSol[v].Cost.Total);
                worksheet.Cells[$"E{row}"].PutValue(allSol[v].Cost.Dist.Total);
                worksheet.Cells[$"F{row}"].PutValue(allSol[v].Cost.Time.Total);
                worksheet.Cells[$"G{row}"].PutValue(allSol[v].Cost.Battery.Total);
                worksheet.Cells[$"H{row}"].PutValue(allSol[v].Cost.RequestBank.Total);
                worksheet.Cells[$"I{row}"].PutValue(allSol[v].Cost.TimeViolation.Total);
                worksheet.Cells[$"J{row}"].PutValue(allSol[v].Cost.BatteryViolation.Total);
                worksheet.Cells[$"K{row}"].PutValue(allSol[v].Cost.LoadViolation.Total);


                if (allSol[v].Iteration == bestSol.Iteration)
                {
                    worksheet.Cells[$"A{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"B{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"C{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"D{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"E{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"F{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"G{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"H{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"I{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"J{row}"].SetStyle(bestCostStyle);
                    worksheet.Cells[$"K{row}"].SetStyle(bestCostStyle);
                }
                else if (allSol[v].Type == TypeSolution.BestSol)
                {
                    worksheet.Cells[$"A{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"B{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"C{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"D{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"E{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"F{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"G{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"H{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"I{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"J{row}"].SetStyle(bestSolStyle);
                    worksheet.Cells[$"K{row}"].SetStyle(bestSolStyle);
                }
                else if(allSol[v].Type == TypeSolution.LocalSol)
                {
                    worksheet.Cells[$"A{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"B{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"C{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"D{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"E{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"F{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"G{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"H{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"I{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"J{row}"].SetStyle(localSolStyle);
                    worksheet.Cells[$"K{row}"].SetStyle(localSolStyle);
                }
                else if(allSol[v].Temperature > allSol[v-1].Temperature)
                {
                    worksheet.Cells[$"A{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"B{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"C{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"D{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"E{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"F{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"G{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"H{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"I{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"J{row}"].SetStyle(raiseTempStyle);
                    worksheet.Cells[$"K{row}"].SetStyle(raiseTempStyle);
                }
                else if (allSol[v].Type == TypeSolution.WorseSol)
                {
                    worksheet.Cells[$"A{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"B{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"C{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"D{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"E{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"F{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"G{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"H{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"I{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"J{row}"].SetStyle(worseSolStyle);
                    worksheet.Cells[$"K{row}"].SetStyle(worseSolStyle);
                }

                row++;
            }
        }

        public static System.Drawing.Color GetColor(int vehicle)
        {
            switch (vehicle)
            {
                case 0:
                    return System.Drawing.Color.LightBlue;
                case 1:
                    return System.Drawing.Color.LightGreen;
                case 2:
                    return System.Drawing.Color.OrangeRed;
                case 3:
                    return System.Drawing.Color.Yellow;
                case 4:
                    return System.Drawing.Color.Orange;
                case 5:
                    return System.Drawing.Color.Pink;
                case 6:
                    return System.Drawing.Color.RosyBrown;
                case 7:
                    return System.Drawing.Color.Gold;
                case 8:
                    return System.Drawing.Color.Fuchsia;
                default:
                    return System.Drawing.Color.Gray;
            }
        }
    }
}
