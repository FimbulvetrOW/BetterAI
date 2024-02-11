using System;
using UnityEngine;
using Mohawk.SystemCore;

namespace BetterAI
{
    public class RoundFractal
    {
//methods aren't virtual, so no inheritance from Fractal
        private int miExternalWidth = 0;
        private int miWidth = 0;
        private int getWidth()
        {
            return miWidth;
        }

        private int miExternalHeight = 0;
        private int miHeight = 0;
        private int getHeight()
        {
            return miHeight;
        }

        private int miRange = 0;
        private int getRange()
        {
            return miRange;
        }

        private int miNoisePercent = 0;
        private int getNoisePercent()
        {
            return miNoisePercent;
        }

        private int[] maiValues = null;
        private int getValue(int iIndex)
        {
            return maiValues[iIndex];
        }
        private int getGridIndex(int iX, int iY)
        {
            return Mathf.Clamp(iX, 0, getWidth() - 1) + (Mathf.Clamp(iY, 0, getHeight() - 1) * getWidth());
        }
        private int getValue(int iX, int iY)
        {
            return getValue(getGridIndex(iX, iY));
        }
        public int getTileValuePercent(int iX, int iY)
        {
            int localXfloor = miWidth * iX / miExternalWidth;
            int localXceil = localXfloor + 1;
            int localYfloor = miHeight * iY / miExternalHeight;
            int localYceil = localYfloor + 1;
            int xPercent = ((miWidth * iX * 100) / miExternalWidth - 100 * localXfloor);
            int yPercent = ((miHeight * iY * 100) / miExternalHeight - 100 * localYfloor);

            int iP1 = (100 - xPercent) * getValuePercent(getValue(localXfloor, localYfloor)) + xPercent * getValuePercent(getValue(localXceil, localYfloor));
            int iP2 = (100 - xPercent) * getValuePercent(getValue(localXfloor, localYceil)) + xPercent * getValuePercent(getValue(localXceil, localYceil));
            return ((100 - yPercent) * iP1 + yPercent * iP2) / 10000;
        }
        private void setValue(int iX, int iY, int iRange)
        {
            maiValues[getGridIndex(iX, iY)] = Mathf.Clamp(iRange, 0, (getRange() - 1));
        }

        private int[] maiValuePercentage = null;
        private int getValuePercent(int iIndex)
        {
            return maiValuePercentage[iIndex];
        }
        private void setValuePercent(int iIndex, int iNewValue)
        {
            maiValuePercentage[iIndex] = iNewValue;
        }

        private Mohawk.SystemCore.Random mpRandom = null;
        private int randomNext(int iRange)
        {
            return mpRandom.Next(iRange);
        }

        [Flags]
        public enum EdgeTypes
        {
            None = 0x0,
            East = 0x1,
            West = 0x2,
            North = 0x4,
            South = 0x8,
            EastWestCenter = 0x10,
            NorthSouthCenter = 0x20
        }


        //borrowed from Utils
        private int euclideanDistanceSquared(int iX1, int iY1, int iX2, int iY2)
        {
            int iXdistance = 2 * (iX2 - iX1);
            int iYdistance = iY2 - iY1;

            if (iYdistance % 2 != 0)
            { //iXdistance should either get +1 or - 1
                if (iY1 % 2 != 0)
                {
                    iXdistance -= 1;
                }
                else
                {
                    iXdistance += 1;
                }
            }


            return (iXdistance * iXdistance) + (3 * iYdistance * iYdistance);
        }

/*####### Better Old World AI - Base DLL #######
  ### Fractal with round edges         START ###
  ##############################################*/
        //private int distance(int iX1, int iY1, int iX2, int iY2)
        //{
        //    iX1 += (iY1 >> 1);
        //    iX2 += (iY2 >> 1);

        //    int iDX = iX2 - iX1;
        //    int iDY = iY2 - iY1;

        //    //calculate sign
        //    bool bSX = (iDX >= 0);
        //    bool bSY = (iDY >= 0);
        //    iDX = bSX ? iDX : -iDX; //abs(iDX)
        //    iDY = bSY ? iDY : -iDY; //abs(iDY)

