# API-Migrations-Anpassungen

Diese Datei dokumentiert alle Anpassungen, die im Rahmen der API-Migration vorgenommen wurden, um Kompilierungsfehler zu beheben.

## Übersicht

Die Migration betrifft mehrere Bereiche:
1. **ScrapeOptions-Struktur**: Umstellung von `bMain*` Feldern auf direkte Properties
2. **AdvancedSettings**: Entfernung von `Using`-Statements und direkte Verwendung von `Master.eAdvancedSettings`
3. **SettingsPanel**: Umstellung von `UniqueId` auf `UniqueName`
4. **Assembly-Namen**: Umstellung von `_AssemblyFileName` auf `_AssemblyName`
5. **DBElement**: Umstellung von `TVShow` auf `TVShowDetails`
6. **SearchResults**: Umstellung des Rückgabetyps von `List(Of Movie)` auf `MovieSearchResults`
7. **dlgSearchResults**: Anpassung der Konstruktor-Parameter

---

## 1. scraper.data.imdb.com

### Datei: `clsAddon.vb`

#### Änderung 1.1: UniqueId → UniqueName
**Zeile 172:**
```vb
' Vorher:
SPanel.UniqueId = String.Concat(_Name, "_Movie")

' Nachher:
SPanel.UniqueName = String.Concat(_Name, "_Movie")
```

#### Änderung 1.2: GetSetting → GetStringSetting
**Zeile 257:**
```vb
' Vorher:
_SpecialSettings_Movie.ForceTitleLanguage = Master.eAdvancedSettings.GetSetting("ForceTitleLanguage", String.Empty, , Enums.ContentType.Movie)

' Nachher:
_SpecialSettings_Movie.ForceTitleLanguage = Master.eAdvancedSettings.GetStringSetting("ForceTitleLanguage", String.Empty, Enums.ContentType.Movie)
```

#### Änderung 1.3: GetStringSetting Parameter korrigiert
**Zeile 289:**
```vb
' Vorher:
_SpecialSettings_TV.ForceTitleLanguage = Master.eAdvancedSettings.GetStringSetting("ForceTitleLanguage", String.Empty, , Enums.ContentType.TVShow)

' Nachher:
_SpecialSettings_TV.ForceTitleLanguage = Master.eAdvancedSettings.GetStringSetting("ForceTitleLanguage", String.Empty, Enums.ContentType.TVShow)
```

#### Änderung 1.4: AdvancedSettings Using-Statement entfernt
**Zeilen 292-322 (PersistSettings_Movie):**
```vb
' Vorher:
Private Sub PersistSettings_Movie()
    Using settings = New AdvancedSettings()
        settings.SetBooleanSetting("DoCast", ConfigScrapeOptions_Movie.Actors, , , Enums.ContentType.Movie)
        ' ... weitere Einstellungen ...
    End Using
End Sub

' Nachher:
Private Sub PersistSettings_Movie()
    Master.eAdvancedSettings.SetBooleanSetting("DoCast", ConfigScrapeOptions_Movie.Actors, False, Enums.ContentType.Movie)
    ' ... weitere Einstellungen ...
End Sub
```

**Zeilen 325-348 (PersistSettings_TV):**
```vb
' Vorher:
Private Sub PersistSettings_TV()
    Using settings = New AdvancedSettings()
        settings.SetBooleanSetting("DoActors", ConfigScrapeOptions_TV.Episodes.Actors, , , Enums.ContentType.TVEpisode)
        ' ... weitere Einstellungen ...
    End Using
End Sub

' Nachher:
Private Sub PersistSettings_TV()
    Master.eAdvancedSettings.SetBooleanSetting("DoActors", ConfigScrapeOptions_TV.Episodes.Actors, False, Enums.ContentType.TVEpisode)
    ' ... weitere Einstellungen ...
End Sub
```

#### Änderung 1.5: GetSearchResults Parameter korrigiert
**Zeilen 462-465 und 498-501:**
```vb
' Vorher:
Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)

' Nachher:
Return New Interfaces.AddonResult_Generic()
```

