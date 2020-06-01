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
			var range = Enumerable.Range(0, indent).ToArray();

			return string.Create(stringLength, splits, (span, strings) => {
				var position = 0;
				for (var i = 0; i < strings.Length; i++) {
					var trimmed = strings[i].Trim();
					var tempChars = new char[trimmed.Length + indent + 1];

					for (var s = 0; s < tempChars.Length; s++) {
						if (range.Contains(s)) {
							tempChars[s] = ' ';
						}
						else {
							tempChars[s] = tempChars.Length - 1 == s
								? '\n'
								: trimmed[s - indent];
						}
					}

					if (i == strings.Length - 1) {
						tempChars.AsSpan().Slice(0, tempChars.Length - 1);
					}

					var charSpan = tempChars.AsSpan();
					var str = i == strings.Length - 1
						? charSpan.Slice(0, charSpan.Length - 1)
						: charSpan;
					str.CopyTo(i == 0 ? span : span.Slice(position));
					position += str.Length;
				}
			});
		}

		public static string Wrap(this string str, int wrapAfter = 50) {
			var words = str.Split(' ');
			if (words.Length <= 50) {
				return str;
			}

			var wrappedAt = 0;
			var result = new string[words.Length + words.Length - 1];
			for (var i = 0; i < words.Length; i++) {
				var s = i++;
				result[i] = words[i];
				if (wrappedAt == wrapAfter) {
					wrappedAt = 0;
					result[s] = "\n";
				}
				else {
					result[s] = " ";
					wrappedAt++;
				}
			}

			return string.Join("", result);
		}

		public static bool IsMatchFilter(this string categoryFilter, ref string category) {
			if (categoryFilter.Contains("*")) {
				categoryFilter = categoryFilter[..^2];
			}

			var splits = category.Split('.');
			var tempCategory = string.Empty;

			foreach (var split in splits) {
				tempCategory += split;
				if (categoryFilter == tempCategory) {
					break;
				}

				tempCategory += ".";
			}

			category = tempCategory;
			return true;
		}

		public static bool IsFuzzyMatch(this string str, string against, int percentage = 95) {
			var n = str.Length;
			var m = against.Length;
			var d = new int[n + 1, m + 1];

			if (n == 0) {
				return false;
			}

			if (m == 0) {
				return false;
			}

			for (var i = 0; i <= n; d[i, 0] = i++) {
			}

			for (var j = 0; j <= m; d[0, j] = j++) {
			}

			for (var i = 1; i <= n; i++) {
				for (var j = 1; j <= m; j++) {
					var cost = against[j - 1] == str[i - 1] ? 0 : 1;
					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}

			var result = 100 - d[n, m];
			return result >= percentage;
		}
	}
}