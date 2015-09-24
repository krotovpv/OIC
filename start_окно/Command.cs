using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace start_окно
{
    class Command
    {
        //общий останов
        byte[] commandAllStop = new byte[]{
            0xAA,//адрес
            0x01,//размер команды
            0x00,//код команды
            0x70, 0x50//CRC
        };

        //**********скорость***************

        //опрос канала скорости
        byte[] commandSpeed = new byte[]{
            0x03,//адрес
            0x01,//размер команды
            0x00,//код команды
            0x80, 0x50//CRC
        };

        //задать скорость вращения бегового барабана
        byte[] commandSpeedLoad = new byte[9];

        //останов бегавого барабана
        byte[] commandSpeedStop = new byte[]{
            0x03,//адрес
            0x01,//размер команды
            0x01,//код команды
            0x41, 0x90//CRC
        };

        //установить напряжение ЦАП на выходе
        byte[] commandCapSpeed = new byte[7];
        

        //***********нагрузка*************

        //опрос нагрузки по 1 стороне
        byte[] commandOilOpros1 = new byte[]{
            0x01,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0x21, 0x90//CRC
        };

        //опрос нагрузки по 2 стороне
        byte[] commandOilOpros2 = new byte[]{
            0x02,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0xD1, 0x90//CRC
        };

        //включить маслостанцию по 1 стороне
        byte[] commandOilOn1 = new byte[]{
            0x01,//адрес устройства
            0x01,//размер команды
            0x01,//код команды
            0xE0, 0x50//CRC
        };

        //включить маслостанцию по 2 стороне
        byte[] commandOilOn2 = new byte[]{
            0x02,//адрес устройства
            0x01,//размер команды
            0x01,//код команды
            0x10, 0x50//CRC
        };

        //выключить маслостанцию по 1 стороне
        byte[] commandOilOff1 = new byte[]{
            0x01,//адрес устройства
            0x01,//размер команды
            0x02,//код команды
            0xA0, 0x51//CRC
        };

        //выключить маслостанцию по 2 стороне
        byte[] commandOilOff2 = new byte[]{
            0x02,//адрес устройства
            0x01,//размер команды
            0x02,//код команды
            0x50, 0x51//CRC
        };

        //отвести каретку в исходное положение по 1 стороне
        byte[] commandKoretkaDefault1 = new byte[]{
            0x01,//адрес устройства
            0x01,//размер команды
            0x05,//код команды
            0xE1, 0x93//CRC
        };

        //отвести каретку в исходное положение по 2 стороне
        byte[] commandKoretkaDefault2 = new byte[]{
            0x02,//адрес устройства
            0x01,//размер команды
            0x05,//код команды
            0x11, 0x93//CRC
        };

        //задать значение автоматически поддерживаемой нагрузки на шину по 1 стороне
        byte[] commandAutoPress1 = new byte[20];

        //задать значение автоматически поддерживаемой нагрузки на шину по 2 стороне
        byte[] commandAutoPress2 = new byte[20];

        //установить напряжение на выходе ЦАП нагрузки по 1 стороне
        byte[] commandCapPress1 = new byte[7];

        //установить напряжение на выходе ЦАП нагрузки по 2 стороне
        byte[] commandCapPress2 = new byte[7];


        //**************динамический радиус*********************

        //опрос динамического радиуса по 1 стороне
        byte[] commandRadius1 = new byte[]{
            0x04,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0x31, 0x91//CRC
        };

        //опрос динамического радиуса по 2 стороне
        byte[] commandRadius2 = new byte[]{
            0x05,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0x60, 0x51//CRC
        };


        //*************темпиратура******************

        //опрос темпиратуры по 1 стороне
        byte[] commandTemperature1 = new byte[]{
            0x08,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0x31, 0x91//CRC
        };

        //опрос темпиратуры по 2 стороне
        byte[] commandTemperature2 = new byte[]{
            0x09,//адрес устройства
            0x01,//размер команды
            0x00,//код команды
            0x60, 0x51//CRC
        };

        //включить питание пирометра по 1 стороне
        byte[] commandTemperatureOn1 = new byte[]{
            0x08,//адрес устройства
            0x01,//размер команды
            0x01,//код команды
            0xE0, 0x50//CRC
        };

        //включить питание пирометра по 2 стороне
        byte[] commandTemperatureOn2 = new byte[]{
            0x09,//адрес устройства
            0x01,//размер команды
            0x01,//код команды
            0x10, 0x50//CRC
        };

        //выключить питание пирометра по 1 стороне
        byte[] commandTemperatureOff1 = new byte[]{
            0x08,//адрес устройства
            0x01,//размер команды
            0x02,//код команды
            0xA0, 0x51//CRC
        };

        //выключить питание пирометра по 2 стороне
        byte[] commandTemperatureOff2 = new byte[]{
            0x09,//адрес устройства
            0x01,//размер команды
            0x02,//код команды
            0x50, 0x51//CRC
        };


        byte[] AllStop()
        {
            return commandAllStop;
        }

        public byte[] Nugruzka(UInt16 storona, UInt16 codCommand, //коды: 0-опрос, 1-включить, 2-выключить, 5-отвод коретки, 6-поддерска нагрузки
            UInt16 Fzad = 0, UInt16 F0 = 0, UInt16 Cf = 0, UInt16 K1 = 0, UInt16 K2 = 0, UInt16 C0 = 0, UInt16 deltaF = 0, 
            UInt16 deltaC0 = 0, UInt16 dCmin = 0, UInt16 dC0 = 0, UInt16 NC0 = 0, UInt16 N = 0 )
        {
            switch (codCommand)
            {
                case 0://опрос нагрузки
                    switch (storona)
                    {
                        case 1:
                            return commandOilOpros1;
                        case 2:
                            return commandOilOpros2;
                        default:
                            return commandOilOpros1;
                    }
                case 1://включение СГП
                    switch (storona)
                    {
                        case 1:
                            return commandOilOn1;
                        case 2:
                            return commandOilOn2;
                        default:
                            return commandOilOpros1;
                    }
                case 2://выключение СГП
                    switch (storona)
                    {
                        case 1:
                            return commandOilOff1;
                        case 2:
                            return commandOilOff2;
                        default:
                            return commandOilOpros1;
                    }
                case 5://отвести коретку в исходное положение
                    switch (storona)
                    {
                        case 1:
                            return commandKoretkaDefault1;
                        case 2:
                            return commandKoretkaDefault2;
                        default:
                            return commandOilOpros1;
                    }
                case 6://задаем автоматическое удержание нагрузки
                    byte[] resaultCRC = new byte[2];
                    byte[] bStorona = BitConverter.GetBytes(storona);
                    byte[] bFzad = BitConverter.GetBytes(Fzad);
                    byte[] bF0 = BitConverter.GetBytes(F0);
                    byte[] bCf = BitConverter.GetBytes(Cf);
                    byte[] bK1 = BitConverter.GetBytes(K1);
                    byte[] bK2 = BitConverter.GetBytes(K2);
                    byte[] bC0 = BitConverter.GetBytes(C0);
                    byte[] bDeltaF = BitConverter.GetBytes(deltaF);
                    byte[] bDeltaC0 = BitConverter.GetBytes(deltaC0);
                    byte[] bDCmin = BitConverter.GetBytes(dCmin);
                    byte[] bDC0 = BitConverter.GetBytes(dC0);
                    byte[] bNC0 = BitConverter.GetBytes(NC0);
                    byte[] bN = BitConverter.GetBytes(N);
                    commandAutoPress1[0] = bStorona[0];
                    commandAutoPress1[1] = 0x0A;
                    commandAutoPress1[2] = 0x06;
                    commandAutoPress1[3] = bFzad[0];
                    commandAutoPress1[4] = bFzad[1];
                    commandAutoPress1[5] = bF0[0];
                    commandAutoPress1[6] = bF0[1];
                    commandAutoPress1[7] = bCf[0];
                    commandAutoPress1[8] = bCf[1];
                    commandAutoPress1[9] = bK1[0];
                    commandAutoPress1[10] = bK2[0];
                    commandAutoPress1[11] = bC0[0];
                    commandAutoPress1[12] = bC0[1];
                    commandAutoPress1[13] = bDeltaF[0];
                    commandAutoPress1[14] = bDeltaC0[0];
                    commandAutoPress1[15] = bDCmin[0];
                    commandAutoPress1[16] = bDC0[0];
                    commandAutoPress1[17] = bNC0[0];
                    resaultCRC = BitConverter.GetBytes(ModRTU_CRC(commandAutoPress1, 18));
                    commandAutoPress1[18] = resaultCRC[0];
                    commandAutoPress1[19] = resaultCRC[1];
                    return commandAutoPress1;
                default:
                    return commandOilOpros1;
            }
        }

        public byte[] Radius(UInt16 storona)
        {
            switch (storona)
            {
                case 1:
                    return commandRadius1;
                case 2:
                    return commandRadius2;
                default:
                    return commandOilOpros1;
            }
        }

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

    }
}
