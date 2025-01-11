using Snowberry.IO;
using BFFNTtoFNT.Classes; 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snowberry.IO.Common;
using Snowberry.IO.Reader;
using System.Reflection.Metadata.Ecma335;

namespace BFFNTtoFNT
{
    internal class ReadBFFNT
    {
        string filedirectory;
        BFFNT bffnt;
        public ReadBFFNT(BFFNT inBffnt)
        {
            filedirectory = inBffnt.fileDirectory;
            bffnt = inBffnt;
        }
        /// <summary>
        /// Parses the BFFNT File
        /// </summary>
        public void ParseBFFNT()
        {
            EndianStreamReader file = new EndianStreamReader(File.OpenRead(filedirectory));
            while (file.Position < file.Length)
            {
                string fileMagic = file.ReadSizedCString(4)??"";
                switch (fileMagic)
                {
                    case "FFNT":
                        ParseHeader(bffnt, file);
                        break;
                    case "FINF":
                        ParseFontInfo(bffnt, file);
                        break;
                    case "TGLP":
                        ParseTextureInfo(bffnt, file);
                        break;
                    case "CWDH":
                        ParseCharWidhtInfo(bffnt, file);
                        break;
                    case "CMAP":
                        ParseCodeMap(bffnt, file);
                        break;
                    case "KRNG":
                        ParseKerning(bffnt, file);
                        break;
                    default:
                        ThrowException(file,"Unkonwn Section Magic: " + fileMagic);
                        break;
                }
                RoundCursor(file);
            }
            //Closes the BFFNT.
            file.Stream.Close();
        }

        /// <summary>
        /// Parses the FFNT Header
        /// </summary>
        private void ParseHeader(BFFNT bffnt, EndianStreamReader file)
        {
            EndianType bom = EndianType.LITTLE;
            //Interprets the Byte Order of the file (Might allow for Switch Versions of file)
            EndianType getByteOrder()
            {
                int[] byteOrder = [file.ReadByte(), file.ReadByte()];
                if (byteOrder.SequenceEqual([0xFE, 0xFF]))
                {
                    return bom = EndianType.BIG;
                }
                else if (byteOrder.SequenceEqual([0xFF, 0xFE]))
                {
                    return bom = EndianType.LITTLE;
                }
                else
                {
                    ThrowException(file, "Unknown Endian Type");
                    return bom = EndianType.BIG;
                }
            }
            //Check if the internal file size meets actual file size.
            int checkFileSize()
            {
                int size = file.ReadInt32(bom);
                if(size != (file.Stream.Length))
                {
                    throw new Exception("Internal FileSize does not match Actual FileSize");
                }
                return size;
            }

            long initPos = file.Position-4;
            bffnt.header = new FFNT
            {
                magic = "FFNT",
                endian = getByteOrder(),
                secSize = file.ReadInt16(bom),
                version = [file.ReadByte(), file.ReadByte(), file.ReadByte(), file.ReadByte()],
                fileSize = checkFileSize(),
                secNum = file.ReadInt16(bom)
            };
            //Set position to end of section.
            file.Stream.Position = initPos + bffnt.header.secSize;
        }

        /// <summary>
        /// Parses the FINF Header
        /// </summary>
        private void ParseFontInfo(BFFNT bffnt, EndianStreamReader file)
        {
            long initPos = file.Stream.Position-4;
            FontType getFntType()
            {
                uint fnt = file.ReadByte();
                //Check for unknown font types. 
                //Dont know how this affects the process, so accept every form. 
                if(fnt == 0x00 || fnt > 0x03)
                {
                    throw new Exception("Unknown Font Type");
                }
                return (FontType) fnt;
            }
            GlyphCode getCharEnc()
            {
                uint enc = file.ReadByte();
                //Continue only if Unicode Encoding is used.
                if (enc > 0x03)
                {
                    throw new Exception("Unknown Encoding");
                }
                if (enc == 0)
                {
                    throw new Exception("UTF 8 Encoding not supported.");
                }
                if (enc == 2)
                {
                    throw new Exception("ShiftJIS Encoding not supported.");
                }
                if (enc == 3)
                {
                    throw new Exception("CP1252 Encoding not supported.");
                }
                return (GlyphCode) enc;
            }
            bffnt.fontInfo = new FINF
            {
                magic = "FINF",
                secSize = file.ReadInt32(bffnt.header.endian),
                fntType = getFntType(),
                width  = file.ReadByte(),
                height = file.ReadByte(),
                ascent = file.ReadByte(),
                lineFeed = file.ReadInt16(),
                invalidCharSym = file.ReadInt16(),
                defLeftDist = file.ReadByte(),
                defGlyphWidth = file.ReadByte(),
                defCharWidth = file.ReadByte(),
                charEncoding = getCharEnc(),
                tglpOffset = file.ReadInt32(),
                cwdhOffset = file.ReadInt32(),
                cmapOffset = file.ReadInt32(),
            };
            //Set position to end of section.
            file.Stream.Position = initPos + bffnt.fontInfo.secSize;
        }

