﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using ImageMagick;

namespace AssetsPV
{
    class OrthoSingle
    {
        public static bool EFFECTS = false;
        const bool FORAYS = false;
        const bool WAR = true;
        const bool RENDER = true;
        const bool USE_PALETTE = true;
        const bool ENABLE_SUPER = false;

        const bool ENABLE_STANDING = true;
        const bool ENABLE_FIRING = true;
        const bool ENABLE_DYING = true;

        public static string addon = TallFacesDithered.addon; // "" // "Zombie_" // "Divine_"
        public const int multiplier = 1, bonus = 1, vwidth = 1, vheight = 1, top = 0,
            widthLarge = (30 * multiplier * vwidth + 1), heightLarge = ((30 + 30) * multiplier * (vheight - top) + 1),
            widthTiny = 11 * multiplier * vwidth + 1, heightTiny = (10 + 11 / 2) * multiplier * (vheight - top) + 1,
            //widthSmall = 60 * vwidth + 2, heightSmall = (60 + 60/2) * (vheight - top) + 2,
            widthHuge = (60 * multiplier * vwidth + 1), heightHuge = ((40 + 60) * multiplier * (vheight - top) + 1),
            widthMassive = (80 * multiplier * vwidth + 1), heightMassive = ((60 + 80) * multiplier * (vheight - top) + 1);
        public static PRNG r = PRNG.r;
        public static ulong rState = PRNG.rState, altState = PRNG.altState;
        public static string altFolder = "", blankFolder = "";

        //private static FileStream offbin = new FileStream("offsets3dither.bin", FileMode.OpenOrCreate);
        //private static BinaryWriter offsets = new BinaryWriter(offbin);
        public static Tuple<string, int>[] undead = TallVoxels.undead,
            living = TallVoxels.living,
            hats = TallVoxels.hats,
            ghost_hats = TallVoxels.ghost_hats,
            terrain = TallVoxels.terrain,
            landscape = TallVoxels.landscape;
        public static string[] colorNames = new string[] { "Dark", "White", "Red", "Orange", "Yellow", "Green", "Blue", "Purple" };
        public static string[] classes = TallVoxels.classes;
        private static byte[][][] storeColorCubesWBold()
        {
            r.FromSnapshot(rState);
            VoxelLogic.wpalettecount = VoxelLogic.wpalettes.Length;
            //            wcolorcount = VoxelLogic.wpalettes[0].Length;
            int width = vwidth;
            int height = vheight + 3;
            byte[,,] cubes = new byte[VoxelLogic.wpalettecount, VoxelLogic.wpalettes[0].Length, 4 * width * height];

            Image image = new Bitmap("white.png");
            ImageAttributes imageAttributes = new ImageAttributes();

            float[][] colorMatrixElements = {
   new float[] {1F, 0,  0,  0,  0},
   new float[] {0, 1F,  0,  0,  0},
   new float[] {0,  0,  1F, 0,  0},
   new float[] {0,  0,  0,  1F, 0},
   new float[] {0,  0,  0,  0, 1F}};

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);
            for(int p = 0; p < VoxelLogic.wpalettes.Length; p++)
            {
                for(int current_color = 0; current_color < VoxelLogic.wpalettes[0].Length; current_color++)
                {
                    if(current_color >= VoxelLogic.wpalettes[p].Length)
                        continue;

                    Bitmap b =
                    new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    Graphics g = Graphics.FromImage((Image)b);

                    if(VoxelLogic.wpalettes[p][current_color][3] == 0F)
                    {
                        colorMatrix = new ColorMatrix(new float[][]{
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 1F}});
                    }
                    else if(!VoxelLogic.terrainPalettes.Contains(p) && current_color == 25)
                    {
                        colorMatrix = new ColorMatrix(new float[][]{
   new float[] {0.22F+VoxelLogic.wpalettes[p][current_color][0],  0,  0,  0, 0},
   new float[] {0,  0.251F+VoxelLogic.wpalettes[p][current_color][1],  0,  0, 0},
   new float[] {0,  0,  0.31F+VoxelLogic.wpalettes[p][current_color][2],  0, 0},
   new float[] {0,  0,  0,  1, 0},
   new float[] {0, 0, 0, 0, 1F}});
                    }
                    else if(VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.eraser_alpha)
                    {
                        colorMatrix = new ColorMatrix(new float[][]{
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  0, 0},
   new float[] {0,  0,  0,  1F, 0},
   new float[] {0,  0,  0,  0, 1F}});
                    }
                    else
                    {
                        colorMatrix = new ColorMatrix(new float[][]{
   new float[] {0.235F+VoxelLogic.wpalettes[p][current_color][0],  0,  0,  0, 0},
   new float[] {0,  0.26F+VoxelLogic.wpalettes[p][current_color][1],  0,  0, 0},
   new float[] {0,  0,  0.30F+VoxelLogic.wpalettes[p][current_color][2],  0, 0},
   new float[] {0,  0,  0,  1F, 0},
   new float[] {0, 0, 0, 0, 1F}});
                    }
                    imageAttributes.SetColorMatrix(
                       colorMatrix,
                       ColorMatrixFlag.Default,
                       ColorAdjustType.Bitmap);