        //    if (bSX == bSY)
        //    {
        //        return (iDX > iDY) ? iDX : iDY;
        //    }
        //    else
        //    {
        //        return iDX + iDY;
        //    }
        //}
/*####### Better Old World AI - Base DLL #######
  ### Fractal with round edges           END ###
  ##############################################*/

        private bool isOnEdge(EdgeTypes eEdge, int iX, int iY)
        {
            if ((eEdge & EdgeTypes.West) != 0 && iX == 0)
            {
                return true;
            }
            if ((eEdge & EdgeTypes.East) != 0 && iX == getWidth() - 1)
            {
                return true;
            }
            if ((eEdge & EdgeTypes.South) != 0 && iY == 0)
            {
                return true;
            }
            if ((eEdge & EdgeTypes.North) != 0 && iY == getHeight() - 1)
            {
                return true;
            }
            if ((eEdge & EdgeTypes.NorthSouthCenter) != 0 && iY == (getHeight() - 1) / 2)
            {
                return true;
            }
            if ((eEdge & EdgeTypes.EastWestCenter) != 0 && iX == (getWidth() - 1) / 2)
            {
                return true;
            }
/*####### Better Old World AI - Base DLL #######
  ### Fractal with round edges         START ###
  ##############################################*/
            int iCenterX = getHeight() / 2;
            int iCenterY = getWidth() / 2;
            
            int iMaxDistanceSquared = Math.Max(euclideanDistanceSquared(0, 0, 0, iCenterY), euclideanDistanceSquared(0, 0, iCenterX, 0)); //is this different from Math.Max(getHeight() / 2, getWidth() / 2) - 1 ?
            //int iXDistanceFromCenter = iX - iCenterX;
            //int iYDistanceFromCenter = iY - iCenterY;

            //if ((eEdge & EdgeTypes.East) != 0 && (eEdge & EdgeTypes.North) != 0 && (iXDistanceFromCenter + iYDistanceFromCenter  > iMaxDistance))
            if ((eEdge & EdgeTypes.East) != 0 && (eEdge & EdgeTypes.North) != 0 && 
                (iX > iCenterX && iY > iCenterY && euclideanDistanceSquared(iX, iY, iCenterX, iCenterY) > iMaxDistanceSquared))
            {
                return true;
            }

            //if ((eEdge & EdgeTypes.West) != 0 && (eEdge & EdgeTypes.South) != 0 && (-iXDistanceFromCenter + -iYDistanceFromCenter > iMaxDistance))
            if ((eEdge & EdgeTypes.West) != 0 && (eEdge & EdgeTypes.South) != 0 && 
                (iX < iCenterX && iY < iCenterY && euclideanDistanceSquared(iX, iY, iCenterX, iCenterY) > iMaxDistanceSquared))
            {
                return true;
            }
            //if ((eEdge & EdgeTypes.West) != 0 && (eEdge & EdgeTypes.North) != 0 && (-iXDistanceFromCenter + iYDistanceFromCenter > iMaxDistance))
            if ((eEdge & EdgeTypes.West) != 0 && (eEdge & EdgeTypes.North) != 0 &&
                (iX < iCenterX && iY > iCenterY && euclideanDistanceSquared(iX, iY, iCenterX, iCenterY) > iMaxDistanceSquared))
            {
                return true;
            }
            //if ((eEdge & EdgeTypes.East) != 0 && (eEdge & EdgeTypes.South) != 0 && (iXDistanceFromCenter + -iYDistanceFromCenter > iMaxDistance))
            if ((eEdge & EdgeTypes.East) != 0 && (eEdge & EdgeTypes.South) != 0 &&
                (iX > iCenterX && iY < iCenterY && euclideanDistanceSquared(iX, iY, iCenterX, iCenterY) > iMaxDistanceSquared))
            {
                return true;
            }
/*####### Better Old World AI - Base DLL #######
  ### Fractal with round edges           END ###
  ##############################################*/

            return false;
        }

