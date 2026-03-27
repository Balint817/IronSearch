# IronSearch Advanced Search Guide (Tags + Scripts)

This is the documentation for the "advanced search" features implemented by IronSearch. It is powered by **IronPython** engine.

---

## 1) Quick Start

### Enable advanced search

Your search text must start with the configured prefix (default: `search:`). The mod then removes the prefix and uses the remaining text as your search expression.

Example (default settings):

```text
search: BPM('160-200') and Length('180+')
```

(This example will search for songs that have a BPM between 160 and 200, and are longer than 180 seconds)

### Basic boolean logic

Use boolean operators:

- `and`, `or`, `not`
- parentheses for grouping

Example:

```text
search: (Custom() and Packed()) or (Unplayed('?') and not Favorite())
```

(This example will search for custom .mdm songs, or for songs that you haven't yet played and aren't favorited.)

### Sorting (optional)

Sorting is enabled by including `Sorter(...)` (alias: `Sort`) somewhere in your expression. \
Multiple comparers can be chained inside one `Sorter(...)` call. \
`Sorter(...)` itself always returns `True` and does not filter; it only registers an ordering. \


Example:

```text
search: Sort(ByBPM(), ByLength()) and Custom()
```

(This example will search for custom songs, then sort them by BPM, or if the BPMs are equal, by length)


---

## 2) Syntax

### Truthiness

The result of your expression is always converted to a True or False value using default 'truthiness' behavior, so for example, if you accidentally typed `AP` instead of `AP()`, this would match every song.

### The implicit current-song object: `M`

