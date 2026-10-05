using System;
using System.Drawing;
using System.Windows.Forms;
using RadiusDimensionMover;
using RoAxisDimensionRemover;
using StyleChanger;

namespace HFTTeklaTools
{
    /// <summary>
    /// Wspólne okno trzech narzędzi (operator, 2026-10-05: jedna aplikacja z
    /// zakładkami zamiast trzech osobnych programów). Każda zakładka to
    /// dawny MainForm swojego programu, przerobiony na UserControl bez zmian
    /// w logice. Tutaj tylko to, co było w każdym z nich osobno: pasek
    /// nowszej wersji i odświeżanie stanu przy fokusie okna.
    /// </summary>
    public class MainForm : Form
    {
        private readonly TabControl _tabs;
        private readonly RoAxisTab _roTab = new RoAxisTab { Dock = DockStyle.Fill };
        private readonly RadiusTab _radiusTab = new RadiusTab { Dock = DockStyle.Fill };
        private readonly StyleTab _styleTab = new StyleTab { Dock = DockStyle.Fill };

        // Pasek z informacją o nowszej wersji, UKRYTY dopóki UpdateCheck nie
        // znajdzie nowszej wersji - wzorzec z dawnych programów. Dock=Top,
        // więc ukryty nie zajmuje miejsca i nie trzeba przesuwać kontrolek.
        private readonly LinkLabel _updateBanner;

        public MainForm()
        {
            Text = "HFT Tekla Tools – Tekla 2025";
            // Najwyższa zakładka (RO: 500 x 431) + nagłówki zakładek i ramka okna.
            ClientSize = new Size(520, 472);
            StartPosition = FormStartPosition.CenterScreen;

            _tabs = new TabControl { Dock = DockStyle.Fill };
            AddTab("RO – wymiary do osi", _roTab);
            AddTab("Wymiary R", _radiusTab);
            AddTab("Styl widoku", _styleTab);
            _tabs.SelectedIndexChanged += (s, e) => RefreshSelectedTab();

            _updateBanner = new LinkLabel
            {
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(12, 6, 0, 0),
                Visible = false,
                LinkColor = Color.FromArgb(0, 102, 204),
                Font = new Font(Font, FontStyle.Bold)
            };
            _updateBanner.LinkClicked += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(UpdateCheck.ReleasesPage);
                }
                catch (Exception ex)
                {
                    _roTab.Log("Nie udało się otworzyć strony z wydaniami: " + ex.Message);
                }
            };

            // Kolejność ma znaczenie: dokowanie idzie od ostatnio dodanej, więc
            // pasek (Top) musi być dodany po zakładkach (Fill).
            Controls.Add(_tabs);
            Controls.Add(_updateBanner);

            // Fokus okna to moment odświeżenia podpisu stanu - tak jak w
            // dawnych programach. Tylko widoczna zakładka, bo każde odświeżenie
            // to zapytania do Tekli.
            Activated += (s, e) => RefreshSelectedTab();

            // W tle, nie blokuje startu, milczy gdy wszystko aktualne. Wynik do
            // logu pierwszej zakładki, żeby było widać, że sprawdzenie się odbyło.
            UpdateCheck.StartInBackground(
                version => UiInvoke(() => ShowUpdateBanner(version)),
                message => UiInvoke(() => _roTab.Log(message)));
        }

        private void AddTab(string title, Control content)
        {
            var page = new TabPage(title);
            page.Controls.Add(content);
            _tabs.TabPages.Add(page);
        }

        private void RefreshSelectedTab()
        {
            switch (_tabs.SelectedTab?.Controls[0])
            {
                case RoAxisTab ro: ro.RefreshState(); break;
                case RadiusTab radius: radius.RefreshState(); break;
                case StyleTab style: style.RefreshState(); break;
            }
        }

        private void ShowUpdateBanner(string version)
        {
            if (_updateBanner.Visible)
            {
                return; // już pokazany
            }

            _updateBanner.Text = "Dostępna nowsza wersja " + version + " - kliknij, aby pobrać";
            _updateBanner.Visible = true;
            Height += _updateBanner.Height;
        }

        /// <summary>
        /// UpdateCheck woła callbacki z wątku roboczego (Task.Run), więc do
        /// kontrolek wracamy przez BeginInvoke. Wyjątki tłumione świadomie -
        /// okno mogło już zniknąć, a to nie powód, żeby przerywać program.
        /// </summary>
        private void UiInvoke(Action action)
        {
            try
            {
                if (IsDisposed || Disposing)
                {
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch
            {
            }
        }
    }
}
