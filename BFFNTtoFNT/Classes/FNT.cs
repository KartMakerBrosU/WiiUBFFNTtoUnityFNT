using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFFNTtoFNT.Classes
{
    public class FontSettings
    {
        public FontYAML Font { get; set; } = new FontYAML();
    }
    public class CharacterRect
    {
        public int serializedVersion { get; set; }
        public int index { get; set; }
        public UV uv { get; set; } = new UV();
        public VERT vert { get; set; } = new VERT();
        public int advance { get; set; }
        public int flipped { get; set; }
    }
    public class UV
    {
        public int serializedVersion { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }
    public class VERT
    {
        public int serializedVersion { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    }
    public class FontYAML
    {
        public int m_OjectHideFlags { get; set; }
        public Dictionary<string, string> m_CorrespondingSourceObject { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> m_PrefabInstance { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> m_PrefabAsset { get; set; } = new Dictionary<string, string>();
        public string m_Name { get; set; } = "";
        public int serializedVersion { get; set; }
        public int m_LineSpacing { get; set; }
        public Dictionary<string, string> m_DefaultMaterial { get; set; } = new Dictionary<string, string>();
        public int m_FontSize { get; set; }
        public Dictionary<string, string> m_Texture { get; set; } = new Dictionary<string, string>();
        public int m_AsciiStartOffset { get; set; }
        public int m_Tracking { get; set; }
        public int m_CharacterSpacing { get; set; }
        public int m_CharacterPadding { get; set; }
        public int m_ConvertCase { get; set; }
        public CharacterRect[] m_CharacterRects { get; set; } = Array.Empty<CharacterRect>();
        public string[] m_KerningValues { get; set; } = Array.Empty<string>();
        public float m_PixelScale { get; set; }
        public int m_FontData { get; set; }
        public float m_Ascent { get; set; }
        public float m_Descent { get; set; }
        public int m_DefaultStyle { get; set; }
        public string[] m_FontNames { get; set; } = Array.Empty<string>();
        public string[] m_FallbackFonts { get; set; } = Array.Empty <string>();
        public int m_FontRenderingMode { get; set; }
        public int m_UseLegacyBoundsCalculation { get; set; }
        public int m_ShouldRoundAdvanceValue { get; set; }
    }
}
