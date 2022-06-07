using System;
using UnityEngine;

namespace Kobbelt
{
	public struct Edge : IEquatable<Edge>
	{
		public Vector3 V1 { get; set; }
		public Vector3 V2 { get; set; }
		
		public bool Equals(Edge other)
		{
			return (V1.Equals(other.V1) && V2.Equals(other.V2)) || (V2.Equals(other.V1) && V1.Equals(other.V2));
		}

		public override bool Equals(object obj)
		{
			return obj is Edge other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int v1Hash = V1.GetHashCode();
				int v2Hash = V2.GetHashCode();

				return v1Hash < v2Hash ? (v1Hash * 397) ^ v2Hash : (v2Hash * 397) ^ v1Hash;
			}
		}
	}
}