using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Drawing;

namespace RoAxisDimensionRemover
{
    /// <summary>
    /// Zakładka „RO – wymiary do osi”: jeden przycisk (usuń wymiary do osi +
    /// wstaw wymiar wcięcia), podpis stanu, log. Logika w
    /// RoAxisDimensionService i NotchPilot. Bez nasłuchu zdarzeń Tekli - stan
    /// odświeża się przy fokusie okna (MainForm woła RefreshState).
    /// Do 2026-10-05 osobny program RO Axis Dimension Remover (MainForm);
    /// pasek aktualizacji przeniesiony do wspólnego okna.
    /// </summary>
    public class RoAxisTab : UserControl
    {
        private bool _busy;

        // Operator (2026-09-30): log ma zostać przez wszystkie operacje na
        // jednym rysunku (kilka kliknięć z rzędu), a czyścić się
        // dopiero przy przejściu na inny. Tekla nie daje zdarzenia zamknięcia
        // rysunku, więc porównujemy Mark aktywnego rysunku przy każdym kliku.
        private string _logDrawingMark;

        private Button _cleanupButton;
        private TextBox _logBox;
        private Label _statusLabel;

        public RoAxisTab()
        {
            // Rozmiar z dawnego okna (kontrolki 15 + 470 + 15, log kończy się
            // 15 px nad dołem), ustawiony PRZED dodaniem kontrolek: kotwica
            // logu liczy odstęp od dołu względem tego rozmiaru, a domyślne
            // 150x150 UserControl dałoby log wystający poza zakładkę.
            Size = new System.Drawing.Size(500, 431);
            _cleanupButton = new Button
            {
                Text = "Posprzątaj wymiary na aktywnym rysunku (profile RO)\nusuwa wymiary do osi, wstawia wymiary wcięcia",
                Left = 15,
                Top = 15,
                Width = 470,
                Height = 75
            };
            _cleanupButton.Click += CleanupButton_Click;

            _statusLabel = new Label
            {
                Left = 15,
                Top = 96,
                Width = 470,
                Height = 20,
                ForeColor = Color.DarkSlateGray
            };

            _logBox = new TextBox
            {
                Left = 15,
                Top = 121,
                Width = 470,
                Height = 295,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 9)
            };

            Controls.Add(_cleanupButton);
            Controls.Add(_statusLabel);
            Controls.Add(_logBox);

            Log($"===== Start sesji {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            RefreshState();
        }

        public void RefreshState()
        {
            if (_busy) return;
            try
            {
                var dh = new DrawingHandler();
                if (!dh.GetConnectionStatus())
                {
                    _statusLabel.Text = "Brak połączenia z Teklą.";
                    return;
                }
                var drawing = dh.GetActiveDrawing();
                _statusLabel.Text = drawing != null
                    ? $"Aktywny rysunek: {drawing.Mark} / {drawing.Name}"
                    : "Brak otwartego rysunku.";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Brak kontaktu z Teklą (" + ex.GetType().Name + ").";
            }
        }

        /// <summary>
        /// Jeden przycisk (operator, 2026-10-05): „Usuń” na wszystkich widokach
        /// arkusza, potem „Wstaw” na całym rysunku. Dawniej dwa osobne
        /// przyciski i wybór widoku kliknięciem; Shift + klik (wszystkie widoki)
        /// stał się domyślny, bo operator i tak go używał. Reguły bez zmian -
        /// to ten sam cykl, który przechodził bramę na żywo (Usuń z Shift →
        /// Wstaw). Wybór pojedynczego widoku (przypadek [35020]) zniknął razem
        /// z pickerem - na takim rysunku Ctrl+Z cofa widok po widoku.
        /// </summary>
        private void CleanupButton_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            _busy = true;
            _cleanupButton.Enabled = false;

            try
            {
                var dh = new DrawingHandler();
                if (!dh.GetConnectionStatus())
                {
                    _statusLabel.Text = "Brak połączenia z Teklą.";
                    return;
                }
                var drawing = dh.GetActiveDrawing();
                if (drawing == null)
                {
                    _statusLabel.Text = "Brak otwartego rysunku.";
                    return;
                }
                // dryRun: false - brama bezpieczeństwa reguły v6 przeszła
                // 2026-09-23 (operator na żywym [35021] i [3.5013]: „rysunek
                // opisuje wszystko” i „tak” na realne kasowanie). Historia:
                // AGENTS.md i wiki 10-Dziennik-2026-09.
                const bool dryRun = false;

                BeginLog(drawing, "PORZĄDKOWANIE WYMIARÓW");
                // Każdy widok zatwierdza się osobno, więc Ctrl+Z cofa widok po
                // widoku, a wstawienie to kolejne kroki cofania.
                int removed = 0, viewNumber = 0;
                foreach (var sheetView in DiagRunner.SheetViews(drawing).ToList())
                {
                    viewNumber++;
                    Log($"--- widok {viewNumber} ({sheetView.GetType().Name}) ---");
                    removed += RoAxisDimensionService.RemoveAxisDimensions(drawing, sheetView, Log, dryRun);
                }

                Log("--- wymiary wcięcia ---");
                int inserted = NotchPilot.InsertMissing(drawing, Log);

                _statusLabel.Text = $"Gotowe. Usunięto {removed}, wstawiono {inserted}. Sprawdź w Tekli (Ctrl+Z cofa).";
                if (removed + inserted > 0) TeklaWindowFocus.BringToFront(Log);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = "Błąd – zobacz log.";
                Log("BŁĄD: " + ex.Message);
                Log(ex.StackTrace);
            }
            finally
            {
                _busy = false;
                _cleanupButton.Enabled = true;
            }
        }

        // Obok .exe, NIE w katalogu tymczasowym sesji Claude Code - ten
        // znika razem z sesją. Jeden plik na uruchomienie. Przyrostek „_ro”,
        // bo zakładka wymiarów R pisze do tego samego katalogu i przy starcie
        // w tej samej sekundzie oba pliki miałyby tę samą nazwę.
        private static readonly string DiagLogPath = System.IO.Path.Combine(
            Application.StartupPath, "logs", $"session_{DateTime.Now:yyyyMMdd_HHmmss}_ro.log");

        private void BeginLog(Drawing drawing, string operation)
        {
            if (drawing.Mark != _logDrawingMark)
            {
                _logBox.Clear();
                _logDrawingMark = drawing.Mark;
            }
            else
            {
                Log("");
            }
            Log($"===== {DateTime.Now:HH:mm:ss} {operation} ({drawing.Mark}) =====");
        }

        // internal: MainForm wpisuje tu wynik sprawdzenia aktualizacji.
        internal void Log(string message)
        {
            _logBox.AppendText(message + Environment.NewLine);
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DiagLogPath));
                System.IO.File.AppendAllText(DiagLogPath, message + Environment.NewLine);
            }
            catch { }
        }
    }
}