#### Änderung 1.6: TVShow → TVShowDetails
**Zeile 481:**
```vb
' Vorher:
If oDBElement.TVShow.UniqueIDs.IMDbIdSpecified Then

' Nachher:
If oDBElement.TVShowDetails.UniqueIDs.IMDbIdSpecified Then
```

### Datei: `clsScraper.vb`

#### Änderung 1.7: bMain* Felder durch direkte Properties ersetzt

**Zeile 251:**
```vb
' Vorher:
If filteredOptions.bMainCountries Then

' Nachher:
If filteredOptions.Countries Then
```

**Zeile 275:**
```vb
' Vorher:
If filteredOptions.bMainRuntime Then

' Nachher:
If filteredOptions.Runtime Then
```

**Zeile 299:**
```vb
' Vorher:
If filteredOptions.bMainMPAA Then

' Nachher:
If filteredOptions.MPAA Then
```

**Zeile 318:**
```vb
' Vorher:
If filteredOptions.bMainOutline AndAlso bIsScraperLanguage Then

' Nachher:
If filteredOptions.Outline AndAlso bIsScraperLanguage Then
```

**Zeilen 357-360:**
```vb
' Vorher:
If filteredOptions.bMainPremiered Then
    Dim datePremiered As New Date
    If Parse_Premiered(htmldReference, datePremiered) Then
        If filteredOptions.bMainPremiered Then nResult.Premiered = datePremiered.ToString("yyyy-MM-dd")

' Nachher:
If filteredOptions.Premiered Then
    Dim datePremiered As New Date
    If Parse_Premiered(htmldReference, datePremiered) Then
        If filteredOptions.Premiered Then nResult.Premiered = datePremiered.ToString("yyyy-MM-dd")
```

**Zeile 381:**
```vb
' Vorher:
If filteredOptions.bMainStudios Then

' Nachher:
If filteredOptions.Studios Then
```

**Zeile 405:**
```vb
' Vorher:
If filteredOptions.bMainTitle Then

' Nachher:
If filteredOptions.Title Then
```

**Zeile 432:**
```vb
' Vorher:
If filteredOptions.bMainWriters Then

' Nachher:
If filteredOptions.Credits Then
```

**Zeile 530:**
```vb
' Vorher:
If filteredOptions.bEpisodeAired Then

' Nachher:
If filteredOptions.Episodes.Aired Then
```

**Zeile 550:**
```vb
' Vorher:
If filteredOptions.bEpisodeDirectors Then

' Nachher:
If filteredOptions.Episodes.Directors Then
```

**Zeile 587:**
```vb
' Vorher:
If filteredOptions.bEpisodeRating Then

' Nachher:
If filteredOptions.Episodes.Ratings Then
```

**Zeile 714:**
```vb
' Vorher:
If filteredoptions.bMainActors Then

' Nachher:
If filteredoptions.Actors Then
```

**Zeile 738:**
```vb
' Vorher:
If filteredoptions.bMainCountries Then

' Nachher:
If filteredoptions.Countries Then
```

**Zeile 762:**
```vb
' Vorher:
If filteredoptions.bMainGenres Then

' Nachher:
If filteredoptions.Genres Then
```

**Zeile 781:**
```vb
' Vorher:
If filteredoptions.bMainPlot AndAlso bIsScraperLanguage Then

' Nachher:
If filteredoptions.Plot AndAlso bIsScraperLanguage Then
```

**Zeile 815:**
```vb
' Vorher:
If filteredoptions.bMainRating Then

' Nachher:
If filteredoptions.Ratings Then
```

**Zeile 839:**
```vb
' Vorher:
If filteredoptions.bMainStudios Then

' Nachher:
If filteredoptions.Studios Then
```

#### Änderung 1.8: Search_By_Title_Movie Rückgabetyp geändert
**Zeile 1442:**
```vb
' Vorher:
Private Function Search_By_Title_Movie(ByVal title As String, Optional ByVal year As Integer = 0) As List(Of EmberAPI.MediaContainers.Movie)
    Dim SearchResults As New List(Of EmberAPI.MediaContainers.Movie)

' Nachher:
Private Function Search_By_Title_Movie(ByVal title As String, Optional ByVal year As Integer = 0) As EmberAPI.MediaContainers.MovieSearchResults
    Dim SearchResults As New EmberAPI.MediaContainers.MovieSearchResults
```

