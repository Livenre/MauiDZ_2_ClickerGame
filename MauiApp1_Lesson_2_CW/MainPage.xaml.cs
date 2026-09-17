using System.Text.Json;
using System.IO;

namespace MauiApp1_Lesson_2_CW;

public partial class MainPage : ContentPage
{
    private bool isGameRunning = true;

    private int coins = 0;
    private int clickPower = 1;
    private int autoClickers = 0;

    private int upgradeCost = 10;
    private int autoClickerCost = 50;
    private int critUpgradeCost = 100;
    private int speedUpgradeCost = 150;

    private Random random = new Random();
    private double critChance = 0.0;
    private int critMultiplier = 5;

    private int autoClickerDelay = 1000;
    private const int MinAutoClickerDelay = 100;

    public class SaveData
    {
        public int Coins { get; set; }
        public int ClickPower { get; set; }
        public int AutoClickers { get; set; }
        public int UpgradeCost { get; set; }
        public int AutoClickerCost { get; set; }
        public int CritUpgradeCost { get; set; }
        public int SpeedUpgradeCost { get; set; }
        public double CritChance { get; set; }
        public int AutoClickerDelay { get; set; }
    }

    public MainPage()
    {
        InitializeComponent();
        StartAutoClicker();

        LoadProgress();

        UpdateUI();
    }

    private void OnClickButtonClicked(object sender, EventArgs e)
    {
        if (critChance > 0 && random.NextDouble() < critChance)
        {
            coins += clickPower * critMultiplier;
        }
        else
        {
            coins += clickPower;
        }
        UpdateUI();
    }

    private void OnUpgradeClicked(object sender, EventArgs e)
    {
        if (coins >= upgradeCost)
        {
            coins -= upgradeCost;
            clickPower++;
            upgradeCost *= 2;
            UpdateUI();
        }
    }

    private void OnBuyAutoClickerClicked(object sender, EventArgs e)
    {
        if (coins >= autoClickerCost)
        {
            coins -= autoClickerCost;
            autoClickers++;
            autoClickerCost *= 2;
            UpdateUI();
        }
    }

    private void OnUpgradeCritClicked(object sender, EventArgs e)
    {
        if (coins >= critUpgradeCost)
        {
            coins -= critUpgradeCost;
            critChance += 0.005;
            critUpgradeCost = (int)(critUpgradeCost * 2.5);
            UpdateUI();
        }
    }

    private void OnUpgradeSpeedClicked(object sender, EventArgs e)
    {
        if (coins >= speedUpgradeCost && autoClickerDelay > MinAutoClickerDelay)
        {
            coins -= speedUpgradeCost;
            autoClickerDelay -= 100;
            speedUpgradeCost *= 2;
            UpdateUI();
        }
    }

    private async void StartAutoClicker()
    {
        while (isGameRunning)
        {
            await Task.Delay(autoClickerDelay);

            if (!isGameRunning)
                break;

            if (autoClickers > 0)
            {
                coins += autoClickers;

                try
                {
                    UpdateUI();
                }
                catch
                {

                }
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        isGameRunning = false;

        SaveProgress();
    }

    // Треба замінити шлях на актуальний для вашої системи, де ви хочете зберігати файл savegame.json
    private string saveFilePath = @"D:\C#2026_mobile_2\MauiApp1_Lesson_2_CW\MauiApp1_Lesson_2_CW\savegame.json";
    private void SaveProgress()
    {
        var data = new SaveData
        {
            Coins = coins,
            ClickPower = clickPower,
            AutoClickers = autoClickers,
            UpgradeCost = upgradeCost,
            AutoClickerCost = autoClickerCost,
            CritUpgradeCost = critUpgradeCost,
            SpeedUpgradeCost = speedUpgradeCost,
            CritChance = critChance,
            AutoClickerDelay = autoClickerDelay
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(saveFilePath, json);
    }

    private void LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);

            if (string.IsNullOrWhiteSpace(json))
                return;

            try
            {
                var data = JsonSerializer.Deserialize<SaveData>(json);

                if (data != null)
                {
                    coins = data.Coins;
                    clickPower = data.ClickPower;
                    autoClickers = data.AutoClickers;
                    upgradeCost = data.UpgradeCost;
                    autoClickerCost = data.AutoClickerCost;
                    critUpgradeCost = data.CritUpgradeCost;
                    speedUpgradeCost = data.SpeedUpgradeCost;
                    critChance = data.CritChance;
                    autoClickerDelay = data.AutoClickerDelay;
                }
            }
            catch (JsonException)
            {
            }
        }
    }

    private void UpdateUI()
    {
        CoinsLabel.Text = $"Монети: {coins}";

        ClickPowerLabel.Text = $"За клік: {clickPower}";
        AutoClickerLabel.Text = $"Автоклікери: {autoClickers}";

        CritLabel.Text = $"Шанс криту: {critChance * 100:F1}% (x{critMultiplier})";

        double clicksPerSecond = 1000.0 / autoClickerDelay;
        DelayLabel.Text = $"Швидкість автоклікера: {clicksPerSecond:F1} кліків/сек";

        CritContainer.IsVisible = critChance > 0;
        DelayContainer.IsVisible = autoClickers > 0;

        UpgradeButton.Text = $"Покращити клік — {upgradeCost} монет";
        AutoClickerButton.Text = $"Купити автоклікер — {autoClickerCost} монет";
        CritButton.Text = $"Шанс криту +0.5% — {critUpgradeCost} монет";

        SpeedButton.IsVisible = autoClickers > 0;

        if (autoClickerDelay <= MinAutoClickerDelay)
        {
            SpeedButton.Text = "Максимальна швидкість!";
            SpeedButton.IsEnabled = false;
        }
        else
        {
            SpeedButton.Text = $"Прискорити автоклікер — {speedUpgradeCost} монет";
        }

        SaveProgress();
    }

}