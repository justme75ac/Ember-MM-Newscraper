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

Public Class Addon
    Implements Interfaces.IAddon_Data_Scraper_Movie
    Implements Interfaces.IAddon_Data_Scraper_TV


#Region "Fields"

    Shared _Logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeOptions_Movie As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_TV As New Structures.ScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_TV As New Structures.ScrapeModifiers

    Private _SpecialSettings_Movie As New Settings
    Private _SpecialSettings_TV As New Settings
    Private _SpecialSettings_TVEpisode As New Settings
    Private _Name As String = "OMDb_Data"
    Private _ScraperEnabled_Movie As Boolean = False
    Private _ScraperEnabled_TV As Boolean = False
    Private _setup_Movie As frmSettingsHolder_Movie
    Private _setup_TV As frmSettingsHolder_TV
    Private _OMDbAPI_Movie As New Scraper
    Private _OMDbAPI_TV As New Scraper

#End Region 'Fields

#Region "Events"

    'Movie part
    Public Event ModuleSettingsChanged_Movie() Implements Interfaces.IAddon_Data_Scraper_Movie.AddonSettingsChanged
    Public Event ScraperSetupChanged_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.IAddon_Data_Scraper_Movie.AddonStateChanged
    Public Event SetupNeedsRestart_Movie() Implements Interfaces.IAddon_Data_Scraper_Movie.AddonNeedsRestart

    'TV part
    Public Event ModuleSettingsChanged_TV() Implements Interfaces.IAddon_Data_Scraper_TV.AddonSettingsChanged
    Public Event ScraperSetupChanged_TV(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.IAddon_Data_Scraper_TV.AddonStateChanged
    Public Event SetupNeedsRestart_TV() Implements Interfaces.IAddon_Data_Scraper_TV.AddonNeedsRestart

#End Region 'Events

#Region "Properties"

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
            If _ScraperEnabled_Movie Then
                _OMDbAPI_Movie.CreateAPI(_SpecialSettings_Movie)
            End If
        End Set
    End Property

    Property ScraperEnabled_TV() As Boolean Implements Interfaces.IAddon_Data_Scraper_TV.ScraperEnabled
        Get
            Return _ScraperEnabled_TV
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_TV = value
            If _ScraperEnabled_TV Then
                _OMDbAPI_TV.CreateAPI(_SpecialSettings_TV)
            End If
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Private Sub Handle_ModuleSettingsChanged_Movie()
        RaiseEvent ModuleSettingsChanged_Movie()
    End Sub

    Private Sub Handle_ModuleSettingsChanged_TV()
        RaiseEvent ModuleSettingsChanged_TV()
    End Sub

    Private Sub Handle_SetupNeedsRestart_Movie()
        RaiseEvent SetupNeedsRestart_Movie()
    End Sub

    Private Sub Handle_SetupNeedsRestart_TV()
        RaiseEvent SetupNeedsRestart_TV()
    End Sub

    Private Sub Handle_SetupScraperChanged_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Movie = state
        RaiseEvent ScraperSetupChanged_Movie(String.Concat(_Name, "_Movie"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_TV(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_TV = state
        RaiseEvent ScraperSetupChanged_TV(String.Concat(_Name, "_TV"), state, difforder)
    End Sub

    Sub Init_Movie(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Data_Scraper_Movie.Init
        _AssemblyName = sAssemblyName
        LoadSettings_Movie()
    End Sub

    Sub Init_TV(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Data_Scraper_TV.Init
        _AssemblyName = sAssemblyName
        LoadSettings_TV()
    End Sub

    Function InjectSetupScraper_Movie() As Containers.SettingsPanel Implements Interfaces.IAddon_Data_Scraper_Movie.InjectSettingsPanel
        Dim SPanel As New Containers.SettingsPanel
        _setup_Movie = New frmSettingsHolder_Movie
        LoadSettings_Movie()
        _setup_Movie.chkEnabled.Checked = _ScraperEnabled_Movie
        _setup_Movie.chkIMDb.Checked = _SpecialSettings_Movie.IMDb
        _setup_Movie.chkMetascore.Checked = _SpecialSettings_Movie.Metascore
        _setup_Movie.chkTomatometer.Checked = _SpecialSettings_Movie.Tomatometer
        _setup_Movie.txtApiKey.Text = _SpecialSettings_Movie.APIKey

        _setup_Movie.OrderChanged()

        SPanel.UniqueName = String.Concat(_Name, "_Movie")
        SPanel.Title = "OMDbAPI.com"
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

    Function InjectSetupScraper_TV() As Containers.SettingsPanel Implements Interfaces.IAddon_Data_Scraper_TV.InjectSettingsPanel
        Dim SPanel As New Containers.SettingsPanel
        _setup_TV = New frmSettingsHolder_TV
        LoadSettings_TV()
        _setup_TV.chkEnabled.Checked = _ScraperEnabled_TV
        _setup_TV.chkEpisodeIMDb.Checked = _SpecialSettings_TVEpisode.IMDb
        _setup_TV.chkShowIMDb.Checked = _SpecialSettings_TV.IMDb
        _setup_TV.txtApiKey.Text = _SpecialSettings_TV.APIKey

        _setup_TV.OrderChanged()

        SPanel.UniqueName = String.Concat(_Name, "_TV")
        SPanel.Title = "OMDbAPI.com"
        SPanel.Order = 110
        SPanel.Type = Master.eLang.GetString(653, "TV Shows")
        SPanel.ImageIndex = If(_ScraperEnabled_TV, 9, 10)
        SPanel.Panel = _setup_TV.pnlSettings

        AddHandler _setup_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_TV
        AddHandler _setup_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_TV
        AddHandler _setup_TV.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_TV
        Return SPanel
    End Function

    Sub LoadSettings_Movie()
        _SpecialSettings_Movie.APIKey = _AddonSettings.GetStringSetting("APIKey", String.Empty, , Enums.ContentType.Movie)
        _SpecialSettings_Movie.IMDb = _AddonSettings.GetBooleanSetting("IMDb", False, , Enums.ContentType.Movie)
        _SpecialSettings_Movie.Metascore = _AddonSettings.GetBooleanSetting("Metascore", False, , Enums.ContentType.Movie)
        _SpecialSettings_Movie.Tomatometer = _AddonSettings.GetBooleanSetting("Tomatometer", False, , Enums.ContentType.Movie)

        ConfigScrapeOptions_Movie.bMainRating = _SpecialSettings_Movie.AnyRatingEnabled
    End Sub

    Sub LoadSettings_TV()
        _SpecialSettings_TV.APIKey = _AddonSettings.GetStringSetting("APIKey", String.Empty, , Enums.ContentType.TV)
        _SpecialSettings_TVEpisode.IMDb = _AddonSettings.GetBooleanSetting("IMDb", False, , Enums.ContentType.TVEpisode)
        _SpecialSettings_TV.IMDb = _AddonSettings.GetBooleanSetting("IMDb", False, , Enums.ContentType.TVShow)

        ConfigScrapeOptions_TV.bMainRating = _SpecialSettings_TV.AnyRatingEnabled
    End Sub

    Sub SaveSettings_Movie()
        _AddonSettings.SetStringSetting("APIKey", _SpecialSettings_Movie.APIKey, , , Enums.ContentType.Movie)
        _AddonSettings.SetBooleanSetting("IMDb", _SpecialSettings_Movie.IMDb, , , Enums.ContentType.Movie)
        _AddonSettings.SetBooleanSetting("Metascore", _SpecialSettings_Movie.Metascore, , , Enums.ContentType.Movie)
        _AddonSettings.SetBooleanSetting("Tomatometer", _SpecialSettings_Movie.Tomatometer, , , Enums.ContentType.Movie)
    End Sub

    Sub SaveSettings_TV()
        _AddonSettings.SetStringSetting("APIKey", _SpecialSettings_TV.APIKey, , , Enums.ContentType.TV)
        _AddonSettings.SetBooleanSetting("IMDb", _SpecialSettings_TVEpisode.IMDb, , , Enums.ContentType.TVEpisode)
        _AddonSettings.SetBooleanSetting("IMDb", _SpecialSettings_TV.IMDb, , , Enums.ContentType.TVShow)
    End Sub

    Sub SaveSetupScraper_Movie(ByVal DoDispose As Boolean) Implements Interfaces.IAddon_Data_Scraper_Movie.SaveSettings
        Dim bAPIKeyChanged = Not _SpecialSettings_Movie.APIKey = _setup_Movie.txtApiKey.Text.Trim
        _SpecialSettings_Movie.APIKey = _setup_Movie.txtApiKey.Text.Trim
        _SpecialSettings_Movie.IMDb = _setup_Movie.chkIMDb.Checked
        _SpecialSettings_Movie.Metascore = _setup_Movie.chkMetascore.Checked
        _SpecialSettings_Movie.Tomatometer = _setup_Movie.chkTomatometer.Checked

        ConfigScrapeOptions_Movie.bMainRating = _SpecialSettings_Movie.AnyRatingEnabled

        SaveSettings_Movie()

        If bAPIKeyChanged Then _OMDbAPI_Movie.CreateAPI(_SpecialSettings_Movie)

        If DoDispose Then
            RemoveHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
            RemoveHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
            _setup_Movie.Dispose()
        End If
    End Sub

    Sub SaveSetupScraper_TV(ByVal DoDispose As Boolean) Implements Interfaces.IAddon_Data_Scraper_TV.SaveSettings
        Dim bAPIKeyChanged = Not _SpecialSettings_TV.APIKey = _setup_TV.txtApiKey.Text.Trim
        _SpecialSettings_TV.APIKey = _setup_TV.txtApiKey.Text.Trim
        _SpecialSettings_TV.IMDb = _setup_TV.chkShowIMDb.Checked
        _SpecialSettings_TVEpisode.IMDb = _setup_TV.chkEpisodeIMDb.Checked

        ConfigScrapeOptions_TV.bMainRating = _SpecialSettings_TV.AnyRatingEnabled

        SaveSettings_TV()

        If bAPIKeyChanged Then _OMDbAPI_TV.CreateAPI(_SpecialSettings_TV)

        If DoDispose Then
            RemoveHandler _setup_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_TV
            RemoveHandler _setup_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_TV
            _setup_TV.Dispose()
        End If
    End Sub

    Function Scraper(ByRef DBMovie As Database.DBElement,
                     ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                     ByVal ScrapeOptions As Structures.ScrapeOptions
                     ) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.Scraper
        _Logger.Trace("[OMDbApi.com_Data] [Scraper] [Start]")
        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_Movie)
        Dim Result As MediaContainers.Movie = Nothing

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch AndAlso _OMDbAPI_Movie.IsClientCreated Then
            If DBMovie.Movie.UniqueIDs.IMDbIdSpecified Then
                Dim nRatings = _OMDbAPI_Movie.GetRatingsByImbId(DBMovie.Movie.UniqueIDs.IMDbId, DBMovie.ContentType, FilteredOptions)
                If nRatings IsNot Nothing Then
                    Result = New MediaContainers.Movie With {.Ratings = nRatings}
                End If
            Else
                _Logger.Trace("[OMDbApi.com_Data] [Scraper] [Abort] No IMDb ID available")
                Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
            End If
        ElseIf Not _OMDbAPI_Movie.IsClientCreated Then
            _Logger.Error("[OMDbApi.com_Data] [Scraper] [Abort] Can't create API client (API key missing?)")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
        End If

        If Result IsNot Nothing Then
            _Logger.Trace("[OMDbApi.com_Data] [Scraper] [Done]")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Result)
        Else
            _Logger.Trace("[OMDbApi.com_Data] [Scraper] [Abort] No result found")
            Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
        End If
    End Function

    Function GetMovieStudio(ByRef DBMovie As Database.DBElement, ByVal sStudio As List(Of String)) As Interfaces.AddonResult_Data_Scraper_Movie Implements Interfaces.IAddon_Data_Scraper_Movie.GetMovieStudio
        Return New Interfaces.AddonResult_Data_Scraper_Movie(Interfaces.ResultStatus.NoResult)
    End Function

    Function GetSearchResults_Movie(ByRef nMovie As Database.DBElement) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_Movie.GetSearchResults
        Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
    End Function

    Function Scraper_TVShow(ByRef DBTVShow As Database.DBElement,
                            ByVal ScrapeModifiers As Structures.ScrapeModifiers,
                            ByVal ScrapeOptions As Structures.ScrapeOptions
                            ) As Interfaces.AddonResult_Data_Scraper_TVShow Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVShow
        _Logger.Trace("[OMDbApi.com_Data] [Scraper_TVShow] [Start]")
        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)
        Dim Result As MediaContainers.TVShow = Nothing

        If ScrapeModifiers.MainNFO AndAlso Not ScrapeModifiers.DoSearch AndAlso _OMDbAPI_TV.IsClientCreated Then
            If DBTVShow.TVShow.UniqueIDs.IMDbIdSpecified Then
                Dim nRatings = _OMDbAPI_TV.GetRatingsByImbId(DBTVShow.TVShow.UniqueIDs.IMDbId, DBTVShow.ContentType, FilteredOptions)
                If nRatings IsNot Nothing Then
                    Result = New MediaContainers.TVShow With {.Ratings = nRatings}
                End If
            Else
                _Logger.Trace("[OMDbApi.com_Data] [Scraper_TVShow] [Abort] No IMDb ID available")
                Return New Interfaces.AddonResult_Data_Scraper_TVShow(Interfaces.ResultStatus.NoResult)
            End If
        ElseIf Not _OMDbAPI_TV.IsClientCreated Then
            _Logger.Error("[OMDbApi.com_Data] [Scraper_TVShow] [Abort] Can't create API client (API key missing?)")
            Return New Interfaces.AddonResult_Data_Scraper_TVShow(Interfaces.ResultStatus.NoResult)
        End If

        If Result IsNot Nothing Then
            _Logger.Trace("[OMDbApi.com_Data] [Scraper_TVShow] [Done]")
            Return New Interfaces.AddonResult_Data_Scraper_TVShow(Result)
        Else
            _Logger.Trace("[OMDbApi.com_Data] [Scraper_TVShow] [Abort] No result found")
            Return New Interfaces.AddonResult_Data_Scraper_TVShow(Interfaces.ResultStatus.NoResult)
        End If
    End Function

    Function GetSearchResults_TV(ByRef nShow As Database.DBElement) As Interfaces.AddonResult_Generic Implements Interfaces.IAddon_Data_Scraper_TV.GetSearchResults
        Return New Interfaces.AddonResult_Generic(Interfaces.ResultStatus.NoResult)
    End Function

    Public Function Scraper_TVEpisode(ByRef DBTVEpisode As Database.DBElement,
                                      ByVal ScrapeOptions As Structures.ScrapeOptions
                                      ) As Interfaces.AddonResult_Data_Scraper_TVEpisode Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVEpisode
        _Logger.Trace("[OMDb_Data] [Scraper_TVEpisode] [Start]")
        Dim Result As MediaContainers.EpisodeDetails = Nothing

        If Result IsNot Nothing Then
            _Logger.Trace("[OMDb_Data] [Scraper_TVEpisode] [Done]")
            Return New Interfaces.AddonResult_Data_Scraper_TVEpisode(Result)
        Else
            _Logger.Trace("[OMDb_Data] [Scraper_TVEpisode] [Abort] No result found")
            Return New Interfaces.AddonResult_Data_Scraper_TVEpisode(Interfaces.ResultStatus.NoResult)
        End If
    End Function

    Public Function Scraper_TVSeason(ByRef DBTVSeason As Database.DBElement,
                                     ByVal ScrapeOptions As Structures.ScrapeOptions
                                     ) As Interfaces.AddonResult_Data_Scraper_TVSeason Implements Interfaces.IAddon_Data_Scraper_TV.Scraper_TVSeason
        _Logger.Trace("[OMDb_Data] [Scraper_TVSeason] [Start]")
        Dim Result As MediaContainers.SeasonDetails = Nothing

        If Result IsNot Nothing Then
            _Logger.Trace("[OMDb_Data] [Scraper_TVSeason] [Done]")
            Return New Interfaces.AddonResult_Data_Scraper_TVSeason(Result)
        Else
            _Logger.Trace("[OMDb_Data] [Scraper_TVSeason] [Abort] No result found")
            Return New Interfaces.AddonResult_Data_Scraper_TVSeason(Interfaces.ResultStatus.NoResult)
        End If
    End Function

    Public Sub ScraperOrderChanged() Implements Interfaces.IAddon_Data_Scraper_Movie.ScraperOrderChanged, Interfaces.IAddon_Data_Scraper_TV.ScraperOrderChanged
        If _setup_Movie IsNot Nothing Then _setup_Movie.OrderChanged()
        If _setup_TV IsNot Nothing Then _setup_TV.OrderChanged()
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Class Settings

#Region "Properties"

        Public ReadOnly Property AnyRatingEnabled As Boolean
            Get
                Return IMDb OrElse Metascore OrElse Tomatometer
            End Get
        End Property

        Public Property APIKey As String = String.Empty

        Public Property IMDb As Boolean

        Public Property Metascore As Boolean

        Public Property Tomatometer As Boolean

#End Region 'Properties

    End Class

#End Region 'Nested Types

End Class