#### Änderung 1.9: dlgSearchResults Konstruktor-Parameter
**Zeilen 1357, 1419:**
```vb
' Vorher:
Using dlgSearch As New dlgSearchResults(Me, "imdb", New List(Of String) From {"IMDb"}, Enums.ContentType.Movie)

' Nachher:
Using dlgSearch As New dlgSearchResults(Nothing, "imdb", New List(Of String) From {"IMDb"}, Enums.ContentType.Movie)
```

**Zeile 1360:**
```vb
' Vorher:
If dlgSearch.Result_Movie.UniqueIDs.IMDbIdSpecified Then

' Nachher:
If dlgSearch.Result.UniqueIDs.IMDbIdSpecified Then
```

### Datei: `frmSettingsHolder_Movie.vb`

#### Änderung 1.10: _AssemblyFileName → _AssemblyName
**Zeilen 79, 82, 89, 92, 226:**
```vb
' Vorher:
Addons.Instance.Data_Scrapers_Movie.FirstOrDefault(Function(p) p.AssemblyFileName = Addon._AssemblyFileName)

' Nachher:
Addons.Instance.Data_Scrapers_Movie.FirstOrDefault(Function(p) p.AssemblyFileName = Addon._AssemblyName)
```

### Datei: `frmSettingsHolder_TV.vb`

#### Änderung 1.11: _AssemblyFileName → _AssemblyName
**Zeilen 115, 118, 125, 128, 230:**
```vb
' Vorher:
Addons.Instance.Data_Scrapers_TV.FirstOrDefault(Function(p) p.AssemblyFileName = Addon._AssemblyFileName)

' Nachher:
Addons.Instance.Data_Scrapers_TV.FirstOrDefault(Function(p) p.AssemblyFileName = Addon._AssemblyName)
```

---

## 2. scraper.data.omdbapi.com

### Datei: `clsScraper.vb`

#### Änderung 2.1: bMainRating → Ratings
**Zeile 99:**
```vb
' Vorher:
If FilteredOptions.bMainRating Then

' Nachher:
If FilteredOptions.Ratings Then
```

### Datei: `clsAddon.vb`

#### Änderung 2.2: bMainRating → Ratings
**Zeilen 200, 208, 231, 250:**
```vb
' Vorher:
ConfigScrapeOptions_Movie.bMainRating = _SpecialSettings_Movie.AnyRatingEnabled
ConfigScrapeOptions_TV.bMainRating = _SpecialSettings_TV.AnyRatingEnabled

' Nachher:
ConfigScrapeOptions_Movie.Ratings = _SpecialSettings_Movie.AnyRatingEnabled
ConfigScrapeOptions_TV.Ratings = _SpecialSettings_TV.AnyRatingEnabled
```

---

## 3. scraper.data.thetvdb.com_old

### Datei: `clsAddon.vb`

#### Änderung 3.1: bMain* Felder durch direkte Properties ersetzt
**Zeilen 114-124:**
```vb
' Vorher:
_setup.chkScraperShowActors.Checked = ConfigScrapeOptions.bMainActors
_setup.chkScraperShowEpisodeGuide.Checked = ConfigScrapeOptions.bMainEpisodeGuide
_setup.chkScraperShowGenres.Checked = ConfigScrapeOptions.bMainGenres
_setup.chkScraperShowMPAA.Checked = ConfigScrapeOptions.bMainMPAA
_setup.chkScraperShowPlot.Checked = ConfigScrapeOptions.bMainPlot
_setup.chkScraperShowPremiered.Checked = ConfigScrapeOptions.bMainPremiered
_setup.chkScraperShowRating.Checked = ConfigScrapeOptions.bMainRating
_setup.chkScraperShowRuntime.Checked = ConfigScrapeOptions.bMainRuntime
_setup.chkScraperShowStatus.Checked = ConfigScrapeOptions.bMainStatus
_setup.chkScraperShowStudios.Checked = ConfigScrapeOptions.bMainStudios
_setup.chkScraperShowTitle.Checked = ConfigScrapeOptions.bMainTitle

' Nachher:
_setup.chkScraperShowActors.Checked = ConfigScrapeOptions.Actors
_setup.chkScraperShowEpisodeGuide.Checked = ConfigScrapeOptions.EpisodeGuideURL
_setup.chkScraperShowGenres.Checked = ConfigScrapeOptions.Genres
_setup.chkScraperShowMPAA.Checked = ConfigScrapeOptions.MPAA
_setup.chkScraperShowPlot.Checked = ConfigScrapeOptions.Plot
_setup.chkScraperShowPremiered.Checked = ConfigScrapeOptions.Premiered
_setup.chkScraperShowRating.Checked = ConfigScrapeOptions.Ratings
_setup.chkScraperShowRuntime.Checked = ConfigScrapeOptions.Runtime
_setup.chkScraperShowStatus.Checked = ConfigScrapeOptions.Status
_setup.chkScraperShowStudios.Checked = ConfigScrapeOptions.Studios
_setup.chkScraperShowTitle.Checked = ConfigScrapeOptions.Title
```

