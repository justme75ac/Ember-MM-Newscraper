# Ember Media Manager - Interface Migration Dokumentation

## �ÜÜbersicht

Dieses Dokument beschreibt die notwendigen �ÄÄnderungen zur Migration der Addon-Interfaces im `active_development` Branch von Ember Media Manager.

**Stand:** 2026-01-10  
**Branch:** active_development  
**Hauptproblem:** Unvollst�ndige Interface-Refaktorierung - alte Interface-Namen werden verwendet, aber neue Interfaces sind bereits definiert

---

## ?? Ziel

Migration aller Addons von den alten Interface-Signaturen zu den neuen, konsistenten Interface-Definitionen in `EmberAPI/clsInterfaces.vb`.

---

## ?? Status der Migration

### ? ERFOLGREICH ABGESCHLOSSEN (9 Addons)

#### Generic-Addons (8/9)
1. ? **core.audiovideocodecmapping** - Vollst�ndig migriert
2. ? **core.videosourcemapping** - Vollst�ndig migriert
3. ? **core.contextmenu** - Vollst�ndig migriert & erfolgreich gebaut ?
4. ? **core.filemanager** - Vollst�ndig migriert
5. ? **core.renamer** - Vollst�ndig migriert
6. ? **core.medialisteditor** - Vollst�ndig migriert
7. ? **addon.websitecreator** - Vollst�ndig migriert
8. ? **addon.trakt.tv** - Vollst�ndig migriert

#### Data Scrapers (1/7)
9. ? **scraper.data.omdbapi.com** - Interface vollst�ndig migriert (Settings-Holder Fehler sind Pre-existing)

### ?? PROBLEMATISCH

**addon.kodiinterface:**
- Code-Duplikation in `CreateContextMenu` Methode (Zeilen ~1440-1470)
- RunGeneric und GenericEvent M�SSEN beibehalten werden (anders als andere Generic-Addons)
- Erfordert manuelle Korrektur wegen komplexer Struktur

### ?? NOCH ZU MIGRIEREN (17 Addons)

**PRIORIT�T: IMDb Data Scraper**
- ? **scraper.data.imdb.com** - ALS N�CHSTES (vom Benutzer angefordert)

**Verbleibende Data Scrapers (5):**
- scraper.data.thetvdb.com
- scraper.data.trakt.tv
- scraper.data.moviepilot.de
- scraper.data.ofdb.de
- scraper.data.thetvdb.com_old

**Image Scrapers (3):**
- scraper.image.fanart.tv
- scraper.image.themoviedb.org
- scraper.image.thetvdb.com_old

**Trailer Scrapers (5):**
- scraper.trailer.youtube.com
- scraper.trailer.videobuster.de
- scraper.trailer.themoviedb.org
- scraper.trailer.apple.com
- scraper.trailer.davestrailerpage.co.uk

**Theme Scrapers (2):**
- scraper.theme.youtube.com
- scraper.theme.televisiontunes.com

**Special Addon (1):**
- addon.themoviedb.org (kombiniert mehrere Interfaces)

---

## ?? Interface-�ÄÄnderungen im Detail

### 1. Generic-Addons (IAddon_Generic)

**Datei:** `EmberAPI/clsInterfaces.vb` (Zeilen ~280-295)

#### Property-Umbenennungen:
```visualbasic
' ALT ? NEU
Enabled ? ScraperEnabled
Name ? ModuleName
Version ? ModuleVersion
```

#### Method-Signaturen:
```visualbasic
' ALT
Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String)

' NEU
Sub Init(ByVal sAssemblyName As String)
```

#### Entfernte Members:
```visualbasic
' Diese existieren NICHT mehr im neuen Interface:
ReadOnly Property EventType() As List(Of Enums.AddonEventType)
ReadOnly Property IsBusy() As Boolean
Event GenericEvent(ByVal eventType As Enums.AddonEventType, ByRef parameters As List(Of Object))
Function RunGeneric(...) As AddonResult_Generic
```