        /// <summary>
        /// Parses the TGLP Header
        /// </summary>
        private void ParseTextureInfo(BFFNT bffnt, EndianStreamReader file)
        {
            long initPos = file.Stream.Position-4;
            bffnt.texInfo = new TGLP
            {
                magic = "TGLP",
                secSize = file.ReadInt32(bffnt.header.endian),
                cellWidth = file.ReadByte(),
                cellHeight = file.ReadByte(),
                sheetNum = file.ReadByte(),
                maxCharWidth = file.ReadByte(),
                sheetSize = file.ReadInt32(bffnt.header.endian),
                baseLinePos = file.ReadInt16(bffnt.header.endian),
                texFormat = file.ReadInt16(bffnt.header.endian),
                cellPerRow = file.ReadInt16(bffnt.header.endian),
                cellPerCol = file.ReadInt16(bffnt.header.endian),
                texWidth = file.ReadInt16(bffnt.header.endian),
                texHeight = file.ReadInt16(bffnt.header.endian),
                texDataOffset = file.ReadInt32(bffnt.header.endian)
            };
            //Only continue if ther is one texture sheet. 
            //TODO: Handle Multiple Sheets
            if(bffnt.texInfo.sheetNum != 1)
            {
                throw new Exception("Expected one Texture Sheet, got: " +  bffnt.texInfo.sheetNum);
            }
            //Set position to end of section.
            file.Stream.Position = initPos + bffnt.texInfo.secSize;
        }

        /// <summary>
        /// Parses the CWDH Header
        /// </summary>
        private void ParseCharWidhtInfo(BFFNT bffnt, EndianStreamReader file)
        {
            long initPos = file.Stream.Position-4;
            int firstGlyphIndex;
            int lastGlyphIndex;
            int dataSize;
            //Gets the Character width information.
            CharWidth[] GetCharWidths()
            {
                int entries = lastGlyphIndex - firstGlyphIndex + 1;
                CharWidth[] charWidths = new CharWidth[entries];
                for(int i = 0; i < entries; i++)
                {
                    charWidths[i] = new CharWidth
                    {
                        leftDistance = (sbyte)file.ReadByte(),
                        charWidth = file.ReadByte(),
                        glyphWidth = file.ReadByte()
                    };
                }
                return charWidths;
            }

            bffnt.charWidthInfo = new CWDH
            {
                magic = "CWDH",
                secSize = file.ReadInt32(bffnt.header.endian),
                firstCharIndex = firstGlyphIndex = file.ReadInt16(bffnt.header.endian),
                lastCharIndex = lastGlyphIndex = file.ReadInt16(bffnt.header.endian),
                widthDataSize = dataSize = file.ReadInt32(bffnt.header.endian),
                charWidths = GetCharWidths()
            };
            //Set position to end of section.
            file.Stream.Position = initPos + bffnt.charWidthInfo.secSize;
        }

