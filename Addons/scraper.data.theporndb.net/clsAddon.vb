' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports EmberAPI
Imports NLog
Imports System.Diagnostics

Public Class Addon
    Implements Interfaces.IAddon_Data_Scraper_Movie


#Region "Fields"

    Shared _Logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeOptions_Movie As New Structures.ScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers

    Private _SpecialSettings_Movie As New Settings
    Private _Name As String = "TPDB_Data"
    Private _ScraperEnabled_Movie As Boolean = False
    Private _setup_Movie As frmSettingsHolder_Movie
    Private _TPDbAPI_Movie As New Scraper
    Private _AddonSettings As New AddonSettings

#End Region 'Fields

#Region "Events"

    'Movie part
    Public Event ModuleSettingsChanged_Movie() Implements Interfaces.IAddon_Data_Scraper_Movie.AddonSettingsChanged
    Public Event ScraperSetupChanged_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.IAddon_Data_Scraper_Movie.AddonStateChanged
    Public Event SetupNeedsRestart_Movie() Implements Interfaces.IAddon_Data_Scraper_Movie.AddonNeedsRestart

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleName() As String Implements Interfaces.IAddon_Data_Scraper_Movie.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.IAddon_Data_Scraper_Movie.ModuleVersion
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
            If _ScraperEnabled_Movie Then
                _TPDbAPI_Movie.CreateAPI(_SpecialSettings_Movie)
            End If
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Private Sub Handle_ModuleSettingsChanged_Movie()
        RaiseEvent ModuleSettingsChanged_Movie()
    End Sub

    Private Sub Handle_SetupNeedsRestart_Movie()
        RaiseEvent SetupNeedsRestart_Movie()
    End Sub

    Private Sub Handle_SetupScraperChanged_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Movie = state
        RaiseEvent ScraperSetupChanged_Movie(String.Concat(_Name, "_Movie"), state, difforder)
    End Sub

    Sub Init_Movie(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Data_Scraper_Movie.Init
        _AssemblyName = sAssemblyName
        LoadSettings_Movie()
    End Sub

    Function InjectSetupScraper_Movie() As Containers.SettingsPanel Implements Interfaces.IAddon_Data_Scraper_Movie.InjectSettingsPanel
        Dim SPanel As New Containers.SettingsPanel
        _setup_Movie = New frmSettingsHolder_Movie
        LoadSettings_Movie()
        _setup_Movie.chkEnabled.Checked = _ScraperEnabled_Movie
        _setup_Movie.txtApiKey.Text = _SpecialSettings_Movie.APIKey

        _setup_Movie.OrderChanged()

        SPanel.UniqueName = String.Concat(_Name, "_Movie")
        SPanel.Title = "ThePornDB"
        SPanel.Order = 110
        SPanel.Parent = "pnlMovieData"
        SPanel.Type = Master.eLang.GetString(36, "Movies")
        SPanel.ImageIndex = If(_ScraperEnabled_Movie, 9, 10)
        SPanel.Panel = _setup_Movie.pnlSettings

        AddHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
        AddHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
        AddHandler _setup_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_Movie
        Return SPanel
    End Function

    Sub LoadSettings_Movie()
        _SpecialSettings_Movie.APIKey = _AddonSettings.GetStringSetting("APIKey", String.Empty, , Enums.ContentType.Movie)
    End Sub

    Sub SaveSettings_Movie()
        _AddonSettings.SetStringSetting("APIKey", _SpecialSettings_Movie.APIKey, , , Enums.ContentType.Movie)
    End Sub

    Sub SaveSetupScraper_Movie(ByVal DoDispose As Boolean) Implements Interfaces.IAddon_Data_Scraper_Movie.SaveSettings
        Dim bAPIKeyChanged = Not _SpecialSettings_Movie.APIKey = _setup_Movie.txtApiKey.Text.Trim
        _SpecialSettings_Movie.APIKey = _setup_Movie.txtApiKey.Text.Trim

        SaveSettings_Movie()

        If bAPIKeyChanged Then _TPDbAPI_Movie.CreateAPI(_SpecialSettings_Movie)

        If DoDispose Then
            RemoveHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
            RemoveHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
            RemoveHandler _setup_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_Movie
            _setup_Movie.Dispose()
        End If
    End Sub

    Function Scraper(ByRef DBMovie As Database.DBElement,
                     ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                     ByVal ScrapeOptions As Structures.ScrapeOptions
                     ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.Scraper
        _Logger.Trace("[ThePornDB_Data] [Scraper] [Start]")
        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_Movie)
        Dim Result As MediaContainers.Movie = Nothing

        If Not _TPDbAPI_Movie.IsClientCreated Then
            _Logger.Error("[ThePornDB_Data] [Scraper] [Abort] Can't create API client (API key missing?)")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
        End If

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch Then
            ' Try to get movie by TPDb ID if available
            If DBMovie.Movie.UniqueIDs.TPDbIdSpecified Then
                Result = _TPDbAPI_Movie.GetInfo_Movie(DBMovie.Movie.UniqueIDs.TPDbId, FilteredOptions)
            ElseIf DBMovie.Movie.UniqueIDs.IMDbIdSpecified Then
                ' Try to search by IMDb ID
                Dim searchResults = _TPDbAPI_Movie.Search_Movie(DBMovie.Movie.Title, DBMovie.Movie.UniqueIDs.IMDbId, FilteredOptions)
                If searchResults IsNot Nothing AndAlso searchResults.Count > 0 Then
                    Result = _TPDbAPI_Movie.GetInfo_Movie(searchResults(0).UniqueIDs.TPDbId, FilteredOptions)
                End If
            ElseIf Not String.IsNullOrEmpty(DBMovie.Movie.Title) Then
                ' Search by title
                Dim searchResults = _TPDbAPI_Movie.Search_Movie(DBMovie.Movie.Title, String.Empty, FilteredOptions)
                If searchResults IsNot Nothing AndAlso searchResults.Count > 0 Then
                    Result = _TPDbAPI_Movie.GetInfo_Movie(searchResults(0).UniqueIDs.TPDbId, FilteredOptions)
                End If
            End If
        ElseIf ScrapeModifiers.DoSearch Then
            ' Return search results
            Dim searchResults = _TPDbAPI_Movie.Search_Movie(DBMovie.Movie.Title, String.Empty, FilteredOptions)
            If searchResults IsNot Nothing AndAlso searchResults.Count > 0 Then
                Return New Interfaces.AddonResult_Data_Scraper_Movie(searchResults)
            End If
        End If

        If Result IsNot Nothing Then
            _Logger.Trace("[ThePornDB_Data] [Scraper] [Done]")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Result)
        Else
            _Logger.Trace("[ThePornDB_Data] [Scraper] [Abort] No result found")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
        End If
    End Function

    Function GetMovieStudio(ByRef DBMovie As Database.DBElement, ByVal sStudio As List(Of String)) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.GetMovieStudio
        Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
    End Function

    Function GetSearchResults_Movie(ByRef nMovie As Database.DBElement) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_Movie.GetSearchResults
        If Not _TPDbAPI_Movie.IsClientCreated Then
            Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
        End If

        Dim searchResults = _TPDbAPI_Movie.Search_Movie(nMovie.Movie.Title, String.Empty, ConfigScrapeOptions_Movie)
        If searchResults IsNot Nothing AndAlso searchResults.Count > 0 Then
            Return New Interfaces.AddonResult_Generic(searchResults)
        End If

        Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
    End Function

    Public Sub ScraperOrderChanged() Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperOrderChanged
        If _setup_Movie IsNot Nothing Then _setup_Movie.OrderChanged()
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Class Settings

#Region "Properties"

        Public Property APIKey As String = String.Empty

#End Region 'Properties

    End Class

#End Region 'Nested Types

End Class
