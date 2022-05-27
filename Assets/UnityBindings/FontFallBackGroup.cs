using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;

public class FontFallBackGroup : MonoBehaviour
{
    public FontStyleGroup[] fontStyles;

	public RFontFallBack fallbackgroup;

	public RFontFallBack Fallback
    {
        get
        {
			if(fallbackgroup == null)
            {
				Start();
            }
			return fallbackgroup;
		}
    }

	// Start is called before the first frame update
	void Start()
    {
        fallbackgroup = new RFontFallBack();
        fallbackgroup.FallBacks = new List<RFont>();
        foreach (var item in fontStyles)
        {
            fallbackgroup.FallBacks.Add(new RFontRoot(item.Fontstyle));
        }
    }

// Update is called once per frame
void Update()
    {
        
    }
}
