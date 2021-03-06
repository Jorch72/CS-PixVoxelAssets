﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AssetsPV
{
    public class Bone
    {
        public static int Multiplier = OrthoSingle.multiplier, Bonus = OrthoSingle.bonus;
        public const float DegreesToRadians = (float)(Math.PI / 180), RadiansToDegrees = (float)(180 / Math.PI);
        private PRNG r = PRNG.r;
        public byte[,,] Colors;
        public string Name;

        public float Roll = 0f, Pitch = 0f, Yaw = 0f, StartRoll = 0f, StartPitch = 0f, StartYaw = 0f,
            MoveX = 0f, MoveY = 0f, MoveZ = 0f, ZeroOutX = 0f, ZeroOutY = 0f, ZeroOutZ = 0f,
            StretchX = 1f, StretchY = 1f, StretchZ = 1f, SpreadChance = 1f;
        public bool SpreadAll = false;

        public static float Lerp(float start, float end, float alpha)
        {
            return (end - start < -180) ? CCW(start, end, alpha) : CW(start, end, alpha);
        }
        public static float CW(float start, float end, float alpha)
        {
            float e = (1080 + end) % 360, s = (end - start > 180) ? 360 + (1080 + start) % 360 : (1080 + start) % 360,
                v = (1080 + s + (e - s) * alpha) % 360;
            if(Math.Abs((s - v) * (s - v) - (e - v) * (e - v)) < 90)
                return (1080 + s + (e - s) * 0.5f) % 360;
            else return v;
        }
        public static float CCW(float start, float end, float alpha)
        {
            float s = (1080 + start) % 360, e = (end - start < -180) ? 360 + (1080 + end) % 360 : (1080 + end) % 360,
                v = (1080 + s + (e - s) * alpha) % 360;
            if(Math.Abs((s - v) * (s - v) - (e - v) * (e - v)) < 90)
                return (1080 + s + (e - s) * 0.5f) % 360;
            else return v;

        }
        /*
         * @param yaw the rotation around the y axis in radians
         * @param pitch the rotation around the x axis in radians
         * @param roll the rotation around the z axis in radians
         * @return this quaternion */
        //private static Vector3GDX YawAxis = new Vector3GDX(0f, 1f, 0f), PitchAxis = new Vector3GDX(1f, 0f, 0f), RollAxis = new Vector3GDX(0f, 0f, 1f);
        public byte[] SpreadColors = null;

        public Bone(string name, byte[,,] c)
        {
            Name = name;
            Colors = c;
        }
        public Bone(string name, byte[,,] colors, float yaw, float pitch, float roll)
        {
            Name = name;
            this.Colors = colors;
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.Roll = roll;
        }
        public Bone InitSpread(byte[] spreadColors, float yaw_back, float pitch_back, float roll_back)
        {
            SpreadColors = spreadColors;
            StartYaw = yaw_back;
            StartPitch = pitch_back;
            StartRoll = roll_back;
            return this;
        }
        public Bone InitFierySpreadCU(float yaw_back, float pitch_back, float roll_back, float spread_chance)
        {
            if(spread_chance <= 0)
            {
                SpreadColors = null;
                SpreadAll = false;
                SpreadChance = 0;
                StartYaw = 0;
                StartPitch = 0;
                StartRoll = 0;
            }
            else
            {
                SpreadColors = new byte[] { 253 - 18 * 4, 253 - 18 * 4, 253 - 17 * 4, 253 - 19 * 4 };
                SpreadAll = true;
                SpreadChance = spread_chance;
                StartYaw = yaw_back;
                StartPitch = pitch_back;
                StartRoll = roll_back;
            }
            return this;
        }
        public Bone InitStretch(float stretchX, float stretchY, float stretchZ)
        {
            StretchX = stretchX;
            StretchY = stretchY;
            StretchZ = stretchZ;
            return this;
        }
        public Bone ZeroOut(float zeroX, float zeroY, float zeroZ)
        {
            ZeroOutX = zeroX;
            ZeroOutY = zeroY;
            ZeroOutZ = zeroZ;
            return this;
        }
        public Bone Translate(float moveX, float moveY, float moveZ)
        {
            MoveX += moveX;
            MoveY += moveY;
            MoveZ += moveZ;
            return this;
        }

        public Bone Reset()
        {
            Roll = 0f;
            Pitch = 0f;
            Yaw = 0f;
            StartRoll = 0f;
            StartPitch = 0f;
            StartYaw = 0f;
            MoveX = 0f;
            MoveY = 0f;
            MoveZ = 0f;
            StretchX = 1f;
            StretchY = 1f;
            StretchZ = 1f;
            SpreadColors = null;
            return this;
        }

        public byte[,,] FinalizeSimple(int xOffset, int yOffset)
        {
            //q.setEulerAngles(360 - Yaw, 360 - Pitch, 360 - Roll);
            //            q = q.idt().slerp(new Quaternion[] { new Quaternion(YawAxis, (360 - Pitch) % 360f), new Quaternion(PitchAxis, (360 - Roll) % 360f), new Quaternion(RollAxis, (360 - Yaw) % 360f) });
            //QuaternionGDX q = new QuaternionGDX().mulLeft(new QuaternionGDX(YawAxis, (360 - Pitch) % 360f)).mulLeft(new QuaternionGDX(PitchAxis, (360 - Roll) % 360f)).mulLeft(new QuaternionGDX(RollAxis, (360 - Yaw) % 360f));
            Quaternion q = Extensions.Euler(Yaw, Pitch, Roll);

            //dir = dir.fromAngles(360 - Yaw, 360 - Pitch, 360 - Roll);
            int xSize = Colors.GetLength(0), ySize = Colors.GetLength(1), zSize = Colors.GetLength(2),
                hxs = xSize / 2, hys = ySize / 2, hzs = zSize / 2;
            Vector3 v = new Vector3(0f, 0f, 0f);

            byte[,,] c2 = new byte[xSize, ySize, zSize];

            float zx = 0, zy = 0, zz = 0;
            {
                v.X = (ZeroOutX - hxs) + xOffset;
                v.Y = (ZeroOutY - hys);
                v.Z = (ZeroOutZ - hzs);
                float normod = v.Length();
                v = Vector3.Divide(v, normod);
                v = Vector3.Transform(v, q);
                v = Vector3.Multiply(normod, v);
                zz = (int)(v.Z + 0.5f + hzs + MoveZ);
                zx = 0;
                zy = 0;
                if(ZeroOutZ == 0) zz = 0;
            }
            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(Colors[x, y, z] > 0)
                        {
                            /*
                            for(double sx = 1.0; sx <= 1.00; sx += 0.02)
                            {
                                for(double sy = 0.94; sy <= 1.06; sy += 0.06)
                                {
                                    for(double sz = 0.94; sz <= 1.06; sz += 0.06)
                                    {*/
                            v.X = (x - hxs) * StretchX + xOffset;
                            v.Y = (y - hys) * StretchY;
                            v.Z = (z - hzs) * StretchZ;
                            float normod = v.Length();
                            v = Vector3.Divide(v, normod);
                            v = Vector3.Transform(v, q);
                            v = Vector3.Multiply(normod, v);

                            int x2 = (int)(v.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v.Z + 0.5f + hzs + MoveZ - zz);
                            if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                c2[x2, y2, z2] = Colors[x, y, z];
                            /*
                            if(StretchZ == 1.0) break;
                        }
                        if(StretchY == 1.0) break;
                    }
                    if(StretchX == 1.0) break;
                }*/
                            //int x2, y2, z2;
                            //float stretchiest = 2f;
                            //for(float sf = stretchiest; sf >= -2f; sf -= 0.5f)
                            //{
                            //    x2 = (int)(v.x + 0.5 + sf * Math.Max(StretchX - 1.0, 0) + hxs - xOffset + MoveX);
                            //    y2 = (int)(v.y + 0.5 + sf * Math.Max(StretchY - 1.0, 0) + hys + MoveY);
                            //    z2 = (int)(v.z + 0.5 + sf * Math.Max(StretchZ - 1.0, 0) + hzs + MoveZ);

                            //    if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                            //    {
                            //        c2[x2, y2, z2] = Colors[x, y, z];
                            //    }
                            //}
                        }
                    }
                }
            }

            for(int z = 1; z < zSize - 1; z++)
            {
                for(int y = 1; y < ySize - 1; y++)
                {
                    for(int x = 1; x < xSize - 1; x++)
                    {
                        if(c2[x, y, z] != 0) continue;
                        if(c2[x - 1, y, z] != 0 && c2[x - 1, y, z] == c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && c2[x, y - 1, z] == c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && c2[x, y, z - 1] == c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                        if(c2[x - 1, y, z] != 0 && 0 != c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1 + r.Next(2) * 2, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && 0 != c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1 + r.Next(2) * 2, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && 0 != c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                    }
                }
            }
            return c2;
        }

        public byte[,,] Finalize(int xOffset, int yOffset)
        {
            if(SpreadColors == null)
                return FinalizeSimple(xOffset, yOffset);

            //q.setEulerAngles(360 - Yaw, 360 - Pitch, 360 - Roll);
            //QuaternionGDX q = new QuaternionGDX().mulLeft(new QuaternionGDX(YawAxis, (360 - Pitch) % 360f)).mulLeft(new QuaternionGDX(PitchAxis, (360 - Roll) % 360f)).mulLeft(new QuaternionGDX(RollAxis, (360 - Yaw) % 360f));
            //Quaternion q = Quaternion.CreateFromYawPitchRoll(-Pitch * DegreesToRadians, -Roll * DegreesToRadians, -Yaw * DegreesToRadians);
            Quaternion q = Extensions.Euler(Yaw, Pitch, Roll);
            //q = q.idt().slerp(new Quaternion[] { new Quaternion(RollAxis, (360 - Roll) % 360f), new Quaternion(PitchAxis, (360 - Pitch) % 360f), new Quaternion(YawAxis, (360 - Yaw) % 360f) });

            //Quaternion q0 = new Quaternion().setEulerAngles((StartYaw - Yaw + 360) % 360, (StartPitch - Pitch + 360) % 360, (StartRoll - Roll + 360) % 360), q2;
            //Quaternion q0 = Quaternion.CreateFromYawPitchRoll(-StartPitch * DegreesToRadians, -StartRoll * DegreesToRadians, -StartYaw * DegreesToRadians), q2;
            Quaternion q0 = q.MulLeft(Extensions.Euler(StartYaw, StartPitch, StartRoll)), q2;


            float distance = Math.Abs(Yaw + StartYaw) % 360 + Math.Abs(Pitch + StartPitch) % 360 + Math.Abs(Roll + StartRoll) % 360;

            float levels = Math.Max(distance / 2.5f, 1f);

            int xSize = Colors.GetLength(0), ySize = Colors.GetLength(1), zSize = Colors.GetLength(2),
                hxs = xSize / 2, hys = ySize / 2, hzs = zSize / 2;
            Vector3 v = new Vector3(0f, 0f, 0f), v2;

            byte[,,] c2 = new byte[xSize, ySize, zSize];

            float zx = 0, zy = 0, zz = 0;
            {

                v.X = (ZeroOutX - hxs) + xOffset;
                v.Y = (ZeroOutY - hys);
                v.Z = (ZeroOutZ - hzs);
                float normod = v.Length();
                v = Vector3.Divide(v, normod);
                v = Vector3.Transform(v, q);
                v = Vector3.Multiply(normod, v);
                zz = (int)(v.Z + 0.5f + hzs + MoveZ);
                zx = 0;
                zy = 0;
                if(ZeroOutZ == 0) zz = 0;
            }

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(Colors[x, y, z] > 0)
                        {
                            /*for(double sx = 0.94; sx <= 1.06; sx += 0.06)
                            {
                                for(double sy = 0.94; sy <= 1.06; sy += 0.06)
                                {
                                    for(double sz = 0.94; sz <= 1.06; sz += 0.06)
                                    {*/
                            v.X = (x - hxs) * StretchX + xOffset;
                            v.Y = (y - hys) * StretchY;
                            v.Z = (z - hzs) * StretchZ;
                            float normod = (float)Math.Sqrt(v.LengthSquared());
                            v = Vector3.Divide(v, normod);
                            v = Vector3.Transform(v, q);
                            v = Vector3.Multiply(normod, v);

                            int x2 = (int)(v.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v.Z + 0.5f + hzs + MoveZ - zz);
                            if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                c2[x2, y2, z2] = Colors[x, y, z];
                            /*
                            if(StretchZ == 1.0) break;
                        }
                        if(StretchY == 1.0) break;
                    }
                    if(StretchX == 1.0) break;
                }
                */
                            //v.x = (x - hxs) * StretchX + xOffset;
                            //v.y = (y - hys) * StretchY;
                            //v.z = (z - hzs) * StretchZ;
                            //float normod = (float)Math.Sqrt(v.len2());
                            //v.scl(1f / normod);
                            //q.transform(v);
                            //v.scl(normod);

                            ////int x2 = (int)(v.x + 0.5f), y2 = (int)(v.y + 0.5f), z2 = (int)(v.z + 0.5f);

                            //int x2, y2, z2;
                            //float stretchiest = 2f;
                            //for(float sf = stretchiest; sf >= -2f; sf -= 0.5f)
                            //{
                            //    x2 = (int)(v.x + 0.5 + sf * Math.Max(StretchX - 1.0, 0) + hxs - xOffset + MoveX);
                            //    y2 = (int)(v.y + 0.5 + sf * Math.Max(StretchY - 1.0, 0) + hys + MoveY);
                            //    z2 = (int)(v.z + 0.5 + sf * Math.Max(StretchZ - 1.0, 0) + hzs + MoveZ);

                            //    if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                            //    {
                            //        c2[x2, y2, z2] = Colors[x, y, z];
                            //    }
                            //}

                        }
                    }
                }
            }

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(SpreadAll)
                        {
                            v.X = (x - hxs) * StretchX + xOffset;
                            v.Y = (y - hys) * StretchY;
                            v.Z = (z - hzs) * StretchZ;
                            float normod = (float)Math.Sqrt(v.LengthSquared());
                            v = Vector3.Divide(v, normod);

                            for(float a = 0.0f; a <= 1.0f; a += 1.0f / levels)
                            {
                                q2 = Quaternion.Lerp(q, q0, a);
                                v2 = Vector3.Multiply(normod, Vector3.Transform(v, q2));
                                int x2 = (int)(v2.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v2.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v2.Z + 0.5f + hzs + MoveZ - zz);

                                if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && c2[x2, y2, z2] == 0 && SpreadChance > TransformLogic.rng.NextDouble())
                                    c2[x2, y2, z2] = SpreadColors.RandomElement();
                            }

                        }
                        else
                        {
                            for(int c = 0; c < SpreadColors.Length; c++)
                            {
                                if(Colors[x, y, z] == SpreadColors[c])
                                {
                                    v.X = (x - hxs) * StretchX + xOffset;
                                    v.Y = (y - hys) * StretchY;
                                    v.Z = (z - hzs) * StretchZ;
                                    float normod = (float)Math.Sqrt(v.LengthSquared());
                                    v = Vector3.Divide(v, normod);

                                    for(float a = 0.0f; a <= 1.0f; a += 1.0f / levels)
                                    {
                                        q2 = Quaternion.Lerp(q, q0, a);
                                        v2 = Vector3.Multiply(normod, Vector3.Transform(v, q2));
                                        int x2 = (int)(v2.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v2.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v2.Z + 0.5f + hzs + MoveZ - zz);

                                        if(x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize && y2 < ySize && z2 < zSize && c2[x2, y2, z2] == 0 && SpreadChance > TransformLogic.rng.NextDouble())
                                            c2[x2, y2, z2] = Colors[x, y, z];
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            for(int z = 1; z < zSize - 1; z++)
            {
                for(int y = 1; y < ySize - 1; y++)
                {
                    for(int x = 1; x < xSize - 1; x++)
                    {
                        if(c2[x, y, z] != 0) continue;
                        if(c2[x - 1, y, z] != 0 && c2[x - 1, y, z] == c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && c2[x, y - 1, z] == c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && c2[x, y, z - 1] == c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                        if(c2[x - 1, y, z] != 0 && 0 != c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1 + r.Next(2) * 2, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && 0 != c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1 + r.Next(2) * 2, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && 0 != c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                    }
                }
            }
            return c2;
        }
        public byte[,,] FinalizeScatter(int xOffset, int yOffset)
        {
            //if(SpreadColors == null)
            //    return FinalizeSimple(xOffset, yOffset);

            //q.setEulerAngles(360 - Yaw, 360 - Pitch, 360 - Roll);
            //QuaternionGDX q = new QuaternionGDX().mulLeft(new QuaternionGDX(YawAxis, (360 - Pitch) % 360f)).mulLeft(new QuaternionGDX(PitchAxis, (360 - Roll) % 360f)).mulLeft(new QuaternionGDX(RollAxis, (360 - Yaw) % 360f));
            //Quaternion q = Quaternion.CreateFromYawPitchRoll(-Pitch * DegreesToRadians, -Roll * DegreesToRadians, -Yaw * DegreesToRadians);
            Quaternion q = Extensions.Euler(Yaw, Pitch, Roll);
            //q = q.idt().slerp(new Quaternion[] { new Quaternion(RollAxis, (360 - Roll) % 360f), new Quaternion(PitchAxis, (360 - Pitch) % 360f), new Quaternion(YawAxis, (360 - Yaw) % 360f) });

            //Quaternion q0 = new Quaternion().setEulerAngles((StartYaw - Yaw + 360) % 360, (StartPitch - Pitch + 360) % 360, (StartRoll - Roll + 360) % 360), q2;
            //Quaternion q0 = Quaternion.CreateFromYawPitchRoll(-StartPitch * DegreesToRadians, -StartRoll * DegreesToRadians, -StartYaw * DegreesToRadians), q2;
            Quaternion q0 = q.MulLeft(Extensions.Euler(StartYaw, StartPitch, StartRoll)), q2;


            float distance = Math.Abs(Yaw + StartYaw) % 360 + Math.Abs(Pitch + StartPitch) % 360 + Math.Abs(Roll + StartRoll) % 360;

            float levels = Math.Max(distance / 2.5f, 1f);

            int xSize = Colors.GetLength(0), ySize = Colors.GetLength(1), zSize = Colors.GetLength(2),
                hxs = xSize / 2, hys = ySize / 2, hzs = zSize / 2;
            Vector3 v = new Vector3(0f, 0f, 0f), v2;

            byte[,,] c2 = new byte[xSize, ySize, zSize];

            float zx = 0, zy = 0, zz = 0;
            {

                v.X = (ZeroOutX - hxs) + xOffset;
                v.Y = (ZeroOutY - hys);
                v.Z = (ZeroOutZ - hzs);
                float normod = v.Length();
                v = Vector3.Divide(v, normod);
                v = Vector3.Transform(v, q);
                v = Vector3.Multiply(normod, v);
                zz = (int)(v.Z + 0.5f + hzs + MoveZ);
                zx = 0;
                zy = 0;
                if(ZeroOutZ == 0) zz = 0;
            }

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(Colors[x, y, z] > 0)
                        {
                            v.X = (x - hxs) * StretchX + xOffset;
                            v.Y = (y - hys) * StretchY;
                            v.Z = (z - hzs) * StretchZ;
                            float normod = (float)Math.Sqrt(v.LengthSquared());
                            v = Vector3.Divide(v, normod);
                            v = Vector3.Transform(v, q);
                            v = Vector3.Multiply(normod, v);

                            int x2 = (int)(v.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v.Z + 0.5f + hzs + MoveZ - zz);
                            if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize)
                            {
                                if(z2 >= 0 && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                {
                                    c2[x2, y2, z2] = Colors[x, y, z];
                                }
                                else if(z2 < 0)
                                {
                                    double xMove = x2 - xSize / 2.0, yMove = y2 - ySize / 2.0;
                                    double mag = Math.Sqrt(xMove * xMove + yMove * yMove);

                                    xMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;
                                    yMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;

                                    x2 += (int)Math.Round(xMove);
                                    y2 += (int)Math.Round(yMove);

                                    z2 = TransformLogic.rng.Next(3);
                                    if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                    {
                                        c2[x2, y2, z2] = Colors[x, y, z];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if(SpreadColors != null)
            {
                for(int z = 0; z < zSize; z++)
                {
                    for(int y = 0; y < ySize; y++)
                    {
                        for(int x = 0; x < xSize; x++)
                        {
                            if(SpreadAll)
                            {
                                if(Colors[x, y, z] > 0)
                                {
                                    if(x > 0 && x < xSize - 1 && y > 0 && y < ySize - 1 && z > 0 && z < zSize - 1 &&
                                        Colors[x - 1, y, z] > 0 && Colors[x + 1, y, z] > 0 &&
                                        Colors[x, y - 1, z] > 0 && Colors[x, y + 1, z] > 0 &&
                                        Colors[x, y, z - 1] > 0 && Colors[x, y, z + 1] > 0)

                                        continue;

                                    v.X = (x - hxs) * StretchX + xOffset;
                                    v.Y = (y - hys) * StretchY;
                                    v.Z = (z - hzs) * StretchZ;
                                    float normod = (float)Math.Sqrt(v.LengthSquared());
                                    v = Vector3.Divide(v, normod);

                                    for(float a = 0.0f; a <= 1.0f; a += 1.0f / levels)
                                    {
                                        q2 = Quaternion.Lerp(q, q0, a);
                                        v2 = Vector3.Multiply(normod, Vector3.Transform(v, q2));

                                        int x2 = (int)(v2.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v2.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v2.Z + 0.5f + hzs + MoveZ - zz);
                                        if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize && SpreadChance > TransformLogic.rng.NextDouble())
                                        {
                                            if(z2 >= 0 && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                            {
                                                c2[x2, y2, z2] = SpreadColors.RandomElement();
                                            }
                                            else if(z2 < 0)
                                            {
                                                double xMove = x2 - xSize / 2.0, yMove = y2 - ySize / 2.0;
                                                double mag = Math.Sqrt(xMove * xMove + yMove * yMove);

                                                xMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;
                                                yMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;

                                                x2 += (int)Math.Round(xMove);
                                                y2 += (int)Math.Round(yMove);

                                                z2 = TransformLogic.rng.Next(4);
                                                if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize)
                                                {
                                                    c2[x2, y2, z2] = SpreadColors.RandomElement();
                                                }
                                            }
                                        }

                                    }
                                }
                            }
                            else
                            {
                                for(int c = 0; c < SpreadColors.Length; c++)
                                {
                                    if(Colors[x, y, z] == SpreadColors[c])
                                    {
                                        v.X = (x - hxs) * StretchX + xOffset;
                                        v.Y = (y - hys) * StretchY;
                                        v.Z = (z - hzs) * StretchZ;
                                        float normod = (float)Math.Sqrt(v.LengthSquared());
                                        v = Vector3.Divide(v, normod);

                                        for(float a = 0.0f; a <= 1.0f; a += 1.0f / levels)
                                        {
                                            q2 = Quaternion.Lerp(q, q0, a);
                                            v2 = Vector3.Multiply(normod, Vector3.Transform(v, q2));

                                            int x2 = (int)(v2.X + 0.5f + hxs - xOffset + MoveX - zx), y2 = (int)(v2.Y + 0.5f + hys + MoveY - zy), z2 = (int)(v2.Z + 0.5f + hzs + MoveZ - zz);
                                            if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize && SpreadChance > TransformLogic.rng.NextDouble())
                                            {
                                                if(z2 >= 0 && z2 < zSize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                                {
                                                    c2[x2, y2, z2] = Colors[x, y, z];
                                                }
                                                else if(z2 < 0)
                                                {
                                                    double xMove = x2 - xSize / 2.0, yMove = y2 - ySize / 2.0;
                                                    double mag = Math.Sqrt(xMove * xMove + yMove * yMove);

                                                    xMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;
                                                    yMove *= TransformLogic.rng.NextDouble() * -z2 * 2 / mag;

                                                    x2 += (int)Math.Round(xMove);
                                                    y2 += (int)Math.Round(yMove);

                                                    z2 = TransformLogic.rng.Next(3);
                                                    if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize && (254 - c2[x2, y2, z2]) % 4 != 0)
                                                    {
                                                        c2[x2, y2, z2] = Colors[x, y, z];
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for(int z = 1; z < zSize - 1; z++)
            {
                for(int y = 1; y < ySize - 1; y++)
                {
                    for(int x = 1; x < xSize - 1; x++)
                    {
                        if(c2[x, y, z] != 0) continue;
                        if(c2[x - 1, y, z] != 0 && c2[x - 1, y, z] == c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && c2[x, y - 1, z] == c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && c2[x, y, z - 1] == c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                        if(c2[x - 1, y, z] != 0 && 0 != c2[x + 1, y, z])
                        {
                            c2[x, y, z] = c2[x - 1 + r.Next(2) * 2, y, z];
                            continue;
                        }
                        if(c2[x, y - 1, z] != 0 && 0 != c2[x, y + 1, z])
                        {
                            c2[x, y, z] = c2[x, y - 1 + r.Next(2) * 2, z];
                            continue;
                        }
                        if(c2[x, y, z - 1] != 0 && 0 != c2[x, y, z + 1])
                        {
                            c2[x, y, z] = c2[x, y, z + 1];
                            continue;
                        }
                    }
                }
            }
            return c2;
        }
        public bool RoughEquals(Bone target)
        {
            return (Pitch == target.Pitch && Roll == target.Roll && Yaw == target.Yaw && StartPitch == target.StartPitch && StartRoll == target.StartRoll && StartYaw == target.StartYaw &&
                MoveX == target.MoveX && MoveY == target.MoveY && MoveZ == target.MoveZ && StretchX == target.StretchX && StretchY == target.StretchY && StretchZ == target.StretchZ);
        }
        public Bone Interpolate(Bone target, float alpha)
        {
            if(alpha <= 0.0)
                return this.Replicate();
            else if(alpha >= 1.0)
                return target.Replicate();
            else if(RoughEquals(target))
                return this.Replicate();
            //QuaternionGDX q = new QuaternionGDX().setEulerAngles(-Pitch, -Roll, -Yaw);
            //Quaternion q = new Quaternion().mulLeft(new Quaternion(YawAxis, (360 - Pitch) % 360f))
            //    .mulLeft(new Quaternion(PitchAxis, (360 - Roll) % 360f))
            //    .mulLeft(new Quaternion(RollAxis, (360 - Yaw) % 360f))
            //    .nor();

            //QuaternionGDX q0 = new QuaternionGDX().setEulerAngles(-target.Pitch, -target.Roll, -target.Yaw), q2;

            //Quaternion q0 = new Quaternion().mulLeft(new Quaternion(YawAxis, (360 - target.Pitch) % 360f))
            //    .mulLeft(new Quaternion(PitchAxis, (360 - target.Roll) % 360f))
            //    .mulLeft(new Quaternion(RollAxis, (360 - target.Yaw) % 360f))
            //    .nor(),

            Vector3 myStarts = new Vector3(StartYaw, StartPitch, StartRoll), tgtStarts = new Vector3(target.StartYaw, target.StartPitch, target.StartRoll),
                myStretches = new Vector3(StretchX, StretchY, StretchZ), tgtStretches = new Vector3(target.StretchX, target.StretchY, target.StretchZ),
                myMoves = new Vector3(MoveX, MoveY, MoveZ), tgtMoves = new Vector3(target.MoveX, target.MoveY, target.MoveZ),
                myZeroes = new Vector3(ZeroOutX, ZeroOutY, ZeroOutZ), tgtZeroes = new Vector3(target.ZeroOutX, target.ZeroOutY, target.ZeroOutZ);
            myStarts = Vector3.Lerp(myStarts, tgtStarts, alpha);
            myStretches = Vector3.Lerp(myStretches, tgtStretches, alpha);
            myMoves = Vector3.Lerp(myMoves, tgtMoves, alpha);
            myZeroes = Vector3.Lerp(myZeroes, tgtZeroes, alpha);
            byte[] newSpread = null;
            if(SpreadColors != null)
            {
                newSpread = SpreadColors;
                if(target.SpreadColors != null)
                {
                    newSpread = new byte[SpreadColors.Length + target.SpreadColors.Length];
                    for(int i = 0; i < SpreadColors.Length; i++)
                    {
                        newSpread[i] = SpreadColors[i];
                    }
                    for(int i = 0; i < target.SpreadColors.Length; i++)
                    {
                        newSpread[SpreadColors.Length + i] = target.SpreadColors[i];
                    }
                }
            }
            else if(target.SpreadColors != null)
                newSpread = target.SpreadColors;
            //Console.WriteLine(Name);
            //Console.WriteLine("q before slerp.  Yaw:" +
            //    q.Yaw() + " Pitch:" + q.Pitch() + " Roll:" + q.Roll());
            //Console.WriteLine("q0 before slerp. Yaw:" +
            //    q0.Yaw() + " Pitch:" + q0.Pitch() + " Roll:" + q0.Roll());
            //q2 = Quaternion.Slerp(q, q0, alpha);

            //Console.WriteLine("q2 after slerp.  Yaw:" +
            //    q2.Yaw() + " Pitch:" + q2.Pitch() + " Roll:" + q2.Roll());
            float y2 = Lerp(Yaw, target.Yaw, alpha), p2 = Lerp(Pitch, target.Pitch, alpha), r2 = Lerp(Roll, target.Roll, alpha);
            return new Bone(Name, Colors, y2, p2, r2)
                .InitSpread(newSpread, myStarts.Z, myStarts.Y, myStarts.X) // not a typo, this IS in ZYX order
                .InitStretch(myStretches.X, myStretches.Y, myStretches.Z)
                .Translate(myMoves.X, myMoves.Y, myMoves.Z)
                .ZeroOut(myZeroes.X, myZeroes.Y, myZeroes.Z);
        }
        public Bone Replicate()
        {
            Bone b = new Bone(Name, Colors.Replicate(), Yaw, Pitch, Roll);
            b.SpreadColors = SpreadColors.Replicate();
            b.SpreadAll = SpreadAll;
            b.StartYaw = StartYaw;
            b.StartPitch = StartPitch;
            b.StartRoll = StartRoll;
            b.StretchX = StretchX;
            b.StretchY = StretchY;
            b.StretchZ = StretchZ;
            b.MoveX = MoveX;
            b.MoveY = MoveY;
            b.MoveZ = MoveZ;
            b.ZeroOutX = ZeroOutX;
            b.ZeroOutY = ZeroOutY;
            b.ZeroOutZ = ZeroOutZ;
            return b;
        }

        public static Bone readBone(string file)
        {
            if(file == null)
                return new Bone(file, new byte[60 * Multiplier, 60 * Multiplier, 60 * Multiplier]);
            BinaryReader bin = new BinaryReader(File.Open(VoxelLogic.voxFolder + file + "_W.vox", FileMode.Open));
            List<MagicaVoxelData> raw = VoxelLogic.FromMagicaRaw(bin);
            //if(Bone.Patterns == null)
            return new Bone(file, TransformLogic.TransformStartLarge(raw, Multiplier));
            //else
            //    return new Bone(file, PatternLogic.ApplyPattern(TransformLogic.TransformStartLarge(raw, Multiplier), Bone.Patterns));
        }
    }
    public struct Connector
    {
        public string plug;
        public string socket;
        public int matchColor;
        public Connector(string plug, string socket, int matchColor)
        {
            this.plug = plug;
            this.socket = socket;
            this.matchColor = matchColor;
        }
    }
    public class Model
    {
        public Dictionary<string, Bone> Bones = new Dictionary<string, Bone>(16);
        public Connector[] Anatomy = new Connector[0];
        public Dictionary<byte, Pattern> Patterns = null;
        public Model()
        {

        }
        public Model(Dictionary<string, Bone> bones)
        {
            Bones = bones;
        }

        public Model(Dictionary<byte, Pattern> patterns)
        {
            Patterns = patterns;
        }
        public Model(Dictionary<string, Bone> bones, Dictionary<byte, Pattern> patterns)
        {
            Bones = bones;
            Patterns = patterns;
        }

        public Model AddBone(string boneName, Bone bone)
        {
            Bones[boneName] = bone;
            return this;
        }

        public Model AddBone(string boneName, byte[,,] bone)
        {
            Bones[boneName] = new Bone(boneName, bone);
            return this;
        }

        public byte[][,,] AllBones()
        {
            if(VoxelLogic.VisualMode != "CU")
                return Bones.Values.Select(b => b.Colors).ToArray();
            byte[][,,] colors = new byte[Bones.Count][,,];
            string check = "Gun";
            int n = 0, i = 0;
            while(Bones.ContainsKey(check + n))
            {
                colors[i++] = Bones[check + n++].Colors;
            }
            check = "Wheel";
            n = 0;
            while(Bones.ContainsKey(check + n))
            {
                colors[i++] = Bones[check + n++].Colors;
            }
            check = "Flesh";
            n = 0;
            while(Bones.ContainsKey(check + n))
            {
                colors[i++] = Bones[check + n++].Colors;
            }
            check = "Glass";
            n = 0;
            while(Bones.ContainsKey(check + n))
            {
                colors[i++] = Bones[check + n++].Colors;
            }
            check = "Bone";
            n = 0;
            while(Bones.ContainsKey(check + n))
            {
                colors[i++] = Bones[check + n++].Colors;
            }
            if(Bones.ContainsKey("Extra"))
            {
                //colors[i++] = Bones["Extra"].Colors;
                int xsize = colors[i - 1].GetLength(0), ysize = colors[i - 1].GetLength(1), zsize = colors[i - 1].GetLength(2);
                colors[i++] = new byte[xsize, ysize, zsize];
            }
            return colors;
        }

        public Model AddSpread(string boneName, float pitchy, float rolly, float yawy, params byte[] spreadColors)
        {
            Bones[boneName].InitSpread(spreadColors, rolly, pitchy, yawy);
            return this;
        }
        public Model AddYaw(float yaw, params string[] names)
        {
            if(names.Length == 0)
            {
                foreach(string nm in Bones.Keys)
                {
                    Bones[nm].Yaw += yaw;
                }
            }
            else
            {
                foreach(string nm in names)
                {
                    Bones[nm].Yaw += yaw;
                }
            }
            return this;
        }
        public Model AddPitch(float pitch, params string[] names)
        {
            if(names.Length == 0)
            {
                foreach(string nm in Bones.Keys)
                {
                    Bones[nm].Pitch += pitch;
                }
            }
            else
            {
                foreach(string nm in names)
                {
                    Bones[nm].Pitch += pitch;
                }
            }
            return this;
        }
        public Model AddRoll(float roll, params string[] names)
        {
            if(names.Length == 0)
            {
                foreach(string nm in Bones.Keys)
                {
                    Bones[nm].Roll += roll;
                }
            }
            else
            {
                foreach(string nm in names)
                {
                    Bones[nm].Roll += roll;
                }
            }
            return this;
        }
        public Model Translate(float x, float y, float z, params string[] names)
        {
            if(names.Length == 0)
            {
                foreach(string nm in Bones.Keys)
                {
                    Bones[nm].Translate(x, y, z);
                }
            }
            else
            {
                foreach(string nm in names)
                {
                    Bones[nm].Translate(x, y, z);
                }
            }
            return this;
        }
        public Model AddStretch(float x, float y, float z, params string[] names)
        {
            if(names.Length == 0)
            {
                foreach(string nm in Bones.Keys)
                {
                    Bones[nm].StretchX *= x;
                    Bones[nm].StretchY *= y;
                    Bones[nm].StretchZ *= z;
                }
            }
            else
            {
                foreach(string nm in names)
                {
                    Bones[nm].StretchX *= x;
                    Bones[nm].StretchY *= y;
                    Bones[nm].StretchZ *= z;
                }
            }
            return this;
        }

        public Model SwapBone(string start, string end)
        {
            Bones[start] = Bones[end];

            return this;
        }

        public Model SetAnatomy(params Connector[] anatomy)
        {
            Anatomy = anatomy;
            return this;
        }
        public byte[,,] Finalize()
        {
            Dictionary<string, byte[,,]> finals = Bones.ToDictionary(kv => kv.Key, kv => kv.Value.Finalize(0 * Bone.Multiplier, 0));
            string finished = "";
            string[] vals = finals.Keys.ToArray();
            byte[,,] first = finals[vals[0]];
            int origX = first.GetLength(0), origY = first.GetLength(1), origZ = first.GetLength(2);
            if(Anatomy.Length == 0)
            {
                foreach(var s in vals)
                {
                    if(finals.ContainsKey(finished))
                    {
                        finals[s] = TransformLogic.Overlap(finals[finished], finals[s]);
                        finished = s;
                    }
                    else
                    {
                        byte[,,] v = finals[s];
                        finished = s;
                        finals[finished] = v;
                    }

                }
            }
            else
            {
                foreach(Connector conn in Anatomy)
                {
                    byte[,,] bs = TransformLogic.MergeVoxels(finals[conn.plug], finals[conn.socket], conn.matchColor, origX + 20, origY + 20, origZ + 20);
                    /*
                    if(bs.Tally(TransformLogic.dismiss) == finals[conn.socket].Tally(TransformLogic.dismiss))
                        Console.WriteLine("PLUG NOT FOUND: " + conn.plug + " connecting to " + conn.socket);
                    else if(bs.Tally(TransformLogic.dismiss) == finals[conn.plug].Tally(TransformLogic.dismiss))
                        Console.WriteLine("SOCKET NOT FOUND: " + conn.socket + " connecting to " + conn.plug);
                    */
                    finals[conn.socket] = bs;
                    finished = conn.socket;
                }
            }
            return TransformLogic.ExpandBounds(finals[finished], origX, origY, origZ);
        }
        public Model Interpolate(Model target, float alpha)
        {
            Model m = new Model();
            m.Anatomy = Anatomy;
            m.Patterns = Patterns;
            Dictionary<string, Bone> finals = Bones.ToDictionary(kv => kv.Key, kv => kv.Value.Interpolate(target.Bones[kv.Key], alpha));
            foreach(var kv in finals)
            {
                m.AddBone(kv.Key, kv.Value);
            }
            return m;
        }
        
        public Model Replicate()
        {
            Dictionary<string, Bone> bones = Bones.ToDictionary(kv => kv.Key, kv => kv.Value.Replicate());
            Model m = new Model(bones);
            m.Anatomy = Anatomy.Replicate();
            m.Patterns = Patterns;
            return m;
        }

        public static Model Humanoid(string body = "Human_Male", string face = "Neutral", string left_weapon = null, string right_weapon = null, Dictionary<byte, Pattern> patterns = null)
        {
            Model model = new Model(patterns);
            model.AddBone("Left_Leg", Bone.readBone(body + "/LLeg"));
            model.AddBone("Right_Leg", Bone.readBone(body + "/RLeg"));
            model.AddBone("Torso", Bone.readBone(body + "/Torso"));
            model.AddBone("Face", Bone.readBone(body + "/" + face + "_Face"));
            model.AddBone("Head", Bone.readBone(body + "/Head"));
            model.AddBone("Left_Upper_Arm", Bone.readBone(body + "/LUpperArm"));
            model.AddBone("Right_Upper_Arm", Bone.readBone(body + "/RUpperArm"));
            model.AddBone("Left_Lower_Arm", Bone.readBone(body + "/LLowerArm"));
            model.AddBone("Right_Lower_Arm", Bone.readBone(body + "/RLowerArm"));
            model.AddBone("Left_Weapon", Bone.readBone(left_weapon));
            model.AddBone("Right_Weapon", Bone.readBone(right_weapon));

            model.AddBone("Neutral_Face", Bone.readBone(body + "/Neutral_Face"));
            model.AddBone("Hurt_Face", Bone.readBone(body + "/Hurt_Face"));
            model.AddBone("Happy_Face", Bone.readBone(body + "/Happy_Face"));

            model.SetAnatomy(new Connector("Face", "Head", 9), new Connector("Head", "Torso", 8), new Connector("Right_Weapon", "Right_Lower_Arm", 6), new Connector("Left_Weapon", "Left_Lower_Arm", 6),
                new Connector("Right_Lower_Arm", "Right_Upper_Arm", 5), new Connector("Left_Lower_Arm", "Left_Upper_Arm", 4), new Connector("Right_Upper_Arm", "Torso", 3), new Connector("Left_Upper_Arm", "Torso", 2),
                new Connector("Right_Leg", "Torso", 1), new Connector("Torso", "Left_Leg", 0));
            return model;
        }
        public static Model HumanoidAlt(string body = "Human_Male", string face = "Neutral", string left_weapon = null, string right_weapon = null, Dictionary<byte, Pattern> patterns = null)
        {
            Model model = new Model(patterns);
            model.AddBone("Left_Lower_Leg", Bone.readBone(body + "/LLowerLeg"));
            model.AddBone("Right_Lower_Leg", Bone.readBone(body + "/RLowerLeg"));
            model.AddBone("Left_Upper_Leg", Bone.readBone(body + "/LUpperLeg"));
            model.AddBone("Right_Upper_Leg", Bone.readBone(body + "/RUpperLeg"));
            model.AddBone("Torso", Bone.readBone(body + "/Torso"));
            model.AddBone("Face", Bone.readBone(body + "/" + face + "_Face"));
            model.AddBone("Head", Bone.readBone(body + "/Head"));
            model.AddBone("Left_Upper_Arm", Bone.readBone(body + "/LUpperArm"));
            model.AddBone("Right_Upper_Arm", Bone.readBone(body + "/RUpperArm"));
            model.AddBone("Left_Lower_Arm", Bone.readBone(body + "/LLowerArm"));
            model.AddBone("Right_Lower_Arm", Bone.readBone(body + "/RLowerArm"));
            model.AddBone("Left_Weapon", Bone.readBone(left_weapon));
            model.AddBone("Right_Weapon", Bone.readBone(right_weapon));

            model.AddBone("Neutral_Face", Bone.readBone(body + "/Neutral_Face"));
            model.AddBone("Hurt_Face", Bone.readBone(body + "/Hurt_Face"));
            model.AddBone("Happy_Face", Bone.readBone(body + "/Happy_Face"));

            model.SetAnatomy(new Connector("Face", "Head", 9), new Connector("Head", "Torso", 8), new Connector("Right_Weapon", "Right_Lower_Arm", 6), new Connector("Left_Weapon", "Left_Lower_Arm", 6),
                new Connector("Right_Lower_Arm", "Right_Upper_Arm", 5), new Connector("Left_Lower_Arm", "Left_Upper_Arm", 4), new Connector("Right_Upper_Arm", "Torso", 3), new Connector("Left_Upper_Arm", "Torso", 2),
                new Connector("Right_Lower_Leg", "Right_Upper_Leg", 11), new Connector("Right_Upper_Leg", "Torso", 1),
                new Connector("Torso", "Left_Upper_Leg", 0), new Connector("Left_Upper_Leg", "Left_Lower_Leg", 10));
            return model;
        }
        private static List<MagicaVoxelData> findContinuousParts(ref byte[,,] data, List<MagicaVoxelData> previous, Func<byte, bool> pred)
        {
            int xSize = data.GetLength(0), ySize = data.GetLength(1), zSize = data.GetLength(2);
            List<MagicaVoxelData> next = new List<MagicaVoxelData>(256), result = new List<MagicaVoxelData>(2048);
            result.AddRange(previous);
            do
            {
                next = new List<MagicaVoxelData>(256);
                foreach(MagicaVoxelData mvd in previous)
                {
                    int x = mvd.x, y = mvd.y, z = mvd.z;
                    if(x - 1 >= 0 && pred(data[x - 1, y, z]))
                    {
                        next.Add(new MagicaVoxelData(x - 1, y, z, data[x - 1, y, z]));
                        data[x - 1, y, z] = 0;
                    }
                    if(x + 1 < xSize && pred(data[x + 1, y, z]))
                    {
                        next.Add(new MagicaVoxelData(x + 1, y, z, data[x + 1, y, z]));
                        data[x + 1, y, z] = 0;
                    }
                    if(y - 1 >= 0 && pred(data[x, y - 1, z]))
                    {
                        next.Add(new MagicaVoxelData(x, y - 1, z, data[x, y - 1, z]));
                        data[x, y - 1, z] = 0;
                    }
                    if(y + 1 < ySize && pred(data[x, y + 1, z]))
                    {
                        next.Add(new MagicaVoxelData(x, y + 1, z, data[x, y + 1, z]));
                        data[x, y + 1, z] = 0;
                    }
                    if(z - 1 >= 0 && pred(data[x, y, z - 1]))
                    {
                        next.Add(new MagicaVoxelData(x, y, z - 1, data[x, y, z - 1]));
                        data[x, y, z - 1] = 0;
                    }
                    if(z + 1 < zSize && pred(data[x, y, z + 1]))
                    {
                        next.Add(new MagicaVoxelData(x, y, z + 1, data[x, y, z + 1]));
                        data[x, y, z + 1] = 0;
                    }
                }
                result.AddRange(next);
                previous = new List<MagicaVoxelData>(next);
            } while(next.Count > 0);
            return result;


        }
        public static Model FromModelCU(byte[,,] data)
        {
            byte[,,] data2 = data.Replicate();
            Model model = new Model();
            int xSize = data.GetLength(0), ySize = data.GetLength(1), zSize = data.GetLength(2);

            int regular_ctr = 0, gun_ctr = 0, wheel_ctr = 0;
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        if(data2[x, y, z] >= 253 - 1 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt >= 253 - 1 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Wheel" + wheel_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        else if(data2[x, y, z] <= 253 - 12 * 4 && data2[x, y, z] >= 253 - 16 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt <= 253 - 12 * 4 && bt >= 253 - 16 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Gun" + gun_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        else if(data2[x, y, z] > 0 && data2[x, y, z] != 253 - 27 * 4) // quick hack to force water splashes into "Extra"
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt > 0 && bt != 253 - 27 * 4 && (bt > 253 - 12 * 4 || bt < 253 - 16 * 4));
                            if(bn.Count > 0)
                                model.AddBone("Bone" + regular_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                    }
                }
            }
            model.AddBone("Extra", data2);
            model.SetAnatomy();
            return model;
        }
        public static Model FromModelCUPrint(byte[,,] data)
        {
            byte[,,] data2 = data.Replicate();
            Model model = new Model();
            int xSize = data.GetLength(0), ySize = data.GetLength(1), zSize = data.GetLength(2);

            int regular_ctr = 0, gun_ctr = 0, wheel_ctr = 0, flesh_counter = 0, glass_counter = 0;
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {

                        if(data2[x, y, z] >= 253 - 1 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt >= 253 - 1 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Wheel" + wheel_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        else if(data2[x, y, z] <= 253 - 6 * 4 && data2[x, y, z] >= 253 - 11 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt <= 253 - 6 * 4 && bt >= 253 - 11 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Flesh" + flesh_counter++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        else if(data2[x, y, z] == 253 - 35 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt <= 253 - 35 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Glass" + glass_counter++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        /*
                        else if(data2[x, y, z] <= 253 - 12 * 4 && data2[x, y, z] >= 253 - 16 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt <= 253 - 12 * 4 && bt >= 253 - 16 * 4);
                            if(bn.Count > 0)
                                model.AddBone("Gun" + gun_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                        */
                        else if(data2[x, y, z] > 0) //  && data2[x, y, z] != 253 - 27 * 4 // quick hack to force water splashes into "Extra"
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt > 0); // && bt != 253 - 27 * 4 && (bt > 253 - 12 * 4 || bt < 253 - 16 * 4)
                            if(bn.Count > 0)
                                model.AddBone("Bone" + regular_ctr++, TransformLogic.VoxListToArray(bn, xSize, ySize, zSize));
                        }
                    }
                }
            }
            model.AddBone("Extra", data2);
            model.SetAnatomy();
            return model;
        }

        public static List<List<MagicaVoxelData>> BytesToListsCU(byte[,,] data)
        {
            byte[,,] data2 = data.Replicate();
            List<List<MagicaVoxelData>> lists = new List<List<MagicaVoxelData>>(10);
            int xSize = data.GetLength(0), ySize = data.GetLength(1), zSize = data.GetLength(2);

            int regular_ctr = 0, gun_ctr = 0, wheel_ctr = 0;
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        if(data2[x, y, z] >= 253 - 1 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt >= 253 - 1 * 4);
                            if(bn.Count > 0)
                                lists.Add(bn);
                        }
                        else if(data2[x, y, z] <= 253 - 12 * 4 && data2[x, y, z] >= 253 - 16 * 4)
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt <= 253 - 12 * 4 && bt >= 253 - 16 * 4);
                            if(bn.Count > 0)
                                lists.Add(bn);
                        }
                        else if(data2[x, y, z] > 0 && data2[x, y, z] != 253 - 27 * 4) // quick hack to force water splashes into "Extra"
                        {
                            var l = new List<MagicaVoxelData>();
                            l.Add(new MagicaVoxelData(x, y, z, data2[x, y, z]));
                            List<MagicaVoxelData> bn = findContinuousParts(ref data2, l,
                                bt => bt > 0 && bt != 253 - 27 * 4 && (bt > 253 - 12 * 4 || bt < 253 - 16 * 4));
                            if(bn.Count > 0)
                                lists.Add(bn);
                        }
                    }
                }
            }
            lists.Add(VoxelLogic.VoxArrayToList(data2));
            return lists;
        }

        public Model[] PoseInterpolate(Pose[] poses, float[][] frames)
        {
            Model[] posed = new Model[poses.Length + 2], modelFrames = new Model[frames.Length];
            for(int i = 0; i < poses.Length; i++)
            {
                posed[i+1] = poses[i](Replicate());
            }
            posed[0] = posed[1].Replicate();
            posed[poses.Length + 1] = posed[poses.Length].Replicate();
            for(int i = 0; i < frames.Length; i++)
            {
                modelFrames[i] = posed[(int)(frames[i][0]) + 1].Interpolate(posed[(int)(frames[i][1]) + 1], VoxelLogic.Clamp(frames[i][2] - 0.05f, 0.0f, 1.0f)).Interpolate(
                    posed[(int)(frames[i][0]) + 1].Interpolate(posed[(int)(frames[i][1]) + 1], VoxelLogic.Clamp(frames[i][2] + 0.05f, 0.0f, 1.0f)), 0.5f);
            }
            return modelFrames;
        }
    }

    public delegate Model Pose(Model m);


    public class TransformLogic
    {
        public const int Multiplier = OrthoSingle.multiplier, Bonus = OrthoSingle.bonus;
        public static PRNG rng = new PRNG(0x1337beef);

        public static byte[,,] MergeVoxels(byte[,,] plug, byte[,,] socket, int matchColor)
        {
            int plugX = 0, plugY = 0, plugZ = 0, socketX = 0, socketY = 0, socketZ = 0, plugCount = 0, socketCount = 0;
            if(socket == null)
                return plug;
            else if(plug == null)
                return socket;
            int xSize = socket.GetLength(0), ySize = socket.GetLength(1), zSize = socket.GetLength(2);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4)
                        {
                            plugCount++;
                            plugX += x;
                            plugY += y;
                            plugZ += z;
                        }
                    }
                }
            }

            if(plugCount <= 0)
                return socket;

            plugX /= plugCount;
            plugY /= plugCount;
            plugZ /= plugCount;

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(socket[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - socket[x, y, z]) == matchColor * 4)
                        {
                            socketCount++;
                            socketX += x;
                            socketY += y;
                            socketZ += z;
                        }
                    }
                }
            }
            if(socketCount <= 0)
                return plug;
            socketX /= socketCount;
            socketY /= socketCount;
            socketZ /= socketCount;

            byte[,,] working = ExpandBounds(socket, xSize + 20, ySize + 20, zSize + 20);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 0 &&
                           x - plugX + socketX >= -10 && y - plugY + socketY >= -10 && z - plugZ + socketZ >= 0 &&
                           x - plugX + socketX < xSize + 10 && y - plugY + socketY < ySize + 10 && z - plugZ + socketZ < zSize + 20)
                        {
                            working[x - plugX + socketX + 10,
                                    y - plugY + socketY + 10,
                                    z - plugZ + socketZ] = (plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4) ? NearbyColor(plug, x, y, z) : plug[x, y, z];
                        }
                    }
                }
            }
            return working;

        }
        public static byte[,,] MergeVoxels(byte[,,] plug, byte[,,] socket, int matchColor, int newXSize, int newYSize, int newZSize)
        {
            int plugX = 0, plugY = 0, plugZ = 0, socketX = 0, socketY = 0, socketZ = 0, plugCount = 0, socketCount = 0;
            if(socket == null)
                return ExpandBounds(plug, newXSize, newYSize, newZSize);
            else if(plug == null)
                return ExpandBounds(socket, newXSize, newYSize, newZSize);

            plug = ExpandBounds(plug, newXSize, newYSize, newZSize);
            socket = ExpandBounds(socket, newXSize, newYSize, newZSize);

            int xSize = socket.GetLength(0), ySize = socket.GetLength(1), zSize = socket.GetLength(2);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4)
                        {
                            plugCount++;
                            plugX += x;
                            plugY += y;
                            plugZ += z;
                        }
                    }
                }
            }

            if(plugCount <= 0)
                return socket;

            plugX /= plugCount;
            plugY /= plugCount;
            plugZ /= plugCount;

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(socket[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - socket[x, y, z]) == matchColor * 4)
                        {
                            socketCount++;
                            socketX += x;
                            socketY += y;
                            socketZ += z;
                        }
                    }
                }
            }
            if(socketCount <= 0)
                return plug;
            socketX /= socketCount;
            socketY /= socketCount;
            socketZ /= socketCount;

            byte[,,] working = socket;

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 0 &&
                           x - plugX + socketX >= 0 && y - plugY + socketY >= 0 && z - plugZ + socketZ >= 0 &&
                           x - plugX + socketX < xSize && y - plugY + socketY < ySize && z - plugZ + socketZ < zSize &&
                           !(working[x - plugX + socketX, y - plugY + socketY, z - plugZ + socketZ] > 257 - VoxelLogic.wcolorcount * 4 &&
                             (254 - working[x - plugX + socketX, y - plugY + socketY, z - plugZ + socketZ]) % 4 == 0))
                        {
                            working[x - plugX + socketX,
                                    y - plugY + socketY,
                                    z - plugZ + socketZ] = (plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4) ? NearbyColor(plug, x, y, z) : plug[x, y, z];
                        }
                    }
                }
            }
            return working;

        }
        public static byte[,,] MergeVoxels(byte[,,] plug, byte[,,] socket, int matchColor, int removeColor, int replaceColor)
        {
            int plugX = 0, plugY = 0, plugZ = 0, socketX = 0, socketY = 0, socketZ = 0, plugCount = 0, socketCount = 0;
            if(socket == null)
                return plug;
            else if(plug == null)
                return socket;
            int xSize = socket.GetLength(0), ySize = socket.GetLength(1), zSize = socket.GetLength(2);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4)
                        {
                            plugCount++;
                            plugX += x;
                            plugY += y;
                            plugZ += z;
                        }
                    }
                }
            }

            if(plugCount <= 0)
                return socket;
            plugX /= plugCount;
            plugY /= plugCount;
            plugZ /= plugCount;

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(socket[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - socket[x, y, z]) == matchColor * 4)
                        {
                            socketCount++;
                            socketX += x;
                            socketY += y;
                            socketZ += z;
                        }
                    }
                }
            }
            if(socketCount <= 0)
                return plug;

            socketX /= socketCount;
            socketY /= socketCount;
            socketZ /= socketCount;

            byte[,,] working = ExpandBounds(socket, xSize + 20, ySize + 20, zSize + 20);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(plug[x, y, z] > 0 &&
                           x - plugX + socketX >= -10 && y - plugY + socketY >= -10 && z - plugZ + socketZ >= 0 &&
                           x - plugX + socketX < xSize + 10 && y - plugY + socketY < ySize + 10 && z - plugZ + socketZ < zSize + 20)
                        {
                            working[x - plugX + socketX + 10,
                                    y - plugY + socketY + 10,
                                    z - plugZ + socketZ] = (plug[x, y, z] > 257 - VoxelLogic.wcolorcount * 4 && (254 - plug[x, y, z]) == matchColor * 4) ? NearbyColor(plug, x, y, z) : plug[x, y, z];
                        }
                    }
                }
            }
            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        if(254L - working[x + 10, y + 10, z] == removeColor * 4L)
                        {
                            working[x + 10, y + 10, z] = (byte)replaceColor;
                        }
                    }
                }
            }
            return working;

        }
        public static byte[,,] Overlap(byte[,,] start, byte[,,] adding)
        {
            byte[,,] next = start.Replicate();
            int xSize = start.GetLength(0), ySize = start.GetLength(1), zSize = start.GetLength(2);
            int xSize2 = adding.GetLength(0), ySize2 = adding.GetLength(1), zSize2 = adding.GetLength(2);

            for(int z = 0; z < zSize && z < zSize2; z++)
            {
                for(int y = 0; y < ySize && y < ySize2; y++)
                {
                    for(int x = 0; x < xSize && x < xSize2; x++)
                    {
                        if(adding[x, y, z] > 0)
                            next[x, y, z] = adding[x, y, z];
                    }
                }
            }
            return next;
        }

        public static byte NearbyColor(byte[,,] voxelData, int x, int y, int z)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            byte[] nearby = new byte[27], counts = new byte[27];
            int running = 0;
            for(int i = Math.Max(0, x - 1); i <= x + 1 && i < xSize; i++)
            {
                for(int j = Math.Max(0, y - 1); j <= y + 1 && j < ySize; j++)
                {
                    for(int k = Math.Max(0, z - 1); k <= z + 1 && k < zSize; k++)
                    {
                        for(int c = 0; c <= running; c++)
                        {
                            if(nearby[c] == voxelData[i, j, k] && voxelData[i, j, k] != 0)
                            {
                                counts[c]++;
                                break;
                            }
                            if(c == running && (253 - voxelData[i, j, k]) % 4 == 0)
                            {
                                nearby[c] = voxelData[i, j, k];
                                counts[c]++;
                                running++;
                                break;
                            }
                        }
                    }
                }
            }
            byte best = 0, bestCount = 0;
            for(int c = 0; c < running; c++)
            {
                if(counts[c] > bestCount)
                {
                    best = nearby[c];
                    bestCount = counts[c];
                }
            }
            return best;
        }

        public static byte[,,] VoxListToArray(IEnumerable<MagicaVoxelData> voxelData, int xSize, int ySize, int zSize)
        {
            byte[,,] data = new byte[xSize, ySize, zSize];
            foreach(MagicaVoxelData mvd in voxelData)
            {
                if(mvd.x < xSize && mvd.y < ySize && mvd.z < zSize)
                    data[mvd.x, mvd.y, mvd.z] = mvd.color;
            }
            return data;
        }
        public static List<MagicaVoxelData> VoxArrayToList(byte[,,] voxelData)
        {
            List<MagicaVoxelData> vlist = new List<MagicaVoxelData>(voxelData.Length);
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            for(byte x = 0; x < xSize; x++)
            {
                for(byte y = 0; y < ySize; y++)
                {
                    for(byte z = 0; z < zSize; z++)
                    {
                        if(voxelData[x, y, z] > 0)
                            vlist.Add(new MagicaVoxelData { x = x, y = y, z = z, color = voxelData[x, y, z] });
                    }
                }
            }
            return vlist;
        }

        public static byte[,,] VoxListToLargerArray(IEnumerable<MagicaVoxelData> voxelData, int multiplier, int xSize, int ySize, int zSize)
        {
            byte[,,] data = new byte[xSize * multiplier, ySize * multiplier, zSize * multiplier];
            foreach(MagicaVoxelData mvd in voxelData)
            {
                if(mvd.color == 0)
                    continue;
                for(int x = 0; x < multiplier; x++)
                {
                    for(int y = 0; y < multiplier; y++)
                    {
                        for(int z = 0; z < multiplier; z++)
                        {
                            if((VoxelLogic.VisualMode != "CU" || (mvd.color != CURedux.emitter0 && mvd.color != CURedux.trail0 && mvd.color != CURedux.emitter1 && mvd.color != CURedux.trail1))
                                && mvd.color != VoxelLogic.clear &&
                                mvd.x * multiplier + x < xSize * multiplier && mvd.y * multiplier + y < ySize * multiplier && mvd.z * multiplier + z < zSize * multiplier)
                                data[mvd.x * multiplier + x, mvd.y * multiplier + y, mvd.z * multiplier + z] = mvd.color;
                        }
                    }
                }
            }
            return data;
        }
        public static byte[,,] VoxListToLargerArray(IEnumerable<MagicaVoxelData> voxelData, int multiplier, int originalX, int originalY, int xSize, int ySize, int zSize)
        {
            byte[,,] data = new byte[xSize * multiplier, ySize * multiplier, zSize * multiplier];
            foreach(MagicaVoxelData mvd in voxelData)
            {
                for(int x = 0; x < multiplier; x++)
                {
                    for(int y = 0; y < multiplier; y++)
                    {
                        for(int z = 0; z < multiplier; z++)
                        {
                            if((VoxelLogic.VisualMode != "CU" || (mvd.color != CURedux.emitter0 && mvd.color != CURedux.trail0 && mvd.color != CURedux.emitter1 && mvd.color != CURedux.trail1))
                                && mvd.color != VoxelLogic.clear &&
                                (mvd.x + (xSize - originalX) / 2) * multiplier + x < xSize * multiplier && (mvd.y + (ySize - originalY) / 2) * multiplier + y < ySize * multiplier &&
                                mvd.z * multiplier + z < zSize * multiplier)
                                data[(mvd.x + (xSize - originalX) / 2) * multiplier + x, (mvd.y + (ySize - originalY) / 2) * multiplier + y, mvd.z * multiplier + z] = mvd.color;
                        }
                    }
                }
            }
            return data;
        }
        public static byte[,,] Translate(byte[,,] voxelData, int xMove, int yMove, int zMove)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] vs = new byte[xSize, ySize, zSize];

            int xmin = 0, ymin = 0, zmin = 0,
                xmax = xSize, ymax = ySize, zmax = zSize;
            if(xMove < 0)
                xmin -= xMove;
            else if(xMove > 0)
                xmax -= xMove;
            if(yMove < 0)
                ymin -= yMove;
            else if(yMove > 0)
                ymax -= yMove;
            if(zMove < 0)
                zmin -= zMove;
            else if(zMove > 0)
                zmax -= zMove;

            for(int x = xmin; x < xmax; x++)
            {
                for(int y = ymin; y < ymax; y++)
                {
                    for(int z = zmin; z < zmax; z++)
                    {
                        vs[x + xMove, y + yMove, z + zMove] = voxelData[x, y, z];
                    }
                }
            }

            return vs;
        }
        public static byte[,,] ExpandBounds(byte[,,] voxelData, int newXSize, int newYSize, int newZSize)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] vs = new byte[newXSize, newYSize, newZSize];

            for(int x = 0; x < xSize; x++)
            {
                if(x + (newXSize - xSize) / 2 >= 0 && x + (newXSize - xSize) / 2 < newXSize)
                {
                    for(int y = 0; y < ySize; y++)
                    {
                        if(y + (newYSize - ySize) / 2 >= 0 && y + (newYSize - ySize) / 2 < newYSize)
                        {
                            for(int z = 0; z < zSize; z++)
                            {
                                if(z < newZSize)
                                {
                                    vs[x + (newXSize - xSize) / 2, y + (newYSize - ySize) / 2, z] = voxelData[x, y, z];
                                }
                            }
                        }
                    }

                }
            }

            return vs;

        }
        public static byte[,,] RunCA(byte[,,] voxelData, int smoothLevel, bool regardless = false, bool canRemove = true)
        {
            if(smoothLevel <= 1)
                return voxelData;
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            //Dictionary<byte, int> colorCount = new Dictionary<byte, int>();
            int[] colorCount = new int[256];
            byte[][,,] vs = new byte[smoothLevel][,,];
            vs[0] = voxelData.Replicate();
            for(int v = 1; v < smoothLevel; v++)
            {
                vs[v] = new byte[xSize, ySize, zSize];
                for(int x = 0; x < xSize; x++)
                {
                    for(int y = 0; y < ySize; y++)
                    {
                        for(int z = 0; z < zSize; z++)
                        {
                            Array.Clear(colorCount, 0, 256);
                            int emptyCount = 0;
                            if(x == 0 || y == 0 || z == 0 || x == xSize - 1 || y == ySize - 1 || z == zSize - 1
                                || (254 - vs[v - 1][x, y, z]) % 4 == 0 || (!regardless && ColorValue(vs[v - 1][x, y, z]) > 50))
                            {
                                colorCount[vs[v - 1][x, y, z]] = 10000;
                            }
                            else
                            {
                                for(int xx = -1; xx < 2; xx++)
                                {
                                    for(int yy = -1; yy < 2; yy++)
                                    {
                                        for(int zz = -1; zz < 2; zz++)
                                        {
                                            byte smallColor = vs[v - 1][x + xx, y + yy, z + zz];
                                            if(smallColor == 0 || ColorValue(smallColor) < 16)
                                            {
                                                emptyCount++;
                                            }
                                            else {
                                                colorCount[smallColor]++;
                                            }
                                        }
                                    }
                                }
                            }
                            if(emptyCount >= 18 && (canRemove || vs[0][x, y, z] == 0))
                            {
                                vs[v][x, y, z] = 0;
                            }
                            else
                            {
                                int max = 0, cc = colorCount[0], tmp;
                                for(int idx = 1; idx < 256; idx++)
                                {
                                    tmp = colorCount[idx];
                                    if(tmp > cc)
                                    {
                                        cc = tmp;
                                        max = idx;
                                    }
                                }
                                vs[v][x, y, z] = (byte)max;
                            }
                        }
                    }
                }
            }
            return vs[smoothLevel - 1];
        }
        public static byte[,,] RunThinningCA(byte[,,] voxelData, int smoothLevel, bool removePlaceholders = false)
        {
            if(smoothLevel <= 1)
                return voxelData;
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            //Dictionary<byte, int> colorCount = new Dictionary<byte, int>();
            byte[][,,] vs = new byte[smoothLevel][,,];
            vs[0] = voxelData.Replicate();
            for(int v = 1; v < smoothLevel; v++)
            {
                vs[v] = new byte[xSize, ySize, zSize];
                for(int x = 0; x < xSize; x++)
                {
                    for(int y = 0; y < ySize; y++)
                    {
                        for(int z = 0; z < zSize; z++)
                        {
                            int emptyCount = 0;
                            if(x == 0 || y == 0 || z == 0 || x == xSize - 1 || y == ySize - 1 || z == zSize - 1
                                || (!removePlaceholders && ColorValue(vs[v - 1][x, y, z]) > 50))
                            {

                            }
                            else if(removePlaceholders && 254 - vs[v - 1][x, y, z] % 4 == 0)
                            {
                                emptyCount = 9999;
                            }
                            else
                            {
                                for(int xx = -1; xx < 2; xx++)
                                {
                                    for(int yy = -1; yy < 2; yy++)
                                    {
                                        for(int zz = -1; zz < 2; zz++)
                                        {
                                            byte smallColor = vs[v - 1][x + xx, y + yy, z + zz];
                                            if(smallColor == 0 || ColorValue(smallColor) < 16 || (removePlaceholders && 254 - smallColor % 4 == 0))
                                            {
                                                emptyCount++;
                                            }
                                        }
                                    }
                                }
                            }
                            if(emptyCount >= 18)
                                vs[v][x, y, z] = 0;
                            else
                            {
                                vs[v][x, y, z] = vs[v - 1][x, y, z];
                            }
                        }
                    }
                }
            }
            return vs[smoothLevel - 1];
        }

        public static byte[,,] EmptyInterior(byte[,,] voxelData)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] voxels = new byte[xSize, ySize, zSize];
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {

                        if(
                            x == 0 || x == xSize - 1 ||
                            y == 0 || y == ySize - 1 ||
                            z == 0 || z == zSize - 1 ||
                            voxelData[x, y, z] == 0 ||
                            voxelData[x - 1, y, z] == 0 ||
                            voxelData[x + 1, y, z] == 0 ||
                            voxelData[x, y - 1, z] == 0 ||
                            voxelData[x, y + 1, z] == 0 ||
                            voxelData[x, y, z - 1] == 0 ||
                            voxelData[x, y, z + 1] == 0 ||

                            voxelData[x - 1, y, z + 1] == 0 ||
                            voxelData[x + 1, y, z + 1] == 0 ||
                            voxelData[x, y - 1, z + 1] == 0 ||
                            voxelData[x, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y, z - 1] == 0 ||
                            voxelData[x + 1, y, z - 1] == 0 ||
                            voxelData[x, y - 1, z - 1] == 0 ||
                            voxelData[x, y + 1, z - 1] == 0 ||

                            voxelData[x - 1, y - 1, z] == 0 ||
                            voxelData[x + 1, y - 1, z] == 0 ||

                            voxelData[x - 1, y + 1, z] == 0 ||
                            voxelData[x + 1, y + 1, z] == 0 ||

                            voxelData[x - 1, y + 1, z + 1] == 0 ||
                            voxelData[x + 1, y - 1, z + 1] == 0 ||
                            voxelData[x - 1, y - 1, z + 1] == 0 ||
                            voxelData[x + 1, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y + 1, z - 1] == 0 ||
                            voxelData[x + 1, y - 1, z - 1] == 0 ||
                            voxelData[x - 1, y - 1, z - 1] == 0 ||
                            voxelData[x + 1, y + 1, z - 1] == 0)

                            voxels[x, y, z] = voxelData[x, y, z];
                        else
                            voxels[x, y, z] = 0;

                    }
                }
            }
            return voxels;
        }

        public static byte[,,] FillInterior(byte[,,] voxelData)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] voxels = new byte[xSize, ySize, zSize];
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {

                        if(
                            x == 0 || x == xSize - 1 ||
                            y == 0 || y == ySize - 1 ||
                            z == 0 || z == zSize - 1 ||
                            voxelData[x, y, z] == 0 ||
                            voxelData[x - 1, y, z] == 0 ||
                            voxelData[x + 1, y, z] == 0 ||
                            voxelData[x, y - 1, z] == 0 ||
                            voxelData[x, y + 1, z] == 0 ||
                            voxelData[x, y, z - 1] == 0 ||
                            voxelData[x, y, z + 1] == 0 ||

                            voxelData[x - 1, y, z + 1] == 0 ||
                            voxelData[x + 1, y, z + 1] == 0 ||
                            voxelData[x, y - 1, z + 1] == 0 ||
                            voxelData[x, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y, z - 1] == 0 ||
                            voxelData[x + 1, y, z - 1] == 0 ||
                            voxelData[x, y - 1, z - 1] == 0 ||
                            voxelData[x, y + 1, z - 1] == 0 ||

                            voxelData[x - 1, y - 1, z] == 0 ||
                            voxelData[x + 1, y - 1, z] == 0 ||

                            voxelData[x - 1, y + 1, z] == 0 ||
                            voxelData[x + 1, y + 1, z] == 0 ||

                            voxelData[x - 1, y + 1, z + 1] == 0 ||
                            voxelData[x + 1, y - 1, z + 1] == 0 ||
                            voxelData[x - 1, y - 1, z + 1] == 0 ||
                            voxelData[x + 1, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y + 1, z - 1] == 0 ||
                            voxelData[x + 1, y - 1, z - 1] == 0 ||
                            voxelData[x - 1, y - 1, z - 1] == 0 ||
                            voxelData[x + 1, y + 1, z - 1] == 0)

                            voxels[x, y, z] = voxelData[x, y, z];
                        else
                            voxels[x, y, z] = 2;

                    }
                }
            }
            return voxels;
        }

        public static byte[,,] FillInteriorAndOpen(byte[,,] voxelData)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] voxels = new byte[xSize, ySize, zSize];
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        if(z <= 8)
                        {
                            if(
                            x <= 1 || x >= xSize - 2 ||
                            y <= 1 || y >= ySize - 2 ||
                            z >= zSize - 2 ||
                            voxelData[x, y, z] == 253 - 17 * 4 ||
                            voxelData[x - 1, y, z] == 0 ||
                            voxelData[x + 1, y, z] == 0 ||
                            voxelData[x, y - 1, z] == 0 ||
                            voxelData[x, y + 1, z] == 0 ||

                            voxelData[x, y, z + 1] == 0 ||
                            voxelData[x - 1, y, z + 1] == 0 ||
                            voxelData[x + 1, y, z + 1] == 0 ||
                            voxelData[x, y - 1, z + 1] == 0 ||
                            voxelData[x, y + 1, z + 1] == 0 ||
                            voxelData[x - 1, y + 1, z + 1] == 0 ||
                            voxelData[x + 1, y - 1, z + 1] == 0 ||
                            voxelData[x - 1, y - 1, z + 1] == 0 ||
                            voxelData[x + 1, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y - 1, z] == 0 ||
                            voxelData[x + 1, y - 1, z] == 0 ||
                            voxelData[x - 1, y + 1, z] == 0 ||
                            voxelData[x + 1, y + 1, z] == 0 ||

                            voxelData[x - 2, y, z] == 0 ||
                            voxelData[x + 2, y, z] == 0 ||
                            voxelData[x, y - 2, z] == 0 ||
                            voxelData[x, y + 2, z] == 0 ||

                            voxelData[x - 2, y - 1, z] == 0 ||
                            voxelData[x + 2, y - 1, z] == 0 ||
                            voxelData[x - 2, y + 1, z] == 0 ||
                            voxelData[x + 2, y + 1, z] == 0 ||

                            voxelData[x - 1, y - 2, z] == 0 ||
                            voxelData[x + 1, y - 2, z] == 0 ||
                            voxelData[x - 1, y + 2, z] == 0 ||
                            voxelData[x + 1, y + 2, z] == 0 ||

                            voxelData[x - 2, y - 2, z] == 0 ||
                            voxelData[x + 2, y - 2, z] == 0 ||
                            voxelData[x - 2, y + 2, z] == 0 ||
                            voxelData[x + 2, y + 2, z] == 0 ||

                            voxelData[x - 2, y, z + 1] == 0 ||
                            voxelData[x + 2, y, z + 1] == 0 ||
                            voxelData[x, y - 2, z + 1] == 0 ||
                            voxelData[x, y + 2, z + 1] == 0 ||

                            voxelData[x - 2, y - 1, z + 1] == 0 ||
                            voxelData[x + 2, y - 1, z + 1] == 0 ||
                            voxelData[x - 2, y + 1, z + 1] == 0 ||
                            voxelData[x + 2, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y - 2, z + 1] == 0 ||
                            voxelData[x + 1, y - 2, z + 1] == 0 ||
                            voxelData[x - 1, y + 2, z + 1] == 0 ||
                            voxelData[x + 1, y + 2, z + 1] == 0 ||

                            voxelData[x - 2, y - 2, z + 1] == 0 ||
                            voxelData[x + 2, y - 2, z + 1] == 0 ||
                            voxelData[x - 2, y + 2, z + 1] == 0 ||
                            voxelData[x + 2, y + 2, z + 1] == 0 ||


                            voxelData[x, y, z + 2] == 0 ||
                            voxelData[x - 1, y, z + 2] == 0 ||
                            voxelData[x + 1, y, z + 2] == 0 ||
                            voxelData[x, y - 1, z + 2] == 0 ||
                            voxelData[x, y + 1, z + 2] == 0 ||
                            voxelData[x - 1, y + 1, z + 2] == 0 ||
                            voxelData[x + 1, y - 1, z + 2] == 0 ||
                            voxelData[x - 1, y - 1, z + 2] == 0 ||
                            voxelData[x + 1, y + 1, z + 2] == 0 ||

                            voxelData[x - 2, y, z + 2] == 0 ||
                            voxelData[x + 2, y, z + 2] == 0 ||
                            voxelData[x, y - 2, z + 2] == 0 ||
                            voxelData[x, y + 2, z + 2] == 0 ||

                            voxelData[x - 2, y - 1, z + 2] == 0 ||
                            voxelData[x + 2, y - 1, z + 2] == 0 ||
                            voxelData[x - 2, y + 1, z + 2] == 0 ||
                            voxelData[x + 2, y + 1, z + 2] == 0 ||

                            voxelData[x - 1, y - 2, z + 2] == 0 ||
                            voxelData[x + 1, y - 2, z + 2] == 0 ||
                            voxelData[x - 1, y + 2, z + 2] == 0 ||
                            voxelData[x + 1, y + 2, z + 2] == 0 ||

                            voxelData[x - 2, y - 2, z + 2] == 0 ||
                            voxelData[x + 2, y - 2, z + 2] == 0 ||
                            voxelData[x - 2, y + 2, z + 2] == 0 ||
                            voxelData[x + 2, y + 2, z + 2] == 0
                            )
                                voxels[x, y, z] = voxelData[x, y, z];
                            else
                            {
                                for(int h = 0; h <= z; h++)
                                {

                                    if(
                                    x > 1 && x < xSize - 2 &&
                                    y > 1 && y < ySize - 2 &&
                                    h < zSize - 2 &&
                                    voxelData[x, y, h] != 253 - 17 * 4 &&

                                    voxelData[x, y, h] != 0 && (
                            voxelData[x - 1, y, h] == 0 ||
                            voxelData[x + 1, y, h] == 0 ||
                            voxelData[x, y - 1, h] == 0 ||
                            voxelData[x, y + 1, h] == 0 ||

                            voxelData[x, y, h + 1] == 0 ||
                            voxelData[x - 1, y, h + 1] == 0 ||
                            voxelData[x + 1, y, h + 1] == 0 ||
                            voxelData[x, y - 1, h + 1] == 0 ||
                            voxelData[x, y + 1, h + 1] == 0 ||
                            voxelData[x - 1, y + 1, h + 1] == 0 ||
                            voxelData[x + 1, y - 1, h + 1] == 0 ||
                            voxelData[x - 1, y - 1, h + 1] == 0 ||
                            voxelData[x + 1, y + 1, h + 1] == 0 ||

                            voxelData[x - 1, y - 1, h] == 0 ||
                            voxelData[x + 1, y - 1, h] == 0 ||
                            voxelData[x - 1, y + 1, h] == 0 ||
                            voxelData[x + 1, y + 1, h] == 0 
                            /* ||
                            
                            voxelData[x - 2, y, h] == 0 ||
                            voxelData[x + 2, y, h] == 0 ||
                            voxelData[x, y - 2, h] == 0 ||
                            voxelData[x, y + 2, h] == 0 ||

                            voxelData[x - 2, y - 1, h] == 0 ||
                            voxelData[x + 2, y - 1, h] == 0 ||
                            voxelData[x - 2, y + 1, h] == 0 ||
                            voxelData[x + 2, y + 1, h] == 0 ||

                            voxelData[x - 1, y - 2, h] == 0 ||
                            voxelData[x + 1, y - 2, h] == 0 ||
                            voxelData[x - 1, y + 2, h] == 0 ||
                            voxelData[x + 1, y + 2, h] == 0 ||

                            voxelData[x - 2, y - 2, h] == 0 ||
                            voxelData[x + 2, y - 2, h] == 0 ||
                            voxelData[x - 2, y + 2, h] == 0 ||
                            voxelData[x + 2, y + 2, h] == 0 ||

                            voxelData[x - 2, y, h + 1] == 0 ||
                            voxelData[x + 2, y, h + 1] == 0 ||
                            voxelData[x, y - 2, h + 1] == 0 ||
                            voxelData[x, y + 2, h + 1] == 0 ||

                            voxelData[x - 2, y - 1, h + 1] == 0 ||
                            voxelData[x + 2, y - 1, h + 1] == 0 ||
                            voxelData[x - 2, y + 1, h + 1] == 0 ||
                            voxelData[x + 2, y + 1, h + 1] == 0 ||

                            voxelData[x - 1, y - 2, h + 1] == 0 ||
                            voxelData[x + 1, y - 2, h + 1] == 0 ||
                            voxelData[x - 1, y + 2, h + 1] == 0 ||
                            voxelData[x + 1, y + 2, h + 1] == 0 ||

                            voxelData[x - 2, y - 2, h + 1] == 0 ||
                            voxelData[x + 2, y - 2, h + 1] == 0 ||
                            voxelData[x - 2, y + 2, h + 1] == 0 ||
                            voxelData[x + 2, y + 2, h + 1] == 0 ||


                            voxelData[x, y, h + 2] == 0 ||
                            voxelData[x - 1, y, h + 2] == 0 ||
                            voxelData[x + 1, y, h + 2] == 0 ||
                            voxelData[x, y - 1, h + 2] == 0 ||
                            voxelData[x, y + 1, h + 2] == 0 ||
                            voxelData[x - 1, y + 1, h + 2] == 0 ||
                            voxelData[x + 1, y - 1, h + 2] == 0 ||
                            voxelData[x - 1, y - 1, h + 2] == 0 ||
                            voxelData[x + 1, y + 1, h + 2] == 0 ||

                            voxelData[x - 2, y, h + 2] == 0 ||
                            voxelData[x + 2, y, h + 2] == 0 ||
                            voxelData[x, y - 2, h + 2] == 0 ||
                            voxelData[x, y + 2, h + 2] == 0 ||

                            voxelData[x - 2, y - 1, h + 2] == 0 ||
                            voxelData[x + 2, y - 1, h + 2] == 0 ||
                            voxelData[x - 2, y + 1, h + 2] == 0 ||
                            voxelData[x + 2, y + 1, h + 2] == 0 ||

                            voxelData[x - 1, y - 2, h + 2] == 0 ||
                            voxelData[x + 1, y - 2, h + 2] == 0 ||
                            voxelData[x - 1, y + 2, h + 2] == 0 ||
                            voxelData[x + 1, y + 2, h + 2] == 0 ||

                            voxelData[x - 2, y - 2, h + 2] == 0 ||
                            voxelData[x + 2, y - 2, h + 2] == 0 ||
                            voxelData[x - 2, y + 2, h + 2] == 0 ||
                            voxelData[x + 2, y + 2, h + 2] == 0 */))
                                        voxels[x, y, h] = voxelData[x, y, h];
                                    else
                                        voxels[x, y, h] = 2;
                                }
                            }
                        }
                        else if(
                            x <= 1 || x >= xSize - 2 ||
                            y <= 1 || y >= ySize - 2 ||
                            z <= 1 || z >= zSize - 2 ||
                            voxelData[x, y, z] == 253 - 17 * 4 ||
                            voxelData[x, y, z] == 0 ||
                            voxelData[x - 1, y, z] == 0 ||
                            voxelData[x + 1, y, z] == 0 ||
                            voxelData[x, y - 1, z] == 0 ||
                            voxelData[x, y + 1, z] == 0 ||

                            voxelData[x - 1, y - 1, z] == 0 ||
                            voxelData[x + 1, y - 1, z] == 0 ||
                            voxelData[x - 1, y + 1, z] == 0 ||
                            voxelData[x + 1, y + 1, z] == 0 ||

                            voxelData[x - 2, y, z] == 0 ||
                            voxelData[x + 2, y, z] == 0 ||
                            voxelData[x, y - 2, z] == 0 ||
                            voxelData[x, y + 2, z] == 0 ||

                            voxelData[x - 2, y - 1, z] == 0 ||
                            voxelData[x + 2, y - 1, z] == 0 ||
                            voxelData[x - 2, y + 1, z] == 0 ||
                            voxelData[x + 2, y + 1, z] == 0 ||

                            voxelData[x - 1, y - 2, z] == 0 ||
                            voxelData[x + 1, y - 2, z] == 0 ||
                            voxelData[x - 1, y + 2, z] == 0 ||
                            voxelData[x + 1, y + 2, z] == 0 ||

                            voxelData[x - 2, y - 2, z] == 0 ||
                            voxelData[x + 2, y - 2, z] == 0 ||
                            voxelData[x - 2, y + 2, z] == 0 ||
                            voxelData[x + 2, y + 2, z] == 0 ||

                            voxelData[x, y, z + 1] == 0 ||
                            voxelData[x - 1, y, z + 1] == 0 ||
                            voxelData[x + 1, y, z + 1] == 0 ||
                            voxelData[x, y - 1, z + 1] == 0 ||
                            voxelData[x, y + 1, z + 1] == 0 ||
                            voxelData[x - 1, y + 1, z + 1] == 0 ||
                            voxelData[x + 1, y - 1, z + 1] == 0 ||
                            voxelData[x - 1, y - 1, z + 1] == 0 ||
                            voxelData[x + 1, y + 1, z + 1] == 0 ||

                            voxelData[x - 2, y, z + 1] == 0 ||
                            voxelData[x + 2, y, z + 1] == 0 ||
                            voxelData[x, y - 2, z + 1] == 0 ||
                            voxelData[x, y + 2, z + 1] == 0 ||

                            voxelData[x - 2, y - 1, z + 1] == 0 ||
                            voxelData[x + 2, y - 1, z + 1] == 0 ||
                            voxelData[x - 2, y + 1, z + 1] == 0 ||
                            voxelData[x + 2, y + 1, z + 1] == 0 ||

                            voxelData[x - 1, y - 2, z + 1] == 0 ||
                            voxelData[x + 1, y - 2, z + 1] == 0 ||
                            voxelData[x - 1, y + 2, z + 1] == 0 ||
                            voxelData[x + 1, y + 2, z + 1] == 0 ||

                            voxelData[x - 2, y - 2, z + 1] == 0 ||
                            voxelData[x + 2, y - 2, z + 1] == 0 ||
                            voxelData[x - 2, y + 2, z + 1] == 0 ||
                            voxelData[x + 2, y + 2, z + 1] == 0 ||


                            voxelData[x, y, z + 2] == 0 ||
                            voxelData[x - 1, y, z + 2] == 0 ||
                            voxelData[x + 1, y, z + 2] == 0 ||
                            voxelData[x, y - 1, z + 2] == 0 ||
                            voxelData[x, y + 1, z + 2] == 0 ||
                            voxelData[x - 1, y + 1, z + 2] == 0 ||
                            voxelData[x + 1, y - 1, z + 2] == 0 ||
                            voxelData[x - 1, y - 1, z + 2] == 0 ||
                            voxelData[x + 1, y + 1, z + 2] == 0 ||

                            voxelData[x - 2, y, z + 2] == 0 ||
                            voxelData[x + 2, y, z + 2] == 0 ||
                            voxelData[x, y - 2, z + 2] == 0 ||
                            voxelData[x, y + 2, z + 2] == 0 ||

                            voxelData[x - 2, y - 1, z + 2] == 0 ||
                            voxelData[x + 2, y - 1, z + 2] == 0 ||
                            voxelData[x - 2, y + 1, z + 2] == 0 ||
                            voxelData[x + 2, y + 1, z + 2] == 0 ||

                            voxelData[x - 1, y - 2, z + 2] == 0 ||
                            voxelData[x + 1, y - 2, z + 2] == 0 ||
                            voxelData[x - 1, y + 2, z + 2] == 0 ||
                            voxelData[x + 1, y + 2, z + 2] == 0 ||

                            voxelData[x - 2, y - 2, z + 2] == 0 ||
                            voxelData[x + 2, y - 2, z + 2] == 0 ||
                            voxelData[x - 2, y + 2, z + 2] == 0 ||
                            voxelData[x + 2, y + 2, z + 2] == 0 ||


                            voxelData[x, y, z - 1] == 0 ||
                            voxelData[x - 1, y, z - 1] == 0 ||
                            voxelData[x + 1, y, z - 1] == 0 ||
                            voxelData[x, y - 1, z - 1] == 0 ||
                            voxelData[x, y + 1, z - 1] == 0 ||
                            voxelData[x - 1, y + 1, z - 1] == 0 ||
                            voxelData[x + 1, y - 1, z - 1] == 0 ||
                            voxelData[x - 1, y - 1, z - 1] == 0 ||
                            voxelData[x + 1, y + 1, z - 1] == 0 ||

                            voxelData[x - 2, y, z - 1] == 0 ||
                            voxelData[x + 2, y, z - 1] == 0 ||
                            voxelData[x, y - 2, z - 1] == 0 ||
                            voxelData[x, y + 2, z - 1] == 0 ||

                            voxelData[x - 2, y - 1, z - 1] == 0 ||
                            voxelData[x + 2, y - 1, z - 1] == 0 ||
                            voxelData[x - 2, y + 1, z - 1] == 0 ||
                            voxelData[x + 2, y + 1, z - 1] == 0 ||

                            voxelData[x - 1, y - 2, z - 1] == 0 ||
                            voxelData[x + 1, y - 2, z - 1] == 0 ||
                            voxelData[x - 1, y + 2, z - 1] == 0 ||
                            voxelData[x + 1, y + 2, z - 1] == 0 ||

                            voxelData[x - 2, y - 2, z - 1] == 0 ||
                            voxelData[x + 2, y - 2, z - 1] == 0 ||
                            voxelData[x - 2, y + 2, z - 1] == 0 ||
                            voxelData[x + 2, y + 2, z - 1] == 0 ||


                            voxelData[x, y, z - 2] == 0 ||
                            voxelData[x - 1, y, z - 2] == 0 ||
                            voxelData[x + 1, y, z - 2] == 0 ||
                            voxelData[x, y - 1, z - 2] == 0 ||
                            voxelData[x, y + 1, z - 2] == 0 ||
                            voxelData[x - 1, y + 1, z - 2] == 0 ||
                            voxelData[x + 1, y - 1, z - 2] == 0 ||
                            voxelData[x - 1, y - 1, z - 2] == 0 ||
                            voxelData[x + 1, y + 1, z - 2] == 0 ||

                            voxelData[x - 2, y, z - 2] == 0 ||
                            voxelData[x + 2, y, z - 2] == 0 ||
                            voxelData[x, y - 2, z - 2] == 0 ||
                            voxelData[x, y + 2, z - 2] == 0 ||

                            voxelData[x - 2, y - 1, z - 2] == 0 ||
                            voxelData[x + 2, y - 1, z - 2] == 0 ||
                            voxelData[x - 2, y + 1, z - 2] == 0 ||
                            voxelData[x + 2, y + 1, z - 2] == 0 ||

                            voxelData[x - 1, y - 2, z - 2] == 0 ||
                            voxelData[x + 1, y - 2, z - 2] == 0 ||
                            voxelData[x - 1, y + 2, z - 2] == 0 ||
                            voxelData[x + 1, y + 2, z - 2] == 0 ||

                            voxelData[x - 2, y - 2, z - 2] == 0 ||
                            voxelData[x + 2, y - 2, z - 2] == 0 ||
                            voxelData[x - 2, y + 2, z - 2] == 0 ||
                            voxelData[x + 2, y + 2, z - 2] == 0)
                        {
                            voxels[x, y, z] = voxelData[x, y, z];
                        }
                        else
                        {
                            voxels[x, y, z] = 2;
                        }
                    }
                }
            }
            return voxels;
        }

        public static byte[,,] ClearInterior(byte[,,] voxelData)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);

            byte[,,] voxels = new byte[xSize, ySize, zSize];
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        if(voxels[x, y, z] == 2)
                            voxels[x, y, z] = 0;
                        else
                            voxels[x, y, z] = voxelData[x, y, z];
                    }
                }
            }
            return voxels;
        }

        public static byte[,,] RunSurfaceCA(byte[,,] voxelData, int smoothLevel, bool regardless = false)
        {
            if(smoothLevel <= 1)
                return voxelData;
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            //Dictionary<byte, int> colorCount = new Dictionary<byte, int>();
            int[] colorCount = new int[256];
            byte[][,,] vs = new byte[smoothLevel][,,];
            vs[0] = FillInterior(voxelData);
            for(int v = 1; v < smoothLevel; v++)
            {
                vs[v] = new byte[xSize, ySize, zSize];
                for(int x = 0; x < xSize; x++)
                {
                    for(int y = 0; y < ySize; y++)
                    {
                        for(int z = 0; z < zSize; z++)
                        {
                            Array.Clear(colorCount, 0, 256);
                            int emptyCount = 0;
                            if(x == 0 || y == 0 || z == 0 || x == xSize - 1 || y == ySize - 1 || z == zSize - 1
                                || (254 - vs[v - 1][x, y, z]) % 4 == 0 || (!regardless && ColorValue(vs[v - 1][x, y, z]) > 50) || vs[v - 1][x, y, z] == 2)
                            {
                                colorCount[vs[v - 1][x, y, z]] = 10000;
                            }
                            else
                            {
                                for(int xx = -1; xx < 2; xx++)
                                {
                                    for(int yy = -1; yy < 2; yy++)
                                    {
                                        for(int zz = -1; zz < 2; zz++)
                                        {
                                            byte smallColor = vs[v - 1][x + xx, y + yy, z + zz];
                                            if(smallColor == 2)
                                            {
                                            }
                                            else if(smallColor == 0 || ColorValue(smallColor) < 16)
                                            {
                                                emptyCount++;
                                            }
                                            else {
                                                colorCount[smallColor]++;
                                            }
                                        }
                                    }
                                }
                            }
                            if(emptyCount >= 18)
                                vs[v][x, y, z] = 0;
                            else
                            {
                                int max = 0, cc = colorCount[0], tmp;
                                for(int idx = 1; idx < 256; idx++)
                                {
                                    tmp = colorCount[idx];
                                    if(tmp > cc)
                                    {
                                        cc = tmp;
                                        max = idx;
                                    }
                                }
                                vs[v][x, y, z] = (byte)max;
                            }
                        }
                    }
                }
            }
            return ClearInterior(vs[smoothLevel - 1]);
        }
        public static bool HasAdjacentColor(byte[,,] voxelData, int xsize, int ysize, int zsize, int x, int y, int z, int color)
        {
            bool gotX = false, gotY = false, gotZ = false;
            if(x > 0 && voxelData[x - 1, y, z] == color)
                gotX = true;
            if(y > 0 && voxelData[x, y - 1, z] == color)
                gotY = true;
            if(z > 0 && voxelData[x, y, z - 1] == color)
                gotZ = true;

            if(x < xsize - 1 && voxelData[x + 1, y, z] == color)
                gotX = true;
            if(y < ysize - 1 && voxelData[x, y + 1, z] == color)
                gotY = true;
            if(z < zsize - 1 && voxelData[x, y, z + 1] == color)
                gotZ = true;

            return gotX && gotY && gotZ;
        }
        public static byte[,,] RandomRemoval(byte[,,] voxelData)
        {
            rng.Reseed(1337u);
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            byte[,,] cpy = voxelData.Replicate();
            int current_color;
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        current_color = (253 - voxelData[x, y, z]) / 4;

                        if(current_color == 36 || current_color == 37) //rotors
                        {
                            cpy[x, y, z] = 0;
                        }
                        else if((current_color == 40 || current_color == VoxelLogic.wcolorcount + 5 || current_color == VoxelLogic.wcolorcount + 20) && rng.Next(11) < 8) //rare sparks
                        {
                            cpy[x, y, z] = 0;
                        }
                        else if((current_color == 27 || current_color == VoxelLogic.wcolorcount + 4) && rng.Next(7) < 2) //water
                        {
                            cpy[x, y, z] = 2;
                        }
                        else if(current_color == 42 && rng.Next(13) < 2 && HasAdjacentColor(voxelData, xSize, ySize, zSize, x, y, z, 0)) //dust
                        {
                            cpy[x, y, z] = 0;
                        }
                    }
                }
            }
            return cpy;
        }

        public static byte[,,] RandomChanges(byte[,,] voxelData)
        {
            rng.Reseed(1337u);
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            byte[,,] cpy = voxelData.Replicate();
            int current_color;
            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        current_color = (253 - cpy[x, y, z]) / 4;

                        if(current_color == 17 && rng.Next(9) < 2 && HasAdjacentColor(voxelData, xSize, ySize, zSize, x, y, z, 0)) //smoke //253 - 17 * 4
                        {
                            cpy[x, y, z] = 0;
                        }
                        else if(current_color == 18) //yellow fire
                        {
                            if(rng.Next(3) > 0)
                            {
                                cpy[x, y, z] -= (byte)(4 * rng.Next(3));
                            }
                        }
                        else if(current_color == 19) // orange fire
                        {
                            if(rng.Next(5) < 4)
                            {
                                cpy[x, y, z] += (byte)(4 * rng.Next(2));
                            }
                        }
                        else if(current_color == 20) // sparks
                        {
                            if(rng.Next(5) > 0)
                            {
                                cpy[x, y, z] += (byte)(4 * rng.Next(3));
                            }
                        }
                        else if(current_color == 42) // dust
                        {
                            if(rng.Next(4) == 0)
                            {
                                cpy[x, y, z] = 253 - 4 * 41;
                            }
                        }
                    }
                }
            }
            return cpy;
        }

        public static List<MagicaVoxelData> VoxArrayToListSmoothed(byte[,,] voxelData)
        {
            return VoxArrayToList(RunCA(voxelData, 3));
        }

        public static byte[,,] SealGaps(byte[,,] voxelData)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            byte[,,] vls = new byte[xSize, ySize, zSize];// = voxelData.Replicate();
            for(int z = 0; z < zSize - 1; z++)
            {
                for(int y = 1; y < ySize - 1; y++)
                {
                    byte currentFill = 0;
                    for(int x = xSize - 2; x > 0; x--)
                    {
                        int clr = 253 - voxelData[x, y, z] / 4;
                        if((clr >= 36 && clr <= 39) || (clr < VoxelLogic.wcolors.Length && VoxelLogic.wcolors[clr][3] == VoxelLogic.waver_alpha))
                        {
                            vls[x, y, z] = voxelData[x, y, z];
                            if(voxelData[x, y, z] > VoxelLogic.clear || voxelData[x, y, z] == 0)
                                currentFill = voxelData[x - 1, y, z];
                        }
                        else
                        {
                            if((voxelData[x + 1, y, z] <= VoxelLogic.clear || voxelData[x - 1, y, z] <= VoxelLogic.clear || voxelData[x, y, z + 1] <= VoxelLogic.clear)
                                && (voxelData[x, y, z] > VoxelLogic.clear || voxelData[x, y, z] == 0))
                            {
                                //if(voxelData[x, y, z] > 253 - 57 * 4 || voxelData[x, y, z] == 0)
                                currentFill = voxelData[x, y, z];
                            }
                            vls[x, y, z] = currentFill;
                        }
                        /*
                        if(voxelData[x,y,z] > 0 && (voxelData[x, y, z + 1] == 0 || voxelData[x + 1, y, z] == 0 || voxelData[x - 1, y, z] == 0 || voxelData[x, y + 1, z] == 0 || voxelData[x, y - 1, z] == 0))
                            vls[x, y, z] = voxelData[x, y, z];
                        else if(voxelData[x + 2, y, z] == 0 && voxelData[x, y, z + 1] > 0 && voxelData[x + 1, y, z] > 0 && voxelData[x, y, z] > 0)
                            vls[x, y, z] = voxelData[x + 1, y, z];
                        else if(voxelData[x + 1, y, z] > 0 && voxelData[x, y, z + 1] > 0 && voxelData[x + 1, y, z + 1] == 0)
                            vls[x, y, z] = voxelData[x + 1, y, z];
                            */
                        /*
                        else if(voxelData[x, y + 2, z] == 0 && voxelData[x, y, z + 1] > 0 && voxelData[x, y + 1, z] > 0)
                            vls[x, y, z] = voxelData[x, y + 1, z];
                        else if(voxelData[x, y - 2, z] == 0 && voxelData[x, y, z + 1] > 0 && voxelData[x, y - 1, z] > 0)
                            vls[x, y, z] = voxelData[x, y - 1, z];*/
                        /*
                    else if(voxelData[x, y, z] == 0 && voxelData[x, y, z + 1] > 0)
                        vls[x, y, z] = voxelData[x, y, z+1];
                    else if(voxelData[x, y - 1, z] > 0 && voxelData[x, y, z] == 0 && voxelData[x, y, z + 1] > 0)
                        vls[x, y, z] = voxelData[x, y - 1, z];
                    else if(voxelData[x, y + 1, z] > 0 && voxelData[x, y, z] == 0 && voxelData[x, y, z + 1] > 0)
                        vls[x, y, z] = voxelData[x, y + 1, z];

                    else if(voxelData[x, y - 1, z] > 0 && voxelData[x, y, z + 1] > 0 && voxelData[x, y - 1, z + 1] == 0)
                        vls[x, y, z] = voxelData[x, y-1, z];
                    else if(voxelData[x, y + 1, z] > 0 && voxelData[x, y, z + 1] > 0 && voxelData[x, y + 1, z + 1] == 0)
                        vls[x, y, z] = voxelData[x, y+1, z];
                        */
                    }
                }
            }
            return vls;
        }

        public static int ColorValue(int color)
        {
            if(color == 0)
                return 5;
            switch(VoxelLogic.VisualMode)
            {
                case "CU":
                case "CU_alt":
                    return VoxelLogic.ImportantColorCU(color);
                case "Mecha":
                    return VoxelLogic.ImportantColorMecha(color);
                case "Mon":
                    return VoxelLogic.ImportantColorMon(color);
                case "None":
                    return 16;
                default:
                    return VoxelLogic.ImportantColorW(color);
            }
        }


        public static byte[,,] Shrink(byte[,,] voxelData, int multiplier)
        {
            if(multiplier == 1) return voxelData;
            return Shrink(voxelData, multiplier, multiplier, multiplier);
        }
        public static byte[,,] Shrink(byte[,,] voxelData, int xmultiplier, int ymultiplier, int zmultiplier)
        {
            int xSize = voxelData.GetLength(0), ySize = voxelData.GetLength(1), zSize = voxelData.GetLength(2);
            Dictionary<byte, int> colorCount = new Dictionary<byte, int>();

            byte[,,] vfinal = new byte[xSize / xmultiplier, ySize / ymultiplier, zSize / zmultiplier];
            Dictionary<byte, MagicaVoxelData> specialColors = new Dictionary<byte, MagicaVoxelData>();
            for(int x = 0; x < xSize / xmultiplier; x++)
            {
                for(int y = 0; y < ySize / ymultiplier; y++)
                {
                    for(int z = 0; z < zSize / zmultiplier; z++)
                    {
                        colorCount = new Dictionary<byte, int>();
                        int fullCount = 0;
                        int emptyCount = 0;
                        int specialCount = 0;
                        for(int xx = 0; xx < xmultiplier; xx++)
                        {
                            for(int yy = 0; yy < ymultiplier; yy++)
                            {
                                for(int zz = 0; zz < zmultiplier; zz++)
                                {
                                    if(x * xmultiplier + xx >= xSize || y * ymultiplier + yy < 0 || z * zmultiplier + zz >= zSize)
                                        continue;
                                    byte smallColor = voxelData[x * xmultiplier + xx, y * ymultiplier + yy, z * zmultiplier + zz];
                                    if((254 - smallColor) % 4 == 0)
                                    {
                                        specialCount++;
                                        if(specialColors.ContainsKey(smallColor))
                                        {
                                            if(specialColors[smallColor].color < specialCount)
                                            {
                                                specialColors[smallColor] = new MagicaVoxelData { x = (byte)x, y = (byte)y, z = (byte)z, color = (byte)specialCount };
                                            }
                                        }
                                        else
                                        {
                                            specialColors[smallColor] = new MagicaVoxelData { x = (byte)x, y = (byte)y, z = (byte)z, color = (byte)specialCount };
                                        }
                                    }
                                    if(smallColor > 0)
                                    {
                                        if(colorCount.ContainsKey(smallColor))
                                        {
                                            colorCount[smallColor] = colorCount[smallColor] + ColorValue(smallColor);
                                        }
                                        else
                                        {
                                            colorCount[smallColor] = ColorValue(smallColor);
                                        }
                                        fullCount += ColorValue(smallColor);
                                    }
                                    else
                                    {
                                        emptyCount += 5;
                                    }
                                }
                            }
                        }
                        byte best = 0;
                        if(fullCount >= emptyCount)
                            best = colorCount.OrderByDescending(kv => kv.Value).First().Key;
                        if(best > 0)
                        {
                            vfinal[x, y, z] = best;
                        }
                    }
                }
            }
            foreach(var kv in specialColors)
            {
                vfinal[kv.Value.x, kv.Value.y, kv.Value.z] = kv.Key;
            }
            return vfinal;
        }

        public static byte[,,] TransformStartLarge(IEnumerable<MagicaVoxelData> voxels)
        {
            return VoxListToLargerArray(voxels, 4, 60, 60, 60);
        }
        public static byte[,,] TransformStartHuge(IEnumerable<MagicaVoxelData> voxels)
        {
            return VoxListToLargerArray(voxels, 4, 120, 120, 80);
        }
        public static byte[,,] TransformStartLarge(IEnumerable<MagicaVoxelData> voxels, int multiplier)
        {
            return VoxListToLargerArray(voxels, multiplier, 60, 60, 60);
        }
        public static byte[,,] TransformStartHuge(IEnumerable<MagicaVoxelData> voxels, int multiplier)
        {
            return VoxListToLargerArray(voxels, multiplier, 120, 120, 80);
        }
        public static byte[,,] TransformStart(IEnumerable<MagicaVoxelData> voxels, int xSize, int ySize, int zSize)
        {
            return VoxListToLargerArray(voxels, 4, xSize, ySize, zSize);
        }
        public static List<MagicaVoxelData> TransformEnd(byte[,,] colors)
        {
            return VoxArrayToListSmoothed(Shrink(colors, 4));
        }

        public static byte[,,] RotateYaw(byte[,,] colors, int degrees)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            byte[,,] vls = new byte[xSize, ySize, zSize];
            switch(degrees / 90)
            {
                case 0:
                    vls = colors.Replicate();
                    break;
                case 1:
                    for(int x = 0; x < xSize; x++)
                    {
                        for(int y = 0; y < ySize; y++)
                        {
                            for(int z = 0; z < zSize; z++)
                            {
                                vls[y, xSize - x - 1, z] = colors[x, y, z];
                            }
                        }
                    }
                    break;
                case 2:
                    for(int x = 0; x < xSize; x++)
                    {
                        for(int y = 0; y < ySize; y++)
                        {
                            for(int z = 0; z < zSize; z++)
                            {
                                vls[xSize - x - 1, ySize - y - 1, z] = colors[x, y, z];
                            }
                        }
                    }
                    break;
                case 3:
                    for(int x = 0; x < xSize; x++)
                    {
                        for(int y = 0; y < ySize; y++)
                        {
                            for(int z = 0; z < zSize; z++)
                            {
                                vls[ySize - y - 1, x, z] = colors[x, y, z];
                            }
                        }
                    }
                    break;
            }
            return vls;
        }
        public static byte[,,] RotateYawPartial(byte[,,] colors, int degrees)
        {
            if(degrees % 90 == 0)
                return RotateYaw(colors, degrees);
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            byte[,,] vls = new byte[xSize, ySize, zSize];

            double angle = (Math.PI / 180) * ((degrees + 720) % 360);
            double sn = Math.Sin(angle), cs = Math.Cos(angle);

            for(int x = 0; x < xSize; x++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    int tempX = (x - (xSize / 2));
                    int tempY = (y - (ySize / 2));
                    int x2 = (int)Math.Round((cs * tempX) + (sn * tempY) + (xSize / 2));
                    int y2 = (int)Math.Round((-sn * tempX) + (cs * tempY) + (ySize / 2));

                    for(int z = 0; z < zSize; z++)
                    {
                        if(x2 >= 0 && y2 >= 0 && x2 < xSize && y2 < ySize && colors[x, y, z] > 0)
                            vls[x2, y2, z] = colors[x, y, z];
                    }
                }
            }

            return vls;
        }
        public static byte[,,] RotatePitchPartial(byte[,,] colors, int degrees)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            byte[,,] vls = new byte[xSize, ySize, zSize];

            double angle = (Math.PI / 180) * ((degrees + 720) % 360);
            double sn = Math.Sin(angle), cs = Math.Cos(angle);

            for(int x = 0; x < xSize; x++)
            {
                for(int z = 0; z < zSize; z++)
                {
                    int tempX = (x - (xSize / 2));
                    int tempZ = (z - (zSize / 2));
                    int x2 = (int)Math.Round((cs * tempX) + (sn * tempZ) + (xSize / 2));
                    int z2 = (int)Math.Round((-sn * tempX) + (cs * tempZ) + (zSize / 2));

                    for(int y = 0; y < ySize; y++)
                    {
                        if(x2 >= 0 && z2 >= 0 && x2 < xSize && z2 < zSize && colors[x, y, z] > 0)
                            vls[x2, y, z2] = colors[x, y, z];
                    }
                }
            }

            return vls;
        }
        public static byte[,,] RotatePitchPartialSpread(byte[,,] colors, int degrees, int effectStartDegrees, byte[] spreadColor)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            byte[,,] vls = new byte[xSize, ySize, zSize];
            double finalAngle = (Math.PI / 180) * ((degrees) % 360), effectStartAngle = (Math.PI / 180) * ((effectStartDegrees) % 360);
            double increm = Math.PI / 36;

            if((degrees - effectStartDegrees + 1440) % 360 < 180)
            {
                increm *= -1.0;
            }

            double sn = 0.0, cs = 0.0;

            sn = Math.Sin(finalAngle);
            cs = Math.Cos(finalAngle);
            for(int x = 0; x < xSize; x++)
            {
                for(int z = 0; z < zSize; z++)
                {
                    int tempX = (x - (xSize / 2));
                    int tempZ = (z - (zSize / 2));
                    int x2 = (int)Math.Round((cs * tempX) + (sn * tempZ) + (xSize / 2));
                    int z2 = (int)Math.Round((-sn * tempX) + (cs * tempZ) + (zSize / 2));

                    for(int y = 0; y < ySize; y++)
                    {
                        if(x2 >= 0 && z2 >= 0 && x2 < xSize && z2 < zSize && colors[x, y, z] > 0)
                            vls[x2, y, z2] = colors[x, y, z];
                    }
                }
            }
            int sclen = 1;
            if(spreadColor != null) sclen = spreadColor.Length;

            for(double angle = finalAngle; Math.Abs(effectStartAngle - angle) >= Math.Abs(increm); angle += increm)
            {
                sn = Math.Sin(angle);
                cs = Math.Cos(angle);
                for(int x = 0; x < xSize; x++)
                {
                    for(int z = 0; z < zSize; z++)
                    {
                        int tempX = (x - (xSize / 2));
                        int tempZ = (z - (zSize / 2));
                        int x2 = (int)Math.Round((cs * tempX) + (sn * tempZ) + (xSize / 2));
                        int z2 = (int)Math.Round((-sn * tempX) + (cs * tempZ) + (zSize / 2));

                        for(int y = 0; y < ySize; y++)
                        {
                            for(int it = 0; it < sclen; it++)
                            {
                                if((spreadColor == null || colors[x, y, z] == spreadColor[it]) && colors[x, y, z] > 0 && x2 >= 0 && z2 >= 0 && x2 < xSize && z2 < zSize && vls[x2, y, z2] == 0)
                                    vls[x2, y, z2] = colors[x, y, z];
                            }
                        }
                    }
                }
            }
            return vls;
        }
        public static byte[,,] RotateRollPartial(byte[,,] colors, int degrees)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            byte[,,] vls = new byte[xSize, ySize, zSize];
            double angle = (Math.PI / 180) * ((degrees + 720) % 360);
            double sn = Math.Sin(angle), cs = Math.Cos(angle);

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    int tempY = (y - (ySize / 2));
                    int tempZ = (z - (zSize / 2));
                    int y2 = (int)Math.Round((-sn * tempZ) + (cs * tempY) + (ySize / 2));
                    int z2 = (int)Math.Round((cs * tempZ) + (sn * tempY) + (zSize / 2));

                    for(int x = 0; x < xSize; x++)
                    {
                        if(z2 >= 0 && y2 >= 0 && z2 < zSize && y2 < ySize && colors[x, y, z] > 0)
                            vls[x, y2, z2] = colors[x, y, z];
                    }
                }
            }

            return vls;
        }
        public static byte[,,] ScalePartial(byte[,,] colors, double scale)
        {
            if(scale == 1.0) return colors;
            return ScalePartial(colors, scale, scale, scale);
        }
        public static byte[,,] ScalePartial(byte[,,] colors, double xScale, double yScale, double zScale)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            if(xScale <= 0 || yScale <= 0 || zScale <= 0)
                return colors;
            byte[,,] vls = new byte[(int)(xSize * xScale), (int)(ySize * yScale), (int)(zSize * zScale)];

            for(int z = 0; z < zSize; z++)
            {
                for(int y = 0; y < ySize; y++)
                {
                    for(int x = 0; x < xSize; x++)
                    {
                        for(double xsc = 0.0; xsc < xScale; xsc += 1.0)
                        {
                            for(double ysc = 0.0; ysc < yScale; ysc += 1.0)
                            {
                                for(double zsc = 0.0; zsc < zScale; zsc += 1.0)
                                {
                                    int tempX = (x - (xSize / 2));
                                    int tempY = (y - (ySize / 2));
                                    int tempZ = (z - (zSize / 2));
                                    int x2 = (int)Math.Round((xScale * tempX) + (xScale * xSize / 2) + ((tempX < 0) ? xsc : -xsc));
                                    int y2 = (int)Math.Round((yScale * tempY) + (yScale * ySize / 2) + ((tempY < 0) ? ysc : -ysc));
                                    int z2 = (int)Math.Round((zScale * tempZ) + (zScale * zSize / 2) + ((tempZ < 0) ? zsc : -zsc));

                                    if(colors[x, y, z] > 0 && x2 >= 0 && y2 >= 0 && z2 >= 0 && x2 < xSize * xScale && y2 < ySize * yScale && z2 < zSize * zScale)
                                        vls[x2, y2, z2] = colors[x, y, z];
                                }
                            }
                        }
                    }
                }
            }
            return vls;
        }

        public static byte[] dismiss = new byte[] { 0, VoxelLogic.clear, 253 - 17 * 4, 253 - 18 * 4, 253 - 19 * 4, 253 - 20 * 4, 253 - 25 * 4 };

        public static void MorphInPlace(ref byte[,,] colors, byte[] changing, float chance)
        {
            if(chance <= 0)
                return;
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);

            for(int vx = 0; vx < xSize; vx++)
            {
                for(int vy = 0; vy < ySize; vy++)
                {
                    for(int vz = 0; vz < zSize; vz++)
                    {
                        if(!dismiss.IsPresent(colors[vx, vy, vz]) && rng.NextDouble() < chance)
                        {
                            colors[vx, vy, vz] = changing.RandomElement();
                        }
                    }
                }
            }
        }
        public static byte[][,,] FieryExplosionW(byte[,,] colors, bool blowback, bool shadowless)
        {
            int xSize = colors.GetLength(0), ySize = colors.GetLength(1), zSize = colors.GetLength(2);
            byte[][,,] voxelFrames = new byte[13][,,];
            voxelFrames[0] = colors;

            for(int f = 1; f < 5; f++)
            {
                byte[,,] working = voxelFrames[f - 1].Replicate(), vls = new byte[working.GetLength(0), working.GetLength(1), working.GetLength(2)]; ;

                int[] minX = new int[zSize];
                int[] maxX = new int[zSize];
                float[] midX = new float[zSize];
                for(int level = 0; level < zSize; level++)
                {
                    minX[level] = working.MinXAtZ(level, dismiss);
                    maxX[level] = working.MaxXAtZ(level, dismiss);
                    midX[level] = (maxX[level] + minX[level]) / 2F;
                }

                int[] minY = new int[zSize];
                int[] maxY = new int[zSize];
                float[] midY = new float[zSize];
                for(int level = 0; level < zSize; level++)
                {
                    minY[level] = working.MinYAtZ(level, dismiss);
                    maxY[level] = working.MaxYAtZ(level, dismiss);
                    midY[level] = (maxY[level] + minY[level]) / 2F;
                }

                int minZ = working.MinZ(dismiss);
                int maxZ = working.MaxZ(dismiss);
                float midZ = (maxZ + minZ) / 2F;

                for(int vx = 0; vx < xSize; vx++)
                {
                    for(int vy = 0; vy < ySize; vy++)
                    {
                        for(int vz = 0; vz < zSize; vz++)
                        {
                            byte vcolor = voxelFrames[f - 1][vx, vy, vz];

                            if(vcolor == 0) continue;

                            int x = 0, y = 0, z = 0;
                            byte color = 0;
                            int c = ((255 - vcolor) % 4 == 0) ? (255 - vcolor) / 4 + VoxelLogic.wcolorcount : (253 - vcolor) / 4;
                            if(c == 8 || c == 9) //flesh
                                color = (byte)((rng.Next(1 + f) == 0) ? 253 - 34 * 4 : (rng.Next(8) == 0) ? 253 - 19 * 4 : vcolor); //random transform to guts
                            else if(c == 34) //guts
                                color = (byte)((rng.Next(18) == 0) ? 253 - 19 * 4 : vcolor); //random transform to orange fire
                            else if(c == VoxelLogic.clear || vcolor == 0) //clear
                                color = 0; //clear stays clear
                            else if(c == 16)
                                color = 0; //clear inner shadow
                            else if(c == 25)
                                color = 253 - 25 * 4; //shadow stays shadow
                            else if(c == 27)
                                color = 253 - 27 * 4; //water stays water
                            else if(c >= VoxelLogic.wcolorcount && c < VoxelLogic.wcolorcount + 5)
                                color = vcolor; // falling water stays falling water
                            else if(c == 40)
                                color = 253 - 20 * 4; //flickering sparks become normal sparks
                            else if(c >= 21 && c <= 24) //lights
                                color = 253 - 35 * 4; //glass color for broken lights
                            else if(c == 35) //windows
                                color = (byte)((rng.Next(3) == 0) ? VoxelLogic.clear : vcolor); //random transform to clear
                            else if(c == 36) //rotor contrast
                                color = 253 - 0 * 4; //"foot contrast" or "raw metal contrast" color for broken rotors contrast
                            else if(c == 37) //rotor
                                color = 253 - 1 * 4; //"foot" or "raw metal" color for broken rotors
                            else if(c == 38 || c == 39)
                                color = VoxelLogic.clear; //clear non-active rotors
                            else if(c == 19) //orange fire
                                color = (byte)((rng.Next(3) <= 1) ? 253 - 18 * 4 : ((rng.Next(5) == 0) ? 253 - 17 * 4 : vcolor)); //random transform to yellow fire or smoke
                            else if(c == 18) //yellow fire
                                color = (byte)((rng.Next(4) <= 1) ? 253 - 19 * 4 : ((rng.Next(4) == 0) ? 253 - 20 * 4 : vcolor)); //random transform to orange fire or sparks
                            else if(c == 20) //sparks
                                color = (byte)((rng.Next(4) == 0) ? VoxelLogic.clear : vcolor); //random transform to clear
                            else if(c == 17)
                                color = vcolor; // smoke stays smoke
                            else
                                color = (byte)((rng.Next(9 - f) == 0) ? 253 - ((rng.Next(4) == 0) ? 18 * 4 : 19 * 4) : vcolor); //random transform to orange or yellow fire
                            float xMove = 0, yMove = 0, zMove = 0;

                            if(color == 253 - 19 * 4 || color == 253 - 18 * 4 || color == 253 - 17 * 4)
                            {
                                zMove = (f - 1) * 1.8f;
                                xMove = (float)(rng.NextDouble() * 2.0 - 1.0);
                                yMove = (float)(rng.NextDouble() * 2.0 - 1.0);
                            }
                            else
                            {
                                if(vx > midX[vz])
                                    xMove = ((0 - rng.Next(3) - ((blowback) ? 9 : 0) + (vx - midX[vz])) * 25F / f * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                else if(vx < midX[vz])
                                    xMove = ((0 + rng.Next(3) - ((blowback) ? 8 : 0) - midX[vz] + vx) * 25F / f * ((vz - minZ + 1) / (maxZ - minZ + 1F)));// / 300F) * (vz + 5); //5 -
                                if(vy > midY[vz])
                                    yMove = ((0 - rng.Next(3) + (vy - midY[vz])) * 25F / f * ((vz - minZ + 1) / (maxZ - minZ + 1F))); //maxX[vz] - minX[vz]
                                else if(vy < midY[vz])
                                    yMove = ((0 + rng.Next(3) - midY[vz] + vy) * 25F / f * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                if(color == 253 - 20 * 4)
                                {
                                    zMove = 0.1f;
                                    xMove *= 2;
                                    yMove *= 2;
                                }
                                else if(minZ > 0)
                                    zMove = ((vz) * (1 - f) / 6F);
                                else
                                    zMove = (vz / ((maxZ + 1 + midZ) * (0.15F))) * (4 - f) * 1.1F;
                            }

                            if(xMove > 20) xMove = 20;
                            if(xMove < -20) xMove = -20;
                            if(yMove > 20) yMove = 20;
                            if(yMove < -20) yMove = -20;
                            //float magnitude = Math.Abs(xMove) + Math.Abs(yMove);
                            float magnitude = (float)Math.Sqrt(xMove * xMove + yMove * yMove);
                            if(xMove > 0)
                            {
                                float nv = vx + (float)rng.NextDouble() * ((xMove / magnitude) * f / 0.3F) + (float)(rng.NextDouble() * 6.0 - 3.0); //(Math.Sqrt(Math.Abs(Math.Abs((xMove * f / 3)) - Math.Abs((yMove * f / 4)))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > xSize - 1)
                                {
                                    nv = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                x = (byte)(Math.Floor(nv));
                            }
                            else if(xMove < 0)
                            {
                                float nv = vx - (float)rng.NextDouble() * ((xMove / magnitude) * f / -0.3F) + (float)(rng.NextDouble() * 6.0 - 3.0); //(float)(Math.Sqrt(Math.Abs(Math.Abs((xMove * f / 3)) - Math.Abs((yMove * f / 4)))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > xSize - 1)
                                {
                                    nv = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                x = (byte)(Math.Floor(nv));
                            }
                            else
                            {
                                if(vx < 0)
                                {
                                    x = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(vx > xSize - 1)
                                {
                                    x = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                else x = vx;
                            }
                            if(yMove > 0)
                            {
                                float nv = vy + (float)rng.NextDouble() * ((yMove / magnitude) * f / 0.3F) + (float)(rng.NextDouble() * 6.0 - 3.0); //(float)(Math.Sqrt(Math.Abs(Math.Abs((yMove * f / 3)) - Math.Abs((xMove * f / 4)))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > ySize - 1)
                                {
                                    nv = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                y = (byte)(Math.Floor(nv));
                            }
                            else if(yMove < 0)
                            {
                                float nv = vy - (float)rng.NextDouble() * ((yMove / magnitude) * f / -0.3F) + (float)(rng.NextDouble() * 6.0 - 3.0); //(float)(Math.Sqrt(Math.Abs(Math.Abs((yMove * f / 3)) - Math.Abs((xMove * f / 4)))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > ySize - 1)
                                {
                                    nv = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                y = (byte)(Math.Floor(nv));
                            }
                            else
                            {
                                if(vy < 0)
                                {
                                    y = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(vy > ySize - 1)
                                {
                                    y = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                else y = vy;
                            }

                            if(zMove != 0)
                            {
                                float nv = (vz + (zMove * 1.5f / f));

                                if(nv <= 0 && f < 8) nv = rng.Next(2); //bounce
                                else if(nv < 0) nv = 0;

                                if(nv > zSize - 1) nv = zSize - 1;
                                z = (byte)Math.Round(nv);
                            }
                            else
                            {
                                z = vz;
                            }
                            vls[x, y, z] = color;
                        }
                    }
                }
                voxelFrames[f] = vls;
            }
            for(int f = 5; f < 13; f++)
            {
                byte[,,] working = voxelFrames[f - 1].Replicate(), vls = new byte[working.GetLength(0), working.GetLength(1), working.GetLength(2)];

                int[] minX = new int[zSize];
                int[] maxX = new int[zSize];
                float[] midX = new float[zSize];
                for(int level = 0; level < zSize; level++)
                {
                    minX[level] = working.MinXAtZ(level, dismiss);
                    maxX[level] = working.MaxXAtZ(level, dismiss);
                    midX[level] = (maxX[level] + minX[level]) / 2F;
                }

                int[] minY = new int[zSize];
                int[] maxY = new int[zSize];
                float[] midY = new float[zSize];
                for(int level = 0; level < zSize; level++)
                {
                    minY[level] = working.MinYAtZ(level, dismiss);
                    maxY[level] = working.MaxYAtZ(level, dismiss);
                    midY[level] = (maxY[level] + minY[level]) / 2F;
                }

                int minZ = working.MinZ(dismiss);
                int maxZ = working.MaxZ(dismiss);
                float midZ = (maxZ + minZ) / 2F;

                for(int vx = 0; vx < xSize; vx++)
                {
                    for(int vy = 0; vy < ySize; vy++)
                    {
                        for(int vz = 0; vz < zSize; vz++)
                        {
                            byte vcolor = voxelFrames[f - 1][vx, vy, vz];

                            if(vcolor == 0) continue;

                            int x = 0, y = 0, z = 0;
                            byte color = 0;
                            int c = ((255 - vcolor) % 4 == 0) ? (255 - vcolor) / 4 + VoxelLogic.wcolorcount : (253 - vcolor) / 4;
                            if(c == 8 || c == 9) //flesh
                                color = (byte)((rng.Next(f) == 0) ? 253 - 34 * 4 : (rng.Next(6) == 0 && f < 10) ? 253 - 19 * 4 : vcolor); //random transform to guts
                            else if(c == 34) //guts
                                color = (byte)((rng.Next(20) == 0 && f < 10) ? 253 - 19 * 4 : vcolor); //random transform to orange fire
                            else if(c == VoxelLogic.clear || vcolor == 0) //clear
                                color = 0; //clear stays clear
                            else if(c == 16)
                                color = 0; //VoxelLogic.clear inner shadow
                            else if(c == 25)
                                color = 253 - 25 * 4; //shadow stays shadow
                            else if(c == 27)
                                color = 253 - 27 * 4; //water stays water
                            else if(c >= VoxelLogic.wcolorcount && c < VoxelLogic.wcolorcount + 5)
                                color = (byte)(255 - (c - VoxelLogic.wcolorcount) * 4); // falling water stays falling water
                            else if(c == 40)
                                color = 253 - 20 * 4; //flickering sparks become normal sparks
                            else if(c >= 21 && c <= 24) //lights
                                color = 253 - 35 * 4; //glass color for broken lights
                            else if(c == 35) //windows
                                color = (byte)((rng.Next(3) == 0) ? VoxelLogic.clear : vcolor); //random transform to VoxelLogic.clear
                            else if(c == 36) //rotor contrast
                                color = 253 - 0 * 4; //"foot contrast" color for broken rotors contrast
                            else if(c == 37) //rotor
                                color = 253 - 1 * 4; //"foot" color for broken rotors
                            else if(c == 38 || c == 39)
                                color = VoxelLogic.clear; //VoxelLogic.clear non-active rotors
                            else if(c == 19) //orange fire
                                color = (byte)((rng.Next(9) + 2 <= f) ? 253 - 17 * 4 : ((rng.Next(3) <= 1) ? 253 - 18 * 4 : ((rng.Next(3) == 0) ? 253 - 17 * 4 : vcolor))); //random transform to yellow fire or smoke
                            else if(c == 18) //yellow fire
                                color = (byte)((rng.Next(9) + 1 <= f) ? 253 - 17 * 4 : ((rng.Next(3) <= 1) ? 253 - 19 * 4 : ((rng.Next(4) == 0) ? 253 - 17 * 4 : ((rng.Next(4) == 0) ? 253 - 20 * 4 : vcolor)))); //random transform to orange fire, smoke, or sparks
                            else if(c == 20) //sparks
                                color = (byte)((rng.Next(4) > 0 && rng.Next(12) > f) ? vcolor : VoxelLogic.clear); //random transform to VoxelLogic.clear
                            else if(c == 17) //smoke
                                color = (byte)((rng.Next(10) + 3 <= f) ? VoxelLogic.clear : 253 - 17 * 4); //random transform to VoxelLogic.clear
                            else
                                color = (byte)((rng.Next(f * 4) <= 6) ? 253 - ((rng.Next(4) == 0) ? 18 * 4 : 19 * 4) : vcolor); //random transform to orange or yellow fire

                            float xMove = 0, yMove = 0, zMove = 0;
                            if(color == 253 - 19 * 4 || color == 253 - 18 * 4 || color == 253 - 17 * 4)
                            {
                                zMove = f * 0.5f;
                                xMove = (float)(rng.NextDouble() * 2.0 - 1.0);
                                yMove = (float)(rng.NextDouble() * 2.0 - 1.0);
                            }
                            else
                            {
                                /*
                                 if (vx > midX[vz])
                                    xMove = ((0 - rng.Next(3) - ((blowback) ? 9 : 0) + (vx - midX[vz])) * 2F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                //for lower values: distance from current voxel x to center + (between 2 to 0), times variable based on height
                                else if (vx < midX[vz])
                                    xMove = ((0 + rng.Next(3) - ((blowback) ? 8 : 0) - midX[vz] + vx) * 2F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));// / 300F) * (vz + 5); //5 -
                                if (vy > midY[vz])
                                    yMove = ((0 - rng.Next(3) + (vy - midY[vz])) * 2F * ((vz - minZ + 3) / (maxZ - minZ + 1F))); //maxX[vz] - minX[vz]
                                else if (vy < midY[vz])
                                    yMove = ((0 + rng.Next(3) - midY[vz] + vy) * 2F * ((vz - minZ + 3) / (maxZ - minZ + 1F)));
                                 */


                                if(vx > midX[vz])
                                    xMove = ((0 - rng.Next(3) - ((blowback) ? 7 : 0) + (vx - midX[vz])) / (f + 8) * 25F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                else if(vx < midX[vz])
                                    xMove = ((0 + rng.Next(3) - ((blowback) ? 6 : 0) - midX[vz] + vx) / (f + 8) * 25F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                if(vy > midY[vz])
                                    yMove = ((0 - rng.Next(3) + (vy - midY[vz])) / (f + 8) * 25F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));
                                else if(vy < midY[vz])
                                    yMove = ((0 + rng.Next(3) - midY[vz] + vy) / (f + 8) * 25F * ((vz - minZ + 1) / (maxZ - minZ + 1F)));

                                if(color == 253 - 20 * 4)
                                {
                                    zMove = 0.1f;
                                    xMove *= 2;
                                    yMove *= 2;
                                }
                                else if(f < 6 && minZ == 0)
                                    zMove = (vz / ((maxZ + 1) * (0.2F))) * (5 - f) * 0.6F;
                                else
                                    zMove = (1 - f * 1.85F);
                            }
                            if(vz <= 1 && f >= 10)
                            {
                                xMove = 0;
                                yMove = 0;
                            }
                            if(xMove > 20) xMove = 20;
                            if(xMove < -20) xMove = -20;
                            if(yMove > 20) yMove = 20;
                            if(yMove < -20) yMove = -20;
                            // float magnitude = Math.Abs(xMove) + Math.Abs(yMove);
                            float magnitude = (float)Math.Sqrt(xMove * xMove + yMove * yMove);

                            if(xMove > 0)
                            {
                                float nv = vx + (float)rng.NextDouble() * ((xMove / magnitude) * 35F / f) + (float)(rng.NextDouble() * 8.0 - 4.0);
                                //                        float nv = (float)(vx + Math.Sqrt(Math.Abs(Math.Abs(xMove / (0.07f * (f + 5))) - Math.Abs((yMove / (0.09f * (f + 5)))))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > xSize - 1)
                                {
                                    nv = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                x = (byte)(Math.Floor(nv));
                            }
                            else if(xMove < 0)
                            {
                                float nv = vx - (float)rng.NextDouble() * ((xMove / magnitude) * -35F / f) + (float)(rng.NextDouble() * 8.0 - 4.0);

                                //float nv = (float)(vx - Math.Sqrt(Math.Abs(Math.Abs(xMove / (0.07f * (f + 5))) - Math.Abs((yMove / (0.09f * (f + 5)))))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > xSize - 1)
                                {
                                    nv = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                x = (byte)(Math.Floor(nv));
                            }
                            else
                            {
                                if(vx < 0)
                                {
                                    x = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(vx > xSize - 1)
                                {
                                    x = xSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                else x = vx;
                            }
                            if(yMove > 0)
                            {
                                float nv = vy + (float)rng.NextDouble() * ((yMove / magnitude) * 35F / f) + (float)(rng.NextDouble() * 8.0 - 4.0);
                                //float nv = (float)(vy + Math.Sqrt(Math.Abs(Math.Abs(yMove / (0.07f * (f + 5))) - Math.Abs((xMove / (0.09f * (f + 5)))))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > ySize - 1)
                                {
                                    nv = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                y = (byte)(Math.Floor(nv));
                            }
                            else if(yMove < 0)
                            {
                                float nv = vy - (float)rng.NextDouble() * ((yMove / magnitude) * -35F / f) + (float)(rng.NextDouble() * 8.0 - 4.0);
                                //(float)(vy - Math.Sqrt(Math.Abs(Math.Abs(yMove / (0.07f * (f + 5))) - Math.Abs((xMove / (0.09f * (f + 5)))))) + (float)(rng.NextDouble() * 2.0 - 1.0));
                                if(nv < 0)
                                {
                                    nv = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(nv > ySize - 1)
                                {
                                    nv = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                y = (byte)(Math.Ceiling(nv));
                            }
                            else
                            {
                                if(vy < 0)
                                {
                                    y = 0;
                                    color = VoxelLogic.clear;
                                }
                                if(vy > ySize - 1)
                                {
                                    y = ySize - 1;
                                    color = VoxelLogic.clear;
                                }
                                else y = vy;
                            }

                            if(zMove != 0)
                            {
                                float nv = (vz + (zMove * 1.3f));

                                if(nv <= 0 && f < 8) nv = rng.Next(2); //bounce
                                else if(nv < 0) nv = 0;

                                if(nv > zSize - 1)
                                {
                                    nv = zSize - 1;
                                    color = VoxelLogic.clear;
                                }
                                z = (byte)Math.Round(nv);
                            }
                            else
                            {
                                z = vz;
                            }
                            vls[x, y, z] = color;
                        }
                    }
                }
                voxelFrames[f] = vls;

            }

            byte[][,,] frames = new byte[12][,,];

            for(int f = 1; f < 13; f++)
            {
                frames[f - 1] = voxelFrames[f];
            }
            return frames;
        }

    }
}
