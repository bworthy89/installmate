using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace InstallVibe.Services;

public interface IMediaStorageService
{
    Task<string?> PickAndSaveImageAsync();
    Task<string?> PickAndSaveVideoAsync();
    Task<bool> DeleteMediaAsync(string filePath);
    string GetMediaFullPath(string relativePath);
}

public class MediaStorageService : IMediaStorageService
{
    private readonly string _mediaFolderPath;

    public MediaStorageService()
    {
        // Create media folder in local app data
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataFolder, "InstallVibe");
        _mediaFolderPath = Path.Combine(appFolder, "Media");

        // Ensure directory exists
        Directory.CreateDirectory(_mediaFolderPath);
    }

    public async Task<string?> PickAndSaveImageAsync()
    {
        try
        {
            var picker = new FileOpenPicker();

            // Get the window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // Configure picker for images
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            var file = await picker.PickSingleFileAsync();
            if (file == null) return null;

            return await SaveFileAsync(file);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking image: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> PickAndSaveVideoAsync()
    {
        try
        {
            var picker = new FileOpenPicker();

            // Get the window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            // Configure picker for videos
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".mkv");
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            var file = await picker.PickSingleFileAsync();
            if (file == null) return null;

            return await SaveFileAsync(file);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking video: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SaveFileAsync(StorageFile file)
    {
        // Generate unique filename
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var extension = Path.GetExtension(file.Name);
        var newFileName = $"{timestamp}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{extension}";
        var destinationPath = Path.Combine(_mediaFolderPath, newFileName);

        // Copy file to media folder
        using (var sourceStream = await file.OpenStreamForReadAsync())
        using (var destinationStream = File.Create(destinationPath))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }

        // Return relative path (for database storage)
        return $"/Media/{newFileName}";
    }

    public Task<bool> DeleteMediaAsync(string filePath)
    {
        try
        {
            var fullPath = GetMediaFullPath(filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                System.Diagnostics.Debug.WriteLine($"Deleted media file: {fullPath}");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting media: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public string GetMediaFullPath(string relativePath)
    {
        // Convert relative path to full path
        if (relativePath.StartsWith("/Media/"))
        {
            var fileName = relativePath.Substring(7); // Remove "/Media/"
            return Path.Combine(_mediaFolderPath, fileName);
        }
        return relativePath;
    }
}
