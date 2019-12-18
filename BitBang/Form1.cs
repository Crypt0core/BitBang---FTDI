using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;

using QRCoder;

using FTD2XX_NET;

namespace BitBang
{
    public partial class Form1 : Form
    {
        // Create new instance of the FTDI device class
        FTDI myFtdiDevice = new FTDI();
        UInt32 ftdiDeviceCount = 0;
        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
        FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = null;
        Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        sc55 scrSmns; // creating null object

        public Form1()
        {
            InitializeComponent();
            FormClosing += Form1_FormClosing;
            checkBoxesRW = new CheckBox[] { cbRW0, cbRW1, cbRW2, cbRW3, cbRW4, cbRW5, cbRW6, cbRW7 };
            checkBoxesPS = new CheckBox[] { cbPS0, cbPS1, cbPS2, cbPS3, cbPS4, cbPS5, cbPS6, cbPS7 };

            foreach (var cbRW in checkBoxesRW)
            {
                cbRW.CheckedChanged += new EventHandler(CheckedRW);
            }
            foreach (var cbST in checkBoxesPS)
            {
                cbST.CheckedChanged += new EventHandler(CheckedST);
            }
            CheckedRW(null, null);
            CheckedST(null, null);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dlgresult = MessageBox.Show("Exit or no?",
                                        "Exit?",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information);
            if (dlgresult == DialogResult.No)
            {
                e.Cancel = true;

            }
            else
            {
                e.Cancel = false;
            }
        }

        CheckBox[] checkBoxesRW;
        CheckBox[] checkBoxesPS;

        private byte bufRW
        {
            get
            {
                bool[] vs = new bool[8];
                for (int i = 0; i < 8; i++) vs[i] = checkBoxesRW[i].Checked;
                return ConvertBoolArrayToByte(vs);
            }
            set
            {
                bool[] vs = ConvertByteToBoolArray(value);
                for (int i = 0; i < 8; i++) checkBoxesRW[i].Checked = vs[i];
            }
        }

        private byte bufPS
        {
            get
            {
                bool[] vs = new bool[8];
                for (int i = 0; i < 8; i++) vs[i] = checkBoxesPS[i].Checked;
                return ConvertBoolArrayToByte(vs);
            }
            set
            {
                bool[] vs = ConvertByteToBoolArray(value);
                for (int i = 0; i < 8; i++) checkBoxesPS[i].Checked = vs[i];
            }
        }
        /*
        List<byte> _display = new List<byte> { };

        private byte display
        {
            get
            {
                return _display[_display.Count - 1];
            }
            set
            {
                _display.Add(value);
            }
        }
        */
        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (index)); //(7 - index)

