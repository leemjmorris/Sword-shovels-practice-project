using UnityEngine;
using UnityEngine.UI;

namespace CrusaderUI.Scripts
{
	public class HPFlowController : MonoBehaviour {
	
		private Material _material;

		private void Start ()
		{
			_material = GetComponent<Image>().material;
		}

		public void SetValue(float value)
		{
			if (_material == null)
			{
				Debug.LogWarning("HPFlowController: Material is null! Make sure the Image component has a material with shader assigned.");
				return;
			}
			_material.SetFloat("_FillLevel", value);
		}
	}
}
