using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace dz_24._04
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cancelTokenSource;
        private Mutex mutex;

        public Form1()
        {
            InitializeComponent();
            mutex = new Mutex();
        }
        private async void Button_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            string searchWord = textBox1.Text;
            string searchPath = textBox2.Text;
            cancelTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(() => SearchFiles(searchWord, searchPath, cancelTokenSource.Token));
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Поиск был отменен");
            }
        }
        private void Button_Click1(object sender, EventArgs e)
        {
            cancelTokenSource.Cancel();
        }
        private void SearchFiles(string searchWord, string searchPath, CancellationToken cancellationToken)
        {
            string[] files = Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                string fileContent = File.ReadAllText(file);
                int wordCount = CountOccurrences(fileContent, searchWord);
                if (wordCount > 0)
                {
                    mutex.WaitOne();
                    listBox1.Items.Add(string.Format("Название файла: {0}", Path.GetFileName(file)));
                    listBox1.Items.Add(string.Format("Путь к файлу: {0}", file));
                    listBox1.Items.Add(string.Format("Количество вхождений слова: {0}", wordCount));
                    listBox1.Items.Add("");
                    mutex.ReleaseMutex();
                }
            }
        }
        private int CountOccurrences(string inputString, string searchString)
        {
            int count = 0;
            int i = 0;
            while ((i = inputString.IndexOf(searchString, i)) != -1)
            {
                i += searchString.Length;
                count++;
            }
            return count;
        }
    }
}
