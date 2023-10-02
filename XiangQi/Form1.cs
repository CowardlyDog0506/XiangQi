using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;

namespace XiangQi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //===============================================================
        //PictureBox動態控制陣列
        PictureBox[] pics = new PictureBox[32];
        //格塊狀態紀錄陣列  空格為 -1  覆蓋為 0
        string[] pics_state = new string[32];
        //圖片庫
        Image[] b_img = new Image[8];
        Image[] r_img = new Image[8];
        //未翻開棋子種類個數紀錄Dict
        Dictionary<string, int> Deck = new Dictionary<string, int>();
        Dictionary<string, int> dict;   //deck的副本
        //移動模式狀態旗標, 紀錄欲移動之棋子的idx
        int move_mode = 0, idx_want_move;
        char turn = '0';
        //===============================================================
        private void Form1_Load(object sender, EventArgs e)
        {
            //匯入圖片
            b_img[0] = r_img[0] = Image.FromFile(Application.StartupPath + "\\XiangQi\\0.png");
            for (int i = 0; i < 2; i++)
            {
                string tmp = i == 0 ? "b" : "r";
                for (int j = 1; j <= 7; j++)
                {
                    if (i == 0)
                        b_img[j] = Image.FromFile(string.Format(Application.StartupPath + "\\XiangQi\\{0}{1}.png", tmp, j));
                    else
                        r_img[j] = Image.FromFile(string.Format(Application.StartupPath + "\\XiangQi\\{0}{1}.png", tmp, j));
                }
            }
            //實體化pics陣列內物件
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int idx = x + y * 8;
                    pics_state[idx] = "0";
                    pics[idx] = new PictureBox();
                    pics[idx].Name = String.Format("p{0}", idx);
                    pics[idx].Width = pics[idx].Height = 120;
                    pics[idx].Left = x * pics[0].Width + 25;
                    pics[idx].Top = y * pics[0].Height + 90;
                    pics[idx].Image = b_img[0];
                    pics[idx].BackColor = Color.Transparent;
                    pics[idx].SizeMode = PictureBoxSizeMode.Zoom;
                    //pics內物件共享單一Click事件
                    pics[idx].Click += new EventHandler(pic_click);
                }
            }
            this.Controls.AddRange(pics);   //將pics陣列加入表單控制清單
            //create deck dic
            for (int j = 0; j < 2; j++)
            {
                char s;
                if (j == 0)
                    s = 'b';
                else
                    s = 'r';
                for (int i = 1; i <= 7; i++)
                {
                    if (i == 1)
                        Deck.Add(s + i.ToString(), 1);
                    else if (i == 7)
                        Deck.Add(s + i.ToString(), 5);
                    else
                        Deck.Add(s + i.ToString(), 2);
                }
            }
            dict = new Dictionary<string, int>(Deck);
        }

        private void change_turn()
        {
            if (turn == 'b')
            {
                turn = 'r';
                turn_label.ForeColor = Color.Red;
            }
            else
            {
                turn = 'b';
                turn_label.ForeColor = Color.Black;
            }
            turn_label.Text = turn.ToString();
        }
        //換圖
        private void change_pic(int idx, char color, int type)
        {
            //b1-7 將 到 卒
            if (type == -1)
            {
                pics_state[idx] = "-1";
                pics[idx].Image = null;
                return;
            }
            else if (color == 'b')
                pics[idx].Image = b_img[type];
            else
                pics[idx].Image = r_img[type];
            pics_state[idx] = color + type.ToString();
        }

        //翻牌
        private void flip(int idx)
        {
            Random r = new Random();
            int type;
            char color;
            do
            {
                color = r.Next(0, 2) == 0 ? 'b' : 'r';
                type = r.Next(1, 8);

            } while (Deck[color + type.ToString()] == 0);
            Deck[color + type.ToString()]--;
            change_pic(idx, color, type);
        }

        //比棋子大小
        private bool who_is_daddy(int idx, int tar)
        {
            if (pics_state[tar] == "-1")
                return true;
            else if (pics_state[idx][1] == '7' && pics_state[tar][1] == '1')
            {
                return true;
            }
            else
            {
                if (pics_state[idx][1] - '0' <= pics_state[tar][1] - '0')
                {
                    if (pics_state[idx][1] == '1' && pics_state[tar][1] == '7')
                    {
                        return false;
                    }
                    else
                        return true;
                }
                return false;
            }
        }

        //移動
        private void move(int idx, int tar)
        {
            change_pic(tar, pics_state[idx][0], pics_state[idx][1] - '0');
            change_pic(idx, 'b', -1);
        }

        //判斷可否移動
        private bool can_move(int idx, int tar)
        {
            if (pics_state[tar][0] != pics_state[idx][0])
            {
                //炮的移動
                if (pics_state[idx][1] == '6')
                {
                    if ((tar == idx + 1 || tar == idx - 1 | tar == idx + 8 || tar == idx - 8) && pics_state[tar] == "-1")
                    {
                        return true;
                    }
                    else if (pics_state[tar] == "-1")
                        return false;
                    bool tmp = can_fly(idx, tar);
                    return tmp;
                }
                //一般移動
                if ((tar == idx + 1 || tar == idx - 1 | tar == idx + 8 || tar == idx - 8) && pics_state[tar] != "0")
                {
                    return who_is_daddy(idx, tar);
                }
            }
            return false;
        }
        private bool count_between(int idx, int tar)
        {
            int dist = tar - idx, count = 0;
            if (idx % 8 == tar % 8)
            {
                if (dist > 0)
                {
                    for (int i = idx + 8; i < tar; i += 8)
                    {
                        if (pics_state[i] != "-1")
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    for (int i = idx - 8; i > tar; i -= 8)
                    {
                        if (pics_state[i] != "-1")
                        {
                            count++;
                        }
                    }
                }
            }
            else
            {
                if (dist > 0)
                {
                    for (int i = idx + 1; i < tar; i++)
                    {
                        if (pics_state[i] != "-1")
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    for (int i = idx - 1; i > tar; i--)
                    {
                        if (pics_state[i] != "-1")
                        {
                            count++;
                        }
                    }
                }
            }
            if (count == 1)
                return true;
            else
                return false;
        }
        private bool can_fly(int idx, int tar)
        {
            if (idx % 8 != tar % 8 && idx / 8 != tar / 8)
            {
                return false;
            }
            else
            {
                return count_between(idx, tar);
            }
        }
        //定義點擊事件:   1.翻牌   2.移動準備 move_mode = 1 => 取消 || 移動
        private void pic_click(object sender, EventArgs e)
        {
            int idx = Array.IndexOf(pics, sender);
            if (pics_state[idx] == "0")
            {
                flip(idx);
                if (turn == '0')
                {
                    turn = pics_state[idx][0];
                }
                if (move_mode == 1)
                {
                    pics[idx_want_move].BackColor = Color.Transparent;
                    move_mode = 0;
                }
                change_turn();
            }
            else if (move_mode == 0 && pics_state[idx][0] == turn)
            {
                move_mode = 1;
                idx_want_move = idx;
                pics[idx].BackColor = Color.Yellow;
            }
            else if (move_mode == 1)
            {
                move_mode = 0;
                if (can_move(idx_want_move, idx) && idx != idx_want_move)
                {
                    move(idx_want_move, idx);
                    change_turn();
                }
                pics[idx_want_move].BackColor = Color.Transparent;
            }
        }

        private void reset_btn_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void reset()
        {
            Deck = new Dictionary<string, int>(dict);
            for (int i = 0; i < 32; i++)
            {
                pics_state[i] = "0";
                pics[i].Image = b_img[0];
            }
            turn_label.Text = "Turn";
            turn_label.ForeColor = Color.Black;
            turn = '0';
        }
    }
}