using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using static System.Windows.Media.Imaging.WriteableBitmapExtensions;
using System.Windows;

namespace MVVMPaintApp.Models
{
    public static class WriteableExtensionMethods
    {
        public static void FillEllipseCenteredAA(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, Color color)
        {
            int color2 = WriteableBitmapExtensions.ConvertColor(color);
            bmp.FillEllipseCenteredAA(xc, yc, xr, yr, color2);
        }

        public unsafe static void FillEllipseCenteredAA(this WriteableBitmap bmp, int xc, int yc, int xr, int yr, int color, bool doAlphaBlend = false)
        {
            using BitmapContext bitmapContext = bmp.GetBitmapContext();
            int* pixels = bitmapContext.Pixels;
            int width = bitmapContext.Width;
            int height = bitmapContext.Height;

            if (xr < 1 || yr < 1 || xc - xr >= width || xc + xr < 0 || yc - yr >= height || yc + yr < 0)
            {
                return;
            }

            int baseAlpha = (color >> 24) & 0xFF;
            int sr = (color >> 16) & 0xFF;
            int sg = (color >> 8) & 0xFF;
            int sb = color & 0xFF;

            // Increase the softness of the edge
            const float softEdgeWidth = 3.0f;  // Increased from 1.0 for softer edges

            // Add padding to account for the wider soft edge
            int padding = (int)Math.Ceiling(softEdgeWidth * Math.Max(xr, yr) / 2);

            float rxSquared = xr * xr;
            float rySquared = yr * yr;

            int minX = Math.Max(0, xc - xr - padding);
            int maxX = Math.Min(width - 1, xc + xr + padding);
            int minY = Math.Max(0, yc - yr - padding);
            int maxY = Math.Min(height - 1, yc + yr + padding);

            for (int y = minY; y <= maxY; y++)
            {
                int rowOffset = y * width;
                float dy = y - yc;
                float dySquared = dy * dy;

                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - xc;
                    float dxSquared = dx * dx;

                    // Calculate distance from ellipse center, normalized by radii
                    float distance = (dxSquared / rxSquared) + (dySquared / rySquared);

                    // Apply smooth falloff using a cosine curve
                    if (distance <= 1.0f + softEdgeWidth)
                    {
                        float alpha;
                        if (distance <= 1.0f)
                        {
                            // Inside the ellipse
                            if (distance > 0.5f)
                            {
                                // Smooth transition starting from halfway point
                                float t = (distance - 0.5f) / 0.5f;
                                alpha = 1.0f - (0.5f * (1.0f - (float)Math.Cos(t * Math.PI)));
                            }
                            else
                            {
                                // Fully opaque in the center
                                alpha = 1.0f;
                            }
                        }
                        else
                        {
                            // Outside the ellipse - smooth falloff
                            float t = (distance - 1.0f) / softEdgeWidth;
                            alpha = 0.5f * (1.0f + (float)Math.Cos(t * Math.PI));
                        }

                        int pixelAlpha = (int)(alpha * baseAlpha);

                        if (pixelAlpha > 0)
                        {
                            if (doAlphaBlend || baseAlpha < 255)
                            {
                                pixels[x + rowOffset] = AlphaBlendColors(pixels[x + rowOffset], pixelAlpha, sr, sg, sb);
                            }
                            else
                            {
                                pixels[x + rowOffset] = (pixelAlpha << 24) | (sr << 16) | (sg << 8) | sb;
                            }
                        }
                    }
                }
            }
        }

        private static int AlphaBlendColors(int backdrop, int alpha, int sr, int sg, int sb)
        {
            if (alpha == 0) return backdrop;
            if (alpha == 255) return (alpha << 24) | (sr << 16) | (sg << 8) | sb;

            int br = (backdrop >> 16) & 0xFF;
            int bg = (backdrop >> 8) & 0xFF;
            int bb = backdrop & 0xFF;
            int ba = (backdrop >> 24) & 0xFF;

            float a = alpha / 255.0f;
            float inverseAlpha = 1.0f - a;

            int r = (int)(sr * a + br * inverseAlpha);
            int g = (int)(sg * a + bg * inverseAlpha);
            int b = (int)(sb * a + bb * inverseAlpha);
            int finalAlpha = (int)(alpha + ba * inverseAlpha);

            return (finalAlpha << 24) | (r << 16) | (g << 8) | b;
        }

