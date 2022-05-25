using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        string pathTreeView = "";
        string backPath; //предыдущий путь
        string currentPath; //текущий путь
        string nextPath; //следующий путь
        List<string> listPathDirs = new List<string>(); //список путей директорий 
        Stack<string> nextPathStack = new Stack<string>(); //стек для следующих путей при нажатии кнопки вперед, если они есть

        public Form1()
        {
            InitializeComponent();
            this.Text = "Мой копьютер";
        }

        //Листвью
        void GetLocalDir() //Получить логические диски
        {
            try
            {
                listView1.Items.Clear();
                string[] directories = Environment.GetLogicalDrives();
                foreach (string item in directories)
                {                   
                    listView1.Items.Add(item, 0);
                    listPathDirs.Add(item);
                }
                btnBack.Enabled = false;                
                backPath = "";
                backPTextBox.Text = backPath;
                currentPath = "Мой компьютер";
                currentPTextBox.Text = currentPath; 
                if (nextPathStack.Count != 0)
                {
                    nextPath = nextPathStack.Peek();
                    btnNext.Enabled = true;
                }
                textBox3.Text = nextPath; 
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }
        void GetFiles() //Получить файлы и директории
        {

            listView1.BeginUpdate();
            try
            {
                btnBack.Enabled = true; 

                var dirs = Directory.GetDirectories(currentPath) //фильтр на директории - убираем скрытые и системные папки
                    .Select(d => new { Attr = new DirectoryInfo(d).Attributes, Dir = d })
                    .Where(x => !x.Attr.HasFlag(FileAttributes.System))
                    .Where(x => !x.Attr.HasFlag(FileAttributes.Hidden))
                    .Select(x => x.Dir)
                    .ToList();

                listPathDirs.Clear(); //очищаем список директорий

                try //получаем путь родительского каталога если он есть
                {
                    DirectoryInfo dInfo = new DirectoryInfo(currentPath);
                    backPath = dInfo.Parent?.FullName ?? "Мой компьютер";
                    backPTextBox.Text = backPath;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }

                listView1.Items.Clear(); //очищаем листвью и начинаем его перезаполнять
                foreach (string s in dirs) //добавляем каталоги из фильтра в листвью и в список путей директорий
                {
                    string dirName = System.IO.Path.GetFileName(s);
                    listView1.Items.Add(dirName, 1);
                    listPathDirs.Add(s);
                }
                string[] files = Directory.GetFiles(currentPath); //получаем и добавляем файлы в листвью в текущей директории
                foreach (string s in files)
                {
                    string fileName = System.IO.Path.GetFileName(s);
                    if (Path.GetExtension(fileName) == ".png" || Path.GetExtension(fileName) == ".jpeg" || Path.GetExtension(fileName) == ".bmp" || Path.GetExtension(fileName) == ".ico")
                    {
                        listView1.Items.Add(fileName, 3);
                    }
                    else if (Path.GetExtension(fileName) == ".txt")
                    {
                        listView1.Items.Add(fileName, 4);
                    }
                    else
                    {
                        listView1.Items.Add(fileName, 2);
                    }
                }

                //Перезаписываем пути для тексбоксов
                currentPTextBox.Text = currentPath; 
                if (nextPathStack.Count != 0) //если стек не пустой
                {
                    textBox3.Text = nextPathStack.Peek();
                    nextPath = nextPathStack.Peek();
                }
                else
                {
                    textBox3.Text = ""; 
                    btnNext.Enabled = false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            listView1.EndUpdate();
        }


        //Тривью
        public void fillNode(string path, TreeNode parentNode)
        {
            var dirs = Directory.GetDirectories(path)
                    .Select(d => new { Attr = new DirectoryInfo(d).Attributes, Dir = d })
                    .Where(x => !x.Attr.HasFlag(FileAttributes.System))
                    .Where(x => !x.Attr.HasFlag(FileAttributes.Hidden))
                    .Select(x => x.Dir)
                    .ToList();
            try
            {
                foreach (string dir in dirs) //заполнение узлов каталогами
                {
                    //c картинками привязываем                    
                    TreeNode node = new TreeNode(System.IO.Path.GetFileName(dir), 1, 1);
                    node.Name = dir; 
                    parentNode.Nodes.Add(node);
                    fillNode(dir, node); //рекурсионный обход каталогов
                }
            }
            catch { } // Если обрабатывать исключения, выдает ошибку "access to the path is denied"
            //штук 8 папок к которым нет доступа пишет, у меня на ноуте один логический диск C:
            //так и не нашел решения(( как понял система в целях безопасности не дает доступ к катим то системным файлам
            //catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        

        //События
        private void Form1_Load(object sender, EventArgs e) 
        {
            btnNext.Enabled = false;
            GetLocalDir();

            string[] directories = Environment.GetLogicalDrives();
            foreach (string item in directories)
            {
                //для treeview собираем дерево
                TreeNode nodeLD = new TreeNode(item, 0, 0);
                nodeLD.Name = item;
                treeView1.Nodes.Add(nodeLD);
                fillNode(item, nodeLD);
            }
        }
        private void listView1_ItemActivate(object sender, EventArgs e) //активация итема
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            ListViewItem item = listView1.SelectedItems[0];
            if (item.ImageIndex == 1 || item.ImageIndex == 0)
            {
                string itemName = item.Text; 
                string title = "";
                
                foreach (string path in listPathDirs) //ищем активированную директорию
                {
                    string dirName = path.Split('\\').LastOrDefault(); //обрезаем каждый путь чтобы получить имя директории
                    try
                    {
                        if (path == itemName || dirName == itemName) //path.Substring(path.Length - it.Length, it.Length) == it
                        {
                            currentPath = path;
                            title = path;
                        }
                    }
                    catch(Exception ex) { MessageBox.Show(ex.Message); }
                }
                try
                {
                    this.Text = title;
                    nextPathStack.Clear();
                    btnNext.Enabled = false;
                    GetFiles();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else
            {
                string start = this.Text + "\\" + item.Text;
                System.Diagnostics.Process.Start(new ProcessStartInfo(start) { UseShellExecute = true });
            }
        }

        private void btnBack_Click(object sender, EventArgs e) //Кнопка переход назад
        {
            bool b = false;
            String[] LogicalDrives = Environment.GetLogicalDrives();
            foreach (string s in LogicalDrives)
            {
                if (backPath == s && currentPath == s) b = true; 
            }
            if ((backPath != null && backPath != "Мой компьютер") && !b) 
            {
                this.Text = backPath;
                nextPath = currentPath;
                currentPath = backPath;
                btnNext.Enabled = true;
                nextPathStack.Push(nextPath); // при нажатии кнопки назад заносим путь из которого вышли в стек                                                    
                GetFiles();
            }
            else //в ином случае вернулись в начало 
            {
                nextPathStack.Push(currentPath);
                btnBack.Enabled = false;
                GetLocalDir();
            }
        }

        private void btnNext_Click(object sender, EventArgs e) //кнопка перехода вперед
        {
            try
            {
                if (nextPath != null && nextPathStack.Count != 0)
                {
                    backPath = currentPath;
                    currentPath = nextPathStack.Pop();
                    GetFiles();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            pathTreeView = treeView1.SelectedNode.Name;
            currentPath = pathTreeView;
            nextPathStack.Clear();
            GetFiles();          
            nextPath = "";
        }
    }
}
