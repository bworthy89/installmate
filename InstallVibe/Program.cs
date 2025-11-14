using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

// Note: No namespace - to avoid conflicts with WinUI 3 code generation
class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start((p) =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new InstallVibe.App();
        });
    }
}
