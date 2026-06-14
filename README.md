# UO Asset Suite

A comprehensive C# WPF application for managing Ultima Online client files.

## Features

- **Art Files**: View, extract, import, remove, and add new item artwork
- **Landtiles**: Manage terrain tiles with full editing support
- **Animations**: View and edit animation sequences
- **Gumps**: Manage UI elements and gump artwork
- **Maps**: View and edit map data
- **Tiledata**: Edit tile properties and flags

## Architecture

- **MVVM Pattern**: Clean separation of concerns
- **Thread-safe**: File operations protected with locks
- **UOFiddler Integration**: Uses Ultima.dll for file operations
- **UOP Support**: Supports both MUL and UOP file formats

## Requirements

- .NET 6.0 or later
- Visual Studio 2022 (for development)
- UOFiddler's Ultima.dll

## Setup

1. Clone this repository
2. Download UOFiddler from https://github.com/potential1/UOFiddler
3. Copy Ultima.dll to the lib folder
4. Open UOAssetSuite.sln in Visual Studio
5. Build the solution
6. Run the application

## Usage

1. Click Browse to select your UO client directory
2. The application will automatically detect available files
3. Use the tabs to manage different asset types
4. Each tab provides specific tools for that asset type

## Project Structure

UOAssetSuite/
  UOAssetSuite.sln
  UOAssetSuite/
    UOAssetSuite.csproj
    App.xaml, App.xaml.cs
    MainWindow.xaml, MainWindow.xaml.cs
    Models/
      FileIndex.cs
      UOPFile.cs
      PixelDataHelper.cs
      UOFileManager.cs
      ArtFile.cs
      LandtilesFile.cs
      AnimationsFile.cs
      GumpsFile.cs
      MapsFile.cs
      TiledataFile.cs
    ViewModels/
      RelayCommand.cs
      MainViewModel.cs
      ArtViewModel.cs
      LandtilesViewModel.cs
      AnimationsViewModel.cs
      GumpsViewModel.cs
      MapsViewModel.cs
      TiledataViewModel.cs
    Views/
      ArtView.xaml, ArtView.xaml.cs
      LandtilesView.xaml, LandtilesView.xaml.cs
      AnimationsView.xaml, AnimationsView.xaml.cs
      GumpsView.xaml, GumpsView.xaml.cs
      MapsView.xaml, MapsView.xaml.cs
      TiledataView.xaml, TiledataView.xaml.cs
  lib/
    Ultima.dll

## Technical Details

### UOP Format Support
- Full support for UOP file format
- Automatic detection of file format (MUL vs UOP)
- Compression handling

### Pixel Data
- RLE encoding/decoding for art data
- 16-bit color to 32-bit ARGb conversion
- Optimized encoding for minimal file size

### Thread Safety
- All file operations protected with locks
- Safe for concurrent access
- Proper disposal of resources

## Contributing

Pull requests are welcome.

## License

MIT