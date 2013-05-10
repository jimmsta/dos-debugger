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
            string fileName = @"E:\Dev\Projects\DosDebugger\Test\H.EXE";
            MZFile mzFile = new MZFile(fileName);
            mzFile.Relocate(0);
            Disassembler16 dasm = new Disassembler16(mzFile.Image, mzFile.BaseAddress);
            dasm.Analyze(mzFile.EntryPoint);

            this.image = dasm.Image;
            //this.viewModel = new ListingViewModel(dasm.Image);
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

        private void mnuHelpTest_Click(object sender, RoutedEventArgs e)
        {
            this.disassemblyList.Image = image;
            this.procedureList.Image = image;
            this.errorList.Image = image;
            this.segmentList.Image = image;
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
        }

        #endregion

    }
}
