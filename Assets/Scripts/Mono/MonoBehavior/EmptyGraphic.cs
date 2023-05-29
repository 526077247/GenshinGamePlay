using UnityEngine;
using UnityEngine.UI;

public class EmptyGraphic : Graphic
{
#if UNITY_EDITOR
	public bool isShow = false;

	protected override void OnPopulateMesh (VertexHelper vh)
	{
		if (isShow)
			base.OnPopulateMesh (vh);
		else
			vh.Clear ();
	}
#else
	protected override void OnPopulateMesh (VertexHelper vh)
	{
		vh.Clear ();
	}
#endif
}
