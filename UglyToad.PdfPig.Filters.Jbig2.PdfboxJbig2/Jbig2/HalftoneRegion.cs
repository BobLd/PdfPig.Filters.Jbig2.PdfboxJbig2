#nullable disable

using UglyToad;

namespace UglyToad.PdfPig.Filters.Jbig2.PdfboxJbig2.Jbig2
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class represents the data of segment type "Halftone region". Parsing is described in 7.4.5, page 67. Decoding
    /// procedure in 6.6.5 and 7.4.5.2.
    /// </summary>
    internal sealed class HalftoneRegion : IRegion
    {
        private static readonly double log2 = Math.Log(2);

        private SubInputStream subInputStream;
        private SegmentHeader segmentHeader;
        private long dataHeaderOffset = 0;
        private long dataHeaderLength;
        private long dataOffset;
        private long dataLength;

        // Decoded data
        private Jbig2Bitmap halftoneRegionBitmap;

        // Previously decoded data from other regions or dictionaries, stored to use as patterns in this region.
        private List<Jbig2Bitmap> patterns;

        // Region segment information field, 7.4.1
        public RegionSegmentInformation RegionInfo { get; private set; }

        // Halftone segment information field, 7.4.5.1.1
        public byte HDefaultPixel { get; private set; }
        public CombinationOperator HCombinationOperator { get; private set; }
        public bool HSkipEnabled { get; private set; }
        public byte HTemplate { get; private set; }
        public bool IsMMREncoded { get; private set; }

        // Halftone grid position and size, 7.4.5.1.2
        // Width of the gray-scale image, 7.4.5.1.2.1
        public int HGridWidth { get; private set; }
        // Height of the gray-scale image, 7.4.5.1.2.2
        public int HGridHeight { get; private set; }
        // Horizontal offset of the grid, 7.4.5.1.2.3
        public int HGridX { get; private set; }
        // Vertical offset of the grid, 7.4.5.1.2.4
        public int HGridY { get; private set; }

        // Halftone grid vector, 7.4.5.1.3
        // Horizontal coordinate of the halftone grid vector, 7.4.5.1.3.1
        public int HRegionX { get; private set; }
        // Vertical coordinate of the halftone grod vector, 7.4.5.1.3.2
        public int HRegionY { get; private set; }

        public HalftoneRegion()
        {
        }

        public HalftoneRegion(SubInputStream subInputStream)
        {
            this.subInputStream = subInputStream;
            RegionInfo = new RegionSegmentInformation(subInputStream);
        }

        public HalftoneRegion(SubInputStream subInputStream, SegmentHeader segmentHeader)
        {
            this.subInputStream = subInputStream;
            this.segmentHeader = segmentHeader;
            RegionInfo = new RegionSegmentInformation(subInputStream);
        }

        private void ParseHeader()
        {
            RegionInfo.ParseHeader();

            // Bit 7
            HDefaultPixel = (byte)subInputStream.ReadBit();

            // Bit 4-6
            HCombinationOperator = CombinationOperators
                    .TranslateOperatorCodeToEnum((short)(subInputStream.ReadBits(3) & 0xf));

            // Bit 3
            if (subInputStream.ReadBit() == 1)
            {
                HSkipEnabled = true;
            }

            // Bit 1-2
            HTemplate = (byte)(subInputStream.ReadBits(2) & 0xf);

            // Bit 0
            if (subInputStream.ReadBit() == 1)
            {
                IsMMREncoded = true;
            }

            HGridWidth = (int)(subInputStream.ReadBits(32) & 0xffffffff);
            HGridHeight = (int)(subInputStream.ReadBits(32) & 0xffffffff);

            HGridX = (int)subInputStream.ReadBits(32);
            HGridY = (int)subInputStream.ReadBits(32);

            HRegionX = (int)subInputStream.ReadBits(16) & 0xffff;
            HRegionY = (int)subInputStream.ReadBits(16) & 0xffff;

            ComputeSegmentDataStructure();
        }

        private void ComputeSegmentDataStructure()
        {
            dataOffset = subInputStream.Position;
            dataHeaderLength = dataOffset - dataHeaderOffset;
            dataLength = subInputStream.Length - dataHeaderLength;
        }

        /// <summary>
        /// The procedure is described in JBIG2 ISO standard, 6.6.5.
        /// </summary>
        /// <returns>The decoded <see cref="Jbig2Bitmap"/></returns>
        public Jbig2Bitmap GetRegionBitmap()
        {
            if (halftoneRegionBitmap is null)
            {
                // 6.6.5, page 40
                // 1)
                halftoneRegionBitmap = new Jbig2Bitmap(RegionInfo.BitmapWidth,
                        RegionInfo.BitmapHeight);

                if (patterns is null)
                {
                    patterns = GetPatterns();
                }

                if (HDefaultPixel == 1)
                {
                    halftoneRegionBitmap.ByteArray.AsSpan().Fill(0xff);
                }

                // 2) 6.6.5.1 Computing HSKIP
                Jbig2Bitmap hSkip = null;
                if (HSkipEnabled)
                {
                    int hPatternHeight = patterns[0].Height;
                    int hPatternWidth = patterns[0].Width;
                    hSkip = ComputeHSkip(hPatternWidth, hPatternHeight);
                }

                // 3)
                int bitsPerValue = (int)Math.Ceiling(Math.Log(patterns.Count) / log2);

                // 4)
                int[][] grayScaleValues = GrayScaleDecoding(bitsPerValue, hSkip);

                // 5), rendering the pattern, described in 6.6.5.2
                RenderPattern(grayScaleValues);
            }
            // 6)
            return halftoneRegionBitmap;
        }

        /// <summary>
        /// This method draws the pattern into the region bitmap({ @code htReg}), as described in 6.6.5.2, page 42
        /// </summary>
        private void RenderPattern(int[][] grayScaleValues)
        {
            // 1)
            for (int m = 0; m < HGridHeight; m++)
            {
                // a)
                for (int n = 0; n < HGridWidth; n++)
                {
                    // i)
                    var x = ComputeX(m, n);
                    var y = ComputeY(m, n);

                    // ii)
                    Jbig2Bitmap patternJbig2Bitmap = patterns[grayScaleValues[m][n]];
                    Jbig2Bitmaps.Blit(patternJbig2Bitmap, halftoneRegionBitmap, x, y,
                            HCombinationOperator);
                }
            }
        }

        private List<Jbig2Bitmap> GetPatterns()
        {
            var patterns = new List<Jbig2Bitmap>();

            foreach (SegmentHeader s in segmentHeader.RtSegments)
            {
                PatternDictionary patternDictionary = (PatternDictionary)s.GetSegmentData();
                patterns.AddRange(patternDictionary.GetDictionary());
            }

            return patterns;
        }


        /// <summary>
        /// Gray-scale image decoding procedure is special for halftone region decoding
        /// and is described in Annex C.5 on page 98.
        /// </summary>
        private int[][] GrayScaleDecoding(int bitsPerValue, Jbig2Bitmap hSkip)
        {
            short[] gbAtX = null;
            short[] gbAtY = null;

            if (!IsMMREncoded)
            {
                gbAtX = new short[4];
                gbAtY = new short[4];
                // Set AT pixel values
                if (HTemplate <= 1)
                {
                    gbAtX[0] = 3;
                }
                else if (HTemplate >= 2)
                {
                    gbAtX[0] = 2;
                }

                gbAtY[0] = -1;
                gbAtX[1] = -3;
                gbAtY[1] = -1;
                gbAtX[2] = 2;
                gbAtY[2] = -2;
                gbAtX[3] = -2;
                gbAtY[3] = -2;
            }

            var grayScalePlanes = new Jbig2Bitmap[bitsPerValue];

            // 1)
            var genericRegion = new GenericRegion(subInputStream);
            genericRegion.SetParameters(IsMMREncoded, dataOffset, dataLength, HGridHeight, HGridWidth,
                    HTemplate, false, HSkipEnabled, hSkip, gbAtX, gbAtY);

            // 2)
            int j = bitsPerValue - 1;

            grayScalePlanes[j] = genericRegion.GetRegionBitmap();

            while (j > 0)
            {
                j--;
                genericRegion.ResetBitmap();
                // 3) a)
                grayScalePlanes[j] = genericRegion.GetRegionBitmap();
                // 3) b)
                grayScalePlanes = CombineGrayScalePlanes(grayScalePlanes, j);
            }

            // 4)
            return ComputeGrayScaleValues(grayScalePlanes, bitsPerValue);
        }

        private static Jbig2Bitmap[] CombineGrayScalePlanes(Jbig2Bitmap[] grayScalePlanes, int j)
        {
            int byteIndex = 0;
            for (int y = 0; y < grayScalePlanes[j].Height; y++)
            {
                for (int x = 0; x < grayScalePlanes[j].Width; x += 8)
                {
                    byte newValue = grayScalePlanes[j + 1].GetByte(byteIndex);
                    byte oldValue = grayScalePlanes[j].GetByte(byteIndex);

                    grayScalePlanes[j].SetByte(byteIndex++,
                        Jbig2Bitmaps.CombineBytes(oldValue, newValue, CombinationOperator.XOR));
                }
            }
            return grayScalePlanes;
        }

        private int[][] ComputeGrayScaleValues(Jbig2Bitmap[] grayScalePlanes, int bitsPerValue)
        {
            // Gray-scale decoding procedure, page 98
            int[][] grayScaleValues = new int[HGridHeight][];
            for (int i = 0; i < grayScaleValues.Length; i++)
            {
                grayScaleValues[i] = new int[HGridWidth];
            }

            // 4)
            for (int y = 0; y < HGridHeight; y++)
            {
                for (int x = 0; x < HGridWidth; x += 8)
                {
                    int minorWidth = HGridWidth - x > 8 ? 8 : HGridWidth - x;
                    int byteIndex = grayScalePlanes[0].GetByteIndex(x, y);

                    for (int minorX = 0; minorX < minorWidth; minorX++)
                    {
                        int i = minorX + x;
                        grayScaleValues[y][i] = 0;

                        for (int j = 0; j < bitsPerValue; j++)
                        {
                            grayScaleValues[y][i] += (grayScalePlanes[j]
                                    .GetByte(byteIndex) >> (7 - i & 7) & 1) * (1 << j);
                        }
                    }
                }
            }

            return grayScaleValues;
        }

        private int ComputeX(int m, int n)
        {
            return (HGridX + m * HRegionY + n * HRegionX) >> 8;
        }

        private int ComputeY(int m, int n)
        {
            return (HGridY + m * HRegionX - n * HRegionY) >> 8;
        }

        // 6.6.5.1 Computing HSKIP
        private Jbig2Bitmap ComputeHSkip(int hPatternWidth, int hPatternHeight)
        {
            // HSKIP is HGW by HGH pixels
            var bitmap = new Jbig2Bitmap(HGridWidth, HGridHeight);
            for (int m = 0; m < HGridHeight; m++)
            {
                for (int n = 0; n < HGridWidth; n++)
                {
                    int x = ComputeX(m, n);
                    int y = ComputeY(m, n);

                    if (x + hPatternWidth <= 0 || x >= halftoneRegionBitmap.Width
                        || y + hPatternHeight <= 0 || y >= halftoneRegionBitmap.Height)
                    {
                        bitmap.SetPixel(n, m, 1);
                    }
                    // else no need to set 0 pixels
                }
            }
            return bitmap;
        }

        public void Init(SegmentHeader header, SubInputStream sis)
        {
            segmentHeader = header;
            subInputStream = sis;
            RegionInfo = new RegionSegmentInformation(subInputStream);
            ParseHeader();
        }
    }
}
