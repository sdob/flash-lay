using System.Collections.Generic;
using System.Linq;
using Assets;
using UnityEngine;

public class Plotter : MonoBehaviour
{
    public GameObject PointPrefab;
    public GameObject BufferPrefab;
    public PolygonCollider2D Collider;
    public float BufferDistance = 40;

	/// <summary>
	/// A tolerance value that we pass to the LineUtility.Simplify method.
	/// Setting this value to 0 means that every point in the collider and its buffer
	/// will be plotted; higher values allow fewer points. 10 is a fairly good value.
	/// </summary>
	public float Tolerance = 10;

	/// <summary>
	/// Use Clipper for generating buffer points, or a somewhat janky best-effort hack?
	/// </summary>
	public bool UseClipperForBufferPoints = true;

	// Container lists for instantiated prefabs
	private List<GameObject> markerPoints = new List<GameObject>();
	private List<GameObject> bufferPoints = new List<GameObject>();

	// Transforms to hold the instantiated prefabs, just to keep the Hierarchy
	// window tidy. We'll instantiate these on retrieval so that we can create them
	// in Edit mode without declaring the whole class ExecuteInEditmode.

	private Transform _mp;
	private Transform MarkerParent
	{
		get
		{
			return _mp ?? (_mp = new GameObject("Marker points").transform);
		}
	}

	private Transform _bp;
	private Transform BufferParent
	{
		get
		{
			return _bp ?? (_bp = new GameObject("Buffer points").transform);
		}
	}


	public void Clear()
    {
        Debug.Log("Clear");
		_Clear(markerPoints);
		_Clear(bufferPoints);
    }

    public void PlotPoints()
    {
        Debug.Log("PlotPoints");
		// Wipe the existing list
		_Clear(markerPoints);
		// Simplify the collider coordinates
		var simplifiedPoints = Simplify(Collider.points.ToList(), Tolerance);
		// Add a PointPrefab for each point
		for (int i = 0; i < simplifiedPoints.Count; i++)
		{
			var name = "Marker point " + (i + 1);
			AddPoint(simplifiedPoints[i], name, PointPrefab, markerPoints, MarkerParent);
		}
    }

    public void PlotBuffer()
    {
        Debug.Log("PlotBuffer");
		// Wipe the existing list
		_Clear(bufferPoints);
		// If we're not using Clipper to compute the buffer coords, then we want to simplify the polygon
		// beforehand to try and reduce the number of outliers
		List<Vector2> candidatePoints = UseClipperForBufferPoints
			? Collider.points.ToList()
			: Simplify(Collider.points, Tolerance);
		// Get an offset of the collider coordinates
		List<Vector2> offsetPoints = OffsetUtility.Offset(candidatePoints, BufferDistance, UseClipperForBufferPoints);
		// Simplify the offset coords
		List<Vector2> simplifiedPoints = Simplify(offsetPoints, Tolerance);
		// Add a BufferPrefab for each point
		for (int i = 0; i < simplifiedPoints.Count; i++)
		{
			var name = "Buffer point " + (i + 1);
			AddPoint(simplifiedPoints[i], name, BufferPrefab, bufferPoints, BufferParent);
		}
	}

	/// <summary>
	/// Instantiate a prefab in the given position (and take care of some management stuff).
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	/// <param name="prefab"></param>
	/// <param name="container"></param>
	/// <param name="parent"></param>
	private void AddPoint(Vector2 position, string name, GameObject prefab, List<GameObject> container, Transform parent)
	{
		var obj = Instantiate(prefab);
		obj.name = name;
		obj.transform.parent = parent;
		obj.transform.position = position;
		container.Add(obj);
	}

	private void _Clear(List<GameObject> list)
	{
		list.ForEach(gameObject =>
		{
			gameObject.transform.parent = null;
#if UNITY_EDITOR
				DestroyImmediate(gameObject);
#else
				Destroy(point);
#endif
		});
		list.Clear();
	}


	/// <summary>
	/// Given a sequence points describing the vertices of a polyline,
	/// and a <c>tolerance</c> value, return a subset of <c>points</c> that simplifies
	/// the polyline.
	/// </summary>
	/// <param name="points">Vertices of a polyline</param>
	/// <param name="tolerance"></param>
	/// <returns></returns>
	private List<Vector2> Simplify(IList<Vector2> points, float tolerance)
	{
		// The Ramer-Douglas-Peucker algorithm automatically marks the first and last
		// point to be kept no matter what, but that's not exactly what we want, since
		// the points describe a closed polygon. So we duplicate the first point before
		// simplifying, then remove it before returning. (Otherwise you get two buffer points
		// very close together, even at high tolerance levels.)
		var sentinelled = points.Concat(new[] { points[0] }).ToList();
		var simplifiedPoints = new List<Vector2>();
		LineUtility.Simplify(sentinelled, Tolerance, simplifiedPoints);
		return simplifiedPoints.Take(simplifiedPoints.Count - 1).ToList();
	}
}