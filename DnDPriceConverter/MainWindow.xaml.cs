using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DnDPriceConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ICollection<string> usableStrings = new[] { "scroll", "potion", "arrow", "bolt", "oil" };

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var inputText = InputBox.Text;
            var modText = ModBox.Text;
            var modifier = int.Parse(modText);
            var detectUsable = DetectUsablesCheck.IsChecked ?? false;

            var lines = inputText.Split(Environment.NewLine);

            var itemList = new List<(string name, int cost)>();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var cost = GetCost(line);
                var modifiedCost = cost * modifier / 100;
                var name = GetName(line);

                if (detectUsable)
                {
                    var nameLowerCase = name.ToLowerInvariant();
                    if (usableStrings.Any(usableString => nameLowerCase.Contains(usableString)))
                    {
                        modifiedCost /= 2;
                    }
                }

                itemList.Add((name, modifiedCost));
            }


            var sb = new StringBuilder();
            foreach (var (name, cost) in itemList.OrderBy(il=>il.cost))
            {
                sb.AppendLine($"{name} {cost}sz");
            }
            OutputBox.Text = sb.ToString();
        }

        private IDictionary<string, (string dice, int basePrice)> rarityDict = new Dictionary<string, (string dice, int basePrice)>
        {
            ["common"] = ("1d6+1", 10),
            ["uncommon"] = ("1d6", 100),
            ["rare"] = ("2d10", 1_000),
            ["very rare"] = ("1d4+1", 10_000),
            ["legendary"] = ("2d6", 25_000)
        };

        private Random rnd = new Random();

        private  int GetCost(string line)
        {
            var lastIndexOfDot = line.LastIndexOf('.');
            if (lastIndexOfDot > 0)
            {
                var costText = line.Substring(lastIndexOfDot + 1).Trim();
                if (!string.IsNullOrEmpty(costText))
                {
                    costText = costText.Substring(0, costText.Length - 2).Trim();
                    return int.Parse(costText);
                }
            }
            var namePart = GetName(line);

            var rarityPart = namePart.Substring(namePart.LastIndexOf('(')+1);
            rarityPart = rarityPart.Substring(0, rarityPart.LastIndexOf(','));
            return GeneratePrice(rarityPart);
        }

        private static string GetName(string line)
        {
            var namePart = line.LastIndexOf('.') > 0 ? line.Substring(0, line.IndexOf('.')) : line;

            return namePart;
        }

        private int GeneratePrice(string rarity)
        {
            if (rarityDict.ContainsKey(rarity) == false)
            {
                return 0;
            }

            var (dice, basePrice) = rarityDict[rarity];
            var add = 0;
            var rollString = "";
            if (dice.Contains('+'))
            {
                add = int.Parse(dice.Substring(dice.IndexOf('+') + 1));
                rollString = dice.Substring(0, dice.IndexOf('+'));
            }
            else
            {
                rollString = dice;
            }

            var split = rollString.Split('d');
            var timesRoll = int.Parse(split[0]);
            var diceType = int.Parse(split[1]);
            var sum = add;
            for (int rollNumber = 0; rollNumber < timesRoll; rollNumber++)
            {
                var random = rnd.Next(1, diceType + 1);
                sum += random;
            }

            return basePrice * sum;
        }
    }
}