During evaluation, the expression has a parameter `M` which contains the objects related to the current search:
- `M.I`, the underlying `MusicInfo`
- `M.PS`, the PeroString object of the current search (if you have no idea what that means, you probably don't need it)
- `M.<name>`, forwards the `name` to access the underlying `MusicInfo`

Examples:

- `M.uid`, `M.name`, `M.author`, `M.scene`, etc.

### Keyword arguments

Keyword arguments are supported, but each tag controls which keywords it accepts.

For example, the `Range(...)` object allows you to specify whether the start or end should be exclusive. (`start=...`, `end=...`)

---

## 3) Range Syntax

Many tags accept "range strings" (e.g. BPM ranges, difficulty ranges, etc.).

### Range format

1. Exact range:
   - `'A-B'` matches `A <= value <= B`
2. Wildcard / Full range:
   - `'*'` matches any value
3. Wildcard / Invalid range:
   - `'?'` matches nothing
4. Open-ended:
   - `'A+'` matches `value >= A` (A-to-infinity)
   - `'A-'` matches `value <= A` (negative infinity-to-A)
5. Exclusive endpoints with `|`
   - `'|A-B'` makes the **start** exclusive: `A < value <= B`
   - `'A-B|'` makes the **end** exclusive: `A <= value < B`

Notes about `?`:

- By default, `'?'` is an invalid range that **normally matches nothing**.
- However, several tags interpret `'?'` specially as "select the highest available difficulty/level"
- Which tags do this is implemented per-tag (see reference below or the Help text).

### Multi-range strings (OR of multiple ranges)

Some inputs accept multi-ranges. A multi-range takes multiple ranges, and matches any value that matches any of the ranges that make it up.

For example, `MR('100- 200+')` matches values lower than 100, or higher than 200.

---

## 4) Settings

Settings are stored under the path: `<Muse Dash Folder>/UserData/IronSearch.cfg`

### `StartSearchText` (default: `search:`)

The prefix that must start your search text for an advanced search to actually happen.

If you set it to an empty string, advanced search is always enabled (not recommended).

(Personally, I have it set to `::`)

### `WaitMultiplier` (default: `2.5`)

How much more time to wait before re-loading search results. This only affects advanced search, normal search is unaffected.

The goal is to set this low enough so that it doesn't make the wait time too long, but not too low such that it constantly refreshes if you stop to think for 0.1 seconds and fills your console with errors.

### `EnableHQSpam` (default: `false`)

Controls whether the mod is allowed to query Headquarters (HQ) for Online/Ranked info about custom charts.

Tags that rely on HQ data:

- `Online()`
- `Ranked()`

If `EnableHQSpam` is disabled, these tags will not work correctly.

**DISCLAIMER: After this setting is enabled, online chart information has to be queried and cached, which may result in significant delay on startup for the first time. Consequent boots are mostly unaffected.**

### `TagAliases` 

A dictionary mapping alias keywords to other tag expressions.

Each entry:

- <newName> = "<originalName>"

For example:

- `TagAliases = { Title = "Name" }`

This will allow you to reference the built-in "Title" tag with the name "Name". \
Aliasing affects parsing/available keywords (autocomplete also includes aliases).

### `Expressions` (advanced)

A dictionary of shorthands for searches.

Each entry:

- <name> = "<expression>"

Example concept:

- `Expressions = { NewCustom = "Unplayed() and Custom()" }`

After that, you can call:

- `NewCustom()`


### `AutoCompleteItems` (advanced)

Dictionary of extra autocomplete keywords.

For example, the default value is:

- `AutoCompleteItems = { Vanilla = "not Custom()" }`

Unlike an expression, does not register an entirely new tag, but simply adds an extra suggestion to the auto-complete.

---

## 5) Sorting Guide

Sorting is done only when:

- advanced search is enabled (prefix match), and
- parsing/evaluation did not produce an error, and
- the expression registered at least one sorter via `Sorter(...)` / `Sort(...)`.

### The sorting call

`Sorter(comparerFunction)` (alias `Sort`)

The comparerFunction concept:

- A comparer is a function that takes **two** `MusicInfo` objects: `A` and `B`
- It returns an `int`:
  - negative: `A` should come before `B`
  - zero: equal
  - positive: `A` should come after `B`

You can provide built-in comparer key functions:

- `ByLength()`, `ByName()`, `ByUID()`, etc.

Example:

```text
search: Sorter(ByLength()) and Custom()
```

### Multiple comparers

Pass multiple comparer callables:

```text
search: Sorter(ByBPM(), ByName()) and Custom()
```

Comparers are evaluated in order. The first non-zero comparison decides the ordering.

### Reverse + priority

The underlying `Sorter(...)` implementation supports additional keyword arguments:

- `reverse=...` (boolean-ish; negates the first non-zero comparison result)
- `priority=...` (integer, lower number is higher priority)

If there are 2 or more sorter calls with equal priority, only the last is executed. \
If there are 2 or more sorter calls with different priorities, they are executed in ascending order.

These arguments can also be passed as positional arguments, omitting the keyword.

### Custom comparers

You can pass a function created in your expression:

For example, the ByUID sorting can be re-implemented like this:

```text
search: Sorter(lambda A, B: (A.uid > B.uid) - (A.uid < B.uid))
```

Important constraints:

- The function must take **exactly 2 arguments**
- It must be callable
- It must return an `int`

---

## 6) Tag Reference

This section documents **every tag** available by default.

If a tag is **not** registered in `ModMain.LoadUserScripts()` (so you may not be able to type it by default), it is explicitly noted.

### 6.1 Filters (return `True`/`False`)

#### `Any`

Usage:

- `Usage: Any(text) or Any(regex)`

Checks if the music matches any of:

- `Album`
- `Author`
- `Designer`
- `Tag`
- `Title`

Notes:

- String matching is a case-insensitive "contains" check.

---

#### `Accuracy` / `Acc`

Usage:

- `Usage: Accuracy(accuracyRange) or Accuracy(accuracyRange, levelRange)`

Checks if the music has scores in the specified accuracy range, optionally restricting to specific difficulty levels.

Notes:

- Input accuracy is a **percentage** range (e.g. `90-100`), but internally it is converted to `0.90-1.00`.
- The accuracy part does **not** allow wildcard `?` in the "accuracyRange" position.
- If the difficulty/level range is wildcard `?`, the tag selects the **highest available difficulty** among 1–4.

---

#### `Album`

Usage:

- `Usage: Album(albumName) or Album(regex)`

Checks if the music belongs to the specified album name.

Notes:

- Album names come from the game/custom album data, not necessarily matching what you see in UI.

---

#### `AP` / `Perfect` / `AllPerfect`

Usage:

- `Usage: AP() or AP(difficultyRange)`

Checks if the music has **all perfect** scores in the specified difficulty range.

Notes:

- Difficulty range values are difficulties (1–4 in practice).
- If you pass wildcard `?` (parsed as invalid range), the implementation selects the **highest available difficulty** among 1–4.
- The tag requires perfect (accuracy exactly `1.0f`) for every selected difficulty.

---

#### `Author`

Usage:

- `Usage: Author(authorName) or Author(regex)`

Checks if the music’s author matches the specified input.

Also checks per-difficulty/localized authors.

---

#### `BPM`

Usage:

- `Usage: BPM(bpmRange)`

Checks if the music’s BPM is within the given BPM range.

---

#### `Callback`

Usage:

- `Usage: Callback(callbackRange) or Callback(callbackRange, levelRange)`

Checks if the music has a callback difficulty that matches the provided range.

Notes:

- If `levelRange` is wildcard `?`, it selects the **highest available level**.
- Callback difficulty is the "difficulty the game servers use", not the displayed difficulty.
- Callback difficulty is always an integer; string difficulty like `E` turns into -1.

---

#### `Cinema`/`Video`

Usage:

- `Usage: Cinema()`

Checks if the music is a custom chart with a video background.

---

#### `Custom`

Usage:

- `Usage: Custom()`

Checks if the music is a custom chart.

---

#### `Designer`

Usage:

- `Usage: Designer(designerName) or Designer(regex)`

Checks the chart’s level designer.

---

#### `Difficulty`/`Diff`

Usage:

- `Usage: Difficulty(difficultyRange, levelRange)`

Checks if the music has a difficulty in the given difficulty range, optionally restricting which levels match.

Important implementation detail:

- `levelRange` set to `?` means "select the highest available level (highest map index)".
- `difficultyRange` set to wildcard `?` will match non-integer difficulties like `?` or `E`.

---

#### `Favorite`/`Fav`

Usage:

- `Usage: Favorite()`

Checks if the music is in your favorites list.

---

#### `FullCombo`/`FC`

Usage:

- `Usage: FullCombo() or FullCombo(levelRange)`

Checks full combo status.

Notes:

- If `levelRange='?'`, it selects the highest applicable difficulty.
- For non-wildcard ranges, it requires full combo on every selected difficulty.

---

#### `Hidden`

Usage:

- `Usage: Hidden()`

Checks whether the music has a "hidden" difficulty (difficulty 4).

---

#### `Touhou`

Usage:

- `Usage: Touhou()`

Checks whether the music has a "Touhou" difficulty (difficulty 5).

---

#### `History`

Usage:

- `Usage: History()`

Checks if the music is in the chart history (last played charts).

---

#### `Length`

Usage:

- `Usage: Length(lengthRange)`

Checks if the music’s length is within the given range.

Notes:

- Accepts numeric/time ranges:
  - normal numeric ranges like `'180-240'` (seconds)
  - time strings like `'1m30s'`

---

#### `Modified`

Usage:

- `Usage: Modified(timeString)`

Checks if the custom chart was last modified within the given time window.

Time string format:
- digits followed by units, concatenated:
  - `s` (seconds), `m` (minutes), `h`, `d` (days), `w` (weeks)
  - examples: `7d`, `24h`, `1h30m`

---

#### `New`

Usage:

- `Usage: New(topRange)`

Checks if the music is among the Nth last added custom charts.

Notes:

- This is based on album/chart filesystem last-write time ordering.
- It only applies to custom charts.

---

#### `Old`

Usage:

- `Usage: Old(bottomRange)`

Checks if the music is among the Nth first added custom charts.

Notes:

- This is based on album/chart filesystem last-write time ordering.
- Only applies to custom charts.

---

#### `Online`

Usage:

- `Usage: Online()`

Checks if the music is an online chart (custom from the website).

Notes:

- Only works when HQ is available (`EnableHQSpam=true`).

---

#### `Packed`

Usage:

- `Usage: Packed()`

Checks if the music is a packed custom chart (e.g. `.mdm` file), not a folder.

---

#### `Ranked`

Usage:

- `Usage: Ranked()`

Checks if the music is a ranked custom chart.

Notes:

- Only works when HQ is available (`EnableHQSpam=true`).
- There may be a delay in ranking info updating after enabling HQ spam (implementation note from help string).

---

#### `Scene`

Usage:

- `Usage: Scene(sceneName) or Scene(sceneIndex)`

Checks if the music is in a specific scene.

Accepted inputs:

- a scene name (case-insensitive substring match)
- a numeric index

Ambiguity:

- If the typed scene name matches multiple known scenes, the search fails.

---

#### `Streamer`

Usage:

- `Usage: Streamer()`

Checks whether the music is in the streamer list (copyright-safe music).

Notes:

- Customs do not support this feature.

---

#### `Tag`

Usage:

- `Usage: Tag(tagName) or Tag(regex)`

Checks if the music has a "music tag" whose text matches the provided string/regex.

---

#### `Title`

Usage:

- `Usage: Title(title) or Title(regex)`

Checks if the music’s title matches.

---

#### `Unplayed`

Usage:

- `Usage: Unplayed() or Unplayed(levelRange)`

Checks whether the music has not been played, or (optionally) not played on specified difficulty levels.

Notes:

- Wildcard `levelRange='?'` selects highest applicable difficulty.

---

### 6.2 Range/time/object constructor tags (return objects/lists)

These tags often return non-bool values that can be consumed by other tags.

#### `Days(dayRange)`

Usage:

- `Usage: Days(dayRange)`

Returns a time range in **ticks** representing N days.

---

#### `Hours(hourRange)`

Usage:

- `Usage: Hours(hourRange)`

Returns N hours in ticks.

---

#### `Minutes(minuteRange)`

Usage:

- `Usage: Minutes(minuteRange)`

Returns N minutes in ticks.

---

#### `Weeks(weekRange)`

Usage:

- `Usage: Weeks(weekRange)`

Returns N weeks in ticks.

---

#### `Range(rangeString)` (aliases: `R`)

Usage:

- `Usage: Range(rangeString)`

Parses a range string and returns a range object usable in other functions.

Range strings:

- `'A-B'`, `'*'`, `'?'`, `'A+'`, `'A-'`, `'|A-B'`, `'A-B|'`

Implementation details:

- `Range(rangeString, start=True/ end=True)` supports keyword arguments:
  - `end=True` makes the end exclusive
  - `start=True` makes the start exclusive
- `Range(x)` where `x` is a number makes a single-value range.
- `Range(start, end)` where both are numbers creates an inclusive numeric range.

Exclusivity can also be controlled directly in the range string itself using the `|` syntax (e.g. `|1-5` or `1-5|`).

---

#### `MultiRange(range1, range2, ...)` (aliases: `MR`)

Usage:

- `Usage: MultiRange(range1, range2, ...)`

Returns a multi-range that matches if **any** of the given ranges match.

Each argument can be:

- a range string
- a `Range` object
- a `MultiRange` object

Special: string multi-range can include multiple segments separated by spaces, e.g. `MultiRange('0-1 5-7')`.

---

#### `Regex(pattern)` (aliases: `Re`)

Usage:

- `Usage: Regex(pattern)`

Parses a regex pattern and returns a regex object to be used in other functions.

Implementation note:

- The underlying `Regex(...)` constructor always sets ignore-case flags.
- Even if you pass a `case=` keyword to `Regex(...)`, the current implementation effectively keeps ignore-case enabled (so matching is case-insensitive in practice).
- IronSearch’s `Regex(...)` implementation also accepts a second string form `(text, pattern)` and returns a boolean match immediately.

---

#### `Random()`

Usage:

- `Usage: Random() or Random(start, end)`

Returns:

- `Random()` => random real number between `0.0` and `1.0` (float)
- `Random(start, end)` => random integer in `[start, end)` (end exclusive)

---

#### `EmptyMultiRange()` (aliases: `EMR`)

Usage:

- `Usage: EmptyMultiRange()`

Returns an empty multi-range which matches nothing.

---

#### `FullRange()` (aliases: `FR`)

Usage:

- `Usage: FullRange()`

Equivalent to the `'*'` wildcard.

---

#### `FullMultiRange()` (aliases: `FMR`)

Usage:

- `Usage: FullMultiRange()`

Equivalent to the `'*'` wildcard.

---

#### `InvalidRange()` (aliases: `IR`)

Usage:

- `Usage: InvalidRange()`

Returns an invalid range (matches nothing).

Implementation note:

- Help text mentions a `',' wildcard` but the actual parser uses `'?'` to represent invalid ranges.

---

#### `InvalidMultiRange()` (aliases: `IMR`)

Usage:

- `Usage: InvalidMultiRange()`

Returns an invalid multi-range (matches nothing).

Help text also states it is "not useable everywhere"; that limitation is implemented by which tags treat invalid ranges specially.

---

#### `GetCallbacks()` (aliases: `Callbacks`)

Usage:

- `Usage: GetCallbacks() or Callbacks()`

Returns a list of callback difficulties for the current song.

---

#### `GetDifficulties()` (aliases: `Difficulties`, `Diffs`)

Usage:

- `Usage: GetDifficulties() or Difficulties() or Diffs()`

Returns a list of the current song’s difficulty strings.

---

#### `GetHighscores()` (aliases: `GetHighScores`, `Highscores`, `HighScores`, `Highs`)

Usage:

- `Usage: GetHighscores() or GetHighScores() or Highscores() or HighScores() or Highs()`

Returns a list of `Highscore` objects.

`Highscore` fields (from `Records/Highscore.cs`):

- `Uid` (string)
- `Evaluate` (int)
- `Score` (int)
- `Combo` (int)
- `Clear` (int)
- `AccuracyStr` (string)
- `Accuracy` (float)

---

#### `GetLanguage()` (aliases: `Language`)

Usage:

- `Usage: GetLanguage() or Language()`

Returns the music’s language as an index (`0` Japanese, `1` English, etc.).

---

#### `GetLength()` (aliases: `Length`)

Usage:

- `Usage: GetLength() or Length()`

Returns the music length in seconds (or `None`/null if unavailable).

---

#### `GetBPM()`

Returns the BPM `Range` of the song (or `None`/null if not available).

---

#### `GetModified()`

It returns the custom chart’s last-modified time.

---

### 6.3 Variables

Variable tags let you keep state across songs or within a song evaluation.

Important lifetime:

- When the search text changes and advanced search re-initializes, `SearchPatch.Prefix` clears:
  - global variables
  - local variables

So variables are not persisted across separate searches.

#### `DefineVar(varName, value)` (alias: `DV`)

Usage:

- `Usage: DefineVar(varName, value)`

If the variable does not exist, initializes a **local** variable for the current song’s filter with the given value.

If the variable already exists, it does not overwrite.

---

#### `GetVar(varName)` (aliases: `GV`, `LoadVar`, `LV`)

Usage:

- `Usage: GetVar(varName)`

Returns the local variable value for the current song filter.

If the variable is undefined, the search fails with a reference error.

---

#### `SetVar(varName, value)` (alias: `SV`)

Usage:

- `Usage: SetVar(varName, value)`

Intended behavior (per help):

- overwrite a local variable for the current song filter

Reality (from current code in `Tags/Vars/SetVar.cs`):

- `SetVar` initializes `LocalVariables[uid]` but then writes to `GlobalVariables[uid][varName]` instead of `LocalVariables[uid][varName]`.
- This means `SetVar` may not work as documented (and may throw if the expected global dictionary entry does not exist).

Recommendation:

- Prefer `DefineVar(...)` + `GetVar(...)` for now.
- If you need overwrite semantics, verify `SetVar` in your build or fix the implementation.

---

#### `DefineGlobal(varName, value)` (alias: `DG`)

Usage:

- `Usage: DefineGlobal(varName, value)`

Initializes a global variable (shared across songs in the same search evaluation) if it does not exist.

---

#### `GetGlobal(varName)` (aliases: `GG`, `LoadGlobal`, `LG`)

Usage:

- `Usage: GetGlobal(varName)`

Returns global variable value created by `DefineGlobal` or `SetGlobal`.

If the variable does not exist, the search fails.

---

#### `SetGlobal(varName, value)` (alias: `SG`)

Usage:

- `Usage: SetGlobal(varName, value)`

Overwrites a global variable value.

---

### 6.4 Control-flow / debugging / sorting tags

#### `Exit(returnValue)`

Usage:

- `Usage: Exit(returnValue)`

Short-circuits the search by throwing a termination exception.

Effect:

- `Exit(True)` makes the search succeed instantly
- `Exit(False)` makes it fail instantly

---

#### `Sorter(comparerFunction)` / `Sort(comparerFunction)`

Usage:

- `Usage: Sorter(comparerFunction)`

Registers the sorting comparers for the final song list ordering.

Implementation details:

- `Sorter(...)` returns `True` (it does not filter)
- It registers a "sort block" containing one or more comparer callables

Optional keywords supported by implementation:

- `reverse=...`
- `priority=...` (int)

---

#### `Log(value)`

Usage:

- `Usage: Log(value)`

Logs values to the console for debugging purposes.

Notes:

- The help warns to prefer `LogOnce`/`LogUnique` because logging too much may overload the console.

---

#### `LogOnce(value, id='someId')`

Usage:

- `Usage: LogOnce(value, id='someId')`

Logs the message only once per `id`.

Implementation also supports keyword `sep=...` to change the separator.

---

#### `LogUnique(value)`

Usage:

- `Usage: LogUnique(value)`

Logs only once per unique message string.

Implementation supports `sep=...`.

---

#### `RunOnce(function, id)`

Usage:

- `Usage: RunOnce(function, id)`

Implementation reality:

- The first argument must be a **callable with no arguments** (lambda/function of zero parameters).
- The second argument must be a **string id**.
- When `id` is encountered for the first time, the callable is executed.
- The return value of the callable is ignored; `RunOnce(...)` always returns `True`.

So `RunOnce(...)` is best used for side effects (warming caches, precomputations, etc.), not for "returning the checked boolean".

---

#### `Help(...)` (TAG IMPLEMENTED, but NOT registered; and currently incomplete)

`Tags/Actions/Help.cs` exists, but:

- `ModMain.LoadUserScripts()` does not register a `Help` tag by default.
- Even in the current implementation, `EvalHelp` only locks/releases a mutex and immediately returns `True`; it never calls `EvalHelpInternal`, so it does not print help output.

If you register/fix it, the intended behavior is:

- `Help("TagName")` prints the built-in help string (from `ModMain.HelpStrings`).

---

### 6.5 Sorting comparer key tags (return a comparer callable)

These tags return callables that can be used inside `Sorter(...)`.

#### `ByAccuracy()` (alias: `ByAcc`)

Usage:

- `Usage: ByAccuracy()`

Returns a comparer callable to sort by score accuracy.

---

#### `ByBPM()`

Usage:

- `Usage: ByBPM()`

Returns a comparer callable to sort by BPM.

---

#### `ByDifficulty()` (alias: `ByDiff`)

Usage:

- `Usage: ByDifficulty()`

Returns a comparer callable to sort by difficulty.

---

#### `ByLength()`

Usage:

- `Usage: ByLength()`

Returns a comparer callable to sort by chart length.

---

#### `ByName()`

Usage:

- `Usage: ByName()`

Returns a comparer callable to sort by localized music title.

---

#### `ByRandom()`

Usage:

- `Usage: ByRandom()`

Returns a comparer callable that sorts songs in a random order.

---

#### `ByModified()`

Usage:

- `Usage: ByModified()`

Returns a comparer callable that sorts by custom chart "newness" index based on filesystem last write times.

---

#### `ByScene()`

Usage:

- `Usage: ByScene()`

Returns a comparer callable that sorts by scene id string.

---

#### `ByUID()`

Usage:

- `Usage: ByUID()`

Returns a comparer callable that sorts by UID.

---

## 6.6 Unregistered / internal placeholder implementations

Some `Tags/*` implementation files exist in the repository but are not registered into the expression language by `ModMain.LoadUserScripts()`. They may not be callable as keywords unless you add registration in code, but they still exist as implementations.

### Implemented, but not registered by default

- `GetBPM()` (`Tags/Objects/GetBPM.cs`)
- `GetModified()` (`Tags/Objects/GetModified.cs`)

### Internal `_Empty` placeholders

The following implementation files are placeholders and not exposed as usable keywords by default:

- `Tags/_Empty.cs`
- `Tags/Actions/_Empty.cs`
- `Tags/Objects/_Empty.cs`
- `Tags/Comparers/_Empty.cs`

## 7) Advanced Features

### 7.1 Custom sorting comparers (your own `lambda A, B: ...`)

You can pass any callable into `Sorter(...)` as long as it matches the comparer contract:

- takes exactly 2 args (`A`, `B`) where both are `MusicInfo`
- returns an `int` comparison result

Example: sort by BPM range object (custom logic; assumes `A.bpm` and `B.bpm` are comparable in your context):

```text
search: Sorter(lambda A, B: A.bpm.CompareTo(B.bpm))  # may require fields to exist as comparable types
```

More reliable: just use the built-in comparer tags like `ByLength()` / `ByName()`.

### 7.2 The `Scripts/` directory (user-defined tags)

User scripts live in a folder created automatically at:

- `MelonEnvironment.UserDataDirectory/Scripts/`

On startup, IronSearch also writes a template example file:

- `Scripts/Unpacked.py`

### Script file format

Every `.py` file in that folder becomes a new callable "tag" whose name is the file name (without extension).

Each script file must define a function:

- name: `run`
- at least two positional parameters: `run(M, T, ...)`

Where:

- `M` is the current song (`SearchArgument` dynamic wrapper)
- `T` is a dictionary-like object holding the underlying tag callables

Inside `run(M, T)`, you typically call built-ins through `T`:

```python
def run(M, T):
    return T['Custom'](M, T) and not T['Packed'](M, T)
```

This matches the mod’s generated example template.

### Calling your script from an expression

If you create:

- `Scripts/Unranked.py`

that defines `run(M, T)`, then in search expressions you can call:

- `Unranked()`

### Hot reloading

`UserScriptManager` watches the `Scripts/` folder (`*.py`) and automatically reloads scripts on file create/change/delete/rename.

### Script naming rules

The script file name must be a valid Python identifier (variable-name rules used in `PythonExpressionManager.Extensions.IsValidVariableName`).

Reserved keywords (like `and`, `def`, `class`, `return`, etc.) are not allowed as script/tag names.

---

### 7.3 Expressions + aliases from settings

In addition to `.py` scripts, the mod can compile expression shorthands and aliases from preferences:

- `Expressions`: registers compiled expression as a callable script with `()`
- `TagAliases`: remaps keywords to other expressions/keywords

Both are compiled by the same `PythonExpressionManager` engine used for built-in tags.

### 7.4 Practical patterns

Warm caches / avoid repeated work:

- use `LogOnce(...)` / `LogUnique(...)` for debugging output
- use `RunOnce(function, id)` for side-effect caching (callable with zero args)

State across songs:

- use `DefineGlobal(...)`, `GetGlobal(...)`, `SetGlobal(...)`

State within the current song evaluation:

- use `DefineVar(...)`, `GetVar(...)`

Note: `SetVar(...)` has a mismatch between help text intent and current implementation (see tag reference).

---