                index++;
            }

            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            //Array.Reverse(result);

            return result;
        }

        void CheckedRW(object sender, EventArgs e)
        {
            int allChecked = 0;
            foreach (var cbO in checkBoxesRW)
            {
                if (cbO.Checked) allChecked++;
            }
            switch (allChecked)
            {
                case 0: crossRW.Enabled = false; break;
                case 8: checkRW.Enabled = false; break;
                default: checkRW.Enabled = true; crossRW.Enabled = true; break;
            }
        }
        void CheckedST(object sender, EventArgs e)
        {
            int allChecked = 0;
            foreach (var cbO in checkBoxesPS)
            {
                if (cbO.Checked) allChecked++;
            }
            switch (allChecked)
            {
                case 0: crossPS.Enabled = false; break;
                case 8: checkPS.Enabled = false; break;
                default: crossPS.Enabled = true; checkPS.Enabled = true; break;
            }
        }

        private void Log(string s)
        {
            textBox1.AppendText(s + Environment.NewLine);
        }

        private void GetIdFTDI()
        {
            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                Log("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                Log("");
                Text = "BitBang - Devices found " + ftdiDeviceCount.ToString();
            }
            else
            {
                // Wait for a key press
                Log("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Log("");
                Text = "BitBang - Error";
                connectBtn.Enabled = false;
                setBtn.Enabled = false;
                comboBox1.Enabled = false;
                return;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                //Log("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                Log("Failed to get IDs of devices");
                Text = "BitBang - No devices";
                connectBtn.Enabled = false;
                setBtn.Enabled = false;
                comboBox1.Enabled = false;
                Log("_____________________________________");
                return;
            }
            else
            {
                comboBox1.Enabled = true;
                // Allocate storage for device info list
                //FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
                ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

                // Populate our device list
                ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);
                comboBox1.Items.Clear();
                foreach (FTDI.FT_DEVICE_INFO_NODE device in ftdiDeviceList)
                {
                    comboBox1.Items.Add(device.LocId);
                }
                if (ftdiDeviceCount == 1) comboBox1.Text = ftdiDeviceList[0].LocId.ToString();
                //ftStatus = myFtdiDevice.OpenByIndex
                if (ftStatus == FTDI.FT_STATUS.FT_OK)
                {
                    for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                    {
                        Log("Device Index: " + i.ToString());
                        Log("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                        Log("Type: " + ftdiDeviceList[i].Type.ToString());
                        Log("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                        Log("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                        Log("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                        Log("Description: " + ftdiDeviceList[i].Description.ToString());
                        Log("");
                    }
                }
                connectBtn.Enabled = true;
            }

            Log("_____________________________________");
        }

        private void OpenIdFTDI()
        {
            if (ftdiDeviceList != null)
            {
                // Open first device in our list by serial number
                //ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
                //ftStatus = myFtdiDevice.OpenBySerialNumber(comboBox1.Text);
                ftStatus = myFtdiDevice.OpenByLocation(Convert.ToUInt32(comboBox1.Text));
                Log("Opening device " + comboBox1.Text);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    Log("Failed to open device (error " + ftStatus.ToString() + ")");
                    Text = "BitBang - Failed to open";
                    return;
                }
                else
                {
                    Log("Device opened succesfully");
                }

            }
            else Log("NULL");
            Log("_____________________________________");
        }

        private static void EnableTab(TabPage page, bool enable)
        {
            foreach (Control ctl in page.Controls) ctl.Enabled = enable;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Text = ConfigurationManager.AppSettings["dev"];
            comboBox2.Text = ConfigurationManager.AppSettings["spd"];
            bufRW = Convert.ToByte(ConfigurationManager.AppSettings["rw"]);
            bufPS = Convert.ToByte(ConfigurationManager.AppSettings["ps"]);
            numericUpDown1.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["dc"]);
            numericUpDown2.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["ce"]);
            numericUpDown3.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["data"]);
            numericUpDown4.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["rst"]);
            numericUpDown5.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["clk"]);
            numericUpDown6.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["dt"]);
            Text = "BitBang - Starting";
            GetIdFTDI();
            comboBox2.Items.AddRange(new object[] { 184, 200, 300, 600, 1200, 2400, 4800, 9600, 19200,
                38400, 57600, 115200, 230400, 460800, 921600, 1000000, 1500000, 2000000, 2500000, 3000000 });
        }

        private void scanBtn_Click(object sender, EventArgs e)
        {
            GetIdFTDI();
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (!myFtdiDevice.IsOpen)
            {
                comboBox1.Enabled = false;
                connectBtn.Text = "Close";
                scanBtn.Enabled = false;
                OpenIdFTDI();
                setBtn.Enabled = true;
                setRW.Enabled = true;
                readPS.Enabled = true;
                writePS.Enabled = true;
                rstBtn.Enabled = true;
                groupBox1.Enabled = true;
                comboBox2_SelectedIndexChanged(sender, e);
            }
            else
            {
                myFtdiDevice.Close();
                groupBox1.Enabled = false;
                connectBtn.Text = "Open";
                scanBtn.Enabled = true;
                setBtn.Enabled = false;
                comboBox1.Enabled = true;
                setRW.Enabled = false;
                readPS.Enabled = false;
                writePS.Enabled = false;
                rstBtn.Enabled = false;
                if (button6.Text == "Kill") button6_Click(sender, e);
                Log("Device closed");
                Log("_____________________________________");
            }
        }

        private void setBtn_Click(object sender, EventArgs e)
        {
            ftStatus = myFtdiDevice.SetBaudRate(Convert.ToUInt32(comboBox2.Text));
            Log("Setting baudrate to " + comboBox2.Text + " with status: " + ftStatus.ToString());
            Log("_____________________________________");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentConfig.AppSettings.Settings["dev"].Value = comboBox1.Text;
            currentConfig.Save(ConfigurationSaveMode.Modified);
            //button2_Click(sender, e);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentConfig.AppSettings.Settings["spd"].Value = comboBox2.Text;
            currentConfig.Save(ConfigurationSaveMode.Modified);
            setBtn_Click(sender, e);
        }

        private void checkRW_Click(object sender, EventArgs e)
        {
            /*foreach (var cbO in checkBoxesRW)
            {
                cbO.Checked = true;
            }*/
            bufRW = 0xff;
        }

        private void crossRW_Click(object sender, EventArgs e)
        {
            /*foreach (var cbO in checkBoxesRW)
            {
                cbO.Checked = false;
            }*/
            bufRW = 0x00;
        }

        private void crossPS_Click(object sender, EventArgs e)
        {
            /*foreach (var cbO in checkBoxesPS)
            {
                cbO.Checked = false;
            }*/
            bufPS = 0x00;
        }

        private void checkPS_Click(object sender, EventArgs e)
        {
            /*foreach (var cbO in checkBoxesPS)
            {
                cbO.Checked = true;
            }*/
            bufPS = 0xff;
        }

        private void setRW_Click(object sender, EventArgs e)
        {
            SetRW(bufRW);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            currentConfig.AppSettings.Settings["rw"].Value = bufRW.ToString();
            currentConfig.AppSettings.Settings["ps"].Value = bufPS.ToString();
            currentConfig.Save(ConfigurationSaveMode.Modified);
            Text = "BitBang - Saved settings";
        }

        private void readPS_Click(object sender, EventArgs e)
        {
            if (myFtdiDevice.IsOpen)
            {
                UInt32 readedBytes = 0;
                uint len = 1;
                byte[] buf = new byte[len];
                ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
                Log("Clearing input buffer with status: " + ftStatus.ToString());
                ftStatus = myFtdiDevice.Read(buf, len, ref readedBytes);
                Log("Read " + readedBytes.ToString() + " with status " + ftStatus.ToString());
                bufPS = buf[0];
            }
            else Log("Closed!");
        }

        private void writePS_Click(object sender, EventArgs e)
        {
            uint len = WritePsBuf(new byte[] { bufPS });
            Log("Wrote " + len.ToString() + " bytes");
        }

        private void rstBtn_Click(object sender, EventArgs e)
        {
            if (myFtdiDevice.IsOpen)
            {
                Log("_____________________________________");
                rstBtn.Enabled = false;
                ftStatus = myFtdiDevice.CyclePort();
                Text = "BitBang - Restarting device";
                Log("Restarting device with status: " + ftStatus.ToString());
                connectBtn.Text = "Open";
                scanBtn.Enabled = true;
                setBtn.Enabled = false;
                comboBox1.Enabled = true;
                setRW.Enabled = false;
                readPS.Enabled = false;
                writePS.Enabled = false;
                Log("Device closed");
                Log("_____________________________________");
                //GetIdFTDI();
            }
        }

        private void SetRW(byte rw)
        {
            if (myFtdiDevice.IsOpen)
            {
                ftStatus = myFtdiDevice.SetBitMode(rw, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
                Log("Setting mode status: " + ftStatus.ToString());
            }
            else Log("Error: Device is closed!");
        }

        private uint WritePsBuf(byte[] stArr)
        {
            if (myFtdiDevice.IsOpen)
            {
                uint writtenBytes = 0;
                //ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);
                ftStatus = myFtdiDevice.Write(stArr, stArr.Length, ref writtenBytes);
                return writtenBytes;
            }
            else Log("Closed!");
            return 0;
        }

        private void WriteDisp()
        {
            WritePsBuf(scrSmns._LCD_PORT.ToArray());
            scrSmns.ListClear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (scrSmns == null)
            {
                currentConfig.AppSettings.Settings["dc"].Value = numericUpDown1.Value.ToString();
                currentConfig.AppSettings.Settings["ce"].Value = numericUpDown2.Value.ToString();
                currentConfig.AppSettings.Settings["data"].Value = numericUpDown3.Value.ToString();
                currentConfig.AppSettings.Settings["rst"].Value = numericUpDown4.Value.ToString();
                currentConfig.AppSettings.Settings["clk"].Value = numericUpDown5.Value.ToString();
                currentConfig.AppSettings.Settings["dt"].Value = numericUpDown6.Value.ToString();
                currentConfig.Save(ConfigurationSaveMode.Modified);
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
                numericUpDown5.Enabled = false;
                numericUpDown6.Enabled = false;
                scrSmns = new sc55((byte)numericUpDown6.Value, (byte)numericUpDown1.Value,
                                        (byte)numericUpDown2.Value, (byte)numericUpDown3.Value,
                                            (byte)numericUpDown4.Value, (byte)numericUpDown5.Value);
                groupBox2.Enabled = true;
                EnableTab(tabPage1, false);
                button6.Text = "Kill";
                Text = "BitBang - Display Init";
                scrSmns.LcdInit();
                SetRW(scrSmns.LCD_DDR);
                WriteDisp();
                Log("Display inited");
            }
            else
            {
                groupBox2.Enabled = false;
                SetRW(0);
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
                numericUpDown6.Enabled = true;
                scrSmns = null;
                Text = "BitBang - Display instance Killed";
                button6.Text = "Init";
                EnableTab(tabPage1, true);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                scrSmns.LcdClear();
                scrSmns.LcdUpdate();
                Log("Display cleared");
                button11_Click(sender, e);
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                scrSmns.LcdContrast((byte)numericUpDown7.Value);
                //scrSmns.LcdStr(1, "\x21\x30");
                Log("Contrast setted");
                button11_Click(sender, e);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                scrSmns.LcdGotoXYFont((byte)numericUpDown8.Value, (byte)numericUpDown9.Value);
                Log("Cursor setted");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.Text = "Big";
                numericUpDown9.Minimum = 1;
                textBox2.Font = new System.Drawing.Font("Lucida Console", 17F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            }
            else
            {
                checkBox1.Text = "Small";
                numericUpDown9.Minimum = 0;
                textBox2.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                button9_Click(sender, e);
                if (!checkBox1.Checked) scrSmns.LcdStr(1, textBox2.Text);
                else scrSmns.LcdStr(2, textBox2.Text);
                Log("Writing text");
                button11_Click(sender, e);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                Log("Writing...");
                scrSmns.LcdUpdate();
                WriteDisp();
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                //scrSmns.LcdCircle(50, 30, 10, 1);
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(textBox4.Text, QRCodeGenerator.ECCLevel.Q); // max symbols 127
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(1);//37*37
                Bitmap bitmap = qrCodeImage.Clone(new Rectangle(4, 4, qrCodeImage.Width - 8, qrCodeImage.Height - 8), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                if (bitmap.Height <= 21)
                {
                    qrCodeImage = qrCode.GetGraphic(3);//37*37
                    bitmap = qrCodeImage.Clone(new Rectangle(12, 12, qrCodeImage.Width - 24, qrCodeImage.Height - 24), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                }
                else
                if (bitmap.Height <= 33)
                {
                    qrCodeImage = qrCode.GetGraphic(2);//37*37
                    bitmap = qrCodeImage.Clone(new Rectangle(8, 8, qrCodeImage.Width - 16, qrCodeImage.Height - 16), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                }
                pictureBox2.Image = bitmap;
                Log(bitmap.Size.ToString());
                List<byte> map = new List<byte>();
                (int _h, int _w) = (bitmap.Height, bitmap.Width);
                Log("");
                int __w = (102 - _w) / 2;
                for (int j = 0; j < _h; j += 8)
                {
                    for (int i = 0; i < __w; i++) map.Add(0);
                    for (int i = 0; i < 102 - __w; i++)
                    {
                        byte clr = 0;
                        if (i < _w)
                            for (byte k = 0; k < 8; k++)
                            {
                                clr |= (byte)((((j + k < _h) ? (bitmap.GetPixel(i, j + k).B == Color.Black.B) : false) ? 1 : 0) << k);
                            }
                        map.Add(clr);
                    }
                }
                byte[] buf = map.ToArray();
                scrSmns.LcdImage(buf);
                Log("Writing BITMAP");
                button11_Click(sender, e);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                byte[] buf = new byte[102 * 64 / 8];
                Random random = new Random();
                random.NextBytes(buf);
                scrSmns.LcdImage(buf);
                Log("Random!");
                button11_Click(sender, e);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (scrSmns != null)
            {
                scrSmns.InvertDisplay();
                Log("Inverting");
                button11_Click(sender, e);
            }
        }
    }
}
