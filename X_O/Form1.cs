using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace X_O
{
    public partial class X_O : Form
    {
        //поле из 9 картинок
        PictureBox[] GameField = new PictureBox[9];
        //переменные для хранение выбора пользователя и игрока кто кем будет играть.
        int Player, Computer = 0, Enemy;

        //игровое поле в виде цифр для просчета выигрыша
        int[] GameFieldMap = {
                             0,0,0,
                             0,0,0,
                             0,0,0
                         };

        Socket listener, socket;

        //список имен картинок используемых в игре
        string[] ImgName =
        {
            "Free_Field.png", //пустой блок
            "X.png", //крестик
            "O.png"  //нолик
        };

        public X_O()
        {
            InitializeComponent();
            MainField();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void MainField()
        {
            //задаем начало рисования поля
            int
                DX = 5,
                DY = 5;

            //размеры картинки
            int
                HeighP = 95, //высота
                WhidthP = 100,  //ширина  
                IndexPicture = 0; //счетчик подсчета картинок
            //имя в ячейке будет начинаться с этих символов
            string NAME = "P_";

            //цикл расстановки ячеек по Y
            for (int YY = 0; YY < 3; YY++)
            {
                //цикл расстановки ячеек по X
                for (int XX = 0; XX < 3; XX++)
                {
                    GameField[IndexPicture] = new PictureBox()
                    {
                        Name = NAME + IndexPicture,                 //Задаем имя картинки
                        Height = HeighP,                            //задаем размер по Y
                        Width = WhidthP,                            //задаем размер 
                        Image = Image.FromFile("Free_Field.png"),        //загружаем изображение пустого поля
                        SizeMode = PictureBoxSizeMode.StretchImage, //заставляем изображение сжаться по размерам картинки
                        Location = new Point(DX, DY)
                    };

                    GameField[IndexPicture].Click += Picture_Click;

                    panelGameField.Controls.Add(GameField[IndexPicture]); //размещаем картинку на пенале управления
                    //рассчитываем новое имя
                    IndexPicture++;

                    DX += WhidthP + 5; //рассчитываем координаты по X для следующей картинки
                }
                DY += HeighP + 5; //рассчитываем координаты по Y для следующей картинки
                DX = 5; //обнуляем позицию для координаты X
            }
        }

        void LoockField()
        {
            //блокируем все поле чтобы игрок не мог на него нажать
            foreach (PictureBox P in GameField)
                P.Enabled = false;
        }

        void UnLoockField()
        {
            int Indexx = 0;
            //разблокируем поля но только те которые не заполнены
            foreach (PictureBox P in GameField)
            {
                //если поле равно 0 значит есть смысл его открывать
                if (GameFieldMap[Indexx++] == 0)
                    Invoke((MethodInvoker)delegate { P.Enabled = true; });
                //P.Enabled = true;
            }
        }

        private void Picture_Click(object sender, EventArgs e)
        {
            if (CanStap())
            {
                PictureBox ClickImage = sender as PictureBox;
                string[] ParsName = ClickImage.Name.Split('_');

                int IndexSelectImage = Convert.ToInt32(ParsName[1]);

                GameField[IndexSelectImage].Image = Image.FromFile(ImgName[Player]);
                GameFieldMap[IndexSelectImage] = Player;

                if (!TestWin(Player))
                {
                    //блокируем поле чтобы игрок не смог ходить
                    LoockField();

                    //if (Computer == 0)
                    //{
                        ReplyStep(ParsName[1]);
                        //ReceivedStep();
                    //}
                    //else  //Шаг ПК
                    //    PC_Step();

                    //пробуем разблокировать поле 
                    //UnLoockField();
                }
                else
                {
                    //panel4.Visible = true;
                    labelResult.Text = "Вы выиграли";
                    LoockField();
                }
            }
        }

        //void PC_Step()
        //{

        //    //объявляем функцию генерации случайных чисел 
        //    Random Rand = new Random();
        //    GENER:

        //    if (CanStap())
        //    {
        //        //получаем число от 0 до 8
        //        int IndexStep = Rand.Next(0, 8);

        //        //смотрим если ячейка пуста то ставим туда символ ПК
        //        if (GameFieldMap[IndexStep] == 0)
        //        {
        //            //рисуем нужную картинку
        //            GameField[IndexStep].Image = Image.FromFile(ImgName[Computer]);
        //            //записываем в поле игры ход компьютера
        //            GameFieldMap[IndexStep] = Computer;

        //        }
        //        else
        //            goto GENER;
        //        if (TestWin(Computer))
        //        {
        //            //panel4.Visible = true;
        //            labelResult.Text = "Вы Проиграли";
        //        }
        //    }
        //}

        bool TestWin(int WHO)
        {
            //список вариантов выигрышных комбинаций
            int[,] WinVariant =
            {      {    //1 вариант
                    1,1,1,  //Х Х Х
                    0,0,0,  //_ _ _
                    0,0,0   //_ _ _
                },
                {    //2 вариант
                    0,0,0,  //_ _ _
                    1,1,1,  //Х Х Х
                    0,0,0   //_ _ _
                },
                {    //3 вариант
                    0,0,0,  //_ _ _
                    0,0,0,  //_ _ _
                    1,1,1   //Х Х Х
                },
                {    //4 вариант
                    1,0,0,  //Х _ _
                    1,0,0,  //Х _ _
                    1,0,0   //Х _ _
                },
                {    //5 вариант
                    0,1,0,  //_ Х _
                    0,1,0,  //_ Х _
                    0,1,0   //_ Х _
                },
                {    //6 вариант
                    0,0,1,  //_ _ Х
                    0,0,1,  //_ _ Х
                    0,0,1   //_ _ Х
                },
                {    //7 вариант
                    1,0,0,  //Х _ _
                    0,1,0,  //_ Х _
                    0,0,1   //_ _ Х
                },
                {    //8 вариант
                    0,0,1,   //_ _ Х
                    0,1,0,   //_ Х _
                    1,0,0    //Х _ _
                }
            };

            //получаем  поле
            int[] TestMap = new int[GameFieldMap.Length];
            //просчитываем поле
            for (int I = 0; I < GameFieldMap.Length; I++)
                //если номер в ячейке нам подходит записываем в карту 1
                if (GameFieldMap[I] == WHO) TestMap[I] = 1;

            //выбираем вариант для сравнения 
            for (int Variant_Index = 0; Variant_Index < WinVariant.GetLength(0); Variant_Index++)
            {
                //счетчик для подсчета соотвествий
                int WinState = 0;
                for (int TestIndex = 0; TestIndex < TestMap.Length; TestIndex++)
                {
                    //если параметр равен 1 то проверяем его иначе 0 тоже = 0
                    if (WinVariant[Variant_Index, TestIndex] == 1)
                    {
                        //если в параметр  в варианте выигрыша совпал с вариантом на карте считаем это в параметре WinState
                        if (WinVariant[Variant_Index, TestIndex] == TestMap[TestIndex]) WinState++;
                    }
                    //если найдены 3 совпадения значит это и есть выигрышная комбинация
                    if (WinState == 3) return true;
                }

            }
            return false;
        }

        bool CanStap()
        {
            //перебираем все значения игрового поля
            foreach (int s in GameFieldMap)
                //если нашли 0 значит есть куда ходить
                if (s == 0) return true;

            //проверяем не выиграл ли игрок
            if (TestWin(Player))
            {
                labelResult.Text = "Вы выиграли";
                LoockField();
                //если не нашли то ходить больше нельзя
                return false;
            }
            //проверяем не выиграл ли игрок
            if (TestWin(Computer))
            {
                labelResult.Text = "Вы проиграли";
                //прячем панель игры
                LoockField();
                return false;
            }
            //если ходить больше нельзя и никто не выиграл значит пишем что ничья
            labelResult.Text = "Ничья";
            LoockField();

            return false;
        }

        private void ToolStripMenuItemNewGame(object sender, EventArgs e)
        {
            NewField();
        }

        void NewField ()
        {
            //обнуляем карту игры
            GameFieldMap = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //обнуляем изображение поля
            foreach (PictureBox P in GameField) P.Image = Image.FromFile(ImgName[0]);
            //пробуем разблокировать поле 
            UnLoockField();
            labelResult.Text = " ";
        }

        private void ToolStripMenuItemCreateServe_Click(object sender, EventArgs e)
        {
            Thread myThread = new Thread(new ThreadStart(CreateServer));
            myThread.Start();
            ToolStripMenuItemCreateServe.Visible = false;
            NewField();
            panelGameField.Visible = true;
            LoockField();
            //Thread startReceived = new Thread(new ThreadStart(ReceivedStep));
            //startReceived.Start();
            Player = 1;
            Enemy = 2;
        }

        private void ToolStripMenuItemConnection_Click(object sender, EventArgs e)
        {
            CreateClient();
            ToolStripMenuItemConnection.Visible = false;
            //ToolStripMenuItemNewGame(sender, e);
            NewField();
            panelGameField.Visible = true;
            Player = 2;
            Enemy = 1;
            LoockField();
            Thread myThread = new Thread(new ThreadStart(ReceivedStep));
            myThread.Start();
            //ReceivedStep();
        }


        #region ServerClient
        private void CreateServer()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 2112));
            listener.Listen(0);
            try
            {
                socket = listener.Accept();

                ReceivedMessage();
                UnLoockField();
                ReplyMessage("Serve");
                ReceivedStep();

            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CloseServer()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            listener.Close();
            ToolStripMenuItemCreateServe.Visible = true;
        }

        private void CreateClient()
        {
            IPHostEntry lpHost = Dns.Resolve("127.0.0.1");
            IPAddress ipAddress = lpHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 2112);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEndPoint);
            ReplyMessage("Client");
            ReceivedMessage();
        }

        private void ReplyMessage(string name)  //отправка сообщения сервероу
        {
            string sendingMessage = name;
            byte[] forwardMessage = Encoding.ASCII.GetBytes(sendingMessage);
            socket.Send(forwardMessage);
        }

        private void ReceivedMessage()  // получение сообщения от сервера
        {
            byte[] receivedBytes = new byte[1024];
            int totalBytesReceived = socket.Receive(receivedBytes);

            Invoke((MethodInvoker)delegate { this.labelPlayer2.Text = Encoding.ASCII.GetString(receivedBytes, 0, totalBytesReceived); });
        }

        private void ReplyStep(string name)  //отправка сообщения сервероу
        {
            string sendingMessage = name;
            byte[] forwardMessage = Encoding.ASCII.GetBytes(sendingMessage);
            socket.Send(forwardMessage);
        }

        private void ReceivedStep()  // получение сообщения от сервера
        {
            byte[] receivedBytes = new byte[1024];
            while (true)
            {
                int totalBytesReceived = socket.Receive(receivedBytes);
                int IndexSelectImage = Convert.ToInt32(Encoding.ASCII.GetString(receivedBytes, 0, totalBytesReceived));
                if (IndexSelectImage != 9)
                {
                    GameField[IndexSelectImage].Image = Image.FromFile(ImgName[Enemy]);
                    GameFieldMap[IndexSelectImage] = Enemy;
                    UnLoockField();
                }
                else
                    NewField();
            }
        }

        private void CloseClient()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        #endregion
    }
}
