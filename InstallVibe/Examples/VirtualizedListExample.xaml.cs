using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace InstallVibe.Examples
{
    /// <summary>
    /// Example page demonstrating efficient list virtualization with ItemsRepeater
    /// </summary>
    public sealed partial class VirtualizedListExample : Page
    {
        public VirtualizedListExample()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles pointer entering a card to provide visual feedback
        /// </summary>
        private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.RenderTransform is TranslateTransform transform)
            {
                // Subtle lift animation on hover
                transform.Y = -2;
            }
        }

        /// <summary>
        /// Handles pointer exiting a card to reset visual state
        /// </summary>
        private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border && border.RenderTransform is TranslateTransform transform)
            {
                // Reset position
                transform.Y = 0;
            }
        }

        /// <summary>
        /// Handles thumbnail image load completion
        /// </summary>
        private void Thumbnail_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (sender is Image image)
            {
                // Fade in animation could be added here
                // For now, just ensure visibility is correct
                image.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Called when an element is prepared for display in the ItemsRepeater (virtualization)
        /// Use this for lazy loading resources like images
        /// </summary>
        private void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            // This is called when an item container is prepared for display
            // You can use this to lazy-load images or other resources
            // The args.Index tells you which item is being prepared
        }

        /// <summary>
        /// Called when an element is being recycled/cleared from the ItemsRepeater
        /// Use this to clean up resources
        /// </summary>
        private void ItemsRepeater_ElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs args)
        {
            // This is called when an item container is being recycled
            // Clean up any resources or cancel any pending operations
            // Good practice to unload images to save memory
            if (args.Element is FrameworkElement element)
            {
                // Find any Image elements and clear their source to free memory
                ClearImagesRecursive(element);
            }
        }

        /// <summary>
        /// Recursively clears image sources to free memory when elements are recycled
        /// </summary>
        private void ClearImagesRecursive(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Image image)
                {
                    image.Source = null;
                }

                ClearImagesRecursive(child);
            }
        }
    }
}
