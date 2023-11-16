// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.ComponentModel;
using System.Runtime.CompilerServices;
using AssetManagement.Geometry.Strategies.Fbx.ExportDIalog.Wiews;
using CommonControls.Common;
using CommonControls.Interfaces.AssetManagement;

namespace AssetManagment.Geometry.Strategies.Fbx.ExportDialog.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExportDialogViewModel : ViewModelBase
    {
        public NotifyAttr<string> TestText { get; set; } = new NotifyAttr<string>();

        /// Static helper, essentially taken from "PinToolViewModel"
        /// </summary>        
        public static bool ShowImportDialog(AssetManagerData inputData)
        {
            var dialog = new ExportDialogView();
            var modelView = new ExportDialogViewModel();
            dialog.DataContext = modelView;
            var result = dialog.ShowDialog();

            return result.Value;
        }
    }
}


//    public class ExportIExport
//    {
//        public NotifyAttr<string> TestText { get; set; } = new NotifyAttr<string>();

//        /// Static helper, essentially taken from "PinToolViewModel"
//        ///        /// </summary>
//        ///        public static bool ShowImportDialog(AssetManagerData inputData)
//        ///        {
//        ///            var dialog = new ExportSettingsDialogView();
//        ///            var modelView = new ExportSettingsDialogViewModel();
//        ///            dialog.DataContext = modelView;
//        ///
//        ///            var result = dialog.ShowDialog().Value;
//        ///            return result;
//        ///        }
//        ///    }
//    }
//}