        public unsafe static void BlitAlphaOverlay(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect, Color color)
        {
            if (color.A == 0)
            {
                return;
            }

            int destWidth = (int)destRect.Width;
            int destHeight = (int)destRect.Height;

            using BitmapContext src = source.GetBitmapContext(ReadWriteMode.ReadOnly);
            using BitmapContext dest = bmp.GetBitmapContext();

            int srcWidth = src.Width;
            int destBmpWidth = dest.Width;
            int destBmpHeight = dest.Height;

            // Calculate intersection with destination bounds
            Rect bounds = new Rect(0.0, 0.0, destBmpWidth, destBmpHeight);
            bounds.Intersect(destRect);
            if (bounds.IsEmpty)
            {
                return;
            }

            int* srcPixels = src.Pixels;
            int* destPixels = dest.Pixels;
            int srcLength = src.Length;

            int srcIndex = -1;
            int destX = (int)destRect.X;
            int destY = (int)destRect.Y;

            // Tinting color components
            int tintA = color.A;
            bool hasTint = color != Colors.White;

            // Scaling factors
            double scaleX = sourceRect.Width / destRect.Width;
            double scaleY = sourceRect.Height / destRect.Height;
            int srcStartX = (int)sourceRect.X;
            int srcStartY = (int)sourceRect.Y;

            double currentY = srcStartY;
            int currentDestY = destY;

            for (int y = 0; y < destHeight; y++)
            {
                if (currentDestY >= 0 && currentDestY < destBmpHeight)
                {
                    double currentX = srcStartX;
                    int destPos = destX + currentDestY * destBmpWidth;
                    int currentDestX = destX;

                    for (int x = 0; x < destWidth; x++)
                    {
                        if (currentDestX >= 0 && currentDestX < destBmpWidth)
                        {
                            srcIndex = (int)currentX + (int)currentY * srcWidth;
                            if (srcIndex >= 0 && srcIndex < srcLength)
                            {
                                int srcPixel = srcPixels[srcIndex];
                                int srcA = (srcPixel >> 24) & 0xFF;

                                if (srcA > 0)
                                {
                                    if (hasTint)
                                    {
                                        srcA = srcA * tintA / 255;
                                    }

                                    int destPixel = destPixels[destPos];
                                    int destA = (destPixel >> 24) & 0xFF;

                                    // Simple alpha compositing while preserving source colors
                                    if (destA == 0)
                                    {
                                        destPixels[destPos] = srcPixel;
                                    }
                                    else if (srcA > 0)
                                    {
                                        int srcR = (srcPixel >> 16) & 0xFF;
                                        int srcG = (srcPixel >> 8) & 0xFF;
                                        int srcB = srcPixel & 0xFF;

                                        int destR = (destPixel >> 16) & 0xFF;
                                        int destG = (destPixel >> 8) & 0xFF;
                                        int destB = destPixel & 0xFF;

                                        // Blend based on source alpha while preserving colors
                                        int newA = srcA + (destA * (255 - srcA) / 255);
                                        int newR = (srcR * srcA + destR * (255 - srcA)) / 255;
                                        int newG = (srcG * srcA + destG * (255 - srcA)) / 255;
                                        int newB = (srcB * srcA + destB * (255 - srcA)) / 255;

                                        destPixels[destPos] = (newA << 24) | (newR << 16) | (newG << 8) | newB;
                                    }
                                }
                            }
                        }

                        currentDestX++;
                        destPos++;
                        currentX += scaleX;
                    }
                }

                currentY += scaleY;
                currentDestY++;
            }
        }
    }
}
