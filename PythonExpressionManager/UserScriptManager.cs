using System.Collections.ObjectModel;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace PythonExpressionManager
{
    public sealed class UserScriptManager: IDisposable
    {
        public readonly string Folder;
        public readonly ScriptExecutor ScriptExecutor;
        public int DefaultPriority { get; set; }
        readonly Dictionary<string, Script> _loadedScripts;
        FileSystemWatcher _watcher;
        public readonly ReadOnlyDictionary<string, Script> LoadedScripts;

        private bool disposedValue;
        public UserScriptManager(string folder, ScriptExecutor executor, int defaultPriority)
        {
            ArgumentNullException.ThrowIfNull(executor, nameof(executor));
            if (!Directory.Exists(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            Folder = folder;
            DefaultPriority = defaultPriority;
            _loadedScripts = new();
            LoadedScripts = new(_loadedScripts);
            ScriptExecutor = executor;


            _watcher = new FileSystemWatcher
            {
                Path = Folder,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.py"
            };

            // Add event handlers
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Renamed += OnRenamed;

            foreach (var item in Directory.EnumerateFiles(Folder))
            {
                LoadScript(item);
            }
            // Start monitoring
            _watcher.EnableRaisingEvents = true;
        }
        private bool LoadScript(string filePath)
        {
            var scriptName = Path.GetFileName(filePath[..^3]);
            if (!scriptName.IsValidVariableName(this.ScriptExecutor))
            {
                return false;
            }
            try
            {
                var script = new Script(ScriptExecutor.Engine, File.ReadAllText(filePath), DefaultPriority);
                if (!ScriptExecutor.TryRegisterScript(scriptName, script))
                {
                    return false;
                };
                _loadedScripts.Add(scriptName, script);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool UpdateScript(string filePath)
        {
            var scriptName = Path.GetFileName(filePath[..^3]);
            if (_loadedScripts.ContainsKey(scriptName))
            {
                RemoveScript(filePath);
            }
            return LoadScript(filePath);
        }
        private bool RenameScript(string oldPath, string newPath)
        {
            var oldScriptName = Path.GetFileName(oldPath[..^3]);
            if (!_loadedScripts.TryGetValue(oldScriptName, out var script))
            {
                return LoadScript(newPath);
            }
            RemoveScript(oldPath);
            return UpdateScript(newPath);
        }
        private bool RemoveScript(string path)
        {
            var scriptName = Path.GetFileName(path[..^3]);
            if (!_loadedScripts.TryGetValue(scriptName, out var script))
            {
                return true;
            }
            _loadedScripts.Remove(scriptName);
            return ScriptExecutor.RemoveScriptWithKey(scriptName, script);
        }
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            RenameScript(e.OldFullPath, e.FullPath);
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    LoadScript(e.FullPath);
                    break;
                case WatcherChangeTypes.Deleted:
                    RemoveScript(e.FullPath);
                    break;
                case WatcherChangeTypes.Changed:
                    UpdateScript(e.FullPath);
                    break;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_watcher is not null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Changed -= OnChanged;
                    _watcher.Created -= OnChanged;
                    _watcher.Deleted -= OnChanged;
                    _watcher.Renamed -= OnRenamed;
                    _watcher.Dispose();

                    _watcher = null!;
                }
                foreach (var item in _loadedScripts)
                {
                    ScriptExecutor.RemoveScriptWithKey(item.Key, item.Value);
                    item.Value.Dispose();
                }
                _loadedScripts.Clear();

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
