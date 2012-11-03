<%@ WebHandler Language="C#" Class="_Captcha" %>

using System;
using System.Linq;
using System.Web;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Moo.Core.Utility;
using Moo.Core.Security;

public class _Captcha : IHttpHandler
{
    static Bitmap sampleBitmap = new Bitmap(1, 1);

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "image/png";
        int token;
        if (!int.TryParse(context.Request.QueryString["token"], out token))
        {
            ShowErrorImage(context, "不是整数");
            return;
        }

        if (!Captcha.TokenValid(token))
        {
            ShowErrorImage(context, "Token无效");
            return;
        }

        Captcha.Ticket ticket = Captcha.GetTicket(token);
        if (ticket.GotImage)
        {
            ShowErrorImage(context, "不可重复获取");
            return;
        }
        ticket.GotImage = true;

        ShowCaptcha(context, ticket.Answer);
    }

    void ShowErrorImage(HttpContext context, string text)
    {
        Size imageSize;
        Font font = new Font(FontFamily.GenericSerif, 14);
        using (Graphics g = Graphics.FromImage(sampleBitmap))
        {
            imageSize = g.MeasureString(text, font).ToSize();
        }
        using (Bitmap bitmap = new Bitmap(imageSize.Width, imageSize.Height))
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawString(text, font, Brushes.Black, PointF.Empty);
            }
            bitmap.Save(context.Response.OutputStream, ImageFormat.Png);
        }
    }

    Color RandomColor()
    {
        return Color.FromArgb(Rand.RAND.Next(256), Rand.RAND.Next(256), Rand.RAND.Next(256));
    }

    void ShowCaptcha(HttpContext context, string text)
    {
        using (Bitmap allText = GenerateBitmap(text))
        {
            using (Bitmap final = new Bitmap(100, 50))
            {
                using (Graphics g = Graphics.FromImage(final))
                {
                    g.DrawImage(allText, new Point[] { new Point(0, 0), new Point(final.Width - 1, 0), new Point(0, final.Height - 1) });
                    g.DrawBezier(new Pen(Color.Black, 4), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)));
                    g.DrawBezier(new Pen(Color.Black, 4), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)), new Point(Rand.RAND.Next(final.Width), Rand.RAND.Next(final.Height)));
                }
                for (int i = 0; i < final.Width; i++)
                {
                    for (int j = 0; j < final.Height; j++)
                    {
                        if (final.GetPixel(i, j).A > 0)
                        {
                            final.SetPixel(i, j, Color.Black);
                        }
                        else
                        {
                            final.SetPixel(i, j, Color.Transparent);
                        }
                    }
                }
                final.Save(context.Response.OutputStream, ImageFormat.Png);
            }
        }
    }

    Bitmap GenerateBitmap(string text)
    {
        Bitmap[] single = new Bitmap[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            single[i] = GenerateSingelCharacter(text[i]);
        }
        Size size = new Size(single.Sum(b => b.Width), single.Max(b => b.Height));
        Bitmap result = new Bitmap(size.Width, size.Height);
        using (Graphics g = Graphics.FromImage(result))
        {
            int xOffset = 0;
            foreach (Bitmap b in single)
            {
                g.DrawImage(b, new Point(xOffset, Rand.RAND.Next(size.Height - b.Height)));
                xOffset += b.Width;
                b.Dispose();
            }
        }
        return result;
    }

    Bitmap GenerateSingelCharacter(char ch)
    {
        Font font = new Font(FontFamily.GenericSansSerif, Rand.RAND.Next(60 - 30) + 30);
        Size size;
        using (Graphics g = Graphics.FromImage(sampleBitmap))
        {
            size = g.MeasureString("" + ch, font).ToSize();
        }
        Bitmap result = new Bitmap(size.Width, size.Height);
        using (Graphics g = Graphics.FromImage(result))
        {
            g.Clear(Color.Transparent);
            g.DrawString("" + ch, font, Brushes.Black,PointF.Empty);
            int? minX = null, minY = null, maxX = null, maxY = null;
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    if (result.GetPixel(x, y).A > 250)
                    {
                        minX = minX == null ? x : Math.Min(x, (int)minX);
                        minY = minY == null ? y : Math.Min(y, (int)minY);
                        maxX = maxX == null ? x : Math.Max(x, (int)maxX);
                        maxY = maxY == null ? y : Math.Max(y, (int)maxY);
                    }
                }
            }
            //留白
            minX -= 3;
            minY -= 3;
            maxX += 3;
            maxY += 3;

            //左闭右开
            maxX++;
            maxY++;

            Bitmap old = result;
            result = new Bitmap((int)(maxX - minX), (int)(maxY - minY));
            using (Graphics newG = Graphics.FromImage(result))
            {
                newG.Clear(Color.Transparent);
                newG.DrawImage(old, new Point(-(int)minX, -(int)minY));
            }
            old.Dispose();
        }
        using (Bitmap cloned = (Bitmap)result.Clone())
        {
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                Point ul = new Point(Rand.RAND.Next(result.Width / 4), Rand.RAND.Next(result.Height / 4));
                Point ur = new Point(result.Width - Rand.RAND.Next(result.Width / 4), Rand.RAND.Next(result.Height / 4));
                Point dl = new Point(Rand.RAND.Next(result.Width / 4), result.Height - Rand.RAND.Next(result.Height / 4));
                g.DrawImage(cloned, new Point[] { ul, ur, dl });
            }
        }
        return result;
    }

    public bool IsReusable
    {
        get
        {
            return true;
        }
    }
}