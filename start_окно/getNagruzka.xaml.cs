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

namespace start_окно
{
    /// <summary>
    /// Логика взаимодействия для getNagruzka.xaml
    /// </summary>
    public partial class getNagruzka : Window
    {
        int storona = 0;
        public getNagruzka(int storona)
        {
            InitializeComponent();
            this.storona = storona;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
                        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow frmMain = this.Owner as  MainWindow;
            if (frmMain != null)
            {
                switch (storona)
                {
                    case 1: frmMain.codComandy1 = 6; frmMain.zadanayaNagruzka1 = Convert.ToUInt16(txtZadatNagruzku.Text); break;
                    case 2: frmMain.codComandy2 = 6; frmMain.zadanayaNagruzka2 = Convert.ToUInt16(txtZadatNagruzku.Text); break;
                    default:
                        break;
                }
                this.Close();
            }

            //frmMain.qwe(Convert.ToUInt16(txtZadatNagruzku.Text));

            this.Close();
        }

    }
}
