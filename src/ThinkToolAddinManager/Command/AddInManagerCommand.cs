using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using ThinkToolAddinManager.Model;
using Exception = System.Exception;

namespace ThinkToolAddinManager.Command;


public class ThinkToolManagerCommand : ICadCommand
{
    
    [CommandMethod("ThinkToolManager",CommandFlags.Session)]
    public override void Execute()
    {
        Trace.Listeners.Clear();
        Trace.Listeners.Clear();
        CodeListener codeListener = new CodeListener();
        Trace.Listeners.Add(codeListener);
        ThinkToolManagerBase.Instance.ExecuteCommand(false);
    }
}

public class ThinkToolManagerRunLast  : ICadCommand
{
    [Autodesk.AutoCAD.Runtime.CommandMethod("ThinkToolManagerRunLast",CommandFlags.Session)]
    public override void Execute()
    {
        try
        {
            ThinkToolManagerBase.Instance.ExecuteCommand(true);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
            Trace.WriteLine(e.Message);
        }
    }
}

