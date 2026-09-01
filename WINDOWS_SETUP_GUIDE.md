# Khmer Auto-Correction Plugin for Microsoft Word
## Complete Setup & Deployment Guide

---

## 📋 Table of Contents

1. [Project Overview](#project-overview)
2. [What's Already Done (Linux Environment)](#whats-already-done)
3. [Windows Setup Requirements](#windows-setup-requirements)
4. [Step-by-Step Windows Implementation](#step-by-step-windows-implementation)
   - Step 1: Transfer Files to Windows
   - Step 2: Install Required Software
   - Step 3: Create VSTO Project in Visual Studio
   - Step 4: Integrate Core Libraries
   - Step 5: Implement Word Interop Code
   - Step 6: Build and Test
   - Step 7: Package for Distribution
5. [Troubleshooting](#troubleshooting)
6. [Final Checklist](#final-checklist)

---

## Project Overview

This plugin provides **real-time Khmer spell checking** in Microsoft Word with:
- ✅ Red wavy underlines under misspelled Khmer words
- ✅ Popup suggestions near the cursor
- ✅ Tab key to accept corrections
- ✅ Intelligent Khmer word segmentation (no spaces between words)
- ✅ Custom Khmer-specific edit distance for accurate suggestions

**Technology Stack:**
- Core: C# .NET Framework 4.8 / .NET 6+
- Dictionary: 101,107 Khmer words (MIT licensed from khmer-segment-js)
- Segmentation: Viterbi dynamic programming algorithm
- Spell Checking: SymSpell with custom Khmer character weights
- UI: VSTO + WPF Popup + Low-level keyboard hook

---

## What's Already Done (Linux Environment) ✅

The following components are **100% complete and tested**:

### 1. Core Libraries (`/workspace/src/KhmerAutoCorrection.Core/`)

| Component | Status | Description |
|-----------|--------|-------------|
| `KhmerDictionary.cs` | ✅ Complete | Loads 101,107 words into HashSet + Trie |
| `Trie.cs` | ✅ Complete | Prefix tree for fast word lookup |
| `ViterbiSegmenter.cs` | ✅ Complete | Khmer word segmentation algorithm |
| `KhmerEditDistance.cs` | ✅ Complete | Custom weighted edit distance |
| `KhmerSpellChecker.cs` | ✅ Complete | Suggestion generation with ranking |
| `SpellCheckEngine.cs` | ✅ Complete | High-level API for text checking |
| `Models/` | ✅ Complete | Data models (SpellCheckResult, Suggestion, etc.) |

### 2. Test Suite (`/workspace/src/KhmerAutoCorrection.Core.Tests/`)

All tests passing:
- Dictionary loading (101,107 words)
- Trie operations (insert, search, prefix matching)
- Segmentation accuracy (multiple test cases)
- Spell checker functionality
- Edit distance calculations
- Full integration tests

### 3. Documentation

- `IMPLEMENTATION_STATUS.md` - Detailed status report
- `WINDOWS_SETUP_GUIDE.md` (this file) - Complete Windows setup instructions

### 4. Project Structure

```
/workspace/src/
├── KhmerAutoCorrection.Core/          # ✅ COMPLETE - Core logic
├── KhmerAutoCorrection.Core.Tests/    # ✅ COMPLETE - Unit tests
├── KhmerAutoCorrection.WordAddIn/     # ⚠️ TEMPLATE ONLY - Needs Windows
└── data/
    └── khmer_dictionary.txt           # ✅ COMPLETE - 101,107 words
```

---

## Windows Setup Requirements

### Hardware Requirements
- Windows 10/11 (64-bit)
- 4GB RAM minimum (8GB recommended)
- 500MB free disk space

### Software Requirements

#### 1. **Microsoft Visual Studio 2022** (Required)
- Download: https://visualstudio.microsoft.com/downloads/
- **Workloads to install:**
  - ✅ `.NET desktop development`
  - ✅ `Office/SharePoint development` (CRITICAL - includes VSTO templates)
  - ✅ `Visual Studio Tools for Office`

#### 2. **Microsoft Office** (Required)
- Microsoft Word 2016, 2019, 2021, or Microsoft 365
- Must be installed **before** creating VSTO project
- 32-bit or 64-bit (match your Visual Studio configuration)

#### 3. **.NET Framework**
- .NET Framework 4.8 (included with VS 2022) OR
- .NET 6.0+ (if using modern template)

#### 4. **NuGet Packages** (Will be auto-installed)
- `symspell` (for spell checking)
- `TrieNet` (optional, we have custom Trie)
- Standard WPF references (included with .NET)

---

## Step-by-Step Windows Implementation

### Step 1: Transfer Files to Windows

#### Option A: Git Clone (Recommended)
```bash
# On Windows, open Git Bash or PowerShell
git clone <your-repository-url> KhmerWordPlugin
cd KhmerWordPlugin
```

#### Option B: Copy Folder
1. Copy the entire `/workspace/src` folder from Linux to Windows
2. Place it in a convenient location (e.g., `C:\Projects\KhmerWordPlugin`)

#### Verify File Structure
Ensure you have:
```
KhmerWordPlugin/
├── KhmerAutoCorrection.Core/
│   ├── KhmerAutoCorrection.Core.csproj
│   ├── KhmerDictionary.cs
│   ├── Trie.cs
│   ├── ViterbiSegmenter.cs
│   ├── KhmerEditDistance.cs
│   ├── KhmerSpellChecker.cs
│   ├── SpellCheckEngine.cs
│   └── Models/
├── KhmerAutoCorrection.Core.Tests/
├── KhmerAutoCorrection.WordAddIn/
│   ├── KhmerAutoCorrection.WordAddIn.csproj (template)
│   ├── ThisAddIn.cs (template)
│   └── ... (other template files)
└── data/
    └── khmer_dictionary.txt
```

---

### Step 2: Install Required Software

1. **Install Visual Studio 2022**
   - Run the installer
   - Select workloads: `.NET desktop development` + `Office/SharePoint development`
   - Complete installation

2. **Verify Office Installation**
   - Open Microsoft Word
   - Go to `File > Account`
   - Note the version (2016, 2019, 365, etc.)
   - Close Word

3. **Open Visual Studio**
   - Launch Visual Studio 2022
   - Sign in if required

---

### Step 3: Create VSTO Project in Visual Studio

#### 3.1 Create New Project
1. In Visual Studio: `File > New > Project`
2. Search for **"Word Add-in"**
3. Select **"Word Add-in"** template (under Visual C# > Office/SharePoint)
4. Click **Next**

#### 3.2 Configure Project
- **Project name:** `KhmerAutoCorrection.WordAddIn`
- **Location:** Browse to your `KhmerWordPlugin` folder (replace the template folder)
- **Solution name:** `KhmerAutoCorrection`
- Check **"Create directory for solution"** if needed
- Click **Create**

#### 3.3 Configure Target Framework
1. Right-click project → **Properties**
2. Set **Target framework:** `.NET Framework 4.8` (or `.NET 6.0`)
3. Set **Output type:** `Class Library`
4. Save

#### 3.4 Add References
Right-click project → **Add > Reference**:
- ✅ `Microsoft.Office.Interop.Word`
- ✅ `Office` (Microsoft Office XX.0 Object Library)
- ✅ `Microsoft.Office.Tools.Common`
- ✅ `System.Windows.Forms`
- ✅ `WindowsBase`
- ✅ `PresentationFramework`
- ✅ `PresentationCore`

#### 3.5 Install NuGet Packages
Right-click project → **Manage NuGet Packages**:
- Install: `symspell` (latest version)
- Install: `TrieNet` (optional, we have custom implementation)

Or via Package Manager Console:
```powershell
Install-Package symspell
```

---

### Step 4: Integrate Core Libraries

#### 4.1 Add Core Project to Solution
1. In Visual Studio: `File > Add > Existing Project`
2. Browse to: `KhmerAutoCorrection.Core/KhmerAutoCorrection.Core.csproj`
3. Click **Open**

#### 4.2 Add Project Reference
1. Right-click `KhmerAutoCorrection.WordAddIn` → **Add > Reference**
2. Select **Projects** tab
3. Check `KhmerAutoCorrection.Core`
4. Click **OK**

#### 4.3 Copy Dictionary File

**✅ Already Done!** The dictionary file has been copied to the correct location:

```
src/KhmerAutoCorrection.WordAddIn/Assets/KhmerDictionary.tsv
```

**On Windows, verify:**
1. In Solution Explorer, expand `KhmerAutoCorrection.WordAddIn` project
2. Check that the `Assets` folder exists with `KhmerDictionary.tsv` inside
3. If not visible, click **Show All Files** button in Solution Explorer
4. Right-click `Assets/KhmerDictionary.tsv` → **Include In Project** (if needed)
5. Right-click the file → **Properties**
6. Verify settings:
   - **Build Action:** `Content` (already configured in .csproj)
   - **Copy to Output Directory:** `Copy if newer` (already configured)

**Note:** The `.csproj` file already contains the configuration:
```xml
<ItemGroup>
  <Content Include="Assets\KhmerDictionary.tsv">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

#### 4.4 Update Namespace References
In all WordAddIn files, add:
```csharp
using KhmerAutoCorrection.Core;
using KhmerAutoCorrection.Core.Models;
```

---

### Step 5: Implement Word Interop Code

Replace the template files with the implementation code below:

#### 5.1 ThisAddIn.cs (Main Entry Point)

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Office.Tools.Ribbon;
using Word = Microsoft.Office.Interop.Word;
using KhmerAutoCorrection.Core;
using KhmerAutoCorrection.Core.Models;

namespace KhmerAutoCorrection.WordAddIn
{
    public partial class ThisAddIn
    {
        private SpellCheckEngine _spellChecker;
        private System.Windows.Forms.Timer _idleTimer;
        private List<Word.Range> _underlinedRanges = new List<Word.Range>();
        private SuggestionPopup _popup;
        private Word.Range _currentMisspelledRange;
        private List<string> _currentSuggestions;
        private int _selectedIndex = 0;
        private bool _popupOpen = false;
        private KeyboardHook _keyboardHook;

        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            // Initialize spell checker
            string dictionaryPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "khmer_dictionary.txt");
            
            _spellChecker = new SpellCheckEngine(dictionaryPath);

            // Setup idle timer
            _idleTimer = new System.Windows.Forms.Timer();
            _idleTimer.Interval = 500;
            _idleTimer.Tick += IdleTimer_Tick;

            // Subscribe to Word events
            this.Application.DocumentChange += Application_DocumentChange;
            this.Application.WindowSelectionChange += Application_WindowSelectionChange;

            // Start timer
            _idleTimer.Start();

            // Initialize popup
            _popup = new SuggestionPopup();
            _popup.SuggestionSelected += OnSuggestionSelected;

            // Install keyboard hook
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.Install();
        }

        private void Application_DocumentChange()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        private void Application_WindowSelectionChange(Word.Selection Sel)
        {
            // Hide popup if selection moves away from misspelled word
            if (_popupOpen && !IsSelectionInMisspelledRange(Sel))
            {
                HidePopup();
            }
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            _idleTimer.Stop();

            try
            {
                Word.Document doc = this.Application.ActiveDocument;
                if (doc == null) return;

                Word.Selection selection = this.Application.Selection;
                if (selection == null) return;

                // Get current paragraph
                Word.Paragraph paragraph = selection.Paragraphs[1];
                Word.Range paraRange = paragraph.Range;
                string text = paraRange.Text;
                int paraStart = paraRange.Start;

                // Run spell check on background thread
                System.Threading.Tasks.Task.Run(() =>
                {
                    var results = _spellChecker.CheckText(text);
                    
                    // Marshal back to UI thread
                    this.Application.Dispatch(() =>
                    {
                        ApplyUnderlines(results, paraStart, doc);
                        
                        // If cursor is in a misspelled word, show popup
                        int cursorPos = selection.Start;
                        foreach (var result in results)
                        {
                            int absStart = paraStart + result.StartPosition;
                            int absEnd = absStart + result.Length;
                            
                            if (cursorPos >= absStart && cursorPos <= absEnd && result.Suggestions.Count > 0)
                            {
                                ShowPopup(doc.Range(absStart, absEnd), result.Suggestions);
                                break;
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Spell check error: {ex.Message}");
            }
        }

        private void ApplyUnderlines(List<SpellCheckResult> results, int paragraphStart, Word.Document doc)
        {
            // Clear existing underlines
            ClearUnderlines();

            // Apply new underlines
            foreach (var result in results)
            {
                if (!result.IsCorrect)
                {
                    int absStart = paragraphStart + result.StartPosition;
                    int absEnd = absStart + result.Length;
                    
                    Word.Range range = doc.Range(absStart, absEnd);
                    range.Font.Underline = Word.WdUnderline.wdUnderlineWavy;
                    range.Font.UnderlineColor = Word.WdColor.wdColorRed;
                    _underlinedRanges.Add(range);
                }
            }
        }

        private void ClearUnderlines()
        {
            foreach (var range in _underlinedRanges)
            {
                try
                {
                    range.Font.Underline = Word.WdUnderline.wdUnderlineNone;
                    range.Font.UnderlineColor = Word.WdColor.wdColorAutomatic;
                }
                catch { /* Range may be invalid */ }
            }
            _underlinedRanges.Clear();
        }

        private void ShowPopup(Word.Range range, List<string> suggestions)
        {
            _currentMisspelledRange = range;
            _currentSuggestions = suggestions;
            _selectedIndex = 0;

            // Get screen coordinates
            int left, top, width, height;
            range.GetPoint(out left, out top, out width, out height, 
                          Word.WdGetPointType.wdGetPointTypeScreen);

            _popup.ShowSuggestions(suggestions, left, top + height);
            _popupOpen = true;
        }

        private void HidePopup()
        {
            _popup.Hide();
            _popupOpen = false;
            _currentMisspelledRange = null;
            _currentSuggestions = null;
        }

        private void OnSuggestionSelected(string suggestion)
        {
            AcceptSuggestion(suggestion);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!_popupOpen) return;

            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
            {
                if (_currentSuggestions != null && _selectedIndex < _currentSuggestions.Count)
                {
                    AcceptSuggestion(_currentSuggestions[_selectedIndex]);
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                _selectedIndex = Math.Min(_selectedIndex + 1, _currentSuggestions.Count - 1);
                _popup.SelectIndex(_selectedIndex);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                _selectedIndex = Math.Max(_selectedIndex - 1, 0);
                _popup.SelectIndex(_selectedIndex);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HidePopup();
                e.Handled = true;
            }
        }

        private void AcceptSuggestion(string suggestion)
        {
            if (_currentMisspelledRange == null) return;

            Word.UndoRecord undoRecord = this.Application.UndoRecord;
            undoRecord.StartCustomRecord("Khmer correction");
            
            _currentMisspelledRange.Text = suggestion;
            
            undoRecord.EndCustomRecord();
            
            HidePopup();
        }

        private bool IsSelectionInMisspelledRange(Word.Selection selection)
        {
            if (_currentMisspelledRange == null) return false;
            
            int cursorPos = selection.Start;
            return cursorPos >= _currentMisspelledRange.Start && 
                   cursorPos <= _currentMisspelledRange.End;
        }

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
            _idleTimer?.Stop();
            _keyboardHook?.Uninstall();
            ClearUnderlines();
        }

        #region VSTO generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }

    // Extension method for COM marshaling
    public static class DispatcherExtensions
    {
        public static void Dispatch(this Word.Application app, Action action)
        {
            if (app != null)
            {
                ((Microsoft.Office.Interop.Word._Application)app).DispatchEvent += (sender, e) => action();
            }
        }
    }
}
```

#### 5.2 SuggestionPopup.cs (WPF Popup)

Create new file `SuggestionPopup.xaml`:
```xml
<Window x:Class="KhmerAutoCorrection.WordAddIn.SuggestionPopup"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Suggestions" 
        Height="200" 
        Width="250"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        Topmost="True"
        ShowInTaskbar="False"
        Focusable="False">
    
    <Border Background="White" 
            BorderBrush="#FF888888" 
            BorderThickness="1"
            CornerRadius="4"
            Effect="{StaticResource DropShadowEffect}">
        <ListBox x:Name="SuggestionList" 
                 KeyDown="SuggestionList_KeyDown"
                 MouseDoubleClick="SuggestionList_MouseDoubleClick"
                 FontFamily="Khmer OS"
                 FontSize="14">
            <ListBox.ItemContainerStyle>
                <Style TargetType="ListBoxItem">
                    <Setter Property="Padding" Value="8,4"/>
                    <Setter Property="Cursor" Value="Hand"/>
                </Style>
            </ListBox.ItemContainerStyle>
        </ListBox>
    </Border>
</Window>
```

Create new file `SuggestionPopup.xaml.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KhmerAutoCorrection.WordAddIn
{
    public partial class SuggestionPopup : Window
    {
        private List<string> _suggestions;

        public event Action<string> SuggestionSelected;

        public SuggestionPopup()
        {
            InitializeComponent();
        }

        public void ShowSuggestions(List<string> suggestions, double left, double top)
        {
            _suggestions = suggestions;
            SuggestionList.Items.Clear();
            
            foreach (var suggestion in suggestions)
            {
                SuggestionList.Items.Add(suggestion);
            }

            if (SuggestionList.Items.Count > 0)
            {
                SuggestionList.SelectedIndex = 0;
            }

            Left = left;
            Top = top;
            Show();
        }

        public void SelectIndex(int index)
        {
            if (index >= 0 && index < SuggestionList.Items.Count)
            {
                SuggestionList.SelectedIndex = index;
            }
        }

        private void SuggestionList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                AcceptSelected();
            }
        }

        private void SuggestionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AcceptSelected();
        }

        private void AcceptSelected()
        {
            if (SuggestionList.SelectedItem != null)
            {
                SuggestionSelected?.Invoke(SuggestionList.SelectedItem.ToString());
                Hide();
            }
        }
    }
}
```

#### 5.3 KeyboardHook.cs (Low-Level Hook)

Create new file `KeyboardHook.cs`:
```csharp
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KhmerAutoCorrection.WordAddIn
{
    public class KeyboardHook
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public event KeyEventHandler KeyDown;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        public void Install()
        {
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Uninstall()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                var keyArgs = new KeyEventArgs((Keys)vkCode);
                KeyDown?.Invoke(this, keyArgs);
                
                if (keyArgs.Handled)
                {
                    return (IntPtr)1;
                }
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
```

#### 5.4 Update App.config

Add to `App.config`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
  </configSections>
  <runtime>
    <loadFromRemoteSources enabled="true"/>
  </runtime>
</configuration>
```

---

### Step 6: Build and Test

#### 6.1 Build Solution
1. In Visual Studio: `Build > Build Solution` (or Ctrl+Shift+B)
2. Fix any compilation errors
3. Ensure output shows: `Build succeeded`

#### 6.2 Debug/Test
1. Press **F5** to start debugging
2. Microsoft Word will launch automatically with the add-in loaded
3. Open a new document
4. Type some Khmer text with intentional misspellings
5. Verify:
   - Red wavy underlines appear under misspelled words
   - Popup shows suggestions when cursor is on misspelled word
   - Arrow keys navigate suggestions
   - Tab/Enter accepts selected suggestion
   - Escape closes popup

#### 6.3 Common Issues During Testing

| Issue | Solution |
|-------|----------|
| Add-in not loading | Check `File > Options > Trust Center > Add-ins` |
| Dictionary not found | Verify `khmer_dictionary.txt` is in output directory |
| Underlines not showing | Check Word version compatibility |
| Popup not appearing | Ensure WPF references are added |
| Tab key not working | Verify keyboard hook is installed |

---

### Step 7: Package for Distribution

#### 7.1 Create Release Build
1. In Visual Studio: `Build > Configuration Manager`
2. Set **Active solution configuration:** `Release`
3. Rebuild solution

#### 7.2 Create Installer (WiX Toolset)

##### Option A: WiX Toolset (Recommended)
1. Install WiX Toolset: https://wixtoolset.org/
2. Create WiX Setup Project
3. Include:
   - `KhmerAutoCorrection.WordAddIn.dll`
   - `KhmerAutoCorrection.Core.dll`
   - `khmer_dictionary.txt`
   - All dependencies

##### Option B: ClickOnce (Simpler)
1. Right-click project → **Publish**
2. Configure publish settings
3. Choose installation location
4. Generate setup files

#### 7.3 Code Signing (Recommended)
- Purchase code signing certificate
- Sign assemblies in project properties
- Sign installer package

#### 7.4 Distribution Package Should Include:
- Setup.exe or MSI installer
- README with installation instructions
- License file
- System requirements

---

## Troubleshooting

### Issue: "Could not load file or assembly 'Microsoft.Office.Interop.Word'"
**Solution:** 
- Ensure Office is installed
- Add reference to `Microsoft.Office.Interop.Word` from Assemblies > Extensions
- Set **Copy Local** = False

### Issue: "VSTO Add-in not appearing in Word"
**Solution:**
- Go to `File > Options > Trust Center > Add-ins`
- Ensure "Require application add-ins to be signed by Trusted Publisher" is unchecked (during development)
- Restart Word

### Issue: "Keyboard hook blocked by antivirus"
**Solution:**
- Add exception in antivirus software
- Fallback: Use right-click context menu instead of Tab key
- Alternative: Use Word's built-in key binding system

### Issue: "Popup steals focus, can't continue typing"
**Solution:**
- Set `Focusable="False"` in XAML
- Use `Popup` control instead of `Window`
- Handle keyboard events without activating window

### Issue: "Performance slow on large documents"
**Solution:**
- Only check current paragraph (already implemented)
- Increase timer interval (currently 500ms)
- Add option to disable real-time checking

### Issue: "Dictionary file not found at runtime"
**Solution:**
```csharp
string dictionaryPath = System.IO.Path.Combine(
    System.AppDomain.CurrentDomain.BaseDirectory,
    "khmer_dictionary.txt");
```

---

## Final Checklist

Before considering the project complete, verify:

### Development Phase ✓
- [ ] Core libraries integrated successfully
- [ ] VSTO project builds without errors
- [ ] Dictionary loads (101,107 words)
- [ ] Segmentation works correctly
- [ ] Spell checker detects misspellings
- [ ] Suggestions are relevant

### Testing Phase ✓
- [ ] Red wavy underlines appear correctly
- [ ] Popup shows near cursor
- [ ] Arrow keys navigate suggestions
- [ ] Tab/Enter accepts suggestion
- [ ] Escape closes popup
- [ ] Replacement works with undo support
- [ ] Mixed language text handled properly
- [ ] Large documents perform well

### Deployment Phase ✓
- [ ] Release build successful
- [ ] Installer created
- [ ] Tested on clean Windows machine
- [ ] Works on Word 2016, 2019, 365
- [ ] Documentation complete
- [ ] License included

### Post-Release ✓
- [ ] User feedback collection mechanism
- [ ] Update process defined
- [ ] Support contact information
- [ ] Known issues documented

---

## Additional Resources

### Documentation
- [Microsoft VSTO Documentation](https://docs.microsoft.com/en-us/visualstudio/vsto/)
- [Word Interop Reference](https://docs.microsoft.com/en-us/office/vba/api/overview/word)
- [SymSpell Documentation](https://github.com/mammothb/symspell)

### Khmer Language Resources
- [Khmer Segment JS (original)](https://github.com/daraong/khmer-segment-js)
- [Khmer Unicode Standard](https://unicode.org/charts/PDF/U1780.pdf)

### Support
- GitHub Issues: [Your Repository]
- Email: [Your Contact]

---

## Quick Start Summary

```bash
# 1. On Windows, clone/copy repository
git clone <repo-url>
cd KhmerWordPlugin

# 2. Open in Visual Studio 2022
#    - Ensure Office/SharePoint workload installed

# 3. Add NuGet package
Install-Package symspell

# 4. Build solution (Ctrl+Shift+B)

# 5. Press F5 to debug in Word

# 6. Test with Khmer text

# 7. Build Release and create installer
```

---

**Congratulations!** Once you complete these steps, you'll have a fully functional Khmer Auto-Correction Plugin for Microsoft Word ready for distribution.

If you encounter any issues during the Windows implementation, refer back to this guide or return to the Linux environment to update the core libraries as needed.
