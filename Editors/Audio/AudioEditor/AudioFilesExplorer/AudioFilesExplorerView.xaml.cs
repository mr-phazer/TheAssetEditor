﻿using System.Windows;
using System.Windows.Controls;

namespace Editors.Audio.AudioEditor.AudioFilesExplorer
{
    public partial class AudioFilesExplorerView : UserControl
    {
        public AudioFilesExplorerView()
        {
            InitializeComponent();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            FilterTextBoxItem.Focus();
        }
    }
}
