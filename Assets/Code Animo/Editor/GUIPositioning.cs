// Copyright © 2014 Laurens Mathot
// Code Animo™ http://codeanimo.com

using UnityEngine;
using System.Collections;

namespace CodeAnimo{

	public class GUIPositioning {
		
		/// <summary>
		/// Centers the range inside another range.
		/// </summary>
		/// <returns>
		/// The new start of the center range
		/// </returns>
		/// <param name='centerRangeWidth'>
		/// The width of the range that should be centered
		/// </param>
		/// <param name='outerRangeStart'>
		/// The start value of the range in which the other range should be centered
		/// </param>
		/// <param name='outerRangeWidth'>
		/// The width of the range in which the other range should be centered.
		/// </param>
		public static float CenterRangeInRange(float centerRangeWidth, float outerRangeStart, float outerRangeWidth){
			float outerCenter = 0.5f * outerRangeWidth + outerRangeStart;
			return outerCenter - (0.5f * centerRangeWidth);
		}
		
		/// <summary>
		/// Centers a Rectangle inside another Rectangle
		/// </summary>
		/// <returns>
		/// The centered Rectangle
		/// </returns>
		/// <param name='centerRect'>
		/// The Rectangle that should be centered
		/// </param>
		/// <param name='outerRect'>
		/// The Rectangle that the other rectangle should be centered in.
		/// </param>
		public static Rect CenterRectInRect(Rect centerRect, Rect outerRect){
			float leftEdge = CenterRangeInRange(centerRect.width, outerRect.x, outerRect.width);
			float topEdge = CenterRangeInRange(centerRect.height, outerRect.y, outerRect.height);
			
			return new Rect(leftEdge, topEdge, centerRect.width,	centerRect.height);
		}
		
	}
}