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
Imports System.IO

Public Class Addon
    Implements Interfaces.IAddon_Generic

#Region "Fields"

    Private WithEvents mnuMainToolsExporter As New ToolStripMenuItem
    Private WithEvents cmnuTrayToolsExporter As New ToolStripMenuItem
    Private _AssemblyName As String = String.Empty
    Private _enabled As Boolean = False
    Private _Name As String = "MovieListExporter"
    Private _setup As frmSettingsHolder
    Private MySettings As New _MySettings

    Private _AddonSettings As New AddonSettings

#End Region 'Fields

#Region "Delegates"

    Public Delegate Sub Delegate_AddToolsStripItem(control As ToolStripMenuItem, value As ToolStripItem)
    Public Delegate Sub Delegate_RemoveToolsStripItem(control As ToolStripMenuItem, value As ToolStripItem)

#End Region 'Delegates

#Region "Events"

    Public Event AddonSettingsChanged() Implements Interfaces.IAddon_Generic.AddonSettingsChanged
    Public Event AddonStateChanged(ByVal name As String, ByVal state As Boolean, ByVal diffOrder As Integer) Implements Interfaces.IAddon_Generic.AddonStateChanged
    Public Event AddonNeedsRestart() Implements Interfaces.IAddon_Generic.AddonNeedsRestart

#End Region 'Events

