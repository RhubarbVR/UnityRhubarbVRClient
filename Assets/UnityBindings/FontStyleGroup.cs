using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;

public class FontStyleGroup : MonoBehaviour
{
	public Font Black;

	public Font BlackItalic;

	public Font Bold;

	public Font BoldItalic;

	public Font ExtraBold;

	public Font ExtraBoldItalic;

	public Font ExtraLight;

	public Font ExtraLightItalic;

	public Font Italic;

	public Font Light;

	public Font LightItalic;

	public Font Medium;

	public Font MediumItalic;

	public Font Regular;

	public Font SemiBold;

	public Font SemiBoldItalic;

	public Font Thin;

	public Font ThinItalic;

	public Font Oblique;

	public Font Strikeout;

	public Font Underline;

	public Font UnderlineItalic;

	public Font UnderlineBold;

	public Font UnderlineLight;

	public Font UnderlineLightItalic;

	public RFontStyleManager fontstyle;

	public RFontStyleManager Fontstyle
    {
        get
        {
			Start();
			return fontstyle;
		}
    }

	// Start is called before the first frame update
	void Start()
    {
		if (fontstyle == null)
		{
			fontstyle = new RFontStyleManager(new RenderFont(Regular));
			fontstyle.Black = new RenderFont(Black);
			fontstyle.BlackItalic = new RenderFont(BlackItalic);
			fontstyle.Bold = new RenderFont(Bold);
			fontstyle.BoldItalic = new RenderFont(BoldItalic);
			fontstyle.ExtraBold = new RenderFont(ExtraBold);
			fontstyle.ExtraBoldItalic = new RenderFont(ExtraBoldItalic);
			fontstyle.ExtraLight = new RenderFont(ExtraLight);
			fontstyle.ExtraLightItalic = new RenderFont(ExtraLightItalic);
			fontstyle.Italic = new RenderFont(Italic);
			fontstyle.Light = new RenderFont(Light);
			fontstyle.LightItalic = new RenderFont(LightItalic);
			fontstyle.Medium = new RenderFont(Medium);
			fontstyle.MediumItalic = new RenderFont(MediumItalic);
			fontstyle.Regular = new RenderFont(Regular);
			fontstyle.SemiBold = new RenderFont(SemiBold);
			fontstyle.SemiBoldItalic = new RenderFont(SemiBoldItalic);
			fontstyle.Thin = new RenderFont(Thin);
			fontstyle.ThinItalic = new RenderFont(ThinItalic);
			fontstyle.Oblique = new RenderFont(Oblique);
			fontstyle.Strikeout = new RenderFont(Strikeout);
			fontstyle.Underline = new RenderFont(Underline);
			fontstyle.UnderlineItalic = new RenderFont(UnderlineItalic);
			fontstyle.UnderlineBold = new RenderFont(UnderlineBold);
			fontstyle.UnderlineLight = new RenderFont(UnderlineLight);
			fontstyle.UnderlineLightItalic = new RenderFont(UnderlineLightItalic);
		}
}

// Update is called once per frame
void Update()
    {
        
    }
}