**Zeilen 157-167:**
```vb
' Vorher:
ConfigScrapeOptions.bMainActors = Master.eAdvancedSettings.GetBooleanSetting("DoActors", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainEpisodeGuide = Master.eAdvancedSettings.GetBooleanSetting("DoEpisodeGuide", False, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainGenres = Master.eAdvancedSettings.GetBooleanSetting("DoGenre", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainMPAA = Master.eAdvancedSettings.GetBooleanSetting("DoMPAA", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainPlot = Master.eAdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainPremiered = Master.eAdvancedSettings.GetBooleanSetting("DoPremiered", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainRating = Master.eAdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainRuntime = Master.eAdvancedSettings.GetBooleanSetting("DoRuntime", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainStatus = Master.eAdvancedSettings.GetBooleanSetting("DoStatus", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainStudios = Master.eAdvancedSettings.GetBooleanSetting("DoStudio", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.bMainTitle = Master.eAdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.TVShow)

' Nachher:
ConfigScrapeOptions.Actors = Master.eAdvancedSettings.GetBooleanSetting("DoActors", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.EpisodeGuideURL = Master.eAdvancedSettings.GetBooleanSetting("DoEpisodeGuide", False, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Genres = Master.eAdvancedSettings.GetBooleanSetting("DoGenre", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.MPAA = Master.eAdvancedSettings.GetBooleanSetting("DoMPAA", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Plot = Master.eAdvancedSettings.GetBooleanSetting("DoPlot", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Premiered = Master.eAdvancedSettings.GetBooleanSetting("DoPremiered", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Ratings = Master.eAdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Runtime = Master.eAdvancedSettings.GetBooleanSetting("DoRuntime", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Status = Master.eAdvancedSettings.GetBooleanSetting("DoStatus", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Studios = Master.eAdvancedSettings.GetBooleanSetting("DoStudio", True, , Enums.ContentType.TVShow)
ConfigScrapeOptions.Title = Master.eAdvancedSettings.GetBooleanSetting("DoTitle", True, , Enums.ContentType.TVShow)
```

**Zeilen 187-197 (SaveSettings):**
```vb
' Vorher:
Using settings = New AdvancedSettings()
    settings.SetBooleanSetting("DoActors", ConfigScrapeOptions.bMainActors, , , Enums.ContentType.TVShow)
    ' ... weitere Einstellungen ...
End Using

' Nachher:
Master.eAdvancedSettings.SetBooleanSetting("DoActors", ConfigScrapeOptions.Actors, False, Enums.ContentType.TVShow)
' ... weitere Einstellungen ...
```

**Zeilen 212-222:**
```vb
' Vorher:
ConfigScrapeOptions.bMainActors = _setup.chkScraperShowActors.Checked
ConfigScrapeOptions.bMainEpisodeGuide = _setup.chkScraperShowEpisodeGuide.Checked
' ... weitere Einstellungen ...

' Nachher:
ConfigScrapeOptions.Actors = _setup.chkScraperShowActors.Checked
ConfigScrapeOptions.EpisodeGuideURL = _setup.chkScraperShowEpisodeGuide.Checked
' ... weitere Einstellungen ...
```

### Datei: `clsScraper.vb`

