using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class LandtilesViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private LandtilesFile _landtilesFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _landtilesFile = _fileManager.GetFile<LandtilesFile>(UOFileType.Landtiles);
            if (_landtilesFile == null)
            {
                _fileManager.LoadFile(UOFileType.Landtiles);
                _landtilesFile = _fileManager.GetFile<LandtilesFile>(UOFileType.Landtiles);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}