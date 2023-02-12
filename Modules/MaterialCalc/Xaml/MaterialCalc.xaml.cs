using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.Json;
using System.Windows.Controls;
using OpenCvSharp.Aruco;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls.Maps;
using ArkHelper.Xaml.Control;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using static ArkHelper.ArkHelperDataStandard.Data.SCHT;
using System.Windows.Media.Media3D;
using System;
using System.Windows.Threading;

namespace ArkHelper.Modules.MaterialCalc.Xaml
{
    /// <summary>
    /// MaterialCalc.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialCalc : Page
    {
        private Dictionary<Material, MaterialStatus> MaterialData = new Dictionary<Material, MaterialStatus>();
        public MaterialCalc()
        {
            InitializeComponent();

            foreach (var level in Info.Data.Keys.ToList())
            {
                var materials = new WrapPanel()
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Orientation = Orientation.Horizontal,
                };

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
            Action<WrapPanel,Material> addUnit = AddUnitIntoBox;
            Action init = new Action(() =>
            {
                foreach (var pair in Info.Data)
                {
                    var getTargetResult = this.Dispatcher.BeginInvoke(getTarget, DispatcherPriority.Background, pair.Key);
                    getTargetResult.Wait();
                    foreach (var material in pair.Value)
                    {
                        this.Dispatcher.BeginInvoke(addUnit, DispatcherPriority.Background, getTargetResult.Result as WrapPanel, material);
                    }
                }
                
            });
            init.BeginInvoke(null, null);
        }
        void AddUnitIntoBox(WrapPanel targetWppel,Material material)
        {
            targetWppel.Children.Add(new MaterialUnit(material, 0));
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

        void Start()
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }

















}