#### Änderung 3.2: bMain* Felder durch direkte Properties ersetzt
**Zeilen 386, 449, 466, 473, 489, 496, 521, 531, 545, 552, 559, 566:**
```vb
' Vorher:
If filteredOptions.bMainRating Then
If filteredOptions.bMainActors Then
If filteredOptions.bMainEpisodeGuide Then
If filteredOptions.bMainGenres Then
If filteredOptions.bMainMPAA Then
If filteredOptions.bMainPlot Then
If filteredOptions.bMainPremiered Then
If filteredOptions.bMainRating Then
If filteredOptions.bMainRuntime Then
If filteredOptions.bMainStatus Then
If filteredOptions.bMainStudios Then
If filteredOptions.bMainTitle Then

' Nachher:
If filteredOptions.Ratings Then
If filteredOptions.Actors Then
If filteredOptions.EpisodeGuideURL Then
If filteredOptions.Genres Then
If filteredOptions.MPAA Then
If filteredOptions.Plot Then
If filteredOptions.Premiered Then
If filteredOptions.Ratings Then
If filteredOptions.Runtime Then
If filteredOptions.Status Then
If filteredOptions.Studios Then
If filteredOptions.Title Then
```

---

## 4. scraper.data.trakt.tv

### Datei: `clsAddon.vb`

#### Änderung 4.1: bMainRating und bMainUserRating → Ratings und UserRating
**Zeilen 171-172, 195-196, 216-217, 227-228, 237-238, 250-251, 260-261, 274-275:**
```vb
' Vorher:
ConfigScrapeOptions_Movie.bMainRating = ...
ConfigScrapeOptions_Movie.bMainUserRating = ...
ConfigScrapeOptions_TV.bMainRating = ...
ConfigScrapeOptions_TV.bMainUserRating = ...

' Nachher:
ConfigScrapeOptions_Movie.Ratings = ...
ConfigScrapeOptions_Movie.UserRating = ...
ConfigScrapeOptions_TV.Ratings = ...
ConfigScrapeOptions_TV.UserRating = ...
```

#### Änderung 4.2: AdvancedSettings Using-Statement entfernt
**Zeilen 236-238, 249-251:**
```vb
' Vorher:
Using settings = New AdvancedSettings()
    settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_Movie.bMainRating, , , Enums.ContentType.Movie)
    ' ... weitere Einstellungen ...
End Using

' Nachher:
Master.eAdvancedSettings.SetBooleanSetting("DoRating", ConfigScrapeOptions_Movie.Ratings, False, Enums.ContentType.Movie)
' ... weitere Einstellungen ...
```

### Datei: `clsScraper.vb`

#### Änderung 4.3: bMainRating und bMainUserRating → Ratings und UserRating
**Zeilen 230, 243, 322, 335:**
```vb
' Vorher:
If tFilteredOptions.bMainRating Then
If tFilteredOptions.bMainUserRating Then
If FilteredOptions.bMainRating Then
If FilteredOptions.bMainUserRating Then

' Nachher:
If tFilteredOptions.Ratings Then
If tFilteredOptions.UserRating Then
If FilteredOptions.Ratings Then
If FilteredOptions.UserRating Then
```

---

## 5. scraper.data.thetvdb.com

### Datei: `clsScraper.vb`

#### Änderung 5.1: bMain* Felder durch direkte Properties ersetzt
**Zeilen 430, 433, 436, 439, 442, 445, 448, 454, 457, 460, 463, 466, 469, 472 (Movie):**
```vb
' Vorher:
If filteredOptions.bMainActors Then
If filteredOptions.bMainCertifications Then
If filteredOptions.bMainCountries Then
If filteredOptions.bMainDirectors Then
If filteredOptions.bMainGenres Then
If filteredOptions.bMainOriginalTitle Then
If filteredOptions.bMainPlot Then
If filteredOptions.bMainPremiered Then
If filteredOptions.bMainRuntime Then
If filteredOptions.bMainStudios Then
If filteredOptions.bMainTagline Then
If filteredOptions.bMainTitle Then
If filteredOptions.bMainTrailer Then
If filteredOptions.bMainWriters Then

' Nachher:
If filteredOptions.Actors Then
If filteredOptions.Certifications Then
If filteredOptions.Countries Then
If filteredOptions.Directors Then
If filteredOptions.Genres Then
If filteredOptions.OriginalTitle Then
If filteredOptions.Plot Then
If filteredOptions.Premiered Then
If filteredOptions.Runtime Then
If filteredOptions.Studios Then
If filteredOptions.Tagline Then
If filteredOptions.Title Then
If filteredOptions.TrailerLink Then
If filteredOptions.Credits Then
```