#### Neue Members:
```visualbasic
' Muss implementiert werden:
Sub ScraperOrderChanged()
```

#### Beispiel-Migration:
```visualbasic
' VORHER:
Public Class Addon
    Implements Interfaces.IAddon_Generic
    
    ReadOnly Property Name() As String Implements Interfaces.IAddon_Generic.Name
        Get
            Return _Name
        End Get
    End Property
    
    Property Enabled() As Boolean Implements Interfaces.IAddon_Generic.Enabled
        Get
            Return _Enabled
        End Get
        Set(value As Boolean)
            _Enabled = value
        End Set
    End Property
    
    Sub Init(ByVal sAssemblyName As String, ByVal sExecutable As String) Implements Interfaces.IAddon_Generic.Init
        _AssemblyName = sAssemblyName
    End Sub
End Class

' NACHHER:
Public Class Addon
    Implements Interfaces.IAddon_Generic
    
    ReadOnly Property ModuleName() As String Implements Interfaces.IAddon_Generic.ModuleName
        Get
            Return _Name
        End Get
    End Property
    
    Property ScraperEnabled() As Boolean Implements Interfaces.IAddon_Generic.ScraperEnabled
        Get
            Return _Enabled
        End Get
        Set(value As Boolean)
            _Enabled = value
        End Set
    End Property
    
    Sub Init(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Generic.Init
        _AssemblyName = sAssemblyName
    End Sub
    
    Sub ScraperOrderChanged() Implements Interfaces.IAddon_Generic.ScraperOrderChanged
        ' Implementation hier
    End Sub
End Class
```

---

### 2. Data Scrapers - Movie (IAddon_Data_Scraper_Movie)

**Datei:** `EmberAPI/clsInterfaces.vb` (Zeilen ~297-309)

#### Interface-Definition:
```visualbasic
Public Interface IAddon_Data_Scraper_Movie
    Event AddonNeedsRestart()
    Event AddonSettingsChanged()
    Event AddonStateChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)
    
    ReadOnly Property ModuleName() As String
    ReadOnly Property ModuleVersion() As String
    Property ScraperEnabled() As Boolean
    
    Sub Init(ByVal sAssemblyName As String)
    Function InjectSettingsPanel() As Containers.SettingsPanel
    Function GetMovieStudio(ByRef DBMovie As Database.DBElement, ByVal sStudio As List(Of String)) As AddonResult_Data_Scraper_Movie
    Function GetSearchResults(ByRef nMovie As Database.DBElement) As AddonResult_Generic
    Function Scraper(ByRef DBMovie As Database.DBElement, ByVal ScrapeModifiers As Structures.ScrapeModifiers, ByVal ScrapeOptions As Structures.ScrapeOptions) As AddonResult_Data_Scraper_Movie
    Sub SaveSettings(ByVal DoDispose As Boolean)
    Sub ScraperOrderChanged()
End Interface
```

#### Wichtige �ÄÄnderungen:

1. **Property-Namen:** Wie bei Generic-Addons
2. **Scraper-Methode:** Hei�t jetzt nur `Scraper()` statt `Scraper_Movie()`
3. **Neue Methoden:**
   - `GetMovieStudio()` - F�r Studio-Information
   - `GetSearchResults()` - F�r Suchresultate
4. **ScraperOrderChanged()** - Muss implementiert werden

