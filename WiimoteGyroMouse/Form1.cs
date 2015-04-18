using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WiimoteLib;                       //WimoteLibの宣言
using System.Runtime.InteropServices;  //DllImportを使うための宣言


namespace WiimoteGyroMouse
{
    public partial class Form1 : Form {


        //DLL読み込み用
        [DllImport("user32.dll")]
        extern static uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        //DLL読み込み用
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        //DLL読み込み用
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }



        // Win32APIを呼び出すためのクラス
        public class win32api
        {
            [DllImport("user32.dll")]
            public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        }



        Boolean isADown = false;                                     //Aボタンが押されたか判定フラグ
        Boolean isBDown = false;                                     //Bボタンが押されたか判定フラグ
        Boolean isHomeDown = false;
        Boolean isMinusDown = false;
        Boolean isPlusDown = false;
        Boolean isUpDown = false;
        Boolean isDownDown = false;
        Boolean isLeftDown = false;
        Boolean isRightDown = false;
        Boolean isOneDown = false;
        Boolean isTwoDown = false;


        //仮想キーコード
        Byte VK_UP = 0x26;
        Byte VK_DOWN = 0x28;
        Byte VK_LEFT = 0x25;
        Byte VK_RIGHT = 0x27;
        Byte VK_ENTER = 0x0D;
        Byte VK_BS = 0x08;
        Byte VK_WINDOWS = 0x5B;
        Byte VK_ALT = 0xA4;



        Wiimote wm = new Wiimote();                                  //Wiimoteの宣言

        public Form1()
        {

            InitializeComponent();

            //他スレッドからのコントロール呼び出し許可
            Control.CheckForIllegalCrossThreadCalls = false;
        }


        //接続・切断ボタンが押されたら
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (button1.Text.Equals("CONNECT"))
            {
                wm.Connect();                                        //Wiimoteの接続
                wm.WiimoteChanged += wm_WiimoteChanged;              //イベント関数の登録
                wm.SetReportType(InputReport.IRExtensionAccel, true);//レポートタイプの設定
                wm.InitializeMotionPlus();

                button1.Text = "DISCONNECT";
                label1.Text = "GYRO DISABLED";

                wm.SetLEDs(1);

            }
            else
            {
                wm.SetLEDs(0);

                wm.Disconnect();                                     //Wiimote切断
                button1.Text = "CONNECT";
                label1.Text = "";
            }

        }


        //Wiiリモコンの値が変化する度に呼ばれる
        private void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiiControl(args.WiimoteState);
        }


        private void GyroMouse(WiimoteState ws)
        {
            //マウスカーソルを指定位置へ移動
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X + getGyroX(ws), System.Windows.Forms.Cursor.Position.Y + getGyroZ(ws));
        }


//        static const int GYRO_NEUTRAL_X = 7859;
        private int getGyroX(WiimoteState ws) {

            int rawValue = 7859 - ws.MotionPlusState.RawValues.X;
            int sign = rawValue < 0 ? -1 : 1;

            return sign * (int)(120.0 * Math.Pow((double)Math.Abs(rawValue) / 7859.0, 1.3));
        }



//        static const int GYRO_NEUTRAL_Y = 7893;
        private int getGyroY(WiimoteState ws)
        {
            int rawValue = 7893 - ws.MotionPlusState.RawValues.Y;
            int sign = rawValue < 0 ? 1 : -1;

            return sign * (int)(120.0 * Math.Pow((double)Math.Abs(rawValue) / 7893.0, 1.3));
        }


