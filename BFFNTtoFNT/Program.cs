using BFFNTtoFNT.Classes;

namespace BFFNTtoFNT
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Open the BFFNT File.
            string ffnt_filedir;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = "Open BFFNT";
                ofd.Filter = "Binary caFe FoNT files (.bffnt)|*.bffnt";
                if(ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                ffnt_filedir = ofd.FileName;
                
            }
            //Read and Parse the BFFNT File.
            BFFNT wiiU_file = new BFFNT(ffnt_filedir);
            wiiU_file.ReadBFFNT();

            //Get CharacterRect's for Unity Font
            CharacterRect[] getCharRects()
            {
                List<CharMap> chars = new List<CharMap>();
                foreach (CMAP cmap in wiiU_file.charMapInfo)
                {
                    chars.AddRange(cmap.charMaps);
                }
                CharacterRect[] outChars = new CharacterRect[chars.Count];
                float leftDistance;
                float cellWidth;
                float cellHeight;
                float textureWidth;
                float textureHeight;
                float glyphWidth;
                float charWidth;
                int cellPerRow;
                int ascent;
                int baselinepos;
                int getKernOffset(int charCode)
                {
                    CharKern[] charKerns = wiiU_file.kerningInfo.charKerns;
                    if (charCode > charKerns.Length)
                    {
                        return 0;
                    }
                    else if (charKerns[charCode] == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return charKerns[charCode].offset;
                    }
                }
                for (int i = 0; i < outChars.Length; i++)
                {
                    cellWidth = wiiU_file.texInfo.cellWidth + 1;
                    cellHeight = wiiU_file.texInfo.cellHeight;
                    leftDistance = wiiU_file.charWidthInfo.charWidths[i].leftDistance;
                    charWidth = wiiU_file.charWidthInfo.charWidths[i].charWidth;
                    glyphWidth = wiiU_file.charWidthInfo.charWidths[i].glyphWidth;
                    textureWidth = wiiU_file.texInfo.texWidth;
                    textureHeight = wiiU_file.texInfo.texHeight;
                    cellPerRow = wiiU_file.texInfo.cellPerRow;
                    ascent = wiiU_file.fontInfo.ascent;
                    baselinepos = wiiU_file.texInfo.baseLinePos;
                    outChars[i] = new CharacterRect
                    {
                        serializedVersion = 2,
                        index = chars[i].charCode,
                        uv = new UV
                        {
                            serializedVersion = 2,
                            //x = ((cellWidth * (i % cellPerRow))+leftDistance) / textureWidth,
                            x = ((cellWidth * (i % cellPerRow)) + 1) / textureWidth, //(i % cellPerRow)
                            y = 1 - (((cellHeight * (i / cellPerRow)) + 1 + cellHeight + (i / cellPerRow)) / textureHeight),

                            width = charWidth / textureWidth,
                            //width = cellWidth/textureWidth,
                            height = cellHeight / textureHeight,
                        },
                        vert = new VERT
                        {
                            serializedVersion = 2,
                            x = leftDistance,
                            y = baselinepos - ascent,
                            //x = 0,
                            //y = 0,
                            width = charWidth,
                            //width = cellWidth,
                            height = -cellHeight
                        },
                        advance = (int)(glyphWidth) + getKernOffset(chars[i].charCode),
                        //advance = (int)(cellWidth),
                        flipped = 0
                    };
                }

                return outChars;
            }

            //Create the UnityFont.
            var Font = new FontSettings
            {
                Font = new FontYAML
                {
                    m_OjectHideFlags = 0,
                    m_CorrespondingSourceObject = new Dictionary<string, string>
                    {
                        { "fileID", "0" },
                    },
                    m_PrefabInstance = new Dictionary<string, string>
                    {
                        { "fileID", "0" },
                    },
                    m_PrefabAsset = new Dictionary<string, string>
                    {
                        { "fileID", "0" },
                    },
                    m_Name = Path.GetFileNameWithoutExtension(wiiU_file.fileDirectory),
                    serializedVersion = 5,
                    m_LineSpacing = wiiU_file.fontInfo.height,
                    m_DefaultMaterial = new Dictionary<string, string>
                    {
                        { "fileID", "0" },
                    },
                    m_FontSize = wiiU_file.texInfo.cellHeight,
                    m_Texture = new Dictionary<string, string>
                    {
                        { "fileID", "0" },
                    },
                    m_AsciiStartOffset = 0,
                    m_Tracking = 1,
                    m_CharacterSpacing = 0,
                    m_CharacterPadding = 1,
                    m_ConvertCase = 0,
                    m_CharacterRects = getCharRects(),
                    m_KerningValues = new string[0],
                    m_PixelScale = 1f,
                    m_Ascent = 0f,
                    m_Descent = 0f,
                    m_DefaultStyle = 0,
                    m_FontNames = new string[]
                    {
                        Path.GetFileNameWithoutExtension(wiiU_file.fileDirectory),
                    },
                    m_FallbackFonts = new string[0],
                    m_FontRenderingMode = 0,
                    m_UseLegacyBoundsCalculation = 0,
                    m_ShouldRoundAdvanceValue = 1,
                },
            };

            //Write the UnityFont
            string fnt_dileDir = ffnt_filedir.Replace(".bffnt", ".fontsettings");
            WriteFNT write = new WriteFNT(Font, fnt_dileDir);
            MessageBox.Show("File converted Sucessfully \n You must extract the textures on your own.");
        }
    }
}