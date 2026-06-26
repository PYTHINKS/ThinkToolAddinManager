using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using ThinkToolAddinManager.Command;
using ThinkToolAddinManager.View.Control;
using ImageSource = System.Windows.Media.ImageSource;
using System.Windows.Media;
using Orientation = System.Windows.Controls.Orientation;

namespace ThinkToolAddinManager;

public class App : ICadCommand
{
    public const string RibbonTitle = "Add-in Manager";
    public const string RibbonId = "ThinkToolAddinManager";

    [CommandMethod("InitThinkToolManager")]
    public override void Execute()
    {
        CreateRibbon();
    }

    private void CreateRibbon()
    {
        RibbonControl ribbon = ComponentManager.Ribbon;
        if (ribbon != null)
        {
            RibbonTab rtab = ribbon.FindTab(RibbonId);
            if (rtab != null)
            {
                ribbon.Tabs.Remove(rtab);
            }

            rtab = new RibbonTab();
            rtab.Title = RibbonTitle;
            rtab.Id = RibbonId;
            ribbon.Tabs.Add(rtab);
            AddContentToTab(rtab);
        }
    }

    private void AddContentToTab(RibbonTab rtab)
    {
        rtab.Panels.Add(AddPanelOne());
    }

    private static RibbonPanel AddPanelOne()
    {
        RibbonPanelSource rps = new RibbonPanelSource();
        rps.Title = "Add-in Manager";
        RibbonPanel rp = new RibbonPanel();
        rp.Source = rps;
        RibbonButton rci = new RibbonButton();
        rci.Name = "ThinkTool Add-in Manager";
        rps.DialogLauncher = rci;
        //create button1
        var addinAssembly = typeof(App).Assembly;
        RibbonButton btnPythonShell = new RibbonButton
        {
            Orientation = Orientation.Vertical,
            AllowInStatusBar = true,
            Size = RibbonItemSize.Large,
            Name = "Open ThinkTool Manager",
            ShowText = true,
            Text = "Add-in\nManager",
            Description = "Open ThinkTool Add-in Manager",
            Image = CreateManagerIcon(16),
            LargeImage = CreateManagerIcon(32),
            CommandHandler = new RelayCommand(new ThinkToolManagerCommand().Execute)
        };
        rps.Items.Add(btnPythonShell);
        //create button2
        RibbonButton btnConfig = new RibbonButton
        {
            Orientation = Orientation.Vertical,
            AllowInStatusBar = true,
            Size = RibbonItemSize.Large,
            Name = "Run Last Command",
            ShowText = true,
            Text = "Run Last\nCommand",
            Description = "Run the last selected add-in command",
            Image = CreateRunIcon(16),
            LargeImage = CreateRunIcon(32),
            CommandHandler = new RelayCommand(new ThinkToolManagerRunLast().Execute)
        };
        rps.Items.Add(btnConfig);
        return rp;
    }
    private static ImageSource CreateManagerIcon(double size)
    {
        var group = CreateIconBase(size, "#2563EB");
        group.Children.Add(CreateGeometry("#FFFFFF", "M8,9 H24 V12 H8 Z M8,15 H24 V18 H8 Z M8,21 H18 V24 H8 Z", size));
        return FreezeIcon(group);
    }

    private static ImageSource CreateRunIcon(double size)
    {
        var group = CreateIconBase(size, "#16A34A");
        group.Children.Add(CreateGeometry("#FFFFFF", "M12,8 L24,16 L12,24 Z", size));
        return FreezeIcon(group);
    }

    private static DrawingGroup CreateIconBase(double size, string background)
    {
        var group = new DrawingGroup();
        var scale = size / 32d;
        group.Transform = new ScaleTransform(scale, scale);
        group.Children.Add(new GeometryDrawing(
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(background)),
            null,
            Geometry.Parse("M6,3 H26 C27.66,3 29,4.34 29,6 V26 C29,27.66 27.66,29 26,29 H6 C4.34,29 3,27.66 3,26 V6 C3,4.34 4.34,3 6,3 Z")));
        return group;
    }

    private static GeometryDrawing CreateGeometry(string color, string path, double size)
    {
        return new GeometryDrawing(
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)),
            null,
            Geometry.Parse(path));
    }

    private static DrawingImage FreezeIcon(DrawingGroup group)
    {
        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }
}
