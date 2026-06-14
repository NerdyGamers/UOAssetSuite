using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class TiledataViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private TiledataFile _tiledataFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _tiledataFile = _fileManager.GetFile<TiledataFile>(UOFileType.Tiledata);
            if (_tiledataFile == null)
            {
                _fileManager.LoadFile(UOFileType.Tiledata);
                _tiledataFile = _fileManager.GetFile<TiledataFile>(UOFileType.Tiledata);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}