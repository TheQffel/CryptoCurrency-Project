using System;
using System.Drawing;
using System.IO;

namespace OneCoin
{
    class Media
    {
        public static string GenerateImage()
        {
            Random Random = new();

            string[] Files = Directory.GetFiles(Settings.ImagesPath);

            if(Files.Length > 0)
            {
                string FileName = Path.GetFileName(Files[Random.Next(0, Files.Length)]);
                Bitmap BitmapImage = (Bitmap)Image.FromFile(Settings.ImagesPath + FileName);
                FileName = ImageToText(BitmapImage);
                if(ImageDataCorrect(FileName))
                {
                    if(FileName.Length < 4000)
                    {
                        return FileName;
                    }
                }
            }

            int ImageSize = Random.Next(16, 29);

            Bitmap RandomImage = new(ImageSize, ImageSize);
            int ColorR = Random.Next(32, 192);
            int ColorG = Random.Next(32, 192);
            int ColorB = Random.Next(32, 192);

            for (int i = 0; i < ImageSize; i++)
            {
                for (int j = 0; j < ImageSize; j++)
                {
                    RandomImage.SetPixel(i, j, Color.FromArgb(ColorR + Random.Next(0, 64) - 32, ColorG + Random.Next(0, 64) - 32, ColorB + Random.Next(0, 64) - 32));
                }
            }
            return ImageToText(RandomImage);
        }

        public static string GenerateMessage()
        {
            Random Random = new();

            string[] Files = Directory.GetFiles(Settings.MessagesPath);

            if (Files.Length > 0)
            {
                string FileName = Path.GetFileName(Files[Random.Next(0, Files.Length)]);
                string Message = File.ReadAllLines(Settings.MessagesPath + FileName)[0];
                if(Message.Length < 500)
                {
                    if(Hashing.CheckStringFormat(Message, 5, 0, int.MaxValue))
                    {
                        return Message;
                    }
                }
            }
            return "One Coin";
        }

        public static string ImageToText(Bitmap Image)
        {
            if(Image.Width != Image.Height)
            {
                return "";
            }
            
            int Counter = 0;
            string Color = "X";

            string CompressedImage = "";

            for (int i = 0; i < Image.Width; i++)
            {
                for (int j = 0; j < Image.Height; j++)
                {
                    Color Pixel = Image.GetPixel(j, i);

                    byte R = (byte)(Pixel.R / 5);
                    byte G = (byte)(Pixel.G / 5);
                    byte B = (byte)(Pixel.B / 5);

                    if (Pixel.R % 5 > 2.5) { R++; }
                    if (Pixel.G % 5 > 2.5) { G++; }
                    if (Pixel.B % 5 > 2.5) { B++; }

                    string NewColor = Convert.ToBase64String(new byte[] { 0, 0, R })[3..] + Convert.ToBase64String(new byte[] { 0, 0, G })[3..] + Convert.ToBase64String(new byte[] { 0, 0, B })[3..];

                    if (Pixel.A < 128) { NewColor = " "; }

                    if(Color == NewColor)
                    {
                        Counter++;
                    }
                    else
                    {
                        CompressedImage += Counter + Color;
                        Color = NewColor;
                        Counter = 1;
                    }
                }
            }

            CompressedImage += Counter + Color;

            return CompressedImage[2..];
        }

        public static Bitmap TextToImage(string Data)
        {
            string PixelsCount = "";
            int LastIndex = 0;
            Color Color;

            Color[] Result = new Color[1024 * 1024];

            for (int i = 0; i < Data.Length; i++)
            {
                if(Data[i] > 47 && Data[i] < 58)
                {
                    PixelsCount += Data[i];
                }
                else
                {
                    if (Data[i] == ' ')
                    {
                        Color = Color.FromArgb(0, 0, 0, 0);
                    }
                    else
                    {
                        Color = Color.FromArgb(Convert.FromBase64String("AAA" + Data[i])[2] * 5, Convert.FromBase64String("AAA" + Data[i + 1])[2] * 5, Convert.FromBase64String("AAA" + Data[i + 2])[2] * 5);

                        i += 2;
                    }
                    int Limit = Convert.ToInt32(PixelsCount) + LastIndex;

                    for (; LastIndex < Limit; LastIndex++)
                    {
                        Result[LastIndex] = Color;
                    }

                    PixelsCount = "";
                }
            }

            int Dimensions = (int)Math.Sqrt(LastIndex);
            
            if(Dimensions*Dimensions != LastIndex || Dimensions < 1)
            {
                return null;
            }
            
            Bitmap Final = new(Dimensions, Dimensions);

            for (int i = 0; i < Dimensions; i++)
            {
                for (int j = 0; j < Dimensions; j++)
                {
                    Final.SetPixel(j, i, Result[(i * Dimensions) + j]);
                }
            }

            return Final;
        }
        
        public static bool ImageDataCorrect(string Data)
        {
            Bitmap Image = TextToImage(Data);
            if(Image == null) { return false; }
            bool Correct = Image.Width == Image.Height;
            if(Image.Width < 16 || Image.Height < 16) { Correct = false; }
            if(Image.Width > 1024 || Image.Height > 1024) { Correct = false; }
            return Correct;
        }
    }
}
