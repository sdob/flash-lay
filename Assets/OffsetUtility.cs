using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using UnityEngine;

namespace Assets
{
	class OffsetUtility
	{
		/// <summary>
		/// Offset the vertices of a given polygon by a fixed distance.
		/// </summary>
		/// 
		/// <remarks>
		/// There are two implementations available; one that uses the Clipper
		/// library, and a naive approach.
		/// </remarks>
		/// <param name="points">A list of vertices.</param>
		/// <param name="bufferDistance">How far to offset the polygon.</param>
		/// <param name="useClipper">A flag to select which implementation to use.</param>
		/// <returns></returns>
		public static List<Vector2> Offset(IList<Vector2> points, float bufferDistance, bool useClipper)
		{
			if (useClipper)
			{
				return OffsetWithClipper(points, bufferDistance);
			}
			return OffsetWithNorms(points, bufferDistance);
		}

		/// <summary>
		/// Offset the polygon defined by <c>points</c>, using Clipper.
		/// This should work nicely on non-convex polygons at arbitrary tolerance levels.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="bufferDistance"></param>
		/// <returns></returns>
		private static List<Vector2> OffsetWithClipper(IList<Vector2> points, float bufferDistance)
		{
			// Clipper operates over integers, so to keep things accurate we want to scale up
			// by some arbitrary biggish number, and then later scale back down
			float scale = 100f;
			ClipperOffset co = new ClipperOffset();
			// This list will get populated with the solution when Clipper runs
			List<List<IntPoint>> solution = new List<List<IntPoint>>();
			// Convert our Vector2s to upscaled IntPoints and add them to Clipper
			List<IntPoint> intPoints = points.Select(p => new IntPoint(p.x * scale, p.y * scale)).ToList();
			co.AddPath(
				intPoints,
				JoinType.jtMiter,
				EndType.etClosedLine
			);
			// Run the offsetting algorithm
			co.Execute(ref solution, bufferDistance * scale);
			// Downscale the solution, convert back to Vector2s, and return
			return solution[0]
				.Select(point => new Vector2(point.X / scale, point.Y / scale))
				.ToList();
		}

		/// <summary>
		/// Estimate the direction in which we should move each point away from the polyline
		/// it's on by looking at itself and its neighbours.
		/// </summary>
		/// <remarks>
		/// This works fine on convex and fairly gently concave polygons, but gets confused by
		/// tight convex areas, particularly at very low tolerance levels. It works slightly
		/// better if the polygon is simplified before being offset but still has difficulty
		/// with the south-southeast part of the example asteroid.
		/// </remarks>
		/// <param name="points"></param>
		/// <param name="bufferDistance"></param>
		/// <returns></returns>
		private static List<Vector2> OffsetWithNorms(IList<Vector2> points, float bufferDistance)
		{
			List<Vector2> offsetPoints = new List<Vector2>(points.Count);
			for (int i = 0; i < points.Count; i++)
			{
				// Get this point and its neighbours
				Vector2 prev = points[i - 1 < 0 ? points.Count - 1 : i - 1];
				Vector2 self = points[i];
				Vector2 next = points[(i + 1) % points.Count];
				// Calculate the midpoints of prev->self and self->next
				Vector2 midA = (prev + self) / 2f;
				Vector2 midB = (self + next) / 2f;
				// Calculate the outward-facing norm of the line between midpoints
				var dx = midB.x - midA.x;
				var dy = midB.y - midA.y;
				var norm = new Vector2(dy, -dx).normalized;
				// Displace the original point along the norm and add it to the list
				Vector2 newPoint = self + norm * bufferDistance;
				offsetPoints.Add(newPoint);
			}
			return offsetPoints;
		}
	}
}
