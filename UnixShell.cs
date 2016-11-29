using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace org.pancake {
	public class UnixShell {
		public static string getHomePath() {
			string homePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
					Environment.OSVersion.Platform == PlatformID.MacOSX)
				? Environment.GetEnvironmentVariable("HOME")
				: Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
			return homePath;
		}

		public static void log(String msg) {
			Console.WriteLine(msg);
		}

		public static void ls(String path) {
			string[] files = Directory.GetFiles(abspath(path));
			foreach (string file in files) {
				int from = file.LastIndexOf(Path.DirectorySeparatorChar);
				if (from != -1) {
					log(file.Substring(from));
				} else {
					log(file);
				}
			}
		}

		public static string abspath(string path) {
			if (path[0] == '~') {
				string next = path.Length > 1
					? path.Substring(2): "";
				path = Path.Combine(getHomePath(), next);
			}
			return path;
		}

		public static void mkdir(string path) {
			// sh("mkdir -p " + path);
			System.IO.Directory.CreateDirectory(abspath(path));
		}

		public static string sh(string cmd, bool getString = true) {
			return run("sh", "-c '" + cmd + "'", getString);
		}

		public static string run(string[] args, bool getString = true) {
			return sh(String.Join(" ", args), getString);
		}

		public static string run(string cmd, string args, bool getString = true) {
			var p = new Process();
			p.StartInfo = new ProcessStartInfo() {
				CreateNoWindow = true,
				RedirectStandardOutput = getString,
				RedirectStandardInput = true,
				UseShellExecute = false,
				Arguments = args,
				FileName = cmd
			};
			p.Start();
			string res = null;
			if (getString) {
				var sb = new StringBuilder();
				while (true) {
					var buffer = p.StandardOutput.Read();
					if (buffer == -1) {
						break;
					}
					sb.Append((char)buffer);
				}
				res = sb.ToString();
			}
			p.StandardInput.AutoFlush = true;
			p.WaitForExit();
			p.Dispose();
			return res;
		}
	}
}
