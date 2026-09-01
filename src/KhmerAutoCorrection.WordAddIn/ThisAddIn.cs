using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace KhmerAutoCorrection.WordAddIn
{
    public partial class ThisAddIn
    {
        private SpellCheckEngine? _spellCheckEngine;
        private Core.KhmerDictionary? _dictionary;
        private System.Windows.Forms.Timer? _idleTimer;
        private List<Word.Range>? _underlinedRanges;
        private SuggestionPopup? _popup;
        private SpellingError? _currentError;
        private bool _isProcessing;
        private const int IDLE_INTERVAL_MS = 500;

        // Keyboard hook constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc? _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        private void ThisAddIn_Startup(object sender, EventArgs e)
        {
            try
            {
                InitializeSpellChecker();
                SetupEventHandlers();
                InstallKeyboardHook();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to start Khmer Auto-Correction: {ex.Message}\n\nThe add-in will be disabled.",
                    "Khmer Auto-Correction Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeSpellChecker()
        {
            string dictionaryPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ThisAddIn).Assembly.Location) ?? 
                Environment.CurrentDirectory,
                "khmer_dictionary.txt");

            if (!System.IO.File.Exists(dictionaryPath))
            {
                // Try alternative locations
                dictionaryPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KhmerAutoCorrection",
                    "khmer_dictionary.txt");
            }

            if (!System.IO.File.Exists(dictionaryPath))
            {
                throw new Exception(
                    $"Dictionary file not found at: {dictionaryPath}\n" +
                    "Please ensure the khmer_dictionary.txt file is installed with the add-in.");
            }

            _dictionary = new KhmerDictionary();
            _dictionary.LoadFromFile(dictionaryPath);

            _spellCheckEngine = new SpellCheckEngine(_dictionary, maxEditDistance: 3, maxSuggestions: 10);
            _underlinedRanges = new List<Word.Range>();
        }

        private void SetupEventHandlers()
        {
            this.Application.DocumentChange += Application_DocumentChange;
            this.Application.WindowSelectionChange += Application_WindowSelectionChange;

            _idleTimer = new System.Windows.Forms.Timer();
            _idleTimer.Interval = IDLE_INTERVAL_MS;
            _idleTimer.Tick += IdleTimer_Tick;
        }

        private void Application_DocumentChange()
        {
            if (_isProcessing || _spellCheckEngine == null) return;

            _idleTimer?.Stop();
            _idleTimer?.Start();
        }

        private void Application_WindowSelectionChange(Word.Selection sel)
        {
            // Optional: Hide popup if selection moves away from misspelled word
            if (_popup != null && _currentError != null)
            {
                var currentRange = sel.Range;
                int errorStart = _currentError.Start;
                int errorEnd = _currentError.Start + _currentError.Length;

                if (currentRange.Start < errorStart || currentRange.End > errorEnd)
                {
                    HidePopup();
                }
            }
        }

        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            if (_idleTimer != null)
                _idleTimer.Stop();

            PerformSpellCheck();
        }

        private void PerformSpellCheck()
        {
            if (_isProcessing || _spellCheckEngine == null || this.Application.ActiveDocument == null)
                return;

            _isProcessing = true;

            try
            {
                // Clear existing underlines
                ClearUnderlines();

                // Get current paragraph
                Word.Selection selection = this.Application.Selection;
                if (selection == null) return;

                Word.Paragraph? paragraph = selection.Paragraphs.Count > 0 
                    ? selection.Paragraphs[1] 
                    : null;

                if (paragraph == null) return;

                Word.Range paraRange = paragraph.Range;
                string text = paraRange.Text;

                // Remove paragraph mark for processing
                if (text.EndsWith("\r"))
                    text = text.Substring(0, text.Length - 1);

                int paraStart = paraRange.Start;

                // Check for spelling errors
                var errors = _spellCheckEngine.CheckText(text);

                // Apply underlines for each error
                foreach (var error in errors)
                {
                    int absStart = paraStart + error.Start;
                    int absEnd = absStart + error.Length;

                    // Ensure range is valid
                    if (absStart >= 0 && absEnd <= paraRange.End && absStart < absEnd)
                    {
                        Word.Range wordRange = this.Application.ActiveDocument.Range(absStart, absEnd);
                        ApplyUnderline(wordRange);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Spell check error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private void ApplyUnderline(Word.Range range)
        {
            try
            {
                range.Font.Underline = Word.WdUnderline.wdUnderlineWavy;
                range.Font.UnderlineColor = Word.WdColor.wdColorRed;
                _underlinedRanges?.Add(range);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply underline: {ex.Message}");
            }
        }

        private void ClearUnderlines()
        {
            if (_underlinedRanges == null) return;

            try
            {
                foreach (var r in _underlinedRanges)
                {
                    try
                    {
                        r.Font.Underline = Word.WdUnderline.wdUnderlineNone;
                        r.Font.UnderlineColor = Word.WdColor.wdColorAutomatic;
                        Marshal.ReleaseComObject(r);
                    }
                    catch { }
                }
                _underlinedRanges.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to clear underlines: {ex.Message}");
            }
        }

        private void ShowPopup(SpellingError error, Word.Range range)
        {
            _currentError = error;

            if (_popup == null)
            {
                _popup = new SuggestionPopup();
                _popup.SuggestionSelected += OnSuggestionSelected;
            }

            // Get screen coordinates
            int left = 0, top = 0, width = 0, height = 0;
            
            try
            {
                range.GetPoint(out left, out top, out width, out height, 
                    Word.WdGetPointType.wdGetPointTypeScreen);
            }
            catch
            {
                // Fallback to cursor position
                left = this.Application.PointsToPixels(selection.Left);
                top = this.Application.PointsToPixels(selection.Top);
            }

            _popup.ShowSuggestions(error.Word, error.Suggestions, left, top + height);
        }

        private void HidePopup()
        {
            if (_popup != null)
            {
                _popup.Hide();
                _currentError = null;
            }
        }

        private void OnSuggestionSelected(string suggestion)
        {
            if (_currentError == null || this.Application.ActiveDocument == null) return;

            Word.Selection selection = this.Application.Selection;
            Word.Range paraRange = selection.Paragraphs[1].Range;
            int paraStart = paraRange.Start;

            int absStart = paraStart + _currentError.Start;
            int absEnd = absStart + _currentError.Length;

            Word.Range errorRange = this.Application.ActiveDocument.Range(absStart, absEnd);

            // Replace with undo support
            Word.UndoRecord? undoRecord = this.Application.UndoRecord;
            object customMark = "Khmer correction";
            
            try
            {
                undoRecord?.StartCustomRecord(ref customMark);
                errorRange.Text = suggestion;
                undoRecord?.EndCustomRecord();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to replace word: {ex.Message}");
            }

            HidePopup();
            _idleTimer?.Stop();
            _idleTimer?.Start(); // Re-check after correction
        }

        private void InstallKeyboardHook()
        {
            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                _hookID = SetHook(_proc);
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule? curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // Check if popup is open and handle Tab/Enter
                if (_popupInstance != null && _popupInstance.IsPopupVisible)
                {
                    if (vkCode == (int)Keys.Tab || vkCode == (int)Keys.Enter)
                    {
                        _popupInstance.AcceptSelectedSuggestion();
                        return (IntPtr)1; // Swallow the key
                    }
                    else if (vkCode == (int)Keys.Down)
                    {
                        _popupInstance.SelectNextSuggestion();
                        return (IntPtr)1;
                    }
                    else if (vkCode == (int)Keys.Up)
                    {
                        _popupInstance.SelectPreviousSuggestion();
                        return (IntPtr)1;
                    }
                    else if (vkCode == (int)Keys.Escape)
                    {
                        _popupInstance.Hide();
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static SuggestionPopup? _popupInstance;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private void ThisAddIn_Shutdown(object sender, EventArgs e)
        {
            // Cleanup
            _idleTimer?.Stop();
            _idleTimer?.Dispose();

            if (_hookID != IntPtr.Zero)
                UnhookWindowsHookEx(_hookID);

            _popup?.Close();

            ClearUnderlines();

            _spellCheckEngine?.Dispose();

            // Release COM objects
            if (_underlinedRanges != null)
            {
                foreach (var r in _underlinedRanges)
                {
                    try { Marshal.ReleaseComObject(r); } catch { }
                }
            }

            this.Application.DocumentChange -= Application_DocumentChange;
            this.Application.WindowSelectionChange -= Application_WindowSelectionChange;
        }

        #region VSTO generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new EventHandler(ThisAddIn_Startup);
            this.Shutdown += new EventHandler(ThisAddIn_Shutdown);
        }
        #endregion
    }
}
