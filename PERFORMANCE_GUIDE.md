# InstallVibe Performance & UX Best Practices

This document outlines performance optimization strategies and UX patterns to keep InstallVibe responsive and snappy, even with large datasets.

## 🚀 Core Performance Principles

### 1. Never Block the UI Thread

**Rule**: All I/O operations (database, file system, network) must be async.

**Bad**:
```csharp
public void LoadGuides()
{
    var guides = _repository.GetAllGuides(); // BLOCKS UI THREAD
    Guides = new ObservableCollection<Guide>(guides);
}
```

**Good**:
```csharp
public async Task LoadGuidesAsync()
{
    var guides = await _repository.GetAllGuidesAsync(); // Async
    Guides = new ObservableCollection<Guide>(guides);
}
```

### 2. Use Virtualization for Long Lists

**Always** use `ItemsRepeater` with `StackLayout` or `UniformGridLayout` for lists with 20+ items.

**Example**:
```xml
<ItemsRepeater ItemsSource="{x:Bind ViewModel.Steps}">
    <ItemsRepeater.Layout>
        <StackLayout Spacing="8" />
    </ItemsRepeater.Layout>
    <ItemsRepeater.ItemTemplate>
        <DataTemplate>
            <!-- Item template -->
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

**Why**: Only visible items are rendered, saving memory and CPU.

### 3. Lazy Load Images and Thumbnails

**Pattern**: Load thumbnails on-demand when items become visible.

```csharp
private async void ItemsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
{
    if (args.Element is FrameworkElement element &&
        element.DataContext is StepViewModel step)
    {
        if (!step.IsThumbnailLoaded)
        {
            await step.LoadThumbnailAsync(); // Lazy load
        }
    }
}
```

**Image Optimization**:
- List thumbnails: 80x80px, low quality (70%)
- Detail view: 800x600px, high quality (90%)
- Cache to disk with LRU eviction

### 4. Throttle Search Inputs (Debouncing)

**Pattern**: Wait for user to stop typing before searching.

```csharp
private DispatcherTimer _searchDebounceTimer;

partial void OnSearchTextChanged(string value)
{
    _searchDebounceTimer?.Stop();
    _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };

    _searchDebounceTimer.Tick += (s, e) =>
    {
        _searchDebounceTimer.Stop();
        PerformSearch(value);
    };

    _searchDebounceTimer.Start();
}
```

**Why**: Prevents running expensive search operations on every keystroke.

### 5. Cache Expensive Computations

**Pattern**: Cache results that don't change frequently.

```csharp
private List<Guide>? _cachedGuides;
private DateTime? _cacheExpiry;

public async Task<List<Guide>> GetGuidesAsync()
{
    if (_cachedGuides != null && _cacheExpiry > DateTime.Now)
    {
        return _cachedGuides; // Return cached
    }

    _cachedGuides = await _repository.GetAllGuidesAsync();
    _cacheExpiry = DateTime.Now.AddMinutes(5);

    return _cachedGuides;
}
```

### 6. Use Background Tasks for Heavy Operations

**Pattern**: Offload CPU-intensive work to background threads.

```csharp
public async Task ProcessLargeDatasetAsync()
{
    IsProcessing = true;

    await Task.Run(() =>
    {
        // CPU-intensive work on background thread
        var result = ExpensiveComputation();
        return result;
    });

    IsProcessing = false;
}
```

## 🎨 UX Performance Patterns

### Loading States

**Always show feedback** for operations taking > 200ms.

```xml
<!-- Loading spinner -->
<ProgressRing IsActive="{x:Bind ViewModel.IsLoading, Mode=OneWay}"
              Width="32" Height="32" />

<!-- Skeleton screens for lists -->
<ItemsRepeater Visibility="{x:Bind ViewModel.IsLoading, Mode=OneWay, Converter={StaticResource BoolToVisibilityConverter}}">
    <ItemsRepeater.ItemTemplate>
        <DataTemplate>
            <SkeletonCard />
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

### Optimistic UI Updates

**Update UI immediately**, rollback if operation fails.

```csharp
public async Task MarkStepCompleteAsync(Step step)
{
    // Optimistic update
    step.IsCompleted = true;

    try
    {
        await _repository.UpdateStepAsync(step);
    }
    catch
    {
        // Rollback on failure
        step.IsCompleted = false;
        ErrorMessage = "Failed to mark step complete";
    }
}
```

### Progressive Loading

**Load critical data first**, defer non-critical data.

```csharp
public async Task LoadGuideDetailAsync(int guideId)
{
    // Load critical data immediately
    var guide = await _repository.GetGuideAsync(guideId);
    CurrentGuide = guide;

    // Defer loading steps
    await Task.Delay(100);
    var steps = await _repository.GetStepsAsync(guideId);
    CurrentGuide.Steps = steps;

    // Defer loading media
    await Task.Delay(100);
    await LoadMediaAsync(steps);
}
```

## 📊 Performance Metrics to Monitor

