using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels;

namespace InstallVibe.Views
{
    public sealed partial class UpdateBanner : UserControl
    {
        public UpdateViewModel ViewModel { get; }

        public UpdateBanner()
        {
            this.InitializeComponent();

            // Inject via DI
            ViewModel = App.Services.GetRequiredService<UpdateViewModel>();
        }

        /// <summary>
        /// Gets appropriate icon glyph based on update state
        /// </summary>
        private string GetIconGlyph(bool isOffline, bool isMandatory)
        {
            if (isOffline)
                return "\uE704";  // CloudOffline

            if (isMandatory)
                return "\uE7BA";  // Warning

            return "\uE895";  // CloudDownload
        }
    }
}
