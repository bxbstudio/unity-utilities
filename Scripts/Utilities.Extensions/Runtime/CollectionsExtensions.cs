#region Namespaces

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

#endregion

namespace Utilities
{
	[BurstCompile]
	public static class CollectionsExtensions
	{
		[BurstCompile]
		public static NativeList<T> AsNativeList<T>(this DynamicBuffer<T> buffer, Allocator allocator) where T : unmanaged, IBufferElementData
		{
			NativeList<T> list = new NativeList<T>(buffer.Length, allocator);

			for (int i = 0; i < buffer.Length; i++)
				list.Add(buffer[i]);

			return list;
		}
	}
}
