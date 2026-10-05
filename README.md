# HFT Tekla Tools — Tekla Structures 2025

Jedna aplikacja (`.exe`) z narzędziami do rysunków Tekla Structures 2025, po
jednej zakładce na narzędzie. Zewnętrzny program, nie wtyczka ładowana w
Tekli — łączy się przez Tekla Open API z już uruchomioną Teklą.

**Program działa wyłącznie na danych z Tekla Open API.** Nie robi zrzutów
ekranu, nie analizuje pikseli, nie czyta okna Tekli. Każdą zmianę na rysunku
cofa zwykłe **Ctrl+Z** w Tekli.

## Zakładki

### RO – wymiary do osi

Dla rysunków pojedynczej części z profilem RO (rura okrągła). Przycisk
**Posprzątaj wymiary** robi po kolei dwie rzeczy na całym aktywnym rysunku:

1. **Usuwa wymiary do osi** — na każdym widoku kasuje wymiary, które Tekla
   przy skośnym cięciu zaczepiła o teoretyczną oś rury zamiast o jej
   powierzchnię. Kasuje tylko wymiar, którego oba końce leżą w obrębie
   ściany cięcia, więc całkowita długość rury zostaje. Widoki z giętą rurą
   pomija. Przycisk **naprawdę kasuje**; Ctrl+Z cofa widok po widoku.
2. **Wstawia wymiar wcięcia** — dla każdej skośnej ściany cięcia (≥ 6,5°)
   prostej rury dorysowuje brakującą długość i szerokość wcięcia, a płaski
   wymiar promienia przy skosie zamienia na średnicę. Nie dubluje wymiarów,
   które już są.

### Wymiary R

Przycisk **Przesuń wszystkie wymiary R (unikaj kolizji)** porządkuje
wszystkie wymiary promieni na otwartym rysunku:

- tekst wymiaru nie wpada w obrys części, w linie wymiarowe ani w inne opisy;
- wcięcia (łuki wklęsłe) dostają tekst po właściwej stronie, nie w materiale;
- wymiary idące w tę samą stronę są wyrównane do jednej linii;
- dwa teksty, które wylądowałyby jeden na drugim, są rozsuwane — a gdy każde
  rozsunięcie byłoby gorsze, program nie rusza niczego i mówi o tym w logu;
- żaden tekst nie wychodzi za arkusz.

Liczone analitycznie, jednym przejściem. Ponowne kliknięcie nic nie psuje.

### Styl widoku

Zaznacz w edytorze rysunku widok (albo dowolny obiekt w widoku; można kilka
widoków naraz), wybierz styl z listy (domyślnie `W_View_Railing_Neighbour`) i
kliknij **Pokaż sąsiadów** — program podmienia zaznaczonym widokom View
Properties na wybrany styl. Lista stylów pochodzi z plików `.vi` modelu.

## Wymagania

- Tekla Structures **2025** na tym samym komputerze, z ważną licencją.
- Internet podczas instalacji (instalator pobiera biblioteki Tekla Open API).

## Instalacja

1. Pobierz `HFTTeklaTools-Setup-vX.Y.Z.exe` z [Releases](../../releases).
2. Uruchom i zaakceptuj pokazaną licencję (EULA Trimble/Tekla).
3. Instalator pobierze biblioteki Tekla Open API świeżo z
   [nuget.org](https://nuget.org) — pod Twoją własną licencją Tekli,
   dokładnie to samo, co zrobiłby `dotnet restore`.

> Biblioteki Tekli **nie są dołączone** do repozytorium ani do instalatora.
> Są własnością Trimble/Tekla, a ich licencja zabrania redystrybucji — to
> repozytorium jest publiczne. Źródła instalatora są jawne w `installer/`.

Program sam sprawdza przy starcie, czy jest nowsza wersja, i pokazuje wtedy
pasek nad zakładkami. Bez internetu milczy.

Log zakładki RO każdej sesji ląduje w `logs\` obok pliku `.exe`.

## Diagnostyka bez GUI

Tryb konsolowy, tylko do odczytu (nigdy nie kasuje ani nie wstawia), log na
`stdout`, np.:

```
HFTTeklaTools.exe --diag-active            # RO: co skasowałby przycisk na aktywnym rysunku
HFTTeklaTools.exe --diag-dimension-style   # RO: istniejące wymiary aktywnego rysunku
HFTTeklaTools.exe --style-diag-active      # Styl: połączenie, zaznaczenie, dostępne style
```

Pełna lista: `AGENTS.md`.

## Budowanie z kodu

```
dotnet build HFTTeklaTools.csproj -c Debug -p:Platform=x64
ISCC.exe installer\setup.iss
```

`-p:Platform=x64` jest wymagane — inaczej ścieżka wyjścia nie zgadza się z
`installer/setup.iss`. Instalator wymaga [Inno Setup](https://jrsoftware.org/isinfo.php) 6
i wcześniejszego buildu (`bin/` nie jest w repozytorium).

Zmiany idą przez gałąź i pull request do `dev`; `release` trzyma
potwierdzony kod. Wiedza techniczna, historia decyzji i zasady pracy:
[AGENTS.md](AGENTS.md) oraz `AGENTS.md` w katalogu każdego narzędzia.
