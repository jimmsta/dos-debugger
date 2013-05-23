using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Disassembler;
using Disassembler2;
using Disassembler2.Omf;
using AvalonDock.Layout;

namespace WpfDebugger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Try load the last layout.
            try
            {
                LoadDockingLayout();
            }
            catch (Exception)
            {
            }
        }

        BinaryImage image;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //string fileName = @"E:\Dev\Projects\DosDebugger\Test\H.EXE";
            //DoOpenFile(fileName);
            mnuHelpTest_Click(null, null);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveDockingLayout();
            }
            catch (Exception)
            {
            }
        }

        #region Docking Layout Save/Load

        private void SaveDockingLayout()
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(
                dockingManager);
            using (var stream = System.IO.File.Create("AvalonLayoutConfig.xml"))
            {
                serializer.Serialize(stream);
            }
        }

        private void LoadDockingLayout()
        {
            var serializer = new AvalonDock.Layout.Serialization.XmlLayoutSerializer(
                dockingManager);
            //serializer.LayoutSerializationCallback += serializer_LayoutSerializationCallback;
            using (var stream = System.IO.File.OpenRead("AvalonLayoutConfig.xml"))
            {
                serializer.Deserialize(stream);
            }
        }

        private void mnuFileSaveLayout_Click(object sender, RoutedEventArgs e)
        {
            SaveDockingLayout();
            MessageBox.Show("Layout saved.");
        }

        private void mnuFileLoadLayout_Click(object sender, RoutedEventArgs e)
        {
            LoadDockingLayout();
        }

        #endregion

        private void DoOpenFile(string fileName)
        {
            MZFile mzFile = new MZFile(fileName);
            mzFile.Relocate(0);
            Disassembler16 dasm = new Disassembler16(mzFile.Image, mzFile.BaseAddress);
            dasm.Analyze(mzFile.EntryPoint);

            this.image = dasm.Image;
            //this.disassemblyList.Image = image;
            this.procedureList.Image = image;
            this.errorList.Image = image;
            this.segmentList.Image = image;
            this.propertiesWindow.Image = image;
        }

        private void mnuHelpTest_Click(object sender, RoutedEventArgs e)
        {
            string fileName = @"..\..\..\..\Test\SLIBC7.LIB";
            ObjectLibrary library = OmfLoader.LoadLibrary(fileName);
            library.ResolveAllSymbols();

            ObjectModule module = library.FindModule("_ctype");
            DefinedSymbol symbol = module.DefinedNames.Find(x => x.Name == "_isupper");
            Address entryPoint = new Address(
                symbol.BaseSegment.Id, (int)symbol.Offset);

            Disassembler16New dasm = new Disassembler16New(library);
            dasm.Analyze(entryPoint);

            this.disassemblyList.SetView(symbol.BaseSegment.Image);

            this.libraryBrowser.Library = library;
        }

        private void mnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Tool Window Activation

        private void mnuViewSegments_Click(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(segmentList);
        }

        private void mnuViewErrors_Click(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(errorList);
        }

        private void mnuViewProcedures_Click(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(procedureList);
        }

        private void mnuViewProperties_Click(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(propertiesWindow);
        }

        private void mnuViewLibraryBrowser_Click(object sender, RoutedEventArgs e)
        {
            ActivateToolWindow(libraryBrowser);
        }

        /// <summary>
        /// Activates the dockable pane that contains the given control.
        /// The search is performed by matching the pane's ContentId to the
        /// controls's Name. If no dockable pane contains the control, one is
        /// created at the bottom side of the docking root; in this case, the
        /// control's ToolTip (if it is a non-null string) is used as the
        /// pane's Title.
        /// </summary>
        /// <param name="control">The control to activate.</param>
        /// <remarks>
        /// This code is partly adapted from AvalonDock samples. It's not
        /// clear how it's done, but normally it works.
        /// </remarks>
        private void ActivateToolWindow(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            string contentId = control.Name;

            LayoutAnchorable pane = dockingManager.Layout.Descendents().OfType<
                LayoutAnchorable>().SingleOrDefault(a => a.ContentId == contentId);

            if (pane == null)
            {
                // The pane is not created. This can happen for example when
                // we load from an old layout configuration file, and the
                // pane is not defined in that file. In this case, we add the
                // control to a default location.
                var anchorSide = dockingManager.BottomSidePanel.Model as LayoutAnchorSide;
                LayoutAnchorGroup anchorGroup;
                if (anchorSide.ChildrenCount == 0)
                {
                    anchorGroup = new LayoutAnchorGroup();
                    anchorSide.Children.Add(anchorGroup);
                }
                else
                {
                    anchorGroup = anchorSide.Children[0];
                }

                pane = new LayoutAnchorable();
                pane.ContentId = contentId;
                pane.Content = control;
                if (control.ToolTip is string)
                {
                    pane.Title = (string)control.ToolTip;
                }
                anchorGroup.Children.Add(pane);
            }

            if (pane.IsHidden)
            {
                pane.Show();
            }
            else if (pane.IsVisible)
            {
                pane.IsActive = true;
            }
            else
            {
                pane.AddToLayout(dockingManager,
                    AnchorableShowStrategy.Bottom |
                    AnchorableShowStrategy.Most);
            }

            //control.Focus
            //if (!control.Focus())
            //    throw new InvalidOperationException();
            //Keyboard.Focus(control);
        }

        #endregion

        #region Select Theme

        private void mnuViewThemeItem_Click(object sender, RoutedEventArgs e)
        {
            mnuViewThemeDefault.IsChecked = false;
            mnuViewThemeAero.IsChecked = false;
            mnuViewThemeExpressionDark.IsChecked = false;
            mnuViewThemeExpressionLight.IsChecked = false;
            mnuViewThemeMetro.IsChecked = false;
            mnuViewThemeVS2010.IsChecked = false;

            if (sender == mnuViewThemeVS2010)
                dockingManager.Theme = new AvalonDock.Themes.VS2010Theme();
            else if (sender == mnuViewThemeExpressionDark)
                dockingManager.Theme = new AvalonDock.Themes.ExpressionDarkTheme();
            else if (sender == mnuViewThemeExpressionLight)
                dockingManager.Theme = new AvalonDock.Themes.ExpressionLightTheme();
            else if (sender == mnuViewThemeAero)
                dockingManager.Theme = new AvalonDock.Themes.AeroTheme();
            else if (sender == mnuViewThemeMetro)
                dockingManager.Theme = new AvalonDock.Themes.MetroTheme();
            else
                dockingManager.Theme = null;

            ((MenuItem)sender).IsChecked = true;
        }
        
        #endregion

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            //MessageBox.Show(string.Format(
            //    "Navigating to {0} in {1}", e.Uri, e.Target));
            Pointer address = Pointer.Parse(e.Uri.Fragment.Substring(1)); // skip #
            //disassemblyList.GoToAddress(address);
        }

        private void FileOpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FileOpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Executable file|*.exe";
            dlg.Title = "Select File To Analyze";

            if (dlg.ShowDialog(this) == true)
            {
                DoOpenFile(dlg.FileName);
            }
        }

        private void mnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "DOS Disassembler\r\nCopyright fanci 2012-2013\r\n",
                            "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void libraryBrowser_RequestProperty(object sender, RequestPropertyEventArgs e)
        {
            propertiesWindow.SelectedObject = e.SelectedObject;
        }
    }
}