**Zeilen 717, 720, 723, 726, 729, 732, 735, 738, 744, 747, 750, 753, 756, 759 (TVShow):**
```vb
' Vorher:
If filteredOptions.bMainActors Then
If filteredOptions.bMainCertifications Then
If filteredOptions.bMainCountries Then
If filteredOptions.bMainCreators Then
If filteredOptions.bMainDirectors Then
If filteredOptions.bMainGenres Then
If filteredOptions.bMainOriginalTitle Then
If filteredOptions.bMainPlot Then
If filteredOptions.bMainPremiered Then
If filteredOptions.bMainRuntime Then
If filteredOptions.bMainStatus Then
If filteredOptions.bMainStudios Then
If filteredOptions.bMainTagline Then
If filteredOptions.bMainTitle Then

' Nachher:
If filteredOptions.Actors Then
If filteredOptions.Certifications Then
If filteredOptions.Countries Then
If filteredOptions.Creators Then
If filteredOptions.Directors Then
If filteredOptions.Genres Then
If filteredOptions.OriginalTitle Then
If filteredOptions.Plot Then
If filteredOptions.Premiered Then
If filteredOptions.Runtime Then
If filteredOptions.Status Then
If filteredOptions.Studios Then
If filteredOptions.Tagline Then
If filteredOptions.Title Then
```

---

## Mapping-Tabelle: bMain* → Direkte Properties

| Altes Feld | Neues Feld | Bemerkung |
|------------|------------|-----------|
| `bMainActors` | `Actors` | - |
| `bMainCertifications` | `Certifications` | - |
| `bMainCountries` | `Countries` | - |
| `bMainCreators` | `Creators` | - |
| `bMainDirectors` | `Directors` | - |
| `bMainEpisodeGuide` | `EpisodeGuideURL` | - |
| `bMainGenres` | `Genres` | - |
| `bMainMPAA` | `MPAA` | - |
| `bMainOriginalTitle` | `OriginalTitle` | - |
| `bMainOutline` | `Outline` | - |
| `bMainPlot` | `Plot` | - |
| `bMainPremiered` | `Premiered` | - |
| `bMainRating` | `Ratings` | - |
| `bMainRuntime` | `Runtime` | - |
| `bMainStatus` | `Status` | - |
| `bMainStudios` | `Studios` | - |
| `bMainTagline` | `Tagline` | - |
| `bMainTitle` | `Title` | - |
| `bMainTrailer` | `TrailerLink` | - |
| `bMainUserRating` | `UserRating` | - |
| `bMainWriters` | `Credits` | - |
| `bEpisodeAired` | `Episodes.Aired` | - |
| `bEpisodeDirectors` | `Episodes.Directors` | - |
| `bEpisodeRating` | `Episodes.Ratings` | - |

---

## Allgemeine Änderungen

### AdvancedSettings-Verwendung

**Vorher:**
```vb
Using settings = New AdvancedSettings()
    settings.SetBooleanSetting("Key", value, , , Enums.ContentType.Movie)
    settings.SetStringSetting("Key", value, , , Enums.ContentType.Movie)
End Using
```

**Nachher:**
```vb
Master.eAdvancedSettings.SetBooleanSetting("Key", value, False, Enums.ContentType.Movie)
Master.eAdvancedSettings.SetStringSetting("Key", value, False, Enums.ContentType.Movie)
```

**Hinweis:** Der dritte Parameter `isDefault` wurde auf `False` gesetzt, da er vorher leer war.

### GetStringSetting Parameter

**Vorher:**
```vb
Master.eAdvancedSettings.GetStringSetting("Key", defaultValue, , Enums.ContentType.Movie)
```