#### Beispiel-Implementation (basierend auf OMDbAPI):
```visualbasic
Public Class Addon
    Implements Interfaces.IAddon_Data_Scraper_Movie
    
    ' Properties
    ReadOnly Property ModuleName() As String Implements Interfaces.IAddon_Data_Scraper_Movie.ModuleName
        Get
            Return "IMDb_Data"
        End Get
    End Property
    
    Property ScraperEnabled() As Boolean Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled
        End Get
        Set(value As Boolean)
            _ScraperEnabled = value
        End Set
    End Property
    
    ' Methods
    Function Scraper(ByRef DBMovie As Database.DBElement, 
                     ByVal ScrapeModifiers As Structures.ScrapeModifiers, 
                     ByVal ScrapeOptions As Structures.ScrapeOptions
                     ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.Scraper
        ' Implementation hier
        Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
    End Function
    
    Function GetMovieStudio(ByRef DBMovie As Database.DBElement, 
                           ByVal sStudio As List(Of String)
                           ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.GetMovieStudio
        ' Kann leer bleiben wenn nicht unterst�tzt
        Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
    End Function
    
    Function GetSearchResults(ByRef nMovie As Database.DBElement
                             ) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_Movie.GetSearchResults
        ' Suchresultate zur�ckgeben
        Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
    End Function
    
    Sub ScraperOrderChanged() Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperOrderChanged
        If _setup IsNot Nothing Then _setup.OrderChanged()
    End Sub
End Class
```

---

### 3. Data Scrapers - TV (IAddon_Data_Scraper_TV)

**Datei:** `EmberAPI/clsInterfaces.vb` (Zeilen ~311-322)

#### Interface-Definition:
```visualbasic
Public Interface IAddon_Data_Scraper_TV
    Event AddonNeedsRestart()
    Event AddonSettingsChanged()
    Event AddonStateChanged(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer)
    
    ReadOnly Property ModuleName() As String
    ReadOnly Property ModuleVersion() As String
    Property ScraperEnabled() As Boolean
    
    Sub Init(ByVal sAssemblyName As String)
    Function InjectSettingsPanel() As Containers.SettingsPanel
    Function GetSearchResults(ByRef nShow As Database.DBElement) As AddonResult_Generic
    Function Scraper_TVEpisode(ByRef DBTVEpisode As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As AddonResult_Data_Scraper_TVEpisode
    Function Scraper_TVSeason(ByRef DBTVSeason As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As AddonResult_Data_Scraper_TVSeason
    Function Scraper_TVShow(ByRef DBTVShow As Database.DBElement, ByVal ScrapeModifiers As Structures.ScrapeModifiers, ByVal ScrapeOptions As Structures.ScrapeOptions) As AddonResult_Data_Scraper_TVShow
    Sub SaveSettings(ByVal DoDispose As Boolean)
    Sub ScraperOrderChanged()
End Interface
```

---

### 4. Image Scrapers

#### Movie (IAddon_Image_Scraper_Movie):
```visualbasic
Public Interface IAddon_Image_Scraper_Movie
    ' Events & Properties wie bei Data Scrapers
    ReadOnly Property ScraperSupportsExtraImages As Boolean
    ReadOnly Property ScraperSupportsBackdropImages As Boolean
    
    Function GetSearchResults(ByRef nMovie As Database.DBElement) As AddonResult_Generic
    Function Scraper(ByRef DBMovie As Database.DBElement, ByVal ScrapeModifiers As Structures.ScrapeModifiers) As AddonResult_Generic
    Sub ScraperOrderChanged()
End Interface
```

#### TV (IAddon_Image_Scraper_TV):
```visualbasic
Public Interface IAddon_Image_Scraper_TV
    ' Events & Properties wie bei Data Scrapers
    ReadOnly Property ScraperSupportsExtraImages As Boolean
    ReadOnly Property ScraperSupportsBackdropImages As Boolean
    
    Function GetSearchResults(ByRef nShow As Database.DBElement) As AddonResult_Generic
    Function Scraper(ByRef DBElement As Database.DBElement, ByVal ScrapeModifiers As Structures.ScrapeModifiers) As AddonResult_Generic
    Sub ScraperOrderChanged()
End Interface
```

---

### 5. Trailer Scrapers

```visualbasic
Public Interface IAddon_Trailer_Scraper_Movie
    ' Events & Properties wie �blich
    
    Function Scraper(ByRef DBMovie As Database.DBElement, ByVal Type As Enums.ModifierType, ByRef TrailerList As List(Of MediaContainers.MediaFile)) As AddonResult_Generic
    Sub ScraperOrderChanged()
End Interface
```

---

