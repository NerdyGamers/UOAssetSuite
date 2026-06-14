using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly UOFileManager _fileManager = new();
        private string _clientPath;
        private string _statusMessage;
        public event PropertyChangedEventHandler PropertyChanged;
        public ArtViewModel ArtVM { get; } = new();
        public LandtilesViewModel LandtilesVM { get; } = new();
        public AnimationsViewModel AnimationsVM { get; } = new();
        public GumpsViewModel GumpsVM { get; } = new();
        public MapsViewModel MapsVM { get; } = new();
        public TiledataViewModel TiledataVM { get; } = new();

        public string ClientPath
        {
            get => _clientPath;
            set
            {
                if (_clientPath != value)
                {
                    _clientPath = value;
                    _fileManager.ClientPath = value;
                    OnPropertyChanged();
                    UpdateStatus();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand BrowseClientPathCommand { get; }
        public ICommand LoadAllFilesCommand { get; }
        public ICommand UnloadAllFilesCommand { get; }
        public ICommand SaveAllFilesCommand { get; }

        public MainViewModel()
        {
            BrowseClientPathCommand = new RelayCommand(BrowseClientPath);
            LoadAllFilesCommand = new RelayCommand(LoadAllFiles);
            UnloadAllFilesCommand = new RelayCommand(UnloadAllFiles);
            SaveAllFilesCommand = new RelayCommand(SaveAllFiles);
        }

        public void Initialize()
        {
            ArtVM.Initialize(_fileManager);
            LandtilesVM.Initialize(_fileManager);
            AnimationsVM.Initialize(_fileManager);
            GumpsVM.Initialize(_fileManager);
            MapsVM.Initialize(_fileManager);
            TiledataVM.Initialize(_fileManager);
            UpdateStatus();
        }

        public void Cleanup() => _fileManager.Dispose();

        private void BrowseClientPath()
        {
            var dialog = new OpenFolderDialog { Title = "Select UO Client Directory" };
            if (dialog.ShowDialog() == true) ClientPath = dialog.FolderName;
        }

        private void LoadAllFiles()
        {
            if (string.IsNullOrEmpty(ClientPath))
            {
                StatusMessage = "Please select a client path first.";
                return;
            }
            bool allLoaded = true;
            foreach (UOFileType fileType in Enum.GetValues(typeof(UOFileType)))
                if (!_fileManager.LoadFile(fileType)) allLoaded = false;
            StatusMessage = allLoaded ? "All files loaded successfully" : "Some files failed to load";
        }

        private void UnloadAllFiles()
        {
            foreach (UOFileType fileType in Enum.GetValues(typeof(UOFileType)))
                _fileManager.UnloadFile(fileType);
            StatusMessage = "All files unloaded";
        }

        private void SaveAllFiles()
        {
            bool allSaved = true;
            foreach (UOFileType fileType in Enum.GetValues(typeof(UOFileType)))
                if (!_fileManager.SaveFile(fileType)) allSaved = false;
            StatusMessage = allSaved ? "All files saved successfully" : "Some files failed to save";
        }

        private void UpdateStatus() => StatusMessage = string.IsNullOrEmpty(ClientPath) ? "Ready" : $"Client: {ClientPath}";

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}