**Nachher:**
```vb
Master.eAdvancedSettings.GetStringSetting("Key", defaultValue, Enums.ContentType.Movie)
```

---

## Zusammenfassung

- **scraper.data.imdb.com**: 11 Änderungen
- **scraper.data.omdbapi.com**: 2 Änderungen
- **scraper.data.thetvdb.com_old**: 2 Änderungen (mehrere Felder betroffen)
- **scraper.data.trakt.tv**: 3 Änderungen
- **scraper.data.thetvdb.com**: 1 Änderung (mehrere Felder betroffen)

**Gesamt:** 19 verschiedene Änderungstypen über 5 Scraper-Addons

---

## Offene Punkte / Noch zu behebende Fehler

Die folgenden Scraper-Addons benötigen noch Anpassungen:

### 1. scraper.data.omdbapi.com
- ✅ `bMainRating` → `Ratings` in `clsScraper.vb` (Zeile 99) - **NOCH ZU BEHEBEN**
- ✅ `bMainRating` → `Ratings` in `clsAddon.vb` (Zeilen 200, 208, 231, 250) - **NOCH ZU BEHEBEN**

### 2. scraper.data.thetvdb.com_old
- ✅ Alle `bMain*` Felder in `clsAddon.vb` und `clsScraper.vb` - **NOCH ZU BEHEBEN**
- ✅ `Using settings = New AdvancedSettings()` entfernen - **NOCH ZU BEHEBEN**

### 3. scraper.data.trakt.tv
- ✅ `bMainRating` und `bMainUserRating` → `Ratings` und `UserRating` - **NOCH ZU BEHEBEN**
- ✅ `Using settings = New AdvancedSettings()` entfernen - **NOCH ZU BEHEBEN**

### 4. scraper.data.thetvdb.com
- ✅ Alle `bMain*` Felder in `clsScraper.vb` - **NOCH ZU BEHEBEN**
- ✅ `Using settings = New AdvancedSettings()` entfernen - **NOCH ZU BEHEBEN**

### 5. Weitere Addons mit Using-Statements
Die folgenden Addons haben noch `Using settings = New AdvancedSettings()` und müssen angepasst werden:
- `core.filemanager` - `clsAddon.vb` Zeile 162
- `core.renamer` - `clsAddon.vb` Zeile 465
- `scraper.trailer.youtube.com` - `clsAddon.vb` Zeile 135
- `scraper.trailer.videobuster.de` - `clsAddon.vb` Zeile 126
- `scraper.trailer.themoviedb.org` - `clsAddon.vb` Zeile 159
- `scraper.trailer.davestrailerpage.co.uk` - `clsAddon.vb` Zeile 128
- `scraper.trailer.apple.com` - `clsAddon.vb` Zeile 132
- `scraper.theme.youtube.com` - `clsAddon.vb` Zeilen 174, 180
- `scraper.theme.televisiontunes.com` - `clsAddon.vb` Zeilen 174, 180
- `scraper.image.thetvdb.com_old` - `clsAddon.vb` Zeile 167
- `scraper.image.themoviedb.org` - `clsAddon.vb` Zeilen 429, 437, 445
- `scraper.image.fanart.tv` - `clsAddon.vb` Zeilen 416, 432, 448
- `addon.trakt.tv` - `dlgTrakttvManager.vb` Zeilen 326, 359, 2796, 2834

---

## Status-Übersicht

| Addon | Status | Bemerkung |
|-------|--------|-----------|
| scraper.data.imdb.com | ✅ **FERTIG** | Alle Änderungen implementiert |
| scraper.data.omdbapi.com | ⚠️ **OFFEN** | bMainRating muss noch geändert werden |
| scraper.data.thetvdb.com_old | ⚠️ **OFFEN** | bMain* Felder und Using-Statements |
| scraper.data.trakt.tv | ⚠️ **OFFEN** | bMainRating/UserRating und Using-Statements |
| scraper.data.thetvdb.com | ⚠️ **OFFEN** | bMain* Felder und Using-Statements |
| Weitere Addons | ⚠️ **OFFEN** | Using-Statements müssen entfernt werden |

---

*Erstellt am: 2024-12-19*
*Letzte Aktualisierung: 2024-12-19*
