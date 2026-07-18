#region Namespaces

using System.Linq;
using UnityEngine;

#endregion

namespace Utilities
{
	public static class GameObjectExtensions
	{
		public static Bounds GetObjectBounds(this GameObject gameObject, LayerMask layerMask, bool keepRotation = false, bool keepScale = true)
		{
			Renderer[] source = gameObject.GetComponentsInChildren<Renderer>();

			if (source.Length > 0)
				source = source.Where(renderer => !(renderer is TrailRenderer) && !(renderer is ParticleSystemRenderer)).ToArray();

			Bounds result = default;
			Quaternion rotation = gameObject.transform.rotation;
			Vector3 localScale = gameObject.transform.localScale;

			if (!keepScale)
				gameObject.transform.localScale = Vector3.one;

			if (!keepRotation)
				gameObject.transform.rotation = Quaternion.identity;

			for (int i = 0; i < source.Length; i++)
				if (layerMask.HasLayer(source[i].gameObject.layer))
				{
					if (result.size == Vector3.zero)
						result = source[i].bounds;
					else
						result.Encapsulate(source[i].bounds);
				}

			if (!keepScale)
				gameObject.transform.localScale = localScale;

			if (!keepRotation)
				gameObject.transform.rotation = rotation;

			return result;
		}
	}
}
