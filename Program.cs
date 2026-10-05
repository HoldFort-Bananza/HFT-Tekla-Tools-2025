using System;
using System.Windows.Forms;

namespace HFTTeklaTools
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            // Tryb konsolowy do diagnostyki bez GUI - DiagRunner każdego
            // narzędzia, wszystkie tylko do odczytu (RO: dryRun na sztywno).
            // Przełączniki RO bez zmian względem dawnego programu; jedyny
            // konflikt nazw - --diag-active był w RO i w Style Changer - więc
            // wersja Style Changer dostała przedrostek --style-.
            // Przełącznik z brakującym "[Mark]" (albo nieznany) uruchamia GUI.
            string mark = args.Length > 1 ? args[1] : null;
            switch (args.Length > 0 ? args[0] : null)
            {
                case "--diag-active": RoAxisDimensionRemover.DiagRunner.RunOnActiveDrawing(); return;
                case "--diag-notch": RoAxisDimensionRemover.DiagRunner.RunNotchDiag(); return;
                case "--diag-dimension-style": RoAxisDimensionRemover.DiagRunner.RunDimensionStyleDiag(); return;
                case "--diag-find-candidates": RoAxisDimensionRemover.DiagRunner.RunFindCandidatesDiag(mark); return;
                case "--diag-mark" when mark != null: RoAxisDimensionRemover.DiagRunner.RunOnMark(mark); return;
                case "--diag-notch-match" when mark != null: RoAxisDimensionRemover.DiagRunner.RunNotchMatchDiag(mark); return;
                case "--diag-notch-insert-dryrun" when mark != null: RoAxisDimensionRemover.DiagRunner.RunNotchInsertDryRun(mark); return;
                case "--diag-notch-fill-dryrun" when mark != null: RoAxisDimensionRemover.DiagRunner.RunNotchFillDryRun(mark); return;
                case "--diag-notch-raw" when mark != null: RoAxisDimensionRemover.DiagRunner.RunNotchRawDiag(mark); return;
                case "--diag-view-objects" when mark != null: RoAxisDimensionRemover.DiagRunner.RunViewObjectsDiag(mark); return;
                case "--diag-view-bounds" when mark != null: RoAxisDimensionRemover.DiagRunner.RunViewBoundsDiag(mark); return;
                case "--diag-connection" when mark != null: RoAxisDimensionRemover.DiagRunner.RunConnectionDiag(mark); return;

                case "--style-diag-active": StyleChanger.DiagRunner.RunOnActiveDrawing(); return;
                case "--test-other-drawing": StyleChanger.DiagRunner.TestOnOtherDrawing(); return;
                case "--dump-style" when mark != null: StyleChanger.DiagRunner.DumpStyleFile(mark); return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
