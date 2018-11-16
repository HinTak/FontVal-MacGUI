// Copyright (c) Hin-Tak Leung

// All rights reserved.

// MIT License

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the ""Software""), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using SharpFont;

namespace OTFontFile.Rasterizer
{
    public class RasterInterf
    {
        private static RasterInterf _Rasterizer;
        private static Library _lib;
        private static Face _face;
        private static IntPtr _face_handle;
        private DevMetricsData m_DevMetricsData;
        private bool m_UserCancelledTest = false;
        private int m_RastErrorCount;

        public delegate void RastTestErrorDelegate (string sStringName, string sDetails);

        public delegate void UpdateProgressDelegate (string s);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int diagnostics_Function(IntPtr face_handle, int messcode, string message, string opcode,
                                                 int range_base, int is_composite,
                                                 int IP, int callTop, int opc, int start);

        [DllImport("freetype6", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TT_Diagnostics_Set(IntPtr face_handle, [MarshalAs(UnmanagedType.FunctionPtr)] diagnostics_Function diagnostics);

        [DllImport("freetype6", CallingConvention = CallingConvention.Cdecl)]
        public static extern void TT_Diagnostics_Unset(IntPtr face_handle);

        private RasterInterf ()
        {
            PlatformID pid = Environment.OSVersion.Platform;
#if __MonoCS__
            if ( pid != PlatformID.Unix && pid != PlatformID.MacOSX )
#else
            if ( pid != PlatformID.Unix )
#endif
            {
                string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                path = Path.Combine(path, IntPtr.Size == 8 ? "Win64" : "Win32");
                if (!SetDllDirectory(path))
                    throw new System.ComponentModel.Win32Exception();
            }
            _lib = new Library();
            //Console.WriteLine("FreeType version: " + _lib.Version);
        }

        public Version FTVersion
        {
            get {
                return _lib.Version;
            }
        }

        static public RasterInterf getInstance()
        {
            if (_Rasterizer == null)
            {
                _Rasterizer = new RasterInterf ();
            }
            if ( _face != null )
            {
                _face.Dispose();
                _face = null;
            }

            return _Rasterizer;
        }

        public bool RastTest (int resX, int resY, int[] arrPointSizes,
                             float stretchX, float stretchY,
                             float rotation, float skew,
                             float[,] matrix,
                             bool setBW, bool setGrayscale, bool setCleartype, uint CTFlags,
                             RastTestErrorDelegate pRastTestErrorDelegate,
                             UpdateProgressDelegate pUpdateProgressDelegate,
                             int numGlyphs)
        {
            int count_sets = 0;
            LoadFlags lf = LoadFlags.Default;
            LoadTarget lt = LoadTarget.Normal;
            if ( setBW )
            {
                lf = LoadFlags.Default|LoadFlags.NoAutohint|LoadFlags.Monochrome|LoadFlags.ComputeMetrics;
                lt = LoadTarget.Mono;
                _lib.PropertySet("truetype", "interpreter-version", 35);

                count_sets++;
            }
            if ( setGrayscale )
            {
                lf = LoadFlags.Default|LoadFlags.NoAutohint|LoadFlags.ComputeMetrics;
                lt = LoadTarget.Normal;
                _lib.PropertySet("truetype", "interpreter-version", 35);

                count_sets++;
            }
            if ( setCleartype )
            {
                lf = LoadFlags.Default|LoadFlags.NoAutohint|LoadFlags.ComputeMetrics;
                lt = LoadTarget.Lcd;
                _lib.PropertySet("truetype", "interpreter-version", 40);

                count_sets++;
            }
            if ( count_sets != 1 )
                throw new ArgumentOutOfRangeException("Only one of BW/Grayscale/Cleartype should be set");

            try
            {
                TT_Diagnostics_Unset(_face_handle);
            }
            catch (Exception)
            {
                throw new NotImplementedException("UnImplemented in this version of Freetype: " + FTVersion);
            };

            FTMatrix fmatrix = new FTMatrix(new Fixed16Dot16( matrix[0,0] * stretchX ), new Fixed16Dot16( matrix[0,1] * stretchX ),
                                            new Fixed16Dot16( matrix[1,0] * stretchY ), new Fixed16Dot16( matrix[1,1] * stretchY ));
            FTVector fdelta = new FTVector(new Fixed16Dot16( matrix[0,2] * stretchX ), new Fixed16Dot16( matrix[1,2] * stretchY ));
            /* matrix[2,0] = matrix[2,1] = 0, matrix[2,2] =1, not used */

            FTMatrix mskew = new FTMatrix(new Fixed16Dot16( 1 ), new Fixed16Dot16( 0 ),
                                          (new Fixed16Dot16(skew)).Tan(), new Fixed16Dot16( 1 ));
            FTMatrix.Multiply(ref mskew, ref fmatrix);
            fdelta.Transform(mskew);

            FTVector rot_row1 = new FTVector(new Fixed16Dot16( 1 ), new Fixed16Dot16( 0 ));
            FTVector rot_row2 = new FTVector(new Fixed16Dot16( 1 ), new Fixed16Dot16( 0 ));
            rot_row1.Rotate(new Fixed16Dot16(rotation));
            rot_row2.Rotate(new Fixed16Dot16(rotation + 90));
            FTMatrix mrot = new FTMatrix(rot_row1, rot_row2);
            FTMatrix.Multiply(ref mrot, ref fmatrix);
            fdelta.Rotate(new Fixed16Dot16(-rotation));

            for (int i = 0; i < arrPointSizes.Length ; i++)
            {
                if ( m_UserCancelledTest ) return true;
                pUpdateProgressDelegate("Processing Size " + arrPointSizes[i]);
                try{
                    _face.SetCharSize(new Fixed26Dot6(arrPointSizes[i]),
                                      new Fixed26Dot6(arrPointSizes[i]),
                                      (uint) resX, (uint) resY);
                } catch (FreeTypeException e) {
                    if (e.Error == Error.InvalidPixelSize)
                    {
                        pRastTestErrorDelegate("_rast_W_FT_InvalidPixelSize", "Setting unsupported size "
                                               + arrPointSizes[i] + " for fixed-size font.");
                        m_RastErrorCount += 1;
                        continue;
                    }
                    else
                        throw;
                }
                _face.SetTransform(fmatrix, fdelta);
                for (uint ig = 0; ig < numGlyphs; ig++) {
                    diagnostics_Function diagnostics =
                        (face_handle, messcode, message, opcode, range_base, is_composite, IP, callTop, opc, start) =>
                        {
                            string sDetails = "Size " + arrPointSizes[i] + ", " + opcode;
                            switch ( range_base )
                            {
                                case 3:
                                    if (is_composite != 0)
                                        sDetails += ", Composite Glyph ID " + ig;
                                    else
                                        sDetails += ", Glyph ID " + ig;
                                    break;
                                case 1: /* font */
                                case 2: /* cvt */ // ?
                                    sDetails += ", Pre-Program";
                                    break;
                                default: /* none */
                                    sDetails += ", Unknown?"; // ?
                                    break;
                            }

                            sDetails += ", At ByteOffset " + IP;

                            if (callTop > 0)
                                sDetails += ", In function " + opc + " offsetted by " + (IP - start);

                            pRastTestErrorDelegate(message, sDetails);
                            m_RastErrorCount += 1;
                            return 0; // Not used currently.
                        };
                    TT_Diagnostics_Set(_face_handle, diagnostics);
                    try{
                        _face.LoadGlyph(ig, lf, lt);
                    } catch (Exception ee) {
                        if (ee is FreeTypeException)
                        {
                            FreeTypeException e = (FreeTypeException) ee;
                            if ( e.Error == Error.InvalidOutline )
                            {
                                pRastTestErrorDelegate("_rast_W_FT_InvalidOutline", "Invalid Outline in Glyph " + ig);
                                m_RastErrorCount += 1;
                                continue;
                            }
                            if ( e.Error == Error.InvalidArgument )
                            {
                                pRastTestErrorDelegate("_rast_W_FT_InvalidArgument", "Invalid Argument in Glyph " + ig);
                                m_RastErrorCount += 1;
                                continue;
                            }
                            if ( e.Error == Error.InvalidSizeHandle )
                            {
                                pRastTestErrorDelegate("_rast_W_FT_InvalidSizeHandle", "Invalid Metrics for Glyph " + ig + " at size "
                                                       + arrPointSizes[i]);
                                m_RastErrorCount += 1;
                                continue;
                            }
                        }

                        pRastTestErrorDelegate("_rast_I_FT_Error_Supplymentary_Info", "Glyph " + ig +
                                               " at size " + arrPointSizes[i]);
                        throw;
                    }
                    TT_Diagnostics_Unset(_face_handle);
                }
            }
            return true;
        }

        public DevMetricsData CalcDevMetrics (int Huge_calcHDMX, int Huge_calcLTSH, int Huge_calcVDMX,
                                              ushort numGlyphs,
                                              byte[] phdmxPointSizes, ushort maxHdmxPointSize,
                                              byte uchPixelHeightRangeStart, byte uchPixelHeightRangeEnd,
                                              ushort[] pVDMXxResolution, ushort[] pVDMXyResolution,
                                              ushort cVDMXResolutions, UpdateProgressDelegate pUpdateProgressDelegate)
        {
            _lib.PropertySet("truetype", "interpreter-version", 35);
            if ( Huge_calcHDMX == 0 && Huge_calcLTSH == 0 && Huge_calcVDMX == 0 )
                return null;

            this.m_DevMetricsData = new DevMetricsData();

            if ( Huge_calcHDMX != 0 )
            {
                List<uint> requestedPixelSize = new List<uint>();
                for( ushort i = 0; i <= maxHdmxPointSize ; i++ ) {
                    if ( phdmxPointSizes[i] == 1 ) {
                        requestedPixelSize.Add((uint)i);
                    }
                }

                this.m_DevMetricsData.hdmxData = new HDMX();
                this.m_DevMetricsData.hdmxData.Records = new HDMX_DeviceRecord[requestedPixelSize.Count];

                for (int i = 0; i < requestedPixelSize.Count; i++) {
                    if ( m_UserCancelledTest ) return null;
                    trySetPixelSizes(0, requestedPixelSize[i]);
                    this.m_DevMetricsData.hdmxData.Records[i] = new HDMX_DeviceRecord();
                    this.m_DevMetricsData.hdmxData.Records[i].Widths = new byte[_face.GlyphCount];
                    for (uint glyphIndex = 0; glyphIndex < _face.GlyphCount; glyphIndex++)
                    {
                        _face.LoadGlyph(glyphIndex, LoadFlags.Default|LoadFlags.ComputeMetrics, LoadTarget.Normal);
                        this.m_DevMetricsData.hdmxData.Records[i].Widths[glyphIndex] =  (byte) _face.Glyph.Advance.X.Round();
                    }
                }
            }

            if ( Huge_calcLTSH != 0 )
            {
                this.m_DevMetricsData.ltshData = new LTSH();
                this.m_DevMetricsData.ltshData.yPels = new byte[numGlyphs];

                for (uint i = 0; i < this.m_DevMetricsData.ltshData.yPels.Length; i++) {
                    this.m_DevMetricsData.ltshData.yPels[i] = 1;
                }
                int remaining = numGlyphs;
                for (uint j = 254; j > 0; j--) {
                    if ( remaining == 0 )
                        break;
                    if ( m_UserCancelledTest ) return null;
                    trySetPixelSizes(0, j);
                    for (uint i = 0; i < this.m_DevMetricsData.ltshData.yPels.Length; i++) {
                        if ( this.m_DevMetricsData.ltshData.yPels[i] > 1 )
                            continue;
                        _face.LoadGlyph(i, LoadFlags.Default|LoadFlags.ComputeMetrics, LoadTarget.Normal);
                        int Advance_X = _face.Glyph.Advance.X.Round() ;
                        int LinearHorizontalAdvance = _face.Glyph.LinearHorizontalAdvance.Round() ;
                        if ( Advance_X == LinearHorizontalAdvance )
                            continue;
                        int difference = Advance_X - LinearHorizontalAdvance ;
                        if ( difference < 0 )
                            difference = - difference;
                        if ( ( j >= 50 ) && (difference * 50 <= LinearHorizontalAdvance) ) // compat "<="
                            continue;
                        // this is off-spec but happens to agree better...
                        difference = (_face.Glyph.Advance.X.Value << 10) - _face.Glyph.LinearHorizontalAdvance.Value;
                        if ( difference < 0 )
                            difference = - difference;
                        if ( ( j >= 50 ) && (difference * 50 <= _face.Glyph.LinearHorizontalAdvance.Value) ) // compat "<=="
                            continue;
                        // off-spec-ness ends.
                        this.m_DevMetricsData.ltshData.yPels[i] = (byte) ( j + 1 );
                        remaining--;
                    }
                }
            }

            if ( Huge_calcVDMX != 0 )
            {
                this.m_DevMetricsData.vdmxData = new VDMX();
                this.m_DevMetricsData.vdmxData.groups = new VDMX_Group[cVDMXResolutions];
                for ( int i = 0 ; i < cVDMXResolutions ; i++ )
                {
                    this.m_DevMetricsData.vdmxData.groups[i] = new VDMX_Group();
                    this.m_DevMetricsData.vdmxData.groups[i].entry = new VDMX_Group_vTable[uchPixelHeightRangeEnd
                                                                                           - uchPixelHeightRangeStart + 1];
                    for ( ushort j = uchPixelHeightRangeStart ; j <= uchPixelHeightRangeEnd ; j++ )
                    {
                        int k = j - uchPixelHeightRangeStart;
                        this.m_DevMetricsData.vdmxData.groups[i].entry[k] = new VDMX_Group_vTable() ;
                        this.m_DevMetricsData.vdmxData.groups[i].entry[k].yPelHeight = j ;

                        uint x_pixelSize = (uint) ( (pVDMXyResolution[i] == 0) ?
                                                    0 : (pVDMXxResolution[i] * j + pVDMXyResolution[i]/2 ) / pVDMXyResolution[i] );
                        if ( m_UserCancelledTest ) return null;
                        trySetPixelSizes(x_pixelSize, j);
                        short yMax = 0;
                        short yMin = 0;
                        BBox box;
                        Glyph glyph;
                        for (uint ig = 0; ig < numGlyphs; ig++) {
                            _face.LoadGlyph(ig, LoadFlags.Default|LoadFlags.ComputeMetrics, LoadTarget.Normal);
                            glyph = _face.Glyph.GetGlyph();
                            box = glyph.GetCBox(GlyphBBoxMode.Truncate);
                            if (box.Top > yMax) yMax = (short) box.Top;
                            if (box.Bottom < yMin) yMin = (short) box.Bottom;
                            glyph.Dispose();
                        }
                        this.m_DevMetricsData.vdmxData.groups[i].entry[k].yMax = yMax ;
                        this.m_DevMetricsData.vdmxData.groups[i].entry[k].yMin = yMin ;
                    }
                }
            }

            return m_DevMetricsData;
        }

        public ushort RasterNewSfnt (FileStream fontFileStream, uint faceIndex)
        {
            _face = _lib.NewFace(fontFileStream.Name, (int)faceIndex);
            _face_handle = (IntPtr) _face.GetType().GetProperty("pReference", BindingFlags.NonPublic |BindingFlags.Instance).GetValue(_face, null);
            m_UserCancelledTest = false;
            m_RastErrorCount = 0;

            return 1; //Not used by caller
        }

        public void CancelRastTest ()
        {
            m_UserCancelledTest = true;
        }

        public void CancelCalcDevMetrics ()
        {
            m_UserCancelledTest = true;
        }

        public int GetRastErrorCount ()
        {
            return m_RastErrorCount;
        }

        public class DevMetricsData
        {
            public HDMX hdmxData;
            public LTSH ltshData;
            public VDMX vdmxData;
        }

        // These structures largely have their OTSPEC meanings,
        // except there is no need to store array lengths separately
        // as .NET arrays know their own lengths.

        public class HDMX
        {
            public HDMX_DeviceRecord[] Records;
        }

        public class HDMX_DeviceRecord
        {
            public byte[] Widths;
        }

        public class LTSH
        {
            public byte[] yPels; // byte[numGlyphs] 1 default
        }

        public class VDMX
        {
            public VDMX_Group[] groups;
        }

        public class VDMX_Group
        {
            public VDMX_Group_vTable[] entry;
        }

        public class VDMX_Group_vTable
        {
            public ushort yPelHeight;
            public short yMax;
            public short yMin;
        }

        private void trySetPixelSizes(uint x_requestedPixelSize, uint y_requestPixelSize)
        {
            try{
                _face.SetPixelSizes(x_requestedPixelSize, y_requestPixelSize);
            } catch (FreeTypeException e) {
                //Console.WriteLine("SetPixelSizes caught " + e + " at size " + x_requestedPixelSize + ", " + y_requestPixelSize);
                if (e.Error == Error.InvalidPixelSize)
                    throw new ArgumentException("FreeType invalid pixel size error: Likely setting unsupported size "
                                                + x_requestedPixelSize + ", " + y_requestPixelSize + " for fixed-size font.");
                else
                    throw;
            }
        }
    }
}
