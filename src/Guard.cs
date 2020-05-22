using System;
using System.Linq;

namespace Rhapsody {
	public readonly struct Guard {
		public static void NotNull(string argumentName, object argument) {
			_ = argument ?? throw new ArgumentNullException(argumentName);
		}

		public static bool IsSafeMatch<T>(T value, params T[] against) where T : struct {
			return against.Contains(value);
		}
	}
}