### 6. Theme Scrapers

```visualbasic
Public Interface IAddon_Theme_Scraper_Movie
    ' Events & Properties wie �blich
    
    Function Scraper(ByRef DBMovie As Database.DBElement, ByVal Type As Enums.ModifierType, ByRef ThemeList As List(Of MediaContainers.MediaFile)) As AddonResult_Generic
    Sub ScraperOrderChanged()
End Interface

Public Interface IAddon_Theme_Scraper_TV
    ' Events & Properties wie �blich
    
    Function Scraper(ByRef DBElement As Database.DBElement, ByVal Type As Enums.ModifierType, ByRef ThemeList As List(Of MediaContainers.MediaFile)) As AddonResult_Generic
    Sub ScraperOrderChanged()
End Interface
```

---

## ?? PRIORIT�T: IMDb Data Scraper Migration

### Notwendige Schritte f�r scraper.data.imdb.com

**Dateien zu �ndern:**
1. `Addons/scraper.data.imdb.com/clsAddon.vb` - Hauptklasse

**�ÄÄnderungen:**

```visualbasic
' 1. Properties umbenennen
Name ? ModuleName
Version ? ModuleVersion
Enabled ? ScraperEnabled (f�r Movie UND TV)

' 2. Init-Signatur anpassen
Sub Init(sAssemblyName As String, sExecutable As String) ? Sub Init(sAssemblyName As String)

' 3. Scraper-Methoden umbenennen/hinzuf�gen
Scraper_Movie(...) ? Scraper(...) ' Ohne _Movie Suffix
' Neue Methoden hinzuf�gen:
Function GetMovieStudio(...) As AddonResult_Data_Scraper_Movie
Function GetSearchResults(...) As AddonResult_Generic ' F�r Movie UND TV

' 4. ScraperOrderChanged implementieren
Sub ScraperOrderChanged() Implements IAddon_Data_Scraper_Movie.ScraperOrderChanged, IAddon_Data_Scraper_TV.ScraperOrderChanged
    If _setup_Movie IsNot Nothing Then _setup_Movie.OrderChanged()
    If _setup_TV IsNot Nothing Then _setup_TV.OrderChanged()
End Sub
```

**Vorlage basierend auf OMDbAPI:**
Siehe `Addons/scraper.data.omdbapi.com/clsAddon.vb` als funktionierendes Beispiel.

---

## ?? Deaktivierung nicht-ben�tigter Addons

### Empfohlene Deaktivierung (w�hrend Migration):

Alle Addons au�er IMDb k�nnen tempor�r deaktiviert werden durch:

**Option 1: Aus Solution entfernen**
- Rechtsklick auf Projekt in Solution Explorer ? "Unload Project"

**Option 2: Build ausschlie�en**
- In Configuration Manager: H�kchen bei "Build" entfernen

**Option 3: Conditional Compilation**
- In Projekteinstellungen: Custom Constants hinzuf�gen

### Zu behalten (aktiv):
1. ? EmberAPI (Core)
2. ? EmberMediaManager (Core)
3. ? scraper.data.imdb.com (Priorit�t)
4. ? core.contextmenu (bereits erfolgreich migriert - kann als Referenz dienen)

---

## ?? Bekannte Probleme & L�sungen

### Problem 1: "Addons.Data_Scrapers_Movie" nicht gefunden

**Fehler:**
```
error BC30456: "Data_Scrapers_Movie" ist kein Member von "Addons"
```

**Ursache:**
- Settings-Holder Formulare (`fürmSettingsHolder_Movie.vb`, `fürmSettingsHolder_TV.vb`) verwenden zentrale Scraper-Listen
- Diese Listen existieren im `active_development` Branch noch nicht oder wurden umbenannt

**L�sung:**
- Pre-existing Issue - nicht Teil der Interface-Migration
- Kann ignoriert werden, solange die Core-Interface-Implementation korrekt ist
- Muss in separatem Bugfix behandelt werden

### Problem 2: Nested Type "AddonSettings" vs "Settings"

