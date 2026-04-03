using Il2CppAssets.Scripts.Database;
using IronSearch.Exceptions;
using IronSearch.Records;
using PythonExpressionManager;
using System.Diagnostics.CodeAnalysis;

namespace IronSearch
{
    internal class SearchState
    {
        public SearchArgument Arg = new(null!, new(1000));
        public List<MusicInfo> Results = new();
    }
    internal class MusicSearchWorkerManager : AbstractInteropWorkerManager<MusicInfo, SearchState>
    {
        private CompiledScript _script;
        private List<SearchState?> _states;
        private volatile bool _failed;

#pragma warning disable CS8618 // nullable fields will be set in Run before any worker can access them
        public MusicSearchWorkerManager(int workers) : base(workers)
        {

        }
#pragma warning restore CS8618

        public bool Run(List<MusicInfo> allMusic, CompiledScript script, [MaybeNullWhen(false)] out List<MusicInfo> results)
        {
            _script = script;
            _states = new(_workerCount);
            _failed = false;

            Execute(allMusic.ToArray(), allMusic.Count);

            if (_failed)
            {
                results = null;
                return false;
            }

            results = new();

            foreach (var s in _states)
            {
                if (s is not null)
                {
                    results.AddRange(s.Results);
                }
            }

            return true;
        }

        protected override SearchState? OnWorkerStart()
        {
            return new();
        }
        protected override void OnWorkerEnd(SearchState? state)
        {
            if (state is null)
            {
                return;
            }
            state.Arg = null!;
            state.Results = null!;
        }
        protected override SearchState? OnWorkerIteration(SearchState? state)
        {
            lock (_states)
            {
                _states.Add(state);
            }
            return new();
        }
        private SearchState? CatastrophicFailure(string message, Exception? ex = null)
        {
            if (!_failed)
            {
                _failed = true;
                new SearchResponse($"A catastrophic error occured ({message}) and the search cannot continue.", SearchResponse.Type.RuntimeError).PrintSearchError();
                if (ex != null)
                {
                    new SearchResponse(ex, SearchResponse.Type.RuntimeError).PrintSearchError();
                }
            }
            Stop();
            return null;
        }
        protected override SearchState? ProcessItem(MusicInfo music, SearchState? state)
        {
            if (state is null)
            {
                return CatastrophicFailure("search state was null");
            }
            try
            {
                state!.Arg.I = music;

                if (ModMain.SearchManager.ScriptManager.ScriptExecutor.Evaluate(state.Arg, _script))
                {
                    state.Results.Add(music);
                }
            }
            catch (Exception ex)
            {
                if (ex is TerminateSearchException ts)
                {
                    if (ts.IsTrue)
                    {
                        state!.Results.Add(music);
                    }
                }
                else
                {
                    if (!_failed)
                    {
                        _failed = true;
                        try
                        {
                            if (!CompiledScript.TryConvertException(ex, ModMain.SearchManager.ScriptManager.ScriptExecutor.Engine))
                            {
                                throw;
                            }
                        }
                        catch (Exception ex2)
                        {
                            new SearchResponse(ex2, SearchResponse.Type.RuntimeError).PrintSearchError();
                        }
                    }
                    Stop();
                }
            }
            return state;
        }
    }
}
