﻿using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the DATabase application
	/// </summary>
	public partial class SabreTools
	{
		// Private required variables
		private static Logger _logger;

		/// <summary>
		/// Start menu or use supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// Perform initial setup and verification
			_logger = new Logger(true, "sabretools.log");

			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}
			Build.Start("SabreTools");
			DatabaseTools.EnsureDatabase(Constants.HeadererDbSchema, Constants.HeadererFileName, Constants.HeadererConnectionString);

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				_logger.Close();
				return;
			}

			// If there's no arguments, show help
			if (args.Length == 0)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// Set all default values
			bool help = false,

				// Feature flags
				datFromDir = false,
				headerer = false,
				splitByExt = false,
				splitByHash = false,
				splitByType = false,
				stats = false,
				update = false,

				// Other flags
				addBlankFilesForEmptyFolder = false,
				addFileDates = false,
				cleanGameNames = false,
				copyFiles = false,
				datPrefix = false,
				dedup = false,
				enableGzip = false,
				excludeOf = false,
				inplace = false,
				merge = false,
				noMD5 = false,
				noSHA1 = false,
				parseArchivesAsFiles = false,
				quotes = false,
				remext = false,
				removeDateFromAutomaticName = false,
				restore = false,
				romba = false,
				showBaddumpColumn = false,
				showNodumpColumn = false,
				single = false,
				softlist = false,
				superdat = false,
				trim = false,
				skip = false,
				usegame = true;
			DiffMode diffMode = 0x0;
			int maxParallelism = 4;
			long sgt = -1,
				slt = -1,
				seq = -1;
			OutputFormat outputFormat = 0x0;
			StatOutputFormat statOutputFormat = StatOutputFormat.None;
			
			// DAT fields
			string
				author = null,
				category = null,
				comment = null,
				date = null,
				description = null,
				email = null,
				filename = null,
				forcemerge = "",
				forcend = "",
				forcepack = "",
				header = null,
				homepage = null,
				name = null,
				rootdir = null,
				url = null,
				version = null,

				// Filter fields
				crc = "",
				gamename = "",
				md5 = "",
				romname = "",
				romtype = "",
				root = "",
				sha1 = "",
				status = "",

				// Missfile fields
				addext = "",
				postfix = "",
				prefix = "",
				repext = "",

				// Misc fields
				exta = null,
				extb = null,
				outDir = "",

				tempDir = "";
			List<string> inputs = new List<string>();

			// Determine which switches are enabled (with values if necessary)
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-?":
					case "-h":
					case "--help":
						help = true;
						break;
					case "-ab":
					case "--add-blank":
						addBlankFilesForEmptyFolder = true;
						break;
					case "-ad":
					case "--add-date":
						addFileDates = true;
						break;
					case "-ae":
					case "--add-ext":
						i++;
						addext = args[i];
						break;
					case "-au":
					case "--author":
						i++;
						author = args[i];
						break;
					case "-b":
					case "--bare":
						removeDateFromAutomaticName = true;
						break;
					case "-bc":
					case "--baddump-col":
						showBaddumpColumn = true;
						break;
					case "-c":
					case "--cascade":
						diffMode |= DiffMode.Cascade;
						break;
					case "-ca":
					case "--category=":
						i++;
						category = args[i];
						break;
					case "-cf":
					case "--copy-files":
						copyFiles = true;
						break;
					case "-co":
					case "--comment":
						i++;
						comment = args[i];
						break;
					case "-crc":
					case "--crc":
						i++;
						crc = args[i];
						break;
					case "-csv":
					case "--csv":
						statOutputFormat = StatOutputFormat.CSV;
						break;
					case "-clean":
					case "--clean":
						cleanGameNames = true;
						break;
					case "-d":
					case "--d2d":
					case "--dfd":
						datFromDir = true;
						break;
					case "-da":
					case "--date":
						i++;
						date = args[i];
						break;
					case "-dd":
					case "--dedup":
						dedup = true;
						break;
					case "-de":
					case "--desc":
						i++;
						description = args[i];
						break;
					case "-di":
					case "--diff":
						diffMode |= DiffMode.All;
						break;
					case "-did":
					case "--diff-du":
						diffMode |= DiffMode.Dupes;
						break;
					case "-dii":
					case "--diff-in":
						diffMode |= DiffMode.Individuals;
						break;
					case "-din":
					case "--diff-nd":
						diffMode |= DiffMode.NoDupes;
						break;
					case "-em":
					case "--email":
						i++;
						email = args[i];
						break;
					case "-es":
					case "--ext-split":
						splitByExt = true;
						break;
					case "-exta":
					case "--exta":
						i++;
						exta = args[i];
						break;
					case "-extb":
					case "--extb":
						i++;
						extb = args[i];
						break;
					case "-f":
					case "--files":
						parseArchivesAsFiles = true;
						break;
					case "-fi":
					case "--filename":
						i++;
						filename = args[i];
						break;
					case "-fm":
					case "--forcemerge":
						i++;
						forcemerge = args[i];
						break;
					case "-fn":
					case "--forcend":
						i++;
						forcend = args[i];
						break;
					case "-fp":
					case "--forcepack":
						i++;
						forcepack = args[i];
						break;
					case "-gn":
					case "--game-name":
						i++;
						gamename = args[i];
						break;
					case "-gp":
					case "--game-prefix":
						datPrefix = true;
						break;
					case "-gz":
					case "--gz-files":
						enableGzip = true;
						break;
					case "-hd":
					case "--headerer":
						headerer = true;
						break;
					case "-he":
					case "--header":
						i++;
						header = args[i];
						break;
					case "-hp":
					case "--homepage":
						i++;
						homepage = args[i];
						break;
					case "-hs":
					case "--hash-split":
						splitByHash = true;
						break;
					case "-html":
					case "--html":
						statOutputFormat = StatOutputFormat.HTML;
						break;
					case "-input":
					case "--input":
						i++;
						if (File.Exists(args[i]) || Directory.Exists(args[i]))
						{
							inputs.Add(args[i]);
						}
						else
						{
							_logger.Error("Invalid input detected: " + args[i]);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							_logger.Error("Invalid input detected: " + args[i]);
							_logger.Close();
							return;
						}
						break;
					case "-ip":
					case "--inplace":
						inplace = true;
						break;
					case "-is":
					case "--status":
						i++;
						status = args[i];
						break;
					case "-m":
					case "--merge":
						merge = true;
						break;
					case "-md5":
					case "--md5":
						i++;
						md5 = args[i];
						break;
					case "-mt":
					case "--mt":
						i++;
						Int32.TryParse(args[i], out maxParallelism);
						break;
					case "-n":
					case "--name":
						i++;
						name = args[i];
						break;
					case "-nc":
					case "--nodump-col":
						showNodumpColumn = true;
						break;
					case "-nm":
					case "--noMD5":
						noMD5 = true;
						break;
					case "-ns":
					case "--noSHA1":
						noSHA1 = true;
						break;
					case "-oa":
					case "--output-all":
						outputFormat |= OutputFormat.ALL;
						break;
					case "-oc":
					case "--output-cmp":
						outputFormat |= OutputFormat.ClrMamePro;
						break;
					case "-ocsv":
					case "--output-csv":
						outputFormat |= OutputFormat.CSV;
						break;
					case "-od":
					case "--output-dc":
						outputFormat |= OutputFormat.DOSCenter;
						break;
					case "-om":
					case "--output-miss":
						outputFormat |= OutputFormat.MissFile;
						break;
					case "-omd5":
					case "--output-md5":
						outputFormat |= OutputFormat.RedumpMD5;
						break;
					case "-ool":
					case "--output-ol":
						outputFormat |= OutputFormat.OfflineList;
						break;
					case "-or":
					case "--output-rc":
						outputFormat |= OutputFormat.RomCenter;
						break;
					case "-os":
					case "--output-sd":
						outputFormat |= OutputFormat.SabreDat;
						break;
					case "-osfv":
					case "--output-sfv":
						outputFormat |= OutputFormat.RedumpSFV;
						break;
					case "-osha1":
					case "--output-sha1":
						outputFormat |= OutputFormat.RedumpSHA1;
						break;
					case "-osl":
					case "--output-sl":
						outputFormat |= OutputFormat.SoftwareList;
						break;
					case "-otsv":
					case "--output-tsv":
						outputFormat |= OutputFormat.TSV;
						break;
					case "-out":
					case "--out":
						i++;
						outDir = args[i];
						break;
					case "-ox":
					case "--output-xml":
						outputFormat |= OutputFormat.Logiqx;
						break;
					case "-post":
					case "--postfix":
						i++;
						postfix = args[i];
						break;
					case "-pre":
					case "--prefix":
						i++;
						prefix = args[i];
						break;
					case "-q":
					case "--quotes":
						quotes = true;
						break;
					case "-r":
					case "--roms":
						usegame = false;
						break;
					case "-rc":
					case "--rev-cascade":
						diffMode |= DiffMode.ReverseCascade;
						break;
					case "-rd":
					case "--root-dir":
						i++;
						root = args[i];
						break;
					case "-re":
					case "--restore":
						restore = true;
						break;
					case "-rep":
					case "--rep-ext":
						i++;
						repext = args[i];
						break;
					case "-rme":
					case "--rem-ext":
						remext = true;
						break;
					case "-rn":
					case "--rom-name":
						i++;
						romname = args[i];
						break;
					case "-ro":
					case "--romba":
						romba = true;
						break;
					case "-root":
					case "--root":
						i++;
						rootdir = args[i];
						break;
					case "-rt":
					case "--rom-type":
						i++;
						romtype = args[i];
						break;
					case "-sd":
					case "--superdat":
						superdat = true;
						break;
					case "-seq":
					case "--equal":
						i++;
						seq = GetSizeFromString(args[i]);
						break;
					case "-sf":
					case "--skip":
						skip = true;
						break;
					case "-sgt":
					case "--greater":
						i++;
						sgt = GetSizeFromString(args[i]);
						break;
					case "-sha1":
					case "--sha1":
						i++;
						sha1 = args[i];
						break;
					case "-si":
					case "--single":
						single = true;
						break;
					case "-sl":
					case "--softlist":
						softlist = true;
						break;
					case "-slt":
					case "--less":
						i++;
						slt = GetSizeFromString(args[i]);
						break;
					case "-st":
					case "--stats":
						stats = true;
						break;
					case "-t":
					case "--temp":
						i++;
						tempDir = args[i];
						break;
					case "-trim":
					case "--trim":
						trim = true;
						break;
					case "-ts":
					case "--type-split":
						splitByType = true;
						break;
					case "-tsv":
					case "--tsv":
						statOutputFormat = StatOutputFormat.TSV;
						break;
					case "-u":
					case "-url":
					case "--url":
						i++;
						url = args[i];
						break;
					case "-ud":
					case "--update":
						update = true;
						break;
					case "-v":
					case "--version":
						i++;
						version = args[i];
						break;
					case "-xof":
					case "--exclude-of":
						excludeOf = true;
						break;
					default:
						string temparg = args[i].Replace("\"", "").Replace("file://", "");

						if (temparg.StartsWith("-") && temparg.Contains("="))
						{
							// Split the argument
							string[] split = temparg.Split('=');
							if (split[1] == null)
							{
								split[1] = "";
							}

							switch (split[0])
							{
								case "-ae":
								case "--add-ext":
									addext = split[1];
									break;
								case "-au":
								case "--author":
									author = split[1];
									break;
								case "-ca":
								case "--category=":
									category = split[1];
									break;
								case "-co":
								case "--comment":
									comment = split[1];
									break;
								case "-crc":
								case "--crc":
									crc = split[1];
									break;
								case "-da":
								case "--date":
									date = split[1];
									break;
								case "-de":
								case "--desc":
									description = split[1];
									break;
								case "-em":
								case "--email":
									email = split[1];
									break;
								case "-exta":
								case "--exta":
									exta = split[1];
									break;
								case "-extb":
								case "--extb":
									extb = split[1];
									break;
								case "-f":
								case "--filename":
									filename = split[1];
									break;
								case "-fm":
								case "--forcemerge":
									forcemerge = split[1];
									break;
								case "-fn":
								case "--forcend":
									forcend = split[1];
									break;
								case "-fp":
								case "--forcepack":
									forcepack = split[1];
									break;
								case "-gn":
								case "--game-name":
									gamename = split[1];
									break;
								case "-h":
								case "--header":
									header = split[1];
									break;
								case "-hp":
								case "--homepage":
									homepage = split[1];
									break;
								case "-input":
								case "--input":
									if (File.Exists(split[1]) || Directory.Exists(split[1]))
									{
										inputs.Add(split[1]);
									}
									else
									{
										_logger.Error("Invalid input detected: " + args[i]);
										Console.WriteLine();
										Build.Help();
										Console.WriteLine();
										_logger.Error("Invalid input detected: " + args[i]);
										_logger.Close();
										return;
									}
									break;
								case "-is":
								case "--status":
									status = split[1];
									break;
								case "-md5":
								case "--md5":
									md5 = split[1];
									break;
								case "-mt":
								case "--mt":
									Int32.TryParse(split[1], out maxParallelism);
									break;
								case "-n":
								case "--name":
									name = split[1];
									break;
								case "-out":
								case "--out":
									outDir = split[1];
									break;
								case "-post":
								case "--postfix":
									postfix = split[1];
									break;
								case "-pre":
								case "--prefix":
									prefix = split[1];
									break;
								case "-r":
								case "--root":
									rootdir = split[1];
									break;
								case "-rd":
								case "--root-dir":
									root = split[1];
									break;
								case "-re":
								case "--rep-ext":
									repext = split[1];
									break;
								case "-rn":
								case "--rom-name":
									romname = split[1];
									break;
								case "-rt":
								case "--rom-type":
									romtype = split[1];
									break;
								case "-seq":
								case "--equal":
									seq = GetSizeFromString(split[1]);
									break;
								case "-sgt":
								case "--greater":
									sgt = GetSizeFromString(split[1]);
									break;
								case "-sha1":
								case "--sha1":
									sha1 = split[1];
									break;
								case "-slt":
								case "--less":
									slt = GetSizeFromString(split[1]);
									break;
								case "-t":
								case "--temp":
									tempDir = split[1];
									break;
								case "-u":
								case "-url":
								case "--url":
									url = split[1];
									break;
								case "-v":
								case "--version":
									version = split[1];
									break;
								default:
									if (File.Exists(temparg) || Directory.Exists(temparg))
									{
										inputs.Add(temparg);
									}
									else
									{
										_logger.Error("Invalid input detected: " + args[i]);
										Console.WriteLine();
										Build.Help();
										Console.WriteLine();
										_logger.Error("Invalid input detected: " + args[i]);
										_logger.Close();
										return;
									}
									break;
							}
						}
						else if (File.Exists(temparg) || Directory.Exists(temparg))
						{
							inputs.Add(temparg);
						}
						else
						{
							_logger.Error("Invalid input detected: " + args[i]);
							Console.WriteLine();
							Build.Help();
							Console.WriteLine();
							_logger.Error("Invalid input detected: " + args[i]);
							_logger.Close();
							return;
						}
						break;
				}
			}

			// If help is set, show the help screen
			if (help)
			{
				Build.Help();
				_logger.Close();
				return;
			}

			// If more than one switch is enabled, show the help screen
			if (!(datFromDir ^ headerer ^ splitByExt ^ splitByHash ^ splitByType ^ stats ^ update))
			{
				_logger.Error("Only one feature switch is allowed at a time");
				Build.Help();
				_logger.Close();
				return;
			}

			// If a switch that requires a filename is set and no file is, show the help screen
			if (inputs.Count == 0
				&& (datFromDir || headerer || splitByExt || splitByHash || splitByType || stats || update))
			{
				_logger.Error("This feature requires at least one input");
				Build.Help();
				_logger.Close();
				return;
			}

			// Now take care of each mode in succesion

			// Create a DAT from a directory or set of directories
			if (datFromDir)
			{
				InitDatFromDir(inputs,
					filename,
					name,
					description,
					category,
					version,
					author,
					forcepack,
					excludeOf,
					outputFormat,
					romba,
					superdat,
					noMD5,
					noSHA1,
					removeDateFromAutomaticName,
					parseArchivesAsFiles,
					enableGzip,
					addBlankFilesForEmptyFolder,
					addFileDates,
					tempDir,
					outDir,
					copyFiles,
					header,
					maxParallelism);
			}

			// If we're in headerer mode
			else if (headerer)
			{
				InitHeaderer(inputs, restore, outDir);
			}

			// Split a DAT by extension
			else if (splitByExt)
			{
				InitExtSplit(inputs, exta, extb, outDir);
			}

			// Split a DAT by available hashes
			else if (splitByHash)
			{
				InitHashSplit(inputs, outDir);
			}

			// Get statistics on input files
			else if (stats)
			{
				InitStats(inputs, filename, single, showBaddumpColumn, showNodumpColumn, statOutputFormat);
			}

			// Split a DAT by item type
			else if (splitByType)
			{
				InitTypeSplit(inputs, outDir);
			}

			// Convert, update, merge, diff, and filter a DAT or folder of DATs
			else if (update)
			{
				InitUpdate(inputs, filename, name, description, rootdir, category, version, date, author, email, homepage, url, comment, header,
					superdat, forcemerge, forcend, forcepack, excludeOf, outputFormat, usegame, prefix,
					postfix, quotes, repext, addext, remext, datPrefix, romba, merge, diffMode, inplace, skip, removeDateFromAutomaticName, gamename, romname,
					romtype, sgt, slt, seq, crc, md5, sha1, status, trim, single, root, outDir, cleanGameNames, softlist, dedup, maxParallelism);
			}

			// If nothing is set, show the help
			else
			{
				Build.Help();
			}

			_logger.Close();
			return;
		}
	}
}
