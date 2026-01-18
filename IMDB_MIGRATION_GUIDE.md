# IMDb Data Scraper - Migrations-Anleitung

## ?? Schnellstart f�r scraper.data.imdb.com

Diese Anleitung beschreibt die notwendigen Schritte zur Migration des IMDb Data Scrapers.

---

## ?? Schritt-f�r-Schritt Anleitung

### Schritt 1: Aktuelle Datei �öffnen
```
Addons/scraper.data.imdb.com/clsAddon.vb
```

### Schritt 2: Properties-Region aktualisieren

**VORHER:**
```visualbasic
ReadOnly Property Name() As String Implements Interfaces.IAddon_Data_Scraper_Movie.Name, Interfaces.IAddon_Data_Scraper_TV.Name
    Get
        Return _Name
    End Get
End Property

ReadOnly Property Version() As String Implements Interfaces.IAddon_Data_Scraper_Movie.Version, Interfaces.IAddon_Data_Scraper_TV.Version
    Get
        Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
    End Get
End Property

Property ScraperEnabled_Movie() As Boolean Implements Interfaces.IAddon_Data_Scraper_Movie.Enabled
    Get
        Return _ScraperEnabled_Movie
    End Get
    Set(ByVal value As Boolean)
        _ScraperEnabled_Movie = value
    End Set
End Property

Property ScraperEnabled_TV() As Boolean Implements Interfaces.IAddon_Data_Scraper_TV.ScraperEnabled
    Get
        Return _ScraperEnabled_TV
    End Get
    Set(ByVal value As Boolean)
        _ScraperEnabled_TV = value
    End Set
End Property
```

**NACHHER:**
```visualbasic
ReadOnly Property ModuleName() As String Implements Interfaces.IAddon_Data_Scraper_Movie.ModuleName, Interfaces.IAddon_Data_Scraper_TV.ModuleName
    Get
        Return _Name
    End Get
End Property

ReadOnly Property ModuleVersion() As String Implements Interfaces.IAddon_Data_Scraper_Movie.ModuleVersion, Interfaces.IAddon_Data_Scraper_TV.ModuleVersion
    Get
        Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
    End Get
End Property

Property ScraperEnabled_Movie() As Boolean Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperEnabled
    Get
        Return _ScraperEnabled_Movie
    End Get
    Set(ByVal value As Boolean)
        _ScraperEnabled_Movie = value
    End Set
End Property

Property ScraperEnabled_TV() As Boolean Implements Interfaces.IAddon_Data_Scraper_TV.ScraperEnabled
    Get
        Return _ScraperEnabled_TV
    End Get
    Set(ByVal value As Boolean)
        _ScraperEnabled_TV = value
    End Set
End Property
```

### Schritt 3: Init-Methoden aktualisieren

**VORHER:**
```visualbasic
Sub Init_Movie(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements Interfaces.IAddon_Data_Scraper_Movie.Init
    _AssemblyName = sAssemblyName
    LoadSettings_Movie()
End Sub

Sub Init_TV(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements Interfaces.IAddon_Data_Scraper_TV.Init
    _AssemblyName = sAssemblyName
    LoadSettings_TV()
End Sub
```

**NACHHER:**
```visualbasic
Sub Init_Movie(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Data_Scraper_Movie.Init
    _AssemblyName = sAssemblyName
    LoadSettings_Movie()
End Sub

Sub Init_TV(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Data_Scraper_TV.Init
    _AssemblyName = sAssemblyName
    LoadSettings_TV()
End Sub
```

### Schritt 4: Scraper-Methoden umbenennen

**VORHER:**
```visualbasic
Function Scraper_Movie(ByRef DBMovie As Database.DBElement,
                       ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                       ByVal ScrapeOptions As Structures.ScrapeOptions
                       ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.Scraper_Movie
    ' Implementation...
End Function
```

**NACHHER:**
```visualbasic
Function Scraper(ByRef DBMovie As Database.DBElement,
                 ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                 ByVal ScrapeOptions As Structures.ScrapeOptions
                 ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.Scraper
    ' Implementation bleibt gleich...
End Function
```

### Schritt 5: Neue Methoden hinzuf�gen

**F�r Movie-Interface:**
```visualbasic
Function GetMovieStudio(ByRef DBMovie As Database.DBElement, 
                       ByVal sStudio As List(Of String)
                       ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.GetMovieStudio
    ' IMDb unterst�tzt dies nicht - leere Implementation
    Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
End Function

Function GetSearchResults_Movie(ByRef nMovie As Database.DBElement
                                ) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_Movie.GetSearchResults
    ' Suchresultate-Logik hier (falls vorhanden)
    ' Oder leer lassen:
    Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
End Function
```

**F�r TV-Interface:**
```visualbasic
Function GetSearchResults_TV(ByRef nShow As Database.DBElement
                            ) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_TV.GetSearchResults
    ' Suchresultate-Logik hier (falls vorhanden)
    Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
End Function
```

### Schritt 6: ScraperOrderChanged implementieren

