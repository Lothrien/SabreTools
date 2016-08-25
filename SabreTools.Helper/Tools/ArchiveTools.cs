﻿using SharpCompress.Archive;
using SharpCompress.Archive.SevenZip;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace SabreTools.Helper
{
	public class ArchiveTools
	{
		/// <summary>
		/// Copy a file to an output archive
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteToArchive(string input, string output, Rom rom)
		{
			string archiveFileName = Path.Combine(output, rom.Game + ".zip");

			System.IO.Compression.ZipArchive outarchive = null;
			try
			{
				if (!File.Exists(archiveFileName))
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
				}
				else
				{
					outarchive = ZipFile.Open(archiveFileName, ZipArchiveMode.Update);
				}

				if (File.Exists(input))
				{
					if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(rom.Name) == null)
					{
						outarchive.CreateEntryFromFile(input, rom.Name, CompressionLevel.Optimal);
					}
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(file) == null)
						{
							outarchive.CreateEntryFromFile(file, file, CompressionLevel.Optimal);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				outarchive?.Dispose();
			}
		}

		/// <summary>
		/// Copy a file to an output archive using SharpCompress
		/// </summary>
		/// <param name="input">Input filename to be moved</param>
		/// <param name="output">Output directory to build to</param>
		/// <param name="rom">RomData representing the new information</param>
		public static void WriteToManagedArchive(string input, string output, Rom rom)
		{
			string archiveFileName = Path.Combine(output, rom.Game + ".zip");

			// Delete an empty file first
			if (File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length == 0)
			{
				File.Delete(archiveFileName);
			}

			// Get if the file should be written out
			bool newfile = File.Exists(archiveFileName) && new FileInfo(archiveFileName).Length != 0;

			using (SharpCompress.Archive.Zip.ZipArchive archive = (newfile
				? ArchiveFactory.Open(archiveFileName, Options.LookForHeader) as SharpCompress.Archive.Zip.ZipArchive
				: ArchiveFactory.Create(ArchiveType.Zip) as SharpCompress.Archive.Zip.ZipArchive))
			{
				try
				{
					if (File.Exists(input))
					{
						archive.AddEntry(rom.Name, input);
					}
					else if (Directory.Exists(input))
					{
						archive.AddAllFromDirectory(input, "*", SearchOption.AllDirectories);
					}

					archive.SaveTo(archiveFileName + ".tmp", CompressionType.Deflate);
				}
				catch (Exception)
				{
					// Don't log archive write errors
				}
			}

			if (File.Exists(archiveFileName + ".tmp"))
			{
				File.Delete(archiveFileName);
				File.Move(archiveFileName + ".tmp", archiveFileName);
			}
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, Logger logger)
		{
			return ExtractArchive(input, tempdir, ArchiveScanLevel.Both, ArchiveScanLevel.External, ArchiveScanLevel.External, ArchiveScanLevel.Both, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, int sevenzip,
			int gz, int rar, int zip, Logger logger)
		{
			return ExtractArchive(input, tempdir, (ArchiveScanLevel)sevenzip, (ArchiveScanLevel)gz, (ArchiveScanLevel)rar, (ArchiveScanLevel)zip, logger);
		}

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="input">Name of the file to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="sevenzip">Archive handling level for 7z</param>
		/// <param name="gz">Archive handling level for GZip</param>
		/// <param name="rar">Archive handling level for RAR</param>
		/// <param name="zip">Archive handling level for Zip</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public static bool ExtractArchive(string input, string tempdir, ArchiveScanLevel sevenzip,
			ArchiveScanLevel gz, ArchiveScanLevel rar, ArchiveScanLevel zip, Logger logger)
		{
			bool encounteredErrors = true;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return encounteredErrors;
			}

			IReader reader = null;
			SevenZipArchive sza = null;
			try
			{
				if (at == ArchiveType.SevenZip && sevenzip != ArchiveScanLevel.External)
				{
					sza = SevenZipArchive.Open(File.OpenRead(input));
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					// Extract all files to the temp directory
					sza.WriteToDirectory(tempdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
					encounteredErrors = false;
				}
				else if (at == ArchiveType.GZip && gz != ArchiveScanLevel.External)
				{
					logger.Log("Found archive of type: " + at);

					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					using (FileStream itemstream = File.OpenRead(input))
					{
						using (FileStream outstream = File.Create(tempdir + Path.GetFileNameWithoutExtension(input)))
						{
							using (GZipStream gzstream = new GZipStream(itemstream, CompressionMode.Decompress))
							{
								gzstream.CopyTo(outstream);
							}
						}
					}
					encounteredErrors = false;
				}
				else
				{
					reader = ReaderFactory.Open(File.OpenRead(input));
					logger.Log("Found archive of type: " + at);

					if ((at == ArchiveType.Zip && zip != ArchiveScanLevel.External) ||
						(at == ArchiveType.Rar && rar != ArchiveScanLevel.External))
					{
						// Create the temp directory
						Directory.CreateDirectory(tempdir);

						// Extract all files to the temp directory
						reader.WriteAllToDirectory(tempdir, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
						encounteredErrors = false;
					}
				}
			}
			catch (EndOfStreamException)
			{
				// Catch this but don't count it as an error because SharpCompress is unsafe
			}
			catch (InvalidOperationException)
			{
				encounteredErrors = true;
			}
			catch (Exception)
			{
				// Don't log file open errors
				encounteredErrors = true;
			}
			finally
			{
				reader?.Dispose();
				sza?.Dispose();
			}

			return encounteredErrors;
		}

		/// <summary>
		/// Attempt to extract a file from an archive
		/// </summary>
		/// <param name="input">Name of the archive to be extracted</param>
		/// <param name="entryname">Name of the entry to be extracted</param>
		/// <param name="tempdir">Temporary directory for archive extraction</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Name of the extracted file, null on error</returns>
		public static string ExtractSingleItemFromArchive(string input, string entryname,
			string tempdir, Logger logger)
		{
			string outfile = null;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return outfile;
			}

			IReader reader = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(input));

				if (at == ArchiveType.Zip || at == ArchiveType.SevenZip || at == ArchiveType.Rar)
				{
					// Create the temp directory
					Directory.CreateDirectory(tempdir);

					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(entryname))
						{
							outfile = Path.GetFullPath(Path.Combine(tempdir, reader.Entry.Key));
							if (!Directory.Exists(Path.GetDirectoryName(outfile)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(outfile));
							}
							reader.WriteEntryToFile(outfile, ExtractOptions.Overwrite);
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				outfile = null;
			}
			finally
			{
				reader?.Dispose();
			}

			return outfile;
		}

		/// <summary>
		/// Attempt to copy a file between archives
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outputArchive">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenArchives(string inputArchive, string outputArchive,
			string sourceEntryName, string destEntryName, Logger logger)
		{
			bool success = false;

			// First get the archive types
			ArchiveType? iat = GetCurrentArchiveType(inputArchive, logger);
			ArchiveType? oat = (File.Exists(outputArchive) ? GetCurrentArchiveType(outputArchive, logger) : ArchiveType.Zip);

			// If we got back null (or the output is not a Zipfile), then it's not an archive, so we we return
			if (iat == null || (oat == null || oat != ArchiveType.Zip) || inputArchive == outputArchive)
			{
				return success;
			}

			IReader reader = null;
			System.IO.Compression.ZipArchive outarchive = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(inputArchive));

				if (iat == ArchiveType.Zip || iat == ArchiveType.SevenZip || iat == ArchiveType.Rar)
				{
					while (reader.MoveToNextEntry())
					{
						logger.Log("Current entry name: '" + reader.Entry.Key + "'");
						if (reader.Entry != null && reader.Entry.Key.Contains(sourceEntryName))
						{
							if (!File.Exists(outputArchive))
							{
								outarchive = ZipFile.Open(outputArchive, ZipArchiveMode.Create);
							}
							else
							{
								outarchive = ZipFile.Open(outputArchive, ZipArchiveMode.Update);
							}

							if (outarchive.Mode == ZipArchiveMode.Create || outarchive.GetEntry(destEntryName) == null)
							{
								System.IO.Compression.ZipArchiveEntry iae = outarchive.CreateEntry(destEntryName, CompressionLevel.Optimal) as System.IO.Compression.ZipArchiveEntry;

								using (Stream iaestream = iae.Open())
								{
									reader.WriteEntryTo(iaestream);
								}
							}
							success = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				success = false;
			}
			finally
			{
				reader?.Dispose();
				outarchive?.Dispose();
			}

			return success;
		}

		/// <summary>
		/// Attempt to copy a file between archives using SharpCompress
		/// </summary>
		/// <param name="inputArchive">Source archive name</param>
		/// <param name="outputArchive">Destination archive name</param>
		/// <param name="sourceEntryName">Input entry name</param>
		/// <param name="destEntryName">Output entry name</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the copy was a success, false otherwise</returns>
		public static bool CopyFileBetweenManagedArchives(string inputArchive, string outputArchive,
			string sourceEntryName, string destEntryName, Logger logger)
		{
			bool success = false;

			// First get the archive types
			ArchiveType? iat = GetCurrentArchiveType(inputArchive, logger);
			ArchiveType? oat = (File.Exists(outputArchive) ? GetCurrentArchiveType(outputArchive, logger) : ArchiveType.Zip);

			// If we got back null (or the output is not a Zipfile), then it's not an archive, so we we return
			if (iat == null || (oat == null || oat != ArchiveType.Zip) || inputArchive == outputArchive)
			{
				return success;
			}

			try
			{
				using (IReader reader = ReaderFactory.Open(File.OpenRead(inputArchive)))
				{
					if (iat == ArchiveType.Zip || iat == ArchiveType.SevenZip || iat == ArchiveType.Rar)
					{
						while (reader.MoveToNextEntry())
						{
							logger.Log("Current entry name: '" + reader.Entry.Key + "'");
							if (reader.Entry != null && reader.Entry.Key.Contains(sourceEntryName))
							{
								// Get if the file should be written out
								bool newfile = File.Exists(outputArchive) && new FileInfo(outputArchive).Length != 0;

								using (SharpCompress.Archive.Zip.ZipArchive archive = (newfile
									? ArchiveFactory.Open(outputArchive, Options.LookForHeader) as SharpCompress.Archive.Zip.ZipArchive
									: ArchiveFactory.Create(ArchiveType.Zip) as SharpCompress.Archive.Zip.ZipArchive))
								{
									try
									{
										Stream tempstream = new MemoryStream();
										reader.WriteEntryTo(tempstream);
										archive.AddEntry(destEntryName, tempstream);

										archive.SaveTo(outputArchive + ".tmp", CompressionType.Deflate);
									}
									catch (Exception)
									{
										// Don't log archive write errors
									}
								}

								if (File.Exists(outputArchive + ".tmp"))
								{
									File.Delete(outputArchive);
									File.Move(outputArchive + ".tmp", outputArchive);
								}

								success = true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
				success = false;
			}

			return success;
		}

		/// <summary>
		/// Generate a list of RomData objects from the header values in an archive
		/// </summary>
		/// <param name="input">Input file to get data from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>List of RomData objects representing the found data</returns>
		public static List<Rom> GetArchiveFileInfo(string input, Logger logger)
		{
			List<Rom> roms = new List<Rom>();
			string gamename = Path.GetFileNameWithoutExtension(input);

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input, logger);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return roms;
			}

			IReader reader = null;
			try
			{
				reader = ReaderFactory.Open(File.OpenRead(input));
				logger.Log("Found archive of type: " + at);

				if (at != ArchiveType.Tar)
				{
					while (reader.MoveToNextEntry())
					{
						if (reader.Entry != null && !reader.Entry.IsDirectory)
						{
							logger.Log("Entry found: '" + reader.Entry.Key + "': " + reader.Entry.Size + ", " + reader.Entry.Crc.ToString("X").ToLowerInvariant());

							roms.Add(new Rom
							{
								Type = "rom",
								Name = reader.Entry.Key,
								Game = gamename,
								Size = reader.Entry.Size,
								CRC = reader.Entry.Crc.ToString("X").ToLowerInvariant(),
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}
			finally
			{
				reader?.Dispose();
			}

			return roms;
		}

		/* (Torrent)GZ Header Format
			(https://tools.ietf.org/html/rfc1952)
			00			Identification 1 (0x1F)
			01			Identification 2 (0x8B)
			02			Compression Method (0-7 reserved, 8 deflate; 0x08)
			03			Flags (0 FTEXT, 1 FHCRC, 2 FEXTRA, 3 FNAME, 4 FCOMMENT, 5 reserved, 6 reserved, 7 reserved; 0x04)
			04-07		Modification time (Unix format; 0x00, 0x00, 0x00, 0x00)
			08			Extra Flags (2 maximum compression, 4 fastest algorithm; 0x00)
			09			OS (See list on https://tools.ietf.org/html/rfc1952; 0x00)
			0A-0B		[if FEXTRA set] Length of extra field (mirrored; 0x1C, 0x00)
			0C-ww		[if FEXTRA set] Extra field
				0C-1B	MD5 Hash
				1C-1F	CRC hash
				20-27	Int64 size (mirrored)
			ww+1-xx		[if FNAME set] Original filename, 00 terminated
			xx+1-yy		[if FCOMMENT set] File comment, 00 terminated
			yy+1-yy+03	[if FHCRC set] CRC16
			yy+04-zz	Compressed blocks
			zz+1-zz+5	CRC32
			zz+6-zz+10	Size (< 4GiB, mirrored)
		*/

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>Populated RomData object if success, empty one on error</returns>
		public static Rom GetTorrentGZFileInfo(string input, Logger logger)
		{
			string datum = Path.GetFileName(input).ToLowerInvariant();
			long filesize = new FileInfo(input).Length;
 
			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{40}\.gz"))
			{
				logger.Warning("Non SHA-1 filename found, skipping: '" + datum + "'");
				return new Rom();
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				logger.Warning("Possibly corrupt file '" + input + "' with size " + Style.GetBytesReadable(filesize));
				return new Rom();
			}

			// Get the Romba-specific header data
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			byte[] headersz; // Int64 size
			using (FileStream itemstream = File.OpenRead(input))
			{
				using (BinaryReader br = new BinaryReader(itemstream))
				{
					br.BaseStream.Seek(12, SeekOrigin.Begin);
					headermd5 = br.ReadBytes(16);
					headercrc = br.ReadBytes(4);
					headersz = br.ReadBytes(8);
				}
			}

			// Now convert the data and get the right position
			string gzmd5 = BitConverter.ToString(headermd5).Replace("-", string.Empty);
			string gzcrc = BitConverter.ToString(headercrc).Replace("-", string.Empty);
			string gzsize = BitConverter.ToString(headersz.Reverse().ToArray()).Replace("-", string.Empty);
			long extractedsize = Convert.ToInt64(gzsize, 16);

			Rom rom = new Rom
			{
				Type = "rom",
				Game = Path.GetFileNameWithoutExtension(input),
				Name = Path.GetFileNameWithoutExtension(input),
				Size = extractedsize,
				CRC = gzcrc,
				MD5 = gzmd5,
				SHA1 = Path.GetFileNameWithoutExtension(input),
			};

			return rom;
		}

		/// <summary>
		/// Write an input file to a torrent GZ file
		/// </summary>
		/// <param name="input">File to write from</param>
		/// <param name="outdir">Directory to write archive to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		public static bool WriteTorrentGZ(string input, string outdir, bool romba, Logger logger)
		{
			// Check that the input file exists
			if (!File.Exists(input))
			{
				logger.Warning("File " + input + " does not exist!");
				return false;
			}
			input = Path.GetFullPath(input);

			// Make sure the output directory exists
			if (!Directory.Exists(outdir))
			{
				Directory.CreateDirectory(outdir);
			}
			outdir = Path.GetFullPath(outdir);

			// Now get the Rom info for the file so we have hashes and size
			Rom rom = RomTools.GetSingleFileInfo(input);

			// Rename the input file based on the SHA-1
			string tempname = Path.Combine(Path.GetDirectoryName(input), rom.SHA1);
			try
			{
				File.Move(input, tempname);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}

			// If it doesn't exist, create the output file and then write
			string outfile = Path.Combine(outdir, rom.SHA1 + ".gz");
			using (FileStream inputstream = new FileStream(tempname, FileMode.Open))
			using (GZipStream output = new GZipStream(File.Open(outfile, FileMode.Create, FileAccess.Write), CompressionMode.Compress))
			{
				inputstream.CopyTo(output);
			}

			// Name the original input file correctly again
			try
			{
				File.Move(tempname, input);
			}
			catch (Exception ex)
			{
				logger.Error(ex.ToString());
			}

			// Now that it's renamed, inject the header info
			using (BinaryWriter sw = new BinaryWriter(new MemoryStream()))
			{
				using (BinaryReader br = new BinaryReader(File.OpenRead(outfile)))
				{
					// Copy the first part of the file to the memory stream
					sw.Write(br.ReadBytes(3)); //0x1F (ID1), 0x8B (ID2), 0x08 (Compression method - Deflate)

					// Now write TGZ info
					byte[] data = new byte[] { 0x4, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1c, 0x0 } // Flags with FEXTRA set (1), MTIME (4), XFLAG (1), OS (1), FEXTRA-LEN (2, Mirrored)
									.Concat(StringToByteArray(rom.MD5)) // MD5
									.Concat(StringToByteArray(rom.CRC)) // CRC
									.Concat(BitConverter.GetBytes(rom.Size).Reverse().ToArray()) // Long size (Mirrored)
								.ToArray();
					sw.Write(data);

					// Finally, copy the rest of the data from the original file
					br.BaseStream.Seek(10, SeekOrigin.Begin);
					int i = 10;
					while (br.BaseStream.Position < br.BaseStream.Length)
					{
						sw.Write(br.ReadByte());
						i++;
					}
				}

				using (BinaryWriter bw = new BinaryWriter(File.Open(outfile, FileMode.Create)))
				{
					// Now write the new file over the original
					sw.BaseStream.Seek(0, SeekOrigin.Begin);
					bw.BaseStream.Seek(0, SeekOrigin.Begin);
					sw.BaseStream.CopyTo(bw.BaseStream);
				}
			}

			// If we're in romba mode, create the subfolder and move the file
			if (romba)
			{
				string subfolder = Path.Combine(rom.SHA1.Substring(0, 2), rom.SHA1.Substring(2, 2), rom.SHA1.Substring(4, 2), rom.SHA1.Substring(6, 2));
				outdir = Path.Combine(outdir, subfolder);
				if (!Directory.Exists(outdir))
				{
					Directory.CreateDirectory(outdir);
				}

				try
				{
					File.Move(outfile, Path.Combine(outdir, Path.GetFileName(outfile)));
				}
				catch (Exception ex)
				{
					logger.Error(ex.ToString());
					File.Delete(outfile);
				}
			}

			return true;
		}

		/// <summary>
		/// Returns the archive type of an input file
		/// </summary>
		/// <param name="input">Input file to check</param>
		/// <param name="logger">Logger object for file and console output</param>
		/// <returns>ArchiveType of inputted file (null on error)</returns>
		public static ArchiveType? GetCurrentArchiveType(string input, Logger logger)
		{
			ArchiveType? outtype = null;

			// First line of defense is going to be the extension, for better or worse
			string ext = Path.GetExtension(input).ToLowerInvariant();
			if (ext != ".7z" && ext != ".gz" && ext != ".lzma" && ext != ".rar"
				&& ext != ".rev" && ext != ".r00" && ext != ".r01" && ext != ".tar"
				&& ext != ".tgz" && ext != ".tlz" && ext != ".zip" && ext != ".zipx")
			{
				return outtype;
			}

			// Read the first bytes of the file and get the magic number
			try
			{
				byte[] magic = new byte[8];
				using (BinaryReader br = new BinaryReader(File.OpenRead(input)))
				{
					magic = br.ReadBytes(8);
				}

				// Convert it to an uppercase string
				string mstr = string.Empty;
				for (int i = 0; i < magic.Length; i++)
				{
					mstr += BitConverter.ToString(new byte[] { magic[i] });
				}
				mstr = mstr.ToUpperInvariant();

				// Now try to match it to a known signature
				if (mstr.StartsWith(Constants.SevenZipSig))
				{
					outtype = ArchiveType.SevenZip;
				}
				else if (mstr.StartsWith(Constants.GzSig))
				{
					outtype = ArchiveType.GZip;
				}
				else if (mstr.StartsWith(Constants.RarSig) || mstr.StartsWith(Constants.RarFiveSig))
				{
					outtype = ArchiveType.Rar;
				}
				else if (mstr.StartsWith(Constants.TarSig) || mstr.StartsWith(Constants.TarZeroSig))
				{
					outtype = ArchiveType.Tar;
				}
				else if (mstr.StartsWith(Constants.ZipSig) || mstr.StartsWith(Constants.ZipSigEmpty) || mstr.StartsWith(Constants.ZipSigSpanned))
				{
					outtype = ArchiveType.Zip;
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
			}

			return outtype;
		}

		/// <summary>
		/// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
		/// </summary>
		public static byte[] StringToByteArray(String hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}
	}
}
