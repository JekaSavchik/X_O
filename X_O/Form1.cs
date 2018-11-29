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
        int Player, Enemy;

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

        void LockedField()
        {
            //блокируем все поле чтобы игрок не мог на него нажать
            foreach (PictureBox P in GameField)
                Invoke((MethodInvoker)delegate { P.Enabled = false; });
            labelPlayer1.BackColor = Color.Blue;
            labelPlayer2.BackColor = Color.Green;
        }

        void UnLockedField()
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
            labelPlayer1.BackColor = Color.Green;
            labelPlayer2.BackColor = Color.Blue;
        }

        private void Picture_Click(object sender, EventArgs e)
        {
            PictureBox ClickImage = sender as PictureBox;
            string[] ParsName = ClickImage.Name.Split('_');

            int IndexSelectImage = Convert.ToInt32(ParsName[1]);

            GameField[IndexSelectImage].Image = Image.FromFile(ImgName[Player]);
            GameFieldMap[IndexSelectImage] = Player;

            LockedField();
            ReplyStep(ParsName[1]);
            CanStep();
        }

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

        void CanStep()
        {
            //проверяем не выиграл ли игрок
            if (TestWin(Player))
            {
                labelResult.Text = "Вы выиграли";
                labelResult.BackColor = Color.Green;
                LockedField();
                //если не нашли то ходить больше нельзя
                return;
            }
            //проверяем не выиграл ли игрок
            else if (TestWin(Enemy))
            {
                Invoke((MethodInvoker)delegate { this.labelResult.Text = "Вы проиграли"; });
                labelResult.BackColor = Color.Red;
                //прячем панель игры
                LockedField();
                return;
            }

            //перебираем все значения игрового поля
            foreach (int s in GameFieldMap)
                //если нашли 0 значит есть куда ходить
                if (s == 0) return;

            //если ходить больше нельзя и никто не выиграл значит пишем что ничья
            labelResult.BackColor = Color.PeachPuff;
            labelResult.Text = "Ничья";
            LockedField();

            //return false;
        }

        private void ToolStripMenuItemNewGame(object sender, EventArgs e)
        {
            NewField();
            if (Player == 1)
                ReplyStep("9");
        }

        void NewField()
        {
            //обнуляем карту игры
            GameFieldMap = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //обнуляем изображение поля
            foreach (PictureBox P in GameField) P.Image = Image.FromFile(ImgName[0]);
            //пробуем разблокировать поле 
            UnLockedField();
            labelResult.Text = " ";
            labelResult.BackColor = Color.LightGray;
        }

        private void ToolStripMenuItemCreateServe_Click(object sender, EventArgs e)
        {
            Thread myThread = new Thread(new ThreadStart(CreateServer));
            myThread.Start();
            новаяИграToolStripMenuItem.Enabled = true;
            ToolStripMenuItemCreateServe.Enabled = false;
            NewField();
            panelGameField.Visible = true;
            LockedField();
            Player = 1;
            Enemy = 2;
        }

        private void ToolStripMenuItemConnection_Click(object sender, EventArgs e)
        {
                CreateClient();
                ToolStripMenuItemConnection.Enabled = false;
                NewField();
                panelGameField.Visible = true;
                Player = 2;
                Enemy = 1;
                LockedField();
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

                Invoke((MethodInvoker)delegate { this.toolStripStatusLabel.BackColor = Color.Green; });
                Invoke((MethodInvoker)delegate { this.toolStripStatusLabel.Text = "Подключено"; });
                ReceivedMessage();
                UnLockedField();
                ReplyMessage(labelPlayer1.Text);
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
            ToolStripMenuItemCreateServe.Enabled = true;
        }

        private void CreateClient()
        {
            IPHostEntry lpHost = Dns.Resolve("127.0.0.1");
            IPAddress ipAddress = lpHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 2112);
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipEndPoint);

                toolStripStatusLabel.BackColor = Color.Green;
                toolStripStatusLabel.Text = "Подключено";
                ReplyMessage(labelPlayer1.Text);
                ReceivedMessage();
                Thread myThread = new Thread(new ThreadStart(ReceivedStep));
                myThread.Start();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                labelPlayer1.Text = textBox1.Text;
                panelStart.Visible = false;
            }
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
                    UnLockedField();

                    CanStep();
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
