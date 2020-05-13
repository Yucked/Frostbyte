using System;
using System.Linq;

namespace Rhapsody.Extensions {
	public static class StringExtensions {
		public static string Indent(this string content, int indent = 5) {
			var splits = content.Split(Environment.NewLine);
			if (splits.Length <= 1) {
				return $"{new string(' ', indent)}{content}";
			}

			var stringLength = splits.Sum(x => x.Length + indent) + splits.Length;
			return string.Create(stringLength, splits, (span, strings) => {
				var position = 0;
				for (var i = 0; i < strings.Length; i++) {
					var trimmed = strings[i].Trim();
					var tempChars = new char[trimmed.Length + indent + 1];
					for (var s = 0; s < tempChars.Length; s++) {
						if (Enumerable.Range(0, indent).Contains(s)) {
							tempChars[s] = ' ';
						}
						else {
							tempChars[s] = tempChars.Length - 1 == s
								? '\n'
								: trimmed[s - indent];
						}
					}

					var str = tempChars.AsSpan();
					str.CopyTo(i == 0
						? span
						: span.Slice(position));
					position += str.Length;
				}
			});
		}
	}
}