        // This function scales the size of the data field with the incoming width and height values. Since the sample range has a different output at different scales, this
        // usage will have different effects on different size maps -- generally an undesirable result, but available if desired.
        public void initScalingLayer(ulong ulSeed, int iWidth, int iHeight, int iSample, int iNoisePercent, int iRange, int iBoundaryValue, EdgeTypes eFixedBoundaries)
        {
            mpRandom = new Mohawk.SystemCore.Random(ulSeed);

            miExternalWidth = iWidth;
            miExternalHeight = iHeight;

            // make power of 2 + 1
            miWidth = 1;
            while (miWidth < miExternalWidth)
            {
                miWidth *= 2;
            }
            ++miWidth;

            // make power of 2 + 1
            miHeight = 1;
            while (miHeight < miExternalHeight)
            {
                miHeight *= 2;
            }
            ++miHeight;

            miNoisePercent = iNoisePercent;
            miRange = iRange;

            int iNumTiles = miWidth * miHeight;
            maiValues = new int[iNumTiles];
            maiValuePercentage = new int[iRange];

            int iStep = 1 << iSample;

            for (int iX = 0; iX < getWidth(); iX += iStep)
            {
                for (int iY = 0; iY < getHeight(); iY += iStep)
                {
                    if (!isOnEdge(eFixedBoundaries, iX, iY))
                    {
                        setValue(iX, iY, randomNext(getRange()));
                    }
                    else
                    {
                        setValue(iX, iY, iBoundaryValue);
                    }
                }
            }

            for (int iI = 0; iI < iSample; iI++)
            {
                diamondSquare(1 << (iSample - iI));
            }

            calculatePercentValues();
        }

        // Standard fractal usage. Exponent controls the size of the data field, thus also the effect that will be generated by each value of Sample size.
        public void init(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent, int iRange, int iBoundaryValue, EdgeTypes eFixedBoundaries)
        {
            miExternalWidth = iWidth;
            miExternalHeight = iHeight;
            mpRandom = new Mohawk.SystemCore.Random(ulSeed);

            miWidth = iWidth;
            miHeight = iHeight;

            // Valid range of exponent values is 5, 6, 7, 8, 9. Below 5 (32x32) there is too little definition. Above 9 (512x512) there is too much memory and processing consumption.
            //MohawkAssert.Assert(iExponentX >= 5 && iExponentX <= 9);
            //MohawkAssert.Assert(iExponentY >= 5 && iExponentY <= 9);
            miWidth = (int)Math.Pow(2, iExponentX) + 1;
            miHeight = (int)Math.Pow(2, iExponentY) + 1;

            miNoisePercent = iNoisePercent;
            miRange = iRange;

            int iNumTiles = miWidth * miHeight;
            maiValues = new int[iNumTiles];
            maiValuePercentage = new int[iRange];

            int iStep = 1 << iSample; // Valid range for iSample is "1 to n", where n increases slightly if larger Exponents are in use. 1-3 for Exp 5, 1-7 for Exp 9.

            for (int iX = 0; iX < getWidth(); iX += iStep)
            {
                for (int iY = 0; iY < getHeight(); iY += iStep)
                {
                    if (!isOnEdge(eFixedBoundaries, iX, iY))
                    {
                        setValue(iX, iY, randomNext(getRange()));
                    }
                    else
                    {
                        setValue(iX, iY, iBoundaryValue);
                    }
                }
            }

            for (int iI = 0; iI < iSample; iI++)
            {
                diamondSquare(1 << (iSample - iI));
            }

            calculatePercentValues();
        }

        public void initContained(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent, int iRange, int iBoundaryValue)
        {
            EdgeTypes box_edge_types = RoundFractal.EdgeTypes.East | RoundFractal.EdgeTypes.West | RoundFractal.EdgeTypes.North | RoundFractal.EdgeTypes.South;
            init(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, iNoisePercent, iRange, iBoundaryValue, box_edge_types);
        }
        public void initContained(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent, int iRange)
        {
            initContained(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, iNoisePercent, iRange, 0);
        }
        public void initContained(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent)
        {
            initContained(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, iNoisePercent, 100, 0);
        }
        public void initContained(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample)
        {
            initContained(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, 0, 100, 0);
        }