                    string which_image = ((!VoxelLogic.terrainPalettes.Contains(p) && ((current_color >= 18 && current_color <= 20) || (VoxelLogic.VisualMode != "CU" && current_color == 40))) || VoxelLogic.wpalettes[p][current_color][3] == 0F
                        || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha
                        || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha_0 || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha_1) ? "shine" :
                       (VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flat_alpha || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.bordered_flat_alpha) ? "flat" : "image";
                    g.DrawImage(image,
                       new Rectangle(0, 0,
                           width, height),  // destination rectangle 
                                            //                   new Rectangle((vx.x + vx.y) * 4, 128 - 6 - 32 - vx.y * 2 + vx.x * 2 - 4 * vx.z, width, height),  // destination rectangle 
                       0, 0,        // upper-left corner of source rectangle 
                       width,       // width of source rectangle
                       height,      // height of source rectangle
                       GraphicsUnit.Pixel,
                       imageAttributes);
                    for(int i = 0; i < width; i++)
                    {
                        for(int j = 0; j < height; j++)
                        {
                            Color c = b.GetPixel(i, j);
                            double h = 0.0, s = 1.0, v = 1.0;
                            bool subtle = VoxelLogic.subtlePalettes.Contains(p);
                            VoxelLogic.ColorToHSV(c, out h, out s, out v);
                            if(subtle)
                            {
                                s = VoxelLogic.Clamp((s * 0.6), 0.0, 0.6);
                                v = VoxelLogic.Clamp(v * 0.95, 0.01, 0.95);
                            }
                            else if(current_color == 40 && VoxelLogic.VisualMode == "CU" && !VoxelLogic.terrainPalettes.Contains(p))
                            {
                                h += (v + s) * 0.075 - 0.11;
                                v *= 0.8 + r.NextDouble() * 0.18;
                            }
                                Color c2 = Color.Transparent;
                                double s_alter = (s * 0.78 + s * s * s * Math.Sqrt(s)),
                                        v_alter = Math.Pow(v, 2.0 - 2.0 * v);
                                v_alter *= Math.Pow(v_alter, 0.38);

                            if(j == height - 1)
                            {
                                if(subtle)
                                    c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp((s + s * s * s * Math.Pow(s, 0.3)) * 1.45, 0.00625, 0.8), VoxelLogic.Clamp(v_alter * 0.725, 0.02, 1.0));
                                else
                                    c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp((s + s * s * s * Math.Pow(s, 0.3)) * 1.55, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.65, 0.01, 1.0));
                            }
                            else
                            {
                                switch(j)
                                {
                                    case 0:
                                        c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.05, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 1.1, 0.09, 1.0));
                                        break;
                                    case 1:
                                        c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.2, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.95, 0.06, 1.0));
                                        break;
                                    case 2:
                                        c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.35, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.8, 0.03, 1.0));
                                        break;
                                }
                            }
                            if(c.A != 0)
                            {
                                cubes[p, current_color, i * 4 + j * width * 4 + 0] = Math.Max((byte)1, c2.B);
                                cubes[p, current_color, i * 4 + j * width * 4 + 1] = Math.Max((byte)1, c2.G);
                                cubes[p, current_color, i * 4 + j * width * 4 + 2] = Math.Max((byte)1, c2.R);
                                cubes[p, current_color, i * 4 + j * width * 4 + 3] = c2.A;
                            }
                            else
                            {
                                cubes[p, current_color, i * 4 + j * 4 * width + 0] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 1] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 2] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 3] = 0;
                            }
                        }
                    }
                    /*
                    if(current_color == 40 && VoxelLogic.VisualMode == "CU" && !VoxelLogic.terrainPalettes.Contains(p))
                    {
                        int avgR = 0, avgG = 0, avgB = 0;
                        for(int i = 0; i < width; i++)
                        {
                            for(int j = 0; j < height; j++)
                            {
                                avgR += cubes[p, current_color, i * 4 + j * width * 4 + 2];
                                avgG += cubes[p, current_color, i * 4 + j * width * 4 + 1];
                                avgB += cubes[p, current_color, i * 4 + j * width * 4 + 0];
                            }
                        }
                        avgR /= (width * height);
                        avgG /= (width * height);
                        avgB /= (width * height);


                        for(int i = 0; i < width; i++)
                        {
                            for(int j = 0; j < height; j++)
                            {
                                if(cubes[p, current_color, i * 4 + j * width * 4 + 3] != 0)
                                {
                                    cubes[p, current_color, i * 4 + j * width * 4 + 2] = (byte)((cubes[p, current_color, i * 4 + j * width * 4 + 2] * 2 + avgR) / 3);
                                    cubes[p, current_color, i * 4 + j * width * 4 + 1] = (byte)((cubes[p, current_color, i * 4 + j * width * 4 + 1] * 2 + avgG) / 3);
                                    cubes[p, current_color, i * 4 + j * width * 4 + 0] = (byte)((cubes[p, current_color, i * 4 + j * width * 4 + 0] * 2 + avgB) / 3);
                                }
                            }
                        }
                    }*/
                }

            }

            byte[][][] cubes2 = new byte[VoxelLogic.wpalettes.Length][][];
            VoxelLogic.wrendered = new byte[VoxelLogic.wpalettes.Length][][];
            for(int i = 0; i < VoxelLogic.wpalettes.Length; i++)
            {
                cubes2[i] = new byte[VoxelLogic.wpalettes[0].Length][];
                VoxelLogic.wrendered[i] = new byte[VoxelLogic.wpalettes[0].Length][];
                for(int c = 0; c < VoxelLogic.wpalettes[0].Length; c++)
                {
                    cubes2[i][c] = new byte[4 * width * height];
                    VoxelLogic.wrendered[i][c] = new byte[4 * width * height];
                    for(int j = 0; j < 4 * width * height; j++)
                    {
                        cubes2[i][c][j] = cubes[i, c, j];
                        VoxelLogic.wrendered[i][c][j] = cubes[i, c, j];
                    }
                }
            }
        
            return cubes2;
        }


        private static byte[][][] storeIndexCubesW()
        {
            VoxelLogic.wpalettecount = VoxelLogic.wpalettes.Length;
            //            wcolorcount = VoxelLogic.wpalettes[0].Length;
            // 17 is the number of Slope enum types.

            int width = vwidth;
            int height = vheight + 3;
            byte[,,] cubes = new byte[VoxelLogic.wpalettecount, VoxelLogic.wpalettes[0].Length, 4 * width * height];


            for(int p = 0; p < VoxelLogic.wpalettes.Length; p++)
            {
                for(int current_color = 0; current_color < VoxelLogic.wpalettes[0].Length; current_color++)
                {
                    if(current_color >= VoxelLogic.wpalettes[p].Length)
                        continue;

                    byte topi = (byte)(253 - current_color * 4);
                    byte brighti = (byte)(topi - 1);
                    byte dimi = (byte)(topi - 2);
                    byte darki = (byte)(topi - 3);
                    string which_image = (((current_color >= 18 && current_color <= 20) || (VoxelLogic.VisualMode != "CU" && current_color == 40)) || VoxelLogic.wpalettes[p][current_color][3] == 0F
                        || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha
                        || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha_0 || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flash_alpha_1) ? "shine" :
                       (VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.flat_alpha || VoxelLogic.wpalettes[p][current_color][3] == VoxelLogic.bordered_flat_alpha) ? "flat" : "image";
                    if(VoxelLogic.terrainPalettes.Contains(p)) which_image = "image";
                    for(int i = 0; i < width; i++)
                    {
                        for(int j = 0; j < height; j++)
                        {
                            byte[] c2 = new byte[] { 0, 0, 0, 0 };

                            if(j == height - 1)
                            {
                                c2 = new byte[] { darki, darki, darki, darki };
                                // c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp((s + s * s * s * Math.Pow(s, 0.3)) * 1.55, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.5, 0.01, 1.0));
                            }
                            else
                            {
                                if(which_image.Equals("image"))
                                {

                                    switch(j)
                                    {
                                        case 0:
                                            c2 = new byte[] { topi, brighti, dimi, darki };
                                            //c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.05, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 1.1, 0.09, 1.0));
                                            break;
                                        case 1:
                                            c2 = new byte[] { brighti, brighti, brighti, brighti };
                                            // c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.2, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.95, 0.06, 1.0));
                                            break;
                                        case 2:
                                            c2 = new byte[] { dimi, dimi, dimi, dimi };
                                            //c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s_alter * 1.35, 0.0112, 1.0), VoxelLogic.Clamp(v_alter * 0.8, 0.03, 1.0));
                                            break;
                                    }
                                }
                                else if(which_image == "shine")
                                {
                                    c2 = new byte[] { topi, topi, topi, topi };
                                    //c2 = VoxelLogic.ColorFromHSV(h, VoxelLogic.Clamp(s * 0.9, 0.0112, 1.0), VoxelLogic.Clamp(v * 1.1, 0.1, 1.0));
                                }
                                else if(which_image == "flat")
                                {
                                    if(current_color == 27)
                                    {
                                        if(j >= top)
                                        {
                                            c2 = new byte[] { brighti, dimi, topi, dimi };
                                        }
                                    }
                                    else
                                    {
                                        if(j >= top)
                                        {
                                            c2 = new byte[] { brighti, brighti, brighti, brighti };
                                        }
                                    }

                                }
                            }
                            if(c2[0] != 0)
                            {
                                cubes[p, current_color, i * 4 + j * width * 4 + 0] = c2[0];
                                cubes[p, current_color, i * 4 + j * width * 4 + 1] = c2[1];
                                cubes[p, current_color, i * 4 + j * width * 4 + 2] = c2[2];
                                cubes[p, current_color, i * 4 + j * width * 4 + 3] = c2[3];
                            }
                            else
                            {
                                cubes[p, current_color, i * 4 + j * 4 * width + 0] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 1] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 2] = 0;
                                cubes[p, current_color, i * 4 + j * 4 * width + 3] = 0;
                            }

                        }
                    }
                }
            }
            byte[][][] cubes2 = new byte[VoxelLogic.wpalettes.Length][][];
            for(int i = 0; i < VoxelLogic.wpalettes.Length; i++)
            {
                cubes2[i] = new byte[VoxelLogic.wpalettes[0].Length][];
                for(int c = 0; c < VoxelLogic.wpalettes[0].Length; c++)
                {
                    cubes2[i][c] = new byte[4 * width * height];
                    for(int j = 0; j < 4 * width * height; j++)
                    {
                        cubes2[i][c][j] = cubes[i, c, j];
                    }
                }
            }
            
            return cubes2;
        }
        public static byte[][][] wrendered, wdithered;
        public static byte[][] wcurrent, wditheredcurrent;

        public static byte[][][] simplepalettes;
        public static byte[][] basepalette, exactpalettes;

        public static void InitializeWPalette()
        {
            if(VoxelLogic.VisualMode == "CU")
            {
                Directory.CreateDirectory(blankFolder + "receiving/");
                foreach(string u in VoxelLogic.CurrentUnits)
                {
                    Directory.CreateDirectory(blankFolder + u);
                }
                foreach(string u in CURedux.WeaponTypes)
                {
                    Directory.CreateDirectory(blankFolder + "receiving/" + u);
                }
                Directory.CreateDirectory(blankFolder + "receiving/Explosion");

                for(int p = 0; p < 8 * (CURedux.wspecies.Length - 2 - (CURedux.ZOMBIES ? 0 : 1)); p++)
                {
                    foreach(string u in VoxelLogic.CurrentUnits)
                    {
                        Directory.CreateDirectory(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/color" + p + "/" + u);
                        Directory.CreateDirectory(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/color" + p + "/" + u);
                    }
                    foreach(string u in CURedux.WeaponTypes)
                    {
                        if(p < 8 || (p >= 208 && p < 37 * 8))
                            Directory.CreateDirectory(altFolder + ((p >= 208) ? "Alien/" : "") + TallFacesDithered.colorNames[p % 8] + "/receiving/color" + p + "/" + u);
                        if(p >= 38 * 8)
                            Directory.CreateDirectory(altFolder + "Divine/" + TallFacesDithered.colorNames[p % 8] + "/receiving/color" + p + "/" + u);
                    }
                    if(p < 8 || (p >= 208 && p < 37 * 8))
                        Directory.CreateDirectory(altFolder + ((p >= 208) ? "Alien/" : "") + TallFacesDithered.colorNames[p % 8] + "/receiving/color" + p + "/Explosion");
                    if(p >= 38 * 8)
                        Directory.CreateDirectory(altFolder + "Divine/" + TallFacesDithered.colorNames[p % 8] + "/receiving/color" + p + "/Explosion");
                }
            }
            wrendered = storeColorCubesWBold();
            simplepalettes = new byte[wrendered.Length][][];
            exactpalettes = new byte[wrendered.Length][];
            Crc32 crc = new Crc32();
            int colorcount = Math.Min(VoxelLogic.wpalettes[0].Length, 63);
            for(int i = 0; i < simplepalettes.Length; i++)
            {
                simplepalettes[i] = new byte[256][];
                exactpalettes[i] = new byte[776];
                exactpalettes[i][0] = 0x50;
                exactpalettes[i][1] = 0x4C;
                exactpalettes[i][2] = 0x54;
                exactpalettes[i][3] = 0x45;
                for(int j = 253, c = 0, k = 253 * 3 + 6; j >= 4 && c < colorcount; c++, j -= 4)
                {
                    simplepalettes[i][j] = new byte[] { wrendered[i][c][2], wrendered[i][c][1], wrendered[i][c][0] };
                    exactpalettes[i][k--] = wrendered[i][c][0];
                    exactpalettes[i][k--] = wrendered[i][c][1];
                    exactpalettes[i][k--] = wrendered[i][c][2];
                    simplepalettes[i][j - 1] = new byte[] { wrendered[i][c][2 + 4], wrendered[i][c][1 + 4], wrendered[i][c][0 + 4] };
                    exactpalettes[i][k--] = wrendered[i][c][4];
                    exactpalettes[i][k--] = wrendered[i][c][5];
                    exactpalettes[i][k--] = wrendered[i][c][6];
                    simplepalettes[i][j - 2] = new byte[] { wrendered[i][c][2 + 8], wrendered[i][c][1 + 8], wrendered[i][c][0 + 8] };
                    exactpalettes[i][k--] = wrendered[i][c][8];
                    exactpalettes[i][k--] = wrendered[i][c][9];
                    exactpalettes[i][k--] = wrendered[i][c][10];
                    simplepalettes[i][j - 3] = new byte[] { wrendered[i][c][2 + 12], wrendered[i][c][1 + 12], wrendered[i][c][0 + 12] };
                    exactpalettes[i][k--] = wrendered[i][c][12];
                    exactpalettes[i][k--] = wrendered[i][c][13];
                    exactpalettes[i][k--] = wrendered[i][c][14];
                }
                simplepalettes[i][0] = new byte[] { 0, 0, 0 };
                exactpalettes[i][0 + 4] = 0;
                exactpalettes[i][1 + 4] = 0;
                exactpalettes[i][2 + 4] = 0;
                simplepalettes[i][1] = new byte[] { 0, 0, 0 };
                exactpalettes[i][3 + 4] = 0;
                exactpalettes[i][4 + 4] = 0;
                exactpalettes[i][5 + 4] = 0;
                simplepalettes[i][254] = new byte[] { 0, 0, 0 };
                exactpalettes[i][254 * 3 + 4] = 0;
                exactpalettes[i][254 * 3 + 1 + 4] = 0;
                exactpalettes[i][254 * 3 + 2 + 4] = 0;
                simplepalettes[i][255] = new byte[] { 255, 255, 255 };
                exactpalettes[i][255 * 3 + 4] = 255;
                exactpalettes[i][255 * 3 + 1 + 4] = 255;
                exactpalettes[i][255 * 3 + 2 + 4] = 255;
                exactpalettes[i] = crc.appendChecksum(exactpalettes[i]);
            }
            basepalette = new byte[256][];
            for(byte i = 1; i < 255; i++)
            {
                basepalette[i] = new byte[] { i, i, i };
            }

            wdithered = storeIndexCubesW();
            wcurrent = wrendered[0];
            wditheredcurrent = wdithered[0];
        }
        /*
        public static void InitializeWPalette()
        {
            if(VoxelLogic.VisualMode == "CU")
            {
                for(int p = 0; p < 8 * CURedux.wspecies.Length; p++)
                {
                    Directory.CreateDirectory(altFolder + "color" + p);
                    Directory.CreateDirectory("frames/" + altFolder + "color" + p);
                    Directory.CreateDirectory("frames/" + altFolder + "receiving/" + "color" + p);
                }
            }
            wrendered = storeColorCubesWBold();
            simplepalettes = new byte[wrendered.Length][][];
            int colorcount = Math.Min(VoxelLogic.wpalettes[0].Length, 63);
            for(int i = 0; i < simplepalettes.Length; i++)
            {
                simplepalettes[i] = new byte[256][];
                for(int j = 253, c = 0; j >= 4 && c < colorcount; c++, j -= 4)
                {
                    simplepalettes[i][j] = new byte[] { wrendered[i][c][2], wrendered[i][c][1], wrendered[i][c][0] };
                    simplepalettes[i][j - 1] = new byte[] { wrendered[i][c][2 + 4 * vwidth], wrendered[i][c][1 + 4 * vwidth], wrendered[i][c][0 + 4 * vwidth] };
                    simplepalettes[i][j - 2] = new byte[] { wrendered[i][c][2 + 8 * vwidth], wrendered[i][c][1 + 8 * vwidth], wrendered[i][c][0 + 8 * vwidth] };
                    simplepalettes[i][j - 3] = new byte[] { wrendered[i][c][2 + 12 * vwidth], wrendered[i][c][1 + 12 * vwidth], wrendered[i][c][0 + 12 * vwidth] };
                }
                simplepalettes[i][0] = new byte[] { 0, 0, 0 };
                simplepalettes[i][1] = new byte[] { 0, 0, 0 };
                simplepalettes[i][254] = new byte[] { 0, 0, 0 };
                simplepalettes[i][255] = new byte[] { 0, 0, 0 };
            }
            basepalette = new byte[256][];
            for(byte i = 1; i < 255; i++)
            {
                basepalette[i] = new byte[] { i, i, i };
            }

            wdithered = storeIndexCubesW();
            wcurrent = wrendered[0];
            wditheredcurrent = wdithered[0];
        }
        */
        public static void WritePNG(PngWriter png, byte[][] b, byte[][] palette)
        {
            png.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
            PngChunkPLTE pal = png.GetMetadata().CreatePLTEChunk();

            pal.SetNentries(256);
            for(int i = 1; i < 255; i++)
            {
                pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
            }
            pal.SetEntry(0, 0, 0, 0);
            pal.SetEntry(254, 0, 0, 0);
            pal.SetEntry(255, 255, 255, 255);
            png.WriteRowsByte(b);
            png.End();
        }

        public static void AlterPNGPalette(string input, string output, byte[][][] palettes)
        {
            byte[] bytes = File.ReadAllBytes(input);
            //Console.WriteLine("{0:X} {1:X} {2:X} TO {3:X} {4:X} {5:X}", bytes[41], bytes[42], bytes[43], bytes[41 + 255 * 3], bytes[42 + 255 * 3], bytes[43 + 255 * 3]);
            int p = (addon == "Divine_") ? 38 * 8 : 0, limit = 8 * (CURedux.wspecies.Length - 2 - (CURedux.ZOMBIES ? 0 : 1));
            for(; p < limit; p++)
            {
                Array.Copy(exactpalettes[p], 0, bytes, 37, 776);
                File.WriteAllBytes(
                    altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + string.Format(output, p),
                    bytes);

                if(!USE_PALETTE)
                    return;
            }
        }
        public static void AlterPNGPaletteZombie(string input, string output, byte[][][] palettes)
        {
            PngReader pngr = FileHelper.CreatePngReader(input);
            ImageLines lines = pngr.ReadRowsByte(0, pngr.ImgInfo.Rows, 1);
            ImageInfo imi = pngr.ImgInfo;

            pngr.End();

            //foreach(int p in new int[] { 2,78})
            for(int p = 8 * 37; p < 8 * 38; p++)
            {
                byte[][] palette = palettes[p];
                PngWriter pngw = FileHelper.CreatePngWriter(altFolder + "Zombie/" + TallFacesDithered.colorNames[p % 8] + "/" + string.Format(output, p), imi, true);

                if(pngw.GetChunksList().GetById1(PngChunkTRNS.ID) == null)
                    pngw.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
                else
                    pngw.GetMetadata().GetTRNS().setIndexEntryAsTransparent(0);

                /*int[] alphas = new int[256];
                alphas.Fill(255);
                alphas[0] = 0;
                
                //alphas[150] = 127;
                //alphas[151] = 127;
                //alphas[152] = 127;
                //alphas[153] = 127;
                
                pngw.GetMetadata().CreateTRNSChunk().SetPalletteAlpha(alphas);
                */

                PngChunkPLTE pal;
                if(pngw.GetChunksList().GetById1(PngChunkPLTE.ID) == null)
                    pal = pngw.GetMetadata().CreatePLTEChunk();
                else
                    pal = pngw.GetMetadata().GetPLTE();

                pal.SetNentries(256);
                for(int i = 1; i < 255; i++)
                {
                    pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
                }
                pal.SetEntry(0, 0, 0, 0);
                pal.SetEntry(255, 0, 0, 0);
                pngw.WriteRowsByte(lines.ScanlinesB);
                pngw.End();
            }
        }


        public static void AlterPNGPaletteLimited(string input, string output, byte[][][] palettes)
        {
            byte[] bytes = File.ReadAllBytes(input);
            //Console.WriteLine("{0:X} {1:X} {2:X} TO {3:X} {4:X} {5:X}", bytes[41], bytes[42], bytes[43], bytes[41 + 255 * 3], bytes[42 + 255 * 3], bytes[43 + 255 * 3]);
            //int p = (addon == "Divine_") ? 38 * 8 : 0, limit = 8 * (CURedux.wspecies.Length - 2 - (CURedux.ZOMBIES ? 0 : 1));
            for(int p = 0; p < 8; p++)
            {
                Array.Copy(exactpalettes[p], 0, bytes, 37, 776);
                File.WriteAllBytes(
                    altFolder + TallFacesDithered.colorNames[p] + "/" + string.Format(output, p),
                    bytes);
            }
            for(int p = 208; p < 8 * 37; p++)
            {

                Array.Copy(exactpalettes[p], 0, bytes, 37, 776);
                File.WriteAllBytes(
                    altFolder + "Alien/" + TallFacesDithered.colorNames[p % 8] + "/" + string.Format(output, p),
                    bytes);
            }
        }
        /*
        public static void AlterPNGPalette(string input, string output, byte[][][] palettes)
        {
            PngReader pngr = FileHelper.CreatePngReader(input);
            ImageLines lines = pngr.ReadRowsByte(0, pngr.ImgInfo.Rows, 1);
            ImageInfo imi = pngr.ImgInfo;

            pngr.End();

            //foreach(int p in new int[] { 2,78})
            int p = (addon == "Divine_") ? 38 * 8 : 0;
            for(; p < 8 * CURedux.wspecies.Length; p++)
            {
                byte[][] palette = palettes[p];
                PngWriter pngw = FileHelper.CreatePngWriter(string.Format(output, p), imi, true);

                pngw.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
                PngChunkPLTE pal = pngw.GetMetadata().CreatePLTEChunk();

                pal.SetNentries(256);
                for(int i = 1; i < 255; i++)
                {
                    pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
                }
                pal.SetEntry(0, 0, 0, 0);
                pal.SetEntry(255, 0, 0, 0);
                pngw.WriteRowsByte(lines.ScanlinesB);
                pngw.End();
            }
        }

        public static void AlterPNGPaletteZombie(string input, string output, byte[][][] palettes)
        {
            PngReader pngr = FileHelper.CreatePngReader(input);
            ImageLines lines = pngr.ReadRowsByte(0, pngr.ImgInfo.Rows, 1);
            ImageInfo imi = pngr.ImgInfo;

            pngr.End();

            //foreach(int p in new int[] { 2,78})
            for(int p = 8 * 37; p < 8 * 38; p++)
            {
                byte[][] palette = palettes[p];
                PngWriter pngw = FileHelper.CreatePngWriter(string.Format(output, p), imi, true);

                pngw.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
                
                PngChunkPLTE pal = pngw.GetMetadata().CreatePLTEChunk();

                pal.SetNentries(256);
                for(int i = 1; i < 255; i++)
                {
                    pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
                }
                pal.SetEntry(0, 0, 0, 0);
                pal.SetEntry(255, 0, 0, 0);
                pngw.WriteRowsByte(lines.ScanlinesB);
                pngw.End();
            }
        }
        
        public static void AlterPNGPaletteLimited(string input, string output, byte[][][] palettes)
        {
            PngReader pngr = FileHelper.CreatePngReader(input);
            ImageLines lines = pngr.ReadRowsByte(0, pngr.ImgInfo.Rows, 1);
            ImageInfo imi = pngr.ImgInfo;

            pngr.End();

            for(int p = 0; p < 8; p++)
            {
                byte[][] palette = palettes[p];
                PngWriter pngw = FileHelper.CreatePngWriter(string.Format(output, p), imi, true);

                pngw.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
                PngChunkPLTE pal = pngw.GetMetadata().CreatePLTEChunk();

                pal.SetNentries(256);
                for(int i = 1; i < 255; i++)
                {
                    pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
                }
                pal.SetEntry(0, 0, 0, 0);
                pal.SetEntry(255, 0, 0, 0);
                pngw.WriteRowsByte(lines.ScanlinesB);
                pngw.End();
            }
            for(int p = 208; p < 8 * CURedux.wspecies.Length; p++)
            {
                byte[][] palette = palettes[p];
                PngWriter pngw = FileHelper.CreatePngWriter(string.Format(output, p), imi, true);

                pngw.GetMetadata().CreateTRNSChunk().setIndexEntryAsTransparent(0);
                PngChunkPLTE pal = pngw.GetMetadata().CreatePLTEChunk();

                pal.SetNentries(256);
                for(int i = 1; i < 255; i++)
                {
                    pal.SetEntry(i, palette[i][0], palette[i][1], palette[i][2]);
                }
                pal.SetEntry(0, 0, 0, 0);
                pal.SetEntry(255, 0, 0, 0);
                pngw.WriteRowsByte(lines.ScanlinesB);
                pngw.End();
            }
        }
        */

        /*
        public static void writePaletteImages()
        {
            Directory.CreateDirectory("palettes");
            for(int c = 0; c < simplepalettes.Length; c++)
            {
                ImageInfo imi = new ImageInfo(256, 1, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter("palettes" + "/color" + c + ".png", imi, true);
                byte[][] b = new byte[1][];
                b[0] = new byte[256];
                for(int i = 0; i < 256; i++)
                {
                    b[0][i] = (byte)i;
                }
                WritePNG(png, b, simplepalettes[c]);
            }
        }
        */

        public static void writePaletteImages()
        {
            Directory.CreateDirectory(altFolder + "palettes");

            for(int c = 0; c < simplepalettes.Length; c++)
            {
                ImageInfo imi = new ImageInfo(256, 1, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(altFolder + "palettes" + "/color" + c + ".png", imi, true);
                byte[][] b = new byte[1][];
                b[0] = new byte[256];
                for(int i = 0; i < 256; i++)
                {
                    b[0][i] = (byte)i;
                }
                WritePNG(png, b, simplepalettes[c]);
            }
            if(VoxelLogic.VisualMode == "CU")
            {
                Bitmap bmp = new Bitmap(256, simplepalettes.Length - 88, PixelFormat.Format32bppArgb),
                    bmp2 = new Bitmap(256, simplepalettes.Length, PixelFormat.Format32bppArgb);

                Graphics g = Graphics.FromImage(bmp), g2 = Graphics.FromImage(bmp2);

                for(int c = 0; c < 208; c++)
                {
                    Image im = new Bitmap(altFolder + "palettes" + "/color" + c + ".png");
                    g.DrawImageUnscaled(im, 0, c);
                    g2.DrawImageUnscaled(im, 0, c);
                    im.Dispose();
                }

                for(int c = 208; c < 296; c++)
                {
                    Image im = new Bitmap(altFolder + "palettes" + "/color" + c + ".png");
                    g2.DrawImageUnscaled(im, 0, c);
                    im.Dispose();
                }

                for(int c = 296; c < simplepalettes.Length; c++)
                {
                    Image im = new Bitmap(altFolder + "palettes" + "/color" + c + ".png");
                    g.DrawImageUnscaled(im, 0, c - 88);
                    g2.DrawImageUnscaled(im, 0, c);
                    im.Dispose();
                }
                bmp.Save(altFolder + "palettes/HumansTerrains.png");
                bmp2.Save(altFolder + "palettes/HumansAliensTerrains.png");
                g.Dispose();
                g2.Dispose();
                bmp.Dispose();
                bmp2.Dispose();
            }
        }

        

        public static void WriteGIF(List<string> images, int delay, string output)
        {
            using(MagickImageCollection collection = new MagickImageCollection())
            {
                foreach(string s in images)
                {
                    MagickImage mimg = new MagickImage(s);
                    mimg.AnimationDelay = delay;
                    mimg.GifDisposeMethod = GifDisposeMethod.Background;
                    collection.Add(mimg);
                }
                //QuantizeSettings settings = new QuantizeSettings();
                //settings.Colors = 256;
                //collection.Quantize(settings);
                //collection.Optimize();
                // Save gif
                collection.Write(output + ".gif");
            }
        }

        public static void WriteAllGIFs()
        {
            Directory.CreateDirectory("gifs/" + altFolder);
            //CURedux.normal_units.AsParallel().ForAll(u =>
            foreach(string u in CURedux.normal_units)
            //new string[] { "Tank_P", "Tank_P_Alt" }.AsParallel().ForAll(u => 
            {
                if(u.Contains("_Alt")) continue;
                string stripped = /*u.Contains("_Alt") ? u.Substring(0, u.LastIndexOf("_Alt")) :*/ u;
                List<string> imageNames = new List<string>(4 * 16 * 8);
                if(ENABLE_STANDING)
                {
                    //foreach(int p in (stripped != u ? CURedux.humanAltHighlights : CURedux.humanHighlights))
                    {
                        int p = 32;
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                            }
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running standing GIF conversion for " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_Large_animated");
                }
                if(ENABLE_DYING)
                {
                    imageNames = new List<string>(4 * 16 * 12);
                    //foreach(int p in (stripped != u ? CURedux.humanAltHighlights : CURedux.humanHighlights))
                    {
                        int p = 32;

                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 12; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_death_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running death GIF conversion for " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_death_Large_animated");
                }
                if(ENABLE_FIRING)
                {
                    for(int w = 0; w < 2; w++)
                    {
                        if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[stripped]][w] <= -1)
                            continue;

                        imageNames = new List<string>(4 * 16 * 16);

                        //foreach(int p in (stripped != u ? CURedux.humanAltHighlights : CURedux.humanHighlights))
                        {
                            int p = 32;
                            for(int dir = 0; dir < 4; dir++)
                            {
                                for(int f = 0; f < 16; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                                }
                            }
                        }
                        Console.WriteLine("Running weapon " + w + " GIF conversion for " + u + "...");
                        WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_attack_" + w + "_Large_animated");
                    }
                }

                if(!u.Contains("_Alt"))
                {
                    int p = 281;
                    if(ENABLE_STANDING)
                    {
                        imageNames = new List<string>(4 * 16 * 8);
                        //foreach(int p in CURedux.alienHighlights)
                        {
                            for(int dir = 0; dir < 4; dir++)
                            {
                                for(int f = 0; f < 4; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                                }
                                for(int f = 0; f < 4; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                                }
                            }
                        }
                        Console.WriteLine("Running alien standing GIF conversion for " + u + "...");
                        WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_alien_Large_animated");
                    }
                    if(ENABLE_DYING)
                    {
                        imageNames = new List<string>(4 * 16 * 12);

                        //foreach(int p in CURedux.alienHighlights)
                        {
                            for(int dir = 0; dir < 4; dir++)
                            {
                                for(int f = 0; f < 12; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_death_" + f + ".png");
                                }
                            }
                        }
                        Console.WriteLine("Running alien death GIF conversion for " + u + "...");
                        WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_death_alien_Large_animated");
                    }
                    if(ENABLE_FIRING)
                    {
                        for(int w = 0; w < 2; w++)
                        {
                            if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                                continue;

                            imageNames = new List<string>(4 * 16 * 16);

                            //foreach(int p in CURedux.alienHighlights)
                            {

                                for(int dir = 0; dir < 4; dir++)
                                {
                                    for(int f = 0; f < 16; f++)
                                    {
                                        imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                                    }
                                }
                            }
                            Console.WriteLine("Running alien weapon " + w + " GIF conversion for " + u + "...");
                            WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_attack_" + w + "_alien_Large_animated");
                        }
                    }
                }
            }
            //});
            if(ENABLE_SUPER)
            {
                //CURedux.super_units.AsParallel().ForAll(u =>
                foreach(string u in CURedux.super_units)
                {
                    List<string> imageNames = new List<string>(4 * 16 * 4);
                    for(int p = 0; p < 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                            }
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running standing GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_Huge_animated");

                    imageNames = new List<string>(4 * 16 * 12);

                    for(int p = 0; p < 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 12; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_death_" + f + ".png");

                            }
                        }
                    }
                    Console.WriteLine("Running death GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_death_Huge_animated");

                    for(int w = 0; w < 2; w++)
                    {
                        if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                            continue;

                        imageNames = new List<string>(4 * 16 * 16);

                        for(int p = 0; p < 8; p++)
                        {
                            for(int dir = 0; dir < 4; dir++)
                            {
                                for(int f = 0; f < 16; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                                }
                            }
                        }
                        Console.WriteLine("Running weapon " + w + " GIF conversion for Super " + u + "...");
                        WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_attack_" + w + "_Huge_animated");
                    }

                    imageNames = new List<string>(4 * 16 * 4);
                    foreach(int p in CURedux.alienHighlights)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                            }
                            for(int f = 0; f < 4; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running alien standing GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_alien_Huge_animated");

                    imageNames = new List<string>(4 * 16 * 12);

                    foreach(int p in CURedux.alienHighlights)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 12; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_death_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running alien death GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_death_alien_Huge_animated");

                    for(int w = 0; w < 2; w++)
                    {
                        if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                            continue;

                        imageNames = new List<string>(4 * 16 * 16);

                        foreach(int p in CURedux.alienHighlights)
                        {
                            for(int dir = 0; dir < 4; dir++)
                            {
                                for(int f = 0; f < 16; f++)
                                {
                                    imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + colorNames[p % 8] + "/" + "/color" + p + "/" + u + "/" + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                                }
                            }
                        }
                        Console.WriteLine("Running alien weapon " + w + " GIF conversion for Super " + u + "...");
                        WriteGIF(imageNames, 10, "gifs/" + altFolder + u + "_attack_" + w + "_alien_Huge_animated");
                    }
                    //});
                }
            }
        }
        
        public static void WriteZombieGIFs()
        {
            Directory.CreateDirectory("gifs/" + altFolder);
            /*CURedux.normal_units.AsParallel().ForAll(u =>
            //foreach(string u in CURedux.normal_units)
            {
                string stripped = u.Contains("_Alt") ? u.Substring(0, u.LastIndexOf("_Alt")) : u;
                List<string> imageNames = new List<string>(4 * 8 * 4);
                for(int p = 37 * 8; p < 38 * 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + addon + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + addon + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for Zombie_" + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_zombie_Large_animated");

                imageNames = new List<string>(4 * 8 * 12);

                for(int p = 37 * 8; p < 38 * 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + addon + u + "_Large_face" + dir + "_death_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running death GIF conversion for Zombie_" + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_death_zombie_Large_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[stripped]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 8 * 16);

                    for(int p = 37 * 8; p < 38 * 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + stripped + "/" + addon + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for Zombie_" + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_attack_" + w + "_zombie_Large_animated");
                }
            });
            */
            CURedux.super_units.AsParallel().ForAll(u =>
            //foreach(string u in CURedux.super_units)
            {
                List<string> imageNames = new List<string>(4 * 8 * 8);
                for(int p = 37 * 8; p < 38 * 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for Super Zombie_" + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_zombie_Huge_animated");

                imageNames = new List<string>(4 * 8 * 12);

                for(int p = 37 * 8; p < 38 * 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + addon + u + "_Huge_face" + dir + "_death_" + f + ".png");

                        }
                    }
                }
                Console.WriteLine("Running death GIF conversion for Super Zombie_" + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_death_zombie_Huge_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 8 * 16);

                    for(int p = 37 * 8; p < 38 * 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add(altFolder + ((p >= 38 * 8) ? "Divine/" : ((p >= 37 * 8) ? "Zombie/" : ((p >= 208) ? "Alien/" : ""))) + TallFacesDithered.colorNames[p % 8] + "/" + "color" + p + "/" + u + "/" + addon + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for Super Zombie_" + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_attack_" + w + "_zombie_Huge_animated");
                }
            });
        }


        /*
        public static void WriteAllGIFs()
        {
            Directory.CreateDirectory("gifs/" + altFolder + "skin/");

            foreach(string u in CURedux.normal_units)
            {
                List<string> imageNames = new List<string>(4 * 16 * 4);
                foreach(int p in CURedux.humanHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_Large_animated");

                imageNames = new List<string>(4 * 16 * 12);

                foreach(int p in CURedux.humanHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_death_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running explosion GIF conversion for " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_death_Large_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 16 * 16);

                    foreach(int p in CURedux.humanHighlights)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for " + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_attack_" + w + "_Large_animated");
                }

                imageNames = new List<string>(4 * 16 * 4);
                foreach(int p in CURedux.alienHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_alien_Large_animated");

                imageNames = new List<string>(4 * 16 * 12);

                foreach(int p in CURedux.alienHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_death_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running explosion GIF conversion for " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_death_alien_Large_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 16 * 16);

                    foreach(int p in CURedux.alienHighlights)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for " + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_attack_" + w + "_alien_Large_animated");
                }
            }
            foreach(string u in CURedux.super_units)
            {
                List<string> imageNames = new List<string>(4 * 16 * 4);
                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for Super " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_Huge_animated");

                imageNames = new List<string>(4 * 16 * 12);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_death_" + f + ".png");

                        }
                    }
                }
                Console.WriteLine("Running explosion GIF conversion for Super " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_death_Huge_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 16 * 16);

                    for(int p = 0; p < 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_attack_" + w + "_Huge_animated");
                }

                imageNames = new List<string>(4 * 16 * 4);
                foreach(int p in CURedux.alienHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                        for(int f = 0; f < 4; f++)
                        {
                            imageNames.Add(altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running standing GIF conversion for Super " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_alien_Huge_animated");

                imageNames = new List<string>(4 * 16 * 12);

                foreach(int p in CURedux.alienHighlights)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add("frames/" + altFolder + "color" + p + "/" + u + "_Huge_face" + dir + "_death_" + f + ".png");
                        }
                    }
                }
                Console.WriteLine("Running explosion GIF conversion for Super " + u + "...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_death_alien_Huge_animated");

                for(int w = 0; w < 2; w++)
                {
                    if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] <= -1)
                        continue;

                    imageNames = new List<string>(4 * 16 * 16);

                    foreach(int p in CURedux.alienHighlights)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add("frames/" + altFolder + "/color" + p + "/" + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                            }
                        }
                    }
                    Console.WriteLine("Running weapon " + w + " GIF conversion for Super " + u + "...");
                    WriteGIF(imageNames, 11, "gifs/" + altFolder + "skin/" + u + "_attack_" + w + "_alien_Huge_animated");
                }
            }
        }
        */
        public static void processUnitTinyW(string u, int palette, bool still, bool shadowless)
        {

            Console.WriteLine("Processing: " + u + ", palette " + palette);

            BinaryReader bin = new BinaryReader(File.Open(altFolder + u + "_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxes = VoxelLogic.FromMagicaRaw(bin); //VoxelLogic.PlaceShadowsW(
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_" + palette + ".vox", voxes, "W", palette, 40, 40, 40);
            MagicaVoxelData[] parsed = voxes.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 3;
                parsed[i].y += 3;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            int framelimit = 4;


            string folder = (altFolder + "ortho/palette" + palette + "/");//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);
            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] colors = TransformLogic.SealGaps(TransformLogic.RotateYaw(TransformLogic.VoxListToArray(parsed, 22, 22, 20), dir * 90));
                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameTinyW(colors, palette, f, framelimit, still, shadowless);
                    ImageInfo imi = new ImageInfo(widthTiny, heightTiny, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + u + "_p" + palette + "_d" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                    // b.Save(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png", ImageFormat.Png);
                }
            }
            /*
            List<string> imageNames = new List<string>(4 * 8 * framelimit);
            for(int dir = 0; dir < 4; dir++)
            {
                for(int f = 0; f < framelimit; f++)
                {
                    imageNames.Add(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png");
                }
            }
            
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running GIF conversion ...");
            WriteGIF(imageNames, 25, "gifs/" + altFolder + u + "_Large_animated");
            */

            /*
            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = folder + "/palette" + palette + "_" + u + "_Large_face* ";
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Large_animated.gif";
            Process.Start(startInfo).WaitForExit();
            */
            //bin.Close();

            //            processFiringLarge(u);

            //processExplosionLargeW(u, palette, shadowless);

        }

        public static void processUnitManyTinyW(string u, bool still, bool shadowless)
        {

            Console.WriteLine("Processing Many: " + u);

            BinaryReader bin = new BinaryReader(File.Open(altFolder + u + "_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxes = VoxelLogic.FromMagicaRaw(bin); //VoxelLogic.PlaceShadowsW(
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_" + palette + ".vox", voxes, "W", palette, 40, 40, 40);
            MagicaVoxelData[] parsed = voxes.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 3;
                parsed[i].y += 3;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            int framelimit = 4;
            byte[][,,] facings = new byte[4][,,];
            for(int dir = 0; dir < 4; dir++)
            {
                facings[dir] = TransformLogic.SealGaps(TransformLogic.RotateYaw(TransformLogic.VoxListToArray(parsed, 22, 22, 20), dir * 90));
            }
            for(int palette = 0; palette < 208; palette++)
            {
                string folder = (altFolder + "ortho/palette" + palette + "/");//"color" + i;
                Directory.CreateDirectory(folder); //("color" + i);
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < framelimit; f++)
                    {
                        byte[][] b = processFrameTinyW(facings[dir], palette, f, framelimit, still, shadowless);
                        ImageInfo imi = new ImageInfo(widthTiny, heightTiny, 8, false, false, true);
                        PngWriter png = FileHelper.CreatePngWriter(folder + u + "_p" + palette + "_d" + dir + "_" + f + ".png", imi, true);
                        WritePNG(png, b, simplepalettes[palette]);
                        // b.Save(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png", ImageFormat.Png);
                    }
                }
            }
            /*
            List<string> imageNames = new List<string>(4 * 8 * framelimit);
            for(int dir = 0; dir < 4; dir++)
            {
                for(int f = 0; f < framelimit; f++)
                {
                    imageNames.Add(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png");
                }
            }
            
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running GIF conversion ...");
            WriteGIF(imageNames, 25, "gifs/" + altFolder + u + "_Large_animated");
            */
            //processExplosionLargeW(u, palette, shadowless);

        }
        public static void processUnitLargeW(string u, int palette, bool still, bool shadowless)
        {

            Console.WriteLine("Processing: " + u + ", palette " + palette);

            BinaryReader bin = new BinaryReader(File.Open(altFolder + u + "_Large_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxes = VoxelLogic.FromMagicaRaw(bin); //VoxelLogic.PlaceShadowsW(
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_" + palette + ".vox", voxes, "W", palette, 40, 40, 40);
            MagicaVoxelData[] parsed = voxes.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 10;
                parsed[i].y += 10;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            int framelimit = 4;


            string folder = (altFolder);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);
            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);
                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameLargeW(colors, palette, f, framelimit, still, shadowless);
                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                    // b.Save(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png", ImageFormat.Png);
                }
            }
            /*
            List<string> imageNames = new List<string>(4 * 8 * framelimit);
            for(int dir = 0; dir < 4; dir++)
            {
                for(int f = 0; f < framelimit; f++)
                {
                    imageNames.Add(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png");
                }
            }
            
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running GIF conversion ...");
            WriteGIF(imageNames, 25, "gifs/" + altFolder + u + "_Large_animated");
            */

            /*
            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = folder + "/palette" + palette + "_" + u + "_Large_face* ";
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Large_animated.gif";
            Process.Start(startInfo).WaitForExit();
            */
            //bin.Close();

            //            processFiringLarge(u);

            //processExplosionLargeW(u, palette, shadowless);

        }
        public static void processUnitLargeWalkW(string u, int palette)
        {

            Console.WriteLine("Processing: " + u);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Walk_0_Large_W.vox", FileMode.Open));
            MagicaVoxelData[][] parsed = new MagicaVoxelData[4][];
            parsed[0] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_1_Large_W.vox", FileMode.Open));
            parsed[1] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_2_Large_W.vox", FileMode.Open));
            parsed[2] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_3_Large_W.vox", FileMode.Open));
            parsed[3] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            for(int i = 0; i < parsed.Length; i++)
            {
                for(int j = 0; j < parsed[i].Length; j++)
                {
                    parsed[i][j].x += 10;
                    parsed[i][j].y += 10;

                }
            }
            byte[][,,] colors = new byte[4][,,];

            int framelimit = 8;


            string folder = (altFolder);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);
            for(int dir = 0; dir < 4; dir++)
            {
                for(int i = 0; i < 4; i++)
                {
                    colors[i] = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed[i], dir), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);
                }
                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameLargeW(colors[f % 4], palette, f, framelimit, true, false);

                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Walk_Large_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);

                    //                    b.Save(folder + "/palette" + palette + "_" + u + "_Walk_Large_face" + dir + "_" + f + ".png", ImageFormat.Png);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";
            for(int dir = 0; dir < 4; dir++)
            {
                for(int i = 0; i < framelimit; i++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Walk_Large_face" + dir + "_" + i + ".png ";
                }
            }
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Walk_Large_animated.gif";
            Process.Start(startInfo).WaitForExit();


            //            processFiringDouble(u);

            //            processExplosionDouble(u);

        }
        private static byte[][] processFrameTinyW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];
            return renderGenericW(parsed, palette, frame, maxFrames, still, shadowless, 22, 22, 20);
        }

        public static int ColorPriorityCU(int c)
        {
            switch((253 - c) >> 2)
            {
                case 6:
                case 7:
                    return 60;// + ((253 - fv) & 3) * 3;
                case 9:
                    return 50;// + ((253 - fv) & 3) * 3;
                case 8:
                    return 88;// + ((253 - fv) & 3) * 3;
                case 10:
                case 11:
                    return 100;// + ((253 - fv) & 3) * 3;
                //case 16: return 24;
                default: return 4;// + ((253 - c) & 3) * -4;
            }
        }

        private static byte mostCommon(byte[][] b, int x, int y)
        {
            byte best1 = b[y][x], best2 = 0, best3 = 0, best4 = 0, best5 = 0, current, weight = (byte)((best1 == 0) ? 3 : 1);
            //if(best1 == 0) return 0;
            int count1 = 1, count2 = 0, count3 = 0, count4 = 0, count5 = 0, t;//, shade = best1 == 0 ? 3 : ((253 - best1) & 3), total = 1;
            for(int j = -1; j <= 1; j++)
            {
                for(int i = -1; i <= 1; i++)
                {
                    if(b[y + j][x + i] == 0 && (i & j) != 0) weight = 3;
                }
            }
            for(int j = -1; j <= 1; j++)
            {
                for(int i = -1; i <= 1; i++)
                {
                    current = b[y + j][x + i];
                    if(current == 0 || (i & j) != 0) continue;
                    //shade += ((253 - current) & 3);
                    //total++;
                    //current = (byte)((253 - current) >> 2);
                    if(best1 == 0 || current == best1)
                    {
                        best1 = current;
                        count1 += (t = ColorPriorityCU(current) * (((253 - current) & 3) == 3 ? weight : 4 - weight));
                    }
                    else if(best2 == 0 || current == best2)
                    {
                        best2 = current;
                        count2 += (t = ColorPriorityCU(current) * (((253 - current) & 3) == 3 ? weight : 4 - weight));
                    }
                    else if(best3 == 0 || current == best3)
                    {
                        best3 = current;
                        count3 += (t = ColorPriorityCU(current) * (((253 - current) & 3) == 3 ? weight : 4 - weight));
                    }
                    else if(best4 == 0 || current == best4)
                    {
                        best4 = current;
                        count4 += (t = ColorPriorityCU(current) * (((253 - current) & 3) == 3 ? weight : 4 - weight));
                    }
                    else if(best5 == 0 || current == best5)
                    {
                        best5 = current;
                        count5 += (t = ColorPriorityCU(current) * (((253 - current) & 3) == 3 ? weight : 4 - weight));
                    }
                }
            }
            return ((count5 > count2 && count5 > count3 && count5 > count1 && count5 > count4) ? best5 :
                 (count4 > count2 && count4 > count3 && count4 > count1 && count4 > count5) ? best4 :
                (count3 > count1 && count3 > count2 && count3 > count4 && count3 > count5) ? best3 :
                (count2 > count1 && count2 > count3 && count2 > count4 && count2 > count5) ? best2 : best1);
            //return current == 0 ? current : (byte)(253 - (current << 2) - (shade / total));
        }


        private static byte[][] processFrameLargeW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            //return renderWSmart(parsed, palette, frame, maxFrames, still, shadowless);
            
            byte[][] b = renderWSmart(parsed, palette, frame, maxFrames, still, shadowless), b2;
            b2 = new byte[heightLarge][];
            for(int i = 0; i < heightLarge; i++)
            {
                b2[i] = new byte[widthLarge];
            }
            for(int y = 1, i = 0; y < b.Length - 1; y += 2, i++)
            {
                for(int x = 1, j = 0; x < b[y].Length - 1; x += 2, j++)
                {
                    b2[i][j] = mostCommon(b, x, y);
                }
            }
            return b2;

            // g2.DrawImage(b.Clone(new Rectangle(32, 46 + 32, 88 * 2, 108 * 2), b.PixelFormat), 0, 0, widthLarge, heightLarge);

            /*
            ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, true, true, true); // 8 bits per channel, no alpha 
            PngWriter png = FileHelper.CreatePngWriter("test.png", imi, true);

            */

            /*string folder = "palette" + palette + "_big";
            System.IO.Directory.CreateDirectory(folder);
            b.Save(folder + "/" + (System.IO.Directory.GetFiles(folder).Length) + "_Gigantic_face" + dir + "_" + frame + ".png", ImageFormat.Png); g = Graphics.FromImage(b);
            */
        }

        private static byte[][] processFrameSmallW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            return renderWSmartSmall(parsed, palette, frame, maxFrames, still, shadowless);
            /*
            b = renderWSmartHuge(parsed, dir, palette, frame, maxFrames, still, shadowless);
            b2 = new byte[heightHuge][];
            for(int i = 0; i < heightHuge; i++)
            {
                b2[i] = new byte[widthHuge];
            }

            for(int y = 0, i = 0; y < heightHuge * 2; y += 2, i++)
            {
                for(int x = 0, j = 0; x < widthHuge * 2; x += 2, j++)
                {
                    if(i < heightHuge && j < widthHuge)
                    {
                        b2[i][j] = b[y][x];
                    }
                }

            }
            return b2;
            */
            /*string folder = "palette" + palette + "_big";
            System.IO.Directory.CreateDirectory(folder);
            b.Save(folder + "/" + (System.IO.Directory.GetFiles(folder).Length) + "_Gigantic_face" + dir + "_" + frame + ".png", ImageFormat.Png); g = Graphics.FromImage(b);
            */
        }
        private static byte[][] processFrameHugeW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            //return renderWSmartHuge(parsed, palette, frame, maxFrames, still, shadowless);
            byte[][] b = renderWSmartHuge(parsed, palette, frame, maxFrames, still, shadowless), b2;
            b2 = new byte[heightHuge][];
            for(int i = 0; i < heightHuge; i++)
            {
                b2[i] = new byte[widthHuge];
            }

            for(int y = 1, i = 0; y < b.Length - 1; y += 2, i++)
            {
                for(int x = 1, j = 0; x < b[y].Length - 1; x += 2, j++)
                {
                    b2[i][j] = mostCommon(b, x, y);
                }
            }
            return b2;

            /*
            b = renderWSmartHuge(parsed, dir, palette, frame, maxFrames, still, shadowless);
            b2 = new byte[heightHuge][];
            for(int i = 0; i < heightHuge; i++)
            {
                b2[i] = new byte[widthHuge];
            }

            for(int y = 0, i = 0; y < heightHuge * 2; y += 2, i++)
            {
                for(int x = 0, j = 0; x < widthHuge * 2; x += 2, j++)
                {
                    if(i < heightHuge && j < widthHuge)
                    {
                        b2[i][j] = b[y][x];
                    }
                }

            }
            return b2;
            */
            /*string folder = "palette" + palette + "_big";
            System.IO.Directory.CreateDirectory(folder);
            b.Save(folder + "/" + (System.IO.Directory.GetFiles(folder).Length) + "_Gigantic_face" + dir + "_" + frame + ".png", ImageFormat.Png); g = Graphics.FromImage(b);
            */
        }
        private static byte[][] processFrameMassiveW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            //byte[][] b, b2;
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            //return renderWSmartMassive(parsed, palette, frame, maxFrames, still, shadowless);
            byte[][] b = renderWSmartMassive(parsed, palette, frame, maxFrames, still, shadowless), b2;
            b2 = new byte[heightMassive][];
            for(int i = 0; i < heightMassive; i++)
            {
                b2[i] = new byte[widthMassive];
            }

            for(int y = 1, i = 0; y < b.Length - 1; y += 2, i++)
            {
                for(int x = 1, j = 0; x < b[y].Length - 1; x += 2, j++)
                {
                    b2[i][j] = mostCommon(b, x, y);
                }
            }
            return b2;

            /*
            b = renderWSmartMassive(parsed, dir, palette, frame, maxFrames, still, shadowless);
            //            return b;
            b2 = new byte[408][];
            for(int y = 0, i = 0; y < 408 * 2; y += 2, i++)
            {
                b2[i] = new byte[328];
                for(int x = 0, j = 0; x < 328 * 2; x += 2, j++)
                {
                    if(i >= 0 && j >= 0)
                    {
                        b2[i][j] = b[y][x];
                    }
                }
            }
            return b2;
            */
        }

        private static byte[][] processFrameSpecialW(byte[,,] parsed, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            //return renderWSmart(parsed, palette, frame, maxFrames, still, shadowless);

            byte[][] b = renderWSmart(parsed, palette, frame, maxFrames, still, shadowless), b2;
            b2 = new byte[heightLarge][];
            for(int i = 0; i < heightLarge; i++)
            {
                b2[i] = new byte[widthLarge];
            }
            for(int y = 1, i = 0; y < b.Length - 1; y += 2, i++)
            {
                for(int x = 1, j = 0; x < b[y].Length - 1; x += 2, j++)
                {
                    if((253 - b[y][x]) % 4 == 2)
                        b2[i][j] = (byte)(b[y][x] + 2);
                }
            }
            return b2;

            // g2.DrawImage(b.Clone(new Rectangle(32, 46 + 32, 88 * 2, 108 * 2), b.PixelFormat), 0, 0, widthLarge, heightLarge);

            /*
            ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, true, true, true); // 8 bits per channel, no alpha 
            PngWriter png = FileHelper.CreatePngWriter("test.png", imi, true);

            */

            /*string folder = "palette" + palette + "_big";
            System.IO.Directory.CreateDirectory(folder);
            b.Save(folder + "/" + (System.IO.Directory.GetFiles(folder).Length) + "_Gigantic_face" + dir + "_" + frame + ".png", ImageFormat.Png); g = Graphics.FromImage(b);
            */
        }


        public static void processUnitHugeW(string u, int palette, bool still, bool shadowless)
        {

            Console.WriteLine("Processing: " + u + ", palette " + palette);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Huge_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxes = VoxelLogic.FromMagicaRaw(bin);
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_" + palette + ".vox", voxes, "W", palette, 80, 80, 80);
            MagicaVoxelData[] parsed = voxes.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 20;
                parsed[i].y += 20;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            int framelimit = 4;


            string folder = (altFolder);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateHuge(parsed, dir), multiplier, 120, 120, 80), 1 + bonus * multiplier / 2);
                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameHugeW(colors, palette, f, framelimit, still, shadowless);

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Huge_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = folder + "/palette" + palette + "_" + u + "_Huge_face* ";
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Huge_animated.gif";
            Process.Start(startInfo).WaitForExit();

            //bin.Close();

            //            processFiringHuge(u);

            processExplosionHugeW(u, palette);

        }

        public static void processUnitLargeWMilitary(string u)
        {
            u = addon + u;
            int framelimit = 4;

            string folder = altFolder;
            Directory.CreateDirectory(folder);
            Console.WriteLine("Processing: " + u + ", palette " + 99);

            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);
            int alt = VoxelLogic.AltVersions[VoxelLogic.UnitLookup[u]];
            alt = 0;
            for(int a = 0; a <= alt && a < 2; a++)
            {
                BinaryReader bin = new BinaryReader(File.Open("CU3/" + u + (alt > 1 && a > 0 ? "_Alt" : "") + "_Large_W.vox", FileMode.Open));
                List<MagicaVoxelData> voxes = zombie ? VoxelLogic.AssembleZombieHeadToModelW(bin, a > 0) : VoxelLogic.AssembleHeadToModelW(bin, a > 0, CURedux.REMOVE_TWINKLE);

                Directory.CreateDirectory("vox/" + altFolder);
                //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_0.vox", voxes, "W", 0, 40, 40, 40);
                MagicaVoxelData[] parsed = voxes.ToArray(), explode_parsed = voxes.ToArray();
                for(int i = 0; i < parsed.Length; i++)
                {
                    parsed[i].x += 10;
                    parsed[i].y += 10;
                    if((254 - parsed[i].color) % 4 == 0)
                        parsed[i].color--;
                }

                if(RENDER)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        byte[,,] colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60)), 1 + bonus * multiplier / 2);

                        for(int f = 0; f < framelimit; f++)
                        {
                            byte[][] b = processFrameLargeW(colors, 0, f, framelimit, (VoxelLogic.CurrentMobilities[VoxelLogic.UnitLookup[u]] != MovementType.Flight), false);

                            ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                            PngWriter png = FileHelper.CreatePngWriter(blankFolder + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_" + f + ".png", imi, true);
                            WritePNG(png, b, basepalette);
                        }
                    }
                }
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < framelimit; f++)
                    {
                        if(zombie)
                            AlterPNGPaletteZombie(blankFolder + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_" + f + ".png", simplepalettes);
                        else
                            AlterPNGPalette(blankFolder + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_" + f + ".png", simplepalettes);
                    }
                }
                processExplosionLargeW(addon + u, -1, explode_parsed, false, (a > 0) ? "_Alt" : "");
                processUnitLargeWFiring(addon + u, a);
            }
        }

        public static void processUnitLargeWFiring(string u, int a)
        {
            Console.WriteLine("Processing: " + u);
            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);
            int alt = VoxelLogic.AltVersions[VoxelLogic.UnitLookup[u]];
            alt = 0;
            string filename = "CU3/" + u + (alt > 1 && a > 0 ? "_Alt" : "") + "_Large_W.vox";
            BinaryReader bin;
            MagicaVoxelData[] parsed;
            string folder = ("frames/" + altFolder);

            for(int w = 0; w < 2; w++)
            {
                if((w == 0 && u == "Infantry" || u == "Tank_S") || (w == 1 && (u == "Infantry_P" || u == "Infantry_T" || u == "Infantry_PT")))
                {
                    filename = "CU3/" + u + "_Firing" + (alt > 1 && a > 0 && u != "Infantry" ? "_Alt" : "") + "_Large_W.vox";
                }
                if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] == 7)
                {

                    if(RENDER)
                    {
                        bin = new BinaryReader(File.Open(filename, FileMode.Open));
                        parsed = zombie ? VoxelLogic.AssembleZombieHeadToModelW(bin, a > 0).ToArray() : VoxelLogic.FromMagicaRaw(bin).ToArray();
                        MagicaVoxelData[][] flying = CURedux.Flyover(parsed);
                        MagicaVoxelData[][] voxelFrames = new MagicaVoxelData[16][];
                        //voxelFrames[0] = new MagicaVoxelData[parsedFrames[0].Length];
                        for(int i = 0; i < 16; i++)
                        {
                            voxelFrames[i] = new MagicaVoxelData[flying[i].Length];
                            flying[i].CopyTo(voxelFrames[i], 0);
                        }

                        voxelFrames = CURedux.weaponAnimationsLarge[VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w]](voxelFrames, VoxelLogic.UnitLookup[u], w);

                        for(int f = 0; f < 16; f++)
                        {
                            List<MagicaVoxelData> altered = new List<MagicaVoxelData>(voxelFrames[f].Length);
                            int[,] taken = new int[120, 120];
                            taken.Fill(-1);
                            for(int i = 0; i < voxelFrames[f].Length; i++)
                            {
                                // do not store this voxel if it lies out of range of the voxel chunk (30x30x30)
                                if(voxelFrames[f][i].x >= 120 || voxelFrames[f][i].y >= 120 || voxelFrames[f][i].z >= 120)
                                {
                                    //Console.Write("Voxel out of bounds: " + voxelFrames[f][i].x + ", " + voxelFrames[f][i].y + ", " + voxelFrames[f][i].z);
                                    continue;
                                }
                                altered.Add(voxelFrames[f][i]);
                            }
                            flying[f] = altered.ToArray();
                        }
                        Directory.CreateDirectory(folder); //("color" + i);
                        for(int d = 0; d < 4; d++)
                        {
                            for(int frame = 0; frame < 16; frame++)
                            {
                                byte[,,] colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateHuge(flying[frame], d), multiplier, 120, 120, 80)), 1 + bonus * multiplier / 2);


                                byte[][] b = processFrameHugeW(colors, 0, frame, 16, true, false);

                                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                                PngWriter png = FileHelper.CreatePngWriter(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + d + "_attack_" + w + "_" + frame + ".png", imi, true);
                                WritePNG(png, b, basepalette);

                            }
                        }
                    }
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 16; f++)
                        {
                            if(zombie)
                                AlterPNGPaletteZombie(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png",
                                    "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                            else
                                AlterPNGPalette(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png",
                                    "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                        }
                    }
                    //bin.Close();
                }
                else if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] != -1)
                {

                    if(RENDER)
                    {
                        Directory.CreateDirectory(folder);

                        bin = new BinaryReader(File.Open(filename, FileMode.Open));
                        parsed = zombie ? VoxelLogic.AssembleZombieHeadToModelW(bin, a > 0).ToArray() : VoxelLogic.AssembleHeadToModelW(bin, a > 0, CURedux.REMOVE_TWINKLE).ToArray();
                        
                        MagicaVoxelData[][] firing = CURedux.makeFiringAnimationLarge(parsed, VoxelLogic.UnitLookup[u], w);

                        for(int d = 0; d < 4; d++)
                        {

                            for(int frame = 0; frame < 16; frame++)
                            {
                                byte[,,] colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateHuge(firing[frame], d), multiplier, 120, 120, 80)), 1 + bonus * multiplier / 2);

                                byte[][] b = processFrameHugeW(colors, 0, frame, 16, true, false);

                                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                                PngWriter png = FileHelper.CreatePngWriter(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + d + "_attack_" + w + "_" + frame + ".png", imi, true);
                                WritePNG(png, b, basepalette);
                            }
                        }
                    }
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 16; f++)
                        {
                            if(zombie)
                                AlterPNGPaletteZombie(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png",
                                    "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                            else
                                AlterPNGPalette(blankFolder  + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png",
                                    "color{0}/" + u + "/" + addon + u + (a > 0 ? "_Alt" : "") + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                        }
                    }
                    //bin.Close();

                }
                else continue;
                /*
                List<string> imageNames = new List<string>(4 * 8 * 16);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 16; f++)
                        {
                            imageNames.Add(folder + "/color" + p + "/" + u + "_Large_face" + dir + "_attack_" + w + "_" + f + ".png");
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_attack_"+w+"_animated");
                */
            }

        }

        public static void processReceivingMilitaryW()
        {
            string folder = ("frames/" + altFolder);
            //START AT 0 WHEN PROCESSING ALL OF THE ANIMATIONS.
            //new int[] { 0, 1, 2, 3, 4, 5, 6, 7}.AsParallel().ForAll(i => {
            for(int i = 8; i < 8; i++)
            {
                Console.WriteLine("Processing receive animation: " + CURedux.WeaponTypes[i]);
                if(RENDER)
                {
                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);

                    for(int s = 0; s < 4; s++)
                    {
                        MagicaVoxelData[][] receive = CURedux.makeReceiveAnimationLarge(i, s + 1);
                        for(int d = 0; d < 4; d++)
                        {
                            for(int frame = 0; frame < 16; frame++)
                            {
                                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToArray(VoxelLogic.BasicRotateHuge(receive[frame], d), 120, 120, 80), 1 + bonus * multiplier / 2);

                                byte[][] b = processFrameHugeW(colors, 0, frame, 16, true, false);

                                PngWriter png = FileHelper.CreatePngWriter(blankFolder + "receiving/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + s + "_" + frame + ".png", imi, true);
                                WritePNG(png, b, basepalette);
                            }
                        }
                    }
                }
                /*
                System.IO.Directory.CreateDirectory("strips_iso");
//                System.IO.Directory.CreateDirectory("strips_ortho");
                ProcessStartInfo startInfo = new ProcessStartInfo(@"montage.exe");
                startInfo.UseShellExecute = false;
                string st = "";
                for (int color = 0; color < ((i > 4) ? 8 : 2); color++)
                {
                    for (int dir = 0; dir < 4; dir++)
                    {
                        for (int strength = 0; strength < 4; strength++)
                        {
                            st = folder + "/color" + color + "_" + WeaponTypes[i] + "_face" + dir + "_strength_" + strength + "_%d.png[0-15] ";
                            startInfo.Arguments = st + " -background none -mode Concatenate -tile x1 strips_iso/color" + color + "_" + WeaponTypes[i] + "_strength" + strength + "_receive_face" + dir + ".png";
                            Process.Start(startInfo).WaitForExit();
                            //st = folder + "/color" + color + "_" + WeaponTypes[i] + "_face" + dir + "_strength_" + strength + "_%d.png[0-15] ";
                            //startInfo.Arguments = st + " -mode Concatenate -tile x1 strips_ortho/color" + color + "_" + WeaponTypes[i] + "_strength" + strength + "_receive_face" + dir + ".png";
                            //Process.Start(startInfo).WaitForExit();
                        }
                    }
                }
                */
                for(int strength = 0; strength < 4; strength++)
                {
                    for(int d = 0; d < 4; d++)
                    {
                        for(int frame = 0; frame < 16; frame++)
                        {
                            AlterPNGPaletteLimited(blankFolder + "receiving/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_" + frame + ".png",
                                "receiving/color{0}" + "/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_" + frame + ".png", simplepalettes);
                        }
                    }
                }

                /*
                List<string> imageNames = new List<string>(4 * 4 * 8 * 16);

                for(int strength = 0; strength < 4; strength++)
                {
                    for(int p = 0; p < 8; p++)
                    {
                        for(int dir = 0; dir < 4; dir++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add(folder + "/color" + p + "/" + CURedux.WeaponTypes[i] + "_face" + dir + "_strength_" + strength + "_" + f + ".png");
                            }
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running receiving GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + CURedux.WeaponTypes[i] + "_animated");
                */
                /*
                System.IO.Directory.CreateDirectory("gifs/" + altFolder);
                ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
                startInfo.UseShellExecute = false;
                
                List<string> imageNames = new List<string>(4 * 4 * 8 * 16);
                for(int strength = 0; strength < 4; strength++)
                {
                    for(int p = 0; p < 8; p++)
                    {
                        for(int d = 0; d < 4; d++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add(folder + "/color" + p + "_" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_" + f + ".png");
                            }
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + CURedux.WeaponTypes[i] + "_animated.gif");
                */
            }//);
            
            for(int s = 0; s < 10; s++)
            {
                byte[][,,] explode = CURedux.Explosions[s];
                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 16; frame++)
                    {
                        byte[,,] colors = new byte[120, 120, 80];
                        if(frame >= 1 && frame <= CURedux.maxExplosionFrames)
                            colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(explode[frame - 1], d * 90)), 1 + bonus * multiplier / 2);

                        byte[][] b = processFrameHugeW(colors, 0, frame, 16, true, false);

                        ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                        PngWriter png = FileHelper.CreatePngWriter(blankFolder + "receiving/Explosion/Explosion_face" + d + "_strength_" + s + "_" + frame + ".png", imi, true);
                        WritePNG(png, b, basepalette);
                    }
                }
            }

            for(int strength = 0; strength < 10; strength++)
            {
                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 16; frame++)
                    {
                        AlterPNGPaletteLimited(blankFolder + "receiving/Explosion/Explosion_face" + d + "_strength_" + strength + "_" + frame + ".png",
                            "receiving/color{0}/Explosion/Explosion_face" + d + "_strength_" + strength + "_" + frame + ".png", simplepalettes);
                    }
                }
            }
            
        }

        public static void processUnitHugeWMilitarySuper(string u)
        {
            u = addon + u;
            int framelimit = 4;

            Console.WriteLine("Processing: " + u + ", palette " + 99 + " Super");
            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);

            BinaryReader bin = new BinaryReader(File.Open("CU3/" + u + "_Large_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxes = zombie ? VoxelLogic.Zombify(bin) : VoxelLogic.FromMagicaRaw(bin);
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_0.vox", voxes, "W", 0, 40, 40, 40);
            MagicaVoxelData[] parsed = voxes.ToArray(), explode_parsed = voxes.ToArray();
            
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 10;
                parsed[i].y += 10;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            if(RENDER)
            {
                for(int dir = 0; dir < 4; dir++)
                {
                    byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier * 2, 120, 120, 80), 2 + bonus * multiplier / 2);
                    //byte[,,] colors = TransformLogic.RunCA(
                    //    TransformLogic.ScalePartial(TransformLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 40), 2),
                    //        2 + bonus * multiplier * 2);
                    
                    //                        1 + bonus * multiplier * 2);

                    //     byte[,,] colors = TransformLogic.RunCA(
                    //         TransformLogic.ScalePartial(TransformLogic.SealGaps(TransformLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 40)), 2),
                    //         1 + bonus * multiplier * 2);

                    //                byte[,,] colors = TransformLogic.RunCA(
                    //                    TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier * 2, 60, 60, 60)),
                    //                    1 + bonus * multiplier * 2);

                    for(int f = 0; f < framelimit; f++)
                    {

                        byte[][] b = processFrameHugeW(colors, 0, f, framelimit, (VoxelLogic.CurrentMobilities[VoxelLogic.UnitLookup[u]] != MovementType.Flight), false);

                        ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                        PngWriter png = FileHelper.CreatePngWriter(blankFolder + "standing_frames/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png", imi, true);
                        WritePNG(png, b, basepalette);
                    }
                }
            }
            for(int dir = 0; dir < 4; dir++)
            {
                for(int f = 0; f < framelimit; f++)
                {
                    if(zombie)
                        AlterPNGPaletteZombie(blankFolder + "standing_frames/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png",
                            "standing_frames/color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png", simplepalettes);
                    else
                        AlterPNGPalette(blankFolder + "standing_frames/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png",
                            "standing_frames/color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_" + f + ".png", simplepalettes);
                }
            }
            /*
            List<string> imageNames = new List<string>(4 * 8 * 2 * framelimit);
            for(int p = 0; p < 8; p++)
            {
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < framelimit; f++)
                    {
                        imageNames.Add(folder + "/color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                    }
                    for(int f = 0; f < framelimit; f++)
                    {
                        imageNames.Add(folder + "/color" + p + "/" + u + "_Huge_face" + dir + "_" + f + ".png");
                    }
                }
            }
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running standing GIF conversion ...");
            WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_Huge_animated");
            */


            /*
            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";
            for(int i = 0; i < 8; i++)
            {
                s += folder + "/color" + i + "_" + u + "_Huge_face* ";
            }
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + u + "_Huge_animated.gif";
            Process.Start(startInfo).WaitForExit();
            */

            processExplosionHugeWSuper(addon + u, -1, explode_parsed, false);

            processUnitHugeWFiringSuper(addon + u);
        }
        public static void processUnitHugeWFiringSuper(string u)
        {
            Console.WriteLine("Processing: " + u + " Super");

            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);

            string filename = "CU3/" + u + "_Large_W.vox";
            BinaryReader bin;
            MagicaVoxelData[] parsed;
            string folder = ("frames/" + altFolder);
            for(int w = 0; w < 2; w++)
            {
                if((w == 0 && u == "Infantry" || u == "Tank_S") || (w == 1 && (u == "Infantry_P" || u == "Infantry_T" || u == "Infantry_PT")))
                {
                    filename = "CU3/" + u + "_Firing_Large_W.vox";
                }
                if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] != -1)
                {
                    if(RENDER)
                    {
                        Directory.CreateDirectory(folder);

                        bin = new BinaryReader(File.Open(filename, FileMode.Open));
                        parsed = zombie ? VoxelLogic.Zombify(bin).ToArray() : VoxelLogic.FromMagicaRaw(bin).ToArray();
                        List<MagicaVoxelData>[] firing;
                        if(VoxelLogic.CurrentWeapons[VoxelLogic.UnitLookup[u]][w] == 7)
                        {
                            firing = CURedux.makeFiringAnimationSuper(CURedux.FlyoverSuper(parsed), VoxelLogic.UnitLookup[u], w);
                        }
                        else
                        {
                            firing = CURedux.makeFiringAnimationSuper(parsed, VoxelLogic.UnitLookup[u], w);
                        }

                        for(int d = 0; d < 4; d++)
                        {

                            for(int frame = 0; frame < 16; frame++)
                            {
                                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(firing[frame], d, 80, 80), multiplier * 2, 80, 80, 60), 2 + bonus * multiplier / 2);

                                //byte[,,] colors = TransformLogic.RunCA(
                                //    TransformLogic.ScalePartial(TransformLogic.VoxListToArray(VoxelLogic.RotateYaw(firing[frame], d, 80, 80), 80, 80, 60), 2),
                                //    2 + bonus * multiplier * 2);
                                //                                    1 + bonus * multiplier * 2);
                                byte[][] b = processFrameMassiveW(colors, 0, frame, 16, true, false);

                                ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);
                                PngWriter png = FileHelper.CreatePngWriter(blankFolder  + u + "/" + addon + u + "_Huge_face" + d + "_attack_" + w + "_" + frame + ".png", imi, true);
                                WritePNG(png, b, basepalette);
                            }
                        }
                    }
                    //FaceVoxel[,,] faces = FaceLogic.GetFaces(TransformLogic.VoxLargerArrayToList(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(firing[frame], d, 80, 80), 2, 80, 80, 60), 1), 160, 160, 120);
                    //byte[,,] colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(firing[frame], d, 80, 80), multiplier * 2, 80, 80, 60)), 1 + bonus * multiplier);
                    /*
                    byte[,,] colors = TransformLogic.RunCA(
                        TransformLogic.ScalePartial(TransformLogic.SealGaps(TransformLogic.VoxListToArray(VoxelLogic.RotateYaw(firing[frame], d, 80, 80), 80, 80, 60)), 2),
                        1 + bonus * multiplier * 2);
                        */


                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 16; f++)
                        {
                            if(zombie)
                            {
                                AlterPNGPaletteZombie(blankFolder  + u + "/" + addon + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png",
                                   "color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                            }
                            else
                            {
                                AlterPNGPalette(blankFolder  + u + "/" + addon + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png",
                                   "color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png", simplepalettes);
                            }
                        }
                    }
                    //bin.Close();

                }
                else continue;
                /*
                Directory.CreateDirectory("gifs/" + altFolder);

                List<string> imageNames = new List<string>(4 * 8 * 16);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 16; f++)
                        {
                            imageNames.Add(folder + "/color" + p + "/" + u + "_Huge_face" + dir + "_attack_" + w + "_" + f + ".png");
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_attack_" + w + "_Huge_animated");
                */
            }

        }

        public static void processReceivingMilitaryWSuper()
        {
            string folder = ("frames/" + altFolder);
            //START AT 0 WHEN PROCESSING ALL OF THE ANIMATIONS.
            //new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }.AsParallel().ForAll(i => {
            for(int i = 0; i < 8; i++)
            {
                Console.WriteLine("Processing receive animation: " + CURedux.WeaponTypes[i]);
                if(RENDER)
                {
                    for(int s = 0; s < 4; s++)
                    {
                        List<MagicaVoxelData>[] receive = CURedux.makeReceiveAnimationSuper(i, s + 1);
                        for(int d = 0; d < 4; d++)
                        {
                            for(int frame = 0; frame < 16; frame++)
                            {
                                //FaceVoxel[,,] faces = FaceLogic.GetFaces(TransformLogic.VoxLargerArrayToList(TransformLogic.TransformStartHuge(VoxelLogic.BasicRotateHuge(receive[frame], d)), 2), 160, 160, 120);
                                //         FaceVoxel[,,] faces = FaceLogic.GetFaces(TransformLogic.VoxLargerArrayToList(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(receive[frame], d, 80, 80), 2, 80, 80, 60), 1), 160, 160, 120);
                                //byte[,,] colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(receive[frame], d, 80, 80), multiplier * 2, 80, 80, 60)), 1 + bonus * multiplier);

                                //                                1 + bonus * multiplier * 2);
                                //                            byte[,,] colors = TransformLogic.RunCA(
                                //                                TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.RotateYaw(receive[frame], d, 80, 80), multiplier * 2, 80, 80, 60)),
                                //                                1 + bonus * multiplier * 2);
                                byte[,,] colors = TransformLogic.RunCA(
                                    TransformLogic.ScalePartial(TransformLogic.VoxListToArray(VoxelLogic.RotateYaw(receive[frame], d, 80, 80), 80, 80, 60), 2),
                                    1 + bonus * multiplier);

                                byte[][] b = processFrameMassiveW(colors, 0, frame, 16, true, false);

                                ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);
                                PngWriter png = FileHelper.CreatePngWriter(blankFolder + "receiving/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + s + "_super_" + frame + ".png", imi, true);
                                WritePNG(png, b, basepalette);
                            }
                        }
                    }
                }
                /*
                System.IO.Directory.CreateDirectory("strips_iso");
//                System.IO.Directory.CreateDirectory("strips_ortho");
                ProcessStartInfo startInfo = new ProcessStartInfo(@"montage.exe");
                startInfo.UseShellExecute = false;
                string st = "";
                for (int color = 0; color < ((i > 4) ? 8 : 2); color++)
                {
                    for (int dir = 0; dir < 4; dir++)
                    {
                        for (int strength = 0; strength < 4; strength++)
                        {
                            st = folder + "/color" + color + "_" + WeaponTypes[i] + "_face" + dir + "_strength_" + strength + "_%d.png[0-15] ";
                            startInfo.Arguments = st + " -background none -mode Concatenate -tile x1 strips_iso/color" + color + "_" + WeaponTypes[i] + "_strength" + strength + "_receive_face" + dir + ".png";
                            Process.Start(startInfo).WaitForExit();
                            //st = folder + "/color" + color + "_" + WeaponTypes[i] + "_face" + dir + "_strength_" + strength + "_%d.png[0-15] ";
                            //startInfo.Arguments = st + " -mode Concatenate -tile x1 strips_ortho/color" + color + "_" + WeaponTypes[i] + "_strength" + strength + "_receive_face" + dir + ".png";
                            //Process.Start(startInfo).WaitForExit();
                        }
                    }
                }
                */
                for(int strength = 0; strength < 4; strength++)
                {
                    for(int d = 0; d < 4; d++)
                    {
                        for(int frame = 0; frame < 16; frame++)
                        {
                            AlterPNGPaletteLimited(blankFolder + "receiving/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_super_" + frame + ".png",
                                "receiving/color{0}/" + "/" + CURedux.WeaponTypes[i] + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_super_" + frame + ".png", simplepalettes);
                        }
                    }
                }
                /*
                Directory.CreateDirectory("gifs/" + altFolder);

                List<string> imageNames = new List<string>(4 * 4 * 8 * 16);
                for(int strength = 0; strength < 4; strength++)
                {
                    for(int p = 0; p < 8; p++)
                    {
                        for(int d = 0; d < 4; d++)
                        {
                            for(int f = 0; f < 16; f++)
                            {
                                imageNames.Add(folder + "/color" + p + "/" + CURedux.WeaponTypes[i] + "_face" + d + "_strength_" + strength + "_super_" + f + ".png");
                            }
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + CURedux.WeaponTypes[i] + "_Huge_animated.gif");
                */
            }//);
            /*
            for(int s = 0; s < 10; s++)
            {
                byte[][,,] explode = CURedux.SuperExplosions[s];

                for(int d = 0; d < 4; d++)
                {
                    Directory.CreateDirectory(folder); //("color" + i);

                    for(int frame = 0; frame < 16; frame++)
                    {
                        byte[,,] colors = new byte[160, 160, 120];
                        
                        if(frame >= 1 && frame <= CURedux.maxExplosionFrames)
                            colors = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(explode[frame - 1], d * 90)), 1 + bonus * multiplier);
                        byte[][] b = processFrameMassiveW(colors, 0, frame, 16, true, false);

                        ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);
                        PngWriter png = FileHelper.CreatePngWriter(blankFolder + "receiving/Explosion/Explosion_face" + d + "_strength_" + s + "_super_" + frame + ".png", imi, true);
                        WritePNG(png, b, basepalette);
                    }
                }
            }

            for(int strength = 0; strength < 10; strength++)
            {
                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 16; frame++)
                    {
                        AlterPNGPaletteLimited(blankFolder + "receiving/Explosion/Explosion_face" + d + "_strength_" + strength + "_super_" + frame + ".png",
                            "receiving/color{0}/Explosion/Explosion_face" + d + "_strength_" + strength + "_super_" + frame + ".png", simplepalettes);
                    }
                }
            }
            */
        }

        public static void processHatLargeW(string u, int palette, string hat)
        {

            Console.WriteLine("Processing: " + hat);
            BinaryReader bin;
            int framelimit = 4;

            string folder = (altFolder);
            Directory.CreateDirectory(folder);

            bin = new BinaryReader(File.Open(hat + "_Hat_W.vox", FileMode.Open));
            List<MagicaVoxelData> vlist = VoxelLogic.FromMagicaRaw(bin);
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + hat + "_" + palette + ".vox", vlist, "W", palette, 40, 40, 40);
            MagicaVoxelData[] parsed = vlist.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 10;
                parsed[i].y += 10;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color = VoxelLogic.clear;
            }

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);
                for(int f = 0; f < framelimit; f++)
                {

                    byte[][] b = processFrameLargeW(colors, palette, f, framelimit, true, true);

                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/" + "palette" + palette + "_" + hat + "_Hat_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }

        }

        public static void processUnitHugeWalkW(string u, int palette)
        {

            Console.WriteLine("Processing: " + u);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Walk_0_Huge_W.vox", FileMode.Open));
            MagicaVoxelData[][] parsed = new MagicaVoxelData[4][];
            parsed[0] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_1_Huge_W.vox", FileMode.Open));
            parsed[1] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_2_Huge_W.vox", FileMode.Open));
            parsed[2] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            bin = new BinaryReader(File.Open(u + "_Walk_3_Huge_W.vox", FileMode.Open));
            parsed[3] = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //bin.Close();

            for(int i = 0; i < parsed.Length; i++)
            {
                for(int j = 0; j < parsed[i].Length; j++)
                {
                    parsed[i][j].x += 20;
                    parsed[i][j].y += 20;

                }
            }
            int framelimit = 8;

            byte[][,,] faces = new byte[4][,,];


            string folder = (altFolder);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);
            for(int dir = 0; dir < 4; dir++)
            {
                for(int i = 0; i < 4; i++)
                {
//                    faces[i] = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateHuge(parsed[i], dir), 120, 120, 80, 153));
                    faces[i] = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateHuge(parsed[i], dir), multiplier, 120, 120, 80), 1 + bonus * multiplier / 2);

                }
                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameHugeW(faces[f % 4], palette, f, framelimit, true, false);

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Walk_Huge_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";
            for(int dir = 0; dir < 4; dir++)
            {
                for(int i = 0; i < framelimit; i++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Walk_Huge_face" + dir + "_" + i + ".png ";
                }
            }
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Walk_Huge_animated.gif";
            Process.Start(startInfo).WaitForExit();


            //            processFiringDouble(u);

            //            processExplosionDouble(u);

        }

        /*
        public static void processUnitLargeWHat(string u, int palette, bool still, string hat)
        {
                      //if (render_hat_gifs)
                      //  {
                      //      processUnitOutlinedWDoubleHatModel(u, palette, still, hat);
                      //      return;
                      //  }
            Console.WriteLine("Processing: " + u + " " + hat);

            BinaryReader bin = new BinaryReader(File.Open(u + "_Large_W.vox", FileMode.Open));
            MagicaVoxelData[] headpoints = VoxelLogic.GetHeadVoxels(bin, hat).ToArray();

            int framelimit = 4;

            string folder = (altFolder);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);

            Bitmap bmp = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);

            PixelFormat pxf = PixelFormat.Format32bppArgb;

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            int stride = bmpData.Stride;
            bmp.UnlockBits(bmpData);
            bmp.Dispose();

            for(int dir = 0; dir < 4; dir++)
            {
                for(int f = 0; f < framelimit; f++)
                {

                    int jitter = (((f % 4) % 3) + ((f % 4) / 3)) * 2;
                    if(framelimit >= 8) jitter = ((f % 8 > 4) ? 4 - ((f % 8) ^ 4) : f % 8);
                    int body_coord = TallFaces.voxelToPixelLargeW(0, 0, headpoints[0 + dir * 2].x, headpoints[0 + dir * 2].y, headpoints[0 + dir * 2].z, (byte)(253 - headpoints[0 + dir * 2].color) / 4, stride, jitter, still) / 4;
                    //model_headpoints.AppendLine("BODY: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //    ((body_coord % (stride / 4) - 32) / 2) + ", y " + (108 - ((body_coord / (stride / 4) - 78) / 2)));
                    int hat_coord = TallFaces.voxelToPixelLargeW(0, 0, headpoints[1 + dir * 2].x, headpoints[1 + dir * 2].y, headpoints[1 + dir * 2].z, (byte)(253 - headpoints[1 + dir * 2].color) / 4, stride, 0, true) / 4;
                    //hat_headpoints.AppendLine("HAT: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //    ((hat_coord % (stride / 4) - 32) / 2) + ", y " + (108 - ((hat_coord / (stride / 4) - 78) / 2)));

                    Graphics hat_graphics;
                    Bitmap hat_image = new Bitmap(Image.FromFile(altFolder + "palette" + ((hat == "Woodsman") ? 44 : (hat == "Farmer") ? 49 : (palette == 7 || palette == 8 || palette == 42) ? 7 : 0) + "_"
                        // + ((palette == 7 || palette == 8 || palette == 42) ? "Spirit_" : "Generic_Male_")
                        + hat + "_Hat_face" + dir + "_" + ((hat == "Farmer") ? 0 : f) + ".png"));
                    Bitmap body_image = new Bitmap(Image.FromFile(altFolder + "palette" + palette + "_" + u + "_Large_face" + dir + "_" + f + ".png"));

                    VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];

                    hat_graphics = Graphics.FromImage(hat_image);
                    Graphics body_graphics = Graphics.FromImage(body_image);
                    body_graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    body_graphics.DrawImage(hat_image, (((body_coord % (stride / 4) - 32) / 2) - ((hat_coord % (stride / 4) - 32) / 2)),
                         (((body_coord / (stride / 4) - 78) / 2) - ((hat_coord / (stride / 4) - 78) / 2)), widthLarge, heightLarge);

                    offsets.Write(((body_coord % (stride / 4) - 32) / 2) - ((hat_coord % (stride / 4) - 32) / 2));
                    offsets.Write(((body_coord / (stride / 4) - 78) / 2) - ((hat_coord / (stride / 4) - 78) / 2));
                    //                    model_headpoints.AppendLine("BODY: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //                        (((body_coord % (stride / 4) - 32) / 2) - ((hat_coord % (stride / 4) - 32) / 2))
                    //                        + ", y " + (((body_coord / (stride / 4) - 78) / 2) - ((hat_coord / (stride / 4) - 78) / 2)));

                    body_graphics.Dispose();
                    body_image.Save(altFolder + "/palette" + palette + "_" + u + "_" + hat + "_Large_face" + dir + "_" + f + ".png");
                    hat_graphics.Dispose();
                    hat_image.Dispose();
                    body_image.Dispose();
                }
            }

            System.IO.Directory.CreateDirectory("gifs");
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = altFolder + "/palette" + palette + "_" + u + "_" + hat + "_Large_face* ";
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_" + hat + "_Large_animated.gif";
            Process.Start(startInfo).WaitForExit();

            //bin.Close();

            //            processFiringDouble(u);

            processExplosionLargeWHat(u, palette, hat, headpoints);

            
            //bin = new BinaryReader(File.Open(hat + "_Hat_W.vox", FileMode.Open));
            //MagicaVoxelData[] parsed = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //for (int i = 0; i < parsed.Length; i++)
            //{
            //    parsed[i].x += 10;
            //    parsed[i].y += 10;
            //    if ((254 - parsed[i].color) % 4 == 0)
            //        parsed[i].color = VoxelLogic.clear;
            //}

            //for (int f = 0; f < framelimit; f++)
            //{ //
            //    for (int dir = 0; dir < 4; dir++)
            //    {
            //        Bitmap b = processSingleOutlinedWDouble(parsed, palette, f, framelimit, true);
            //        b.Save(folder + "/" + u + "_" + hat + "_Hat_face" + dir + "_" + f + ".png", ImageFormat.Png);
            //        b.Dispose();
            //    }
            //}
            
        }
        */
        public static void processUnitLargeDeadW(string u, int palette, bool still)
        {

            Console.WriteLine("Processing: " + u + " Dead");
            BinaryReader bin = new BinaryReader(File.Open(u + "_Dead_Large_W.vox", FileMode.Open));
            List<MagicaVoxelData> vlist = VoxelLogic.PlaceBloodPoolW(VoxelLogic.FromMagicaRaw(bin));
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + u + "_Dead_" + palette + ".vox", vlist, "W", palette, 40, 40, 40);
            MagicaVoxelData[] parsed = vlist.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 10;
                parsed[i].y += 10;
                if((254 - parsed[i].color) % 4 == 0)
                    parsed[i].color--;
            }
            int framelimit = 4;


            string folder = (altFolder + "palette" + palette);//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);

            for(int dir = 0; dir < 4; dir++)
            {
//                FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);

                for(int f = 0; f < framelimit; f++)
                {
                    byte[][] b = processFrameLargeW(colors, palette, f, framelimit, still, false);

                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Dead_Large_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = folder + "/palette" + palette + "_" + u + "_Dead_Large_face* ";
            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_Dead_Large_animated.gif";
            Process.Start(startInfo).WaitForExit();

            //bin.Close();

            //            processFiringDouble(u);

            //processFieryExplosionDoubleW(u, palette);

        }

        private static StringBuilder model_headpoints = new StringBuilder(), hat_headpoints = new StringBuilder();

        /*
        private static void processHats(string u, int palette, bool hover, string[] classes)
        {

            processUnitLargeW(u, palette, hover, false);
            //processUnitOutlinedWDoubleDead(u, palette, hover);
            foreach(string s in classes)
            {
                processUnitLargeWHat(u, palette, hover, s);
            }

            // // REMOVE COMMENT
            //string doc = File.ReadAllText("TemplateStill.html");
            //string html = String.Format(doc, palette, u);


            //string doc2 = File.ReadAllText("TemplateGif.html");
            //string html2 = String.Format(doc2, palette, u);
            //System.IO.Directory.CreateDirectory(altFolder + "html");
            //File.WriteAllText(altFolder + "html/" + u + "_still.html", html);
            //File.WriteAllText(altFolder + "html/" + u + ".html", html2);
            // //

        }
        
        private static void processHats(string u, int palette, bool hover, string[] classes, string alternateName)
        {

            processUnitLargeW(u, palette, hover, false);
            processUnitLargeDeadW(u, palette, hover);
            foreach(string s in classes)
            {
                processUnitLargeWHat(u, palette, hover, s);
            }
            // // REMOVE COMMENT
            //string doc = File.ReadAllText("LivingTemplateStill.html");
            //string html = String.Format(doc, palette, u);

            //string doc2 = File.ReadAllText("LivingTemplateGif.html");
            //string html2 = String.Format(doc2, palette, u);
            //System.IO.Directory.CreateDirectory(altFolder + "html");
            //File.WriteAllText(altFolder + "html/" + alternateName + "_still.html", html);
            //File.WriteAllText(altFolder + "html/" + alternateName + ".html", html2);
            // //
        }
        */
        
        public static void processExplosionLargeW(string u, int palette, bool shadowless)
        {
            Console.WriteLine("Processing: " + u);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Large_W.vox", FileMode.Open));
            MagicaVoxelData[] parsed = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //renderLarge(parsed, 0, 0, 0)[0].Save("junk_" + u + ".png");
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];
            // MagicaVoxelData[][] explode = VoxelLogic.FieryExplosionDoubleW(parsed, false, true); //((CurrentMobilities[UnitLookup[u]] == MovementType.Immobile) ? false : true)
            string folder = ("frames/" + altFolder);
            Directory.CreateDirectory(folder); //("color" + i);

            for(int d = 0; d < 4; d++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostLarge(parsed, d), multiplier, 40, 40, 120, 120, 80), 1 + bonus * multiplier / 2);
                byte[][,,] explode = TransformLogic.FieryExplosionW(colors, false, false);

                for(int frame = 0; frame < 12; frame++)
                {

                    byte[][] b = processFrameHugeW(explode[frame], palette, frame, 16, true, false);

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Large_face" + d + "_fiery_explode_" + frame + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Large_face" + d + "_fiery_explode_" + frame + ".png ";
                }
            }

            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_explosion_animated.gif";
            Console.WriteLine("Running convert.exe ...");
            Process.Start(startInfo).WaitForExit();

            //bin.Close();
        }

        public static void processExplosionLargeW(string u, int palette, MagicaVoxelData[] voxels, bool shadowless, string extra_name = "")
        {
            Console.WriteLine("Processing: " + u);
            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);
            if(palette >= 0)
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
                wditheredcurrent = wdithered[palette];
            }
            else
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[0];
                wditheredcurrent = wdithered[0];
            }

            string folder = ("frames/" + altFolder);
            Directory.CreateDirectory(folder);

            if(RENDER)
            {
                if(palette >= 0)
                {
                    VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
                    wditheredcurrent = wdithered[palette];
                }
                else
                {
                    VoxelLogic.wcolors = VoxelLogic.wpalettes[0];
                    wditheredcurrent = wdithered[0];
                }
                //MagicaVoxelData[][] explode = VoxelLogic.FieryExplosionDoubleW(parsed, false, true); //((CurrentMobilities[UnitLookup[u]] == MovementType.Immobile) ? false : true)
                byte[,,] colors = TransformLogic.VoxListToLargerArray(voxels, multiplier, 40, 40, 120, 120, 80);

                Model m = Model.FromModelCU(colors);

                byte[][,,] explode = CURedux.FieryDeathW(u, m);

                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 12; frame++)
                    {
                        byte[][] b = processFrameHugeW(TransformLogic.SealGaps(TransformLogic.RotateYaw(explode[frame], d * 90)), (palette >= 0) ? palette : 0, frame, 8, true, shadowless);

                        ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                        PngWriter png = (palette >= 0)
                            ? (FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + addon + u + extra_name + "_Large_face" + d + "_death_" + frame + ".png", imi, true))
                            : (FileHelper.CreatePngWriter(blankFolder  + u + "/" + addon + u + extra_name + "_Large_face" + d + "_death_" + frame + ".png", imi, true));
                        WritePNG(png, b, (palette >= 0) ? simplepalettes[palette] : basepalette);
                    }
                }
            }
            if(palette >= 0)
            {
                //Directory.CreateDirectory("gifs/" + altFolder);
                //ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
                //startInfo.UseShellExecute = false;
                //string s = "";

                //for(int d = 0; d < 4; d++)
                //{
                //    for(int frame = 0; frame < 12; frame++)
                //    {
                //        s += folder + "/palette" + palette + "_" + u + "_Large_face" + d + "_death_" + frame + ".png ";
                //    }
                //}

                //startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_death_animated.gif";
                //Console.WriteLine("Running convert.exe ...");
                //Process.Start(startInfo).WaitForExit();

                //for(int dir = 0; dir < 4; dir++)
                //{
                //    for(int f = 0; f < 12; f++)
                //    {
                //        AlterPNGPalette(folder + "/palette" + 99 + "_" + u + "_Large_face" + dir + "_death_" + f + ".png",
                //            folder + "/color{0}_" + u + "_Large_face" + dir + "_death_" + f + ".png", simplepalettes);
                //    }
                //}

                //Directory.CreateDirectory("gifs/" + altFolder);
                /*
                List<string> imageNames = new List<string>(4 * 8 * 12);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_death_" + f + ".png ");
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running explosion GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + "palette" + palette + "_" + u + "_death_animated");
                */
            }
            else
            {

                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < 12; f++)
                    {
                        if(zombie)
                            AlterPNGPaletteZombie(blankFolder  + u + "/" + addon + u + extra_name + "_Large_face" + dir + "_death_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + extra_name + "_Large_face" + dir + "_death_" + f + ".png", simplepalettes);
                        else
                            AlterPNGPalette(blankFolder  + u + "/" + addon + u + extra_name + "_Large_face" + dir + "_death_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + extra_name + "_Large_face" + dir + "_death_" + f + ".png", simplepalettes);
                    }
                }
                /*
                Directory.CreateDirectory("gifs/" + altFolder);

                List<string> imageNames = new List<string>(4 * 8 * 16);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add(folder + "/color" + p + "/" + u + "_Large_face" + dir + "_death_" + f + ".png");
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running explosion GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_death_animated");
                */
            }

        }

        public static void OLD_processExplosionLargeW(string u, int palette, MagicaVoxelData[] voxels, bool shadowless)
        {
            Console.WriteLine("Processing: " + u);
            //            BinaryReader bin = new BinaryReader(File.Open(u + "_Large_W.vox", FileMode.Open));
            //renderLarge(parsed, 0, 0, 0)[0].Save("junk_" + u + ".png");
            if(palette >= 0)
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
                wditheredcurrent = wdithered[palette];
            }
            else
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[0];
                wditheredcurrent = wdithered[0];
            }
            //MagicaVoxelData[][] explode = VoxelLogic.FieryExplosionDoubleW(parsed, false, true); //((CurrentMobilities[UnitLookup[u]] == MovementType.Immobile) ? false : true)
            string folder = ("frames/" + altFolder);
            Directory.CreateDirectory(folder); //("color" + i);

            for(int d = 0; d < 4; d++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostLarge(voxels, d), multiplier, 40, 40, 120, 120, 80), 1 + bonus * multiplier / 2);
                byte[][,,] explode = TransformLogic.FieryExplosionW(colors, false, false);

                for(int frame = 0; frame < 12; frame++)
                {

                    byte[][] b = processFrameHugeW(TransformLogic.SealGaps(explode[frame]), (palette >= 0) ? palette : 0, frame, 8, true, shadowless);

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + ((palette >= 0) ? palette : 99) + "_" + u + "_Large_face" + d + "_fiery_explode_" + frame + ".png", imi, true);
                    WritePNG(png, b, (palette >= 0) ? simplepalettes[palette] : basepalette);
                }
            }

            if(palette >= 0)
            {
                Directory.CreateDirectory("gifs/" + altFolder);
                ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
                startInfo.UseShellExecute = false;
                string s = "";

                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 12; frame++)
                    {
                        s += folder + "/palette" + palette + "_" + u + "_Large_face" + d + "_fiery_explode_" + frame + ".png ";
                    }
                }

                startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_explosion_animated.gif";
                Console.WriteLine("Running convert.exe ...");
                Process.Start(startInfo).WaitForExit();
            }
            else
            {

                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < 12; f++)
                    {
                        AlterPNGPalette(folder + "/palette" + 99 + "_" + u + "_Large_face" + dir + "_fiery_explode_" + f + ".png",
                            folder + "/color{0}/" + u + "_Large_face" + dir + "_fiery_explode_" + f + ".png", simplepalettes);
                    }
                }
                /*
                Directory.CreateDirectory("gifs/" + altFolder);

                List<string> imageNames = new List<string>(4 * 8 * 16);

                for(int p = 0; p < 8; p++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        for(int f = 0; f < 12; f++)
                        {
                            imageNames.Add(folder + "/color" + p + "/" + u + "_Large_face" + dir + "_fiery_explode_" + f + ".png");
                        }
                    }
                }
                Directory.CreateDirectory("gifs/" + altFolder);
                Console.WriteLine("Running GIF conversion ...");
                WriteGIF(imageNames, 11, "gifs/" + altFolder + u + "_explosion_animated");
                */
            }

        }



        public static void processExplosionHugeWSuper(string u, int palette, MagicaVoxelData[] voxels, bool shadowless)
        {
            Console.WriteLine("Processing: " + u);
            bool zombie = u.StartsWith("Zombie_");
            if(addon.Length > 0)
                u = u.Remove(0, addon.Length);

            if(palette >= 0)
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
                wditheredcurrent = wdithered[palette];
            }
            else
            {
                VoxelLogic.wcolors = VoxelLogic.wpalettes[0];
                wditheredcurrent = wdithered[0];
            }
            string folder = ("frames/" + altFolder);


            if(RENDER)
            {
                byte[,,] colors = TransformLogic.VoxListToLargerArray(voxels, 2, 40, 40, 80, 80, 60);

                Model m = Model.FromModelCU(colors);

                byte[][,,] explode = CURedux.FieryDeathW(u, m);

                for(int d = 0; d < 4; d++)
                {
                    //byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostLarge(voxels, d), multiplier * 2, 40, 40, 80, 80, 60), 1 + bonus * multiplier);

                    // byte[,,] colors = TransformLogic.RunCA(
                    //     TransformLogic.ExpandBounds(
                    //             TransformLogic.ScalePartial(TransformLogic.VoxListToArray(VoxelLogic.BasicRotateAlmostLarge(voxels, d), 40, 40, 40), 2, 2, 2), 160, 160, 120),
                    //     1 + bonus * multiplier * 2);


                    //                byte[,,] colors = TransformLogic.RunCA(
                    //                    TransformLogic.SealGaps(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostLarge(voxels, d), multiplier * 2, 40, 40, 80, 80, 60)),
                    //                    1 + bonus * multiplier * 2);
                    //byte[][,,] explode = TransformLogic.FieryExplosionW(colors, false, false);

                    for(int frame = 0; frame < 12; frame++)
                    {
                        byte[][] b = processFrameMassiveW(TransformLogic.SealGaps(
                               TransformLogic.RotateYaw(explode[frame], d * 90)
                            ), (palette >= 0) ? palette : 0, frame, 8, true, shadowless);

                        ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);

                        PngWriter png = (palette >= 0)
                            ? (FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + addon + u + "_Huge_face" + d + "_death_" + frame + ".png", imi, true))
                            : (FileHelper.CreatePngWriter(blankFolder  + u + "/" + addon + u + "_Huge_face" + d + "_death_" + frame + ".png", imi, true));
                        WritePNG(png, b, (palette >= 0) ? simplepalettes[palette] : basepalette);
                    }
                }
            }

            if(palette >= 0)
            {
                Directory.CreateDirectory("gifs/" + altFolder);
                ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
                startInfo.UseShellExecute = false;
                string s = "";

                for(int d = 0; d < 4; d++)
                {
                    for(int frame = 0; frame < 12; frame++)
                    {
                        s += folder + "/palette" + palette + "_" + addon + u + "_Huge_face" + d + "_death_" + frame + ".png ";
                    }
                }

                startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + addon + u + "_death_super_animated.gif";
                Console.WriteLine("Running convert.exe ...");
                Process.Start(startInfo).WaitForExit();
            }
            else
            {


                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < 12; f++)
                    {
                        if(zombie)
                            AlterPNGPaletteZombie(blankFolder  + u + "/" + addon + u + "_Huge_face" + dir + "_death_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_death_" + f + ".png", simplepalettes);
                        else
                            AlterPNGPalette(blankFolder  + u + "/" + addon + u + "_Huge_face" + dir + "_death_" + f + ".png",
                                "color{0}/" + u + "/" + addon + u + "_Huge_face" + dir + "_death_" + f + ".png", simplepalettes);
                    }
                }
            }
        }
        /*
        public static void processExplosionLargeWHat(string u, int palette, string hat, MagicaVoxelData[] headpoints)
        {
            Console.WriteLine("Processing: " + u + " " + hat + ", palette " + palette);
            Stream body = File.Open(u + "_Large_W.vox", FileMode.Open);
            BinaryReader bin = new BinaryReader(body);
            int framelimit = 12;

            string folder = ("frames");//"color" + i;
            Directory.CreateDirectory(folder); //("color" + i);
            Bitmap bmp = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);

            PixelFormat pxf = PixelFormat.Format32bppArgb;

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
            int stride = bmpData.Stride;
            bmp.UnlockBits(bmpData);
            Bitmap hat_image = new Bitmap(bmp);
            Bitmap body_image = new Bitmap(bmp);
            Graphics body_graphics = Graphics.FromImage(body_image);
            Graphics hat_graphics = Graphics.FromImage(hat_image);
            for(int dir = 0; dir < 4; dir++)
            {
                int minimum_z = headpoints[0 + dir * 2].z + 16;

                for(int f = 0; f < framelimit; f++)
                {

                    int jitter = (((f % 4) % 3) + ((f % 4) / 3)) * 2;
                    if(framelimit >= 8) jitter = ((f % 8 > 4) ? 4 - ((f % 8) ^ 4) : f % 8);
                    minimum_z += (10 - ((f * f > 22) ? 22 : f * f)) * 7 / 5;
                    if(minimum_z <= 0) minimum_z = 0;
                    int body_coord = TallFaces.voxelToPixelHugeW(0, 0, headpoints[0 + dir * 2].x, headpoints[0 + dir * 2].y + ((minimum_z == 0) ? 0 : (jitter - 2) * 2),
                        minimum_z, (byte)(253 - headpoints[0 + dir * 2].color) / 4, stride, jitter, true) / 4;
                    //     model_headpoints.AppendLine("EXPLODE: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //         ((body_coord % (stride / 4) - 32) / 2 + 80) + ", y " + (308 - ((body_coord / (stride / 4) - (308 - 90)) / 2)));
                    int hat_coord = TallFaces.voxelToPixelLargeW(0, 0, headpoints[1 + dir * 2].x, headpoints[1 + dir * 2].y,
                        headpoints[1 + dir * 2].z, (byte)(253 - headpoints[1 + dir * 2].color) / 4, stride, 0, true) / 4;
                    //     hat_headpoints.AppendLine("EXPLODE_HAT: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //         ((hat_coord % (stride / 4) - 32) / 2) + ", y " + (108 - ((hat_coord / (stride / 4) - 78) / 2)));
                    Image h2 = Image.FromFile(altFolder + "palette" + ((hat == "Woodsman") ? 44 : (hat == "Farmer") ? 49 : (palette == 7 || palette == 8 || palette == 42) ? 7 : 0) + "_"
                        // + ((palette == 7 || palette == 8 || palette == 42) ? "Spirit_" : "Generic_Male_")
                        + hat + "_Hat_face" + dir + "_" + ((hat == "Farmer") ? 0 : f % 4) + ".png");
                    hat_image = new Bitmap(h2);
                    h2.Dispose();
                    Image b2 = Image.FromFile(folder + "/palette" + palette + "_" + u + "_Large_face" + dir + "_fiery_explode_" + f + ".png");
                    body_image = new Bitmap(b2);
                    b2.Dispose();

                    VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];

                    hat_graphics = Graphics.FromImage(hat_image);
                    body_graphics = Graphics.FromImage(body_image);
                    body_graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    body_graphics.DrawImage(hat_image, 80 + (((body_coord % (stride / 4) - 32) / 2) - ((hat_coord % (stride / 4) - 32) / 2)),
                         40 + (((body_coord / (stride / 4) - (108 - 30)) / 2) - ((hat_coord / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
                    //                    model_headpoints.AppendLine("EXPLODE: " + u + "_" + hat + " facing " + dir + " frame " + f + ": x " +
                    //                        (80 + (((body_coord % (stride / 4) - 32) / 2) - ((hat_coord % (stride / 4) - 32) / 2))) +
                    //                        ", y " + (40 + (((body_coord / (stride / 4) - (108 - 30)) / 2) - ((hat_coord / (stride / 4) - (108 - 30)) / 2))));
                    body_image.Save(folder + "/palette" + palette + "_" + u + "_" + hat + "_Large_face" + dir + "_fiery_explode_" + f + ".png", ImageFormat.Png);

                }
            }

            body_graphics.Dispose();
            hat_graphics.Dispose();
            hat_image.Dispose();
            body_image.Dispose();
            bmp.Dispose();
            body.Close();
            bin.Close();

            System.IO.Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_" + hat + "_Large_face" + d + "_fiery_explode_" + frame + ".png ";
                }
            }

            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_" + hat + "_explosion_animated.gif";
            Console.WriteLine("Running convert.exe ...");
            Process.Start(startInfo).WaitForExit();

            //bin.Close();
        }
        */
        public static void processExplosionHugeW(string u, int palette)
        {
            Console.WriteLine("Processing: " + u + ", palette " + palette);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Huge_W.vox", FileMode.Open));
            MagicaVoxelData[] parsed = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //renderLarge(parsed, 0, 0, 0)[0].Save("junk_" + u + ".png");

            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];

            // MagicaVoxelData[][] explode = VoxelLogic.FieryExplosionQuadW(parsed, false, true); //((CurrentMobilities[UnitLookup[u]] == MovementType.Immobile) ? false : true)
            string folder = ("frames");

            Directory.CreateDirectory(folder); //("color" + i);

            for(int d = 0; d < 4; d++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostHuge(parsed, d), multiplier, 80, 80, 160, 160, 120), 1 + bonus * multiplier / 2);
                byte[][,,] explode = TransformLogic.FieryExplosionW(colors, false, false);

                for(int frame = 0; frame < 12; frame++)
                {

                    byte[][] b = processFrameMassiveW(explode[frame], palette, frame, 8, true, false);

                    ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Huge_face" + d + "_fiery_explode_" + frame + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Huge_face" + d + "_fiery_explode_" + frame + ".png ";
                }
            }

            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_explosion_animated.gif";
            Console.WriteLine("Running convert.exe ...");
            Process.Start(startInfo).WaitForExit();

            //bin.Close();
        }
        public static void processExplosionHugeW(string u, int palette, bool shadowless)
        {
            Console.WriteLine("Processing: " + u + ", palette " + palette);
            BinaryReader bin = new BinaryReader(File.Open(u + "_Huge_W.vox", FileMode.Open));
            MagicaVoxelData[] parsed = VoxelLogic.FromMagicaRaw(bin).ToArray();
            //renderLarge(parsed, 0, 0, 0)[0].Save("junk_" + u + ".png");

            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];
            // MagicaVoxelData[][] explode = VoxelLogic.FieryExplosionQuadW(parsed, false, true); //((CurrentMobilities[UnitLookup[u]] == MovementType.Immobile) ? false : true)
            string folder = ("frames");

            Directory.CreateDirectory(folder); //("color" + i);

            for(int d = 0; d < 4; d++)
            {
                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateAlmostHuge(parsed, d), multiplier, 80, 80, 160, 160, 120), 1 + bonus * multiplier / 2);
                byte[][,,] explode = TransformLogic.FieryExplosionW(colors, false, false);

                for(int frame = 0; frame < 12; frame++)
                {
                    byte[][] b = processFrameMassiveW(explode[frame], palette, frame, 8, true, shadowless);

                    ImageInfo imi = new ImageInfo(widthMassive, heightMassive, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Huge_face" + d + "_fiery_explode_" + frame + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Huge_face" + d + "_fiery_explode_" + frame + ".png ";
                }
            }

            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_explosion_animated.gif";
            Console.WriteLine("Running convert.exe ...");
            Process.Start(startInfo).WaitForExit();

            //bin.Close();
        }

        public static void processPureAttackW(string u, int palette)
        {
            Console.WriteLine("Processing: " + u);
            BinaryReader[] bins = new BinaryReader[12];
            MagicaVoxelData[][] attacking = new MagicaVoxelData[12][];
            for(int i = 0; i < 12; i++)
            {
                bins[i] = new BinaryReader(File.Open("animations/" + u + "/" + u + "_Attack_" + i + ".vox", FileMode.Open));
                attacking[i] = VoxelLogic.FromMagicaRaw(bins[i]).ToArray();
                for(int j = 0; j < attacking[i].Length; j++)
                {
                    attacking[i][j].x += 10;
                    attacking[i][j].y += 10;
                    /*if((254 - attacking[i][j].color) % 4 == 0)
                    {
                        Console.WriteLine("Base Found: X" + attacking[i][j].x + ", Y " + attacking[i][j].y + ", Z " + attacking[i][j].z);
                    }*/
                }
                bins[i].Close();
            }
            VoxelLogic.wcolors = VoxelLogic.wpalettes[palette];
            wditheredcurrent = wdithered[palette];
            string folder = ("frames");

            Directory.CreateDirectory(folder); //("color" + i);

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(attacking[frame], d), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);

                    byte[][] b = processFrameLargeW(colors, palette, frame, 12, true, true);
                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + u + "_Attack_face" + d + "_" + frame + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            for(int d = 0; d < 4; d++)
            {
                for(int frame = 0; frame < 12; frame++)
                {
                    s += folder + "/palette" + palette + "_" + u + "_Attack_face" + d + "_" + frame + ".png ";
                }
            }

            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + u + "_attack_animated.gif";
            Console.WriteLine("Running convert.exe ...");
            Process.Start(startInfo).WaitForExit();
            //Console.ReadKey();
            //bin.Close();
        }

        public static void processUnitLargeWMecha(string moniker = "Maku", bool still = true,
            string legs = "Armored", string torso = "Armored", string left_arm = "Armored", string right_arm = "Armored", string head = "Blocky", string left_weapon = null, string right_weapon = null)
        {

            Dictionary<string, List<MagicaVoxelData>> components = new Dictionary<string, List<MagicaVoxelData>>
            { {"Legs", VoxelLogic.readPart(legs + "_Legs")},
                {"Torso", VoxelLogic.readPart(torso + "_Torso")},
                 {"Left_Arm", VoxelLogic.readPart(left_arm + "_Left_Arm")},
                 {"Right_Arm", VoxelLogic.readPart(right_arm + "_Right_Arm")},
                 {"Head",  VoxelLogic.readPart(head + "_Head")},
                 {"Left_Weapon", VoxelLogic.readPart(left_weapon)},
                 {"Right_Weapon", VoxelLogic.readPart(right_weapon)}
            };
            List<MagicaVoxelData> work = VoxelLogic.MergeVoxels(components["Head"], components["Torso"], 0);
            work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4, 6, VoxelLogic.clear), work, 2);
            work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, VoxelLogic.clear), work, 3);
            work = VoxelLogic.MergeVoxels(work, components["Legs"], 1);
            Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_0.vox", work, "W", 0, 40, 40, 40);

            MagicaVoxelData[] parsed = work.ToArray();
            for(int i = 0; i < parsed.Length; i++)
            {
                parsed[i].x += 10;
                parsed[i].y += 10;
            }
            int framelimit = 4;

            for(int palette = 0; palette < VoxelLogic.wpalettecount; palette++)
            {
                Console.WriteLine("Processing: " + moniker + ", palette " + palette);
                string folder = (altFolder);//"color" + i;
                Directory.CreateDirectory(folder); //("color" + i);

                for(int dir = 0; dir < 4; dir++)
                {
//                    FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
                    byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60), 1 + bonus * multiplier / 2);

                    for(int f = 0; f < framelimit; f++)
                    {
                        byte[][] b = processFrameLargeW(colors, palette, f, framelimit, still, false);
                        ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                        PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_Large_face" + dir + "_" + f + ".png", imi, true);
                        WritePNG(png, b, simplepalettes[palette]);
                    }
                }


                Directory.CreateDirectory("gifs/" + altFolder);
                ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
                startInfo.UseShellExecute = false;
                string s = "";

                s = folder + "/palette" + palette + "_" + moniker + "_Large_face* ";
                startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_Large_animated.gif";
                Process.Start(startInfo).WaitForExit();

                //bin.Close();

                //            processFiringDouble(u);

                processExplosionLargeW(moniker, palette, work.ToArray(), false);

            }
        }
        //public static void processUnitLargeWMechaFiring(string moniker = "Maku", bool still = true,
        //    string legs = "Armored", string torso = "Armored", string left_arm = "Armored", string right_arm = "Armored", string head = "Blocky", string left_weapon = null, string right_weapon = null,
        //    string left_projectile = null, string right_projectile = null)
        //{
        //    Bitmap bmp = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);

        //    PixelFormat pxf = PixelFormat.Format32bppArgb;

        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
        //    int stride = bmpData.Stride;
        //    bmp.UnlockBits(bmpData);
        //    bmp.Dispose();

        //    string left_name = (left_projectile != null) ? left_projectile : "Nothing";
        //    string right_name = (right_projectile != null) ? right_projectile : "Nothing";


        //    for(int combo = 0; combo < 3; combo++)
        //    {
        //        string firing_name = "Firing_Left";
        //        Dictionary<string, List<MagicaVoxelData>> components = new Dictionary<string, List<MagicaVoxelData>>
        //    { {"Legs", VoxelLogic.readPart(legs + "_Legs")},
        //        {"Torso", VoxelLogic.readPart(torso + "_Torso")},
        //         {"Left_Arm", VoxelLogic.readPart(left_arm + "_Left_Arm")},
        //         {"Right_Arm", VoxelLogic.readPart(right_arm + "_Right_Arm")},
        //         {"Head",  VoxelLogic.readPart(head + "_Head")},
        //         {"Left_Weapon", VoxelLogic.readPart(left_weapon)},
        //         {"Right_Weapon", VoxelLogic.readPart(right_weapon)}
        //    };
        //        List<MagicaVoxelData> work = VoxelLogic.MergeVoxels(components["Head"], components["Torso"], 0),
        //            right_projectors = new List<MagicaVoxelData>(1), left_projectors = new List<MagicaVoxelData>(1);
        //        MagicaVoxelData bogus = new MagicaVoxelData { x = 255, y = 255, z = 255, color = 255 }, right_projector = bogus, left_projector = bogus;
        //        switch(combo)
        //        {
        //            case 0:
        //                if(right_weapon != null)
        //                {
        //                    firing_name = "Firing_Right";
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work, 2);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, VoxelLogic.clear), work, 3);

        //                }
        //                else continue;
        //                break;
        //            case 1:
        //                if(left_weapon != null)
        //                {
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work, 3);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4, 6, VoxelLogic.clear), work, 2);
        //                }
        //                else continue;
        //                break;
        //            case 2:
        //                if(left_weapon != null && right_weapon != null)
        //                {
        //                    firing_name = "Firing_Both";
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work, 2);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work, 3);
        //                }
        //                else continue;
        //                break;
        //        }
        //        work = VoxelLogic.MergeVoxels(work, components["Legs"], 1);

        //        try
        //        {
        //            right_projector = work.First(m => (254 - m.color) == 4 * 6);
        //            right_projector.x += 10;
        //            right_projector.y += 10;
        //            right_projectors.Add(right_projector);
        //        }
        //        catch(InvalidOperationException)
        //        {
        //            right_projector = bogus;
        //        }
        //        try
        //        {
        //            left_projector = work.First(m => (254 - m.color) == 4 * 7);
        //            left_projector.x += 10;
        //            left_projector.y += 10;
        //            left_projectors.Add(left_projector);
        //        }
        //        catch(InvalidOperationException)
        //        {
        //            left_projector = bogus;
        //        }
        //        Directory.CreateDirectory("vox/" + altFolder);
        //        VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_" + firing_name + "_0.vox", work, "W", 0, 40, 40, 40);
        //        MagicaVoxelData[] parsed = work.ToArray();
        //        for(int i = 0; i < parsed.Length; i++)
        //        {
        //            parsed[i].x += 10;
        //            parsed[i].y += 10;
        //        }
        //        int framelimit = 4;

        //        for(int palette = 0; palette < VoxelLogic.wpalettecount; palette++)
        //        {
        //            Console.WriteLine("Processing: " + moniker + ", palette " + palette + ", " + firing_name);
        //            string folder = (altFolder);//"color" + i;
        //            Directory.CreateDirectory(folder); //("color" + i);

        //            for(int dir = 0; dir < 4; dir++)
        //            {
        //                //FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
        //                byte[,,] colors = TransformLogic.RunCA(TransformLogic.VoxListToLargerArray(VoxelLogic.BasicRotateLarge(parsed, dir), multiplier, 60, 60, 60), 3);

        //                for(int f = 0; f < framelimit; f++)
        //                {
        //                    byte[][] b = processFrameLargeW(colors, palette, f, framelimit, still, false);
        //                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
        //                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_"
        //                        + left_name + "_" + right_name + "_" + firing_name + "_Large_face" + dir + "_" + f + ".png", imi, true);
        //                    WritePNG(png, b, simplepalettes[palette]);
        //                }
        //            }

        //            Directory.CreateDirectory("gifs/" + altFolder);
        //            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
        //            startInfo.UseShellExecute = false;
        //            string s = "";

        //            s = folder + "/palette" + palette + "_" + moniker + "_" + left_name + "_" + right_name + "_" + firing_name + "_Large_face* ";
        //            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_"
        //                + left_name + "_" + right_name + "_" + firing_name + "_Large_animated.gif";
        //            Process.Start(startInfo).WaitForExit();

        //            // Animation generation starts here

        //            switch(combo)
        //            {
        //                case 0:
        //                    if(right_projectile == null)
        //                        continue;
        //                    break;
        //                case 1:
        //                    if(left_projectile == null)
        //                        continue;
        //                    break;
        //                case 2:
        //                    if(left_projectile == null || right_projectile == null)
        //                        continue;
        //                    break;
        //            }
        //            for(int f = 0; f < 12; f++)
        //            {
        //                for(int dir = 0; dir < 4; dir++)
        //                {
        //                    List<MagicaVoxelData> right_projectors_adj = VoxelLogic.RotateYaw(right_projectors, dir, 60, 60), left_projectors_adj = VoxelLogic.RotateYaw(left_projectors, dir, 60, 60);
        //                    Bitmap b = new Bitmap(folder + "/palette" + palette + "_" + moniker + "_" + left_name + "_" + right_name + "_" + firing_name + "_Large_face" + dir + "_" + (f % framelimit) + ".png"),
        //                        b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);// new Bitmap("palette50/palette50_Terrain_Huge_face0_0.png"),
        //                    Bitmap b_left = new Bitmap(widthLarge, heightLarge, PixelFormat.Format32bppArgb), b_right = new Bitmap(widthLarge, heightLarge, PixelFormat.Format32bppArgb);
        //                    if(left_projectile != null) b_left = new Bitmap("frames/palette0_" + left_projectile + "_Attack_face" + dir + "_" + f + ".png");
        //                    if(right_projectile != null) b_right = new Bitmap("frames/palette0_" + right_projectile + "_Attack_face" + dir + "_" + f + ".png");
        //                    MagicaVoxelData left_emission = bogus, right_emission = bogus;
        //                    if(left_projectile != null)
        //                    {
        //                        BinaryReader leftbin = new BinaryReader(File.Open("animations/" + left_projectile + "/" + left_projectile + "_Attack_" + f + ".vox", FileMode.Open));
        //                        left_emission = VoxelLogic.RotateYaw(VoxelLogic.FromMagicaRaw(leftbin), dir, 40, 40).First(m => 254 - m.color == 4 * 6);
        //                    }

        //                    if(right_projectile != null)
        //                    {
        //                        BinaryReader rightbin = new BinaryReader(File.Open("animations/" + right_projectile + "/" + right_projectile + "_Attack_" + f + ".vox", FileMode.Open));
        //                        right_emission = VoxelLogic.RotateYaw(VoxelLogic.FromMagicaRaw(rightbin), dir, 40, 40).First(m => 254 - m.color == 4 * 6);
        //                    }
        //                    //left_emission.x += 40;
        //                    //left_emission.y += 40;
        //                    //right_emission.x += 40;
        //                    //right_emission.y += 40;
        //                    //Bitmap b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);
        //                    Graphics g = Graphics.FromImage(b_base);
        //                    if(dir < 2)
        //                    {
        //                        g.DrawImage(b, 80, 160);
        //                        if(right_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, right_projectors_adj.First().x, right_projectors_adj.First().y, right_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, right_emission.x, right_emission.y, right_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        if(left_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, left_projectors_adj.First().x, left_projectors_adj.First().y, left_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, left_emission.x, left_emission.y, left_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_left, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if(right_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, right_projectors_adj.First().x, right_projectors_adj.First().y, right_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, right_emission.x, right_emission.y, right_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        if(left_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, left_projectors_adj.First().x, left_projectors_adj.First().y, left_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, left_emission.x, left_emission.y, left_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_left, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        g.DrawImage(b, 80, 160);
        //                    }
        //                    b_base.Save("frames/palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                        ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile)
        //                        + "_Huge_face" + dir + "_" + f + ".png");

        //                }
        //            }

        //            s = "";
        //            for(int dir = 0; dir < 4; dir++)
        //            {
        //                for(int f = 0; f < 12; f++)
        //                {
        //                    s += "frames/palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                    ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile) +
        //                    "_Huge_face" + dir + "_" + f + ".png ";
        //                }
        //            }
        //            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                        ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile) + "_Huge_animated.gif";
        //            Process.Start(startInfo).WaitForExit();

        //            //bin.Close();

        //            //            processFiringDouble(u);

        //            //processFieryExplosionDoubleW(moniker, work.ToList(), palette);

        //        }
        //    }
        //}
        //public static void processUnitLargeWMechaAiming(string moniker = "Mark_Zero", bool still = true,
        //    string legs = "Armored", string torso = "Armored", string left_arm = "Armored_Aiming", string right_arm = "Armored_Aiming", string head = "Armored_Aiming", string right_weapon = "Rifle", string right_projectile = "Autofire")
        //{
        //    Bitmap bmp = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);

        //    PixelFormat pxf = PixelFormat.Format32bppArgb;

        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
        //    int stride = bmpData.Stride;
        //    bmp.UnlockBits(bmpData);
        //    bmp.Dispose();

        //    string right_name = (right_projectile != null) ? right_projectile : "Nothing";


        //    Dictionary<string, List<MagicaVoxelData>> components = new Dictionary<string, List<MagicaVoxelData>>
        //    { {"Legs", VoxelLogic.readPart(legs + "_Legs")},
        //        {"Torso", VoxelLogic.readPart(torso + "_Torso")},
        //         {"Left_Arm", VoxelLogic.readPart(left_arm + "_Left_Arm")},
        //         {"Right_Arm", VoxelLogic.readPart(right_arm + "_Right_Arm")},
        //         {"Head",  VoxelLogic.readPart(head + "_Head")},
        //         {"Right_Weapon", VoxelLogic.readPart(right_weapon)}
        //    };
        //    List<MagicaVoxelData> work = VoxelLogic.MergeVoxels(components["Head"], components["Torso"], 0),
        //                            projectors = new List<MagicaVoxelData>(1);
        //    MagicaVoxelData bogus = new MagicaVoxelData { x = 255, y = 255, z = 255, color = 255 }, right_projector = bogus;

        //    work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), work, 2);
        //    work = VoxelLogic.MergeVoxels(components["Left_Arm"], work, 3);
        //    work = VoxelLogic.MergeVoxels(work, components["Legs"], 1);

        //    work = VoxelLogic.RotateYaw(work, 1, 40, 40);
        //    try
        //    {
        //        right_projector = work.First(m => (254 - m.color) == 4 * 6);
        //        right_projector.x += 10;
        //        right_projector.y += 10;
        //        projectors.Add(right_projector);
        //    }
        //    catch(InvalidOperationException)
        //    {
        //        right_projector = bogus;
        //    }
        //    work = VoxelLogic.PlaceShadowsPartialW(work);
        //    Directory.CreateDirectory("vox/" + altFolder);
        //    VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_Firing_Both_0.vox", work, "W", 0, 40, 40, 40);
        //    MagicaVoxelData[] parsed = work.ToArray();
        //    for(int i = 0; i < parsed.Length; i++)
        //    {
        //        parsed[i].x += 10;
        //        parsed[i].y += 10;
        //    }
        //    int framelimit = 4;

        //    for(int palette = 0; palette < VoxelLogic.wpalettecount; palette++)
        //    {
        //        Console.WriteLine("Processing: " + moniker + ", palette " + palette + ", " + "Firing_Both");
        //        string folder = (altFolder);//"color" + i;
        //        Directory.CreateDirectory(folder); //("color" + i);

        //        for(int dir = 0; dir < 4; dir++)
        //        {
        //            FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
        //            for(int f = 0; f < framelimit; f++)
        //            {
        //                byte[][] b = processFrameLargeW(faces, palette, f, framelimit, still, false);
        //                ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
        //                PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_" + right_name + "_Firing_Both_Large_face" + dir + "_" + f + ".png", imi, true);
        //                WritePNG(png, b, simplepalettes[palette]);
        //            }
        //        }


        //        Directory.CreateDirectory("gifs/" + altFolder);
        //        ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
        //        startInfo.UseShellExecute = false;
        //        string s = "";

        //        s = folder + "/palette" + palette + "_" + moniker + "_" + right_name + "_Firing_Both_Large_face* ";
        //        startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_Firing_Both_Large_animated.gif";
        //        Process.Start(startInfo).WaitForExit();

        //        // Animation generation starts here

        //        if(right_projectile == null)
        //            continue;

        //        for(int f = 0; f < 12; f++)
        //        {
        //            for(int dir = 0; dir < 4; dir++)
        //            {
        //                List<MagicaVoxelData> projectors_adj = VoxelLogic.RotateYaw(projectors, dir, 60, 60);
        //                Bitmap b = new Bitmap(folder + "/palette" + palette + "_" + moniker + "_" + right_projectile + "_Firing_Both_Large_face" + dir + "_" + (f % framelimit) + ".png"),
        //                    b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);// new Bitmap("palette50/palette50_Terrain_Huge_face0_0.png"),
        //                Bitmap b_right = new Bitmap(widthLarge, heightLarge, PixelFormat.Format32bppArgb);
        //                if(right_projectile != null) b_right = new Bitmap("frames/palette0_" + right_projectile + "_Attack_face" + dir + "_" + f + ".png");
        //                MagicaVoxelData emission = bogus;
        //                if(right_projectile != null)
        //                {
        //                    BinaryReader bin = new BinaryReader(File.Open("animations/" + right_projectile + "/" + right_projectile + "_Attack_" + f + ".vox", FileMode.Open));
        //                    emission = VoxelLogic.RotateYaw(VoxelLogic.FromMagicaRaw(bin), dir, 40, 40).First(m => 254 - m.color == 4 * 6);
        //                }
        //                //left_emission.x += 40;
        //                //left_emission.y += 40;
        //                //right_emission.x += 40;
        //                //right_emission.y += 40;
        //                //Bitmap b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);
        //                Graphics g = Graphics.FromImage(b_base);
        //                if(dir < 2)
        //                {
        //                    g.DrawImage(b, 80, 160);
        //                    if(projectors_adj.Count > 0)
        //                    {
        //                        int proj_location = TallFaces.voxelToPixelHugeW(0, 0, projectors_adj.First().x, projectors_adj.First().y, projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                            emit_location = TallFaces.voxelToPixelHugeW(0, 0, emission.x, emission.y, emission.z, 0, stride, 0, still) / 4;

        //                        g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                             160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                    }
        //                }
        //                else
        //                {
        //                    if(projectors_adj.Count > 0)
        //                    {
        //                        int proj_location = TallFaces.voxelToPixelHugeW(0, 0, projectors_adj.First().x, projectors_adj.First().y, projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                            emit_location = TallFaces.voxelToPixelHugeW(0, 0, emission.x, emission.y, emission.z, 0, stride, 0, still) / 4;

        //                        g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                             160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                    }
        //                    g.DrawImage(b, 80, 160);
        //                }
        //                b_base.Save("frames/palette" + palette + "_" + moniker + "_Firing_Both_" + right_projectile
        //                    + "_Huge_face" + dir + "_" + f + ".png");

        //            }
        //        }

        //        s = "";
        //        for(int dir = 0; dir < 4; dir++)
        //        {
        //            for(int f = 0; f < 12; f++)
        //            {
        //                s += "frames/palette" + palette + "_" + moniker + "_Firing_Both_" + right_projectile + "_Huge_face" + dir + "_" + f + ".png ";
        //            }
        //        }
        //        startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_Firing_Both_" +
        //                    right_projectile + "_Huge_animated.gif";
        //        Process.Start(startInfo).WaitForExit();


        //        //bin.Close();

        //        //            processFiringDouble(u);

        //        //processFieryExplosionDoubleW(moniker, work.ToList(), palette);
        //    }
        //}
        //public static void processUnitLargeWMechaSwinging(string moniker = "Maku", bool still = true,
        //    string legs = "Armored", string torso = "Armored", string left_arm = "Armored", string right_arm = "Armored", string head = "Blocky", string left_weapon = null, string right_weapon = "Katana",
        //    string left_projectile = null, string right_projectile = "Swing")
        //{
        //    Bitmap bmp = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);

        //    PixelFormat pxf = PixelFormat.Format32bppArgb;

        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
        //    int stride = bmpData.Stride;
        //    bmp.UnlockBits(bmpData);
        //    bmp.Dispose();

        //    string left_name = (left_projectile != null) ? left_projectile : "Nothing";
        //    string right_name = (right_projectile != null) ? right_projectile : "Nothing";

        //    for(int combo = 0; combo < 3; combo++)
        //    {
        //        string firing_name = "Firing_Left";
        //        Dictionary<string, List<MagicaVoxelData>> components = new Dictionary<string, List<MagicaVoxelData>>
        //        {
        //            { "Legs", VoxelLogic.readPart(legs + "_Legs")},
        //            { "Torso", VoxelLogic.readPart(torso + "_Torso")},
        //            {"Left_Arm", VoxelLogic.readPart(left_arm + "_Left_Arm")},
        //            {"Right_Arm", VoxelLogic.readPart(right_arm + "_Right_Arm")},
        //            {"Head",  VoxelLogic.readPart(head + "_Head")},
        //            {"Left_Weapon", VoxelLogic.readPart(left_weapon)},
        //            {"Right_Weapon", VoxelLogic.readPart(right_weapon)}
        //        };
        //        List<MagicaVoxelData> work = VoxelLogic.MergeVoxels(components["Head"], components["Torso"], 0),
        //            right_projectors = new List<MagicaVoxelData>(1), left_projectors = new List<MagicaVoxelData>(1);
        //        MagicaVoxelData bogus = new MagicaVoxelData { x = 255, y = 255, z = 255, color = 255 }, right_projector = bogus, left_projector = bogus;
        //        switch(combo)
        //        {
        //            case 0:
        //                if(right_weapon != null)
        //                {
        //                    firing_name = "Firing_Right";
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work, 2);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, VoxelLogic.clear), work, 3);

        //                }
        //                else continue;
        //                break;
        //            case 1:
        //                if(left_weapon != null)
        //                {
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work, 3);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4, 6, VoxelLogic.clear), work, 2);
        //                }
        //                else continue;
        //                break;
        //            case 2:
        //                if(left_weapon != null && right_weapon != null)
        //                {
        //                    firing_name = "Firing_Both";
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work, 2);
        //                    work = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work, 3);
        //                }
        //                else continue;
        //                break;
        //        }
        //        work = VoxelLogic.MergeVoxels(work, components["Legs"], 1);

        //        try
        //        {
        //            right_projector = work.First(m => (254 - m.color) == 4 * 6);
        //            right_projector.x += 10;
        //            right_projector.y += 10;
        //            right_projectors.Add(right_projector);
        //        }
        //        catch(InvalidOperationException)
        //        {
        //            right_projector = bogus;
        //        }
        //        try
        //        {
        //            left_projector = work.First(m => (254 - m.color) == 4 * 7);
        //            left_projector.x += 10;
        //            left_projector.y += 10;
        //            left_projectors.Add(left_projector);
        //        }
        //        catch(InvalidOperationException)
        //        {
        //            left_projector = bogus;
        //        }
        //        work = VoxelLogic.PlaceShadowsPartialW(work);
        //        Directory.CreateDirectory("vox/" + altFolder);
        //        VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_" + firing_name + "_0.vox", work, "W", 0, 40, 40, 40);
        //        MagicaVoxelData[] parsed = work.ToArray();
        //        for(int i = 0; i < parsed.Length; i++)
        //        {
        //            parsed[i].x += 10;
        //            parsed[i].y += 10;
        //        }
        //        int framelimit = 4;
        //        byte[] spreadColors = new byte[] { 253 - 12 * 4, 253 - 40 * 4 };
        //        for(int palette = 0; palette < VoxelLogic.wpalettecount; palette++)
        //        {
        //            Console.WriteLine("Processing: " + moniker + ", palette " + palette + ", " + firing_name);
        //            string folder = (altFolder);//"color" + i;
        //            Directory.CreateDirectory(folder); //("color" + i);

        //            for(int dir = 0; dir < 4; dir++)
        //            {
        //                FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
        //                for(int f = 0; f < framelimit; f++)
        //                {
        //                    byte[][] b = processFrameLargeW(faces, palette, f, framelimit, still, false);
        //                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
        //                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_" + left_name + "_" + right_name + "_" + firing_name + "_Large_face" + dir + "_" + f + ".png", imi, true);
        //                    WritePNG(png, b, simplepalettes[palette]);

        //                }
        //            }

        //            Directory.CreateDirectory("gifs/" + altFolder);
        //            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
        //            startInfo.UseShellExecute = false;
        //            string s = "";

        //            s = folder + "/palette" + palette + "_" + moniker + "_" + left_name + "_" + right_name + "_" + firing_name + "_Large_face* ";
        //            startInfo.Arguments = "-dispose background -delay 25 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_" + firing_name + "_Large_animated.gif";
        //            Process.Start(startInfo).WaitForExit();

        //            // Animation generation starts here

        //            switch(combo)
        //            {
        //                case 0:
        //                    if(right_projectile != "Swing")
        //                        continue;
        //                    break;
        //                case 1:
        //                    if(left_projectile != "Swing")
        //                        continue;
        //                    break;
        //                case 2:
        //                    if(left_projectile != "Swing" && right_projectile != "Swing")
        //                        continue;
        //                    break;
        //            }
        //            for(int f = 0; f < 12; f++)
        //            {
        //                Dictionary<string, List<MagicaVoxelData>> components2 = new Dictionary<string, List<MagicaVoxelData>>
        //        {
        //            { "Legs", VoxelLogic.readPart(legs + "_Legs")},
        //            { "Torso", VoxelLogic.readPart(torso + "_Torso")},
        //            {"Left_Arm", VoxelLogic.readPart(left_arm + "_Left_Arm")},
        //            {"Right_Arm", VoxelLogic.readPart(right_arm + "_Right_Arm")},
        //            {"Head",  VoxelLogic.readPart(head + "_Head")},
        //            {"Left_Weapon", VoxelLogic.readPart(left_weapon)},
        //            {"Right_Weapon", VoxelLogic.readPart(right_weapon)}
        //        };

        //                int[] swingTable = new int[] { 0, 330, 300, 270, 240, 225, 285, 345, 0, 0, 0, 0, },
        //                     effectTable = new int[] { -1, -1, -1, -1, -1, 215, 225, 255, -1, -1, -1, -1, };
        //                //int[] swingTable = new int[] { 0, 330, 300, 270, 240, 210, 180, 150, 120, 90, 60, 30};
        //                List<MagicaVoxelData> work2 = VoxelLogic.MergeVoxels(components["Head"], components["Torso"], 0);
        //                switch(combo)
        //                {
        //                    case 0:
        //                        if(right_weapon != null && right_projectile == "Swing")
        //                        {
        //                            firing_name = "Firing_Right";
        //                            if(effectTable[f] >= 0)
        //                            {
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartialSpread(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4),
        //                                    swingTable[f], effectTable[f], 40, 40, 40, spreadColors), work2, 2);
        //                            }
        //                            else
        //                            {
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartial(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4),
        //                                    swingTable[f], 40, 40, 40), work2, 2);
        //                            }
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, VoxelLogic.clear), work2, 3);
        //                            right_projectors.Clear();

        //                        }
        //                        else if(right_weapon != null)
        //                        {
        //                            firing_name = "Firing_Right";
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work2, 2);
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, VoxelLogic.clear), work2, 3);

        //                        }
        //                        else continue;
        //                        break;
        //                    case 1:
        //                        if(left_weapon != null && left_projectile == "Swing")
        //                        {
        //                            if(effectTable[f] >= 0)
        //                            {
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartialSpread(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4),
        //                                    swingTable[f], effectTable[f], 40, 40, 40, spreadColors), work2, 3);
        //                            }
        //                            else
        //                            {
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartial(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4),
        //                                    swingTable[f], 40, 40, 40), work2, 3);
        //                            }
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4, 6, VoxelLogic.clear), work2, 2);
        //                            left_projectors.Clear();
        //                        }
        //                        else if(left_weapon != null)
        //                        {
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work2, 3);
        //                            work2 = VoxelLogic.MergeVoxels(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4, 6, VoxelLogic.clear), work2, 2);
        //                        }
        //                        else continue;
        //                        break;
        //                    case 2:

        //                        if(left_weapon != null && right_weapon != null)
        //                        {

        //                            firing_name = "Firing_Both";
        //                            if(right_projectile == "Swing")
        //                            {
        //                                if(effectTable[f] >= 0)
        //                                {
        //                                    work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartialSpread(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4),
        //                                        swingTable[f], effectTable[f], 40, 40, 40, spreadColors), work2, 2);
        //                                }
        //                                else
        //                                {
        //                                    work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartial(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4),
        //                                        swingTable[f], 40, 40, 40), work2, 2);
        //                                }
        //                            }
        //                            else
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Right_Arm"], components["Right_Weapon"], 4), 3, 40, 40), work2, 2);
        //                            if(left_projectile == "Swing")
        //                            {
        //                                if(effectTable[f] >= 0)
        //                                {
        //                                    work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartialSpread(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4),
        //                                        swingTable[f], effectTable[f], 40, 40, 40, spreadColors), work2, 3);
        //                                }
        //                                else
        //                                {
        //                                    work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitchPartial(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4),
        //                                        swingTable[f], 40, 40, 40), work2, 3);
        //                                }
        //                            }
        //                            else
        //                                work2 = VoxelLogic.MergeVoxels(VoxelLogic.RotatePitch(VoxelLogic.MergeVoxels(components["Left_Arm"], components["Left_Weapon"], 4, 6, 254 - 7 * 4), 3, 40, 40), work2, 3);
        //                        }
        //                        else continue;
        //                        break;
        //                }
        //                work2 = VoxelLogic.MergeVoxels(work2, components["Legs"], 1);

        //                try
        //                {
        //                    right_projector = work.First(m => (254 - m.color) == 4 * 6);
        //                    right_projector.x += 10;
        //                    right_projector.y += 10;
        //                    right_projectors.Add(right_projector);
        //                }
        //                catch(InvalidOperationException)
        //                {
        //                    right_projector = bogus;
        //                }
        //                try
        //                {
        //                    left_projector = work.First(m => (254 - m.color) == 4 * 7);
        //                    left_projector.x += 10;
        //                    left_projector.y += 10;
        //                    left_projectors.Add(left_projector);
        //                }
        //                catch(InvalidOperationException)
        //                {
        //                    left_projector = bogus;
        //                }

        //                work2 = VoxelLogic.PlaceShadowsPartialW(work2);
        //                parsed = work2.ToArray();
        //                for(int i = 0; i < parsed.Length; i++)
        //                {
        //                    parsed[i].x += 10;
        //                    parsed[i].y += 10;
        //                }

        //                for(int dir = 0; dir < 4; dir++)
        //                {
        //                    FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
        //                    List<MagicaVoxelData> right_projectors_adj = VoxelLogic.RotateYaw(right_projectors, dir, 60, 60), left_projectors_adj = VoxelLogic.RotateYaw(left_projectors, dir, 60, 60);

        //                    byte[][] bytez = processFrameLargeW(faces, palette, dir, f % 4, 4, still, false);
        //                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
        //                    PngWriter png = FileHelper.CreatePngWriter("temp_swinging_" + dir + "_" + f + ".png", imi, true);
        //                    WritePNG(png, bytez, simplepalettes[palette]);


        //                    Bitmap b = new Bitmap("temp_swinging_" + dir + "_" + f + ".png"),
        //                        b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);// new Bitmap("palette50/palette50_Terrain_Huge_face0_0.png"),
        //                    Bitmap b_left = new Bitmap(widthLarge, heightLarge, PixelFormat.Format32bppArgb), b_right = new Bitmap(widthLarge, heightLarge, PixelFormat.Format32bppArgb);
        //                    if(left_projectile != null && left_projectile != "Swing") b_left = new Bitmap("frames/palette0_" + left_projectile + "_Attack_face" + dir + "_" + f + ".png");
        //                    if(right_projectile != null && right_projectile != "Swing") b_right = new Bitmap("frames/palette0_" + right_projectile + "_Attack_face" + dir + "_" + f + ".png");
        //                    MagicaVoxelData left_emission = bogus, right_emission = bogus;
        //                    if(left_projectile != null && left_projectile != "Swing")
        //                    {
        //                        BinaryReader leftbin = new BinaryReader(File.Open("animations/" + left_projectile + "/" + left_projectile + "_Attack_" + f + ".vox", FileMode.Open));
        //                        left_emission = VoxelLogic.RotateYaw(VoxelLogic.FromMagicaRaw(leftbin), dir, 40, 40).First(m => 254 - m.color == 4 * 6);
        //                    }

        //                    if(right_projectile != null && right_projectile != "Swing")
        //                    {
        //                        BinaryReader rightbin = new BinaryReader(File.Open("animations/" + right_projectile + "/" + right_projectile + "_Attack_" + f + ".vox", FileMode.Open));
        //                        right_emission = VoxelLogic.RotateYaw(VoxelLogic.FromMagicaRaw(rightbin), dir, 40, 40).First(m => 254 - m.color == 4 * 6);
        //                    }
        //                    //left_emission.x += 40;
        //                    //left_emission.y += 40;
        //                    //right_emission.x += 40;
        //                    //right_emission.y += 40;
        //                    //Bitmap b_base = new Bitmap(widthHuge, heightHuge, PixelFormat.Format32bppArgb);
        //                    Graphics g = Graphics.FromImage(b_base);
        //                    if(dir < 2)
        //                    {
        //                        g.DrawImage(b, 80, 160);
        //                        if(right_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, right_projectors_adj.First().x, right_projectors_adj.First().y, right_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, right_emission.x, right_emission.y, right_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        if(left_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, left_projectors_adj.First().x, left_projectors_adj.First().y, left_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, left_emission.x, left_emission.y, left_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_left, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if(right_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, right_projectors_adj.First().x, right_projectors_adj.First().y, right_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, right_emission.x, right_emission.y, right_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_right, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        if(left_projectors_adj.Count > 0)
        //                        {
        //                            int proj_location = TallFaces.voxelToPixelHugeW(0, 0, left_projectors_adj.First().x, left_projectors_adj.First().y, left_projectors_adj.First().z, 0, stride, 0, still) / 4,
        //                                emit_location = TallFaces.voxelToPixelHugeW(0, 0, left_emission.x, left_emission.y, left_emission.z, 0, stride, 0, still) / 4;

        //                            g.DrawImage(b_left, 60 + (((proj_location % (stride / 4) - 32) / 2) - ((emit_location % (stride / 4) - 32) / 2)),
        //                                 160 + (((proj_location / (stride / 4) - (108 - 30)) / 2) - ((emit_location / (stride / 4) - (108 - 30)) / 2)), widthLarge, heightLarge);
        //                        }
        //                        g.DrawImage(b, 80, 160);
        //                    }
        //                    b_base.Save("frames/palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                        ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile)
        //                        + "_Huge_face" + dir + "_" + f + ".png");

        //                }
        //            }

        //            s = "";
        //            for(int dir = 0; dir < 4; dir++)
        //            {
        //                for(int f = 0; f < 12; f++)
        //                {
        //                    s += "frames/palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                    ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile) +
        //                    "_Huge_face" + dir + "_" + f + ".png ";
        //                }
        //            }
        //            startInfo.Arguments = "-dispose background -delay 11 -loop 0 " + s + " gifs/" + altFolder + "palette" + palette + "_" + moniker + "_" + firing_name + "_" +
        //                        ((firing_name == "Firing_Left") ? left_projectile : (firing_name == "Firing_Right") ? right_projectile : left_projectile + "_" + right_projectile) + "_Huge_animated.gif";
        //            Process.Start(startInfo).WaitForExit();

        //            //bin.Close();

        //            //            processFiringDouble(u);

        //            //processFieryExplosionDoubleW(moniker, work.ToList(), palette);

        //        }
        //    }
        //}

        public static void processUnitLargeWModel(string moniker, bool still, int palette, Model model, Pose[] poses, float[][] frames, bool alt = false)
        {
            Console.WriteLine("Processing: " + moniker + ", palette " + palette);
            string folder = (altFolder);
            Directory.CreateDirectory(folder);
            Model[] modelFrames = (frames != null && frames.Length > 0) ? new Model[frames.Length] : new Model[] { model };

            int framelimit = modelFrames.Length;

            if(poses != null && poses.Length > 0 && frames != null && frames.Length > 0 && frames[0] != null && frames[0].Length == 3)
            {
                Model[] posed = poses.Select(p => p(model.Replicate())).ToArray();
                for(int i = 0; i < modelFrames.Length; i++)
                {
                    modelFrames[i] = posed[(int)(frames[i][0])].Interpolate(posed[(int)(frames[i][1])], frames[i][2]).Translate(10 * multiplier, 10 * multiplier, 0, (alt) ? "Left_Lower_Leg" : "Left_Leg");
                }
            }

            //Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_" + palette + ".vox", work, "W", palette, 40, 40, 40);

            //MagicaVoxelData[] explodeParsed = parsed.Replicate();
            for(int f = 0; f < framelimit; f++)
            {
                byte[,,] work = modelFrames[f].Finalize();
                for(int dir = 0; dir < 4; dir++)
                {
                    //                FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
                    byte[,,] colors = TransformLogic.SealGaps(TransformLogic.RunCA(TransformLogic.RotateYaw(work, 90 * dir), 1 + multiplier * bonus / 2));

                    byte[][] b = processFrameLargeW(colors, palette, f, framelimit, still, false);

                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_Ortho_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            List<string> imageNames = new List<string>(4 * 8 * framelimit);
            for(int p = 0; p < 8; p++)
            {
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < framelimit; f++)
                    {
                        imageNames.Add(folder + "/palette" + palette + "_" + moniker + "_Ortho_face" + dir + "_" + f + ".png");
                    }
                }
            }
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running GIF conversion ...");
            WriteGIF(imageNames, 9, "gifs/" + altFolder + "palette" + palette + "_" + moniker + "_Ortho");

        }



        private static int voxelToPixelLargeW(int innerX, int innerY, int x, int y, int z, int current_color, int cols, int jitter, bool still)
        {
            return ((y) * vwidth + 1)
                + innerX +
                cols * (60 * multiplier * (vheight - 1) + x * 1 - z * (vheight - 1) + innerY);
        }
        private static int voxelToPixelHugeW(int innerX, int innerY, int x, int y, int z, int current_color, int cols, int jitter, bool still)
        {
            return ((y) * vwidth + 1)
                + innerX +
                cols * (120 * multiplier * (vheight - 1) + x * 1 - z * (vheight - 1) + innerY);
        }
        private static int voxelToPixelMassiveW(int innerX, int innerY, int x, int y, int z, int current_color, int cols, int jitter, bool still)
        {
            return ((y) * vwidth + 1)
                + innerX +
                cols * (160 * multiplier * (vheight - 1) + x * 1 - z * (vheight - 1) + innerY);
        }

        private static int voxelToPixelGenericW(int innerX, int innerY, int x, int y, int z, int current_color, int cols, int jitter, bool still, int xdim, int zdim)
        {
            return ((y) * vwidth + 1 + ((VoxelLogic.wcolors[current_color][3] == VoxelLogic.waver_alpha) ? jitter - 1 : 0))
                + innerX +
                cols * ((zdim) * multiplier * (vheight - top) + x /* / 2 */ - z * (vheight - top) + innerY + ((still || VoxelLogic.wcolors[current_color][3] == VoxelLogic.flat_alpha || current_color == 27) ? 0 : jitter - 1));
        }

        private static byte Shade(byte[] sprite, int innerX, int innerY, int aboveback, int above, int abovefront, int belowFront)
        {
            //            switch((((7 * innerX) * (3 * innerY) + x + y + z) ^ ((11 * innerX) * (5 * innerY) + x + y + z) ^ (7 - innerX - innerY)) % 16)
            if(above > 0 && belowFront == 0)
                return sprite[2];
            else if(aboveback > 0 || abovefront > 0)
                return sprite[1];
            else if(belowFront > 0)
                return sprite[0];
            else return sprite[2];
        }
        private static byte RandomDither(byte[] sprite, int innerX, int innerY, Random rng)
        {
            switch((rng.Next(2) + 1) * rng.Next(9) + rng.Next(5))
            {
                case 0:
                case 3:
                case 5:
                case 7:
                case 11:
                case 13:
                case 17:
                case 20:
                    return sprite[innerY * 4];
                case 1:
                case 12:
                case 18:
                    return sprite[innerY * 4 + 3];
                case 2:
                case 9:
                case 14:
                case 16:
                    return sprite[innerY * 4 + 2];
                default:
                    return sprite[innerY * 4 + 1];
            }

        }
        public static PRNG rng = new PRNG(0x1337BEEF);

        private static byte[][] renderGenericW(byte[,,] colors, int palette, int frame, int maxFrames, bool still, bool shadowless, int xDim, int yDim, int zDim)
        {
            rng.Reseed(0xb335u + ((uint)frame >> 1));
            bool useColorIndices = !VoxelLogic.terrainPalettes.Contains(palette);

            //int rows = xDim * multiplier * (vheight - top) * 2 + 2, cols = yDim * multiplier * vwidth + 2;
            int rows = (xDim + zDim) * multiplier * (vheight - top) + 2, cols = yDim * multiplier * vwidth + 2;
            byte[][] data = new byte[rows][];
            for(int i = 0; i < rows; i++)
                data[i] = new byte[cols];
            
            int numBytes = rows * cols;
            byte[] argbValues = new byte[numBytes];
            byte[] shadowValues = new byte[numBytes];
            byte[] outlineValues = new byte[numBytes];

            byte[] editValues = new byte[numBytes];

            bool[] barePositions = new bool[numBytes];
            int xSize = xDim * multiplier, ySize = yDim * multiplier, zSize = zDim * multiplier;
            int[] zbuffer = new int[numBytes];
            zbuffer.Fill<int>(-999);
            int[] xbuffer = new int[numBytes];
            xbuffer.Fill<int>(-999);

            int jitter = (((frame % 4) % 3) + ((frame % 4) / 3));
            //if(maxFrames >= 8) jitter = ((frame % 8 > 4) ? 4 - ((frame % 8) ^ 4) : frame % 8);
            bool[,] taken = new bool[xSize, ySize];
            for(int fz = zSize - 1; fz >= 0; fz--)
            {
                for(int fx = xSize - 1; fx >= 0; fx--)
                {
                    for(int fy = 0; fy < ySize; fy++)
                    {
                        if(colors[fx, fy, fz] == 0) continue;
                        byte vx = colors[fx, fy, fz];
                        int current_color = ((255 - vx) % 4 == 0) ? (255 - vx) / 4 + VoxelLogic.wcolorcount : ((254 - vx) % 4 == 0) ? (253 - VoxelLogic.clear) / 4 : (253 - vx) / 4;
                        int p = 0;
                        if(useColorIndices)
                        {
                            if((255 - vx) % 4 != 0 && current_color >= VoxelLogic.wcolorcount)
                                continue;

                            if(current_color >= 21 && current_color <= 24)
                                current_color = 21 + ((current_color + frame) % 4);

                            if(current_color >= VoxelLogic.wcolorcount && current_color < VoxelLogic.wcolorcount + 4)
                                current_color = VoxelLogic.wcolorcount + ((current_color + frame) % 4);
                            if(current_color >= VoxelLogic.wcolorcount + 6 && current_color < VoxelLogic.wcolorcount + 10)
                                current_color = VoxelLogic.wcolorcount + 6 + ((current_color + frame) % 4);
                            if(current_color >= VoxelLogic.wcolorcount + 14 && current_color < VoxelLogic.wcolorcount + 18)
                                current_color = VoxelLogic.wcolorcount + 14 + ((current_color + frame) % 4);
                        }
                        if((frame % 2 != 0) && (VoxelLogic.wcolors[current_color][3] == VoxelLogic.spin_alpha_0 || VoxelLogic.wcolors[current_color][3] == VoxelLogic.flash_alpha_0))
                            continue;
                        else if((frame % 2 != 1) && (VoxelLogic.wcolors[current_color][3] == VoxelLogic.spin_alpha_1 || VoxelLogic.wcolors[current_color][3] == VoxelLogic.flash_alpha_1))
                            continue;
                        else if(VoxelLogic.wcolors[current_color][3] == 0F)
                            continue;
                        else if(useColorIndices && current_color >= 17 && current_color <= 20)
                        {
                            int mod_color = current_color;
                            if(mod_color == 17 && r.Next(7) < 3) //smoke
                                continue;
                            if(current_color == 18) //yellow fire
                            {
                                if(r.Next(3) > 0)
                                {
                                    mod_color += r.Next(3);
                                }
                            }
                            else if(current_color == 19) // orange fire
                            {
                                if(r.Next(5) < 4)
                                {
                                    mod_color -= Math.Min(r.Next(3), r.Next(3));
                                }
                            }
                            else if(current_color == 20) // sparks
                            {
                                if(r.Next(5) > 0)
                                {
                                    mod_color -= r.Next(3);
                                }
                            }
                            
                            for(int j = 0; j < vheight; j++)
                            {
                                for(int i = 0; i < vwidth; i++)
                                {
                                    p = voxelToPixelGenericW(i, j, fx, fy, fz, mod_color, cols, jitter, still, xDim, zDim);
                                    if(argbValues[p] == 0)
                                    {
                                        if(wditheredcurrent[mod_color][i * 4 + j * vwidth * 4] != 0)
                                        {
                                            barePositions[p] = !(VoxelLogic.wcolors[current_color][3] == VoxelLogic.bordered_alpha || VoxelLogic.wcolors[current_color][3] == VoxelLogic.bordered_flat_alpha);
                                            if(VoxelLogic.wcolors[current_color][3] == VoxelLogic.bordered_alpha || VoxelLogic.wcolors[current_color][3] == VoxelLogic.bordered_flat_alpha)
                                            {
                                                zbuffer[p] = fz;
                                                xbuffer[p] = fx;
                                            }
                                            if(fz == zSize - 1)
                                                argbValues[p] = Shade(wditheredcurrent[mod_color], i, j,
                                                    0, 0, 0, fz == 0 || fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz - 1]);
                                            else
                                                argbValues[p] = Shade(wditheredcurrent[mod_color], i, j,
                                                    fx == 0 ? 0 : colors[fx - 1, fy, fz + 1],
                                                    colors[fx, fy, fz + 1],
                                                    fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz + 1],
                                                    fz == 0 || fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz - 1]);

                                        }

                                        if(!barePositions[p] && outlineValues[p] == 0)
                                            outlineValues[p] = wditheredcurrent[mod_color][12 * vwidth];

                                    }
                                }
                            }
                        }
                        else if(useColorIndices && current_color == 25)
                        {
                            taken[fx, fy] = true;
                            for(int j = 0; j < vheight; j++)
                            {
                                for(int i = 0; i < vwidth; i++)
                                {
                                    p = voxelToPixelGenericW(i, j, fx, fy, fz, current_color, cols, jitter, still, xDim, zDim);

                                    if(shadowValues[p] == 0)
                                    {
                                        shadowValues[p] = wditheredcurrent[current_color][i * 4 + j * vwidth * 4];
                                    }
                                }
                            }
                        }
                        else
                        {
                            int mod_color = current_color;
                            //                            if((mod_color == 27 || mod_color == VoxelLogic.wcolorcount + 4) && r.Next(7) < 2) //water
                            //                              continue;
                            if(useColorIndices && ((VoxelLogic.VisualMode != "CU" && mod_color == 40) || mod_color == VoxelLogic.wcolorcount + 5 || mod_color == VoxelLogic.wcolorcount + 20)) //rare sparks
                            {
                                if(r.Next(11) < 8) continue;
                            }
                            else
                                taken[fx, fy] = true;

                            for(int j = 0; j < vheight; j++)
                            {
                                for(int i = 0; i < vwidth; i++)
                                {
                                    p = voxelToPixelGenericW(i, j, fx, fy, fz, mod_color, cols, jitter, still, xDim, zDim);

                                    if(argbValues[p] == 0)
                                    {

                                        if(wditheredcurrent[mod_color][i * 4 + j * vwidth * 4] != 0)
                                        {
                                            // FIGURE THIS OUT LATER
                                            /*
                                            if(VoxelLogic.wcolors[mod_color][3] == VoxelLogic.gloss_alpha && r.Next(12) == 0)
                                            {
                                                argbValues[p - 3] = (byte)Math.Min(wcurrent[mod_color][sp][i - 3 + j * 16] + 160, 255);
                                                argbValues[p - 2] = (byte)Math.Min(wcurrent[mod_color][sp][i - 2 + j * 16] + 160, 255);
                                                argbValues[p - 1] = (byte)Math.Min(wcurrent[mod_color][sp][i - 1 + j * 16] + 160, 255);
                                                argbValues[p - 0] = wcurrent[mod_color][sp][i + j * 16];
                                            }
                                            else if(VoxelLogic.wcolors[mod_color][3] == VoxelLogic.grain_hard_alpha)
                                            {
                                                float n = Simplex.FindNoiseBold(facing, vx.x + 50, vx.y + 50, vx.z);
                                                argbValues[p - 3] = (byte)Math.Min(wcurrent[mod_color][sp][i - 3 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 2] = (byte)Math.Min(wcurrent[mod_color][sp][i - 2 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 1] = (byte)Math.Min(wcurrent[mod_color][sp][i - 1 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 0] = wcurrent[mod_color][sp][i + j * 16];
                                            }
                                            else if(VoxelLogic.wcolors[mod_color][3] == VoxelLogic.grain_some_alpha)
                                            {
                                                float n = Simplex.FindNoise(facing, vx.x + 50, vx.y + 50, vx.z);
                                                argbValues[p - 3] = (byte)Math.Min(wcurrent[mod_color][sp][i - 3 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 2] = (byte)Math.Min(wcurrent[mod_color][sp][i - 2 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 1] = (byte)Math.Min(wcurrent[mod_color][sp][i - 1 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 0] = wcurrent[mod_color][sp][i + j * 16];
                                            }
                                            else if(VoxelLogic.wcolors[mod_color][3] == VoxelLogic.grain_mild_alpha)
                                            {
                                                float n = Simplex.FindNoiseLight(facing, vx.x + 50, vx.y + 50, vx.z);
                                                argbValues[p - 3] = (byte)Math.Min(wcurrent[mod_color][sp][i - 3 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 2] = (byte)Math.Min(wcurrent[mod_color][sp][i - 2 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 1] = (byte)Math.Min(wcurrent[mod_color][sp][i - 1 + j * 16] * n + 16 * (n - 0.8), 255);
                                                argbValues[p - 0] = wcurrent[mod_color][sp][i + j * 16];
                                            }
                                            else if(VoxelLogic.wcolors[mod_color][3] == VoxelLogic.fuzz_alpha)
                                            {
                                                float n = Simplex.FindNoiseTight(frame % 4, facing, vx.x + 50, vx.y + 50, vx.z) + 0.3f;
                                                argbValues[p - 3] = (byte)VoxelLogic.Clamp(wcurrent[mod_color][sp][i - 3 + j * 16] * n + 24 * (n - 0.8), 1, 255);
                                                argbValues[p - 2] = (byte)VoxelLogic.Clamp(wcurrent[mod_color][sp][i - 2 + j * 16] * n + 24 * (n - 0.8), 1, 255);
                                                argbValues[p - 1] = (byte)VoxelLogic.Clamp(wcurrent[mod_color][sp][i - 1 + j * 16] * n + 24 * (n - 0.8), 1, 255);
                                                argbValues[p - 0] = wcurrent[mod_color][sp][i + j * 16];
                                            }
                                            else
                                            {*/
                                            if(useColorIndices && (mod_color == 27 || (VoxelLogic.VisualMode == "CU" && mod_color == 40))) //water or grass
                                                argbValues[p] = RandomDither(wditheredcurrent[mod_color], i, j, rng);
                                            else
                                            {
                                                if(fz == zSize - 1)
                                                    argbValues[p] = Shade(wditheredcurrent[mod_color], i, j,
                                                        0, 0, 0, fz == 0 || fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz - 1]);
                                                else
                                                    argbValues[p] = Shade(wditheredcurrent[mod_color], i, j,
                                                        fx == 0 ? 0 : colors[fx - 1, fy, fz + 1],
                                                        colors[fx, fy, fz + 1],
                                                        fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz + 1],
                                                        fz == 0 || fx == xSize - 1 ? 0 : colors[fx + 1, fy, fz - 1]);
                                            }

                                            //}


                                            zbuffer[p] = fz;
                                            xbuffer[p] = fx;


                                            barePositions[p] = (VoxelLogic.wcolors[mod_color][3] == VoxelLogic.flash_alpha || VoxelLogic.wcolors[mod_color][3] == VoxelLogic.flash_alpha_0 ||
                                                VoxelLogic.wcolors[mod_color][3] == VoxelLogic.flash_alpha_1 || VoxelLogic.wcolors[mod_color][3] == VoxelLogic.borderless_alpha ||
                                                VoxelLogic.wcolors[mod_color][3] == VoxelLogic.flat_alpha);

                                            if(!barePositions[p] && outlineValues[p] == 0)
                                                outlineValues[p] = wditheredcurrent[mod_color][12 * vwidth];      //(argbValues[p] * 1.2 + 2 < 255) ? (byte)(argbValues[p] * 1.2 + 2) : (byte)255;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }

            int[] xmods = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 }, ymods = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 };
            bool[,] nextTaken = new bool[xSize, ySize];
            for(int iter = 0; iter < 4; iter++)
            {
                for(int x = 1; x < xSize - 1; x++)
                {
                    for(int y = 1; y < ySize - 1; y++)
                    {
                        int ctr = 0;
                        for(int m = 0; m < 9; m++)
                        {
                            if(taken[x + xmods[m], y + ymods[m]])
                                ctr++;
                        }
                        if(ctr >= 5 || (taken[x,y] && ctr >= 3))
                            nextTaken[x, y] = true;

                    }
                }
                taken = nextTaken.Replicate();
            }
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    if(taken[x, y])
                    {
                        int p = 0;

                        for(int j = 0; j < vheight; j++)
                        {
                            for(int i = 0; i < vwidth; i++)
                            {
                                p = voxelToPixelGenericW(i, j, x, y, 0, 25, cols, jitter, still, xDim, zDim);

                                if(shadowValues[p] == 0)
                                {
                                    shadowValues[p] = wditheredcurrent[25][i * 4 + j * vwidth * 4];
                                }
                            }
                        }
                    }
                }
            }
            int ii;
            bool lightOutline = true; //!VoxelLogic.subtlePalettes.Contains(palette);
            for(int i = 0; i < numBytes; i++)
            {
                if(argbValues[i] > 0 && barePositions[i] == false)
                {
                    /*
                    if (i - 4 >= 0 && i - 4 < argbValues.Length && argbValues[i - 4] == 0 && lightOutline) { editValues[i - 4] = 255; editValues[i - 4 - 1] = 0; editValues[i - 4 - 2] = 0; editValues[i - 4 - 3] = 0; blacken = true; } else if (i - 4 >= 0 && i - 4 < argbValues.Length && barePositions[i - 4] == false && zbuffer[i] - 5 > zbuffer[i - 4]) { editValues[i - 4] = 255; editValues[i - 4 - 1] = outlineValues[i - 1]; editValues[i - 4 - 2] = outlineValues[i - 2]; editValues[i - 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (i + 4 >= 0 && i + 4 < argbValues.Length && argbValues[i + 4] == 0 && lightOutline) { editValues[i + 4] = 255; editValues[i + 4 - 1] = 0; editValues[i + 4 - 2] = 0; editValues[i + 4 - 3] = 0; blacken = true; } else if (i + 4 >= 0 && i + 4 < argbValues.Length && barePositions[i + 4] == false && zbuffer[i] - 5 > zbuffer[i + 4]) { editValues[i + 4] = 255; editValues[i + 4 - 1] = outlineValues[i - 1]; editValues[i + 4 - 2] = outlineValues[i - 2]; editValues[i + 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (i - bmpData.Stride >= 0 && i - bmpData.Stride < argbValues.Length && argbValues[i - bmpData.Stride] == 0 && lightOutline) { editValues[i - bmpData.Stride] = 255; editValues[i - bmpData.Stride - 1] = 0; editValues[i - bmpData.Stride - 2] = 0; editValues[i - bmpData.Stride - 3] = 0; blacken = true; } else if (i - bmpData.Stride >= 0 && i - bmpData.Stride < argbValues.Length && barePositions[i - bmpData.Stride] == false && zbuffer[i] - 5 > zbuffer[i - bmpData.Stride]) { editValues[i - bmpData.Stride] = 255; editValues[i - bmpData.Stride - 1] = outlineValues[i - 1]; editValues[i - bmpData.Stride - 2] = outlineValues[i - 2]; editValues[i - bmpData.Stride - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (i + bmpData.Stride >= 0 && i + bmpData.Stride < argbValues.Length && argbValues[i + bmpData.Stride] == 0 && lightOutline) { editValues[i + bmpData.Stride] = 255; editValues[i + bmpData.Stride - 1] = 0; editValues[i + bmpData.Stride - 2] = 0; editValues[i + bmpData.Stride - 3] = 0; blacken = true; } else if (i + bmpData.Stride >= 0 && i + bmpData.Stride < argbValues.Length && barePositions[i + bmpData.Stride] == false && zbuffer[i] - 5 > zbuffer[i + bmpData.Stride]) { editValues[i + bmpData.Stride] = 255; editValues[i + bmpData.Stride - 1] = outlineValues[i - 1]; editValues[i + bmpData.Stride - 2] = outlineValues[i - 2]; editValues[i + bmpData.Stride - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    */

                    for(int x = -1; x <= 1; x++)
                    {
                        for(int y = -1; y <= 1; y++)
                        {
                            if((x | y) == 0/* || (x * x * y * y) > 1*/) continue;
                            ii = i + x + cols * y;
                            if((ii >= 0 && ii < argbValues.Length) && ((argbValues[ii] == 0 && lightOutline) || (barePositions[ii] == false && x >= 0 && (zbuffer[i] - vheight + top - 5 + y > zbuffer[ii] || xbuffer[i] - vwidth > xbuffer[ii]))))
                            {
                                editValues[ii] = outlineValues[i];
                            }
                        }
                    }

                    /*
                    if((i - 1 >= 0 && i - 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i - 1] == 0 && lightOutline) || (barePositions[i - 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i - 1] || xbuffer[i] - vwidth - 1 > xbuffer[i - 1])))) { editValues[i - 1] = outlineValues[i]; }
                    if((i + 1 >= 0 && i + 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i + 1] == 0 && lightOutline) || (barePositions[i + 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i + 1] || xbuffer[i] - vwidth - 1 > xbuffer[i + 1])))) { editValues[i + 1] = outlineValues[i]; }
                    if((i - cols >= 0 && i - cols < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i - cols] == 0 && lightOutline) || (barePositions[i - cols] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i - cols] || xbuffer[i] - vwidth - 1 > xbuffer[i - cols])))) { editValues[i - cols] = outlineValues[i]; }
                    if((i + cols >= 0 && i + cols < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i + cols] == 0 && lightOutline) || (barePositions[i + cols] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i + cols] || xbuffer[i] - vwidth - 0 > xbuffer[i + cols])))) { editValues[i + cols] = outlineValues[i]; }
                    */
                    /*
                    if(!useColorIndices) // terrain
                    {
                        if((i - cols - 1 >= 0 && i - cols - 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i - cols - 1] == 0 && lightOutline) || (barePositions[i - cols - 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i - cols - 1] || xbuffer[i] - vwidth - 1 > xbuffer[i - cols - 1])))) { editValues[i - cols - 1] = outlineValues[i]; }
                        if((i - cols + 1 >= 0 && i - cols + 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i - cols + 1] == 0 && lightOutline) || (barePositions[i - cols + 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i - cols + 1] || xbuffer[i] - vwidth - 1 > xbuffer[i - cols + 1])))) { editValues[i - cols + 1] = outlineValues[i]; }
                        if((i + cols - 1 >= 0 && i + cols - 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i + cols - 1] == 0 && lightOutline) || (barePositions[i + cols - 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i + cols - 1] || xbuffer[i] - vwidth - 0 > xbuffer[i + cols - 1])))) { editValues[i + cols - 1] = outlineValues[i]; }
                        if((i + cols + 1 >= 0 && i + cols + 1 < argbValues.Length) && ((argbValues[i] > 0 && argbValues[i + cols + 1] == 0 && lightOutline) || (barePositions[i + cols + 1] == false && (zbuffer[i] - vheight + top - 2 > zbuffer[i + cols + 1] || xbuffer[i] - vwidth - 0 > xbuffer[i + cols + 1])))) { editValues[i + cols + 1] = outlineValues[i]; }
                    }
                    */

                    /*
                    if (argbValues[i] > 0 && i + 4 >= 0 && i + 4 < argbValues.Length && argbValues[i + 4] == 0 && lightOutline) { argbValues[i - 4] = 255; argbValues[i - 4 - 1] = 0; argbValues[i - 4 - 2] = 0; argbValues[i - 4 - 3] = 0; blacken = true; } else if (i + 4 >= 0 && i + 4 < argbValues.Length && barePositions[i + 4] == false && zbuffer[i] - 2 > zbuffer[i + 4]) { argbValues[i - 4] = 255; argbValues[i - 4 - 1] = outlineValues[i - 1]; argbValues[i - 4 - 2] = outlineValues[i - 2]; argbValues[i - 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i - 4 >= 0 && i - 4 < argbValues.Length && argbValues[i - 4] == 0 && lightOutline) { argbValues[i + 4] = 255; argbValues[i + 4 - 1] = 0; argbValues[i + 4 - 2] = 0; argbValues[i + 4 - 3] = 0; blacken = true; } else if (i - 4 >= 0 && i - 4 < argbValues.Length && barePositions[i - 4] == false && zbuffer[i] - 2 > zbuffer[i - 4]) { argbValues[i + 4] = 255; argbValues[i + 4 - 1] = outlineValues[i - 1]; argbValues[i + 4 - 2] = outlineValues[i - 2]; argbValues[i + 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i + bmpData.Stride >= 0 && i + bmpData.Stride < argbValues.Length && argbValues[i + bmpData.Stride] == 0 && lightOutline) { argbValues[i - bmpData.Stride] = 255; argbValues[i - bmpData.Stride - 1] = 0; argbValues[i - bmpData.Stride - 2] = 0; argbValues[i - bmpData.Stride - 3] = 0; blacken = true; } else if (i + bmpData.Stride >= 0 && i + bmpData.Stride < argbValues.Length && barePositions[i + bmpData.Stride] == false && zbuffer[i] - 2 > zbuffer[i + bmpData.Stride]) { argbValues[i - bmpData.Stride] = 255; argbValues[i - bmpData.Stride - 1] = outlineValues[i - 1]; argbValues[i - bmpData.Stride - 2] = outlineValues[i - 2]; argbValues[i - bmpData.Stride - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i - bmpData.Stride >= 0 && i - bmpData.Stride < argbValues.Length && argbValues[i - bmpData.Stride] == 0 && lightOutline) { argbValues[i + bmpData.Stride] = 255; argbValues[i + bmpData.Stride - 1] = 0; argbValues[i + bmpData.Stride - 2] = 0; argbValues[i + bmpData.Stride - 3] = 0; blacken = true; } else if (i - bmpData.Stride >= 0 && i - bmpData.Stride < argbValues.Length && barePositions[i - bmpData.Stride] == false && zbuffer[i] - 2 > zbuffer[i - bmpData.Stride]) { argbValues[i + bmpData.Stride] = 255; argbValues[i + bmpData.Stride - 1] = outlineValues[i - 1]; argbValues[i + bmpData.Stride - 2] = outlineValues[i - 2]; argbValues[i + bmpData.Stride - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    */
                    /*
                    if (argbValues[i] > 0 && i + bmpData.Stride + 4 >= 0 && i + bmpData.Stride + 4 < argbValues.Length && argbValues[i + bmpData.Stride + 4] == 0 && lightOutline) { argbValues[i - bmpData.Stride - 4] = 255; argbValues[i - bmpData.Stride - 4 - 1] = 0; argbValues[i - bmpData.Stride - 4 - 2] = 0; argbValues[i - bmpData.Stride - 4 - 3] = 0; blacken = true; } else if (i + bmpData.Stride + 4 >= 0 && i + bmpData.Stride + 4 < argbValues.Length && barePositions[i + bmpData.Stride + 4] == false && zbuffer[i] - 2 > zbuffer[i + bmpData.Stride + 4]) { argbValues[i - bmpData.Stride - 4] = 255; argbValues[i - bmpData.Stride - 4 - 1] = outlineValues[i - 1]; argbValues[i - bmpData.Stride - 4 - 2] = outlineValues[i - 2]; argbValues[i - bmpData.Stride - 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i - bmpData.Stride - 4 >= 0 && i - bmpData.Stride - 4 < argbValues.Length && argbValues[i - bmpData.Stride - 4] == 0 && lightOutline) { argbValues[i + bmpData.Stride + 4] = 255; argbValues[i + bmpData.Stride + 4 - 1] = 0; argbValues[i + bmpData.Stride + 4 - 2] = 0; argbValues[i + bmpData.Stride + 4 - 3] = 0; blacken = true; } else if (i - bmpData.Stride - 4 >= 0 && i - bmpData.Stride - 4 < argbValues.Length && barePositions[i - bmpData.Stride - 4] == false && zbuffer[i] - 2 > zbuffer[i - bmpData.Stride - 4]) { argbValues[i + bmpData.Stride + 4] = 255; argbValues[i + bmpData.Stride + 4 - 1] = outlineValues[i - 1]; argbValues[i + bmpData.Stride + 4 - 2] = outlineValues[i - 2]; argbValues[i + bmpData.Stride + 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i + bmpData.Stride - 4 >= 0 && i + bmpData.Stride - 4 < argbValues.Length && argbValues[i + bmpData.Stride - 4] == 0 && lightOutline) { argbValues[i - bmpData.Stride + 4] = 255; argbValues[i - bmpData.Stride + 4 - 1] = 0; argbValues[i - bmpData.Stride + 4 - 2] = 0; argbValues[i - bmpData.Stride + 4 - 3] = 0; blacken = true; } else if (i + bmpData.Stride - 4 >= 0 && i + bmpData.Stride - 4 < argbValues.Length && barePositions[i + bmpData.Stride - 4] == false && zbuffer[i] - 2 > zbuffer[i + bmpData.Stride - 4]) { argbValues[i - bmpData.Stride + 4] = 255; argbValues[i - bmpData.Stride + 4 - 1] = outlineValues[i - 1]; argbValues[i - bmpData.Stride + 4 - 2] = outlineValues[i - 2]; argbValues[i - bmpData.Stride + 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    if (argbValues[i] > 0 && i - bmpData.Stride + 4 >= 0 && i - bmpData.Stride + 4 < argbValues.Length && argbValues[i - bmpData.Stride + 4] == 0 && lightOutline) { argbValues[i + bmpData.Stride - 4] = 255; argbValues[i + bmpData.Stride - 4 - 1] = 0; argbValues[i + bmpData.Stride - 4 - 2] = 0; argbValues[i + bmpData.Stride - 4 - 3] = 0; blacken = true; } else if (i - bmpData.Stride + 4 >= 0 && i - bmpData.Stride + 4 < argbValues.Length && barePositions[i - bmpData.Stride + 4] == false && zbuffer[i] - 2 > zbuffer[i - bmpData.Stride + 4]) { argbValues[i + bmpData.Stride - 4] = 255; argbValues[i + bmpData.Stride - 4 - 1] = outlineValues[i - 1]; argbValues[i + bmpData.Stride - 4 - 2] = outlineValues[i - 2]; argbValues[i + bmpData.Stride - 4 - 3] = outlineValues[i - 3]; if (!blacken) shade = true; }
                    */
                    /*
                    if(blacken)
                    {
                        editValues[i] = 255;
                    }
                    else if(shade) {
                    //    editValues[i] = outlineValues[i];
                    }
                    */
                }
            }

            for(int i = 0; i < numBytes; i++)
            {
                if(editValues[i] > 0)
                {
                    argbValues[i] = editValues[i];
                }
            }

            if(!shadowless)
            {
                for(int i = 0; i < numBytes; i++)
                {
                    if(argbValues[i] == 0 && shadowValues[i] > 0)
                    {
                        argbValues[i] = shadowValues[i];
                    }
                }
            }

            for(int i = 0; i < numBytes; i++)
            {
                data[i / cols][i % cols] = argbValues[i];
            }
            return data;
        }

        private static byte[][] renderWSmartSmall(byte[,,] colors, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            return renderGenericW(colors, palette, frame, maxFrames, still, shadowless, 60 / multiplier, 60 / multiplier, 60 / multiplier);
        }
        private static byte[][] renderWSmart(byte[,,] colors, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            return renderGenericW(colors, palette, frame, maxFrames, still, shadowless, 60, 60, 60);
        }
        private static byte[][] renderWSmartHuge(byte[,,] colors, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            return renderGenericW(colors, palette, frame, maxFrames, still, shadowless, 120, 120, 80);
        }
        private static byte[][] renderWSmartMassive(byte[,,] colors, int palette, int frame, int maxFrames, bool still, bool shadowless)
        {
            return renderGenericW(colors, palette, frame, maxFrames, still, shadowless, 160, 160, 120);
        }

        public static void renderTerrain()
        {
            string tmpVisual = VoxelLogic.VisualMode;
            VoxelLogic.VisualMode = "None";
            //byte[][][] tilings = new byte[12][][];

            BinaryReader bin = new BinaryReader(File.Open("CU3/Terrain_Huge_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxlist = VoxelLogic.FromMagicaRaw(bin);
            int terrainPalette = CURedux.wspecies.Length * 8;
            wcurrent = wrendered[terrainPalette];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette];
            wditheredcurrent = wdithered[terrainPalette];

            for(int i = 0; i < 15; i++)
            {
                Console.WriteLine("Palette " + i);
                byte[,,] colors = TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, m.color - 16 * i)).ToList(), 120, 120, 80);
                //Console.WriteLine(colors.ToList().Max());

                for(int shp = 0; shp < 16; shp++)
                {
                    byte[,,] c2 = colors.Replicate();
                    string nm = CURedux.Terrains[i];

                    if((shp & 8) != 0)
                    {
                        nm += "_S";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[80 + q, side, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 1) != 0)
                    {
                        nm += "_W";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[side, 39 - q, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 2) != 0)
                    {
                        nm += "_N";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[39 - q, side, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 4) != 0)
                    {
                        nm += "_E";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[side, 80 + q, 12 - z] = 0;
                                }
                            }
                        }
                    }

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    c2 = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.ScalePartial(c2, multiplier)), 1 + bonus * multiplier / 2);

                    PngWriter png = FileHelper.CreatePngWriter("CU_Ortho/Terrains2/" + nm + ".png", imi, true);
                    WritePNG(png, processFrameHugeW(c2, 8, 0, 1, true, true), simplepalettes[terrainPalette]);
                    PngWriter png2 = FileHelper.CreatePngWriter("CU_Ortho/TerrainsBlank/" + nm + ".png", imi, true);
                    WritePNG(png2, processFrameHugeW(c2, 8, 0, 1, true, true), basepalette);
                }
            }
            VoxelLogic.VisualMode = tmpVisual;

            //int[,] grid = new int[9, 17];

            //int[,] takenLocations = new int[9, 17];
            //for(int i = 0; i < 9; i++)
            //{
            //    for(int j = 0; j < 17; j++)
            //    {
            //        takenLocations[i, j] = 0;
            //        grid[i, j] = 0;
            //        if(i > 0 && i < 8 && j > 0 && j < 16 && r.Next(3) > 0)
            //        {
            //            grid[i, j] = r.Next(11);
            //        }
            //    }
            //}
            ///*
            //int numMountains = r.Next(17, 30);
            //int iter = 0;
            //int rx = r.Next(8) + 1, ry = r.Next(15) + 2;
            //do
            //{
            //    if(takenLocations[rx, ry] < 1 && r.Next(6) > 0 && ((ry + 1) / 2 != ry))
            //    {
            //        takenLocations[rx, ry] = 2;
            //        grid[rx, ry] = r.Next(9) + 1;
            //        int ydir = ((ry + 1) / 2 > ry) ? 1 : -1;
            //        int xdir = (ry % 2 == 0) ? rx + r.Next(2) : rx - r.Next(2);
            //        if(xdir <= 1) xdir++;
            //        if(xdir >= 9) xdir--;
            //        rx = xdir;
            //        ry = ry + ydir;

            //    }
            //    else
            //    {
            //        rx = r.Next(8) + 1;
            //        ry = r.Next(15) + 2;
            //    }
            //    iter++;
            //} while(iter < numMountains);

            //int extreme = 0;
            //switch(r.Next(5))
            //{
            //    case 0:
            //        extreme = 7;
            //        break;
            //    case 1:
            //        extreme = 2;
            //        break;
            //    case 2:
            //        extreme = 2;
            //        break;
            //    case 3:
            //        extreme = 1;
            //        break;
            //    case 4:
            //        extreme = 1;
            //        break;
            //}
            //*/
            ///*
            //for(int i = 1; i < 8; i++)
            //{
            //    for(int j = 2; j < 15; j++)
            //    {
            //        for(int v = 0; v < 3; v++)
            //        {

            //            int[] adj = { 0, 0, 0, 0,
            //                            0,0,0,0,
            //                        0, 0, 0, 0, };
            //            adj[0] = grid[i, j + 1];
            //            adj[1] = grid[i, j - 1];
            //            if(j % 2 == 0)
            //            {
            //                adj[2] = grid[i + 1, j + 1];
            //                adj[3] = grid[i + 1, j - 1];
            //            }
            //            else
            //            {
            //                adj[2] = grid[i - 1, j + 1];
            //                adj[3] = grid[i - 1, j - 1];
            //            }
            //            adj[4] = grid[i, j + 2];
            //            adj[5] = grid[i, j - 2];
            //            adj[6] = grid[i + 1, j];
            //            adj[7] = grid[i - 1, j];
            //            int likeliest = 0;
            //            if(!adj.Contains(1) && extreme == 2 && r.Next(5) > 1)
            //                likeliest = extreme;
            //            if((adj.Contains(2) && r.Next(4) == 0))
            //                likeliest = extreme;
            //            if(extreme == 7 && (r.Next(4) == 0) || (adj.Contains(7) && r.Next(3) > 0))
            //                likeliest = extreme;
            //            if((adj.Contains(1) && r.Next(5) > 1) || r.Next(4) == 0)
            //                likeliest = r.Next(2) * 2 + 1;
            //            if(adj.Contains(5) && r.Next(3) == 0)
            //                likeliest = r.Next(4, 6);
            //            if(r.Next(45) == 0)
            //                likeliest = 6;
            //            if(takenLocations[i, j] == 0)
            //            {
            //                grid[i, j] = likeliest;
            //            }
            //        }
            //    }
            //}
            //*/

            //for(int j = 0; j < 17; j++)
            //{
            //    for(int i = 0; i < 9; i++)
            //    {
            //        g.DrawImageUnscaled(tilings[grid[i, j]], (48 * 4 * i) - ((j % 2 == 0) ? 0 : 24 * 4) - 24 * 4 + 20, (12 * 4 * j) - 12 * 2 * 17 - 40);
            //    }
            //}
            //return b;
        }

        public static void renderTerrainDetailed(string name)
        {

            string tmpVisual = VoxelLogic.VisualMode;
            VoxelLogic.VisualMode = "None";

            BinaryReader bin = new BinaryReader(File.Open("CU3/Terrain2/" + name + "_Huge_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxlist = VoxelLogic.FromMagicaRaw(bin);
            int terrainPalette = CURedux.wspecies.Length * 8;
            //NORMAL WEATHER
            wcurrent = wrendered[terrainPalette];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette];
            wditheredcurrent = wdithered[terrainPalette];

            Console.WriteLine("Terrain: " + name);
            byte[,,] colors = TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, m.color)), 120, 120, 80);
            string folder = altFolder + "Terrains2/", blankFolder = altFolder + "TerrainsBlank2/";
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(blankFolder);

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c2 = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(colors, 90 * dir)), 1 + bonus * multiplier / 2);

                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Normal_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette]);
                PngWriter png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Normal_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);

                png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Dark_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette+4]);
                png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Dark_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);

                png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Bright_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette+5]);
                png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Bright_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);
            }
            

            //RAINY WEATHER
            wcurrent = wrendered[terrainPalette+1];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette+1];
            wditheredcurrent = wdithered[terrainPalette+1];

            colors = TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, (m.color != 0 && r.Next(15) <= 1) ? 253 - 37 * 4 : m.color)), 120, 120, 80);


            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c2 = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(colors, 90 * dir)), 1 + bonus * multiplier / 2);

                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Rain_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette+1]);
                PngWriter png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Rain_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);
            }

            //SNOWY WEATHER
            wcurrent = wrendered[terrainPalette+2];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette+2];
            wditheredcurrent = wdithered[terrainPalette+2];

            colors = TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, (m.color != 0 && r.Next(5) != 0) ? 253 - 31 * 4 : m.color)), 120, 120, 80);

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c2 = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(colors, 90 * dir)), 1 + bonus * multiplier / 2);

                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Snow_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette+2]);
                PngWriter png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Snow_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);
            }

            //TOXIC WEATHER
            wcurrent = wrendered[terrainPalette+3];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette+3];
            wditheredcurrent = wdithered[terrainPalette+3];

            colors = TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, (m.color != 0 && r.Next(20) <= 1) ? 253 - 52 * 4 : (r.Next(8) == 0) ? 0 : m.color)), 120, 120, 80);


            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c2 = TransformLogic.RunCA(TransformLogic.SealGaps(TransformLogic.RotateYaw(colors, 90 * dir)), 1 + bonus * multiplier / 2);

                ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(folder + name + "_Huge_face" + dir + "_Poison_0.png", imi, true);
                WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette+3]);
                PngWriter png2 = FileHelper.CreatePngWriter(blankFolder + name + "_Huge_face" + dir + "_Poison_0.png", imi, true);
                WritePNG(png2, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);
            }
            
            VoxelLogic.VisualMode = tmpVisual;
        }

        public static void renderTerrainSimple(string name, string kind, byte[] changers, byte standard, byte dark, byte highlight)
        {

            string tmpVisual = VoxelLogic.VisualMode;
            VoxelLogic.VisualMode = "None";
            string folder = altFolder + "Terrains/", bFolder = blankFolder + "Terrains/";
            BinaryReader bin = new BinaryReader(File.Open("CU3/Terrain3/" + (kind == "Ordered" ? kind : "Normal") + "_Huge_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxlist = VoxelLogic.FromMagicaRaw(bin);
            int terrainPalette = CURedux.wspecies.Length * 8 - 16 - (CURedux.ZOMBIES ? 0 : 8);

            //NORMAL WEATHER
            wcurrent = wrendered[terrainPalette];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette];
            wditheredcurrent = wdithered[terrainPalette];

            Console.WriteLine("Terrain: " + name);

            byte[,,] colors = TransformLogic.RunCA(
                    PatternLogic.ApplyTerrain(
                        TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 20, 20, 0, m.color)), 120, 120, 80), changers, standard, dark, highlight),
                    (kind == "Ordered" ? 1 : 2), true, false);

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c1 = TransformLogic.RotateYaw(colors, 90 * dir);
                for(int shp = 0; shp < 16; shp++)
                {
                    byte[,,] c2 = c1.Replicate();
                    string nm = name;

                    if((shp & 8) != 0)
                    {
                        nm += "_S";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[80 + q, side, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 1) != 0)
                    {
                        nm += "_W";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[side, 39 - q, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 2) != 0)
                    {
                        nm += "_N";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[39 - q, side, 12 - z] = 0;
                                }
                            }
                        }
                    }
                    if((shp & 4) != 0)
                    {
                        nm += "_E";
                        for(int side = 20; side < 100; side++)
                        {
                            for(int q = 0; q < 12; q++)
                            {
                                for(int z = 0; z <= q; z++)
                                {
                                    c2[side, 80 + q, 12 - z] = 0;
                                }
                            }
                        }
                    }

                    c2 = TransformLogic.SealGaps(c2);
                    //Directory.CreateDirectory("vox2");
                    //VoxelLogic.WriteVOX("vox2/" + nm + "_face" + dir + "_0.vox", c2, "W", terrainPalette);

                    ImageInfo imi = new ImageInfo(widthHuge, heightHuge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + nm + "_face" + dir + "_0.png", imi, true);
                    WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), simplepalettes[terrainPalette]);
                    png = FileHelper.CreatePngWriter(bFolder + nm + "_face" + dir + "_0.png", imi, true);
                    WritePNG(png, processFrameHugeW(c2, terrainPalette, 0, 1, true, true), basepalette);
                }
            }

            VoxelLogic.VisualMode = tmpVisual;
        }


        public static void renderSubTerrainSimple(string name, string kind, byte[] changers, byte standard, byte dark, byte highlight)
        {

            string tmpVisual = VoxelLogic.VisualMode;
            VoxelLogic.VisualMode = "None";
            string folder = altFolder + "SubTerrains/", bFolder = blankFolder + "SubTerrains/";
            BinaryReader bin = new BinaryReader(File.Open("CU3/Terrain3/" + (kind == "Ordered" ? kind : "Normal") + "_Sub_W.vox", FileMode.Open));
            List<MagicaVoxelData> voxlist = VoxelLogic.FromMagicaRaw(bin);
            int terrainPalette = CURedux.wspecies.Length * 8 - 16 - (CURedux.ZOMBIES ? 0 : 8);

            //NORMAL WEATHER
            wcurrent = wrendered[terrainPalette];

            VoxelLogic.wcolors = VoxelLogic.wpalettes[terrainPalette];
            wditheredcurrent = wdithered[terrainPalette];

            Console.WriteLine("Terrain: " + name);

            byte[,,] colors = TransformLogic.RunCA(
                    PatternLogic.ApplySubTerrain(
                        TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 10, 10, 0, m.color)), 60, 60, 60), changers, standard, dark, highlight),
                    (kind == "Ordered" ? 1 : 2), true, false);

            for(int dir = 0; dir < 4; dir++)
            {
                byte[,,] c2 = TransformLogic.RotateYaw(colors, 90 * dir);

                //c2 = TransformLogic.SealGaps(c2);

                ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(folder + name + "_face" + dir + "_0.png", imi, true);
                byte[][] bytes = processFrameSpecialW(c2, terrainPalette, 0, 1, true, true);
                WritePNG(png, bytes, simplepalettes[terrainPalette]);
                png = FileHelper.CreatePngWriter(bFolder + name + "_face" + dir + "_0.png", imi, true);
                WritePNG(png, bytes, basepalette);
            }
            if(kind == "Ordered")
            {
                VoxelLogic.VisualMode = tmpVisual;
                return;
            }
            foreach(string part in new string[] { "Half_On", "Half_Off", "Most_On", "Most_Off" })
            {
                bin = new BinaryReader(File.Open("CU3/Terrain3/Normal_" + part + "_Sub_W.vox", FileMode.Open));
                voxlist = VoxelLogic.FromMagicaRaw(bin);

                colors = TransformLogic.RunCA(
                        PatternLogic.ApplySubTerrain(
                            TransformLogic.VoxListToArray(voxlist.Select(m => VoxelLogic.AlterVoxel(m, 10, 10, 0, m.color)), 60, 60, 60), changers, standard, dark, highlight),
                        2, true, false);

                for(int dir = 0; dir < 4; dir++)
                {
                    byte[,,] c2 = TransformLogic.RotateYaw(colors, 90 * dir);

                    //c2 = TransformLogic.SealGaps(c2);

                    ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + name + "_" + part + "_face" + dir + "_0.png", imi, true);
                    byte[][] bytes = processFrameSpecialW(c2, terrainPalette, 0, 1, true, true);
                    WritePNG(png, bytes, simplepalettes[terrainPalette]);
                    png = FileHelper.CreatePngWriter(bFolder + name + "_" + part + "_face" + dir + "_0.png", imi, true);
                    WritePNG(png, bytes, basepalette);
                }
            }
            VoxelLogic.VisualMode = tmpVisual;
        }

        static void makeFlatTiling()
        {
            Bitmap[] tilings = new Bitmap[15];
            for(int i = 0; i < 15; i++)
            {
                tilings[i] = new Bitmap(altFolder + "Terrains2/" + CURedux.Terrains[i] + ".png");
            }
            Bitmap b = new Bitmap(64 * 20 + 1, 32 * 20 + 1);
            Graphics tiling = Graphics.FromImage(b);

            LocalMap lm = new LocalMap(20, 20, 1);
            for(int j = 0; j < 20; j++)
            {
                for(int i = 0; i < 20; i++)
                {
                    tiling.DrawImageUnscaled(tilings[lm.Land[i, j]], (64 * i) + 2 - 30, (32 * j) - 20 - 64 + 2);
                }
            }
            b.Save(altFolder + "tiling_flat.png", ImageFormat.Png);
        }

        static void makeDetailedTiling()
        {
            Bitmap[] tilings = new Bitmap[15];
            for(int i = 0; i < 11; i++)
            {
                tilings[i] = new Bitmap(altFolder + "Terrains2/" + CURedux.Terrains[i] + "_Huge_face0_Normal_0.png");
            }
            Bitmap b = new Bitmap(64 * 20 + 1, 32 * 20 + 1);
            Graphics tiling = Graphics.FromImage(b);

            LocalMap lm = new LocalMap(20, 20, 1);
            for(int j = 0; j < 20; j++)
            {
                for(int i = 0; i < 20; i++)
                {
                    if(lm.Land[i, j] < 11)
                        tiling.DrawImageUnscaled(tilings[lm.Land[i, j]], (64 * i) + 2 - 30, (32 * j) - 20 - 64 + 2);
                    else
                        tiling.DrawImageUnscaled(tilings[0], (64 * i) + 2 - 30, (32 * j) - 20 - 64 + 2);
                }
            }
            b.Save(altFolder + "tiling_detailed.png", ImageFormat.Png);
        }

        static void makeSimpleTiling()
        {
            for(int n = 0; n < 40; n++)
            {
                Bitmap[] tilings = new Bitmap[44];
                for(int i = 0; i < 44; i++)
                {
                    tilings[i] = new Bitmap(altFolder + "Terrains/" + CURedux.Terrains[i % 11] + "_face" + (i / 11) + "_0.png");
                }
                Bitmap b = new Bitmap(32 * 30, 32 * 30);
                Graphics tiling = Graphics.FromImage(b);

                LocalMap lm = new LocalMap(30, 30, 1);
                for(int j = 0; j < 30; j++)
                {
                    for(int i = 0; i < 30; i++)
                    {
                        if(lm.Land[i, j] < 11)
                            tiling.DrawImageUnscaled(tilings[lm.Land[i, j] + r.Next(4) * 11], (32 * i) + 1 - 15, (32 * j) - 18 - 32 + 1);
                        else
                            tiling.DrawImageUnscaled(tilings[r.Next(4) * 11], (32 * i) + 1 - 15, (32 * j) - 18 - 32 + 1);
                    }
                }
                b.Save(altFolder + "tiling"+ n + ".png", ImageFormat.Png);
            }
        }

        public static void processVoxGiantWModel(string moniker, bool still, int palette, Model model, Pose[] poses, float[][] frames, bool alt = false)
        {
            Console.WriteLine("Processing: " + moniker + ", palette " + palette);
            string folder = (altFolder + "vox/");
            Directory.CreateDirectory(folder);
            Model[] modelFrames = (frames != null && frames.Length > 0) ? new Model[frames.Length] : new Model[] { model };
            int framelimit = modelFrames.Length;

            if(poses != null && poses.Length > 0 && frames != null && frames.Length > 0 && frames[0] != null && frames[0].Length == 3)
            {
                Model[] posed = poses.Select(p => p(model.Replicate())).ToArray();
                for(int i = 0; i < modelFrames.Length; i++)
                {
                    byte[,,] colors = PatternLogic.ApplyPattern(
                            //TransformLogic.RunSurfaceCA(
                            TransformLogic.RunCA(
                                                FaceLogic.FaceArrayToByteArray(
                                                        //FaceLogic.DoubleSize(
                                                        FaceLogic.GetFaces(
                                                            TransformLogic.RunThinningCA(
                                                                    posed[(int)(frames[i][0])]
                                                                    .Interpolate(posed[(int)(frames[i][1])], frames[i][2])
                                                                    .Translate(10 * OrthoSingle.multiplier, 10 * OrthoSingle.multiplier, 0, (alt) ? "Left_Lower_Leg" : "Left_Leg")
                                                                    .Finalize(),
                                                                2)//1 + OrthoSingle.multiplier * OrthoSingle.bonus / 2)
                                                            )
                                                   //)
                                                   ),
                            2, true),
                            model.Patterns);
                    /*
                    byte[,,] colors = PatternLogic.ApplyPattern(
                        TransformLogic.RunCA(
                            FaceLogic.FaceArrayToByteArray(
                                FaceLogic.DoubleSize(
                                    FaceLogic.GetFaces(
                                                posed[(int)(frames[i][0])]
                                                .Interpolate(posed[(int)(frames[i][1])], frames[i][2])
                                                .Translate(10 * multiplier, 10 * multiplier, 0, (alt) ? "Left_Lower_Leg" : "Left_Leg")
                                                .Finalize()
                                        )
                                    )
                                ),
                            1 + OrthoSingle.multiplier * OrthoSingle.bonus), 
                        model.Patterns);
                    */
                    /*
                    byte[,,] colors = PatternLogic.ApplyPattern(
                                            TransformLogic.RunSurfaceCA(
                                                FaceLogic.FaceArrayToByteArray(
                                                    FaceLogic.DoubleSize(
                                                        FaceLogic.GetFaces(
                                                            TransformLogic.RunCA(
                                                                TransformLogic.Translate(
                                                                    posed[(int)(frames[i][0])]
                                                                    .Interpolate(posed[(int)(frames[i][1])], frames[i][2])
                                                                    .Translate(10 * multiplier, 10 * multiplier, 0, (alt) ? "Left_Lower_Leg" : "Left_Leg")
                                                                    .Finalize(),
                                                                    10, 10, 0),
                                                                1 + OrthoSingle.multiplier * OrthoSingle.bonus / 2)
                                                            )
                                                       )
                                                   ), 2, true),
                                            model.Patterns);
                    */
                    /*
                    TransformLogic.RunCA(
                        posed[(int)(frames[i][0])]
                        .Interpolate(posed[(int)(frames[i][1])], frames[i][2])
                        .Translate(10 * multiplier, 10 * multiplier, 0, (alt) ? "Left_Lower_Leg" : "Left_Leg")
                        .Finalize(),
                        1 + multiplier * bonus / 2);

                    */

                    VoxelLogic.WriteVOX(folder + moniker + "_palette" + palette + "_" + i + ".vox", colors, "W", palette);

                }
            }

        }


        public static void processUnitMassiveWModel(string moniker, bool still, int palette, Model model, Pose[] poses, float[][] frames)
        {
            Console.WriteLine("Processing: " + moniker + ", palette " + palette);
            string folder = (altFolder);
            Directory.CreateDirectory(folder);
            Model[] modelFrames = (frames != null && frames.Length > 0) ? new Model[frames.Length] : new Model[] { model };

            int framelimit = modelFrames.Length;

            if(poses != null && poses.Length > 0 && frames != null && frames.Length > 0 && frames[0] != null && frames[0].Length == 3)
            {
                Model[] posed = poses.Select(p => p(model.Replicate())).ToArray();
                for(int i = 0; i < modelFrames.Length; i++)
                {
                    modelFrames[i] = posed[(int)(frames[i][0])].Interpolate(posed[(int)(frames[i][1])], frames[i][2]).Translate(10 * 1, 10 * 1, 0, "Left_Leg");
                }
            }

            //Directory.CreateDirectory("vox/" + altFolder);
            //VoxelLogic.WriteVOX("vox/" + altFolder + moniker + "_" + palette + ".vox", work, "W", palette, 40, 40, 40);

            //MagicaVoxelData[] explodeParsed = parsed.Replicate();
            for(int f = 0; f < framelimit; f++)
            {
                for(int dir = 0; dir < 4; dir++)
                {

                    //FaceVoxel[,,] work = FaceLogic.GetFaces(TransformLogic.RotateYaw(modelFrames[f].Finalize(), dir * 90));

                    //                FaceVoxel[,,] faces = FaceLogic.GetFaces(FaceLogic.VoxListToArray(VoxelLogic.BasicRotateLarge(parsed, dir), 60, 60, 60, 153));
                    byte[][] b = renderGenericW(
                        PatternLogic.ApplyPattern(
                            //TransformLogic.RunSurfaceCA(
                            TransformLogic.RunCA(
                                                FaceLogic.FaceArrayToByteArray(
                                                        //FaceLogic.DoubleSize(
                                                        FaceLogic.GetFaces(
                                                            TransformLogic.RunThinningCA(
                                                                    modelFrames[f]
                                                                    .Finalize(),
                                                                2)//1 + OrthoSingle.multiplier * OrthoSingle.bonus / 2)
                                                            )
                                                   //)
                                                   ),
                            2, true),
                            model.Patterns)
                    , palette, f, framelimit, still, false, 120, 120, 120);

                    /*
                                        byte[][] b = renderGenericW(
                        PatternLogic.ApplyPattern(
                        TransformLogic.RunCA(
                            FaceLogic.FaceArrayToByteArray(
                                FaceLogic.DoubleSize(
                                    FaceLogic.GetFaces(
                                            modelFrames[f]
                                            .Finalize())
                                    )
                                ),
                            1 + OrthoSingle.multiplier * OrthoSingle.bonus),
                        model.Patterns)
                    , palette, f, framelimit, still, false, 120, 120, 120);

                    */
                    ImageInfo imi = new ImageInfo(b[0].Length, b.Length, 8, false, false, true);
                    PngWriter png = FileHelper.CreatePngWriter(folder + "/palette" + palette + "_" + moniker + "_Ortho_face" + dir + "_" + f + ".png", imi, true);
                    WritePNG(png, b, simplepalettes[palette]);
                }
            }


            List<string> imageNames = new List<string>(4 * 8 * framelimit);
            for(int p = 0; p < 8; p++)
            {
                for(int dir = 0; dir < 4; dir++)
                {
                    for(int f = 0; f < framelimit; f++)
                    {
                        imageNames.Add(folder + "/palette" + palette + "_" + moniker + "_Ortho_face" + dir + "_" + f + ".png");
                    }
                }
            }
            Directory.CreateDirectory("gifs/" + altFolder);
            Console.WriteLine("Running GIF conversion ...");
            WriteGIF(imageNames, 11, "gifs/" + altFolder + "palette" + palette + "_" + moniker + "_Ortho");

        }

        static void Main(string[] args)
        {
            VoxelLogic.rowWidthBytes = 0;
            VoxelLogic.Initialize();

            //            altFolder = "botl6/";
            //            FaceLogic.VisualMode = "Mecha";

            VoxelLogic.Initialize();
            VoxelLogic.VisualMode = "CU";
            altFolder = "Diverse_PixVoxel_Wargame_Ortho_New/";
            blankFolder = "Blank_PixVoxel_Wargame_Ortho_New/";
            CURedux.Initialize(true);
            
            //VoxelLogic.VisualMode = "W";
            //altFolder = "Forays2/";
            //VoxelLogic.voxFolder = "ForaysBones/";
            //ForaysPalettes.Initialize();

            //VoxelLogic.VisualMode = "Mon";
            //altFolder = "Mon/";
            //MonPalettes.Initialize();
            /*
            Directory.CreateDirectory("SDW_Ortho");
            altFolder = "sdw/";
            CURedux.Initialize(true);
            VoxelLogic.VisualMode = "W";
            VoxelLogic.voxFolder = "sdw/";
            */
            //System.IO.Directory.CreateDirectory("Mon");
            //System.IO.Directory.CreateDirectory("Forays2");

            //  altFolder = "mecha3/";
            //VoxelLogic.wpalettes = AlternatePalettes.mecha_palettes;
            //altFolder = "CU3/";
            //System.IO.Directory.CreateDirectory("CU_Ortho_S2");
            //System.IO.Directory.CreateDirectory("CU_Ortho_S2/Terrains");
            //System.IO.Directory.CreateDirectory("CU_Ortho_S2/TerrainsBlank");
            //System.IO.Directory.CreateDirectory("mecha3");
            //System.IO.Directory.CreateDirectory("vox/mecha3");

            
            //            SaPalettes.Initialize();
            InitializeWPalette();
            //processUnitLargeW("Burnbruin", 9, true, false);

            //            altFolder = "sau10/";
            //            makeFlatTiling().Save("tiling_flat_gray.png");

            /*
            processUnitHugeW("Grass", 35, true, false);
            processUnitHugeW("Tree", 35, true, false);
            processUnitHugeW("Boulder", 36, true, false);
            processUnitHugeW("Rubble", 36, true, false);

            processUnitLargeW("Rakgar", 18, true, false);

            processUnitLargeW("Lomuk", 13, false, false);
            processUnitLargeWalkW("Lomuk", 13);

            processUnitLargeW("Axarik", 0, true, false);
            processUnitLargeWalkW("Axarik", 0);
            processUnitLargeW("Tassar", 17, false, false);
            processUnitLargeWalkW("Tassar", 17);
            processUnitLargeW("Erezdo", 2, true, false);
            processUnitLargeWalkW("Erezdo", 2);
            processUnitLargeW("Ceglia", 1, true, false);
            processUnitLargeWalkW("Ceglia", 1);

            processUnitHugeW("Nodebpe", 14, true, false);
            processUnitHugeWalkW("Nodebpe", 14);

            processUnitLargeW("Glarosp", 3, true, false);
            processUnitLargeWalkW("Glarosp", 3);

            processUnitLargeW("Ilapa", 11, true, false);
            processUnitLargeWalkW("Ilapa", 11);
            processUnitLargeW("Kurguiv", 12, false, false);
            processUnitLargeWalkW("Kurguiv", 12);
            processUnitHugeW("Oah", 15, true, false);
            processUnitHugeWalkW("Oah", 15);
            processUnitLargeW("Sfyst", 16, true, false);
            processUnitLargeWalkW("Sfyst", 16);
            processUnitLargeW("Tassar", 17, false, false);
            processUnitLargeWalkW("Tassar", 17);
            processUnitHugeW("Vashk", 18, true, false);
            processUnitHugeWalkW("Vashk", 18);
            processUnitLargeW("Meisan", 43, false, false);
            processUnitLargeWalkW("Meisan", 43);


            processUnitLargeW("Human_Male", 4, true, false);
            processUnitLargeWalkW("Human_Male", 4);
            processUnitLargeW("Human_Male", 5, true, false);
            processUnitLargeWalkW("Human_Male", 5);
            processUnitLargeW("Human_Male", 6, true, false);
            processUnitLargeWalkW("Human_Male", 6);
            processUnitLargeW("Human_Male", 7, true, false);
            processUnitLargeWalkW("Human_Male", 7);
            processUnitLargeW("Human_Male", 8, true, false);
            processUnitLargeWalkW("Human_Male", 8);
            processUnitLargeW("Human_Male", 9, true, false);
            processUnitLargeWalkW("Human_Male", 9);
            processUnitLargeW("Human_Male", 10, true, false);
            processUnitLargeWalkW("Human_Male", 10);

            processUnitLargeW("Human_Female", 4, true, false);
            processUnitLargeWalkW("Human_Female", 4);
            processUnitLargeW("Human_Female", 5, true, false);
            processUnitLargeWalkW("Human_Female", 5);
            processUnitLargeW("Human_Female", 6, true, false);
            processUnitLargeWalkW("Human_Female", 6);
            processUnitLargeW("Human_Female", 7, true, false);
            processUnitLargeWalkW("Human_Female", 7);
            processUnitLargeW("Human_Female", 8, true, false);
            processUnitLargeWalkW("Human_Female", 8);
            processUnitLargeW("Human_Female", 9, true, false);
            processUnitLargeWalkW("Human_Female", 9);
            processUnitLargeW("Human_Female", 10, true, false);
            processUnitLargeWalkW("Human_Female", 10);


            processUnitHugeW("Barrel", 38, true, false);

            processUnitHugeW("Table", 39, true, false);
            processUnitHugeW("Desk", 39, true, false);
            processUnitHugeW("Computer_Desk", 39, true, false);
            processUnitHugeW("Computer_Desk", 40, true, false);

            processUnitHugeW("Table", 41, true, false);
            processUnitHugeW("Desk", 41, true, false);
            processUnitHugeW("Computer_Desk", 41, true, false);
            processUnitHugeW("Computer_Desk", 42, true, false);
            */
            /*
            processHatLargeW("Generic_Male", 0, "Berserker");
            processHatLargeW("Generic_Male", 0, "Witch");
            processHatLargeW("Generic_Male", 0, "Scout");
            processHatLargeW("Generic_Male", 0, "Captain");
            processHatLargeW("Generic_Male", 0, "Mystic");
            processHatLargeW("Generic_Male", 0, "Wizard");
            processHatLargeW("Generic_Male", 0, "Provocateur");
            processHatLargeW("Generic_Male", 0, "Noble");
            processHatLargeW("Generic_Male", 44, "Woodsman");
            processHatLargeW("Generic_Male", 0, "Sheriff");
            processHatLargeW("Generic_Male", 0, "Thief");
            processHatLargeW("Generic_Male", 0, "Merchant");
            processHatLargeW("Generic_Male", 49, "Farmer");
            processHatLargeW("Generic_Male", 0, "Officer");
            processHatLargeW("Generic_Male", 0, "Dervish");
            processHatLargeW("Generic_Male", 0, "Thug");
            processHatLargeW("Generic_Male", 0, "Bishop");


            processHatLargeW("Spirit", 7, "Berserker");
            processHatLargeW("Spirit", 7, "Witch");
            processHatLargeW("Spirit", 7, "Scout");
            processHatLargeW("Spirit", 7, "Captain");
            processHatLargeW("Spirit", 7, "Mystic");
            processHatLargeW("Spirit", 7, "Wizard");
            processHatLargeW("Spirit", 7, "Provocateur");
            processHatLargeW("Spirit", 7, "Noble");
            processHatLargeW("Spirit", 44, "Woodsman");
            processHatLargeW("Spirit", 7, "Sheriff");
            processHatLargeW("Spirit", 7, "Thief");
            processHatLargeW("Spirit", 7, "Merchant");
            processHatLargeW("Spirit", 49, "Farmer");
            processHatLargeW("Spirit", 7, "Officer");
            processHatLargeW("Spirit", 7, "Dervish");
            processHatLargeW("Spirit", 7, "Thug");
            processHatLargeW("Spirit", 7, "Bishop");
            */

            /*
            processHats("Zombie", 2, true, classes);

            processHats("Skeleton", 6, true, classes);

            processHats("Spirit", 7, false, classes);

            processHats("Wraith", 8, false, classes);

            processHats("Cinder", 9, true, classes);

            processHats("Ghoul", 39, true, classes);

            processHats("Wight", 40, true, classes);

            processHats("Spectre", 42, false, classes);

            processHats("Mummy", 43, true, classes);

            processHats("Drowned", 45, true, classes);

            processHats("Banshee", 46, false, classes);

            processHats("Damned", 63, true, classes);

            processHats("Husk", 64, true, classes);


            processHats("Generic_Male", 0, true, classes, "Living_Men_A");

            processHats("Generic_Male", 1, true, classes, "Living_Men_B");

            processHats("Generic_Male", 15, true, classes, "Living_Men_C");

            processHats("Generic_Male", 16, true, classes, "Living_Men_D");

            processHats("Generic_Male", 17, true, classes, "Living_Men_E");

            processHats("Generic_Female", 0, true, classes, "Living_Women_A");

            processHats("Generic_Female", 1, true, classes, "Living_Women_B");

            processHats("Generic_Female", 15, true, classes, "Living_Women_C");

            processHats("Generic_Female", 16, true, classes, "Living_Women_D");

            processHats("Generic_Female", 17, true, classes, "Living_Women_E");


            processHats("Bulky_Male", 0, true, classes, "Bulky_Men_A");

            processHats("Bulky_Male", 1, true, classes, "Bulky_Men_B");

            processHats("Bulky_Male", 15, true, classes, "Bulky_Men_C");

            processHats("Bulky_Male", 16, true, classes, "Bulky_Men_D");

            processHats("Bulky_Male", 17, true, classes, "Bulky_Men_E");


            processHats("Bulky_Female", 0, true, classes, "Bulky_Women_A");

            processHats("Bulky_Female", 1, true, classes, "Bulky_Women_B");

            processHats("Bulky_Female", 15, true, classes, "Bulky_Women_C");

            processHats("Bulky_Female", 16, true, classes, "Bulky_Women_D");

            processHats("Bulky_Female", 17, true, classes, "Bulky_Women_E");


            processHats("Armored_Male", 0, true, classes, "Armored_Men_A");

            processHats("Armored_Male", 1, true, classes, "Armored_Men_B");

            processHats("Armored_Male", 15, true, classes, "Armored_Men_C");

            processHats("Armored_Male", 16, true, classes, "Armored_Men_D");

            processHats("Armored_Male", 17, true, classes, "Armored_Men_E");

            processUnitLargeW("Necromancer", 65, true, false);

            File.WriteAllText("relative-hat-positions.txt", model_headpoints.ToString());
            File.WriteAllText("hats.txt", hat_headpoints.ToString());

            processHats("Birthday_Necromancer", 65, true, classes);


            offsets.Close();
            offbin.Close();

            TallVoxels.makeFlatTilingDrab().Save("tiling_96x48.png", ImageFormat.Png);

            for(int i = 50; i <= 60; i++)
            {
                processUnitHugeW("Terrain", i, true, true);
            }


            processUnitHugeW("Grass", 47, true, true);
            processUnitHugeW("Tree", 47, true, false);
            processUnitHugeW("Boulder", 48, true, false);
            processUnitHugeW("Rubble", 48, true, false);
            processUnitHugeW("Headstone", 48, true, false);

            processUnitHugeW("Roof_Flat", 49, true, true);
            processUnitHugeW("Roof_Straight", 49, true, true);
            processUnitHugeW("Roof_Corner", 49, true, true);
            processUnitHugeW("Roof_Solid_Flat", 49, true, true);
            processUnitHugeW("Roof_Solid_Straight", 49, true, true);
            processUnitHugeW("Roof_Solid_Corner", 49, true, true);
            processUnitHugeW("Roof_Solid_Straight_Off", 49, true, true);
            processUnitHugeW("Roof_Solid_Corner_Off", 49, true, true);

            processUnitHugeW("Door_Closed", 49, true, false);
            processUnitHugeW("Door_Open", 49, true, false);
            processUnitHugeW("Wall_Straight", 49, true, false);
            processUnitHugeW("Wall_Cross", 49, true, false);
            processUnitHugeW("Wall_Tee", 49, true, false);
            processUnitHugeW("Wall_Corner", 49, true, false);
            processUnitHugeW("Wall_Straight_Upper", 49, true, true);
            processUnitHugeW("Wall_Cross_Upper", 49, true, true);
            processUnitHugeW("Wall_Tee_Upper", 49, true, true);
            processUnitHugeW("Wall_Corner_Upper", 49, true, true);
            TallVoxels.altFolder = altFolder;
            TallVoxels.generateBotLSpritesheet();
            */
            /*
            processPureAttackW("Autofire", 0);
            processPureAttackW("Beam", 0);
            processPureAttackW("Cannon", 0);
            processPureAttackW("Lightning", 0);

            processUnitLargeWMecha(moniker: "Vox_Populi", head: "Blocky", torso: "Blocky", legs: "Blocky", left_arm: "Blocky", right_arm: "Blocky", right_weapon: "Bazooka", left_weapon: "Pistol");
            processUnitLargeWMechaFiring(moniker: "Vox_Populi", head: "Blocky", torso: "Blocky", legs: "Blocky", left_arm: "Blocky", right_arm: "Blocky", right_weapon: "Bazooka", left_weapon: "Pistol", right_projectile: "Beam", left_projectile: "Autofire");
            processUnitLargeWMechaFiring(moniker: "Vox_Populi", head: "Blocky", torso: "Blocky", legs: "Blocky", left_arm: "Blocky", right_arm: "Blocky", right_weapon: "Bazooka", left_weapon: "Pistol", right_projectile: "Lightning", left_projectile: "Cannon");

            processUnitLargeWMecha(moniker: "Vox_Nihilus", head: "Blocky", torso: "Blocky", legs: "Blocky", left_arm: "Blocky_Aiming", right_arm: "Blocky_Aiming", right_weapon: "Rifle");
            processUnitLargeWMechaAiming(moniker: "Vox_Nihilus", head: "Blocky_Aiming", torso: "Blocky", legs: "Blocky", left_arm: "Blocky_Aiming", right_arm: "Blocky_Aiming", right_weapon: "Rifle", right_projectile: "Cannon");
            processUnitLargeWMechaAiming(moniker: "Vox_Nihilus", head: "Blocky_Aiming", torso: "Blocky", legs: "Blocky", left_arm: "Blocky_Aiming", right_arm: "Blocky_Aiming", right_weapon: "Rifle", right_projectile: "Lightning");

            processUnitLargeWMecha(moniker: "Maku", left_weapon: "Bazooka");
            processUnitLargeWMechaFiring(moniker: "Maku", left_weapon: "Bazooka", left_projectile: "Beam");
            processUnitLargeWMechaFiring(moniker: "Maku", left_weapon: "Bazooka", left_projectile: "Lightning");

            processUnitLargeWMecha(moniker: "Mark_Zero", head: "Armored", left_arm: "Armored_Aiming", right_arm: "Armored_Aiming", right_weapon: "Rifle");
            processUnitLargeWMechaAiming(moniker: "Mark_Zero", head: "Armored_Aiming", left_arm: "Armored_Aiming", right_arm: "Armored_Aiming", right_weapon: "Rifle", right_projectile: "Beam");
            processUnitLargeWMechaAiming(moniker: "Mark_Zero", head: "Armored_Aiming", left_arm: "Armored_Aiming", right_arm: "Armored_Aiming", right_weapon: "Rifle", right_projectile: "Lightning");

            processUnitLargeWMecha(moniker: "Deadman", head: "Armored", left_weapon: "Pistol", right_weapon: "Katana");
            processUnitLargeWMechaSwinging(moniker: "Deadman", head: "Armored", left_weapon: "Pistol", right_weapon: "Katana", left_projectile: "Autofire");
            processUnitLargeWMechaSwinging(moniker: "Deadman", head: "Armored", left_weapon: "Pistol", right_weapon: "Katana", left_projectile: "Lightning");

            processUnitLargeWMecha(moniker: "Chivalry", head: "Armored", right_weapon: "Beam_Sword");
            processUnitLargeWMechaSwinging(moniker: "Chivalry", head: "Armored", right_weapon: "Beam_Sword");

            processUnitLargeWMecha(moniker: "Banzai", left_weapon: "Pistol", right_weapon: "Pistol");
            processUnitLargeWMecha(moniker: "Banzai_Flying", left_weapon: "Pistol", right_weapon: "Pistol",
                legs: "Armored_Jet", still: false);
            processUnitLargeWMechaFiring(moniker: "Banzai", left_weapon: "Pistol", right_weapon: "Pistol", left_projectile: "Autofire", right_projectile: "Autofire");
            processUnitLargeWMechaFiring(moniker: "Banzai_Flying", left_weapon: "Pistol", right_weapon: "Pistol", left_projectile: "Autofire", right_projectile: "Autofire",
                legs: "Armored_Jet", still: false);
            */

            //writePaletteImages();
            //renderTerrain();

            /*
            renderTerrainDetailed("Plains");
            renderTerrainDetailed("Forest");
            renderTerrainDetailed("Desert");
            renderTerrainDetailed("Jungle");
            renderTerrainDetailed("Hill");
            renderTerrainDetailed("Mountain");
            renderTerrainDetailed("Road");
            renderTerrainDetailed("Tundra");
            renderTerrainDetailed("Ruins");
            renderTerrainDetailed("River");
            renderTerrainDetailed("Ocean");

            makeDetailedTiling();
            */
            /*
            
            Directory.CreateDirectory(altFolder + "Terrains");
            Directory.CreateDirectory(blankFolder + "Terrains");

            renderTerrainSimple("Road", "Ordered", new byte[] {
                    32, 5, 3,
                    33, 7, 3,
                    34, 9, 2}, 33, 32, 56);

            renderTerrainSimple("Plains", "High", new byte[] {
                    0, 27, 7,
                    2, 34, 2, }, 1, 0, 2);
            renderTerrainSimple("Forest", "High", new byte[] {
                    4, 27, 7,
                    6, 34, 2, }, 5, 4, 6);
            renderTerrainSimple("Sand", "High", new byte[] { //was called Desert
                    8, 27, 7,
                    10, 34, 2, }, 9, 8, 10);
            renderTerrainSimple("Jungle", "High", new byte[] {
                    12, 27, 7,
                    14, 34, 2, }, 13, 12, 14);
            renderTerrainSimple("Hill", "High", new byte[] {
                    16, 27, 7,
                    18, 34, 2, }, 17, 16, 18);
            renderTerrainSimple("Mountain", "High", new byte[] {
                    20, 27, 7,
                    22, 34, 2, }, 21, 20, 22);
            renderTerrainSimple("Ruins", "High", new byte[] {
                    24, 27, 7,
                    26, 34, 2, }, 25, 24, 26);
            renderTerrainSimple("Ice", "High", new byte[] { //was called Tundra
                    28, 27, 7,
                    30, 34, 2, }, 29, 28, 30);

            renderTerrainSimple("River", "Water", new byte[] {
                    36, 7, 2,
                    38, 13, 2,
                    45, 39, 7,
                    4, 51, 2, }, 37, 36, 38);
            renderTerrainSimple("Ocean", "Water", new byte[] {
                    46, 23, 5,
                    47, 21, 4,
                    44, 29, 7, }, 45, 44, 46);
            renderTerrainSimple("Pit", "Normal", new byte[] {
                    41, 11, 3,
                    }, 40, 40, 40);

            renderTerrainSimple("Volcano", "Normal", new byte[] {
                    49, 30, 2,
                    57, 29, 5,
                    51, 13, 3,
                    50, 25, 8, }, 49, 48, 50);
            renderTerrainSimple("Poison", "Water", new byte[] {
                    3, 27, 3,
                    2, 24, 9,
                    1, 22, 11,
                    52, 13, 7,}, 53, 52, 54);
            renderTerrainSimple("Warning", "Ordered", new byte[] {
                    59, 5, 2,
                    }, 58, 58, 58);
            
            makeSimpleTiling();
            
             */

            Directory.CreateDirectory(altFolder + "SubTerrains");
            Directory.CreateDirectory(blankFolder + "SubTerrains");

            renderSubTerrainSimple("Road", "Ordered", new byte[] {
                    32, 5, 3,
                    33, 7, 3,
                    34, 9, 2}, 33, 32, 56);

            renderSubTerrainSimple("Asphalt", "Normal", new byte[] {
                    32, 5, 3,
                    33, 7, 3,
                    34, 9, 2}, 33, 32, 34);

            renderSubTerrainSimple("Plains", "Normal", new byte[] {
                    0, 27, 7,
                    2, 34, 2, }, 1, 0, 2);
            renderSubTerrainSimple("Forest", "Normal", new byte[] {
                    4, 27, 7,
                    6, 34, 2, }, 5, 4, 6);
            renderSubTerrainSimple("Sand", "Normal", new byte[] { //was called Desert
                    8, 27, 7,
                    10, 34, 2, }, 9, 8, 10);
            renderSubTerrainSimple("Jungle", "Normal", new byte[] {
                    12, 27, 7,
                    14, 34, 2, }, 13, 12, 14);
            renderSubTerrainSimple("Hill", "Normal", new byte[] {
                    16, 27, 7,
                    18, 34, 2, }, 17, 16, 18);
            renderSubTerrainSimple("Mountain", "Normal", new byte[] {
                    20, 27, 7,
                    22, 34, 2, }, 21, 20, 22);
            renderSubTerrainSimple("Ruins", "Normal", new byte[] {
                    24, 27, 7,
                    26, 34, 2, }, 25, 24, 26);
            renderSubTerrainSimple("Ice", "Normal", new byte[] { //was called Tundra
                    28, 27, 7,
                    30, 34, 2, }, 29, 28, 30);

            renderSubTerrainSimple("River", "Normal", new byte[] {
                    36, 7, 2,
                    38, 13, 2,
                    45, 39, 7,
                    4, 51, 2, }, 37, 36, 38);
            renderSubTerrainSimple("Ocean", "Normal", new byte[] {
                    46, 23, 5,
                    47, 21, 4,
                    44, 29, 7, }, 45, 44, 46);
            renderSubTerrainSimple("Pit", "Normal", new byte[] {
                    41, 11, 3,
                    }, 40, 40, 40);

            renderSubTerrainSimple("Volcano", "Normal", new byte[] {
                    49, 30, 2,
                    57, 29, 5,
                    51, 13, 3,
                    50, 25, 8, }, 49, 48, 50);
            renderSubTerrainSimple("Poison", "Normal", new byte[] {
                    3, 27, 3,
                    2, 24, 9,
                    1, 22, 11,
                    52, 13, 7,}, 53, 52, 54);
            renderSubTerrainSimple("Warning", "Normal", new byte[] {
                    59, 5, 2,
                    }, 58, 58, 58);

            //makeSimpleTiling();

            for(int a = 2; a < 2; a++)
            {
                /*
                for(int u = 0; u < VoxelLogic.CurrentUnits.Length; u++)
                {
                    processUnitLargeWMilitary(VoxelLogic.CurrentUnits[u]);
                }
                */
                processUnitLargeWMilitary("Infantry");
                processUnitLargeWMilitary("Infantry_P");
                processUnitLargeWMilitary("Infantry_S");
                processUnitLargeWMilitary("Infantry_T");

                processUnitLargeWMilitary("Artillery");
                processUnitLargeWMilitary("Artillery_P");
                processUnitLargeWMilitary("Artillery_S");
                processUnitLargeWMilitary("Artillery_T");

                processUnitLargeWMilitary("Tank");
                processUnitLargeWMilitary("Tank_P");
                processUnitLargeWMilitary("Tank_S");
                processUnitLargeWMilitary("Tank_T");

                processUnitLargeWMilitary("Infantry_PS");
                processUnitLargeWMilitary("Infantry_PT");
                processUnitLargeWMilitary("Infantry_ST");

                processUnitLargeWMilitary("Recon");
                processUnitLargeWMilitary("Flamethrower");

                processUnitLargeWMilitary("Volunteer");
                processUnitLargeWMilitary("Volunteer_P");
                processUnitLargeWMilitary("Volunteer_S");
                processUnitLargeWMilitary("Volunteer_T");


                processUnitLargeWMilitary("Civilian");

                processUnitLargeWMilitary("Truck");
                processUnitLargeWMilitary("Truck_P");
                processUnitLargeWMilitary("Truck_S");
                processUnitLargeWMilitary("Truck_T");

                processUnitLargeWMilitary("Copter");
                processUnitLargeWMilitary("Copter_P");
                processUnitLargeWMilitary("Copter_S");
                processUnitLargeWMilitary("Copter_T");

                processUnitLargeWMilitary("Plane");
                processUnitLargeWMilitary("Plane_P");
                processUnitLargeWMilitary("Plane_S");
                processUnitLargeWMilitary("Plane_T");

                processUnitLargeWMilitary("Boat");
                processUnitLargeWMilitary("Boat_P");
                processUnitLargeWMilitary("Boat_S");
                processUnitLargeWMilitary("Boat_T");

                processUnitLargeWMilitary("Laboratory");
                processUnitLargeWMilitary("Dock");
                processUnitLargeWMilitary("Airport");
                processUnitLargeWMilitary("City");
                processUnitLargeWMilitary("Factory");
                processUnitLargeWMilitary("Castle");
                processUnitLargeWMilitary("Estate");

                processUnitLargeWMilitary("Hospital");
                processUnitLargeWMilitary("Farm");
                processUnitLargeWMilitary("Oil_Well");
                /*
                for(int v = 0; v < CURedux.super_units.Length; v++)
                {
                    processUnitHugeWMilitarySuper(CURedux.super_units[v]);
                }
                */
                //CURedux.super_units.AsParallel().ForAll(u => processUnitHugeWMilitarySuper(u));
                /*
                processUnitHugeWMilitarySuper("Copter");
                processUnitHugeWMilitarySuper("Copter_P");
                processUnitHugeWMilitarySuper("Copter_S");
                processUnitHugeWMilitarySuper("Copter_T");

                processUnitHugeWMilitarySuper("Plane");
                processUnitHugeWMilitarySuper("Plane_P");
                processUnitHugeWMilitarySuper("Plane_S");
                processUnitHugeWMilitarySuper("Plane_T");

                processUnitHugeWMilitarySuper("Boat");
                processUnitHugeWMilitarySuper("Boat_P");
                processUnitHugeWMilitarySuper("Boat_S");
                processUnitHugeWMilitarySuper("Boat_T");

                processUnitHugeWMilitarySuper("Laboratory");
                processUnitHugeWMilitarySuper("Dock");
                processUnitHugeWMilitarySuper("City");
                processUnitHugeWMilitarySuper("Factory");
                processUnitHugeWMilitarySuper("Castle");
                processUnitHugeWMilitarySuper("Estate");
                processUnitHugeWMilitarySuper("Airport");
                processUnitHugeWMilitarySuper("Hospital");
                processUnitHugeWMilitarySuper("Farm");
                processUnitHugeWMilitarySuper("Oil_Well");
                */
                addon = TallFacesDithered.addon = "";
            }
            //addon = TallFacesDithered.addon = "";

            //processReceivingMilitaryW();

            //processReceivingMilitaryWSuper();
            //WriteAllGIFs();

            //addon = "Zombie_";
            //WriteZombieGIFs();

            /*
            Pose walk0 = (model => model
            ),
            walk1 = (model => model
            .AddPitch(40, "Left_Upper_Leg")
            .AddPitch(25, "Left_Lower_Leg")
            .AddPitch(360-30, "Right_Upper_Leg")
            .AddPitch(360-30, "Right_Lower_Leg")
            .Translate(-5, -1, 7, "Left_Lower_Leg")
            ),
            walk2 = (model => model
            //.AddStretch(1, 1, 0.85f)
            //.AddStretch(1, 1, 1, "Left_Weapon")
            ),
            walk3 = (model => model
            .AddPitch(40, "Right_Upper_Leg")
            .AddPitch(25, "Right_Lower_Leg")
            .AddPitch(360-30, "Left_Upper_Leg")
            .AddPitch(360-30, "Left_Lower_Leg")
            .Translate(6, 1, -8, "Left_Lower_Leg")
            );
            Pattern blackLeatherPattern = new Pattern(253 - 48 * 4, 0, 253 - 47 * 4, 0, 5, 2, 3, 1, 0.5f),
                brownLeatherPattern = new Pattern(253 - 44 * 4, 0, 253 - 43 * 4, 0, 3, 2, 2, 1, 0.6f),
                tanLeatherPattern = new Pattern(253 - 46 * 4, 0, 253 - 45 * 4, 0, 3, 2, 2, 1, 0.6f),
                mailPattern = new Pattern(253 - 56 * 4, 0, 253 - 57 * 4, 0, 2, 1, 1, 1, 1f),
                camoPattern = new Pattern(new byte[] { 253 - 3 * 4, 253 - 5 * 4, 253 - 29 * 4, 253 - 33 * 4, }, new byte[] { 0 },
                new byte[] { 253 - 2 * 4, 253 - 4 * 4, 253 - 28 * 4, 253 - 32 * 4, }, new byte[] { 0 }, 3, 3, 4, 4, 0.8f, true);
            Dictionary<byte, Pattern> camo = new Dictionary<byte, Pattern>() {
                { 253 - 5 * 4, camoPattern },
                { 253 - 4 * 4, camoPattern },
                { 253 - 3 * 4, camoPattern },
                { 253 - 2 * 4, camoPattern },
            };

            //            processUnitLargeWBones(left_weapon: "Longsword", pose: pose1);
            Model soldier = Model.HumanoidAlt(left_weapon: "Machine_Gun", body: "Foot_Soldier", patterns: camo);
            processUnitLargeWModel("Foot_Soldier", true, 0, soldier,
                new Pose[] { walk0, walk1, walk2, walk3 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.33f },
                new float[] { 0, 1, 0.66f },
                new float[] { 0, 1, 1f },
                new float[] { 1, 2, 0.33f },
                new float[] { 1, 2, 0.66f },
                new float[] { 1, 2, 1f },
                new float[] { 2, 3, 0.33f },
                new float[] { 2, 3, 0.66f },
                new float[] { 2, 3, 1.0f },
                new float[] { 3, 0, 0.33f },
                new float[] { 3, 0, 0.66f },}, true);
            */

            Pose bow0 = (model => model
            .AddPitch(90, "Left_Weapon", "Left_Lower_Arm", "Right_Lower_Arm")
            .AddPitch(45, "Left_Upper_Arm", "Right_Upper_Arm")
            .AddYaw(45, "Torso", "Left_Leg", "Right_Leg")
            .AddYaw(30, "Right_Upper_Arm")
            .AddYaw(-15, "Right_Lower_Arm")
            .AddYaw(60, "Left_Upper_Arm", "Left_Lower_Arm")
            .Translate(0, -10, 0, "Left_Leg")
            ),
                bow1 = (model => model
            .AddPitch(90, "Left_Weapon", "Left_Lower_Arm", "Right_Lower_Arm")
            .AddPitch(45, "Left_Upper_Arm", "Right_Upper_Arm")
            .AddYaw(45, "Torso", "Left_Leg", "Right_Leg")
            .AddYaw(50, "Right_Upper_Arm")
            .AddYaw(-35, "Right_Lower_Arm")
            .AddYaw(60, "Left_Upper_Arm", "Left_Lower_Arm")
            .AddStretch(0.85f, 1.0f, 1.6f, "Left_Weapon")
            .Translate(0, -10, 0, "Left_Leg")
            ),

                bow2 = (model => model
            .AddPitch(90, "Left_Weapon", "Left_Lower_Arm", "Right_Lower_Arm")
            .AddPitch(45, "Left_Upper_Arm", "Right_Upper_Arm")
            .AddYaw(45, "Torso", "Left_Leg", "Right_Leg")
            .AddYaw(60, "Right_Upper_Arm")
            .AddYaw(-15, "Right_Lower_Arm")
            .AddYaw(60, "Left_Upper_Arm", "Left_Lower_Arm")
            .Translate(0, -10, 0, "Left_Leg")
            );
            Pattern blackLeatherPattern = new Pattern(253 - 48 * 4, 0, 253 - 47 * 4, 0, 5, 2, 3, 1, 0.5f),
                brownLeatherPattern = new Pattern(253 - 44 * 4, 0, 253 - 43 * 4, 0, 3, 2, 2, 1, 0.6f),
                tanLeatherPattern = new Pattern(253 - 46 * 4, 0, 253 - 45 * 4, 0, 3, 2, 2, 1, 0.6f),
                mailPattern = new Pattern(253 - 56 * 4, 0, 253 - 57 * 4, 0, 2, 1, 1, 1, 1f);
            Dictionary<byte, Pattern> leather = new Dictionary<byte, Pattern>() {
                { 253 - 29 * 4, brownLeatherPattern },
                { 253 - 5 * 4, brownLeatherPattern },
                { 253 - 4 * 4, brownLeatherPattern },
                { 253 - 3 * 4, brownLeatherPattern },
                { 253 - 2 * 4, brownLeatherPattern },
            }, chain = new Dictionary<byte, Pattern>() {
                { 253 - 29 * 4, mailPattern },
                { 253 - 5 * 4, mailPattern },
                { 253 - 4 * 4, mailPattern },
                { 253 - 3 * 4, brownLeatherPattern },
                { 253 - 2 * 4, brownLeatherPattern },
            };
            
            
            Pose swing0l = (model => model),
                swing1l = (model => model
            .AddPitch(10, "Left_Weapon")
            .AddPitch(70, "Left_Lower_Arm", "Left_Weapon")
            //.AddYaw(-10, "Left_Upper_Arm", "Left_Lower_Arm", "Left_Weapon")
            .AddPitch(75, "Left_Upper_Arm", "Left_Lower_Arm", "Left_Weapon")),
                swing2l = (model => model
            .AddPitch(-30, "Left_Weapon")
            .AddPitch(40, "Left_Upper_Arm", "Left_Lower_Arm", "Left_Weapon")
            .AddYaw(30, "Left_Lower_Arm", "Left_Weapon")
            .AddSpread("Left_Weapon", 60f, 0f, 30f, 253 - 12 * 4));

            Pose swing0r = (model => model),
                swing1r = (model => model
            .AddPitch(10, "Right_Weapon")
            .AddPitch(70, "Right_Lower_Arm", "Right_Weapon")
            //.AddYaw(-10, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(75, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")),
                swing2r = (model => model
            .AddPitch(-30, "Right_Weapon")
            .AddPitch(40, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddYaw(330, "Right_Lower_Arm", "Right_Weapon")
            .AddSpread("Right_Weapon", 60f, 0f, 30f, 253 - 12 * 4));

            Pose stab0l = (model => model),
                stab1l = (model => model
            .AddPitch(10, "Left_Weapon")
            .AddPitch(60, "Left_Lower_Arm")
            //.AddYaw(-10, "Left_Upper_Arm", "Left_Lower_Arm", "Left_Weapon")
            .AddPitch(360 - 35, "Left_Upper_Arm")
            .AddYaw(340, "Right_Lower_Arm", "Right_Upper_Arm", "Right_Weapon", "Torso")),
                stab1l_alt = (model => model
            .AddPitch(10, "Left_Weapon")
            .AddPitch(65, "Left_Lower_Arm")
            //.AddYaw(-10, "Left_Upper_Arm", "Left_Lower_Arm", "Left_Weapon")
            .AddPitch(360 - 45, "Left_Upper_Arm")
            .AddYaw(20, "Right_Lower_Arm", "Right_Upper_Arm", "Right_Weapon", "Torso")),
                stab2l = (model => model
            .AddPitch(80, "Left_Lower_Arm")
            .AddPitch(65, "Left_Upper_Arm")
            .AddYaw(20, "Right_Lower_Arm", "Right_Upper_Arm", "Right_Weapon", "Torso"));

            Pose stab0r = (model => model),
                stab1r = (model => model
            .AddPitch(10, "Right_Weapon")
            .AddPitch(60, "Right_Lower_Arm")
            //.AddYaw(-10, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(360 - 35, "Right_Upper_Arm")
            .AddYaw(20, "Left_Lower_Arm", "Left_Upper_Arm", "Left_Weapon", "Torso")),
                stab1r_alt = (model => model
            .AddPitch(10, "Right_Weapon")
            .AddPitch(65, "Right_Lower_Arm")
            //.AddYaw(-10, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(360 - 45, "Right_Upper_Arm")
            .AddYaw(340, "Left_Lower_Arm", "Left_Upper_Arm", "Left_Weapon", "Torso")),
                stab2r = (model => model
            .AddPitch(80, "Right_Lower_Arm")
            .AddPitch(65, "Right_Upper_Arm")
            .AddYaw(340, "Left_Lower_Arm", "Left_Upper_Arm", "Left_Weapon", "Torso"));

            Pose spin0r = (model => model
            .AddPitch(90, "Right_Lower_Arm", "Right_Weapon")),
                spin1r_a = (model => model
            .Translate(10, 0, 0, "Right_Weapon")
            .AddYaw(20, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(90, "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(70, "Right_Upper_Arm")
            .AddPitch(-60, "Right_Weapon")
            .AddSpread("Right_Weapon", 25f, 0f, -5f, 253 - 44 * 4)),
                spin1r_b = (model => model
            .Translate(10, 0, 0, "Right_Weapon")
            .AddYaw(20, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(90, "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(70, "Right_Upper_Arm")
            .AddPitch(180, "Right_Weapon")
            .AddSpread("Right_Weapon", 25f, 0f, -5f, 253 - 44 * 4)),
                spin1r_c = (model => model
            .Translate(10, 0, 0, "Right_Weapon")
            .AddYaw(20, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(90, "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(70, "Right_Upper_Arm")
            .AddPitch(60, "Right_Weapon")
            .AddSpread("Right_Weapon", 25f, 0f, -5f, 253 - 44 * 4)),
                spin2r = (model => model
            .Translate(10, 0, 0, "Right_Weapon")
            .AddYaw(350, "Right_Upper_Arm", "Right_Lower_Arm", "Right_Weapon")
            .AddPitch(10, "Right_Weapon")
            .AddPitch(75, "Right_Upper_Arm", "Right_Lower_Arm")
            .AddSpread("Right_Weapon", -15f, 0f, 5f, 253 - 44 * 4));

            /*
            Model hero_sword = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Longsword", patterns: leather);

            processVoxGiantWModel("Hero_Leather_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});

            hero_sword = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Longsword", patterns: chain);

            processVoxGiantWModel("Hero_Chain_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});

            hero_sword = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Longsword");

            processVoxGiantWModel("Hero_Plate_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
                */
            /*
            Model hero_mace = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Mace", patterns: leather);

            processVoxGiantWModel("Hero_Leather_Mace", true, 0, hero_mace,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.65f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.5f },
                new float[] { 2, 0, 1.0f },});

            hero_mace = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Mace", patterns: chain);

            processVoxGiantWModel("Hero_Chain_Mace", true, 0, hero_mace,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.65f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.5f },
                new float[] { 2, 0, 1.0f },});

            hero_mace = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Mace");

            processVoxGiantWModel("Hero_Plate_Mace", true, 0, hero_mace,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.65f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.5f },
                new float[] { 2, 0, 1.0f },});

            Model hero_dagger = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Dagger", patterns: leather);

            processVoxGiantWModel("Hero_Leather_Dagger", true, 0, hero_dagger,
                new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.5f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.5f },
                new float[] { 1, 2, 1.0f },
                new float[] { 3, 2, 0.35f },
                new float[] { 3, 2, 0.1f },
                new float[] { 3, 2, 0.45f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.35f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});

            hero_dagger = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Dagger", patterns: chain);

            processVoxGiantWModel("Hero_Chain_Dagger", true, 0, hero_dagger,
                new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.5f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.5f },
                new float[] { 1, 2, 1.0f },
                new float[] { 3, 2, 0.35f },
                new float[] { 3, 2, 0.1f },
                new float[] { 3, 2, 0.45f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.35f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});

            hero_dagger = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Dagger");

            processVoxGiantWModel("Hero_Plate_Dagger", true, 0, hero_dagger,
                new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.5f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.5f },
                new float[] { 1, 2, 1.0f },
                new float[] { 3, 2, 0.35f },
                new float[] { 3, 2, 0.1f },
                new float[] { 3, 2, 0.45f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.35f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});


            Model hero_staff = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Staff", patterns: leather);

            processVoxGiantWModel("Hero_Leather_Staff", true, 0, hero_staff,
                new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.7f },
                new float[] { 1, 2, 0.1f },
                new float[] { 1, 2, 0.6f },
                new float[] { 2, 3, 0.2f },
                new float[] { 2, 3, 0.9f },
                new float[] { 3, 1, 0.7f },
                new float[] { 1, 4, 0.6f },
                new float[] { 1, 4, 0.9f },
                new float[] { 4, 0, 0.2f },
                new float[] { 4, 0, 0.6f },
                new float[] { 4, 0, 1.0f },});

            hero_staff = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Staff", patterns: chain);

            processVoxGiantWModel("Hero_Chain_Staff", true, 0, hero_staff,
                new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.7f },
                new float[] { 1, 2, 0.1f },
                new float[] { 1, 2, 0.6f },
                new float[] { 2, 3, 0.2f },
                new float[] { 2, 3, 0.9f },
                new float[] { 3, 1, 0.7f },
                new float[] { 1, 4, 0.6f },
                new float[] { 1, 4, 0.9f },
                new float[] { 4, 0, 0.2f },
                new float[] { 4, 0, 0.6f },
                new float[] { 4, 0, 1.0f },});

            hero_staff = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Staff");

            processVoxGiantWModel("Hero_Plate_Staff", true, 0, hero_staff,
                new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.7f },
                new float[] { 1, 2, 0.1f },
                new float[] { 1, 2, 0.6f },
                new float[] { 2, 3, 0.2f },
                new float[] { 2, 3, 0.9f },
                new float[] { 3, 1, 0.7f },
                new float[] { 1, 4, 0.6f },
                new float[] { 1, 4, 0.9f },
                new float[] { 4, 0, 0.2f },
                new float[] { 4, 0, 0.6f },
                new float[] { 4, 0, 1.0f },});


            Model hero_bow = Model.Humanoid(body: "Human_Male_Leather", left_weapon: "Bow", patterns: leather);

            processVoxGiantWModel("Hero_Leather_Bow", true, 0, hero_bow,
                new Pose[] { bow0, bow1, bow2 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.4f },
                new float[] { 2, 0, 0.8f },
                new float[] { 2, 0, 1.0f },});

            hero_bow = Model.Humanoid(body: "Human_Male_Chain", left_weapon: "Bow", patterns: chain);

            processVoxGiantWModel("Hero_Chain_Bow", true, 0, hero_bow,
                new Pose[] { bow0, bow1, bow2 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.4f },
                new float[] { 2, 0, 0.8f },
                new float[] { 2, 0, 1.0f },});

            hero_bow = Model.Humanoid(body: "Human_Male_Plate", left_weapon: "Bow");


            processVoxGiantWModel("Hero_Plate_Bow", true, 0, hero_bow,
                new Pose[] { bow0, bow1, bow2 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.4f },
                new float[] { 2, 0, 0.8f },
                new float[] { 2, 0, 1.0f },});
            */

            /*
            Model hero_sword = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Longsword", patterns: leather);
            
            processVoxGiantWModel("Hero_Leather_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
            
            processUnitMassiveWModel("Hero_Leather_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
            
            hero_sword = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Longsword", patterns: chain);
            
            processVoxGiantWModel("Hero_Chain_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
            
            processUnitMassiveWModel("Hero_Chain_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
                
            hero_sword = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Longsword");
            
            processVoxGiantWModel("Hero_Plate_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
            /*
            processUnitMassiveWModel("Hero_Plate_Sword", true, 0, hero_sword,
                new Pose[] { swing0r, swing1r, swing2r },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.3f },
                new float[] { 0, 1, 0.55f },
                new float[] { 0, 1, 0.75f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 0.35f },
                new float[] { 1, 2, 0.7f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.3f },
                new float[] { 2, 0, 0.65f },
                new float[] { 2, 0, 1.0f },});
            */

            /*
Model hero_sword = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Longsword", patterns: leather);

processUnitLargeWModel("Hero_Leather_Sword", true, 0, hero_sword,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.3f },
    new float[] { 0, 1, 0.55f },
    new float[] { 0, 1, 0.75f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.7f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.3f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});

hero_sword = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Longsword", patterns: chain);

processUnitLargeWModel("Hero_Chain_Sword", true, 0, hero_sword,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.3f },
    new float[] { 0, 1, 0.55f },
    new float[] { 0, 1, 0.75f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.7f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.3f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});

hero_sword = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Longsword");

processUnitLargeWModel("Hero_Plate_Sword", true, 0, hero_sword,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.3f },
    new float[] { 0, 1, 0.55f },
    new float[] { 0, 1, 0.75f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.7f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.3f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});

Model hero_mace = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Mace", patterns: leather);

processUnitLargeWModel("Hero_Leather_Mace", true, 0, hero_mace,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.65f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.5f },
    new float[] { 2, 0, 1.0f },});

hero_mace = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Mace", patterns: chain);

processUnitLargeWModel("Hero_Chain_Mace", true, 0, hero_mace,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.65f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.5f },
    new float[] { 2, 0, 1.0f },});

hero_mace = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Mace");

processUnitLargeWModel("Hero_Plate_Mace", true, 0, hero_mace,
    new Pose[] { swing0r, swing1r, swing2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.35f },
    new float[] { 1, 2, 0.65f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.5f },
    new float[] { 2, 0, 1.0f },});

Model hero_dagger = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Dagger", patterns: leather);

processUnitLargeWModel("Hero_Leather_Dagger", true, 0, hero_dagger,
    new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.5f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.5f },
    new float[] { 1, 2, 1.0f },
    new float[] { 3, 2, 0.35f },
    new float[] { 3, 2, 0.1f },
    new float[] { 3, 2, 0.45f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.35f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});

hero_dagger = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Dagger", patterns: chain);

processUnitLargeWModel("Hero_Chain_Dagger", true, 0, hero_dagger,
    new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.5f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.5f },
    new float[] { 1, 2, 1.0f },
    new float[] { 3, 2, 0.35f },
    new float[] { 3, 2, 0.1f },
    new float[] { 3, 2, 0.45f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.35f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});

hero_dagger = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Dagger");

processUnitLargeWModel("Hero_Plate_Dagger", true, 0, hero_dagger,
    new Pose[] { stab0r, stab1r, stab2r, stab1r_alt },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.5f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 0.5f },
    new float[] { 1, 2, 1.0f },
    new float[] { 3, 2, 0.35f },
    new float[] { 3, 2, 0.1f },
    new float[] { 3, 2, 0.45f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.35f },
    new float[] { 2, 0, 0.65f },
    new float[] { 2, 0, 1.0f },});


Model hero_staff = Model.Humanoid(body: "Human_Male_Leather", right_weapon: "Staff", patterns: leather);

processUnitLargeWModel("Hero_Leather_Staff", true, 0, hero_staff,
    new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.7f },
    new float[] { 1, 2, 0.1f },
    new float[] { 1, 2, 0.6f },
    new float[] { 2, 3, 0.2f },
    new float[] { 2, 3, 0.9f },
    new float[] { 3, 1, 0.7f },
    new float[] { 1, 4, 0.6f },
    new float[] { 1, 4, 0.9f },
    new float[] { 4, 0, 0.2f },
    new float[] { 4, 0, 0.6f },
    new float[] { 4, 0, 1.0f },});

hero_staff = Model.Humanoid(body: "Human_Male_Chain", right_weapon: "Staff", patterns: chain);

processUnitLargeWModel("Hero_Chain_Staff", true, 0, hero_staff,
    new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.7f },
    new float[] { 1, 2, 0.1f },
    new float[] { 1, 2, 0.6f },
    new float[] { 2, 3, 0.2f },
    new float[] { 2, 3, 0.9f },
    new float[] { 3, 1, 0.7f },
    new float[] { 1, 4, 0.6f },
    new float[] { 1, 4, 0.9f },
    new float[] { 4, 0, 0.2f },
    new float[] { 4, 0, 0.6f },
    new float[] { 4, 0, 1.0f },});

hero_staff = Model.Humanoid(body: "Human_Male_Plate", right_weapon: "Staff");

processUnitLargeWModel("Hero_Plate_Staff", true, 0, hero_staff,
    new Pose[] { spin0r, spin1r_a, spin1r_b, spin1r_c, spin2r },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.7f },
    new float[] { 1, 2, 0.1f },
    new float[] { 1, 2, 0.6f },
    new float[] { 2, 3, 0.2f },
    new float[] { 2, 3, 0.9f },
    new float[] { 3, 1, 0.7f },
    new float[] { 1, 4, 0.6f },
    new float[] { 1, 4, 0.9f },
    new float[] { 4, 0, 0.2f },
    new float[] { 4, 0, 0.6f },
    new float[] { 4, 0, 1.0f },});


Model hero_bow = Model.Humanoid(body: "Human_Male_Leather", left_weapon: "Bow", patterns: leather);

processUnitLargeWModel("Hero_Leather_Bow", true, 0, hero_bow,
    new Pose[] { bow0, bow1, bow2 },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.4f },
    new float[] { 2, 0, 0.8f },
    new float[] { 2, 0, 1.0f },});

hero_bow = Model.Humanoid(body: "Human_Male_Chain", left_weapon: "Bow", patterns: chain);

processUnitLargeWModel("Hero_Chain_Bow", true, 0, hero_bow,
    new Pose[] { bow0, bow1, bow2 },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.4f },
    new float[] { 2, 0, 0.8f },
    new float[] { 2, 0, 1.0f },});

hero_bow = Model.Humanoid(body: "Human_Male_Plate", left_weapon: "Bow");


processUnitLargeWModel("Hero_Plate_Bow", true, 0, hero_bow,
    new Pose[] { bow0, bow1, bow2 },
    new float[][] {
    new float[] { 0, 1, 0.0f },
    new float[] { 0, 1, 0.2f },
    new float[] { 0, 1, 0.4f },
    new float[] { 0, 1, 0.6f },
    new float[] { 0, 1, 0.8f },
    new float[] { 0, 1, 0.9f },
    new float[] { 0, 1, 1.0f },
    new float[] { 0, 1, 1.0f },
    new float[] { 1, 2, 1.0f },
    new float[] { 2, 0, 0.4f },
    new float[] { 2, 0, 0.8f },
    new float[] { 2, 0, 1.0f },});
*/

            /*
            Model dude = Model.Humanoid(left_weapon: "Bow"),
                orc_assassin = Model.Humanoid(body: "Orc", face: "Mask", left_weapon: "Bow", patterns: leather);
            
            processUnitLargeWModel("Orc_Assassin", true, 10, orc_assassin,
                new Pose[] { bow0, bow1, bow2 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.4f },
                new float[] { 2, 0, 0.8f },
                new float[] { 2, 0, 1.0f },});



            processUnitLargeWModel("Dude_Bow", true, 0, dude,
                new Pose[] { bow0, bow1, bow2 },
                new float[][] {
                new float[] { 0, 1, 0.0f },
                new float[] { 0, 1, 0.2f },
                new float[] { 0, 1, 0.4f },
                new float[] { 0, 1, 0.6f },
                new float[] { 0, 1, 0.8f },
                new float[] { 0, 1, 0.9f },
                new float[] { 0, 1, 1.0f },
                new float[] { 0, 1, 1.0f },
                new float[] { 1, 2, 1.0f },
                new float[] { 2, 0, 0.4f },
                new float[] { 2, 0, 0.8f },
                new float[] { 2, 0, 1.0f },});
            */


            /*
            Model m = new Model(new Dictionary<string, Bone> { { "Left_Weapon", Bone.readBone("Longsword") } });
            Pose poseSword = (model => model.AddPitch(30, "Left_Weapon")
            .AddYaw(20, "Left_Weapon")
            .AddPitch(-70, "Left_Weapon")
            .AddSpread("Left_Weapon", 80f, 30f, 0f, 253 - 12 * 4));
            
            byte[,,] by = poseSword(m).Bones["Left_Weapon"].Finalize(10 * multiplier * bonus, 0);
            by = TransformLogic.Translate(by, 10 * multiplier * bonus, 10 * multiplier * bonus, 0);
            for(int d = 0; d < 4; d++)
            {
                byte[,,] colors = TransformLogic.SealGaps(TransformLogic.RunCA(TransformLogic.Shrink(TransformLogic.RotateYawPartial(by, 90 * d), bonus), 1 + bonus/2));

                byte[][] b = processFrameLargeW(colors, 0, 0, 1, true, false);

                ImageInfo imi = new ImageInfo(widthLarge, heightLarge, 8, false, false, true);
                PngWriter png = FileHelper.CreatePngWriter(altFolder + "/palette" + 0 + "_" + "Just_Sword" + "_Ortho_face" + d + "_" + 0 + ".png", imi, true);
                WritePNG(png, b, simplepalettes[0]);
            }


            Directory.CreateDirectory("gifs/" + altFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo(@"convert.exe");
            startInfo.UseShellExecute = false;
            string s = "";

            s = altFolder + "/palette" + 0 + "_" + "Just_Sword" + "_Ortho_face* ";
            startInfo.Arguments = "-dispose background -delay 150 -loop 0 " + s + " gifs/" + altFolder + "palette" + 0 + "_" + "Just_Sword" + "_Ortho_animated.gif";
            Process.Start(startInfo).WaitForExit();
            */
            /*
            processUnitManyTinyW("Human_Male", true, false);
            processUnitManyTinyW("Human_Female", true, false);
            processUnitManyTinyW("Mage_Male", true, false);
            processUnitManyTinyW("Mage_Female", true, false);
            processUnitManyTinyW("Rogue_Male", true, false);
            processUnitManyTinyW("Rogue_Female", true, false);
            processUnitManyTinyW("Priest_Male", true, false);
            processUnitManyTinyW("Priest_Female", true, false);
            processUnitManyTinyW("Warrior_Male", true, false);
            processUnitManyTinyW("Warrior_Female", true, false);
            */
        }
    }
}
