﻿#pragma checksum "..\..\MainWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4D23A66720CF468A579E953D92B15FAD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Core;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.DataSources;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Core.ServerMode;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.DataPager;
using DevExpress.Xpf.Editors.DateNavigator;
using DevExpress.Xpf.Editors.ExpressionEditor;
using DevExpress.Xpf.Editors.Filtering;
using DevExpress.Xpf.Editors.Flyout;
using DevExpress.Xpf.Editors.Popups;
using DevExpress.Xpf.Editors.Popups.Calendar;
using DevExpress.Xpf.Editors.RangeControl;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors.Settings.Extension;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.LookUp;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.Xpf.WindowsUI;
using STHM;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace STHM {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Bars.Bar statusBar;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Bars.BarStaticItem stbTaikhoan;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRun;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtExportData;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnStartSchedule;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSetup;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnExitApplication;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Grid.GridControl dtgCommLines;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.ChartControl chart1;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chPGiao;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chPNhan;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chQGiao;
        
        #line default
        #line hidden
        
        
        #line 81 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chQNhan;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.XYDiagram2D diagram1;
        
        #line default
        #line hidden
        
        
        #line 113 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.LineSeries2D linePGiao;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.LineSeries2D linePNhan;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.LineSeries2D lineQGiao;
        
        #line default
        #line hidden
        
        
        #line 130 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Charts.LineSeries2D lineQNhan;
        
        #line default
        #line hidden
        
        
        #line 141 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grLog;
        
        #line default
        #line hidden
        
        
        #line 142 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox rtbLog1;
        
        #line default
        #line hidden
        
        
        #line 150 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Editors.ComboBoxEdit cboTASKNAME;
        
        #line default
        #line hidden
        
        
        #line 151 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal DevExpress.Xpf.Editors.DateEdit dtpickerStartfrom;
        
        #line default
        #line hidden
        
        
        #line 152 "..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnXem;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/STHM;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.statusBar = ((DevExpress.Xpf.Bars.Bar)(target));
            return;
            case 2:
            this.stbTaikhoan = ((DevExpress.Xpf.Bars.BarStaticItem)(target));
            return;
            case 3:
            this.btnRun = ((System.Windows.Controls.Button)(target));
            
            #line 41 "..\..\MainWindow.xaml"
            this.btnRun.Click += new System.Windows.RoutedEventHandler(this.btnRun_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.BtExportData = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\MainWindow.xaml"
            this.BtExportData.Click += new System.Windows.RoutedEventHandler(this.BtExportData_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnStartSchedule = ((System.Windows.Controls.Button)(target));
            
            #line 45 "..\..\MainWindow.xaml"
            this.btnStartSchedule.Click += new System.Windows.RoutedEventHandler(this.btnStartSchedule_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.btnSetup = ((System.Windows.Controls.Button)(target));
            
            #line 46 "..\..\MainWindow.xaml"
            this.btnSetup.Click += new System.Windows.RoutedEventHandler(this.btnSetup_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnExitApplication = ((System.Windows.Controls.Button)(target));
            
            #line 47 "..\..\MainWindow.xaml"
            this.btnExitApplication.Click += new System.Windows.RoutedEventHandler(this.btnExitApplication_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.dtgCommLines = ((DevExpress.Xpf.Grid.GridControl)(target));
            
            #line 49 "..\..\MainWindow.xaml"
            this.dtgCommLines.SelectedItemChanged += new DevExpress.Xpf.Grid.SelectedItemChangedEventHandler(this.dtgCommLines_SelectedItemChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.chart1 = ((DevExpress.Xpf.Charts.ChartControl)(target));
            
            #line 65 "..\..\MainWindow.xaml"
            this.chart1.MouseMove += new System.Windows.Input.MouseEventHandler(this.chart1_MouseMove);
            
            #line default
            #line hidden
            return;
            case 10:
            this.chPGiao = ((System.Windows.Controls.CheckBox)(target));
            
            #line 72 "..\..\MainWindow.xaml"
            this.chPGiao.Checked += new System.Windows.RoutedEventHandler(this.chPGiao_Checked);
            
            #line default
            #line hidden
            
            #line 72 "..\..\MainWindow.xaml"
            this.chPGiao.Unchecked += new System.Windows.RoutedEventHandler(this.chPGiao_Unchecked);
            
            #line default
            #line hidden
            return;
            case 11:
            this.chPNhan = ((System.Windows.Controls.CheckBox)(target));
            
            #line 75 "..\..\MainWindow.xaml"
            this.chPNhan.Unchecked += new System.Windows.RoutedEventHandler(this.chPNhan_Unchecked);
            
            #line default
            #line hidden
            
            #line 75 "..\..\MainWindow.xaml"
            this.chPNhan.Checked += new System.Windows.RoutedEventHandler(this.chPNhan_Checked);
            
            #line default
            #line hidden
            return;
            case 12:
            this.chQGiao = ((System.Windows.Controls.CheckBox)(target));
            
            #line 78 "..\..\MainWindow.xaml"
            this.chQGiao.Checked += new System.Windows.RoutedEventHandler(this.chQGiao_Checked);
            
            #line default
            #line hidden
            
            #line 78 "..\..\MainWindow.xaml"
            this.chQGiao.Unchecked += new System.Windows.RoutedEventHandler(this.chQGiao_Unchecked);
            
            #line default
            #line hidden
            return;
            case 13:
            this.chQNhan = ((System.Windows.Controls.CheckBox)(target));
            
            #line 81 "..\..\MainWindow.xaml"
            this.chQNhan.Unchecked += new System.Windows.RoutedEventHandler(this.chQNhan_Unchecked);
            
            #line default
            #line hidden
            
            #line 81 "..\..\MainWindow.xaml"
            this.chQNhan.Checked += new System.Windows.RoutedEventHandler(this.chQNhan_Checked);
            
            #line default
            #line hidden
            return;
            case 14:
            this.diagram1 = ((DevExpress.Xpf.Charts.XYDiagram2D)(target));
            return;
            case 15:
            this.linePGiao = ((DevExpress.Xpf.Charts.LineSeries2D)(target));
            return;
            case 16:
            this.linePNhan = ((DevExpress.Xpf.Charts.LineSeries2D)(target));
            return;
            case 17:
            this.lineQGiao = ((DevExpress.Xpf.Charts.LineSeries2D)(target));
            return;
            case 18:
            this.lineQNhan = ((DevExpress.Xpf.Charts.LineSeries2D)(target));
            return;
            case 19:
            this.grLog = ((System.Windows.Controls.Grid)(target));
            return;
            case 20:
            this.rtbLog1 = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 21:
            this.cboTASKNAME = ((DevExpress.Xpf.Editors.ComboBoxEdit)(target));
            return;
            case 22:
            this.dtpickerStartfrom = ((DevExpress.Xpf.Editors.DateEdit)(target));
            
            #line 151 "..\..\MainWindow.xaml"
            this.dtpickerStartfrom.EditValueChanged += new DevExpress.Xpf.Editors.EditValueChangedEventHandler(this.dtpickerStartfrom_EditValueChanged);
            
            #line default
            #line hidden
            return;
            case 23:
            this.btnXem = ((System.Windows.Controls.Button)(target));
            
            #line 152 "..\..\MainWindow.xaml"
            this.btnXem.Click += new System.Windows.RoutedEventHandler(this.btnXem_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

