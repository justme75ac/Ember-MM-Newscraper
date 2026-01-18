# Addon Deaktivierungs-Strategie

## ?? Ziel

Nur **IMDb Data Scraper** aktiv lassen, alle anderen Addons temporär deaktivieren für schnelleren Build und fokussierte Migration.

---

## ? AKTIV HALTEN

Diese Projekte **MÜSSEN** aktiv bleiben:

1. **EmberAPI.vbproj** - Core API Library
2. **EmberMediaManager.vbproj** - Hauptanwendung
3. **scraper.data.imdb.com.vbproj** - IMDb Data Scraper (PRIORITÄT)
4. **core.contextmenu.vbproj** - ? Bereits erfolgreich migriert (als Referenz)

---

## ?? ZU DEAKTIVIEREN

### Methode: Projekte aus Solution entfernen (empfohlen)

**In Visual Studio:**
1. Solution Explorer öffnen
2. Rechtsklick auf Projekt ? **"Unload Project"**
3. Projekt wird ausgegraut und nicht mehr gebaut

**Oder über Solution-Datei bearbeiten:**
Öffne `Ember Media Manager.sln` in einem Texteditor und kommentiere Projekt-Einträge aus:

```
# VORHER:
Project("{Projekt-GUID}") = "scraper.data.thetvdb.com", "Addons\scraper.data.thetvdb.com\scraper.data.thetvdb.com.vbproj", "{Projekt-GUID}"
EndProject

# NACHHER (auskommentiert):
#Project("{Projekt-GUID}") = "scraper.data.thetvdb.com", "Addons\scraper.data.thetvdb.com\scraper.data.thetvdb.com.vbproj", "{Projekt-GUID}"
#EndProject
```

---

## ?? Liste der zu deaktivierenden Projekte

### Generic-Addons (können später reaktiviert werden)
Bereits migriert, können erstmal deaktiviert bleiben:

- [ ] core.audiovideocodecmapping.vbproj
- [ ] core.videosourcemapping.vbproj
- [ ] core.globalmapping.vbproj
- [ ] core.filemanager.vbproj
- [ ] core.medialisteditor.vbproj
- [ ] core.renamer.vbproj
- [ ] addon.websitecreator.vbproj
- [ ] addon.trakt.tv.vbproj
- [ ] addon.kodiinterface.vbproj (hat Probleme, siehe Doku)

### Data Scrapers (alle außer IMDb)
- [ ] scraper.data.thetvdb.com.vbproj
- [ ] scraper.data.moviepilot.de.vbproj
- [ ] scraper.data.ofdb.de.vbproj
- [ ] scraper.data.omdbapi.com.vbproj (bereits migriert, kann als Referenz dienen)
- [ ] scraper.data.trakt.tv.vbproj
- [ ] scraper.data.thetvdb.com_old.vbproj

### Image Scrapers
- [ ] scraper.image.fanart.tv.vbproj
- [ ] scraper.image.themoviedb.org.vbproj
- [ ] scraper.image.thetvdb.com_old.vbproj

### Trailer Scrapers
- [ ] scraper.trailer.youtube.com.vbproj
- [ ] scraper.trailer.videobuster.de.vbproj
- [ ] scraper.trailer.themoviedb.org.vbproj
- [ ] scraper.trailer.apple.com.vbproj
- [ ] scraper.trailer.davestrailerpage.co.uk.vbproj

### Theme Scrapers
- [ ] scraper.theme.youtube.com.vbproj
- [ ] scraper.theme.televisiontunes.com.vbproj

### Special Addons
- [ ] addon.themoviedb.org.vbproj (kombiniert mehrere Interfaces)

---

## ?? Workflow

### Phase 1: IMDb Migration (AKTUELL)
```
AKTIV:
? EmberAPI
? EmberMediaManager  
? scraper.data.imdb.com (MIGRATION IN PROGRESS)
? core.contextmenu (Referenz)

DEAKTIVIERT:
?? Alle anderen Addons
```

### Phase 2: Nach erfolgreicher IMDb-Migration
```
AKTIV:
? EmberAPI
? EmberMediaManager
? scraper.data.imdb.com (FERTIG)
? scraper.data.omdbapi.com (als Vorlage für weitere Migrationen)
? [Nächstes Addon zur Migration]

DEAKTIVIERT:
?? Alle nicht benötigten Addons
```

---

## ??? PowerShell Script zur Deaktivierung

**Optional - Backup erstellen und Projekte auskommentieren:**

