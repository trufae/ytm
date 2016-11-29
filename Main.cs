using System;
using System.IO;
using System.Collections.Generic;
using static org.pancake.UnixShell;

static class MyExtensions {
	private static Random rng = new Random();  
	public static void Shuffle<T>(this IList<T> list) {
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}
}

public class YoutubeMusic {
	private static string getLocalMediaPath() {
		string homeDir = getHomePath();
		return Path.Combine(homeDir, ".ytm");
	}

	static string getFormat(string url) {
		string formats = sh("youtube-dl -F " + url);
		string[] lines = formats.Split('\n');
		bool formatCode = false;
		foreach (string line in lines) {
			if (formatCode) {
				if (line.IndexOf("audio only") != -1) {
					if (line.IndexOf("m4a") != -1) {
						return line.Split(' ')[0];
					}
				}
			} else if (line.StartsWith("format code")) {
				formatCode = true;
			}
		}
		return null;
	}

	private static void ytDownload(string url, string fmt = null) {
		Directory.SetCurrentDirectory(abspath("~/.ytm"));
		/* TODO: avoid command injection vuln here */
		if (fmt != null) {
			run(new string[] { "youtube-dl", "-f", fmt, url }, false);
		} else {
		//	run((string[])["youtube-dl", url], false);
		}
	}

	private static void listLocalMedia() {
		string[] files = Directory.GetFiles(getLocalMediaPath());
		foreach (string file in files) {
			int from = file.LastIndexOf(Path.DirectorySeparatorChar);
			int to = file.LastIndexOf(".");
			if (from != -1 && to != -1) {
				string mediaName = file.Substring(from + 1, to - from - 1);
				log(mediaName);
			}
		}
	}

	private static void playMediaMatching(string arg) {
		List<string> matching = new List<string>();
		string[] files = Directory.GetFiles(getLocalMediaPath());
		/* find matching media */
		foreach (string file in files) {
			int from = file.LastIndexOf(Path.DirectorySeparatorChar);
			int to = file.LastIndexOf(".");
			if (from != -1 && to != -1) {
				string mediaName = file.Substring(from + 1, to - from - 1);
				if (mediaName.IndexOf(arg) != -1) {
					matching.Add(file);
				}
			}
		}
		/* randomize playlist */
		matching.Shuffle();
		foreach (string file in matching) {
			log(file);
			run("afplay", "\"" + file + "\"", false);
		}
	}

	public static void Main(String[] args) {
		Console.CancelKeyPress += (obj, arg) => {
			/* just close the youtube-dl or the afplay running .. */
			arg.Cancel = true;
		};
		mkdir("~/.ytm");
		if (args.Length > 0) {
			if (args[0].Length > 8 && args[0].Substring(0, 8) == "https://") {
				string url = args[0];
				log("Downloading media...");
				string format = getFormat(url);
				if (format == null) {
					log("Cannot find supported media format.");
				} else {
					ytDownload(url, format);
				}
			} else {
				playMediaMatching(args[0]);
			}
		} else {
			listLocalMedia();
		}
	}
}
