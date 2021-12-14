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
using System.Windows.Shapes;

namespace STHM
{
    /// <summary>
    /// Interaction logic for frm_SetupMangager.xaml
    /// </summary>
    public partial class frm_SetupMangager : Window
    {
        public bool IsChanged { get; set; }
        public frm_SetupMangager()
        {
            InitializeComponent();
            this.Closing += frm_SetupMangager_Closing;
        }

        void frm_SetupMangager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ctConfig.IsChanged == true)
            {
                IsChanged = true;
            }
            else if (ctSchedule.IsChanged == true)
            {
                IsChanged = true;
            }
            else if (ctLineMerter.IsChanged == true)
            {
                IsChanged = true;
            }
            else IsChanged = false;
        }
    }
}
