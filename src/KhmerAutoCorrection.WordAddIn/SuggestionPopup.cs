using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace KhmerAutoCorrection.WordAddIn
{
    /// <summary>
    /// A borderless WPF popup window that displays spelling suggestions.
    /// Designed to not steal focus from Word while allowing keyboard navigation.
    /// </summary>
    public partial class SuggestionPopup : Window
    {
        private ListBox? _suggestionList;
        private List<string> _suggestions;
        private int _selectedIndex;
        private bool _isProcessingSelection;

        public event Action<string>? SuggestionSelected;

        public SuggestionPopup()
        {
            _suggestions = new List<string>();
            _selectedIndex = 0;

            // Configure window as a tool window (no taskbar entry, stays on top)
            Width = 250;
            Height = 150;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            Topmost = true;
            Focusable = false;
            IsHitTestVisible = true;

            // Create content
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Child = CreateContent()
            };

            Content = border;

            // Prevent stealing focus
            Deactivated += (s, e) => 
            {
                if (!_isProcessingSelection)
                    Hide();
            };
        }

        private UIElement CreateContent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header showing the misspelled word
            var headerText = new TextBlock
            {
                Text = "Spelling Suggestions",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(8, 6, 8, 4),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(240, 240, 240))
            };
            Grid.SetRow(headerText, 0);

            // Suggestions list
            _suggestionList = new ListBox
            {
                BorderThickness = new Thickness(0),
                Padding = new Thickness(4),
                SelectionMode = SelectionMode.Single
            };
            _suggestionList.MouseDoubleClick += SuggestionList_MouseDoubleClick;
            _suggestionList.KeyDown += SuggestionList_KeyDown;
            Grid.SetRow(_suggestionList, 1);

            grid.Children.Add(headerText);
            grid.Children.Add(_suggestionList);

            return grid;
        }

        /// <summary>
        /// Shows the popup with suggestions at the specified screen coordinates.
        /// </summary>
        public void ShowSuggestions(string misspelledWord, IReadOnlyList<string> suggestions, int screenX, int screenY)
        {
            _suggestions = new List<string>(suggestions);
            _selectedIndex = 0;

            // Update header
            var headerText = ((Grid)Content).Children[0] as TextBlock;
            if (headerText != null)
                headerText.Text = $"Suggestions for: {misspelledWord}";

            // Populate list
            if (_suggestionList != null)
            {
                _suggestionList.Items.Clear();
                foreach (var suggestion in _suggestions)
                {
                    _suggestionList.Items.Add(suggestion);
                }

                if (_suggestions.Count > 0)
                    _suggestionList.SelectedIndex = 0;
            }

            // Position popup
            Left = screenX;
            Top = screenY;

            // Convert to device-independent pixels (WPF units)
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                var source = System.Windows.Interop.HwndSource.FromHwnd(hwnd);
                if (source != null)
                {
                    double dpiX = source.CompositionTarget.TransformToDevice.M11;
                    double dpiY = source.CompositionTarget.TransformToDevice.M22;
                    
                    Left = screenX / dpiX;
                    Top = screenY / dpiY;
                }
            }

            Show();
            
            // Ensure it doesn't take focus
            Activate();
            var helper = new WindowInteropHelper(this);
            int extendedStyle = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int style);

        public bool IsPopupVisible => Visibility == Visibility.Visible;

        private void SuggestionList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_suggestionList?.SelectedItem is string selected)
            {
                AcceptSuggestion(selected);
            }
        }

        private void SuggestionList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                e.Handled = true;
                if (_suggestionList?.SelectedItem is string selected)
                {
                    AcceptSuggestion(selected);
                }
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Hide();
            }
        }

        public void SelectNextSuggestion()
        {
            if (_suggestions.Count == 0 || _suggestionList == null) return;

            _selectedIndex = (_selectedIndex + 1) % _suggestions.Count;
            _suggestionList.SelectedIndex = _selectedIndex;
        }

        public void SelectPreviousSuggestion()
        {
            if (_suggestions.Count == 0 || _suggestionList == null) return;

            _selectedIndex = (_selectedIndex - 1 + _suggestions.Count) % _suggestions.Count;
            _suggestionList.SelectedIndex = _selectedIndex;
        }

        public void AcceptSelectedSuggestion()
        {
            if (_suggestionList?.SelectedItem is string selected)
            {
                AcceptSuggestion(selected);
            }
        }

        private void AcceptSuggestion(string suggestion)
        {
            _isProcessingSelection = true;
            try
            {
                SuggestionSelected?.Invoke(suggestion);
                Hide();
            }
            finally
            {
                _isProcessingSelection = false;
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            // Don't hide immediately - let the parent handle it
        }
    }
}
