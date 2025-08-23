using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTA.Game.Attacking
{
    public class Sector
    {
        private int attackerX, attackerY, attackX, attackY;
        private int degree, sectorsize, leftside, rightside;
        private int distance;
        private bool addextra;
        private int angle = 360;

        public Sector(int attackerX, int attackerY, int attackX, int attackY)
        {
            this.attackerX = attackerX;
            this.attackerY = attackerY;
            this.attackX = attackX;
            this.attackY = attackY;
            this.degree = Kernel.GetDegree(attackerX, attackX, attackerY, attackY);
            this.addextra = false;
        }

        public void Arrange(int sectorsize, int distance)
        {
            this.distance = Math.Min(distance, 14);
            this.sectorsize = sectorsize;
            this.leftside = this.degree - (sectorsize / 2);
            if (this.leftside < 0)
                this.leftside += angle;
            this.rightside = this.degree + (sectorsize / 2);
            if (this.leftside < this.rightside || this.rightside - this.leftside != this.sectorsize)
            {
                this.rightside += angle;
                this.addextra = true;
            }
        }


        public bool Inside(int X, int Y)
        {
            if (Kernel.GetDistance((ushort)X, (ushort)Y, (ushort)attackerX, (ushort)attackerY) <= distance)
            {
                int degree = Kernel.GetDegree(attackerX, X, attackerY, Y);
                if (this.addextra)
                    degree += angle;
                if (degree >= this.leftside && degree <= this.rightside)
                    return true;
            }
            return false;
        }
    }
    public class Fan
    {
        private ushort UserX, UserY = 0;
        private ushort SourceX, SourceY = 0;

        private int Range = 0;
        private int Width = 0;

        public Fan(ushort x, ushort y, ushort x2, ushort y2, int nRange, int nWidth)
        {
            UserX = x;
            UserY = y;

            SourceX = x2;
            SourceY = y2;

            Range = nRange;
            Width = nWidth;
        }

        public bool IsInFan(ushort TargetX, ushort TargetY)
        {
            if (UserX == SourceX && UserY == SourceY)
                return false;

            if (GetDistance(UserX, UserY, TargetX, TargetY) > Range)
                return false;

            double PI = Math.PI;
            double fRadianDelta = (PI * Width / 180) / 2;
            float fCenterLine = GetRadian(UserX, UserY, TargetX, TargetY);
            float fTargetLine = GetRadian(SourceX, SourceY, UserX, UserY);
            float fDelta = Math.Abs(fCenterLine - fTargetLine);
            if (fDelta <= fRadianDelta || fDelta >= 2 * PI - fRadianDelta)
                return false;

            return true;
        }
        private static float GetRadian(float posSourX, float posSourY, float posTargetX, float posTargetY)
        {
            float PI = 3.1415926535f;
            float fDeltaX = posTargetX - posSourX;
            float fDeltaY = posTargetY - posSourY;
            float fDistance = SquareRootFloat(fDeltaX * fDeltaX + fDeltaY * fDeltaY);

            double fRadian = (float)Math.Asin(fDeltaX / fDistance);

            return (float)(fDeltaY > 0 ? (PI / 2 - fRadian) : (PI + fRadian + PI / 2));
        }
        unsafe static float SquareRootFloat(float number)
        {
            long i;
            float x, y;
            const float f = 1.5F;

            x = number * 0.5F;
            y = number;
            i = *(long*)&y;
            i = 0x5f3759df - (i >> 1);
            y = *(float*)&i;
            y = y * (f - (x * y * y));
            y = y * (f - (x * y * y));
            return number * y;
        }
        private static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            short x = 0;
            short y = 0;
            if (X >= X2)
            {
                x = (short)(X - X2);
            }
            else if (X2 >= X)
            {
                x = (short)(X2 - X);
            }
            if (Y >= Y2)
            {
                y = (short)(Y - Y2);
            }
            else if (Y2 >= Y)
            {
                y = (short)(Y2 - Y);
            }
            if (x > y)
                return x;
            else
                return y;
        }
    }
}
