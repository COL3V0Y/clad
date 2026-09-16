using System.Globalization;
using CladTracker.Models;
using CladTracker.Services;

namespace CladTracker;

public sealed class MainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(24, 24, 27);
    private static readonly Color PanelColor = Color.FromArgb(35, 35, 39);
    private static readonly Color InputColor = Color.FromArgb(48, 48, 53);
    private static readonly Color PrimaryColor = Color.FromArgb(210, 145, 65);
    private static readonly Color TextColor = Color.FromArgb(238, 238, 240);
    private static readonly Color MutedTextColor = Color.FromArgb(165, 165, 172);

    private readonly EntryStore store;
    private readonly BindingSource bindingSource = new();
    private readonly List<CladEntry> entries;
    private readonly DataGridView historyGrid = new();
    private readonly ComboBox cladmanSelector = new();
    private readonly NumericUpDown weightInput = new();
    private readonly RadioButton plannedRadio = new();
    private readonly RadioButton actualRadio = new();
    private readonly Label plannedTotalLabel = new();
    private readonly Label actualTotalLabel = new();
    private readonly Label recordCountLabel = new();

    public MainForm(EntryStore store)
    {
        this.store = store;
        entries = store.Load().OrderByDescending(entry => entry.CreatedAt).ToList();

        Text = "Учет кладов";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 620);
        Size = new Size(1120, 760);
        BackColor = BackgroundColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10F);

        BuildLayout();
        RefreshView();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(24, 20, 24, 20)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildEntryPanel(), 0, 1);
        root.Controls.Add(BuildHistoryPanel(), 0, 2);
        root.Controls.Add(BuildTotalsPanel(), 0, 3);
    }

    private Control BuildHeader()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(new Label
        {
            Text = "УЧЕТ КЛАДОВ",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 20F),
            ForeColor = TextColor,
            Location = new Point(0, 5)
        });
        panel.Controls.Add(new Label
        {
            Text = "История плановых и фактически полученных кладов",
            AutoSize = true,
            ForeColor = MutedTextColor,
            Location = new Point(3, 38)
        });
        return panel;
    }

    private Control BuildEntryPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            Padding = new Padding(18, 12, 18, 12),
            ColumnCount = 5,
            RowCount = 2
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

        panel.Controls.Add(MakeCaption("Кладмен", "№ от 1 до 20"), 0, 0);
        panel.Controls.Add(MakeCaption("Вес", "в граммах"), 1, 0);
        panel.Controls.Add(MakeCaption("Тип записи", "для итогов"), 2, 0);

        ConfigureComboBox();
        panel.Controls.Add(cladmanSelector, 0, 1);
        ConfigureWeightInput();
        panel.Controls.Add(weightInput, 1, 1);

        var typePanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = PanelColor, WrapContents = false };
        ConfigureRadio(plannedRadio, "План", true);
        ConfigureRadio(actualRadio, "Факт", false);
        typePanel.Controls.Add(plannedRadio);
        typePanel.Controls.Add(actualRadio);
        panel.Controls.Add(typePanel, 2, 1);

        var rateLabel = new Label
        {
            Text = "Тариф: 350 ₽ / г",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = MutedTextColor
        };
        panel.Controls.Add(rateLabel, 3, 1);

        var addButton = new Button
        {
            Text = "+  Добавить",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = PrimaryColor,
            ForeColor = Color.FromArgb(24, 24, 27),
            Font = new Font("Segoe UI Semibold", 10F),
            Cursor = Cursors.Hand
        };
        addButton.FlatAppearance.BorderSize = 0;
        addButton.Click += AddEntry;
        panel.Controls.Add(addButton, 4, 1);
        return panel;
    }

    private Control BuildHistoryPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor, Padding = new Padding(0, 16, 0, 0) };
        var title = new Label
        {
            Text = "История записей",
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI Semibold", 13F),
            ForeColor = TextColor
        };
        panel.Controls.Add(title);

        ConfigureGrid();
        panel.Controls.Add(historyGrid);
        return panel;
    }

    private Control BuildTotalsPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = PanelColor,
            ColumnCount = 3,
            Padding = new Padding(18, 10, 18, 10)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));

        panel.Controls.Add(MakeTotalBlock("ПЛАНОВЫЙ ИТОГ", plannedTotalLabel, Color.FromArgb(130, 180, 220)), 0, 0);
        panel.Controls.Add(MakeTotalBlock("ФАКТИЧЕСКИЙ ИТОГ", actualTotalLabel, Color.FromArgb(130, 200, 145)), 1, 0);
        recordCountLabel.Dock = DockStyle.Fill;
        recordCountLabel.TextAlign = ContentAlignment.MiddleRight;
        recordCountLabel.ForeColor = MutedTextColor;
        panel.Controls.Add(recordCountLabel, 2, 0);
        return panel;
    }

    private static Label MakeCaption(string title, string subtitle) => new()
    {
        Text = $"{title}\r\n{subtitle}",
        Dock = DockStyle.Fill,
        ForeColor = MutedTextColor,
        TextAlign = ContentAlignment.BottomLeft
    };

    private static Control MakeTotalBlock(string title, Label valueLabel, Color accent)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(new Label { Text = title, AutoSize = true, ForeColor = accent, Location = new Point(0, 2) });
        valueLabel.AutoSize = true;
        valueLabel.Font = new Font("Segoe UI Semibold", 12F);
        valueLabel.ForeColor = TextColor;
        valueLabel.Location = new Point(0, 27);
        panel.Controls.Add(valueLabel);
        return panel;
    }

    private void ConfigureComboBox()
    {
        cladmanSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        cladmanSelector.Items.AddRange(Enumerable.Range(1, 20).Cast<object>().ToArray());
        cladmanSelector.SelectedIndex = 0;
        StyleInput(cladmanSelector);
    }

    private void ConfigureWeightInput()
    {
        weightInput.DecimalPlaces = 2;
        weightInput.Increment = 0.1m;
        weightInput.Maximum = 1_000_000;
        weightInput.Minimum = 0.01m;
        weightInput.ThousandsSeparator = true;
        weightInput.Value = 1;
        StyleInput(weightInput);
    }

    private void ConfigureRadio(RadioButton radio, string text, bool isChecked)
    {
        radio.Text = text;
        radio.Checked = isChecked;
        radio.AutoSize = true;
        radio.ForeColor = TextColor;
        radio.BackColor = PanelColor;
        radio.Margin = new Padding(0, 4, 15, 0);
    }

    private static void StyleInput(Control control)
    {
        control.Dock = DockStyle.Fill;
        control.BackColor = InputColor;
        control.ForeColor = TextColor;
        control.Margin = new Padding(0, 4, 12, 0);
    }

    private void ConfigureGrid()
    {
        historyGrid.Dock = DockStyle.Fill;
        historyGrid.BackgroundColor = BackgroundColor;
        historyGrid.BorderStyle = BorderStyle.None;
        historyGrid.GridColor = Color.FromArgb(62, 62, 68);
        historyGrid.RowHeadersVisible = false;
        historyGrid.AllowUserToAddRows = false;
        historyGrid.AllowUserToDeleteRows = false;
        historyGrid.ReadOnly = true;
        historyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        historyGrid.AutoGenerateColumns = false;
        historyGrid.EnableHeadersVisualStyles = false;
        historyGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = PanelColor,
            ForeColor = MutedTextColor,
            SelectionBackColor = PanelColor,
            SelectionForeColor = MutedTextColor,
            Padding = new Padding(5)
        };
        historyGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = InputColor,
            ForeColor = TextColor,
            SelectionBackColor = Color.FromArgb(86, 67, 43),
            SelectionForeColor = TextColor,
            Padding = new Padding(5)
        };
        AddTextColumn("Дата и время", "CreatedAt", 150);
        AddTextColumn("Кладмен", "CladmanNumber", 90);
        AddTextColumn("Тип", "Type", 130);
        AddTextColumn("Вес", "WeightGrams", 120);
        AddTextColumn("Сумма", "Amount", 150);
        historyGrid.DataSource = bindingSource;
    }

    private void AddTextColumn(string header, string propertyName, int width)
    {
        historyGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = header,
            Name = propertyName,
            Width = width,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DataPropertyName = propertyName
        });
    }

    private void AddEntry(object? sender, EventArgs e)
    {
        var entry = new CladEntry
        {
            CladmanNumber = (int)cladmanSelector.SelectedItem!,
            WeightGrams = weightInput.Value,
            Type = actualRadio.Checked ? EntryType.Actual : EntryType.Planned,
            CreatedAt = DateTime.Now
        };
        entries.Insert(0, entry);
        SaveAndRefresh();
    }

    private void SaveAndRefresh()
    {
        store.Save(entries);
        RefreshView();
    }

    private void RefreshView()
    {
        bindingSource.DataSource = entries.Select(entry => new
        {
            entry.Id,
            CreatedAt = entry.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
            CladmanNumber = $"№ {entry.CladmanNumber}",
            Type = entry.Type == EntryType.Planned ? "План" : "Факт",
            WeightGrams = $"{entry.WeightGrams.ToString("N2", CultureInfo.CurrentCulture)} г",
            Amount = FormatMoney(entry.Amount)
        }).ToList();

        var planned = entries.Where(entry => entry.Type == EntryType.Planned).ToList();
        var actual = entries.Where(entry => entry.Type == EntryType.Actual).ToList();
        plannedTotalLabel.Text = $"{planned.Sum(entry => entry.WeightGrams):N2} г   •   {FormatMoney(planned.Sum(entry => entry.Amount))}";
        actualTotalLabel.Text = $"{actual.Sum(entry => entry.WeightGrams):N2} г   •   {FormatMoney(actual.Sum(entry => entry.Amount))}";
        recordCountLabel.Text = $"Записей: {entries.Count}";
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("C", CultureInfo.GetCultureInfo("ru-RU"));
}
