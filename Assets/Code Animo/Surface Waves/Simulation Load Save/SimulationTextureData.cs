using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeAnimo.SurfaceWaves
{

	public class SimulationTextureData : ScriptableObject
	{

		[HideInInspector] [SerializeField] public Vector4[] pixels;
	}
}