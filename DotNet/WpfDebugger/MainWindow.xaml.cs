using System;
using System.Collections.Generic;
using System.IO;
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

        Assembly program;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
#if false
            //string fileName = @"E:\Dev\Projects\DosDebugger\Test\H.EXE";
            string fileName = @"E:\Dev\Projects\DosDebugger\Test\New\NEWHELLO.EXE";
            DoOpenFile(fileName);
#else
            string fileName = @"..\..\..\..\Test\SLIBC7.LIB";
            //string fileName = @"E:\Dev\Projects\DosDebugger\Test\Q.EXE";
            //mnuHelpTest_Click(null, null);
            DoOpenFile(fileName);
#endif
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
            if (fileName.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                DoOpenExeFile(fileName);
            }
            else if (fileName.EndsWith(".lib", StringComparison.InvariantCultureIgnoreCase))
            {
                DoOpenLibFile(fileName);
            }
            else
            {
                MessageBox.Show("File type is not supported.");
            }
        }

        private void DoOpenExeFile(string fileName)
        {
            //MZFile mzFile = new MZFile(fileName);
            //mzFile.Relocate(0x1000); // TODO: currently we don't support loadin
            // at segment 0. We should fix this later.
            Executable executable = new Executable(fileName);
            ExecutableDisassembler dasm = new ExecutableDisassembler(executable);
            dasm.Analyze(executable.EntryPoint);

            this.program = executable;
            //this.disassemblyList.Image = image;
            this.procedureList.Program=program;
            this.errorList.Program = program;
            this.segmentList.Program = program;
            // this.propertiesWindow.Image = image;
        }

        private void DoOpenLibFile(string fileName)
        {
            ObjectLibrary library = OmfLoader.LoadLibrary(fileName);
            library.ResolveAllSymbols();

            LibraryDisassembler dasm = new LibraryDisassembler(library);
            dasm.Analyze();

            this.program = library;
            this.procedureList.Program = program;
            this.errorList.Program = program;
            this.segmentList.Program = program;

            // Display all unresolved symbols.
            foreach (string key in library.GetUnresolvedSymbols())
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    "Symbol {0} is unresolved.", key));
            }

            this.libraryBrowser.Library = library;

#if false
            string symbolToFind = "FISRQQ";
            foreach (var mod in library.Symbols[symbolToFind])
            {
                System.Diagnostics.Debug.WriteLine(string.Format(
                    "Symbol {0} is defined in module {1}",
                    symbolToFind, mod.Name));
            }
            //library.DuplicateSymbols

            ObjectModule module = library.FindModule("_ctype");
            DefinedSymbol symbol = module.DefinedNames.Find(x => x.Name == "_isupper");
            Address entryPoint = new Address(
                symbol.BaseSegment.Id, (int)symbol.Offset);

            Disassembler16New dasm = new Disassembler16New(library);
            dasm.Analyze(entryPoint);

            this.disassemblyList.SetView(library, symbol.BaseSegment);
#endif
        }

        private void mnuHelpTest_Click(object sender, RoutedEventArgs e)
        {
            string fileName = @"..\..\..\..\Test\SLIBC7.LIB";
            DoOpenLibFile(fileName);
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
            //Pointer address = Pointer.Parse(e.Uri.Fragment.Substring(1)); // skip #
            //disassemblyList.GoToAddress(address);

            AssemblyUri uri = e.Uri as AssemblyUri;
            if (uri == null)
                return;

#if false
            if (uri.Referent is Segment)
            {
                this.disassemblyList.SetView(program, uri.Referent as Segment, uri.Offset);
            }
            else
            {
                MessageBox.Show("Not supported");
            }
#else
            this.disassemblyList.SetView(program, uri.Address);
#endif
        }

        private void FileOpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void FileOpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All supported files|*.exe;*.lib|Executable file|*.exe|Library file|*.lib";
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

        private void mnuToolsExportChecksum_Click(object sender, RoutedEventArgs e)
        {
            if (program == null)
                return;

            BinaryImage image = program.GetImage();
            using (StreamWriter writer = new StreamWriter(@"E:\TestDDD-Procedures.txt"))
            {
                foreach (Procedure procedure in image.Procedures)
                {
                    CodeChecksum checksum = CodeChecksum.Compute(procedure, image);
                    writer.WriteLine("{0} {1} {2} {3}",
                                     image.FormatAddress(procedure.EntryPoint),
                                     procedure.Name,
                                     BytesToString(checksum.OpcodeChecksum).ToLowerInvariant(),
                                     procedure.Size);
                }
            }
        }

        private static string BytesToString(byte[] bytes)
        {
            var soapBinary = new System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary(bytes);
            return soapBinary.ToString();
        }
    }
}