#Region "Properties"

    Property ScraperEnabled() As Boolean Implements Interfaces.IAddon_Generic.ScraperEnabled
        Get
            Return _enabled
        End Get
        Set(ByVal value As Boolean)
            If _enabled = value Then Return
            _enabled = value
            If _enabled Then
                Enable()
            Else
                Disable()
            End If
        End Set
    End Property

    ReadOnly Property ModuleName() As String Implements Interfaces.IAddon_Generic.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.IAddon_Generic.ModuleVersion
        Get
            Return FileVersionInfo.GetVersionInfo(Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

#End Region 'Properties

#Region "Methods"

    Sub Disable()
        Dim tsi As New ToolStripMenuItem

        'mnuMainTools
        tsi = DirectCast(Addons.Instance.RuntimeObjects.MainMenu.Items("mnuMainTools"), ToolStripMenuItem)
        RemoveToolsStripItem(tsi, mnuMainToolsExporter)

        'cmnuTrayTools
        tsi = DirectCast(Addons.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayTools"), ToolStripMenuItem)
        RemoveToolsStripItem(tsi, cmnuTrayToolsExporter)
    End Sub

    Public Sub RemoveToolsStripItem(control As ToolStripMenuItem, value As ToolStripItem)
        If control.Owner.InvokeRequired Then
            control.Owner.Invoke(New Delegate_RemoveToolsStripItem(AddressOf RemoveToolsStripItem), New Object() {control, value})
        Else
            control.DropDownItems.Remove(value)
        End If
    End Sub

    Sub Enable()
        Dim tsi As New ToolStripMenuItem

        'mnuMainTools
        mnuMainToolsExporter.Image = New Bitmap(My.Resources.icon)
        mnuMainToolsExporter.Text = Master.eLang.GetString(336, "Export Movie List")
        tsi = DirectCast(Addons.Instance.RuntimeObjects.MainMenu.Items("mnuMainTools"), ToolStripMenuItem)
        AddToolsStripItem(tsi, mnuMainToolsExporter)

        'cmnuTrayTools
        cmnuTrayToolsExporter.Image = New Bitmap(My.Resources.icon)
        cmnuTrayToolsExporter.Text = Master.eLang.GetString(336, "Export Movie List")
        tsi = DirectCast(Addons.Instance.RuntimeObjects.TrayMenu.Items("cmnuTrayTools"), ToolStripMenuItem)
        AddToolsStripItem(tsi, cmnuTrayToolsExporter)
    End Sub

    Public Sub AddToolsStripItem(control As ToolStripMenuItem, value As ToolStripItem)
        If control.Owner.InvokeRequired Then
            control.Owner.Invoke(New Delegate_AddToolsStripItem(AddressOf AddToolsStripItem), New Object() {control, value})
        Else
            control.DropDownItems.Add(value)
        End If
    End Sub

    Private Sub Handle_ModuleEnabledChanged(ByVal state As Boolean)
        RaiseEvent AddonStateChanged(_Name, state, 0)
    End Sub

    Private Sub Handle_ModuleSettingsChanged()
        RaiseEvent AddonSettingsChanged()
    End Sub

    Public Sub Init(ByVal sAssemblyName As String) Implements Interfaces.IAddon_Generic.Init
        _AssemblyName = sAssemblyName
        LoadSettings()
    End Sub

    Function InjectSettingsPanel() As Containers.SettingsPanel Implements Interfaces.IAddon_Generic.InjectSettingsPanel
        _setup = New frmSettingsHolder
        _setup.cbEnabled.Checked = _enabled
        Dim SPanel As New Containers.SettingsPanel

        _setup.txtExportPath.Text = MySettings.ExportPath
        _setup.chkExportMissingEpisodes.Checked = MySettings.ExportMissingEpisodes

        SPanel.UniqueName = _Name
        SPanel.Title = Master.eLang.GetString(335, "Website Creator")
        SPanel.Type = Master.eLang.GetString(802, "Addons")
        SPanel.ImageIndex = If(_enabled, 9, 10)
        SPanel.Order = 100
        SPanel.Panel = _setup.pnlSettings
        AddHandler _setup.ModuleEnabledChanged, AddressOf Handle_ModuleEnabledChanged
        AddHandler _setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
        Return SPanel
    End Function

    Private Sub MyMenuItem_Click(ByVal sender As Object, ByVal e As EventArgs) Handles mnuMainToolsExporter.Click, cmnuTrayToolsExporter.Click
        'RaiseEvent GenericEvent(Enums.AddonEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", False}))

        Using dExportMovies As New dlgExportMovies
            dExportMovies.ShowDialog()
        End Using

        'RaiseEvent GenericEvent(Enums.AddonEventType.Generic, New List(Of Object)(New Object() {"controlsenabled", True}))
    End Sub

    Sub LoadSettings()
        MySettings.DefaultTemplate = _AddonSettings.GetStringSetting("DefaultTemplate", "template")
        MySettings.ExportPath = _AddonSettings.GetStringSetting("ExportPath", String.Empty)
        MySettings.ExportMissingEpisodes = _AddonSettings.GetBooleanSetting("ExportMissingEpisodes", False)
    End Sub

    Sub SaveSettings(ByVal doDispose As Boolean) Implements Interfaces.IAddon_Generic.SaveSettings
        ScraperEnabled = _setup.cbEnabled.Checked
        MySettings.ExportPath = _setup.txtExportPath.Text
        MySettings.ExportMissingEpisodes = _setup.chkExportMissingEpisodes.Checked
        SaveSettings()
        If doDispose Then
            RemoveHandler _setup.ModuleEnabledChanged, AddressOf Handle_ModuleEnabledChanged
            RemoveHandler _setup.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged
            _setup.Dispose()
        End If
    End Sub

    Sub SaveSettings()
        _AddonSettings.SetStringSetting("DefaultTemplate", MySettings.DefaultTemplate)
        _AddonSettings.SetStringSetting("ExportPath", MySettings.ExportPath)
        _AddonSettings.SetBooleanSetting("ExportMissingEpisodes", MySettings.ExportMissingEpisodes)
    End Sub

    Public Sub ScraperOrderChanged() Implements Interfaces.IAddon_Generic.ScraperOrderChanged
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Structure _MySettings

#Region "Fields"

        Dim DefaultTemplate As String
        Dim ExportPath As String
        Dim ExportMissingEpisodes As Boolean

#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class