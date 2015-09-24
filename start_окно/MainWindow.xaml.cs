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
using System.Windows.Threading;
using System.IO.Ports;
using System.IO;

namespace start_окно
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort serialPort = new SerialPort();
        Command command = new Command();

        public delegate void NextPrimeDelegate();
        string izmerenoNagruzka = "";

        const string fileNameSettingPort = "SettingsPort.dat";//файл для хранения настроек
        const string fileNameSettingNagruzka = "SettingsNagruzka.dat";//файл для хранения настроек

        Int16 settingPort = 0;//настройка порта
        Int16 settingPortSpeed = 0;//скорость порта

        int numberOprosa = 0;//указывает на то, какой модуль опрашиваеться в данный момент
        int colOpros = 0;//колличество выполненых опросов
        public UInt16 codComandy1 = 0;//код команды для 1 стороны
        public UInt16 codComandy2 = 0;//код команды для 2 стороны

        //переменные для настроек нагрузки
        public UInt16 _Fzad, _F0, _Cf, _K1, _K2, _C0, _deltaF, _deltaC0, _dCmin, _dC0, _NC0, _N = 0;
        public UInt16 zadanayaNagruzka1, zadanayaNagruzka2 = 0;//заданная нагрузка

        int[] sostoyanieNagruzkaHi1 = new int[8];//расшифровка в int старшего байта состояния 1 стророны
        int[] sostoyanieNagruzkaLow1 = new int[8];//расшифровка в int младшего байта состояния 1 стророны



        int[] sostoyanieNagruzkaHi2 = new int[8];//расшифровка в int старшего байта состояния 2 стророны
        int[] sostoyanieNagruzkaLow2 = new int[8];//расшифровка в int младшего байта состояния 2 стророны


        public MainWindow()
        {
            InitializeComponent(); 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //прочтение настроек
            ReadDefaultValues();

            //настройки ком порта
            settingComPort();

            //открываем порт
            //serialPortOpen(comboPortName.SelectedItem.ToString(), Convert.ToInt32(comboPortSpeed.SelectedItem));

            //после открытия порта приступаем к опросу стенда в потоке
            //System.Threading.Thread cycleOpros = new System.Threading.Thread(serialPortTransmitCycleStart);
            //cycleOpros.Start();


            TC_jornal_1.Visibility = System.Windows.Visibility.Hidden;
            TC_jornal_2.Visibility = System.Windows.Visibility.Hidden;
            TC_jornal_KSK.Visibility = System.Windows.Visibility.Hidden;
        }

        //настройки для ком порта
        private void settingComPort()
        {
            string[] ports = SerialPort.GetPortNames();//создаем массив и заполняем его именами доступных ком-партов
            comboPortName.ItemsSource = ports;//заполняем комбобокс именами доступных ком-портов
            comboPortName1.ItemsSource = ports;//заполняем комбобокс именами доступных ком-портов
            //если есть свободные порты, выбрать первый из списка
            if (ports.Length > 0 && ports.Length > settingPort) { comboPortName.SelectedIndex = settingPort; comboPortName1.SelectedIndex = settingPort; }
            else { MessageBox.Show("Подключение невозможно из за отсутствия выбраного COM порта"); comboPortName.SelectedIndex = 0; comboPortName1.SelectedIndex = 0; }
            comboPortSpeed.Items.Add("9600");
            comboPortSpeed1.Items.Add("9600");
            comboPortSpeed.SelectedIndex = settingPortSpeed;//указываем скорость порта
            comboPortSpeed1.SelectedIndex = settingPortSpeed;//указываем скорость порта
        }

        //метод вносит необходимые настройки и открывает порт
        public void serialPortOpen(string _portName, int _baudRate)
        {
            serialPort.PortName = _portName;
            serialPort.BaudRate = _baudRate;

            if (!serialPort.IsOpen)
                serialPort.Open();

            this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort_DataReceived);//подписываемся на свойство приема сообщений
        }

        //прием данных из порта
        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!serialPort.IsOpen) return;
            //принимаем сообщение
            System.Threading.Thread.Sleep(50);
            byte[] bytesToListen = new byte[serialPort.BytesToRead];
            UInt16 checkSumm = 0;//контрольная сумма (CRC)
            serialPort.Read(bytesToListen, 0, serialPort.BytesToRead);

            //составляем контрольную сумму из полученого сообщения
            checkSumm = ModRTU_CRC(bytesToListen, bytesToListen.Length - 2);
            byte[] bCheckSumm = BitConverter.GetBytes(checkSumm);
            
            //если проверка контрольной суммы пройдена тогда приступаем к обработке сообщения
            if (bytesToListen.Length == 0) return;
            if (bCheckSumm[0] == bytesToListen[bytesToListen.Length - 2] && bCheckSumm[1] == bytesToListen[bytesToListen.Length - 1])
            {               
                if(numberOprosa == 0)//нагрузка 1 стороны
                {
                    lbl_nagruzka_zadano_Copy.Dispatcher.Invoke(new Action(() =>
                    { 
                        lbl_temperatura_izmer_Copy.Content = bytesToListen[2];
                        lbl_temperatura_izmer.Content = bytesToListen[2];
                        lbl_davlenie_izmer_Copy.Content = bytesToListen[3];
                        lbl_davlenie_izmer.Content = bytesToListen[3];
                        lbl_nagruzka_zadano_Copy.Content = zadanayaNagruzka1;//вписываем заданную нагрузку
                        lbl_nagruzka_zadano.Content = zadanayaNagruzka1;//вписываем заданную нагрузку
                        lbl_nagruzka_izmer_Copy.Content = BitConverter.ToInt16(bytesToListen, 4).ToString();
                        lbl_nagruzka_izmer.Content = BitConverter.ToInt16(bytesToListen, 4).ToString();

                        sostoyanieNagruzkaHi1 = byteToArreyInt(bytesToListen[0]);
                        sostoyanieNagruzkaLow1 = byteToArreyInt(bytesToListen[1]);

                        if (sostoyanieNagruzkaHi1[1] == 1)
                        {
                            lbl_sgp_on_off.Content = "СГП: включена";
                            lbl_sgp_on_off_Copy.Content = "СГП: включена";
                            lbl_sgp_on_off.Background = Brushes.Green;
                            lbl_sgp_on_off_Copy.Background = Brushes.Green;
                            lbl_avariyniy_uroven_masla.Background = Brushes.Transparent;
                            lbl_avariyniy_uroven_masla_Copy.Background = Brushes.Transparent;
                        }
                        else
                        {
                            lbl_sgp_on_off.Content = "СГП: выключена";
                            lbl_sgp_on_off_Copy.Content = "СГП: включена";
                            lbl_sgp_on_off.Background = Brushes.Red;
                            lbl_sgp_on_off_Copy.Background = Brushes.Red;
                            lbl_avariyniy_uroven_masla.Background = Brushes.Red;
                            lbl_avariyniy_uroven_masla_Copy.Background = Brushes.Red;
                        }

                        //индикация исходного полажения
                        if (sostoyanieNagruzkaLow1[4] == 1) lbl_ishodnoe_pologenie_Copy.Background = Brushes.Green;
                        else lbl_ishodnoe_pologenie_Copy.Background = Brushes.Gray;
                        if (sostoyanieNagruzkaLow1[4] == 1) lbl_ishodnoe_pologenie.Background = Brushes.Green;
                        else lbl_ishodnoe_pologenie.Background = Brushes.Gray;        
                    }));

                    if (codComandy1 == 1 && sostoyanieNagruzkaHi1[1] == 1) codComandy1 = 0;//если включение произошло то сбрасываем команду в нуль
                    if (codComandy1 == 2 && sostoyanieNagruzkaHi1[1] == 0) codComandy1 = 0;//если выключение произошло то сбрасываем команду в нуль
                    if (codComandy1 == 5 && sostoyanieNagruzkaLow1[4] == 1) codComandy1 = 0;//отвод произошол
                    if (codComandy1 == 6 && sostoyanieNagruzkaLow1[1] == 1) codComandy1 = 0;//автоматическое поддержание нагрузки началось

                    numberOprosa += 1;
                    colOpros = 0;
                }
                else if(numberOprosa == 1)//нагрузка 2 стороны
                {
                    lbl_temperatura_izmer_Copy1.Dispatcher.Invoke(new Action(() => 
                    { 
                        lbl_temperatura_izmer_Copy1.Content = bytesToListen[2];
                        lbl_temperatura_izmer1.Content = bytesToListen[2];
                        lbl_davlenie_izmer_Copy1.Content = bytesToListen[3];
                        lbl_davlenie_izmer1.Content = bytesToListen[3];
                        lbl_nagruzka_zadano_Copy1.Content = zadanayaNagruzka2;//вписываем заданную нагрузку
                        lbl_nagruzka_zadano1.Content = zadanayaNagruzka2;//вписываем заданную нагрузку
                        lbl_nagruzka_izmer1.Content = BitConverter.ToInt16(bytesToListen, 4).ToString();
                        lbl_nagruzka_izmer_Copy1.Content = BitConverter.ToInt16(bytesToListen, 4).ToString();

                        sostoyanieNagruzkaHi2 = byteToArreyInt(bytesToListen[0]);
                        sostoyanieNagruzkaLow2 = byteToArreyInt(bytesToListen[1]);

                        if (sostoyanieNagruzkaHi2[1] == 1)
                        {
                            lbl_sgp_on_off1.Content = "СГП: включена";
                            lbl_sgp_on_off_Copy1.Content = "СГП: включена";
                            lbl_sgp_on_off1.Background = Brushes.Green;
                            lbl_sgp_on_off_Copy1.Background = Brushes.Green;
                            lbl_avariyniy_uroven_masla1.Background = Brushes.Transparent;
                            lbl_avariyniy_uroven_masla_Copy1.Background = Brushes.Transparent;
                        }
                        else
                        {
                            lbl_sgp_on_off1.Content = "СГП: выключена";
                            lbl_sgp_on_off_Copy1.Content = "СГП: включена";
                            lbl_sgp_on_off1.Background = Brushes.Red;
                            lbl_sgp_on_off_Copy1.Background = Brushes.Red;
                            lbl_avariyniy_uroven_masla1.Background = Brushes.Red;
                            lbl_avariyniy_uroven_masla_Copy1.Background = Brushes.Red;
                        }

                        //индикация исходного полажения
                        if (sostoyanieNagruzkaLow2[4] == 1) lbl_ishodnoe_pologenie_Copy1.Background = Brushes.Green;
                        else lbl_ishodnoe_pologenie_Copy1.Background = Brushes.Gray;
                        if (sostoyanieNagruzkaLow2[4] == 1) lbl_ishodnoe_pologenie1.Background = Brushes.Green;
                        else lbl_ishodnoe_pologenie1.Background = Brushes.Gray;
                    }));

                    if (codComandy2 == 1 && sostoyanieNagruzkaHi2[1] == 1) codComandy2 = 0;//если включение произошло то сбрасываем команду в нуль
                    if (codComandy2 == 2 && sostoyanieNagruzkaHi2[1] == 0) codComandy2 = 0;//если выключение произошло то сбрасываем команду в нуль
                    if (codComandy2 == 5 && sostoyanieNagruzkaLow2[4] == 1) codComandy2 = 0;//отвод произошол
                    if (codComandy2 == 6 && sostoyanieNagruzkaLow2[1] == 1) codComandy2 = 0;//автоматическое поддержание нагрузки началось

                    numberOprosa += 1;
                    colOpros = 0;
                }

                else if(numberOprosa == 2)//радиус 1 стороны
                {
                    lbl_din_radius_izmer_Copy.Dispatcher.Invoke(new Action(() =>
                    {
                        lbl_din_radius_izmer_Copy.Content = BitConverter.ToUInt16(bytesToListen, 1);
                        lbl_din_radius_izmer.Content = BitConverter.ToUInt16(bytesToListen, 1);
                    }));

                    numberOprosa += 1;
                    colOpros = 0;
                }

                else if (numberOprosa == 3)//радиус 2 стороны
                {
                    lbl_din_radius_izmer_Copy1.Dispatcher.Invoke(new Action(() =>
                    {
                        lbl_din_radius_izmer_Copy1.Content = BitConverter.ToUInt16(bytesToListen, 1);
                        lbl_din_radius_izmer1.Content = BitConverter.ToUInt16(bytesToListen, 1);
                    }));

                    numberOprosa = 0;
                    colOpros = 0;
                }
            }
        }

        //цикл постоянного опроса стенда
        void serialPortTransmitCycleStart()
        {
            byte[] temp = new byte[1];

            while (true)
            {
                //составляем запрос
                switch (numberOprosa)
                {
                    case 0:
                        temp = command.Nugruzka(1, codComandy1, zadanayaNagruzka1, _F0, _Cf, _K1, _K2, _C0, _deltaF, _deltaC0, _dCmin, _dC0, _NC0, _N);
                        if(serialPort.IsOpen) serialPort.Write(temp, 0, temp.Length);//если порт открыт отправляем сообщение                         
                        colOpros += 1;
                        if (colOpros >= 3) { numberOprosa += 1; colOpros = 0; System.Threading.Thread.Sleep(500); }//если было уже сделано 3 опроса но ответ так и не получен то прекратить опрос
                        else System.Threading.Thread.Sleep(500);//если опросов менее 3 то остановить поток, что бы дать время на ответ
                        break;
                    case 1:
                        temp = command.Nugruzka(2, codComandy2, zadanayaNagruzka2, _F0, _Cf, _K1, _K2, _C0, _deltaF, _deltaC0, _dCmin, _dC0, _NC0, _N);
                        if (serialPort.IsOpen) serialPort.Write(temp, 0, temp.Length);//если порт открыт отправляем сообщение                         
                        colOpros += 1;
                        if (colOpros >= 3) { numberOprosa += 1; colOpros = 0; System.Threading.Thread.Sleep(500); }//если было уже сделано 3 опроса но ответ так и не получен то прекратить опрос
                        else System.Threading.Thread.Sleep(500);//если опросов менее 3 то остановить поток, что бы дать время на ответ
                        break;
                    case 2:
                        temp = command.Radius(1);
                        if (serialPort.IsOpen) serialPort.Write(temp, 0, temp.Length);//если порт открыт отправляем сообщение
                        colOpros += 1;
                        if (colOpros >= 3) { numberOprosa += 1; colOpros = 0; System.Threading.Thread.Sleep(500); }//если было уже сделано 3 опроса но ответ так и не получен то прекратить опрос
                        else System.Threading.Thread.Sleep(500);//если опросов менее 3 то остановить поток, что бы дать время на ответ
                        break;
                    case 3:
                        temp = command.Radius(2);
                        if (serialPort.IsOpen) serialPort.Write(temp, 0, temp.Length);//если порт открыт отправляем сообщение
                        colOpros += 1;
                        if (colOpros >= 3) { numberOprosa = 0; colOpros = 0; System.Threading.Thread.Sleep(500); }//если было уже сделано 3 опроса но ответ так и не получен то прекратить опрос
                        else System.Threading.Thread.Sleep(500);//если опросов менее 3 то остановить поток, что бы дать время на ответ
                        break;
                }
            }
        }

        //считываем настройки из файла
        public void ReadDefaultValues()
        {
            if (File.Exists(fileNameSettingPort))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileNameSettingPort, FileMode.Open)))
                {
                    settingPort = reader.ReadInt16();//настройка порта
                    settingPortSpeed = reader.ReadInt16();//скорость порта
                }
            }

            if (File.Exists(fileNameSettingNagruzka))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileNameSettingNagruzka, FileMode.Open)))
                {
                    txtF0.Text = reader.ReadUInt16().ToString();
                    txtCf.Text = reader.ReadUInt16().ToString();
                    txtK1.Text = reader.ReadUInt16().ToString();
                    txtK2.Text = reader.ReadUInt16().ToString();
                    txtDCmin.Text = reader.ReadUInt16().ToString();
                    txtDC0.Text = reader.ReadUInt16().ToString();
                    txtC0.Text = reader.ReadUInt16().ToString();
                    txtDeltaF.Text = reader.ReadUInt16().ToString();
                    txtDeltaC0.Text = reader.ReadUInt16().ToString();
                    txtNC0.Text = reader.ReadUInt16().ToString();
                    txtN.Text = reader.ReadUInt16().ToString();

                    _F0 = Convert.ToUInt16(txtF0.Text);
                    _Cf = Convert.ToUInt16(txtCf.Text);
                    _K1 = Convert.ToUInt16(txtK1.Text);
                    _K2 = Convert.ToUInt16(txtK2.Text);
                    _dCmin = Convert.ToUInt16(txtDCmin.Text);
                    _dC0 = Convert.ToUInt16(txtDC0.Text);
                    _C0 = Convert.ToUInt16(txtC0.Text);
                    _deltaF = Convert.ToUInt16(txtDeltaF.Text);
                    _deltaC0 = Convert.ToUInt16(txtDeltaC0.Text);
                    _NC0 = Convert.ToUInt16(txtNC0.Text);
                    _N = Convert.ToUInt16(txtN.Text);
                }
            }
        }

        //расчет CRC
        UInt16 ModRTU_CRC(byte[] buf, int len)
        {
            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < len; pos++)
            {
                crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
            return crc;
        }

        //конвертируем байт в массив int
        private int[] byteToArreyInt(byte _byte)
        {
            int[] result = new int[8];
            for (int i = 0; i < 8; i++)
            {
                result[i] = (_byte >> i) & 0x01;
            }
            return result;
        }
        
        //включение СГП по 1 стороне
        private void btn_oil1_Click(object sender, RoutedEventArgs e)
        {
            if (sostoyanieNagruzkaHi1[1] == 0)
            {
                codComandy1 = 1;
                btn_oil1.Content = "Выкл. СГП";
            }
            else if (sostoyanieNagruzkaHi1[1] == 1)
            {
                codComandy1 = 2;
                btn_oil1.Content = "Вкл. СГП";
            }
        }

        //включение СГП по 2 стороне
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (sostoyanieNagruzkaHi2[1] == 0)
            {
                codComandy2 = 1;
                btn_oil2.Content = "Выкл. СГП";
            }
            else if (sostoyanieNagruzkaHi2[1] == 1)
            {
                codComandy2 = 2;
                btn_oil2.Content = "Вкл. СГП";
            }
        }

        //отвести в исходное положение по первой стороне
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            codComandy1 = 5;
            zadanayaNagruzka1 = 0;
        }

        //отвести в исходное положение по второй стороне
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            codComandy2 = 5;
            zadanayaNagruzka2 = 0;
        }

        //задать нагрузку для первой стороны
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            getNagruzka frmGetNugruzka = new getNagruzka(1);
            frmGetNugruzka.Owner = this;
            frmGetNugruzka.Show();
        }

        //задать нагрузку для второй стороны
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            getNagruzka frmGetNugruzka = new getNagruzka(2);
            frmGetNugruzka.Owner = this;
            frmGetNugruzka.Show();
        }

        //сохранение настроек порта
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            settingPort = (Int16)comboPortName.SelectedIndex;
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameSettingPort, FileMode.Create)))
            {
                writer.Write(settingPort);//настройка порта
                writer.Write(settingPortSpeed);//скорость порта
            }
            MessageBox.Show("Сохранено!");
        }

        //сохранение настроек порта
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingPort = (Int16)comboPortName1.SelectedIndex;
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameSettingPort, FileMode.Create)))
            {
                writer.Write(settingPort);//настройка порта
                writer.Write(settingPortSpeed);//скорость порта
            }
            MessageBox.Show("Сохранено!");
        }

        //закрытие программы
        private void Window_Closed_1(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }

        //сохранение настроек нагрузки
        private void btn_primenit_Click(object sender, RoutedEventArgs e)
        {
            _F0 = Convert.ToUInt16(txtF0.Text);
            _Cf = Convert.ToUInt16(txtCf.Text);
            _K1 = Convert.ToUInt16(txtK1.Text);
            _K2 = Convert.ToUInt16(txtK2.Text);
            _dCmin = Convert.ToUInt16(txtDCmin.Text);
            _dC0 = Convert.ToUInt16(txtDC0.Text);
            _C0 = Convert.ToUInt16(txtC0.Text);
            _deltaF = Convert.ToUInt16(txtDeltaF.Text);
            _deltaC0 = Convert.ToUInt16(txtDeltaC0.Text);
            _NC0 = Convert.ToUInt16(txtNC0.Text);
            _N = Convert.ToUInt16(txtN.Text);

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameSettingNagruzka, FileMode.Create)))
            {
                writer.Write(_F0);//код нулевой нагрузки
                writer.Write(_Cf);//код подвода каретки до касания
                writer.Write(_K1);//коэф. регулятора при увеличении нагрузки
                writer.Write(_K2);//коэф. регулятора при уменьшении нагрузки
                writer.Write(_dCmin);//мин. шаг изменения кода управления
                writer.Write(_dC0);//шаг изменения кода управления
                writer.Write(_C0);//коэф. кода управления
                writer.Write(_deltaF);//код допустимога откланения нагрузки
                writer.Write(_deltaC0);//код допустимого отклонения коэф. кода управления
                writer.Write(_NC0);//кол-во выборок, в течении которых не вычисляется новое значение С0
                writer.Write(_N);//кол-во выборок для усреднения при управлении нагрузкой
            }
            MessageBox.Show("Сохранено!");
        }

        private void btn_nachat_ispytanie_Click(object sender, RoutedEventArgs e)
        {
            Protokol_x_2 frmProtokol = new Protokol_x_2();
            frmProtokol.Show();
        }

       
    }

}
