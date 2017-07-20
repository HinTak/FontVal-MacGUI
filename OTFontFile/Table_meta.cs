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
namespace OTFontFile
{
    public class Table_meta : OTTable
    {
        public Table_meta(OTTag tag, MBOBuffer buf) : base(tag, buf)
        {
        }

        public enum FieldOffsets
        {
            version      = 0, // uint32_t
            flags        = 4, // uint32_t
            dataOffset   = 8, // uint32_t
            numDataMaps  = 12 // uint32_t
        }

        public class DataMap
        {
            public DataMap(uint offset, MBOBuffer bufTable)
            {
                m_offsetIndex = offset;
                m_bufTable = bufTable;
            }

            public enum FieldOffsets
            {
                tag        = 0, // FourCharCode
                dataOffset = 4, // uint32_t
                dataLength = 8  // uint32_t
            }

            // accessors
            public string tag
            {
                get { return (string) m_bufTable.GetTag(m_offsetIndex + (uint)FieldOffsets.tag); }
            }

            public uint dataOffset
            {
                get { return m_bufTable.GetUint(m_offsetIndex + (uint)FieldOffsets.dataOffset); }
            }

            public uint dataLength
            {
                get { return m_bufTable.GetUint(m_offsetIndex + (uint)FieldOffsets.dataLength); }
            }

            uint m_offsetIndex;
            MBOBuffer m_bufTable;
        }

        public DataMap GetDataMap(uint i)
        {
            DataMap entry = null;

            if ( i < numDataMaps )
            {
                uint offset = 16 /*sizeof(header) */ + 12 /*sizeof(DataMap) */ * i;
                entry = new DataMap( offset, m_bufTable );
            }
            return entry;
        }

        public byte[] GetData(uint i)
        {
            DataMap entry = this.GetDataMap(i);
            uint length = entry.dataLength;
            byte [] buf = new byte[length];
            /* Apple TrueType spec is wrong - does not add this.dataOffset */
            uint offset = entry.dataOffset;
            System.Buffer.BlockCopy(m_bufTable.GetBuffer(), (int)offset, buf, 0, (int)length);

            return buf;
        }

        public string GetStringData(uint i)
        {
            return System.Text.UTF8Encoding.UTF8.GetString(GetData(i));
        }

        // accessors
        public uint version
        {
            get { return m_bufTable.GetUint((uint)FieldOffsets.version); }
        }

        public uint flags
        {
            get { return m_bufTable.GetUint((uint)FieldOffsets.flags); }
        }

        public uint dataOffset
        {
            get { return m_bufTable.GetUint((uint)FieldOffsets.dataOffset); }
        }

        public uint numDataMaps
        {
            get { return m_bufTable.GetUint((uint)FieldOffsets.numDataMaps); }
        }
    }
}
