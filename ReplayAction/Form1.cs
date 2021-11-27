using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ReplayAction.MyHook;


namespace ReplayAction
{
    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents m_GlobalHook;
        List<string> EditTemp = new List<string>();
        public void Edit()
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.KeyDown += (s, e) =>
            {
                pButton.Add(e.KeyValue);
                pressStop = false;
                if (!pressTime.Enabled)
                    pressTime.Start();
            };
        }
        private void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            //e.Button
            m_GlobalHook.MouseDownExt += (s, e) => listBox1.Items.Add($"Click | {e.Location.X};{e.Location.Y}");

            string HoldLocation = "";
            m_GlobalHook.MouseDragStartedExt += (s, e) => HoldLocation = $"{e.Location.X};{e.Location.Y}";
            m_GlobalHook.MouseDragFinishedExt += (s, e) => 
            { 
                listBox1.Items.RemoveAt(listBox1.Items.Count - 1);
                listBox1.Items?.Add($"Hold | {HoldLocation};{e.Location.X};{e.Location.Y}"); 
            };
            m_GlobalHook.KeyDown += (s, e) => 
            {
                pButton.Add(e.KeyValue);
                pressStop = false;
                if(!pressTime.Enabled)
                    pressTime.Start();
            };
        }
        List<int> pButton = new List<int>();
        bool pressStop = false;
        void PressingButton()
        {
            if(pButton.Count > 0)
            {
                string temp = "";
                foreach (var item in pButton)
                {
                    temp += $"{item}, ";
                }
                if (textBox1.Focused)
                    textBox1.Text = $"{temp}";
                else
                listBox1.Items.Add($"Press | {temp}");
                pButton.Clear();
            }
        }
        private void pressTime_Tick(object sender, EventArgs e)
        {
            if (pressStop)
            {
                PressingButton();
                pressTime.Stop();
            }
            pressStop = true;
        }
        public Form1()
        {
            InitializeComponent();
            for (int i = 0; i < action.Length; i++)
            {
                bool skip = false;
                for (int j = i + 1; j < action.Length; j++)
                {
                    if(action[i].Name == action[j].Name)
                    {
                        skip = true; break;
                    }
                }
                if (!skip)
                {
                    comboBox1.Items.Add(action[i].Name);
                }

            }
        }
        private void Play_Click(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex != -1)
                parseComand(listBox1.Items[listBox1.SelectedIndex].ToString());
        }
        MyAction[] action = { 
            new MyAction("Click", 2, ClickMouse, typeof(int)), 
            new MyAction("Hold", 4, HoldMouse, typeof(int)), 
            new MyAction("Press", 1, SendKey, typeof(string)) };
        private void parseComand(string text)
        {
            if (text != "")
            {
                string[] res = text.Replace(';', ' ').Replace(",", "").Split();
                List<object> list = new List<object>();
                int p = 0;
                for (int i = 0; i < res.Length; i++)
                {
                    if (res[i].Contains("|"))
                        p = i;
                    if(i > p && res[i] != "")
                        list.Add(res[i]);
                }
                for (int i = 0; i < action.Length; i++)
                {
                    if (action[i].Name == res[0])
                        action[i].make(list);
                }
            }
        }
        private static void ClickMouse(List<object> param)
        {
            int X = Int32.Parse(param[0].ToString()), Y = Int32.Parse(param[1].ToString());
            SetCursorPos(X, Y);
            mouse_event(0x02, X, Y, 0, 0);
            mouse_event(0x04, X, Y, 0, 0);
        }
        private static void HoldMouse(List<object> param)
        {
            int X = Int32.Parse(param[0].ToString()), Y = Int32.Parse(param[1].ToString()),
                X2 = Int32.Parse(param[2].ToString()), Y2 = Int32.Parse(param[3].ToString());
            SetCursorPos(X, Y);
            mouse_event(0x02, X, Y, 0, 0);
            Thread.Sleep(10);
            SetCursorPos(X2, Y2);
            Thread.Sleep(10);
            mouse_event(0x04, X2, Y2, 0, 0);
        }

        private static void SendKey(List<object> param)
        {
            for (int i = 0; i < param.Count; i++)
            {
                keybd_event((byte)Int32.Parse(param[i].ToString()), 0, 0, 0);
            }
            for (int i = 0; i < param.Count; i++)
            {
                keybd_event((byte)Int32.Parse(param[i].ToString()), 0, 2, 0);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //timer1.Enabled = checkBox1.Checked;
            if (checkBox1.Checked)
            {
                Subscribe();
            }
            else
            {
                m_GlobalHook.Dispose();
            }
            UpdateButton(!checkBox1.Checked);
        }
        void UpdateButton(bool e)
        {
            //comboBox1.Enabled = e;
            //textBox1.Enabled = e;
            numericUpDown1.Enabled = e;
            checkBox2.Enabled = e;
            button1.Enabled = e;
            button2.Enabled = e;
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
                if (listBox1.Items.Count > 0)
                    listBox1.SelectedIndex = 0;
                UpdateButton(listBox1.Items.Count > 0);
            }
        }
        private void Change_Click(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && !CheckError())
            {
                listBox1.Items[listBox1.SelectedIndex] = $"{comboBox1.Text} | {textBox1.Text}";
            }
        }
        private void Add_Click(object sender, EventArgs e)
        {
            if (!CheckError())
            {
                string result = $"{comboBox1.Text} | {textBox1.Text}";
                if (listBox1.SelectedIndex == -1){
                    listBox1.Items.Add(result);
                }else
                    listBox1.Items.Insert(listBox1.SelectedIndex + 1, result);
            }
        }
        bool CheckError()
        {
            bool error = false;
            string[] res = textBox1.Text.Replace(";", " ").Replace(",", " ").Split();
            var arr = Array.Find(action, a => a.Name.Contains(comboBox1.Text));
            if (arr.type == typeof(int))
            {
                if (arr.reqParamNum != res.Length)
                    error = true;
                foreach (var r in res)
                {
                    try
                    {
                        if (Int32.Parse(r).GetType() != arr.type)
                        {
                            error = true;
                        }
                    }
                    catch (Exception)
                    {
                        error = true;
                    }
                }
            }
            return error;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked && listBox1.SelectedIndex != -1)
            {
                string[] res = listBox1.SelectedItem.ToString().Replace(" ", "").Split('|');
                comboBox1.Text = res[0];
                textBox1.Text = res[1];
            }
            else
            {
                textBox1.Text = null;
                comboBox1.Text = null;
                //listBox1.ClearSelected();
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (Array.Find(action, a => a.Name.Contains(comboBox1.Text)).type == typeof(string))
            {
                Edit();
                textBox1.ReadOnly = true;
            }
            else
            {
                if (m_GlobalHook != null)
                {
                    m_GlobalHook.Dispose();
                }
                textBox1.ReadOnly = false;
            }
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            if(m_GlobalHook != null)
            {
                m_GlobalHook.Dispose();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            PlayTime.Interval = (int)numericUpDown1.Value;
            if(listBox1.Items.Count <= 0 || checkBox1.Checked)
            {
                checkBox2.Checked = false;
            }
            else
            {
                listBox1.SelectedIndex = 0;
                PlayTime.Enabled = checkBox2.Checked;
            }
        }
        private void PlayTime_Tick(object sender, EventArgs e)
        {
            if(!checkBox2.Checked)
                PlayTime.Stop();
            if (listBox1.SelectedIndex != -1)
            {
                parseComand(listBox1.Items[listBox1.SelectedIndex].ToString());
                if (listBox1.Items.Count - 1 <= listBox1.SelectedIndex)
                {
                    checkBox2.Checked = false;
                }
                else {
                    listBox1.SelectedIndex++;
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if(!checkBox1.Checked && listBox1.SelectedIndex != -1)
            parseComand(listBox1.Items[listBox1.SelectedIndex].ToString());
        }
    }
}