**AM ENDE der Methods-Region einf�gen:**
```visualbasic
Public Sub ScraperOrderChanged() Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperOrderChanged, Interfaces.IAddon_Data_Scraper_TV.ScraperOrderChanged
    ' Beide Settings-Panels updaten wenn vorhanden
    If _setup_Movie IsNot Nothing Then _setup_Movie.OrderChanged()
    If _setup_TV IsNot Nothing Then _setup_TV.OrderChanged()
End Sub
```

### Schritt 7: TV Scraper-Methoden pr�fen

Sicherstellen dass folgende Signaturen korrekt sind:

```visualbasic
' TVShow - mit ScrapeModifiers
Function Scraper_TVShow(ByRef DBTVShow As Database.DBElement,
                        ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                        ByVal ScrapeOptions As Structures.ScrapeOptions
                        ) As Interfaces.AddonResult_Data_Scraper_TVShow Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVShow
    ' Implementation...
End Function

' TVEpisode - OHNE ScrapeModifiers
Function Scraper_TVEpisode(ByRef DBTVEpisode As Database.DBElement,
                          ByVal ScrapeOptions As Structures.ScrapeOptions
                          ) As Interfaces.AddonResult_Data_Scraper_TVEpisode Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVEpisode
    ' Implementation...
End Function

' TVSeason - OHNE ScrapeModifiers
Function Scraper_TVSeason(ByRef DBTVSeason As Database.DBElement,
                         ByVal ScrapeOptions As Structures.ScrapeOptions
                         ) As Interfaces.AddonResult_Data_Scraper_TVSeason Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVSeason
    ' Implementation...
End Function
```

---

## ?? H�ufige Fehler & L�sungen

### Fehler 1: "Scraper_Movie kann nicht implementiert werden"
```
BC30401: "Scraper_Movie" kann nicht implementiert werden
```

**L�sung:** Methode muss `Scraper` hei�en (ohne `_Movie` Suffix)

### Fehler 2: "ScraperOrderChanged_Movie kann nicht implementiert werden"
```
BC30149: Class "Addon" muss "Sub ScraperOrderChanged()" implementieren
```

**L�sung:** Eine kombinierte Methode ohne Suffix erstellen

### Fehler 3: "GetMovieStudio muss implementiert werden"
```
BC30149: Class "Addon" muss "Function GetMovieStudio(...)" implementieren
```

**L�sung:** Neue Methode hinzuf�gen (siehe Schritt 5)

### Fehler 4: "Data_Scrapers_Movie ist kein Member"
```
BC30456: "Data_Scrapers_Movie" ist kein Member von "Addons"
```

**L�sung:** **IGNORIEREN** - Dies ist ein Pre-existing Issue in den Settings-Holder Formularen, nicht Teil der Interface-Migration.

---

## ? Verifikation

Nach allen �ÄÄnderungen:

```powershell
# Build testen
cd C:\GitHub\Ember-MM-Newscraper
msbuild Addons/scraper.data.imdb.com/scraper.data.imdb.com.vbproj /p:Configuration=Debug /p:Platform=x64 /t:Rebuild
```

**Erwartetes Ergebnis:**
- ? Keine BC30401 Fehler (Interface-Implementation)
- ? Keine BC30149 Fehler (fehlende Members)
- ?? BC30456 Fehler in Settings-Holder Formularen sind OK (Pre-existing)

---

## ?? Zusammenfassung der �ÄÄnderungen

| Kategorie | Alt | Neu |
|-----------|-----|-----|
| Property | `Name` | `ModuleName` |
| Property | `Version` | `ModuleVersion` |
| Property | `Enabled` | `ScraperEnabled` |
| Method | `Init(name, exe)` | `Init(name)` |
| Method | `Scraper_Movie(...)` | `Scraper(...)` |
| Method | - | `GetMovieStudio(...)` |
| Method | - | `GetSearchResults(...)` (2x) |
| Method | `OrderChanged_*()` | `ScraperOrderChanged()` |

---

## 📚 Referenz-Implementation

**Siehe:** `Addons/scraper.data.omdbapi.com/clsAddon.vb`

Diese Datei ist bereits erfolgreich migriert und kann als Vorlage dienen.

---

## ✅ Checkliste

- [ ] Properties umbenannt (3x: ModuleName, ModuleVersion, ScraperEnabled)
- [ ] Init-Signaturen angepasst (2x Parameter entfernt)
- [ ] Scraper_Movie ? Scraper umbenannt
- [ ] GetMovieStudio hinzugef�gt
- [ ] GetSearchResults f�r Movie hinzugef�gt
- [ ] GetSearchResults f�r TV hinzugef�gt
- [ ] ScraperOrderChanged implementiert (kombiniert)
- [ ] TV Scraper-Signaturen gepr�ft (3x)
- [ ] Build getestet
- [ ] Interface-Errors behoben

**Status nach Completion:** ? IMDb Data Scraper erfolgreich migriert
