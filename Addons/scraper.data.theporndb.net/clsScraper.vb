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
Imports System.Net.Http
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Threading.Tasks

Public Class Scraper

#Region "Fields"

    Shared _Logger As Logger = LogManager.GetCurrentClassLogger()

    Private _addonSettings As Addon.Settings
    Private _HttpClient As HttpClient = Nothing
    Private Const API_BASE_URL As String = "https://api.theporndb.net"
    Private _lastRequestTime As DateTime = DateTime.MinValue
    Private Const MIN_REQUEST_INTERVAL_MS As Integer = 500 ' 120 requests per minute = 500ms between requests

#End Region 'Fields

#Region "Properties"

    Public ReadOnly Property IsClientCreated As Boolean
        Get
            Return _HttpClient IsNot Nothing
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Public Sub CreateAPI(ByVal addonSettings As Addon.Settings)
        If Not String.IsNullOrEmpty(addonSettings.APIKey) Then
            Try
                _addonSettings = addonSettings

                _HttpClient = New HttpClient
                _HttpClient.DefaultRequestHeaders.Add("Authorization", String.Concat("Bearer ", addonSettings.APIKey))
                _HttpClient.DefaultRequestHeaders.Add("User-Agent", "EmberMediaManager/1.0")
                _HttpClient.Timeout = TimeSpan.FromSeconds(30)
                _Logger.Trace("[ThePornDB_Data] [CreateAPI] Client created")
            Catch ex As Exception
                _HttpClient = Nothing
                _Logger.Error(ex, "[ThePornDB_Data] [CreateAPI] [Error]")
            End Try
        Else
            _Logger.Error("[ThePornDB_Data] [CreateAPI] [Error] No API key available")
            _HttpClient = Nothing
        End If
    End Sub

    Private Sub RateLimit()
        Dim timeSinceLastRequest = (DateTime.Now - _lastRequestTime).TotalMilliseconds
        If timeSinceLastRequest < MIN_REQUEST_INTERVAL_MS Then
            Threading.Thread.Sleep(CInt(MIN_REQUEST_INTERVAL_MS - timeSinceLastRequest))
        End If
        _lastRequestTime = DateTime.Now
    End Sub

    Private Async Function MakeRequestAsync(ByVal url As String) As Task(Of String)
        If _HttpClient Is Nothing Then Return Nothing

        Try
            RateLimit()
            Dim response = Await _HttpClient.GetAsync(url)
            response.EnsureSuccessStatusCode()
            Return Await response.Content.ReadAsStringAsync()
        Catch ex As Exception
            _Logger.Error(ex, String.Format("[ThePornDB_Data] [MakeRequestAsync] [Error] URL: {0}", url))
            Return Nothing
        End Try
    End Function

    Public Function Search_Movie(ByVal title As String, ByVal imdbId As String, ByVal filteredOptions As Structures.ScrapeOptions) As List(Of MediaContainers.MainDetails)
        If String.IsNullOrEmpty(title) AndAlso String.IsNullOrEmpty(imdbId) Then Return Nothing

        Try
            Dim url As New StringBuilder(API_BASE_URL)
            url.Append("/movies?")

            Dim hasParam As Boolean = False

            If Not String.IsNullOrEmpty(title) Then
                url.Append("parse=").Append(Uri.EscapeDataString(title))
                hasParam = True
            End If

            If Not String.IsNullOrEmpty(imdbId) Then
                If hasParam Then url.Append("&")
                url.Append("imdb=").Append(Uri.EscapeDataString(imdbId))
            End If

            Dim jsonResponse = MakeRequestAsync(url.ToString()).Result
            If String.IsNullOrEmpty(jsonResponse) Then Return Nothing

            Dim json = JObject.Parse(jsonResponse)
            Dim results As New List(Of MediaContainers.MainDetails)

            ' TPDB API kann ein einzelnes Objekt oder ein Array zurückgeben
            Dim dataItems As JToken = Nothing
            If json("data") IsNot Nothing Then
                If json("data").Type = JTokenType.Array Then
                    dataItems = json("data")
                ElseIf json("data").Type = JTokenType.Object Then
                    ' Einzelnes Objekt in Array umwandeln
                    Dim singleItem As New JArray()
                    singleItem.Add(json("data"))
                    dataItems = singleItem
                End If
            End If

            If dataItems IsNot Nothing Then
                For Each item In dataItems
                    Dim movieResult As New MediaContainers.Movie With {
                        .Scrapersource = "ThePornDB"
                    }

                    If item("id") IsNot Nothing Then
                        movieResult.UniqueIDs.TPDbId = item("id").ToString()
                    End If

                    If item("name") IsNot Nothing Then
                        movieResult.Title = item("name").ToString()
                    End If

                    If item("date") IsNot Nothing Then
                        Dim dateStr = item("date").ToString()
                        If Not String.IsNullOrEmpty(dateStr) AndAlso dateStr.Length >= 4 Then
                            movieResult.Year = dateStr.Substring(0, 4)
                        End If
                    End If

                    If item("site") IsNot Nothing AndAlso item("site")("name") IsNot Nothing Then
                        movieResult.Studios.Add(item("site")("name").ToString())
                    End If

                    results.Add(movieResult)
                Next
            End If

            Return results
        Catch ex As Exception
            _Logger.Error(ex, "[ThePornDB_Data] [Search_Movie] [Error]")
            Return Nothing
        End Try
    End Function

    Public Function GetInfo_Movie(ByVal tpdbId As String, ByVal filteredOptions As Structures.ScrapeOptions) As MediaContainers.Movie
        If String.IsNullOrEmpty(tpdbId) Then Return Nothing

        Try
            Dim url = String.Format("{0}/movies/{1}", API_BASE_URL, Uri.EscapeDataString(tpdbId))
            Dim jsonResponse = MakeRequestAsync(url).Result
            If String.IsNullOrEmpty(jsonResponse) Then Return Nothing

            Dim json = JObject.Parse(jsonResponse)
            Dim result As New MediaContainers.Movie With {
                .Scrapersource = "ThePornDB"
            }

            Dim data = json("data")
            If data Is Nothing Then
                _Logger.Warn("[ThePornDB_Data] [GetInfo_Movie] [Warning] No data in response for TPDbId: {0}", tpdbId)
                Return Nothing
            End If

            ' TPDb ID
            If data("id") IsNot Nothing Then
                result.UniqueIDs.TPDbId = data("id").ToString()
            End If

            ' Title
            If data("name") IsNot Nothing Then
                result.Title = data("name").ToString()
            End If

            ' Original Title
            If data("name") IsNot Nothing Then
                result.OriginalTitle = data("name").ToString()
            End If

            ' Year/Date
            If data("date") IsNot Nothing Then
                Dim dateStr = data("date").ToString()
                If Not String.IsNullOrEmpty(dateStr) AndAlso dateStr.Length >= 4 Then
                    result.Year = dateStr.Substring(0, 4)
                    result.Premiered = dateStr
                End If
            End If

            ' Plot/Description
            If filteredOptions.bMainPlot AndAlso data("description") IsNot Nothing Then
                result.Plot = data("description").ToString()
            End If

            ' Outline
            If filteredOptions.bMainOutline AndAlso data("description") IsNot Nothing Then
                Dim desc = data("description").ToString()
                If desc.Length > 200 Then
                    result.Outline = desc.Substring(0, 200) & "..."
                Else
                    result.Outline = desc
                End If
            End If

            ' Duration/Runtime
            If data("duration") IsNot Nothing Then
                Dim duration As Integer
                If Integer.TryParse(data("duration").ToString(), duration) Then
                    result.Runtime = duration.ToString()
                End If
            End If

            ' Studio/Site
            If data("site") IsNot Nothing AndAlso data("site")("name") IsNot Nothing Then
                result.Studios.Add(data("site")("name").ToString())
            End If

            ' Genres/Tags
            If data("tags") IsNot Nothing AndAlso data("tags").Type = JTokenType.Array Then
                For Each tag In data("tags")
                    If tag("name") IsNot Nothing Then
                        result.Genres.Add(tag("name").ToString())
                    End If
                Next
            End If

            ' Actors/Performers
            If data("performers") IsNot Nothing AndAlso data("performers").Type = JTokenType.Array Then
                For Each performer In data("performers")
                    If performer("name") IsNot Nothing Then
                        Dim actor As New MediaContainers.Person With {
                            .Name = performer("name").ToString()
                        }
                        If performer("id") IsNot Nothing Then
                            actor.ID = CLng(performer("id").ToString())
                        End If
                        result.Actors.Add(actor)
                    End If
                Next
            End If

            ' Rating
            If filteredOptions.bMainRating AndAlso data("rating") IsNot Nothing Then
                Dim rating As Double
                If Double.TryParse(data("rating").ToString(), Globalization.NumberStyles.AllowDecimalPoint, Globalization.CultureInfo.InvariantCulture, rating) Then
                    result.Ratings.Add(New MediaContainers.RatingDetails With {
                                       .Max = 10,
                                       .Type = "tpdb",
                                       .Value = rating
                                       })
                End If
            End If

            ' IMDb ID (if available)
            If data("external_ids") IsNot Nothing AndAlso data("external_ids")("imdb") IsNot Nothing Then
                result.UniqueIDs.IMDbId = data("external_ids")("imdb").ToString()
            End If

            ' TMDB ID (if available)
            If data("external_ids") IsNot Nothing AndAlso data("external_ids")("tmdb") IsNot Nothing Then
                Dim tmdbId As Integer
                If Integer.TryParse(data("external_ids")("tmdb").ToString(), tmdbId) Then
                    result.UniqueIDs.TMDbId = tmdbId
                End If
            End If

            Return result
        Catch ex As Exception
            _Logger.Error(ex, String.Format("[ThePornDB_Data] [GetInfo_Movie] [Error] TPDbId: {0}", tpdbId))
            Return Nothing
        End Try
    End Function

#End Region 'Methods

End Class
