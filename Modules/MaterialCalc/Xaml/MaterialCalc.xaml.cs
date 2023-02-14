using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ArkHelper.Xaml.Control;
using System.Windows;
using System;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Win32;
using Windows.Foundation;

namespace ArkHelper.Modules.MaterialCalc.Xaml
{
    /// <summary>
    /// MaterialCalc.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialCalc : Page
    {
        public class MaterialStatus
        {
            public int Number
            {
                get
                {
                    return UI.Number;
                }
                set
                {
                    UI.Number = value;
                    if (!Vis0 && value == 0) UI.Visibility = Visibility.Collapsed;
                    else UI.Visibility = Visibility.Visible;
                }
            }

            public MaterialStatus(MaterialUnit ui)
            {
                UI = ui;
            }
            public MaterialUnit UI { get; set; }
        }

        private Dictionary<Material, MaterialStatus> MaterialData = new Dictionary<Material, MaterialStatus>();

        public MaterialCalc()
        {
            InitializeComponent();
            Vis0 = true;

            foreach (var level in Info.Data.Keys.ToList())
            {
                var materials = new WrapPanel()
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Orientation = Orientation.Horizontal,
                };

                mode.Items.Add(new ComboBoxItem()
                { Content = "全部转换为" + level + "稀有度材料", Tag = "level_" + level });

                Levels.Children.Add(new StackPanel()
                {
                    Name = "materials_Level" + level,
                    Margin = new Thickness(0, 0, 0, 20),
                    Children =
                    {
                        new ChapterTitle()
                        {
                            Icon = MaterialDesignThemes.Wpf.PackIconKind.Star,
                            Text = "稀有度:" + level
                        },
                        materials
                    }
                });
            }

            Func<int, WrapPanel> getTarget = GetTarget;
            Func<WrapPanel, Material, MaterialUnit> addUnit = AddUnitIntoBox;
            Action init = new Action(() =>
            {
                foreach (var pair in Info.Data)
                {
                    var getTargetResult = this.Dispatcher.BeginInvoke(getTarget, DispatcherPriority.Background, pair.Key);
                    getTargetResult.Wait();
                    foreach (var material in pair.Value)
                    {
                        var getUIResult = this.Dispatcher.BeginInvoke(addUnit, DispatcherPriority.Background, getTargetResult.Result as WrapPanel, material);
                        getUIResult.Wait();
                        MaterialData.Add(material, new MaterialStatus((MaterialUnit)getUIResult.Result));
                    }
                }

            });
            init.BeginInvoke(null, null);
        }
        MaterialUnit AddUnitIntoBox(WrapPanel targetWppel, Material material)
        {
            var UI = new MaterialUnit(material, 0);
            targetWppel.Children.Add(UI);
            return UI;
        }
        WrapPanel GetTarget(int key)
        {
            WrapPanel targetWppel = null;
            foreach (var UIE in Levels.Children)
            {
                if (UIE is StackPanel)
                {
                    var UIEStack = (StackPanel)UIE;
                    if (UIEStack.Name != "materials_Level" + key) continue;

                    if (UIEStack.Children[1] is WrapPanel)
                    {
                        targetWppel = (WrapPanel)UIEStack.Children[1];
                        break;
                    }

                }
            }
            return targetWppel;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var calcMode = (mode.SelectedItem as ComboBoxItem).Tag as string;
            switch (calcMode)
            {
                case "Auto":
                    CalcLevel(2);
                    break;
                case "UntilEqualZero":
                    CalcLevel(1);
                    break;
                default:
                    var level = Convert.ToInt32(calcMode.Replace("level_", ""));
                    CalcLevel(level); 
                    break;
                    
            }
            void CalcLevel(int num)
            {
                foreach (var pair in MaterialData)
                {
                    if (pair.Key.level <= num) continue;
                    if (pair.Key.equal.Count == 0) continue;
                    foreach (var eqPair in pair.Key.equal)
                    {
                        MaterialData[eqPair.Key].Number += pair.Value.Number * eqPair.Value;
                    }
                    pair.Value.Number = 0;
                }
                
            }
        }

        private static bool Vis0 = false;
        private void ChangeVis()
        {
            if (!Vis0) // 不显示0
            {
                foreach (var pair in MaterialData)
                {
                    if (pair.Value.Number == 0)
                        pair.Value.UI.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                foreach (var pair in MaterialData)
                {
                    pair.Value.UI.Visibility = Visibility.Visible;
                }
            }

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Vis0 = !(bool)vis.IsChecked;
            ChangeVis();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            JObject json = new JObject();
            foreach (var pair in MaterialData)
            {
                if (pair.Value.Number != 0)
                    json.Add(pair.Key.name, pair.Value.Number);
            }
            var dia = new SaveFileDialog()
            {
                Title = "保存json文件",
                Filter = "json文件(*.json)|*.json",
                FileName = "result"
            };
            dia.ShowDialog();
            File.WriteAllText(dia.FileName, json.ToString());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var pair in MaterialData)
            {
                pair.Value.Number = 0;

            }
        }
    }

















}
