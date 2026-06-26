using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ThinkToolAddinManager.Command;
using ThinkToolAddinManager.Model;
using ThinkToolAddinManager.View;
using ThinkToolAddinManager.View.Control;
using Microsoft.Win32;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ThinkToolAddinManager.ViewModel;

public class AddInManagerViewModel : ViewModelBase
{
    //public AddInPlugin ExternalCommandData { get; set; }
    public FrmAddInManager FrmAddInManager { get; set; }
    public AssemLoader AssemLoader { get; set; }

    public ThinkToolManagerBase MThinkToolManagerBase { get; set; }

    private ObservableCollection<AddinModel> commandItems;
    
    public ObservableCollection<AddinModel> CommandItems
    {
        get => commandItems;
        set
        {
            if (value == commandItems) return;
            commandItems = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<AddinModel> lispFunctionItems;

    public ObservableCollection<AddinModel> LispFunctionItems
    {
        get => lispFunctionItems;
        set
        {
            if (value == lispFunctionItems) return;
            lispFunctionItems = value;
            OnPropertyChanged();
        }
    }

    private AddinModel selectedCommandItem;

    public AddinModel SelectedCommandItem
    {
        get
        {
            if (selectedCommandItem != null && selectedCommandItem.IsParentTree == true && IsTabCmdSelected)
            {
                IsCanRun = false;
                MThinkToolManagerBase.ActiveCmd = selectedCommandItem.Addin;
            }
            else if (selectedCommandItem != null && selectedCommandItem.IsParentTree == false && IsTabCmdSelected)
            {
                IsCanRun = true;
                MThinkToolManagerBase.ActiveCmdItem = selectedCommandItem.AddinItem;
                MThinkToolManagerBase.ActiveCmd = selectedCommandItem.Addin;
                VendorDescription = MThinkToolManagerBase.ActiveCmdItem.Description;
            }
            else IsCanRun = false;
            return selectedCommandItem;
        }
        set => OnPropertyChanged(ref selectedCommandItem, value);
    }
    private AddinModel selectedLispItem;

    public AddinModel SelectedLispItem
    {
        get
        {
            if (selectedLispItem != null && selectedLispItem.IsParentTree == true && IsTabLispSelected)
            {
                MThinkToolManagerBase.ActiveLisp = selectedLispItem.Addin;
            }
            else if (selectedLispItem != null && selectedLispItem.IsParentTree == false && IsTabLispSelected)
            {
                MThinkToolManagerBase.ActiveLispItem = selectedLispItem.AddinItem;
                MThinkToolManagerBase.ActiveLisp = selectedLispItem.Addin;
                VendorDescription = MThinkToolManagerBase.ActiveLispItem.Description;
            }
            return selectedLispItem;
        }
        set => OnPropertyChanged(ref selectedLispItem, value);
    }

    public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
    public ICommand ClearCommand => new RelayCommand(ClearCommandClick);

    public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
    private ICommand _executeAddinCommand;
    public ICommand ExecuteAddinCommand => _executeAddinCommand ??= new RelayCommand(ExecuteAddinCommandClick);
    public ICommand OpenLcAssemblyCommand => new RelayCommand(OpenLcAssemblyCommandClick);
    public ICommand ReloadCommand => new RelayCommand(ReloadCommandClick);
    public ICommand FreshSearch => new RelayCommand(FreshSearchClick);

    private string searchText;

    public string SearchText
    {
        get => searchText;
        set
        {
            if (value == searchText) return;
            searchText = value;
            OnPropertyChanged();
            FreshSearchClick();
        }
    }

    private bool isCurrentVersion = true;

    public bool IsCurrentVersion
    {
        get => isCurrentVersion;
        set => OnPropertyChanged(ref isCurrentVersion, value);
    }

    private ObservableCollection<CadAddin> addinStartup;

    public ObservableCollection<CadAddin> AddInStartUps
    {
        get { return addinStartup ??= new ObservableCollection<CadAddin>(); }
        set => OnPropertyChanged(ref addinStartup, value);
    }

    public ICommand HelpCommand => new RelayCommand(HelpCommandClick);

    public string AppTitle => "ThinkTool Add-in Manager";
    public string VersionRangeText => "AutoCAD 2020-2026";
    public string HelpText => "Help";
    public string HelpTooltip => "Open the ThinkTool Add-in Manager GitHub page";
    public string TempFilesText => "Temp Files";
    public string TempFilesTooltip => "Open the temporary folder used for copied add-in assemblies and logs";
    public string SearchTooltip => "Search commands";
    public string LoadDllText => "Load DLL";
    public string ReloadText => "Reload";
    public string RemoveText => "Remove";
    public string CommandsText => "Commands";
    public string LispText => "Autolisp";
    public string OutputText => "Output";
    public string OpenFolderText => "Open Folder";
    public string RunSelectedText => "Run Selected";
    public string VendorDescriptionTooltip => "Vendor description";
    public string ContextLoadDllText => "Load DLL";
    public string ContextReloadText => "Reload";
    public string ContextRunText => "Run";
    public string ContextOpenLocationText => "Open Location";
    public string ContextRemoveText => "Remove";

    private string vendorDescription = string.Empty;

    public string VendorDescription
    {
        get => vendorDescription;
        set => OnPropertyChanged(ref vendorDescription, value);
    }

    private bool isTabCmdSelected = true;

    public bool IsTabCmdSelected
    {
        get => isTabCmdSelected;
        set => OnPropertyChanged(ref isTabCmdSelected, value);
    }
    private bool isTabAppSelected;

    public bool IsTabLispSelected
    {
        get
        {
            if (isTabAppSelected) IsCanRun = false;
            return isTabAppSelected;
        }
        set => OnPropertyChanged(ref isTabAppSelected, value);
    }

    private bool isCanRun;

    public bool IsCanRun
    {
        get => isCanRun;
        set => OnPropertyChanged(ref isCanRun, value);
    }

    private void HelpCommandClick()
    {
        OpenWithShell(DefaultSetting.HelpUrl);
    }

    public AddInManagerViewModel()
    {
        AssemLoader = new AssemLoader();
        MThinkToolManagerBase = ThinkToolManagerBase.Instance;
        CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
        LispFunctionItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.LispFunctions);
    }

    private ObservableCollection<AddinModel> FreshTreeItems(bool isSearchText, Addins addins)
    {
        var MainTrees = new ObservableCollection<AddinModel>();
        foreach (var keyValuePair in addins.AddinDict)
        {
            var addin = keyValuePair.Value;
            var title = keyValuePair.Key;
            var addinItemList = addin.ItemList;
            var addinModels = new List<AddinModel>();
            foreach (var addinItem in addinItemList)
            {
                if (isSearchText)
                {
                    if (addinItem.FullClassName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        addinModels.Add(new AddinModel(addinItem.FullClassName)
                        {
                            IsChecked = true,
                            Addin = addin,
                            AddinItem = addinItem,
                        });
                    }
                }
                else
                {
                    addinModels.Add(new AddinModel(addinItem.FullClassName)
                    {
                        IsChecked = true,
                        Addin = addin,
                        AddinItem = addinItem,
                    });
                }
            }
            var root = new AddinModel(title)
            {
                IsChecked = true,
                Children = addinModels,
                IsParentTree = true,
                Addin = addin,
            };
            root.Initialize();
            MainTrees.Add(root);
        }

        return MainTrees;
    }

    public void ExecuteAddinCommandClick()
    {
        try
        {
            if (SelectedCommandItem?.IsParentTree == false && FrmAddInManager != null)
            {
                MThinkToolManagerBase.ActiveCmd = SelectedCommandItem.Addin;
                MThinkToolManagerBase.ActiveCmdItem = SelectedCommandItem.AddinItem;
                CheckCountSelected(CommandItems, out var result);
                if (result > 0)
                {
                    FrmAddInManager.Close();
                    Execute();
                }
            }
            if (SelectedLispItem?.IsParentTree == false && FrmAddInManager != null)
            {
                MThinkToolManagerBase.ActiveLisp = SelectedLispItem.Addin;
                MThinkToolManagerBase.ActiveLispItem = SelectedLispItem.AddinItem;
                CheckCountSelected(LispFunctionItems, out var result);
                if (result > 0)
                {
                    //TODO
                    // FrmAddInManager.Close();
                    // Execute();

                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
    private void Execute()
    {
        MThinkToolManagerBase.RunActiveCommand();
    }

    private void OpenLcAssemblyCommandClick()
    {
        bool flag = MThinkToolManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MThinkToolManagerBase.ActiveCmd.FilePath;
        if (!File.Exists(path))
        {
            ShowFileNotExit(path);
            return;
        }
        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
        {
            UseShellExecute = true
        });
    }

    private void ShowFileNotExit(string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(Resource.FileNotExit);
        sb.AppendLine("Path :");
        sb.AppendLine(path);
        MessageBox.Show(sb.ToString(), DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    private void CheckCountSelected(ObservableCollection<AddinModel> addinModels, out int result)
    {
        result = 0;
        foreach (var addinModel in addinModels)
        {
            if (addinModel.IsInitiallySelected) result++;
            foreach (var modelChild in addinModel.Children)
            {
                if (modelChild.IsInitiallySelected) result++;
            }
        }
    }

    private void LoadCommandClick()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = @"assembly files (*.dll)|*.dll|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        var fileName = openFileDialog.FileName;
        if (!File.Exists(fileName)) return;
        LoadAssemblyCommand(fileName);
    }

    private void ReloadCommandClick()
    {
        if (SelectedCommandItem == null)
        {
            SortedDictionary<string, Addin> Commands = MThinkToolManagerBase.ThinkToolManager.Commands.AddinDict;
            SortedDictionary<string, Addin> OldCommands = new SortedDictionary<string, Addin>(Commands);
            foreach (var Command in OldCommands.Values)
            {
                string fileName = Command.FilePath;
                if (File.Exists(fileName)) MThinkToolManagerBase.ThinkToolManager.LoadAddin(fileName, AssemLoader);
            }
            MThinkToolManagerBase.ThinkToolManager.SaveToAimIni();
            CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
            return;
        }
        bool flag = MThinkToolManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MThinkToolManagerBase.ActiveCmd.FilePath;
        if (!File.Exists(path)) return;
        LoadAssemblyCommand(path);
    }

    private void LoadAssemblyCommand(string fileName)
    {
        var addinType = MThinkToolManagerBase.ThinkToolManager.LoadAddin(fileName, AssemLoader);
        switch (addinType)
        {
            case AddinType.Invalid:
                MessageBox.Show(Resource.LoadInvalid);
                return;
            case AddinType.Command:
                IsTabCmdSelected = true;
                FrmAddInManager.TabCommand.Focus();
                break;
            case AddinType.LispFunction:
                IsTabLispSelected = true;
                FrmAddInManager.TabLispFunction.Focus();
                break;
            case AddinType.Mixed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
       
        MThinkToolManagerBase.ThinkToolManager.SaveToAimIni();
        CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
        LispFunctionItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.LispFunctions);
    }
    private void RemoveAddinClick()
    {
        try
        {
            if (IsTabCmdSelected)
            {
                foreach (var parent in CommandItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MThinkToolManagerBase.ActiveCmd = parent.Addin;
                        MThinkToolManagerBase.ActiveCmdItem = parent.AddinItem;
                        if (MThinkToolManagerBase.ActiveCmd != null)
                        {
                            MThinkToolManagerBase.ThinkToolManager.Commands.RemoveAddIn(MThinkToolManagerBase.ActiveCmd);
                        }
                        MThinkToolManagerBase.ActiveCmd = null;
                        MThinkToolManagerBase.ActiveCmdItem = null;
                        CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            MThinkToolManagerBase.ActiveCmd = parent.Addin;
                            MThinkToolManagerBase.ActiveCmdItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MThinkToolManagerBase.ActiveCmdItem != null)
                {
                    MThinkToolManagerBase.ActiveCmd.RemoveItem(MThinkToolManagerBase.ActiveCmdItem);
                    MThinkToolManagerBase.ActiveCmd = null;
                    MThinkToolManagerBase.ActiveCmdItem = null;
                }
                CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
            }
            if (IsTabLispSelected)
            {
                foreach (var parent in LispFunctionItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MThinkToolManagerBase.ActiveLisp = parent.Addin;
                        MThinkToolManagerBase.ActiveLispItem = parent.AddinItem;
                        if (MThinkToolManagerBase.ActiveLisp != null)
                        {
                            MThinkToolManagerBase.ThinkToolManager.LispFunctions.RemoveAddIn(MThinkToolManagerBase.ActiveLisp);
                        }
                        MThinkToolManagerBase.ActiveLisp = null;
                        MThinkToolManagerBase.ActiveLispItem = null;
                        LispFunctionItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.LispFunctions);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            MThinkToolManagerBase.ActiveLisp = parent.Addin;
                            MThinkToolManagerBase.ActiveLispItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MThinkToolManagerBase.ActiveLispItem != null)
                {
                    MThinkToolManagerBase.ActiveLisp.RemoveItem(MThinkToolManagerBase.ActiveLispItem);
                    MThinkToolManagerBase.ActiveLisp = null;
                    MThinkToolManagerBase.ActiveLispItem = null;
                }
                LispFunctionItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.LispFunctions);
            }
            //Save All SetTings
            MThinkToolManagerBase.ThinkToolManager.SaveToAimIni();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    private void FreshSearchClick()
    {
        var flag = string.IsNullOrEmpty(searchText);
        if (IsTabCmdSelected)
        {
            if (flag)
            {
                CommandItems = FreshTreeItems(false, MThinkToolManagerBase.ThinkToolManager.Commands);
                return;
            }
            CommandItems = FreshTreeItems(true, MThinkToolManagerBase.ThinkToolManager.Commands);
        }
    }
    private void ClearCommandClick()
    {
        var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", DefaultSetting.TempFolderName);
        if (Directory.Exists(tempFolder))
        {
            OpenWithShell(tempFolder);
        }
    }

    private static void OpenWithShell(string target)
    {
        Process.Start(new ProcessStartInfo(target)
        {
            UseShellExecute = true
        });
    }

}