//        static const int GYRO_NEUTRAL_Z = 7825;
        private int getGyroZ(WiimoteState ws)
        {
            int rawValue = 7825 - ws.MotionPlusState.RawValues.Z;
            int sign = rawValue < 0 ? 1 : -1;

            return sign * (int)(120.0 * Math.Pow((double)Math.Abs(rawValue) / 7825.0, 1.3));
        }



        public void WiiControl(WiimoteState ws)
        {

            INPUT[] input = new INPUT[1];                             //マウスイベントを格納


            if (ws.ButtonState.B)
                GyroMouse(ws);


            //Aボタン処理
            if (ws.ButtonState.A)
            {
                //もしAボタンが押されたら
                if (!isADown)
                {
                    input[0].mi.dwFlags = 0x0002;                       //左マウスダウン
                    SendInput(1, input, Marshal.SizeOf(input[0]));      //マウスイベントを送信
                    isADown = true;
                    label1.Text = "MOUSE LEFT";
                }
            }
            else
            {
                if (isADown)
                {
                    //もしAボタンが押されていて離されたら
                    isADown = false;

                    input[0].mi.dwFlags = 0x0004;                      //左マウスアップ
                    SendInput(1, input, Marshal.SizeOf(input[0]));     //マウスイベントを送信
                    label1.Text = "";
                }
            }

            //Bボタン処理
            if (ws.ButtonState.One)
            {
                //もしOneボタンが押されたら

                if (!isOneDown)
                {

                    input[0].mi.dwFlags = 0x0008;                     //右マウスダウン
                    SendInput(1, input, Marshal.SizeOf(input[0]));    //マウスイベントを送信

                    isOneDown = true;
                    label1.Text = "MOUSE RIGHT";
                }
            }
            else
            {
                if (isOneDown)
                {
                    //もしOneボタンが押されていて離されたら
                    isOneDown = false;
                    input[0].mi.dwFlags = 0x0010;                    //右マウスアップ
                    SendInput(1, input, Marshal.SizeOf(input[0]));   //マウスイベントを送信
                    label1.Text = "";
                }
            }



            if (ws.ButtonState.Up)
            {

                if (!isUpDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_UP, 0, 0, (UIntPtr)0);
                    isUpDown = true;
                    label1.Text = "↑";
                }
            }
            else
            {
                if (isUpDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_UP, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isUpDown = false;

                }
            }

            if (ws.ButtonState.Down)
            {

                if (!isDownDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_DOWN, 0, 0, (UIntPtr)0);
                    isDownDown = true;
                    label1.Text = "↓";
                }
            }
            else
            {
                if (isDownDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_DOWN, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isDownDown = false;

                }
            }


            if (ws.ButtonState.Right)
            {

                if (!isRightDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_RIGHT, 0, 0, (UIntPtr)0);
                    isRightDown = true;
                    label1.Text = "→";
                }
            }
            else
            {
                if (isRightDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_RIGHT, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isRightDown = false;

                }
            }


            if (ws.ButtonState.Left)
            {

                if (!isLeftDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_LEFT, 0, 0, (UIntPtr)0);
                    isLeftDown = true;
                    label1.Text = "←";
                }
            }
            else
            {
                if (isLeftDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_LEFT, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isLeftDown = false;

                }
            }


            if (ws.ButtonState.Plus)
            {

                if (!isPlusDown)
                {
                    isPlusDown = true;
                    label1.Text = "ENTER";
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_ENTER, 0, 0, (UIntPtr)0);
                }
            }
            else
            {
                if (isPlusDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_ENTER, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isPlusDown = false;

                }
            }


            if (ws.ButtonState.Minus)
            {

                if (!isMinusDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_BS, 0, 0, (UIntPtr)0);
                    isMinusDown = true;
                    label1.Text = "BACK SPACE";
                }
            }
            else
            {
                if (isMinusDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_BS, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isMinusDown = false;

                }
            }

            if (ws.ButtonState.Home)
            {

                if (!isHomeDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_WINDOWS, 0, 0, (UIntPtr)0);
                    isHomeDown = true;
                    label1.Text = "WINDOWS";
                }
            }
            else
            {
                if (isHomeDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_WINDOWS, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isHomeDown = false;

                }
            }


            if (ws.ButtonState.Two)
            {

                if (!isTwoDown)
                {
                    // キーの押し下げをシミュレートする。
                    win32api.keybd_event(VK_ALT, 0, 0, (UIntPtr)0);
                    isTwoDown = true;
                    label1.Text = "ALT";
                }
            }
            else
            {
                if (isTwoDown)
                {
                    // キーの解放をシミュレートする。
                    win32api.keybd_event(VK_ALT, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    win32api.keybd_event(VK_DOWN, 0, 0, (UIntPtr)0);
                    win32api.keybd_event(VK_DOWN, 0, 2/*KEYEVENTF_KEYUP*/, (UIntPtr)0);
                    label1.Text = "";
                    isTwoDown = false;

                }
            }

            
            if (ws.ButtonState.B)
            {

                if (!isBDown)
                {
                    isBDown = true;
                    label1.Text = "GYRO ENABLED";
                }
            }
            else
            {
                if (isBDown)
                {
                    label1.Text = "GYRO DISABLED";
                    isBDown = false;
                }
            }
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
