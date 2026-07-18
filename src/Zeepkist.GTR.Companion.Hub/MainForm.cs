using System;
using System.Drawing;
using System.Windows.Forms;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal sealed class MainForm : Form
{
    private readonly CheckBox _enableCheckBox;
    private readonly Label _connectionValue;
    private readonly ListBox _recentEvents;
    private bool _allowClose;

    public MainForm(bool enabled)
    {
        Text = "Zeepkist GTR Companion";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(620, 470);
        Size = new Size(720, 540);
        Font = new Font("Segoe UI", 10F);

        var title = new Label
        {
            AutoSize = true,
            Font = new Font(Font.FontFamily, 19F, FontStyle.Bold),
            Text = "Zeepkist GTR Companion"
        };

        var introduction = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(640, 0),
            Text = "Connect Zeepkist to companion experiences without changing or controlling the game."
        };

        _enableCheckBox = new CheckBox
        {
            AutoSize = true,
            Checked = enabled,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Enable live companion data on this PC"
        };

        var privacy = new Label
        {
            AutoSize = true,
            ForeColor = Color.DimGray,
            MaximumSize = new Size(640, 0),
            Text = "Only the local Windows hub is enabled. LAN sharing, mobile pairing, player identity, cloud services, and run recording remain off."
        };

        var statusTitle = new Label
        {
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Zeepkist connection"
        };

        _connectionValue = new Label
        {
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = enabled ? "Waiting for Zeepkist" : "Disabled"
        };

        var recentTitle = new Label
        {
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Text = "Recent local events"
        };

        _recentEvents = new ListBox
        {
            Dock = DockStyle.Fill,
            IntegralHeight = false
        };

        var closeButton = new Button
        {
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            Text = "Hide to tray"
        };
        closeButton.Click += (_, _) => Hide();

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            RowCount = 9
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(title);
        layout.Controls.Add(WithTopMargin(introduction, 4));
        layout.Controls.Add(WithTopMargin(_enableCheckBox, 20));
        layout.Controls.Add(WithTopMargin(privacy, 4));
        layout.Controls.Add(WithTopMargin(statusTitle, 20));
        layout.Controls.Add(_connectionValue);
        layout.Controls.Add(WithTopMargin(recentTitle, 20));
        layout.Controls.Add(_recentEvents);
        layout.Controls.Add(WithTopMargin(closeButton, 12));
        Controls.Add(layout);

        _enableCheckBox.CheckedChanged += (_, _) => EnablementChanged?.Invoke(_enableCheckBox.Checked);
        FormClosing += OnFormClosing;
    }

    public event Action<bool>? EnablementChanged;

    public void SetConnectionState(CompanionConnectionState state)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetConnectionState(state));
            return;
        }

        (_connectionValue.Text, _connectionValue.ForeColor) = state switch
        {
            CompanionConnectionState.Disabled => ("Disabled", Color.DimGray),
            CompanionConnectionState.WaitingForGame => ("Waiting for Zeepkist", Color.DarkGoldenrod),
            CompanionConnectionState.Connected => ("Connected", Color.ForestGreen),
            CompanionConnectionState.Faulted => ("Needs attention", Color.Firebrick),
            _ => ("Unknown", Color.DimGray)
        };
    }

    public void AddEvent(ReceivedCompanionEvent companionEvent)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AddEvent(companionEvent));
            return;
        }

        _recentEvents.Items.Insert(
            0,
            $"{companionEvent.Timestamp.ToLocalTime():T}  {companionEvent.Type}");

        while (_recentEvents.Items.Count > 100)
        {
            _recentEvents.Items.RemoveAt(_recentEvents.Items.Count - 1);
        }
    }

    public void AllowClose()
    {
        _allowClose = true;
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_allowClose || e.CloseReason == CloseReason.WindowsShutDown)
        {
            return;
        }

        e.Cancel = true;
        Hide();
    }

    private static Control WithTopMargin(Control control, int top)
    {
        control.Margin = new Padding(0, top, 0, 0);
        return control;
    }
}
