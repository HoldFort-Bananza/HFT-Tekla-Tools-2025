# AGENTS.md — HFT Tekla Tools

Plik dla agentów (Claude Code i innych) pracujących w tym repo. Nadrzędny
`..\AGENTS.md` (w `Projekty`: środowisko Tekla Structures 2025, twarde zasady,
konwencje, pułapki API wspólne dla wszystkich projektów) obowiązuje też tutaj
bez powtarzania.

## Jednym zdaniem

Jedna aplikacja WinForms z zakładkami (zewnętrzny `.exe`, łączy się z Teklą
przez Open API), złożona 2026-10-05 na prośbę operatora z trzech wcześniej
osobnych programów — po jednej zakładce na każdy. **To jest teraz główny
projekt**; trzy stare repo to archiwum i baza wiedzy (patrz niżej).

| Zakładka | Dawny program (repo HoldFort-Bananza) | Katalog | Baza wiedzy |
|---|---|---|---|
| RO – wymiary do osi | RO Axis Dimension Remover 0.3.13 + PR #80 (`RO-Axis-Dimension-Remover`) | `RoAxis\` | `RoAxis\AGENTS.md` |
| Wymiary R | Radius Dimension Mover 1.11.0 (`Radius-Dimention-Mover`) | `RadiusMover\` | `RadiusMover\AGENTS.md` |
| Styl widoku | Style Changer 0.1.0 (`Tekla-Style-Changer`) | `StyleChanger\` | `StyleChanger\AGENTS.md` |

**Przed zmianą w zakładce przeczytaj jej `AGENTS.md` w całości.** To pełne,
dosłowne kopie baz wiedzy trzech projektów (decyzja operatora 2026-10-05:
zebrać wszystko tutaj, a nie odsyłać) — historia, pomiary, ślepe uliczki,
brama bezpieczeństwa RO. Pisane były dla starych repo, więc nazwy plików i
procedury w nich trzeba czytać przez tabelę „Co się zmieniło” niżej.

## Skąd jest kod (stan kopii)

Kod skopiowany 1:1 (serwisy, `DiagRunner`, `NotchPilot`,
`TeklaWindowFocus` — bez zmian w logice), przestrzenie nazw zostawione
(`RoAxisDimensionRemover`, `RadiusDimensionMover`, `StyleChanger`), żeby
diff ze starym repo był czytelny:

- RO: `origin/one-button-cleanup` = `5b4132a` (PR #80, jeden przycisk
  „Posprzątaj wymiary”). PR #80 zmergowany 2026-10-05 jako `11d663a` —
  porównanie `5b4132a...11d663a` przez API GitHuba: zero zmienionych
  plików, więc kod tutaj = RO `dev` po merge'u.
- Radius: `main` = `079f32f`.
- Style Changer: `dev` = `44f516f`.

## Co się zmieniło względem trzech programów

| W starej bazie wiedzy | Tutaj |
|---|---|
| `MainForm.cs` danego programu | `RoAxis\RoAxisTab.cs`, `RadiusMover\RadiusTab.cs`, `StyleChanger\StyleTab.cs` (`UserControl`, logika kliknięć bez zmian) |
| `RoAxisDimensionRemover.exe` / `RadiusDimensionMover.exe` / `StyleChanger.exe` | `HFTTeklaTools.exe` |
| `Program.cs` każdego programu | jeden `Program.cs` z przełącznikami obu `DiagRunner` |
| `--diag-active` Style Changer | `--style-diag-active` (konflikt nazwy z RO; przełączniki RO bez zmian) |
| `UpdateCheck.cs` per program, pasek nad przyciskiem | jeden `UpdateCheck.cs` (repo `HFT-Tekla-Tools-2025`), pasek nad zakładkami w `MainForm.cs` |
| `Activated` w każdym oknie | `MainForm` woła `RefreshState()` widocznej zakładki przy fokusie okna i zmianie zakładki |
| Radius: `FormClosing` zdejmuje zdarzenia Tekli | `Disposed` zakładki |
| log RO i Radius: `logs\session_<czas>.log` | RO: `logs\session_<czas>_ro.log`. Radius i Style Changer: tylko w oknie (Radius bez pliku od 2026-10-05, prośba operatora — razem z komunikatem „Log tej sesji zapisywany do pliku”) |
| `installer\` per program, osobne `AppId` | jeden `installer\` z nowym `AppId`, instalacja do `%LOCALAPPDATA%\Programs\HFTTeklaTools\` — **obok** starych programów, nie zamiast |
| `bin\x64\Debug\net48\*.exe` śledzone w gicie | `bin\` ignorowany w całości — przed `ISCC` trzeba zbudować |
| wersje, tagi, gałęzie, release'y starych repo | nie dotyczą tego repo — patrz „Wydawanie” i „Git” niżej |

## Twarde zasady per zakładka (skrót — pełne uzasadnienie w ich AGENTS.md)

- **RO kasuje dane naprawdę.** `dryRun` w `RoAxisTab.CleanupButton_Click`
  jest `false` (brama przeszła 2026-09-23). `RoAxis\DiagRunner.cs` ma
  `dryRun` na sztywno `true` **na zawsze**. Każda zmiana reguły
  kasowania/wstawiania = nowa brama od zera: dry-run → operator patrzy na
  żywy rysunek → pytanie bez podpowiedzi „Czy po tej operacji rysunek nadal
  opisuje wszystko, co musi opisywać?” → dopiero wtedy realny zapis. **Nie
  waliduj reguły jej własnym dry-runem.**
- **Wymiary R:** `Distance` w mm papieru, `ArcPoint*` w jednostkach modelu;
  zabezpieczenie odległości na KOŃCU potoku; po każdej zmianie rozstawiania
  zestaw regresyjny z `RadiusMover\AGENTS.md` („Testowanie”); strona wiki
  `4-Slepe-uliczki` — 18 podejść, które nie działają.
- **Styl widoku:** `Identifier.GUID` obiektów `View` to same zera — do
  identyfikacji widoków `Identifier.ID`. Wynik operacji zawsze do logu
  (`SetResult`), etykieta bywa nadpisywana przez `Activated`.
- Wszędzie: mierz, nie zgaduj; dane tylko z API.

## Struktura

| Plik | Zawartość |
|---|---|
| `Program.cs` | punkt wejścia: GUI albo tryb konsolowy `--diag-*` |
| `MainForm.cs` | okno z zakładkami, pasek nowszej wersji, odświeżanie przy fokusie |
| `UpdateCheck.cs` | w tle sprawdza najnowszy release tego repo; cisza bez internetu |
| `RoAxis\` | `RoAxisTab`, `RoAxisDimensionService`, `NotchPilot`, `DiagRunner`, `TeklaWindowFocus` |
| `RadiusMover\` | `RadiusTab`, `RadiusDimensionService` |
| `StyleChanger\` | `StyleTab`, `StyleChangerService`, `DiagRunner` |
| `installer\` | Inno Setup + `fetch-dependencies.ps1` (biblioteki Tekli z nuget.org po instalacji, nigdy w repo ani w instalatorze — EULA) |

## Tryb konsolowy (bez GUI, tylko odczyt)

Automatyzacja nie klika w przyciski, więc `DiagRunner` obu narzędzi jest
**trwałą** częścią aplikacji. Log na `stdout` w cp1250 — czytać przez
`iconv -c -f cp1250 -t utf-8`.

```
HFTTeklaTools.exe --diag-active                      # RO: aktywny rysunek, wszystkie widoki, reguła „Usuń” w dry-run
HFTTeklaTools.exe --diag-mark "[Mark]"               # RO: otwiera rysunek po Mark, potem jak wyżej
HFTTeklaTools.exe --diag-notch                       # RO: geometria bryły, kandydat na wymiar wcięcia
HFTTeklaTools.exe --diag-dimension-style             # RO: styl/położenie istniejących wymiarów (odczyt z osobnego procesu)
HFTTeklaTools.exe --diag-notch-fill-dryrun "[Mark]"  # RO: co wstawiłby przycisk
HFTTeklaTools.exe --diag-find-candidates [plik]      # RO: skan modelu (> 1 h bez pliku)
HFTTeklaTools.exe --style-diag-active                # Styl: połączenie, zaznaczenie, katalogi i pliki .vi
HFTTeklaTools.exe --dump-style <nazwa>               # Styl: zawartość pliku .vi
HFTTeklaTools.exe --test-other-drawing               # Styl: SZUKA i PRZEŁĄCZA rysunki (w praktyce nieużywane)
```

Pełna lista RO (`--diag-notch-match`, `--diag-notch-insert-dryrun`,
`--diag-notch-raw`, `--diag-view-objects`, `--diag-view-bounds`,
`--diag-connection`) i ich znaczenie: `RoAxis\AGENTS.md`, „Jak testować bez
klikania w GUI”. Tryby z `"[Mark]"` otwierają rysunek na ekranie operatora —
nie puszczać w trakcie jego testu. Nieznany przełącznik albo brak `[Mark]`
uruchamia GUI, które blokuje polecenie do timeoutu.

Sprawdzone 2026-10-05 na żywej Tekli: `--style-diag-active` i
`--diag-dimension-style` łączą się (`GetConnectionStatus(): True`), GUI
startuje, zakładka R rejestruje zdarzenia Tekli.

Tego samego dnia operator potwierdził przez GUI na żywym rysunku, że
**działają wszystkie trzy zakładki**. Styl widoku: dwa zaznaczone widoki →
„zastosowano styl "W_View_Railing_Neighbour" na 2/2 widok(ach)”, oba z
ramką rozszerzenia dla sąsiadów. Rozszerzona ramka może nachodzić na
sąsiedni widok — to skutek większego widoku, nie błąd.

## Budowanie i wydawanie

```
taskkill /F /IM HFTTeklaTools.exe        # jeśli działa - zamyka też program operatora, uprzedzić
dotnet build HFTTeklaTools.csproj -c Debug -p:Platform=x64
"%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" installer\setup.iss
```

`-p:Platform=x64` jest wymagane — inaczej ścieżka wyjścia nie zgadza się z
`setup.iss`. `..\Directory.Build.Props` ustawia `TeklaVersion` dla Debug.
Ostrzeżenia `MSB3277` (konflikty wersji zależności Tekli) i `CS0618` w
`NotchPilot` (przestarzały konstruktor atrybutów z nazwą pliku — ten
sprawdzony na żywo, patrz `RoAxis\AGENTS.md` pkt 31; wariant z `ModelObject`
nie był testowany) pojawiają się przy każdym buildzie.

Wersja w **trzech** miejscach musi się zgadzać, bo `UpdateCheck` porównuje
assembly z tagiem: `<Version>` w `HFTTeklaTools.csproj`, `MyAppVersion` w
`installer\setup.iss`, tag release na GitHubie. Po wydaniu instalator
aktualizuje `%LOCALAPPDATA%\Programs\HFTTeklaTools\` — przebudowanie
projektu tej kopii **nie** zmienia. Release na GitHubie tworzy operator.

## Git

Repo publiczne: https://github.com/HoldFort-Bananza/HFT-Tekla-Tools-2025

- `dev` — domyślna, robocza. `release` — potwierdzony kod. Obie chronione
  (PR wymagany, także dla admina). Zmiany: gałąź tematyczna → PR do `dev`;
  wydanie: PR `dev → release`, tag na `release`.
- **Merge PR-ów klika operator** (klasyfikator auto mode blokuje merge przez
  API). Przed commitem `git branch --show-current`, push z jawną nazwą
  gałęzi.
- `gh`: `C:\Program Files\GitHub CLI\gh.exe` (może nie być na `PATH`).
- `README.md` bez numerów rysunków (repo publiczne). Pozostałe pliki,
  w tym `AGENTS.md` zakładek, numery mają — to baza do diagnozy.
- `HANDOFF.md` w katalogu repo to lokalna notatka operatora — w
  `.gitignore`, nigdy nie pushować.

## Trzy stare projekty — tylko do odczytu

`..\RO Axis Dimension Remover`, `..\Radius Dimention Mover`,
`..\Style Changer` zostają **nietknięte** jako archiwum: tylko czytać i
kopiować stamtąd. Nie edytować ich plików, nie commitować, nie przełączać
gałęzi, nie robić `pull`/`reset`/`stash`/`clean`, **nie budować w ich
katalogach** (build nadpisuje śledzone `bin\`), nie ruszać ich release'ów,
tagów, issue ani PR-ów; żadnych submodułów ani `ProjectReference` do nich.
Ich zainstalowane kopie i skróty z pulpitu zostają. (Jedyny wyjątek, PR #80
w RO, jest już zmergowany.)

Wiki RO i Radius (linki w `AGENTS.md` zakładek) zostają przy starych repo,
kopie robocze w `..\Archiwum\*.wiki`. To repo nie ma jeszcze własnego wiki.

## Otwarte

- Ikona aplikacji (logo w `..\HFT-Logo`) — niezrobiona, nikt jeszcze nie
  prosił.
- Instalator skompilowany, ale nie zainstalowany próbnie (wzorzec i lista
  pakietów `fetch-dependencies.ps1` bez zmian względem RO — 23 DLL-e, tyle
  samo co w buildzie).
