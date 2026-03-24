using System.Collections.ObjectModel;
using IronPython.Compiler.Ast;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace PythonExpressionManager
{

    public delegate dynamic WrappableCLRDelegate(dynamic input, Dictionary<string,dynamic> tagDict, dynamic[] varArgs, Dictionary<string,dynamic> varKwargs);
    public delegate dynamic WrappedCLRDelegate(dynamic input, PythonDictionary tagDict, PythonTuple varArgs, PythonDictionary varKwargs);

    public class ScriptExecutor
    {
        static ScriptExecutor()
        {
            var engine = Script.Engine;

            var scope = engine.CreateScope();

            var script = engine.CreateScriptSourceFromString("f2 = lambda f: (lambda arg, tagDict, *args, **kwargs: f(arg, tagDict, args, kwargs))");

            script.Execute(scope);

            CLRScriptGenerator = scope.GetVariable("f2");
        }



        static readonly dynamic CLRScriptGenerator;
        public static Script FromDelegate(WrappableCLRDelegate del, int priority = (int)Priorities.CustomCLR)
        {
            return new Script(CLRScriptGenerator(FromUnwrapped(del)), priority);
        }
        public static Script FromDelegate(WrappedCLRDelegate del, int priority = (int)Priorities.CustomCLR)
        {
            return new Script(CLRScriptGenerator(del), priority);
        }
        public static WrappedCLRDelegate FromUnwrapped(WrappableCLRDelegate del)
        {
            dynamic F(dynamic input, PythonDictionary tagDictBoxed, PythonTuple varArgsBoxed, PythonDictionary varKwargsBoxed)
            {
                ArgumentNullException.ThrowIfNull(input, nameof(input));
                ArgumentNullException.ThrowIfNull(tagDictBoxed, nameof(tagDictBoxed));
                ArgumentNullException.ThrowIfNull(varArgsBoxed, nameof(varArgsBoxed));
                ArgumentNullException.ThrowIfNull(varKwargsBoxed, nameof(varKwargsBoxed));

                var tagDict = tagDictBoxed.ToDictionary(x => (string)x.Key, x => (dynamic)x.Value);
                var varArgs = varArgsBoxed!.ToArray<dynamic>();
                var varKwargs = varKwargsBoxed.ToDictionary(x => (string)x.Key, x => (dynamic)x.Value);
                return del(input, tagDict, varArgs, varKwargs);
            }
            return F;
        }
        public string ArgumentName { get; private set; }
        public string BaseDictName { get; private set; }
        public ScriptExecutor(ILogger? logger = null, string argumentName = "arg", string baseDictName = "tags")
        {
            if (argumentName == baseDictName)
            {
                throw new ArgumentException("argument name and base dict name cannot match");
            }
            if (!argumentName.IsValidVariableName(this))
            {
                throw new ArgumentException("invalid argument name", nameof(argumentName));
            }
            ArgumentName = argumentName;


            if (!baseDictName.IsValidVariableName(this))
            {
                throw new ArgumentException("invalid base dict name", nameof(baseDictName));
            }
            BaseDictName = baseDictName;

            Scripts = new();

            KeyAliases = new();

            _registeredKeys = new();

            Logger = logger ??= new ConsoleLogger();
        }
        public readonly ILogger Logger;
        public bool Evaluate(dynamic data, string input)
        {
            return Evaluate(data, new CompiledScript(input, this));
        }
        public bool Evaluate(dynamic data, CompiledScript input)
        {
            return PythonOps.IsTrue(EvaluateObject(data, input));
        }
        public dynamic EvaluateObject(dynamic data, string input)
        {
            return EvaluateObject(data, new CompiledScript(input, this));
        }

        public dynamic EvaluateObject(dynamic data, CompiledScript input)
        {
            return input.Function(data);
        }

        public CompiledScript Compile(string input)
        {
            return new CompiledScript(input, this);
        }

        bool TryRegisterScriptInternal(string key, Script script, bool refresh = true)
        {

            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(script, nameof(script));

            if (!key.IsValidVariableName(this))
            {
                return false;
            }
            if (!Scripts.TryGetValue(key, out var l))
            {
                Scripts[key] = l = new();
            }
            l.Add(script);
            if (refresh)
            {
                RefreshScripts();
            }
            return true;
        }
        public bool TryRegisterScript(string key, Script script) => TryRegisterScriptInternal(key, script);

        void RegisterScriptInternal(string key, Script script, bool refresh = true)
        {
            if (!TryRegisterScriptInternal(key, script, refresh))
            {
                throw new ArgumentException("illegal variable name", nameof(key));
            }
        }
        public void RegisterScript(string key, Script script) => RegisterScriptInternal(key, script);

        bool RemoveScriptWithKeyInternal(string key, Script script, bool refresh = true)
        {

            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(script, nameof(script));
            if (Scripts.TryGetValue(key, out var scripts))
            {
                if (!scripts.Remove(script))
                {
                    return false;
                }
                if (scripts.Count == 0)
                {
                    RemoveKeyInternal(key, refresh);
                }
                else if (refresh)
                {
                    RefreshScripts();
                }
                return true;
            }
            return false;
        }
        public bool RemoveScriptWithKey(string key, Script script) => RemoveScriptWithKeyInternal(key, script);

        public bool RemoveScriptInternal(Script script, bool refresh = true)
        {
            ArgumentNullException.ThrowIfNull(script, nameof(script));
            bool result = false;
            var removeKeys = new List<string>();
            foreach (var item in Scripts)
            {
                while (item.Value.Remove(script))
                {
                    result = true;
                }
                if (item.Value.Count == 0)
                {
                    removeKeys.Add(item.Key);
                }
            }
            foreach (var key in removeKeys)
            {
                Scripts.Remove(key);
            }
            if (result && refresh)
            {
                RefreshScripts();
            }
            return result;
        }
        public bool RemoveScript(Script script) => RemoveScriptInternal(script);

        public bool RemoveKeyInternal(string key, bool refresh = true)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            var result = Scripts.Remove(key);
            if (refresh)
            {
                RefreshScripts();
            }
            return result;
        }
        public bool RemoveKey(string key) => RemoveKeyInternal(key);

        public bool IsKeyRegistered(string key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            return Scripts.ContainsKey(key);
        }
        public bool IsScriptRegistered(Script script)
        {
            foreach (var item in Scripts)
            {
                if (item.Value.Contains(script))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsScriptRegisteredWithKey(string key, Script script)
        {
            if (Scripts.TryGetValue(key, out var scripts))
            {
                return scripts.Contains(script);
            }
            return false;
        }
        public bool IsScriptAvailableWithKey(string key, Script script)
        {
            if (Scripts.TryGetValue(key, out var scripts))
            {
                return scripts.Min() == script;
            }
            return false;
        }

        public Dictionary<string, bool> TryRegisterMultipleScriptsInternal(IDictionary<string, Script> scripts, bool refresh = true)
        {
            ArgumentNullException.ThrowIfNull(scripts, nameof(scripts));
            if (scripts.Values.Contains(null!))
            {
                throw new ArgumentException($"encountered null in '{nameof(scripts)}.{nameof(scripts.Values)}'", nameof(scripts));
            }
            try
            {
                var result = new Dictionary<string, bool>();
                foreach (var item in scripts)
                {
                    result.Add(item.Key, TryRegisterScriptInternal(item.Key, item.Value, false));
                }
                return result;
            }
            finally
            {
                if (refresh)
                {
                    RefreshScripts();
                }
            }
        }
        public Dictionary<string, bool> TryRegisterMultipleScripts(IDictionary<string, Script> scripts) => TryRegisterMultipleScriptsInternal(scripts);

        void RegisterAliasInternal(string key, string otherKey, bool refresh = true)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            ArgumentNullException.ThrowIfNull(otherKey, nameof(otherKey));
            if (!key.IsValidVariableName(this))
            {
                throw new ArgumentException("illegal variable name", nameof(key));
            }
            if (!otherKey.IsValidVariableName(this))
            {
                throw new ArgumentException("illegal variable name", nameof(otherKey));
            }
            KeyAliases.Add(key, otherKey);
        }
        public void RegisterAlias(string key, string otherKey) => RegisterAliasInternal(key, otherKey);

        Dictionary<string, List<Script>> Scripts { get; set; }
        Dictionary<string, string> KeyAliases;

        Dictionary<string, Script> _registeredKeys;
        public ReadOnlyDictionary<string, Script> RegisteredKeys => new(_registeredKeys);
        public void RefreshScripts()
        {
            Logger.LogDebug("Refreshing registered scripts...");
            _registeredKeys.Clear();

            foreach (var item in Scripts)
            {
                if (item.Value.Count == 0)
                {
                    Logger.LogError($"Attempted to refresh scripts in an invalid state (a key registry was empty), aborting.");
                    _registeredKeys.Clear();
                    return;
                }
                if (item.Value.Count != 1)
                {
                    Logger.LogWarning($"There is more than script with the name '{item.Key}', only the one with the highest priority will be available.");
                }
                item.Value.Sort();
                _registeredKeys[item.Key] = item.Value[0];
            }

            foreach (var item in KeyAliases)
            {
                if (_registeredKeys.ContainsKey(item.Key))
                {
                    Logger.LogWarning($"The key alias '{item.Key}' was overriden and will not be available.");
                }
                if (!_registeredKeys.TryGetValue(item.Value, out var script))
                {
                    Logger.LogError($"Failed to register key alias '{item.Key}' because the target '{item.Value}' does not exist.");
                    continue;
                }
                _registeredKeys[item.Key] = script;
            }
        }
    }
}
