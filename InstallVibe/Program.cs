using Microsoft.UI.Xaml;

WinRT.ComWrappersSupport.InitializeComWrappers();
Application.Start((p) =>
{
    var context = new DispatcherQueueSynchronizationContext(
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
    System.Threading.SynchronizationContext.SetSynchronizationContext(context);
    new InstallVibe.App();
});