        /// <summary>
        /// Parses the CMAP Header
        /// </summary>
        private void ParseCodeMap(BFFNT bffnt, EndianStreamReader file)
        {
            List<CMAP> returnCMAPS = new List<CMAP>();
            MapMethod mapMethod = MapMethod.Direct;
            int firstChar;
            int lastChar;
            int nextCMAPOffset;
            MapMethod getMapMethod()
            {
                mapMethod = (MapMethod)file.ReadInt16(bffnt.header.endian);
                file.Stream.Position += 2;
                return mapMethod;
            }

            CharMap[] getCharMaps()
            {
                CharMap[] outCharMap=new CharMap[1];
                switch (mapMethod)
                {
                    case MapMethod.Direct:
                        //throw new Exception("Direct Mapping Method not Implemented yet");
                        outCharMap = new CharMap[lastChar - firstChar];
                        int startIndex = file.ReadInt16(bffnt.header.endian);
                        file.Stream.Position += 2;
                        for (int i = 0; i < outCharMap.Length; i++)
                        {
                            CharMap charMap = new CharMap
                            {
                                charCode = firstChar + i,
                                index = startIndex + i,
                            };
                            outCharMap[i] = charMap;
                        }
                        break;
                    case MapMethod.Table:
                        throw new Exception("Table Mapping Method not Implemented yet");
                        //outCharMap = new CharMap[lastChar - firstChar];
                        ////int index = file.ReadInt16(bffnt.header.endian);
                        //file.Stream.Position += 2;
                        //for (int i = 0; i < outCharMap.Length; i++)
                        //{
                        //    CharMap charMap = new CharMap
                        //    {
                        //        charCode = index + i,
                        //        index = i,
                        //    };
                        //    outCharMap[i] = charMap;
                        //}
                        //break;
                    case MapMethod.Scan:
                        int entries = file.ReadInt16(bffnt.header.endian);
                        outCharMap = new CharMap[entries];
                        for (int i = 0; i < entries; i++)
                        {
                            CharMap charMap = new CharMap
                            {
                                charCode = file.ReadInt16(bffnt.header.endian),
                                index = file.ReadInt16(bffnt.header.endian)
                            };
                            outCharMap[i] = charMap;
                        }
                        break;
                    default:
                        break;

                }
                return outCharMap;
            }

            //Repeate while the next CMAP Offset is not 0. (0 means there are no more CMAP's)
            do
            {
                long initPos = file.Stream.Position;
                CMAP cMAP = new CMAP
                {
                    magic = "CMAP",
                    secSize = file.ReadInt32(bffnt.header.endian),
                    firstCharCode = firstChar = file.ReadInt16(bffnt.header.endian),
                    lastCharCode = lastChar = file.ReadInt16(bffnt.header.endian),
                    mapType = getMapMethod(),
                    mapDataSize = nextCMAPOffset = file.ReadInt32(),
                    charMaps = getCharMaps()
                };
                returnCMAPS.Add(cMAP);
                //Set position to end of section.
                file.Stream.Position = initPos + cMAP.secSize;
            } while (nextCMAPOffset != 0);
            bffnt.charMapInfo = returnCMAPS.ToArray();
        }

        /// <summary>
        /// Parses the KRNG Header
        /// </summary>
        private void ParseKerning(BFFNT bffnt, EndianStreamReader file)
        {
            long initPos = file.Stream.Position-4;
            int secSize;
            //Turns the Dictionary into an array of Character Kerns.
            CharKern[] toCharKernArray(Dictionary<int, CharKern> charKerns)
            {
                int maxIndex = charKerns.Keys.Max();
                CharKern[] outKerns = new CharKern[maxIndex + 1];
                foreach (KeyValuePair<int,CharKern> kern in charKerns)
                {
                    outKerns[kern.Key] = kern.Value;
                }
                return outKerns;
            }

            //Reads the Character Kernings.
            CharKern[] readCharKern()
            {
                Dictionary<int,CharKern> charKerns = new Dictionary<int, CharKern>();
                while (file.Position < (initPos + secSize))
                {
                    CharKern charKern = new CharKern
                    {
                        charCode = file.ReadUInt16(bffnt.header.endian),
                        offset = file.ReadInt16(bffnt.header.endian)
                    };
                    charKerns[charKern.charCode] = charKern;
                }
                return toCharKernArray(charKerns);
            }
            
            bffnt.kerningInfo = new KRNG
            {
                magic = "KRNG",
                secSize = secSize = file.ReadInt32(bffnt.header.endian),
                charKerns = readCharKern()
            };

            //Set position to end of section.
            file.Stream.Position = initPos + secSize;
        }

        /// <summary>
        /// Rounds the cursor to the nearest 4 bytes. 
        /// </summary>
        private void RoundCursor(EndianStreamReader file)
        {
            if(file.Position % 4 != 0)
            {
                file.Position = ((file.Position + 3) / 4) * 4;
            }
        }

        //Throws Custom exception and closes the file. 
        private void ThrowException(EndianStreamReader file, string message)
        {
            file.Stream.Close();
            throw new Exception(message);
        }
    }
}
