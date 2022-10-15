using Memento;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace Memento
{

    public class Originator
    {
        public string _state;

        public Originator(string state)
        {
            _state = state;
            Console.WriteLine("Originator : My Initial state is : " + _state);
        }

        public void ChangeState(string newState)
        {
            this._state = newState;
        }
        public string GenerateRandomString(int length = 30)
        {
            string allowedSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            while (length > 0)
            {
                result += allowedSymbols[new Random().Next(0, allowedSymbols.Length)];

                Thread.Sleep(12);
                length--;
            }
            return result;
        }
        public IMemento Save()
        {
            return new TextMemento(this._state);
        }

        public void Restore(IMemento memento)
        {
            if (!(memento is TextMemento))
            {
                throw new Exception("Unknown memento class " + memento.ToString());
            }
            this._state = memento.GetState();
        }
    }
    public interface IMemento
    {
        string GetName();
        string GetState();
        DateTime GetDate();
    }
    public class TextMemento : IMemento
    {
        private string _state;
        private DateTime _date;
        public TextMemento(string state)
        {
            this._state = state;
            this._date = DateTime.Now;
        }
        public DateTime GetDate()
        {
            return _date;
        }

        public string GetName()
        {
            return $"{_date} / {_state.Substring(0, 9)}";
        }

        public string GetState()
        {
            return _state;
        }
    }
    public class CareTaker
    {
        private List<IMemento> _mementos = new List<IMemento>();
        private Originator _originator = null;

        public CareTaker(Originator originator)
        {
            _originator = originator;
        }

        public void Undo()
        {
            if (_mementos.Count == 0)
            {
                return;
            }

            //var memento = _mementos.Last();
            //this._mementos.Remove(memento);
            var memento = _mementos[Index];
            if (Index >= 0)
            {
                if (Index != 0)
                {
                    Index--;
                }
            }
            //Console.WriteLine("Care Taker :  Restoring state" + memento.GetName());

            try
            {
                _originator.Restore(memento);
            }
            catch (Exception)
            {
                this.Undo();
            }

        }
        public void Redo()
        {
            var memento = _mementos[Index];
            if (_mementos.Count - 1 >= Index)
            {
                if (_mementos.Count-1 > Index)
                {
                    Index++;
                }
            }
            try
            {
                _originator.Restore(memento);
            }
            catch (Exception)
            {
                this.Undo();
            }

        }
        public int Index { get; set; } = -1;
        public void BackUp()
        {
            this._mementos.Add(_originator.Save());
            Index++;
        }


        public void ShowHistory()
        {
            Console.WriteLine("CareTaker :  Here\'s the list of mementos");
            foreach (var item in _mementos)
            {
                Console.WriteLine(item.GetName()); ;
            }
        }
    }
}
namespace WpfApp4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Originator originator { get; set; }
        public static CareTaker caretaker { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            originator = new Originator("null");
            caretaker = new CareTaker(originator);

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (originator._state != "null")
            {
                caretaker.Undo();
                MainImg.Source = new ImageSourceConverter().ConvertFromString(originator._state) as ImageSource;
            }
            //originator._state
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (originator._state != "null")
            {
                caretaker.Redo();
                MainImg.Source = new ImageSourceConverter().ConvertFromString(originator._state) as ImageSource;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Bitmap captureBitmap = new Bitmap(1024, 728, System.Drawing.Imaging.PixelFormat.Format64bppArgb);

            System.Drawing.Rectangle captureRectangle = Screen.AllScreens[0].Bounds;

            Graphics captureGraphics = Graphics.FromImage(captureBitmap);

            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);

            string guidGen = Guid.NewGuid().ToString();

            string imgPath = $"~..\\..\\..\\..\\{guidGen}.jpg";
        
            captureBitmap.Save($@"{imgPath}", ImageFormat.Jpeg);

            originator.ChangeState(imgPath);
            caretaker.BackUp();

            System.Windows.MessageBox.Show("Succesfully Added");

        }
    }
}
