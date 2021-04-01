using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Puzzles_Bitmap_
{
    //Структура с текущими настройками части пазла (текущая локация и поворот)
    public struct Settings
    {
        public int rotation;
        public Point currentLoc;
    }

    //Структура, содержащая главную информацию о части пазла (сам битмап, первичная локация (там, где часть должна быть для решения пазла) и настройки)
    public class Images
    {
        public Bitmap bitmap;
        public Point primaryLoc;
        public Settings settings;
    }
    public partial class Form1 : Form
    {
        //Картинка, с которой будет составляться пазл и структура частей пазла, вырезанных с главной картинки
        public Bitmap image;
        public List<Images> images = new List<Images>();

        //Количество частей в ширину и в высоту
        public int widthPictures = 4;
        public int heightPictures = 3;

        //Индексы выбранных частей (нужно для замены частей местами)
        public int selected_1 = -1;
        public int selected_2 = -1;

        public Random r = new Random();
        public Form1()
        {
            InitializeComponent();
         
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.MouseDown += Form1_Click;
            this.Text = "Пазл";

            //Двойная буфферизация (убирает мигание)
            this.DoubleBuffered = true;

            MessageBox.Show("ЛКМ - повернуть часть пазла на 90°;\nПКМ - поменять две части местами.", "Инструкция", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //При загрузке формы
        private void Form1_Load(object sender, EventArgs e)
        {
            //Загрузка картинки и редактирование размера окна под картинку
            image = new Bitmap("pic.png");
            //Окно загружается с размером, подогнанным под картинку (2 - расстояние между частями, 20 - отступ от верхнего левого угла)
            this.ClientSize = new Size(image.Width + (widthPictures * widthPictures) * 2 + 20, image.Height + (heightPictures * heightPictures) * 2 + 20);
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            Bitmap temp;
            for (int i = 0; i < heightPictures; i++)
            {
                for (int j = 0; j < widthPictures; j++)
                {
                    //Создание прямоугольной области, откуда будут производиться более мелкие части. Первые два параметра - начальная точка, остальные - размер.
                    Rectangle rect = new Rectangle(j * (image.Width / widthPictures), i * (image.Height / heightPictures),
                        image.Width / widthPictures, image.Height / heightPictures);

                    //Копирование
                    temp = image.Clone(rect, image.PixelFormat);
                    //Запись положения части для её дальнейшей отрисовки на форме
                    Point loc = new Point(j * (image.Width / widthPictures) + j * 2 + 20, i * (image.Height / heightPictures) + i * 2 + 20);

                    //Добавление части
                    images.Add(new Images
                    {
                        bitmap = temp,
                        primaryLoc = loc,
                        settings = new Settings { currentLoc = loc, rotation = 0 }
                    });
                }
            }

            //Рандомизатор
            Randomizer(r.Next(widthPictures * heightPictures, (widthPictures * heightPictures) * 2));
            //this.Invalidate();          
        }

        //Метод "рандомизатор"
        private void Randomizer(int iterations)
        {
            int index;
            for (int i = 0; i < iterations; i++)
            {
                index = r.Next(0, images.Count());
                //Рандомная картинка поворачивается на 90 градусов и записывается информация об этом действии в объект класса
                images[index].settings.rotation = (images[index].settings.rotation + 1) % 4;
                images[index].bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

                //Выбор индексов двух картинок для смены их местами
                selected_1 = r.Next(0, images.Count());
                selected_2 = r.Next(0, images.Count());

                //Объект класса картинки, нужен для смены данных картинок местами
                Images swap = new Images { bitmap = new Bitmap(images[selected_1].bitmap), settings = new Settings { currentLoc = images[selected_1].settings.currentLoc, rotation = images[selected_1].settings.rotation} };

                //Изменяется сама картинка, текущее положение и поворот. Начальное положение остаётся, оно нужно для проверки
                images[selected_1].bitmap = new Bitmap(images[selected_2].bitmap);
                images[selected_1].settings.currentLoc = images[selected_2].settings.currentLoc;
                images[selected_1].settings.rotation = images[selected_2].settings.rotation;

                images[selected_2].bitmap = new Bitmap(swap.bitmap);
                images[selected_2].settings.currentLoc = swap.settings.currentLoc;
                images[selected_2].settings.rotation = swap.settings.rotation;
            }

            //Индексы возвращаются в нейтральное положение
            selected_1 = -1;
            selected_2 = -1;
        }

        //Клик по форме
        private void Form1_Click(object sender, MouseEventArgs e)
        {
            //Если была нажата левая кнопка мыши
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < images.Count(); i++)
                {
                    if (e.Location.X > images[i].primaryLoc.X && e.Location.X < images[i].bitmap.Width + images[i].primaryLoc.X &&
                        e.Location.Y > images[i].primaryLoc.Y && e.Location.Y < images[i].bitmap.Height + images[i].primaryLoc.Y)
                    //Производится поворот для кликнутой картинки
                    {
                        images[i].settings.rotation = (images[i].settings.rotation + 1) % 4;
                        images[i].bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }
                }
            }
            //Если была нажата правая кнопка мыши
            else if (e.Button == MouseButtons.Right)
            {
                //Если первая картинка ещё не была выбрана
                if (selected_1 == -1)
                {
                    for (int i = 0; i < images.Count(); i++)
                    {
                        if (e.Location.X > images[i].primaryLoc.X && e.Location.X < images[i].bitmap.Width + images[i].primaryLoc.X &&
                            e.Location.Y > images[i].primaryLoc.Y && e.Location.Y < images[i].bitmap.Height + images[i].primaryLoc.Y)
                            selected_1 = i;
                    }

                    this.Text = "Выберите вторую часть...";
                }
                //Если была выбрана первая картинка
                else
                {
                    for (int i = 0; i < images.Count(); i++)
                    {
                        if (e.Location.X > images[i].primaryLoc.X && e.Location.X < images[i].bitmap.Width + images[i].primaryLoc.X &&
                            e.Location.Y > images[i].primaryLoc.Y && e.Location.Y < images[i].bitmap.Height + images[i].primaryLoc.Y)
                            selected_2 = i;
                    }

                    //Если была выбрана вторая картинка
                    if (selected_2 != -1)
                    {
                        //Происходит обмен данных картинок (их смена)
                        Images swap = new Images { bitmap = new Bitmap(images[selected_1].bitmap), settings = new Settings { currentLoc = images[selected_1].settings.currentLoc, rotation = images[selected_1].settings.rotation } };

                        images[selected_1].bitmap = new Bitmap(images[selected_2].bitmap);
                        images[selected_1].settings.currentLoc = images[selected_2].settings.currentLoc;
                        images[selected_1].settings.rotation = images[selected_2].settings.rotation;

                        images[selected_2].bitmap = new Bitmap(swap.bitmap);
                        images[selected_2].settings.currentLoc = swap.settings.currentLoc;
                        images[selected_2].settings.rotation = swap.settings.rotation;

                        selected_1 = -1;
                        selected_2 = -1;

                        this.Text = "Пазл";
                    }
                }
            }

            //Делает одно и то же - заставляет форму перерисовать элементы
            //this.Invalidate();
            this.Refresh();

            //Если у всех картинок значение поворота на нуле (они в изначальном положении) и их текущее положение равняется изначальному положению (каждая картинка на своём месте)
            if (images.All(x => x.settings.rotation == 0 && x.primaryLoc == x.settings.currentLoc))
            {
                //Победа!
                MessageBox.Show("Вы победили!", "Победа!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                this.Dispose();
            }
        }

        //Событие прорисовки
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Отрисовка картинки на её позиции
            images.ForEach(x => e.Graphics.DrawImage(x.bitmap, x.primaryLoc));
        }
    }
}
