using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class GumpsViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private GumpsFile _gumpsFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _gumpsFile = _fileManager.GetFile<GumpsFile>(UOFileType.Gumps);
            if (_gumpsFile == null)
            {
                _fileManager.LoadFile(UOFileType.Gumps);
                _gumpsFile = _fileManager.GetFile<GumpsFile>(UOFileType.Gumps);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}