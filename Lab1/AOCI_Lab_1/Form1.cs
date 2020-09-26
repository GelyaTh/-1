using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace AOCI_Lab_1
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> sourceImage; //<Цветовое пространство(rgb), глубина цвета>
        private VideoCapture capture;
        private string videoFileName;
        int frameCounter = 0;
        bool isVideoFiltering = false;

        double cannyThreshold = 10.0;
        double cannyThresholdLinking = 10.0;

        public Form1()
        {
            InitializeComponent();
        }

        //Картинка
        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg,  *.jpeg,  *.jpe,  *.jfif,  *.png)  |  *.jpg;  *.jpeg;  *.jpe;  *.jfif; *.png";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                sourceImage = new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);

                imageBox1.Image = sourceImage;
            }
        }

        //Метод примененяет фильтр Канни к картинке и возвращает картинку с фильтром
        private Image<Gray, byte> GetImageWithCanny()
        {
            Image<Gray, byte> grayImage = sourceImage.Convert<Gray, byte>();

            var tempImage = grayImage.PyrDown();
            var destImage = tempImage.PyrUp();

            Image<Gray, byte> cannyEdges = destImage.Canny(cannyThreshold, cannyThresholdLinking);

            return cannyEdges;
        }

        //Метод порогового фильтра
        private Image<Bgr, byte> GetImageWithCellShading(int level)
        {
            Image<Gray, byte> cannyEdges = GetImageWithCanny();

            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();
            var resultImage = sourceImage.Sub(cannyEdgesBgr); // попиксельное вычитание

            //обход по каналам
            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++) // обход по пискелям
                    {
                        // получение цвета пикселя
                        byte color = resultImage.Data[y, x, channel];

                        for (int interval = 0; interval < level; interval++)
                        {
                            if ((color > (255 / level) * interval) && (color <= (255 / level) * (interval + 1)))
                            {
                                color = (byte)((255 / level) * interval);
                                break;
                            }
                        }

                        resultImage.Data[y, x, channel] = color; // изменение цвета пикселя
                    }

            return resultImage;
        }
        
        //Фильтр Канни
        private void Button2_Click(object sender, EventArgs e)
        {
            imageBox2.Image = GetImageWithCanny();

            //расширение светлых областей(иттерации)
            // cannyEdges._Dilate(1);
        }

        //Пороговый фильтр
        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = GetImageWithCellShading(trackBar3.Value);
        }

        //Webcam
        //private void Button3_Click(object sender, EventArgs e)
        //{
        //    // инициализация веб-камеры
        //    capture = new VideoCapture();
        //    capture.ImageGrabbed += ProcessFrame;
        //    capture.Start(); // начало обработки видеопотока
        //}

        // захват кадра из видеопотока
        //private void ProcessFrame(object sender, EventArgs e)
        //{
        //    var frame = new Mat();
        //    capture.Retrieve(frame); // получение текущего кадра

        //    Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();

        //    Image<Gray, byte> grayImage = image.Convert<Gray, byte>();

        //    var tempImage = grayImage.PyrDown();
        //    var destImage = tempImage.PyrUp();

        //    double cannyThreshold = 10.0;
        //    double cannyThresholdLinking = 40.0;
        //    Image<Gray, byte> cannyEdges = destImage.Canny(cannyThreshold, cannyThresholdLinking);

        //    imageBox2.Image = cannyEdges;
        //}

        //Video
        private void Button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы видео (*.webm,  *.mp4)  |  *.webm;  *.mp4";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                videoFileName = fileName;
                capture = new VideoCapture(fileName);               
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();
            sourceImage = frame.ToImage<Bgr, byte>();
            imageBox1.Image = sourceImage;

            if (isVideoFiltering)
                imageBox2.Image = GetImageWithCellShading(trackBar3.Value);
                //imageBox2.Image = GetImageWithCanny();
            frameCounter++;

            if (frameCounter >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer1.Enabled = false;
                frameCounter = 0;
            }
                
        }

        //Обработка
        private void button5_Click(object sender, EventArgs e)
        {
            if (isVideoFiltering)
                isVideoFiltering = false;
            else
                isVideoFiltering = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            cannyThreshold = 10.0 * trackBar1.Value;
            imageBox2.Image = GetImageWithCanny();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            cannyThresholdLinking = 10.0 * trackBar2.Value;
            imageBox2.Image = GetImageWithCanny();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            imageBox2.Image = GetImageWithCellShading(trackBar3.Value);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (capture is null) return;
            frameCounter = 0;
            capture = new VideoCapture(videoFileName);
            timer1.Enabled = true;
        }
    }
}