        public void initWithoutEdges(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent, int iRange)
        {
            EdgeTypes unbound_edge_types = RoundFractal.EdgeTypes.None;
            init(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, iNoisePercent, iRange, 0, unbound_edge_types);
        }
        public void initWithoutEdges(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample, int iNoisePercent)
        {
            initWithoutEdges(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, iNoisePercent, 100);
        }
        public void initWithoutEdges(ulong ulSeed, int iExponentX, int iExponentY, int iWidth, int iHeight, int iSample)
        {
            initWithoutEdges(ulSeed, iExponentX, iExponentY, iWidth, iHeight, iSample, 0, 100);
        }

        private void calculatePercentValues()
        {
            int[] aiValueCount = new int[miRange];
            int iNumTiles = miHeight * miWidth;
            for (int iI = 0; iI < iNumTiles; iI++)
            {
                aiValueCount[getValue(iI)]++;
            }

            int iCumulativeCount = 0;

            for (int iI = 0; iI < miRange; iI++)
            {
                iCumulativeCount += aiValueCount[iI];

                setValuePercent(iI, ((iCumulativeCount * 100) / iNumTiles));
            }
        }

        private void doSquare(int iX, int iY, int iStep, int iValue)
        {
            int iHalfStep = iStep / 2;

            // a     b 
            //
            //    x
            //
            // c     d

            //MohawkAssert.Assert(iX - iHalfStep >= 0);
            //MohawkAssert.Assert(iX + iHalfStep < getWidth());
            //MohawkAssert.Assert(iY - iHalfStep >= 0);
            //MohawkAssert.Assert(iY + iHalfStep < getHeight());

            int iA = getValue(iX - iHalfStep, iY - iHalfStep);
            int iB = getValue(iX + iHalfStep, iY - iHalfStep);
            int iC = getValue(iX - iHalfStep, iY + iHalfStep);
            int iD = getValue(iX + iHalfStep, iY + iHalfStep);

            setValue(iX, iY, ((iA + iB + iC + iD) / 4) + iValue);
        }

        private void doDiamond(int iX, int iY, int iStep, int iValue)
        {
            int iHalfStep = iStep / 2;

            //   c
            //
            //a  x  b
            //
            //   d

            int iNumPoints = 0;
            int iTotal = 0;
            if (iX - iHalfStep >= 0)
            {
                iTotal += getValue(iX - iHalfStep, iY);
                ++iNumPoints;
            }
            if (iX + iHalfStep < getWidth())
            {
                iTotal += getValue(iX + iHalfStep, iY);
                ++iNumPoints;
            }
            if (iY - iHalfStep >= 0)
            {
                iTotal += getValue(iX, iY - iHalfStep);
                ++iNumPoints;
            }
            if (iY + iHalfStep < getHeight())
            {
                iTotal += getValue(iX, iY + iHalfStep);
                ++iNumPoints;
            }

            setValue(iX, iY, iTotal / iNumPoints + iValue);
        }

        private void diamondSquare(int iStep)
        {
            int iHalfStep = iStep / 2;

            for (int x = iHalfStep; x < getWidth(); x += iStep)
            {
                for (int y = iHalfStep; y < getHeight(); y += iStep)
                {
                    doSquare(x, y, iStep, randomNext(getNoisePercent() + 1) - (getNoisePercent() / 2));
                }
            }

            for (int x = 0; x < getWidth(); x += iStep)
            {
                for (int y = 0; y < getHeight(); y += iStep)
                {
                    if (x + iHalfStep < miWidth)
                    {
                        doDiamond(x + iHalfStep, y, iStep, randomNext(getNoisePercent() + 1) - (getNoisePercent() / 2));
                    }
                    if (y + iHalfStep < miHeight)
                    {
                        doDiamond(x, y + iHalfStep, iStep, randomNext(getNoisePercent() + 1) - (getNoisePercent() / 2));
                    }
                }
            }
        }
    }
}