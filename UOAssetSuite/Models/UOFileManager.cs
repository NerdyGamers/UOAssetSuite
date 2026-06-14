using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UOAssetSuite.Models
{
    public enum UOFileType { Art, Landtiles, Animations, Gumps, Maps, Tiledata }

    public class UOFileInfo
    {
        public UOFileType FileType { get; set; }
        public string FilePath { get; set; }
        public bool IsLoaded { get; set; }
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
    }

    public class UOFileManager : IDisposable
    {
        private readonly Dictionary<UOFileType, object> _fileCache = new();
        private readonly object _lock = new();
        private string _clientPath;

        public string ClientPath
        {
            get => _clientPath;
            set { lock (_lock) { _clientPath = value; RefreshFileList(); } }
        }

        public List<UOFileInfo> AvailableFiles { get; } = new();

        public void RefreshFileList()
        {
            lock (_lock)
            {
                AvailableFiles.Clear();
                if (string.IsNullOrEmpty(ClientPath) || !Directory.Exists(ClientPath)) return;
                var fileMappings = new Dictionary<UOFileType, string[]>
                {
                    { UOFileType.Art, new[] { "art.mul", "artidx.mul", "artLegacyMUL.uop" } },
                    { UOFileType.Landtiles, new[] { "landtile.mul", "landtileidx.mul" } },
                    { UOFileType.Animations, new[] { "anim.mul", "animidx.mul" } },
                    { UOFileType.Gumps, new[] { "gump.mul", "gumpidx.mul" } },
                    { UOFileType.Maps, new[] { "map0.mul", "map1.mul", "map2.mul", "map3.mul", "map4.mul", "map5.mul" } },
                    { UOFileType.Tiledata, new[] { "tiledata.mul" } }
                };
                foreach (var mapping in fileMappings)
                {
                    var foundFiles = mapping.Value.Select(f => Path.Combine(ClientPath, f)).Where(File.Exists).ToList();
                    if (foundFiles.Any())
                    {
                        var fileInfo = new FileInfo(foundFiles.First());
                        AvailableFiles.Add(new UOFileInfo
                        {
                            FileType = mapping.Key,
                            FilePath = foundFiles.First(),
                            IsLoaded = false,
                            LastModified = fileInfo.LastWriteTime,
                            FileSize = fileInfo.Length
                        });
                    }
                }
            }
        }

        public bool LoadFile(UOFileType fileType, string filePath = null)
        {
            lock (_lock)
            {
                try
                {
                    string path = filePath ?? GetFilePath(fileType);
                    if (string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
                    switch (fileType)
                    {
                        case UOFileType.Art: _fileCache[fileType] = new ArtFile(path); break;
                        case UOFileType.Landtiles: _fileCache[fileType] = new LandtilesFile(path); break;
                        case UOFileType.Animations: _fileCache[fileType] = new AnimationsFile(path); break;
                        case UOFileType.Gumps: _fileCache[fileType] = new GumpsFile(path); break;
                        case UOFileType.Maps: _fileCache[fileType] = new MapsFile(path); break;
                        case UOFileType.Tiledata: _fileCache[fileType] = new TiledataFile(path); break;
                    }
                    var fileInfo = AvailableFiles.FirstOrDefault(f => f.FileType == fileType);
                    if (fileInfo != null) fileInfo.IsLoaded = true;
                    return true;
                }
                catch { return false; }
            }
        }

        public bool UnloadFile(UOFileType fileType)
        {
            lock (_lock)
            {
                if (_fileCache.ContainsKey(fileType))
                {
                    if (_fileCache[fileType] is IDisposable disposable) disposable.Dispose();
                    _fileCache.Remove(fileType);
                    var fileInfo = AvailableFiles.FirstOrDefault(f => f.FileType == fileType);
                    if (fileInfo != null) fileInfo.IsLoaded = false;
                    return true;
                }
                return false;
            }
        }

        public bool SaveFile(UOFileType fileType, string filePath = null)
        {
            lock (_lock)
            {
                try
                {
                    var file = GetFile<object>(fileType);
                    if (file == null) return false;
                    string path = filePath ?? GetFilePath(fileType);
                    if (string.IsNullOrEmpty(path)) return false;
                    switch (fileType)
                    {
                        case UOFileType.Art: ((ArtFile)file).Save(path); break;
                        case UOFileType.Landtiles: ((LandtilesFile)file).Save(path); break;
                        case UOFileType.Animations: ((AnimationsFile)file).Save(path); break;
                        case UOFileType.Gumps: ((GumpsFile)file).Save(path); break;
                        case UOFileType.Maps: ((MapsFile)file).Save(path); break;
                        case UOFileType.Tiledata: ((TiledataFile)file).Save(path); break;
                    }
                    return true;
                }
                catch { return false; }
            }
        }

        public T GetFile<T>(UOFileType fileType) where T : class
        {
            lock (_lock)
            {
                if (_fileCache.TryGetValue(fileType, out var file) && file is T t) return t;
                return null;
            }
        }

        public string GetFilePath(UOFileType fileType)
        {
            lock (_lock)
            {
                return AvailableFiles.FirstOrDefault(f => f.FileType == fileType)?.FilePath;
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var file in _fileCache.Values)
                {
                    if (file is IDisposable disposable) disposable.Dispose();
                }
                _fileCache.Clear();
            }
        }
    }
}