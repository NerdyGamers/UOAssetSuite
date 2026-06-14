using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class ArtViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private ArtFile _artFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _artFile = _fileManager.GetFile<ArtFile>(UOFileType.Art);
            if (_artFile == null)
            {
                _fileManager.LoadFile(UOFileType.Art);
                _artFile = _fileManager.GetFile<ArtFile>(UOFileType.Art);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}