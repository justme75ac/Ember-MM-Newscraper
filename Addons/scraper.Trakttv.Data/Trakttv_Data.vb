﻿' ################################################################################
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
Imports System.Threading.Tasks

Public Class Addon
    Implements Interfaces.ScraperModule_Data_Movie
    Implements Interfaces.ScraperModule_Data_TV


#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

    Public Shared _AssemblyName As String
    Public Shared ConfigScrapeOptions_Movie As New Structures.ScrapeOptions
    Public Shared ConfigScrapeOptions_TV As New Structures.ScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifiers
    Public Shared ConfigScrapeModifier_TV As New Structures.ScrapeModifiers

    Private _Name As String = "Trakttv_Data"
    Private _ScraperEnabled_Movie As Boolean = False
    Private _ScraperEnabled_TV As Boolean = False
    Private _setup_Movie As frmSettingsHolder_Movie
    Private _setup_TV As frmSettingsHolder_TV

    Private _AddonSettings_Movie As New AddonSettings
    Private _AddonSettings_TV As New AddonSettings
    Private _TraktAPI_Movie As New clsAPITrakt
    Private _TraktAPI_TV As New clsAPITrakt

#End Region 'Fields

#Region "Events"

    'Movie part
    Public Event ModuleSettingsChanged_Movie() Implements Interfaces.ScraperModule_Data_Movie.ModuleSettingsChanged
    Public Event ScraperEvent_Movie(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object) Implements Interfaces.ScraperModule_Data_Movie.ScraperEvent
    Public Event ScraperSetupChanged_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModule_Data_Movie.ScraperSetupChanged
    Public Event SetupNeedsRestart_Movie() Implements Interfaces.ScraperModule_Data_Movie.SetupNeedsRestart

    'TV part
    Public Event ModuleSettingsChanged_TV() Implements Interfaces.ScraperModule_Data_TV.ModuleSettingsChanged
    Public Event ScraperEvent_TV(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object) Implements Interfaces.ScraperModule_Data_TV.ScraperEvent
    Public Event ScraperSetupChanged_TV(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModule_Data_TV.ScraperSetupChanged
    Public Event SetupNeedsRestart_TV() Implements Interfaces.ScraperModule_Data_TV.SetupNeedsRestart

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleName() As String Implements Interfaces.ScraperModule_Data_Movie.ModuleName, Interfaces.ScraperModule_Data_TV.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.ScraperModule_Data_Movie.ModuleVersion, Interfaces.ScraperModule_Data_TV.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    Property ScraperEnabled_Movie() As Boolean Implements Interfaces.ScraperModule_Data_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled_Movie
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Movie = value
            If _ScraperEnabled_Movie Then
                Task.Run(Function() _TraktAPI_Movie.CreateAPI(_AddonSettings_Movie, "0679d762ec6de44d93e0ea42a3b7662cfd8c4e9f430e82550d83a2bc8e25072b", "0581ff6f5075003dbe4620c959e1200d06cce7de639f268f7182b588ffdaa1a1"))
            End If
        End Set
    End Property

    Property ScraperEnabled_TV() As Boolean Implements Interfaces.ScraperModule_Data_TV.ScraperEnabled
        Get
            Return _ScraperEnabled_TV
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_TV = value
            If _ScraperEnabled_TV Then
                Task.Run(Function() _TraktAPI_TV.CreateAPI(_AddonSettings_TV, "f91af6501263371353f6d1f5d9ff924934c796b555c0341044336e94080021f1", "20ec71842c1da85dadb554d7920c22b4efb0aeb198fa606880a9e36ffd7c95c4"))
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

    Private Sub Handle_NewToken_Movie()
        _AddonSettings_Movie.APIAccessToken = _TraktAPI_Movie.AccessToken
        _AddonSettings_Movie.APICreated = Functions.ConvertToUnixTimestamp(_TraktAPI_Movie.Created).ToString
        _AddonSettings_Movie.APIExpiresInSeconds = _TraktAPI_Movie.ExpiresInSeconds.ToString
        _AddonSettings_Movie.APIRefreshToken = _TraktAPI_Movie.RefreshToken
        SaveSettings_Movie()
    End Sub

    Private Sub Handle_NewToken_TV()
        _AddonSettings_TV.APIAccessToken = _TraktAPI_TV.AccessToken
        _AddonSettings_TV.APICreated = Functions.ConvertToUnixTimestamp(_TraktAPI_TV.Created).ToString
        _AddonSettings_TV.APIExpiresInSeconds = _TraktAPI_TV.ExpiresInSeconds.ToString
        _AddonSettings_TV.APIRefreshToken = _TraktAPI_TV.RefreshToken
        SaveSettings_TV()
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

    Sub Init_Movie(ByVal sAssemblyName As String) Implements Interfaces.ScraperModule_Data_Movie.Init
        _AssemblyName = sAssemblyName
        LoadSettings_Movie()
        AddHandler _TraktAPI_Movie.NewTokenCreated, AddressOf Handle_NewToken_Movie
    End Sub

    Sub Init_TV(ByVal sAssemblyName As String) Implements Interfaces.ScraperModule_Data_TV.Init
        _AssemblyName = sAssemblyName
        LoadSettings_TV()
        AddHandler _TraktAPI_TV.NewTokenCreated, AddressOf Handle_NewToken_TV
    End Sub

    Function InjectSetupScraper_Movie() As Containers.SettingsPanel Implements Interfaces.ScraperModule_Data_Movie.InjectSetupScraper
        Dim SPanel As New Containers.SettingsPanel
        _setup_Movie = New frmSettingsHolder_Movie
        LoadSettings_Movie()
        _setup_Movie.chkEnabled.Checked = _ScraperEnabled_Movie

        _setup_Movie.chkRating.Checked = ConfigScrapeOptions_Movie.bMainRating
        _setup_Movie.chkUserRating.Checked = ConfigScrapeOptions_Movie.bMainUserRating

        _setup_Movie.orderChanged()

        SPanel.Name = String.Concat(_Name, "_Movie")
        SPanel.Text = "Trakttv"
        SPanel.Prefix = "TrakttvMovieInfo_"
        SPanel.Order = 110
        SPanel.Parent = "pnlMovieData"
        SPanel.Type = Master.eLang.GetString(36, "Movies")
        SPanel.ImageIndex = If(_ScraperEnabled_Movie, 9, 10)
        SPanel.Panel = _setup_Movie.pnlSettings

        AddHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
        AddHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
        Return SPanel
    End Function

    Function InjectSetupScraper_TV() As Containers.SettingsPanel Implements Interfaces.ScraperModule_Data_TV.InjectSetupScraper
        Dim SPanel As New Containers.SettingsPanel
        _setup_TV = New frmSettingsHolder_TV
        LoadSettings_TV()
        _setup_TV.chkEnabled.Checked = _ScraperEnabled_TV

        _setup_TV.chkScraperShowRating.Checked = ConfigScrapeOptions_TV.bMainRating
        _setup_TV.chkScraperShowUserRating.Checked = ConfigScrapeOptions_TV.bMainUserRating
        _setup_TV.chkScraperEpisodeRating.Checked = ConfigScrapeOptions_TV.bEpisodeRating
        _setup_TV.chkScraperEpisodeUserRating.Checked = ConfigScrapeOptions_TV.bEpisodeUserRating

        _setup_TV.orderChanged()

        SPanel.Name = String.Concat(_Name, "_TV")
        SPanel.Text = "Trakttv"
        SPanel.Prefix = "TrakttvTVInfo_"
        SPanel.Order = 110
        SPanel.Parent = "pnlTVData"
        SPanel.Type = Master.eLang.GetString(653, "TV Shows")
        SPanel.ImageIndex = If(_ScraperEnabled_TV, 9, 10)
        SPanel.Panel = _setup_TV.pnlSettings

        AddHandler _setup_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_TV
        AddHandler _setup_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_TV
        Return SPanel
    End Function

    Sub LoadSettings_Movie()
        ConfigScrapeOptions_Movie.bMainRating = AdvancedSettings.GetBooleanSetting("DoRating", True)
        ConfigScrapeOptions_Movie.bMainUserRating = AdvancedSettings.GetBooleanSetting("DoUserRating", True)
        _AddonSettings_Movie.APIAccessToken = AdvancedSettings.GetSetting("APIAccessToken", String.Empty, , Enums.ContentType.Movie)
        _AddonSettings_Movie.APICreated = AdvancedSettings.GetSetting("APICreatedAt", "0", , Enums.ContentType.Movie)
        _AddonSettings_Movie.APIExpiresInSeconds = AdvancedSettings.GetSetting("APIExpiresInSeconds", "0", , Enums.ContentType.Movie)
        _AddonSettings_Movie.APIRefreshToken = AdvancedSettings.GetSetting("APIRefreshToken", String.Empty, , Enums.ContentType.Movie)
    End Sub

    Sub LoadSettings_TV()
        ConfigScrapeOptions_TV.bEpisodeRating = AdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bEpisodeUserRating = AdvancedSettings.GetBooleanSetting("DoUserRating", True, , Enums.ContentType.TVEpisode)
        ConfigScrapeOptions_TV.bMainRating = AdvancedSettings.GetBooleanSetting("DoRating", True, , Enums.ContentType.TVShow)
        ConfigScrapeOptions_TV.bMainUserRating = AdvancedSettings.GetBooleanSetting("DoUserRating", True, , Enums.ContentType.TVShow)
        _AddonSettings_TV.APIAccessToken = AdvancedSettings.GetSetting("APIAccessToken", String.Empty, , Enums.ContentType.TV)
        _AddonSettings_TV.APICreated = AdvancedSettings.GetSetting("APICreatedAt", "0", , Enums.ContentType.TV)
        _AddonSettings_TV.APIExpiresInSeconds = AdvancedSettings.GetSetting("APIExpiresInSeconds", "0", , Enums.ContentType.TV)
        _AddonSettings_TV.APIRefreshToken = AdvancedSettings.GetSetting("APIRefreshToken", String.Empty, , Enums.ContentType.TV)
    End Sub

    Sub SaveSettings_Movie()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_Movie.bMainRating, , , Enums.ContentType.Movie)
            settings.SetBooleanSetting("DoUserRating", ConfigScrapeOptions_Movie.bMainUserRating, , , Enums.ContentType.Movie)
            settings.SetSetting("APIAccessToken", _AddonSettings_Movie.APIAccessToken, , , Enums.ContentType.Movie)
            settings.SetSetting("APICreatedAt", _AddonSettings_Movie.APICreated, , , Enums.ContentType.Movie)
            settings.SetSetting("APIExpiresInSeconds", _AddonSettings_Movie.APIExpiresInSeconds, , , Enums.ContentType.Movie)
            settings.SetSetting("APIRefreshToken", _AddonSettings_Movie.APIRefreshToken, , , Enums.ContentType.Movie)
        End Using
    End Sub

    Sub SaveSettings_TV()
        Using settings = New AdvancedSettings()
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_TV.bEpisodeRating, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoUserRating", ConfigScrapeOptions_TV.bEpisodeUserRating, , , Enums.ContentType.TVEpisode)
            settings.SetBooleanSetting("DoRating", ConfigScrapeOptions_TV.bMainRating, , , Enums.ContentType.TVShow)
            settings.SetBooleanSetting("DoUserRating", ConfigScrapeOptions_TV.bMainUserRating, , , Enums.ContentType.TVShow)
            settings.SetSetting("APIAccessToken", _AddonSettings_TV.APIAccessToken, , , Enums.ContentType.TV)
            settings.SetSetting("APICreatedAt", _AddonSettings_TV.APICreated, , , Enums.ContentType.TV)
            settings.SetSetting("APIExpiresInSeconds", _AddonSettings_TV.APIExpiresInSeconds, , , Enums.ContentType.TV)
            settings.SetSetting("APIRefreshToken", _AddonSettings_TV.APIRefreshToken, , , Enums.ContentType.TV)
        End Using
    End Sub

    Sub SaveSetupScraper_Movie(ByVal DoDispose As Boolean) Implements Interfaces.ScraperModule_Data_Movie.SaveSetupScraper
        ConfigScrapeOptions_Movie.bMainRating = _setup_Movie.chkRating.Checked
        ConfigScrapeOptions_Movie.bMainUserRating = _setup_Movie.chkUserRating.Checked

        SaveSettings_Movie()
        If DoDispose Then
            RemoveHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
            RemoveHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
            _setup_Movie.Dispose()
        End If
    End Sub

    Sub SaveSetupScraper_TV(ByVal DoDispose As Boolean) Implements Interfaces.ScraperModule_Data_TV.SaveSetupScraper
        ConfigScrapeOptions_TV.bEpisodeRating = _setup_TV.chkScraperEpisodeRating.Checked
        ConfigScrapeOptions_TV.bEpisodeUserRating = _setup_TV.chkScraperEpisodeUserRating.Checked
        ConfigScrapeOptions_TV.bMainRating = _setup_TV.chkScraperShowRating.Checked
        ConfigScrapeOptions_TV.bMainUserRating = _setup_TV.chkScraperShowUserRating.Checked

        SaveSettings_TV()
        If DoDispose Then
            RemoveHandler _setup_TV.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_TV
            RemoveHandler _setup_TV.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_TV
            _setup_TV.Dispose()
        End If
    End Sub
    ''' <summary>
    '''  Scrape MovieDetails from Trakttv
    ''' </summary>
    ''' <param name="oDBMovie">Movie to be scraped. oDBMovie as ByRef to use existing data for identifing movie and to fill with IMDB/TMDB ID for next scraper</param>
    ''' <param name="nMovie">New scraped movie data</param>
    ''' <param name="Options">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>Database.DBElement Object (nMovie) which contains the scraped data</returns>
    ''' <remarks></remarks>
    Function Scraper_Movie(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_Movie Implements Interfaces.ScraperModule_Data_Movie.Scraper_Movie
        logger.Trace("[Tracktv_Data] [Scraper_Movie] [Start]")

        Dim nMovie As MediaContainers.Movie = Nothing
        Dim nDBElement = oDBElement
        If ScrapeModifiers.MainNFO Then
            Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_Movie)
            Dim nResult = Task.Run(Function() _TraktAPI_Movie.GetInfo_Movie(_TraktAPI_Movie.GetID_Trakt(nDBElement), FilteredOptions))
            If nResult.Exception Is Nothing AndAlso nResult.Result IsNot Nothing Then
                nMovie = nResult.Result
            ElseIf nResult.Exception IsNot Nothing Then
                logger.Error(String.Concat("[Tracktv_Data] [Scraper_Movie]: ", nResult.Exception.InnerException.Message))
                Task.Run(Function() _TraktAPI_Movie.RefreshAuthorization())
                logger.Error("[Tracktv_Data] [Scraper_Movie] [Abort] API error")
            End If
        End If

        logger.Trace("[Tracktv_Data] [Scraper_Movie] [Done]")
        Return New Interfaces.ModuleResult_Data_Movie With {.Result = nMovie}
    End Function

    ''' <summary>
    '''  Scrape tv show details from Trakttv
    ''' </summary>
    ''' <param name="oDBElement">TV Show to be scraped. DBTV as ByRef to use existing data for identifing tv show and to fill with IMDB/TMDB/TVDB ID for next scraper</param>
    ''' <param name="ScrapeModifiers">what additional data to scrape (images, episode details, season details...)</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>modifies Database.DBElement Object which contains the scraped data</returns>
    ''' <remarks></remarks>
    Function Scraper_TV(ByRef oDBElement As Database.DBElement, ByRef ScrapeModifiers As Structures.ScrapeModifiers, ByRef ScrapeType As Enums.ScrapeType, ByRef ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVShow Implements Interfaces.ScraperModule_Data_TV.Scraper_TVShow
        logger.Trace("[Tracktv_Data] [Scraper_TV] [Start]")

        Dim nTVShow As New MediaContainers.TVShow

        If ScrapeModifiers.MainNFO Then
            Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)
            Dim nResult = _TraktAPI_TV.GetInfo_TVShow(_TraktAPI_TV.GetID_Trakt(oDBElement), FilteredOptions, ScrapeModifiers, oDBElement.Episodes)
            While Not nResult.IsCompleted
                Threading.Thread.Sleep(50)
            End While
            If nResult.Exception Is Nothing AndAlso nResult.Result IsNot Nothing Then
                nTVShow = nResult.Result
            ElseIf nResult.Exception IsNot Nothing Then
                logger.Error(String.Concat("[Tracktv_Data] [Scraper_TV]: ", nResult.Exception.InnerException.Message))
                Task.Run(Function() _TraktAPI_TV.RefreshAuthorization())
                logger.Error("[Tracktv_Data] [Scraper_TV] [Abort] API error")
            End If
        End If

        logger.Trace("[Tracktv_Data] [Scraper_TV] [Done]")
        Return New Interfaces.ModuleResult_Data_TVShow With {.Result = nTVShow}
    End Function
    ''' <summary>
    '''  Scrape episode details from Trakttv
    ''' </summary>
    ''' <param name="oDBElement">TV Season to be scraped as ByRef to use existing data for identifing tv show and to fill with IMDB/TMDB/TVDB ID for next scraper</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>modifies Database.DBElement Object which contains the scraped data</returns>
    ''' <remarks></remarks>
    Public Function Scraper_TVEpisode(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVEpisode Implements Interfaces.ScraperModule_Data_TV.Scraper_TVEpisode
        logger.Trace("[Tracktv_Data] [Scraper_TVEpisode] [Start]")

        Dim nTVEpisode As New MediaContainers.EpisodeDetails

        Dim FilteredOptions As Structures.ScrapeOptions = Functions.ScrapeOptionsAndAlso(ScrapeOptions, ConfigScrapeOptions_TV)
        If FilteredOptions.bEpisodeRating OrElse FilteredOptions.bEpisodeUserRating Then
            Dim nResult = _TraktAPI_TV.GetInfo_TVEpisode(_TraktAPI_TV.GetID_Trakt(oDBElement, True), oDBElement.TVEpisode.Season, oDBElement.TVEpisode.Episode, FilteredOptions)
            While Not nResult.IsCompleted
                Threading.Thread.Sleep(50)
            End While
            If nResult.Exception Is Nothing AndAlso nResult.Result IsNot Nothing Then
                nTVEpisode = nResult.Result
            ElseIf nResult.Exception IsNot Nothing Then
                logger.Error(String.Concat("[Tracktv_Data] [Scraper_TVEpisode]: ", nResult.Exception.InnerException.Message))
                Task.Run(Function() _TraktAPI_TV.RefreshAuthorization())
                logger.Error("[Tracktv_Data] [Scraper_TVEpisode] [Abort] API error")
            End If
        End If

        logger.Trace("[Tracktv_Data] [Scraper_TVEpisode] [Done]")
        Return New Interfaces.ModuleResult_Data_TVEpisode With {.Result = nTVEpisode}
    End Function
    ''' <summary>
    '''  Scrape season details from Trakttv
    ''' </summary>
    ''' <param name="oDBElement">TV Season to be scraped as ByRef to use existing data for identifing tv show and to fill with IMDB/TMDB/TVDB ID for next scraper</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape(global scraper settings)</param>
    ''' <returns>Database.DBElement Object (nMovie) which contains the scraped data</returns>
    ''' <remarks></remarks>
    Public Function Scraper_TVSeason(ByRef oDBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions) As Interfaces.ModuleResult_Data_TVSeason Implements Interfaces.ScraperModule_Data_TV.Scraper_TVSeason
        Return New Interfaces.ModuleResult_Data_TVSeason With {.Result = Nothing}
    End Function

    Function GetMovieStudio(ByRef DBMovie As Database.DBElement, ByRef studio As List(Of String)) As Interfaces.ModuleResult Implements Interfaces.ScraperModule_Data_Movie.GetMovieStudio
        If (DBMovie.Movie Is Nothing OrElse String.IsNullOrEmpty(DBMovie.Movie.UniqueIDs.IMDbId)) Then
            logger.Error("Attempting to get studio for undefined movie")
            Return New Interfaces.ModuleResult
        End If

        LoadSettings_Movie()
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Function GetTMDbIdByIMDbId(ByVal imdbId As String, ByRef tmdbId As Integer) As Interfaces.ModuleResult Implements Interfaces.ScraperModule_Data_Movie.GetTMDbIdByIMDbId
        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Public Sub ScraperOrderChanged_Movie() Implements Interfaces.ScraperModule_Data_Movie.ScraperOrderChanged
        _setup_Movie.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_TV() Implements Interfaces.ScraperModule_Data_TV.ScraperOrderChanged
        _setup_TV.orderChanged()
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Structure AddonSettings

#Region "Fields"

        Dim APIAccessToken As String
        Dim APICreated As String
        Dim APIExpiresInSeconds As String
        Dim APIRefreshToken As String

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class