```powershell
# Solution-Datei Backup erstellen
$solutionFile = "C:\GitHub\Ember-MM-Newscraper\Ember Media Manager.sln"
Copy-Item $solutionFile "$solutionFile.backup"

# Projekte zum Deaktivieren (Beispiel)
$projectsToDisable = @(
    "scraper.data.thetvdb.com",
    "scraper.data.moviepilot.de",
    "scraper.data.ofdb.de",
    "scraper.data.trakt.tv",
    "scraper.image.fanart.tv",
    "scraper.image.themoviedb.org",
    "scraper.trailer.youtube.com",
    "scraper.theme.youtube.com"
)

# Solution-Datei lesen
$content = Get-Content $solutionFile -Raw

# Projekte auskommentieren
foreach ($project in $projectsToDisable) {
    $content = $content -replace "(?m)^(Project.*$project.*\r?\n)", "#`$1"
    $content = $content -replace "(?m)^(EndProject.*\r?\n)((?!#))", "#`$1"
}

# Speichern
Set-Content $solutionFile $content -NoNewline

Write-Host "? Projekte deaktiviert. Backup: $solutionFile.backup"
```

---

## ?? Build-Zeit Verbesserung

**Vorher (alle Projekte):**
- ~30+ Projekte
- Build-Zeit: ~5-10 Minuten
- Viele Interface-Fehler

**Nachher (nur Core + IMDb):**
- ~4 Projekte
- Build-Zeit: ~30-60 Sekunden
- Fokus auf einen Scraper

---

## ?? Wichtige Hinweise

### 1. Keine Dateien löschen!
- Projekte nur **"Unload"**, nicht **"Remove"**
- Alle Dateien bleiben im Filesystem
- Leicht reversibel

### 2. KodiAPI behalten
```
KodiAPI.csproj sollte AKTIV bleiben
```
- Wird von addon.kodiinterface benötigt
- Ist C# Projekt (keine VB Interface-Probleme)
- Build-Zeit minimal

### 3. EmberAPI.vbproj niemals deaktivieren
```
EmberAPI.vbproj muss IMMER aktiv sein
```
- Enthält alle Interface-Definitionen
- Core-Library für alle Addons
- Ohne EmberAPI funktioniert nichts

---

## ?? Reaktivierung

**Um Projekte später wieder zu aktivieren:**

### In Visual Studio:
1. Solution Explorer öffnen
2. "Show All Files" aktivieren
3. Rechtsklick auf ausgegraut Projekt ? **"Reload Project"**

### Über Solution-Datei:
1. `Ember Media Manager.sln` öffnen
2. Kommentar-Zeichen (#) entfernen
3. Solution neu laden

### Über PowerShell (Backup wiederherstellen):
```powershell
Copy-Item "C:\GitHub\Ember-MM-Newscraper\Ember Media Manager.sln.backup" `
          "C:\GitHub\Ember-MM-Newscraper\Ember Media Manager.sln" -Force
```

---

## ? Verifikation

**Nach Deaktivierung prüfen:**

```powershell
# Nur aktive Projekte bauen
cd C:\GitHub\Ember-MM-Newscraper
msbuild "Ember Media Manager.sln" /p:Configuration=Debug /p:Platform=x64 /t:Rebuild /v:minimal
```

**Erwartung:**
- ? EmberAPI.dll wird gebaut
- ? EmberMediaManager.exe wird gebaut
- ? scraper.data.imdb.com.dll wird gebaut
- ? core.contextmenu.dll wird gebaut
- ? Alle anderen Addons werden übersprungen

---

## ?? Status-Tracking

### Deaktivierte Projekte
Markiere deaktivierte Projekte:

- [ ] Generic-Addons (8 Projekte)
- [ ] Data Scrapers außer IMDb (6 Projekte)  
- [ ] Image Scrapers (3 Projekte)
- [ ] Trailer Scrapers (5 Projekte)
- [ ] Theme Scrapers (2 Projekte)
- [ ] Special Addons (1 Projekt)

**Total:** ~25 Projekte temporär deaktiviert

### Aktive Projekte
- [x] EmberAPI
- [x] EmberMediaManager
- [x] scraper.data.imdb.com
- [x] core.contextmenu (Referenz)
- [x] KodiAPI (optional, minimal)

**Total:** 4-5 Projekte aktiv

---

## ?? Nächste Schritte

1. ? Dokumentation gelesen
2. ?? Nicht-benötigte Projekte deaktivieren
3. ?? IMDb Migration durchführen (siehe `IMDB_MIGRATION_GUIDE.md`)
4. ?? Build testen
5. ?? Bei Erfolg: Nächstes Addon aktivieren und migrieren

**Aktueller Fokus:** scraper.data.imdb.com Migration
