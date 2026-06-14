using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class MapsViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private MapsFile _mapsFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _mapsFile = _fileManager.GetFile<MapsFile>(UOFileType.Maps);
            if (_mapsFile == null)
            {
                _fileManager.LoadFile(UOFileType.Maps);
                _mapsFile = _fileManager.GetFile<MapsFile>(UOFileType.Maps);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}