using System.ComponentModel;
using System.Runtime.CompilerServices;
using UOAssetSuite.Models;

namespace UOAssetSuite.ViewModels
{
    public class AnimationsViewModel : INotifyPropertyChanged
    {
        private UOFileManager _fileManager;
        private AnimationsFile _animationsFile;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize(UOFileManager fileManager) => _fileManager = fileManager;

        public void Load()
        {
            _animationsFile = _fileManager.GetFile<AnimationsFile>(UOFileType.Animations);
            if (_animationsFile == null)
            {
                _fileManager.LoadFile(UOFileType.Animations);
                _animationsFile = _fileManager.GetFile<AnimationsFile>(UOFileType.Animations);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}