# Q2

A couple of quick notes on the implementation:

1. The marker and buffer logic leans on `LineUtility.Simplify` to pick a subset of the asteroid's collider's points.
(In my defence, I'd almost certainly go with Ramer-Douglas-Peucker to simplify the polygon even if it weren't available
in the Unity standard library.) This requires a `tolerance` value, which is exposed in the inspector.
Setting `tolerance` to 0 plots every collider and buffer point;
higher numbers will produce fewer points. 10 is a pretty good value, although YMMV.
1. Generating buffer points on non-convex polygons is tricky, so I cheated. I've introduced an external dependency in the form of
the Clipper library, which implements Vatti's polygon clipping algorithm. Because this seems to violate the spirit of the
problem, I've also included a naïve algorithm that attempts to offset (i.e., buffer) the collider polygon by estimating the
best outward-facing norm for each point, based on its neighbours. This would work pretty unexceptionably for convex
(and gently concave) polygons but has difficulty handling tight convex areas (it'll produce outliers in the south-southeast part of the asteroid
in particular, for example). There's a checkbox in the inspector that allows you to switch between the Clipper-driven
implementation and my hand-rolled approach.