### App Launch Time
- **Target**: < 2 seconds cold start
- **Measure**: Time from icon click to first frame

### Page Navigation Time
- **Target**: < 100ms for navigation
- **Measure**: Time from button click to new page visible

### List Scrolling FPS
- **Target**: 60 FPS sustained
- **Tool**: Windows Performance Recorder (WPR)

### Memory Usage
- **Target**: < 200 MB for typical session
- **Monitor**: Task Manager or Visual Studio Diagnostic Tools

### Database Query Time
- **Target**: < 50ms for simple queries, < 200ms for complex
- **Tool**: EF Core logging

## 🔧 Optimization Techniques

### Database Optimization

```csharp
// BAD: N+1 query problem
foreach (var guide in guides)
{
    guide.Steps = await _repository.GetStepsAsync(guide.Id); // N queries!
}

// GOOD: Single query with Include
var guides = await _context.Guides
    .Include(g => g.Steps)
    .ThenInclude(s => s.Media)
    .ToListAsync(); // 1 query
```

### Image Caching

```csharp
public class ImageCacheService
{
    private readonly MemoryCache _memoryCache = new();
    private readonly string _diskCachePath;

    public async Task<BitmapImage?> GetImageAsync(string url)
    {
        // Check memory cache
        if (_memoryCache.TryGetValue(url, out BitmapImage? cached))
            return cached;

        // Check disk cache
        var diskPath = GetDiskCachePath(url);
        if (File.Exists(diskPath))
        {
            var image = await LoadFromDiskAsync(diskPath);
            _memoryCache.Set(url, image, TimeSpan.FromMinutes(10));
            return image;
        }

        // Download and cache
        var downloaded = await DownloadImageAsync(url);
        await SaveToDiskAsync(diskPath, downloaded);
        _memoryCache.Set(url, downloaded, TimeSpan.FromMinutes(10));

        return downloaded;
    }
}
```

### Reduce XAML Complexity

```xml
<!-- BAD: Deeply nested layout -->
<Grid>
    <StackPanel>
        <Grid>
            <StackPanel>
                <Grid>
                    <TextBlock /> <!-- 5 levels deep! -->
                </Grid>
            </StackPanel>
        </Grid>
    </StackPanel>
</Grid>

<!-- GOOD: Flatter hierarchy -->
<StackPanel>
    <TextBlock />
</StackPanel>
```

## ⚡ Quick Wins Checklist

- [ ] All database queries are async
- [ ] Lists > 20 items use ItemsRepeater
- [ ] Images lazy-loaded with placeholders
- [ ] Search input debounced (300ms)
- [ ] Loading spinners for operations > 200ms
- [ ] No blocking calls on UI thread
- [ ] Page transitions < 100ms
- [ ] Database queries use Include for related data
- [ ] Images cached to disk
- [ ] Avoid unnecessary re-renders with `x:Bind` Mode=OneWay

## 🐛 Common Performance Pitfalls

### 1. Forgetting to Dispose

```csharp
// BAD
public void LoadData()
{
    var context = new InstallVibeDbContext();
    var guides = context.Guides.ToList(); // Context never disposed
}

// GOOD
public async Task LoadDataAsync()
{
    using var context = new InstallVibeDbContext();
    var guides = await context.Guides.ToListAsync();
}
```

### 2. Synchronous File I/O

```csharp
// BAD
var content = File.ReadAllText(path); // Blocks thread

// GOOD
var content = await File.ReadAllTextAsync(path);
```

### 3. Large ObservableCollections

```csharp
// BAD: Triggers UI update for every Add
foreach (var item in 1000Items)
{
    Collection.Add(item); // 1000 UI updates!
}

// GOOD: Single update
Collection = new ObservableCollection<T>(1000Items);
```

### 4. Expensive Property Getters

```csharp
// BAD
public string FullName => ExpensiveComputation(); // Called every binding refresh!

// GOOD
private string? _fullName;
public string FullName => _fullName ??= ExpensiveComputation();
```

## 🎯 Performance Testing

### Manual Testing
1. Test with 100+ guides, 500+ steps
2. Scroll lists rapidly
3. Navigate between pages quickly
4. Monitor Task Manager memory usage

### Automated Performance Tests
```csharp
[TestMethod]
[Timeout(2000)] // Must complete in < 2 seconds
public async Task LoadGuideList_PerformanceTest()
{
    var stopwatch = Stopwatch.StartNew();

    await viewModel.LoadGuidesAsync();

    stopwatch.Stop();
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
        $"LoadGuides took {stopwatch.ElapsedMilliseconds}ms");
}
```

## 📈 Profiling Tools

- **Visual Studio Profiler**: CPU, Memory, and Performance Profiler
- **Windows Performance Recorder (WPR)**: System-wide performance analysis
- **PerfView**: Advanced performance analysis
- **BenchmarkDotNet**: Micro-benchmarking for critical code paths

---

**Last Updated**: 2025-11-15
**Version**: 1.0
**Owner**: InstallVibe Development Team
