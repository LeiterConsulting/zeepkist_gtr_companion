using System;
using System.Drawing;
using System.Windows.Forms;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal sealed class HubApplicationContext : ApplicationContext
{
    private readonly HubSettingsStore _settingsStore = new();
    private readonly HubSettings _settings;
    private readonly CompanionPipeServer _pipeServer = new();
    private readonly MainForm _mainForm;
    private readonly NotifyIcon _notifyIcon;

    public HubApplicationContext()
    {
        var settingsExisted = _settingsStore.Exists;
        _settings = _settingsStore.Load();
        _mainForm = new MainForm(_settings.LocalCompanionEnabled);
        _mainForm.EnablementChanged += OnEnablementChanged;

        _pipeServer.ConnectionStateChanged += _mainForm.SetConnectionState;
        _pipeServer.EventReceived += _mainForm.AddEvent;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());

        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = SystemIcons.Application,
            Text = "Zeepkist GTR Companion",
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        if (_settings.LocalCompanionEnabled)
        {
            _pipeServer.Start();
        }
        else
        {
            _mainForm.SetConnectionState(CompanionConnectionState.Disabled);
        }

        if (!settingsExisted || !_settings.LocalCompanionEnabled)
        {
            ShowMainWindow();
        }
    }

    protected override void ExitThreadCore()
    {
        _pipeServer.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _mainForm.AllowClose();
        _mainForm.Close();
        _mainForm.Dispose();
        base.ExitThreadCore();
    }

    private void OnEnablementChanged(bool enabled)
    {
        _settings.LocalCompanionEnabled = enabled;

        try
        {
            _settingsStore.Save(_settings);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                _mainForm,
                $"The preference could not be saved. {exception.Message}",
                "Zeepkist GTR Companion",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        if (enabled)
        {
            _pipeServer.Start();
        }
        else
        {
            _pipeServer.Stop();
        }
    }

    private void ShowMainWindow()
    {
        if (!_mainForm.Visible)
        {
            _mainForm.Show();
        }

        if (_mainForm.WindowState == FormWindowState.Minimized)
        {
            _mainForm.WindowState = FormWindowState.Normal;
        }

        _mainForm.Activate();
    }

    private void ExitApplication()
    {
        ExitThread();
    }
}
