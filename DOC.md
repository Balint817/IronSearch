# IronSearch Advanced Search Guide

This is the documentation for the "advanced search" features implemented by IronSearch.\
For example, functions like `BPM('160-200')`.

**Practical tip:** Typing long queries in-game may get tedious.\
It's a good idea to put longer queries into an 'Expression' to re-use later.

| Section | Contents |
|---------|----------|
| [§1 Quick Start](#1-quick-start) | Prefix, first examples, booleans, sorting teaser |
| [§2 Syntax](#2-syntax) | Truthiness, quoting, types, operators, errors, `M`, keyword arguments |
| [§3 Range syntax](#3-range-syntax) | `'10+'`, `'?\|…'`, multi-ranges |
| [§4 Settings](#4-settings) | `IronSearch.cfg`, aliases, `AutoCompleteItems` |
| [§5 Auto-complete](#5-auto-complete-tab-suggestions) | **Tab** dropdown: when it works, keys, behavior (`AutoCompleteManager.cs`) |
| [§6 Sorting](#6-sorting-guide) | `Sorter(...)` / comparers |
| [§7 Tag reference](#7-tag-reference) | Full tag list |
| [§8 Advanced](#8-advanced-features) | Lambdas, `Scripts/`, config hooks |
| [§9 Cookbook](#9-example-cookbook) | Copy-paste patterns |

---

## 1) Quick Start

### Enable advanced search

Your search text must start with the configured prefix (default: `search:`). The mod then removes the prefix and uses the remaining text as your search expression.

Example (using default settings):

```text
search: BPM('160-200') and Length('180+')
```

(This example will search for songs that have a BPM between 160 and 200, and are longer than 180 seconds)

![Search demo](/Resources/hook.gif)

### Basic boolean logic

Use boolean operators:

- `and`, `or`, `not` (must be lowercase)
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

The result of your expression is always converted to a True or False value.\
This is a common point of failure:

```text
search: AP()
```

The above matches songs that have all-perfect on the highest map (probably what you intended).

```text
search: AP
```

If you forget the parentheses, the game thinks you are just naming the command instead of actually running it, so it accidentally matches everything.

![Search demo](/Resources/parenthesis.gif)

### 'string'-ing 'quote'-d 'text' along (quotes and strings)

Single quotes (`'...'`) and double quotes (`"..."`) are interchangeable - they both produce the same 'string' ('string' means 'text'). Use whichever you prefer; just make sure the opening and closing quotes match.

```text
search: BPM('160+')
search: BPM("160+")
```

Both of these are identical.

A string that is passed to a tag expecting a range will be **parsed as a range string** automatically. A bare (unquoted) number is just a number — it is *not* a range.

```text
search: BPM('160-180')    # range string '160-180' → matches BPM from 160 to 180
search: BPM(160-180)      # the result of the mathematical expression 160-180, e.g. -20, or BPM(-20) (obviously no such song exists)
```

So when a tag expects a range, you should pass a quoted string.

#### Escape sequences

You may ask, "if quotes decide where strings start and end, how do I include a quote in the text itself?"\
This is where 'escaping' comes in. By using a backslash (`\`) before a special character, you can include it in the string without it being interpreted as a control character:

For example:
- `\'` or `\"` (escaped quote)
- `\\` (to type one backslash)

In practice you rarely need them, but they are available if you do.

### Operators and precedence

The boolean operators, from **highest** to **lowest** precedence:

| Priority | Operator | Description |
|----------|----------|-------------------------------|
| 1 | `not` | Negation (evaluated first) |
| 2 | `and` | Logical AND |
| 3 | `or` | Logical OR (evaluated last) |

Use parentheses to override the default order:

```text
search: Custom() and not Packed() or Favorite()
```

is interpreted as `(Custom() and (not Packed())) or Favorite()`. If you meant "custom charts that are either unpacked or favorited", write:

```text
search: Custom() and (not Packed() or Favorite())
```

The `in` operator is also available (e.g. `'text' in M.author`), but you will rarely need it.\
Dedicated tags like `Author(...)` cover most use-cases, and are usually preferred as they perform many more checks (Romanization, multiple languages, etc.)

### Error messages

If your expression has a problem, the error will appear in the MelonLoader console. There most important ones you'll see are:

1. **Syntax error** — Your expression could not be parsed. The message shows the character position where parsing failed.
   ```
   SyntaxError: unexpected ')' at position 12
   ```
2. **Runtime error** — The expression parsed, and your usage of tags was correct, but something else went wrong during execution. For example, the expression `1/0` would give:
   ```
   ZeroDivisionError: division by zero
   ```
3. **Tag/Input error** — A specific tag encountered an issue. The message shows which tag was called (including the arguments you passed) and the error:
   ```
   Error in Cinema(5): Unexpected positional arguments (Cinema does not take any arguments)
   ```

Example of a syntax error:

![Syntax error demo](/Resources/error_syntax.gif)

Example of an invalid input:

![Input error demo](/Resources/error_tag.gif)

### Keyword arguments

Keyword arguments are supported, but each tag controls which keywords it accepts.

For example, the `Range(...)` object allows you to specify whether the start or end should be exclusive. (`start=True/False`, `end=True/False`)

---

## 3) Range Syntax

Many tags accept "range strings" (e.g. BPM ranges, difficulty ranges, etc.).

### Map index reference

Several tags accept a `mapRange` argument that refers to difficulty slots by number:

| Index | Difficulty slot |
|-------|-----------------|
| 1 | Easy |
| 2 | Hard |
| 3 | Master |
| 4 | Hidden |
| 5 | Touhou |

For example, `Difficulty('11+', '3')` checks the **Master** slot, and `FC('4')` checks the **Hidden** slot.

### Range format

1. Exact range:
   - `'A-B'` matches `A <= value <= B`
2. Wildcard / Full range:
   - `'*'` matches any value
3. Wildcard / Invalid range:
   - `'?'`, see notes below.
4. Open-ended:
   - `'A+'` matches `value >= A` (A-to-infinity)
   - `'A-'` matches `value <= A` (negative infinity-to-A)
5. Exclusive endpoints with `|`
   - `'|A-B'` makes the **start** exclusive: `A < value <= B`
   - `'A-B|'` makes the **end** exclusive: `A <= value < B`

Notes about `?`:

- By default, `'?'` is a range that **matches nothing**.
- However, several tags interpret `'?'` specially as "select the highest available map"
- Which tags do this is implemented per-tag (see reference below or the Help text).

### Multi-range strings (OR of multiple ranges)

Some inputs accept multi-ranges. A multi-range takes multiple ranges, and matches any value that matches any of the ranges that make it up.

For example, `MR('100- 200+')` or `MR('100-', '200+')` (preference), matches values lower than 100, or higher than 200.

### Working examples for simple tags using ranges:

```text
search: BPM('160+')
```

BPM 160 or higher.

```text
search: Difficulty('9-11', '?')
```

Displayed difficulty 9 through 11 on the **highest** map index.

```text
search: Accuracy('95-100', '?')
```

Narrow to songs where your **top-slot** scores are 95%–100% accuracy.

![Search demo narrowing BPM](/Resources/bpm_narrowing.gif)


---

## 4) Settings

Settings are stored under the path: `<Muse Dash Folder>/UserData/IronSearch.cfg`

### `StartSearchText` (default: `search:`)

The prefix that must start your search text for an advanced search to actually happen.

If you set it to an empty string, advanced search is always enabled (NOT recommended).

(Personally, I have it set to `::`)

### `EnablePersistentSearchCaching` (default: `true`)

Enable to cache search results.

For some reason, the game often refreshes the search results multiple times in a row, refreshes them when you open the search UI (without the search text changing), etc...\
This can cause a lot of lag if your search expression is complex (and/or has sorting) and has to be re-evaluated multiple times.\
For this reason, this setting is enabled by default, and it caches the results of searches.

If you plan to use advanced features with side-effects, this may interfere with the expected behavior. Otherwise, it's generally recommended to keep this enabled to avoid random long freezes.

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
Autocomplete also includes aliases.

### `Expressions`

A dictionary of shorthands for searches.

Each entry:

- <name> = "<expression>"

Example concept:

- `Expressions = { NewCustom = "Unplayed() and Custom()" }`

After that, you can call:

- `NewCustom()`

#### Expression arguments (advanced)

In expressions, the built-in `M` parameter gets 2 extra properties:

- `M.A`, which are the 'Arguments' passed to the expression.
- `M.K`, which are the 'Keyword arguments' passed to the expression.

Expanding on the previous example:

- `Expressions = { NewCustom = "Unplayed(M.A[0]) and Custom()" }`

After that, you can call:

- `NewCustom('?')`

This will pass the `?` argument into the `Unplayed()` tag, selecting only customs that are unplayed on their highest map.

### `AutoCompleteItems`

Dictionary of extra autocomplete keywords.

For example, the default value is:

- `AutoCompleteItems = { Vanilla = "not Custom()" }`

Unlike an expression, does not register an entirely new tag, but simply adds an extra suggestion to the auto-complete.

---

## 5) Auto-complete (Tab suggestions)

IronSearch can suggest keywords while you type an advanced search.

### When you can use it

All of the following must be true:

1. The **game search UI is open** and PopupLib is installed.
2. The current search has to be an advanced search (starting with the configured start text).
3. The caret position is within the search itself and not the start text.
4. The characters immediately around the caret form a valid keyword. (e.g. cannot autocomplete `search:123` because it's just a number)

### What to press

| Action | Effect |
|--------|--------|
| **Tab** (first time) | Opens the suggestion dropdown near the caret. |
| **Up** / **Down**, **Page Up** / **Page Down**, mouse wheel | Move the highlighted row. |
| **Tab** again (or **click** a row) | Replaces the partial text with the selected auto-complete keyword. |
| **Left** / **Right** arrow (or continue typing) | Close the list **without** inserting. |

![Quick search demo using autocomplete](/Resources/autocomplete.gif)

---

## 6) Sorting Guide

Sorting is done only when:

- advanced search is on, and
- parsing/evaluation did not produce an error, and
- the expression registered at least one sorter via `Sorter(...)` / `Sort(...)`.

### The sorting call

`Sorter(comparer)` (alias `Sort`)

You can provide built-in comparer key functions:

- `ByLength()`, `ByName()`, `ByUID()`, etc.

Example:

```text
search: Custom() and Sort(ByLength())
```

![Sorting demo](/Resources/sorting.gif)

### Multiple comparers

You can pass multiple comparer functions:

```text
search: Sorter(ByBPM(), ByName()) and Custom()
```

Comparers are evaluated in order. The first non-equal comparison decides the ordering.

### Reverse + priority (advanced)

The underlying `Sorter(...)` implementation supports additional keyword arguments:

- `reverse=...` (True/False; negates the first non-equal comparison result)
- `priority=...` (integer, lower number is higher priority)

If there are 2 or more sorter calls with equal priority, only the last is executed. \
If there are 2 or more sorter calls with different priorities, they are executed in ascending order.

These arguments can also be passed as positional arguments, omitting the keyword.

### Custom comparers (VERY advanced)

You can pass a function created in your expression.

The `comparer` concept:

- A comparer is a function that takes **two** `MusicInfo` objects: `A` and `B`
- It returns an `int`:
  - negative: `A` should come **before** `B`
  - zero: `A` is **equal** to `B`
  - positive: `A` should come **after** `B`

For example, the ByUID sorting can be re-implemented like this:

```text
search: Sorter(lambda A, B: (A.uid > B.uid) - (A.uid < B.uid))
```

---

## 7) Tag Reference

This section documents **every tag** available by default. But first...

#### `Help`

- `Help("TagName")` prints the built-in help text for the given tag.

### 7.1 Filters (returns `True`/`False`)

#### `Accuracy` / `Acc`

Usage:

- `Accuracy(accuracyRange) or Accuracy(accuracyRange, mapRange)`

Checks if the music has scores in the specified accuracy range, optionally restricting to specific maps.

Notes:

- Input accuracy is a **percentage** range (e.g. `90-100`).
- If the map range is a wildcard `?`, the tag selects the **highest available map**.

Example:

```text
search: Accuracy('90+', '3')
```

Matches songs where you have 90% or higher accuracy on the Master of the map.

---

#### `Album`

Usage:

- `Album(albumName) or Album(regex)`

Checks if the music belongs to the specified album name.

Notes:

- Album names come from the game/custom album data, not necessarily matching what you see in UI.

Example:

```text
search: Album('treatment')
```

Matches if `treatment` appears in album name.

---

#### `Any`

Usage:

- `Any(text) or Any(regex)`

Checks if the music matches any of the following:

- `Album`
- `Author`
- `Designer`
- `Tag`
- `Title`

Example:

```text
search: Any('cyber')
```

Matches if `cyber` appears in album, author, designer, tag, or title text.

---

#### `AP` / `AllPerfect`

Usage:

- `AP() or AP(mapRange)`

Checks if the music has **all perfect** scores in the specified map range.

Notes:

- If you pass wildcard `?`, the implementation selects the **highest available map**.
  - This is the default behavior for `AP()`.
- The tag requires perfect for every map in the selected range.

Example:

```text
search: AP('3-4')
```

Matches songs where you have an AP in either the master and the hidden maps.

---

#### `Author`

Usage:

- `Author(authorName) or Author(regex)`

Checks if the music’s author matches the specified input.

Example:

```text
search: Author('leaf')
```

Matches if `leaf` appears in the author's name.

---

#### `BPM`

Usage:

- `BPM(bpmRange)`

Checks if the music’s BPM is within the given BPM range.

Example:

```text
search: BPM('150-200')
```

Matches if the BPM of the song is within the range 150-200 (inclusive).

---

#### `Callback`

Usage:

- `Callback(callbackRange) or Callback(callbackRange, mapRange)`

Checks if the music has a callback difficulty that matches the provided range.

Notes:

- If `mapRange` is the wildcard `?`, it selects the **highest available map**.
- Callback difficulty is the "difficulty the game servers use", not the displayed difficulty.
- Callback difficulty is always an integer; string difficulty like `E` turns into -1.

Examples:

```text
search: Callback('11+', '3')
```

Matches if there's a callback difficulty of 11 or higher on the Master of the map.

---

#### `Cinema`/`Video`

Usage:

- `Cinema()`

Checks if the music is a custom chart with an animated video background.

---

#### `Clears`

Usage:

- `Clears(clearRange) or Clears(clearRange, mapRange)`

Checks if the music has clears in the specified range, optionally restricting to specific maps.

Notes:

- If the map range is a wildcard `?`, the tag selects the **highest available map**.

Example:

```text
search: Clears('5+', '3')
```

Matches songs where you have 5 or more clears on the Master of the map.

---

#### `Custom`

Usage:

- `Custom()`

Checks if the music is a custom chart.

---

#### `Designer`/`Design`/`LevelDesigner`/`LevelDesign`

Usage:

- `Designer(designerName) or Designer(regex)`

Checks if the chart’s level designer matches the specified input.

Example:

```text
search: Designer('vig')
```

Matches if `vig` appears in the designer's name.

---

#### `Difficulty`/`Diff`

Usage:

- `Difficulty(difficultyRange, mapRange)`

Checks if the music has a difficulty in the given difficulty range, optionally restricting which maps match.

Notes:

- `mapRange` set to `?` means "select the highest available map".
- `difficultyRange` set to wildcard `?` will match non-integer difficulties like `?` or `E`.

Example:

```text
search: Difficulty('11+', '3')
```

Matches songs where the Master of the map is difficulty 11 or higher.

---

#### `Favorite`/`Fav`

Usage:

- `Favorite()`

Checks if the music is in your favorites list.

---

#### `FullCombo`/`FC`

Usage:

- `FullCombo() or FullCombo(mapRange)`

Checks full combo status.

Notes:

- If `mapRange` is `'?'`, it selects the highest applicable difficulty.

Example:

```text
search: FC('3')
```

Matches songs where the Master of the map has a full combo.

---

#### `HasHidden`/`Hidden`/`HasSupreme`/`Supreme`

Usage:

- `Hidden()`

Checks whether the music has a "hidden" difficulty (difficulty 4).

---

#### `HasMap`/`Map`

Usage:

- `Map(mapRange)`

Checks whether the music has a map in the specified range.

Example:

```text
search: Map('3')
```

Matches songs that have a Master difficulty.

---

#### `HasTouhou`/`Touhou`

Usage:

- `Touhou()`

Checks whether the music has a "Touhou" difficulty (difficulty 5).

---

#### `History`

Usage:

- `History()`

Checks if the music is in the chart history (recently played charts in the 'History' tab).

---

#### `Length`

Usage:

- `Length(lengthRange)`

Returns `True` if the music’s length falls within the given range (see also `GetLength` in §7.3 which returns the actual length as a number).

Notes:

- Accepts numeric/time ranges:
  - normal numeric ranges like `'180-240'` (seconds)
  - time strings like `'1m30s'`

Example:

```text
search: Length('1m30s-3m')
```

Matches songs with length between 1 and a half minutes, and 3 minutes.

---

#### `Modified`

Usage:

- `Modified(timeString)`

Checks if the custom chart was last modified within the given time window.

- Accepts time ranges (like `'1h'`).

Example:

```text
search: Modified('7d')
```

Matches songs that have been modified (or added) in the last 7 days.

---

#### `New`

Usage:

- `New(topRange)`

Checks if the music is among the Nth last added custom charts.

Notes:

- It only applies to custom charts.

Example:

```text
search: New(5)
```

Returns the 5 most recently added custom charts.

---

#### `Old`

Usage:

- `Old(bottomRange)`

Checks if the music is among the Nth first added custom charts.

Notes:

- Only applies to custom charts.

Example:

```text
search: Old(5)
```

Returns the 5 oldest custom charts.

---

#### `Online`

Usage:

- `Online()`

Checks if the music is an online chart (custom from the website).

Notes:

- Only works when HQ is available (`EnableHQSpam=true`).

---

#### `Packed`

Usage:

- `Packed()`

Checks if the music is a packed custom chart (e.g. `.mdm` file), not a folder.

---

#### `Ranked`

Usage:

- `Ranked()`

Checks if the music is a ranked custom chart.

Notes:

- Only works when HQ is available (`EnableHQSpam=true`).
- There may be a delay in ranking info updating after enabling HQ spam (implementation note from help string).

---

#### `Scene`

Usage:

- `Scene(sceneName) or Scene(sceneIndex)`

Checks if the music is in a specific scene.

Accepted inputs:

- a scene name (case-insensitive substring match)
- a numeric scene index

Ambiguity:

- If the typed scene name matches multiple known scenes, the search fails.

Example:

```text
search: Scene('candy')
```

Gets the scene that matches `candy` in its name (e.g. `Candyland`),\
and matches every song that has that scene.

---

#### `Streamer`

Usage:

- `Streamer()`

Checks whether the music is in the streamer list (copyright-safe music).

Notes:

- Customs do not support this feature.

---

#### `Tag`

Usage:

- `Tag(tagName) or Tag(regex)`

Checks if the music has a "music tag" whose text matches the provided string/regex.

Example:

```text
search: Tag('anime')
```

Matches songs that have a search tag that contains `anime`.

---

#### `Title`

Usage:

- `Title(title) or Title(regex)`

Checks if the music’s title matches.

Example:

```text
search: Title('Muse')
```

Matches songs where the title contains `Muse`.

---

#### `Unplayed`

Usage:

- `Unplayed() or Unplayed(mapRange)`

Checks whether the music has not been played, or (optionally) not played on one of the specified difficulty maps.

Notes:

- Wildcard `'?'` as `mapRange` selects highest applicable difficulty.

Example:

```text
search: Unplayed('3')
```

Matches songs where the Master is unplayed.

---

### 7.2 Sorting comparers

These tags return comparers that can be used inside `Sorter(...)`.

#### `ByAccuracy`/`ByAcc`

Usage:

- `ByAccuracy()`

Returns a comparer function to sort by score accuracy.

---

#### `ByBPM`

Usage:

- `ByBPM()`

Returns a comparer function to sort by BPM.

---

#### `ByDifficulty`/`ByDiff`

Usage:

- `ByDifficulty()`

Returns a comparer function to sort by difficulty.

---

#### `ByLength`

Usage:

- `ByLength()`

Returns a comparer function to sort by chart length.

---

#### `ByName`

Usage:

- `ByName()`

Returns a comparer function to sort by localized music title.

---

#### `ByRandom`

Usage:

- `ByRandom()`

Returns a comparer function that sorts songs in a random order.

---

#### `ByModified`

Usage:

- `ByModified()`

Returns a comparer function that sorts by custom chart "newness" index based on filesystem last write times.

---

#### `ByScene`

Usage:

- `ByScene()`

Returns a comparer function that sorts by scene id string.

---

#### `ByUID`

Usage:

- `ByUID()`

Returns a comparer function that sorts by UID.

### 7.3 Objects (advanced)

---

#### `Range`/`R`

Usage:

- `Range(rangeString) or Range(a, b)`

Parses a range string and returns a range object usable in other functions.

Examples:

- `'A-B'`, `'*'`, `'?'`, `'A+'`, `'A-'`, `'|A-B'`, `'A-B|'`

Implementation details:

- `Range(rangeString, start=..., end=...)` supports keyword arguments:
  - `start=...` True/False, makes the **start** exclusive
  - `end=...` True/False, makes the **end** exclusive
- `Range(x)` where `x` is a number makes a single-value range.
- `Range(x)` where `x` is a string parses it into a Range.
- `Range(start, end)` where both are numbers creates an inclusive numeric range.

Examples:

```text
search: R('10-12')
```

```text
search: R(10, 12)
```

---

#### `MultiRange`/`MR`

Usage:

- `MultiRange(range1, range2, ...)`

Returns a multi-range that matches if **any** of the given ranges match.

Each argument can be:

- a range string
- a `Range` object
- a `MultiRange` object

Special case: strings in multi-range can include multiple segments separated by spaces, e.g. `MultiRange('0-1 5-7')`.

---

#### `Regex` / `Re`

Usage:

- `Regex(pattern)`

Parses a regex pattern and returns a regex object to be used in other functions. If you have no idea what that means, you probably don't need it.

Implementation note:

- The underlying `Regex(...)` constructor is ignore-case by default
- If you pass a `case=true` keyword to `Regex(...)`, you can make it case-sensitive
- If you call `Regex(pattern, text)`, a boolean match is returned instantly instead.

---

#### `NotNone`

Usage:

- `NotNone(list)`

Returns a copy of the list with None/null values filtered out.

---

#### `Random`

Usage:

- `Random() or Random(start, end)`

Returns:

- `Random()` => random **real number** in `[0.0, 1.0)` (end exclusive)
- `Random(range)` => random **real number** in `[start, end)` (end exclusive)
- `Random(start, end)` => random **integer** in `[start, end)` (end exclusive)

---

#### `EmptyMultiRange`/`EMR`

Usage:

- `EmptyMultiRange()`

Returns an empty multi-range which matches nothing.

---

#### `FullRange`/`FR`

Usage:

- `FullRange()`

Equivalent to the `'*'` wildcard. Matches everything.

---

#### `FullMultiRange`/`FMR`

Usage:

- `FullMultiRange()`

Equivalent to the `'*'` wildcard. Matches everything.

---

#### `Fuzzy`/`F`

Usage:

- `Fuzzy(pattern, case=true/false) or Fuzzy(pattern, text, case=true/false)`

The first usage returns an object which can be used to fuzzy-match text, while the latter instantly fuzzy-matches the provided text.
The first one will have a method `.Match(text)` that returns `True` if the text matches the pattern.

---

#### `InvalidRange`/`IR`

Usage:

- `InvalidRange()`

Equivalent to the `'?'` wildcard. Matches nothing.

---

#### `InvalidMultiRange`/`IMR`

Usage:

- `InvalidMultiRange()`

Returns an invalid multi-range.

---

#### `GetCallbacks`/`Callbacks`

Usage:

- `GetCallbacks()`

Returns a list of callback difficulties for the current song.

---

#### `GetDifficulties`/`Difficulties`/`Diffs`

Usage:

- `GetDifficulties()`

Returns a list of the current song’s difficulty strings.

---

#### `GetHighscores`/`GetHighScores`/`GetScores`

Usage:

- `GetHighscores()`

Returns a list of `Highscore` objects.

`Highscore` fields:

- `Uid` (string)
- `Evaluate` (int)
- `Score` (int)
- `Combo` (int)
- `Clears` (int)
- `AccuracyStr` (string)
- `Accuracy` (float)

---

#### `GetLanguage`/`Language`

Usage:

- `GetLanguage()`

Returns the current language index.

---

#### `GetLength`

Usage:

- `GetLength()`

Returns the music length in seconds **as a number**, or `None` if the mod failed to obtain it. (The latter is only possible for corrupted customs)

This is different from the `Length(range)` filter in §7.1 which checks whether the length is *within* a range and returns `True`/`False`.

---

#### `GetBPM`

Returns the BPM `Range` of the song (or `None` if not available).

---

#### `GetModified`

It returns the custom chart’s last-modified time.

---

### 7.4 Variables (VERY advanced)

Variable tags let you keep state across songs or within a song evaluation.

When the search text changes and advanced search re-initializes, all variables all cleared.

Local variables persist per song, global variables persist for the current search.

#### `DefineVar`/`DV`

Usage:

- `DefineVar(varName, value)`

If the variable does not exist, initializes a **local** variable for the current song’s filter with the given value.

If the variable already exists, it does not overwrite.

---

#### `GetVar`/`GV`/`LoadVar`/`LV`

Usage:

- `GetVar(varName)`

Returns the local variable value for the current song filter.

If the variable is undefined, the search fails with a reference error.

---

#### `SetVar`/`SV`

Usage:

- `SetVar(varName, value)`

Creates, or overwrites a local variable for the current song filter with the given value.

---

#### `DefineGlobal`/`DG`

Usage:

- `DefineGlobal(varName, value)`

Initializes a global variable (shared across songs in the same search evaluation) if it does not exist.

---

#### `GetGlobal`/`GG`/`LoadGlobal`/`LG`

Usage:

- `GetGlobal(varName)`

Returns global variable value created by `DefineGlobal` or `SetGlobal`.

If the variable does not exist, the search fails.

---

#### `SetGlobal`/`SG`

Usage:

- `SetGlobal(varName, value)`

Creates or overwrites a global variable value.

---

### 7.5 Control-flow / debugging (VERY advanced)

#### `Exit`

Usage:

- `Exit(returnValue)`

Short-circuits the search by raising an exception with the provided value.

Effect:

- `Exit(True)` makes the search succeed instantly
- `Exit(False)` makes it fail instantly

---

#### `Log`

Usage:

- `Log(value)`

Logs values to the console for debugging purposes.

Notes:

- Prefer `LogOnce`/`LogUnique` because logging too much (1000 songs worth of logs) may crash MelonLoader. (I know that sounds ridiculous but it has actually happened during testing...)

---

#### `LogOnce`

Usage:

- `LogOnce(value, id='someId')`

Logs the message only once per `id`.

Function also supports keyword `sep=...` to change the separator.

---

#### `LogUnique`

Usage:

- `LogUnique(value)`

Logs only once per unique message string.

Function supports `sep=...` to change the separator.

---

#### `RunOnce`

Usage:

- `RunOnce(function, id)`

Runs the provided function only once per `id`.

Notes:

- The first argument must be a **function with no arguments**.
- The second argument must be a **string id**.
- When `id` is encountered for the first time, the function is executed.
- The return value of the function is ignored; `RunOnce(...)` always returns `True`.

So `RunOnce(...)` is best used for side effects (warming caches, precomputations, etc.).

---

#### `RunSync`

Usage:

- `RunSync(function)`

Runs the provided function in a semi-synchronous context (synchronous across other calls to `RunSync`, but not blocking the entire rest of the search).\
Useful if you have a function that is not thread-safe.

Notes:

- The first argument must be a **function with no arguments**.
- The return value of the function is ignored; `RunSync(...)` always returns `True`.

---

## 8) Advanced Features

### 8.1 The implicit current-song object: `M`

During evaluation, the expression has a parameter `M` which contains the objects related to the current search:
- `M.I`, the underlying `MusicInfo`
- `M.PS`, the PeroString object of the current search (if you have no idea what that means, you probably don't need it)
- `M.<name>`, is a shorter way to access `M.I.<name>`

Examples:

- `M.uid`, `M.name`, `M.author`, `M.scene`, etc.

Example filter using `M` directly:

```text
search: Custom() and 'banana' in M.author
```

Custom charts whose author string contains `banana` (case-sensitive).\
For this example, you can use `Author('banana')` instead; `M` is for checks or logic that do not yet have a dedicated tag.


| Property | Value |
|------|----------------|
| `M.uid` | The song's uid, for example `'0-8'` |
| `M.name` | The song's title, for example `'Yuki no Shizuku Ame no Oto'` |
| `M.author` | The song's author, for example `'Tianyou feat.Tokyo Tower'` |
| `M.bpm` | The song's BPM **as a string** (for range, use `GetBPM()`), for example `'130'` |
| `M.scene` | The song's scene in the scene ID format, for example `'scene_01'` |
| `M.levelDesignerX` | The level designer for a specific map of the chart, where X is the index, for example `'Howard_Y'`  |
| `M.levelDesigner` | Hidden level designer field for the chart itself in general, for example, `Howard_Y` |
| `M.difficultyX` | The difficulty **as a string** for a specific map of the chart, for example, `'6'` |
| `M.callbackDifficultyX` | The callback difficulty **as an `int`** for a specific map of the chart, for example, `6` |

### 8.2 The `Scripts/` directory (user-defined tags)

User scripts live in a folder created automatically at:

- `<Muse Dash folder>/UserData/Scripts/`

On startup, IronSearch also writes an example file:

- `Scripts/Unpacked.py`

Scripts use IronPython 3.4 syntax.

### Script file format

Every `.py` file in that folder becomes a new tag whose name is the file name (without extension).

Each script file must define a function that:

- has the name `run`
- at least two positional parameters: `run(M, T, ...)`

Where:

- `M` is the current song (`SearchArgument` dynamic wrapper)
- `T` is a dictionary-like object holding the underlying tag callables

Inside `run(M, T)`, you typically call built-ins through `T`:

```python
def run(M, T):
    return T['Custom'](M, T) and not T['Packed'](M, T)
```

This example matches the mod’s generated example template.

### Calling your script from an expression

If you create:

- `Scripts/MyTag.py`

that defines `run(M, T)`, then in search expressions you can call:

- `MyTag()`

### Script naming rules

The script file name must be a valid Python identifier.

Reserved keywords (like `and`, `def`, `class`, `return`, etc.) are not allowed as script/tag names.

### 8.3 Custom sorting comparers (your own `lambda A, B: ...`)

You can pass any callable into `Sorter(...)` as long as it matches the comparer contract:

- takes exactly 2 args (`A`, `B`) where both are `MusicInfo`
- returns an `int` comparison result

example, sort by UID:

```text
search: Sorter(lambda A, B: (A.uid > B.uid) - (A.uid < B.uid))
```

## 9) Example cookbook

Short patterns you can copy, then edit. Prefix is assumed to be present.

| Goal | Expression |
|------|----------------|
| Customs only | `Custom()` |
| Official charts only | `not Custom()` |
| Folder customs | `Custom() and not Packed()` |
| Has hidden or touhou| `Hidden() or Touhou()` |
| Never played (any diff) | `Unplayed()` |
| Never played on top chart | `Unplayed('?')` |
| FC on top chart | `FullCombo('?')` |
| Streamer-safe only | `Streamer()` |
| Recently added | `Modified('7d')` |
| Text search | `Any('text')` |
| High intensity | `BPM('180+') and Length("120+")` |
| Challenge | `Difficulty('11+', '?') and Unplayed('?')` |
| Non-website customs | `Custom() and not Online()` |
| Favorites, shuffled | `Sort(ByRandom()) and Favorite()` |
| Oldest customs | `Sort(ByModified()) and Custom()` |

Final Pro-Tip: If you find yourself using one of these constantly, consider adding it to your Expressions in IronSearch.cfg config so you can just type that instead!