**Fehler:**
```
error BC30002: Der Typ "Addon.AddonSettings" ist nicht definiert
```

**L�sung:**
```visualbasic
' In clsScraper.vb oder anderen Helper-Klassen:
' VORHER:
Private _addonSettings As Addon.AddonSettings

' NACHHER:
Private _addonSettings As Addon.Settings
```

### Problem 3: ScraperOrderChanged mit Movie/TV Suffix

**Fehler:**
```
error BC30401: "ScraperOrderChanged_Movie" kann nicht implementiert werden
```

**L�sung:**
```visualbasic
' FALSCH:
Public Sub ScraperOrderChanged_Movie() Implements IAddon_Data_Scraper_Movie.ScraperOrderChanged
Public Sub ScraperOrderChanged_TV() Implements IAddon_Data_Scraper_TV.ScraperOrderChanged

' RICHTIG (kombiniert):
Public Sub ScraperOrderChanged() Implements IAddon_Data_Scraper_Movie.ScraperOrderChanged, IAddon_Data_Scraper_TV.ScraperOrderChanged
    If _setup_Movie IsNot Nothing Then _setup_Movie.OrderChanged()
    If _setup_TV IsNot Nothing Then _setup_TV.OrderChanged()
End Sub
```

---

## ? Erfolgs-Kriterien

Ein Addon ist erfolgreich migriert wenn:

1. ? Keine Compiler-Errors bzgl. Interface-Implementation
2. ? Build erfolgreich (ohne Interface-Fehler)
3. ? Alle Interface-Members implementiert
4. ? Property-Namen entsprechen neuer Konvention
5. ? ScraperOrderChanged() implementiert

**Hinweis:** Settings-Holder Fehler (`Addons.Data_Scrapers_*`) sind Pre-existing und kein Teil dieser Migration.

---

## ?? Migrations-Checkliste f�r IMDb

- [ ] `clsAddon.vb` �öffnen
- [ ] Properties umbenennen (Name?ModuleName, Version?ModuleVersion, Enabled?ScraperEnabled)
- [ ] Init-Signatur anpassen (2 Parameter ? 1 Parameter)
- [ ] Scraper_Movie() ? Scraper() umbenennen
- [ ] GetMovieStudio() hinzuf�gen (kann NoResult zur�ckgeben)
- [ ] GetSearchResults() f�r Movie hinzuf�gen
- [ ] GetSearchResults() f�r TV hinzuf�gen
- [ ] ScraperOrderChanged() implementieren (kombiniert f�r Movie & TV)
- [ ] Scraper_TVEpisode() Signatur pr�fen
- [ ] Scraper_TVSeason() Signatur pr�fen
- [ ] Scraper_TVShow() Signatur pr�fen
- [ ] Build testen
- [ ] Compiler-Errors beheben (au�er Settings-Holder Pre-existing Issues)

---

## ?? Referenzen

**Erfolgreich migrierte Beispiele:**
- `Addons/core.contextmenu/clsAddon.vb` - Generic Addon
- `Addons/scraper.data.omdbapi.com/clsAddon.vb` - Data Scraper (Movie & TV)

**Interface-Definitionen:**
- `EmberAPI/clsInterfaces.vb` (Zeilen 270-420)

**Build-Verifikation:**
```powershell
# Generic Addon testen:
msbuild Addons/core.contextmenu/core.contextmenu.vbproj /p:Configuration=Debug /p:Platform=x64 /t:Rebuild

# Data Scraper testen:
msbuild Addons/scraper.data.imdb.com/scraper.data.imdb.com.vbproj /p:Configuration=Debug /p:Platform=x64 /t:Rebuild
```

---

## ?? Kontakt / Weitere Informationen

Diese Migration basiert auf der Analyse des `active_development` Branch.
Bei füragen zur Interface-Struktur siehe `EmberAPI/clsInterfaces.vb`.

**N�chster Schritt:** Migration von `scraper.data.imdb.com` durchf�hren.
