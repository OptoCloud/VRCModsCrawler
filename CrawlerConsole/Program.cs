using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VRChatRipper
{
	class Program
	{
		const string mainPageRegex = @"<div class='col-12 col-md-6 col-lg-4 mb-4 item-wrapper'>\s*<a href='\/item\/([0-9]+)'>\s*<div class='item-bg' style=?'background-image: url\((https:\/\/vrcmods.com\/imgs\/\w+\.[a-z]+)\);'>\s*<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>(.+)<[^<>]+>";
		const string assetPageRegex = "<span class=\"timeAgo\">(\\d+)<\\/span> ago<\\/span><\\/p>";

		static object procLock = new object();
		static object fileLock = new object();
		static object consoleLock = new object();

		static ConcurrentDictionary<string, ulong> urlAgeDictionary = new ConcurrentDictionary<string, ulong>();

		static String crashMessage = String.Empty;

		static void Main(string[] args)
		{
			try
			{
				File.AppendAllText("cache.dat", "");
				string[] entries = File.ReadAllLines("cache.dat");
				if (entries != null)
				{
					foreach (string entry in entries)
					{
						string[] strings = entry.Split('-');

						if (strings.Length != 2)
						{
							crashMessage = "Invalid cache file! (Too many fields)";
							goto crash;
						}

						if (string.IsNullOrEmpty(strings[0]) || string.IsNullOrEmpty(strings[1]))
						{
							crashMessage = "Invalid cache file! (Field is empty)";
							goto crash;
						}

						if (!ulong.TryParse(strings[1], out ulong time))
						{
							crashMessage = "Invalid cache file! (Cannot parse time)";
							goto crash;
						}

						urlAgeDictionary.TryAdd(strings[0], time);
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				crashMessage = ex.Message;
				return;
#endif
			}


			EnsureFolder("downloads");

			const string orderbyString = "downloads";
			int pageNum = 0;
			int procNum = 0;
			try
			{
				while (true)
				{
					try
					{
						pageNum++;
						string pageUrl = string.Format("https://vrcmods.com/search/{0}/{1}/", orderbyString, pageNum);

						WebRequest req = WebRequest.Create(pageUrl);

						using (WebResponse response = req.GetResponse())
						{
							string filename = Path.GetFileName(response.ResponseUri.AbsoluteUri);
							using (StreamReader sr = new StreamReader(response.GetResponseStream()))
							{
								string html = sr.ReadToEnd();
								MatchCollection matches = Regex.Matches(html, mainPageRegex);
								foreach (Match match in matches)
								{
									string itemID = match.Groups[1].Value;
									string itemName = match.Groups[3].Value;
									string imageLink = match.Groups[2].Value;

									if (itemID == "1")
										continue;

									Task.Run(() =>
									{
										lock (procLock)
											procNum++;
										try
										{
											string assetID = MakeValidFileName(itemName + '-' + itemID);
											string assetID_B64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(assetID));

											bool exists = false;
											bool gotAge = GetAssetAge(itemID, out ulong age);

											string assetLink = string.Format("https://vrcmods.com/download/direct/{0}/", itemID);

											if (urlAgeDictionary.ContainsKey(assetID_B64))
											{
												exists = true;
												if (gotAge && age >= urlAgeDictionary[assetID_B64])
												{
													lock (consoleLock)
														Console.WriteLine("Skipped {0}", itemName);
													goto end;
												}
											}

											string folder = string.Format("downloads\\{0}\\", assetID, '_', false);

											lock (fileLock)
												EnsureFolder(folder);

											{
												int retval = DownloadFile(folder, "thumbnail", imageLink);
												if (retval == -1)
												{
													lock (consoleLock)
														Console.WriteLine("Failed to download {0}!", imageLink);
													goto end;
												}
											}

											{
												int retval = DownloadFile(folder, assetID, assetLink);
												if (retval == -1)
												{
													lock (consoleLock)
														Console.WriteLine("Failed to download {0}!", imageLink);
													goto end;
												}
											}

											if (!exists)
											{
												urlAgeDictionary.TryAdd(assetID_B64, age);

												lock (fileLock)
												{
													File.AppendAllText("cache.dat", assetID_B64 + '-' + age.ToString() + '\n', Encoding.UTF8);
													File.AppendAllText("lookup.txt", "Filename: " + assetID + " Url: " + assetLink + '\n', Encoding.UTF8);
												}

												lock (consoleLock)
													Console.WriteLine("Downloaded {0}", itemName);
											}
											else
											{
												lock (consoleLock)
													Console.WriteLine("Updated {0}", itemName);
											}
										}
										catch (Exception ex)
										{
#if DEBUG
											lock (consoleLock)
												Console.WriteLine(ex.Message);
#endif
										}
									end:
										lock (procLock)
											procNum--;
									}).ConfigureAwait(false);
								}
							}
						}

						while (procNum > 0)
						{
							Thread.Sleep(100);
						}
					}
					catch (Exception ex)
					{
#if DEBUG
						lock (consoleLock)
							Console.WriteLine(ex.Message);
#endif
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				crashMessage = ex.Message;
#endif
			}
		crash:

			Console.WriteLine("Application failed to start:");
			Console.WriteLine(crashMessage);
			Console.WriteLine();
			Console.WriteLine("Press return to exit...");

			lock (consoleLock)
				Console.ReadLine();
		}

		static char[] _invalids;

		/// <summary>Replaces characters in <c>text</c> that are not allowed in 
		/// file names with the specified replacement character.</summary>
		/// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
		/// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
		/// <param name="fancy">Whether to replace quotes and slashes with the non-ASCII characters ” and ⁄.</param>
		/// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
		public static string MakeValidFileName(string text, char? replacement = '_', bool fancy = true)
		{
			StringBuilder sb = new StringBuilder(text.Length);
			var invalids = _invalids ?? (_invalids = Path.GetInvalidFileNameChars());
			bool changed = false;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (invalids.Contains(c))
				{
					changed = true;
					var repl = replacement ?? '\0';
					if (fancy)
					{
						if (c == '"') repl = '”'; // U+201D right double quotation mark
						else if (c == '\'') repl = '’'; // U+2019 right single quotation mark
						else if (c == '/') repl = '⁄'; // U+2044 fraction slash
					}
					if (repl != '\0')
						sb.Append(repl);
				}
				else
					sb.Append(c);
			}
			if (sb.Length == 0)
				return "_";
			return changed ? sb.ToString() : text;
		}

		static void EnsureFolder(string foldername)
		{
			foldername = Path.Combine(Directory.GetCurrentDirectory(), foldername);
			bool exists = System.IO.Directory.Exists(foldername);
			if (!exists)
				System.IO.Directory.CreateDirectory(foldername);
		}

		static int WriteStreamToFile(string rPath, Stream inputStream)
		{
			if (string.IsNullOrEmpty(rPath))
				return -1;

			int retval = 1;

			try
			{

				if (File.Exists(rPath))
				{
					File.Delete(rPath);
					retval = 0;
				}

				using (var fileStream = File.Create(Path.Combine(Directory.GetCurrentDirectory(), rPath)))
				{
					inputStream.CopyTo(fileStream);
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				lock (consoleLock)
					Console.WriteLine(ex.Message);
#endif
				return -1;
			}

			return retval;
		}

		static int DownloadFile(string folder, string filename, string url)
		{
			if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(filename) || string.IsNullOrWhiteSpace(url))
				return -1;

			try
			{
				WebRequest req = WebRequest.Create(url);

				using (WebResponse response = req.GetResponse())
				{
					string contDispo = response.Headers["Content-Disposition"];

					if (string.IsNullOrWhiteSpace(contDispo))
					{
						filename = filename + Path.GetExtension(Path.GetFileName(response.ResponseUri.AbsoluteUri));
					}
					else
					{
						int startIndex = contDispo.IndexOf("filename=\"");
						if (startIndex == -1)
							return -1;

						startIndex = startIndex + "filename=\"".Length;

						int endIndex = contDispo.IndexOf("\"", startIndex);
						if (endIndex == -1)
							return -1;

						filename = contDispo.Substring(startIndex, endIndex - startIndex);
					}

					string path = folder + filename;

					using (Stream stream = response.GetResponseStream())
					{
						return WriteStreamToFile(path, stream);
					}
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("(404) Not Found"))
				{
#if DEBUG
					lock (consoleLock)
						Console.WriteLine("Invalid link!");
#endif
					return -1;
				}
#if DEBUG
				lock (consoleLock)
					Console.WriteLine(ex.Message);
#endif
				return -1;
			}
		}

		static bool GetAssetAge(string AssetID, out ulong age)
		{
			age = 0;

			try
			{
				WebRequest req = WebRequest.Create("https://vrcmods.com/item/" + AssetID);

				using (WebResponse response = req.GetResponse())
				{
					string filename = Path.GetFileName(response.ResponseUri.AbsoluteUri);
					using (StreamReader sr = new StreamReader(response.GetResponseStream()))
					{
						string html = sr.ReadToEnd();
						MatchCollection matches = Regex.Matches(html, assetPageRegex);
						foreach (Match match in matches)
						{
							string ageStr = match.Groups[1].Value;
							if (string.IsNullOrWhiteSpace(ageStr) || !ulong.TryParse(ageStr, out age))
								return false;
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
#if DEBUG
				lock (consoleLock)
					Console.WriteLine(ex.Message);
#endif
			}
			return false;
		}
	}
}
