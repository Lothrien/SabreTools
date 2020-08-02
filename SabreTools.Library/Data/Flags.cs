﻿using System;

namespace SabreTools.Library.Data
{
    #region DatFile related

    /// <summary>
    /// DAT output formats
    /// </summary>
    [Flags]
    public enum DatFormat
    {
        #region XML Formats

        /// <summary>
        /// Logiqx XML (using machine)
        /// </summary>
        Logiqx = 1 << 0,

        /// <summary>
        /// Logiqx XML (using game)
        /// </summary>
        LogiqxDeprecated = 1 << 1,

        /// <summary>
        /// MAME Softare List XML
        /// </summary>
        SoftwareList = 1 << 2,

        /// <summary>
        /// MAME Listxml output
        /// </summary>
        Listxml = 1 << 3,

        /// <summary>
        /// OfflineList XML
        /// </summary>
        OfflineList = 1 << 4,

        /// <summary>
        /// SabreDat XML
        /// </summary>
        SabreDat = 1 << 5,

        /// <summary>
        /// openMSX Software List XML
        /// </summary>
        OpenMSX = 1 << 6,

        #endregion

        #region Propietary Formats

        /// <summary>
        /// ClrMamePro custom
        /// </summary>
        ClrMamePro = 1 << 7,

        /// <summary>
        /// RomCetner INI-based
        /// </summary>
        RomCenter = 1 << 8,

        /// <summary>
        /// DOSCenter custom
        /// </summary>
        DOSCenter = 1 << 9,

        /// <summary>
        /// AttractMode custom
        /// </summary>
        AttractMode = 1 << 10,

        #endregion

        #region Standardized Text Formats

        /// <summary>
        /// ClrMamePro missfile
        /// </summary>
        MissFile = 1 << 11,

        /// <summary>
        /// Comma-Separated Values (standardized)
        /// </summary>
        CSV = 1 << 12,

        /// <summary>
        /// Semicolon-Separated Values (standardized)
        /// </summary>
        SSV = 1 << 13,

        /// <summary>
        /// Tab-Separated Values (standardized)
        /// </summary>
        TSV = 1 << 14,

        /// <summary>
        /// MAME Listrom output
        /// </summary>
        Listrom = 1 << 15,

        /// <summary>
        /// Everdrive Packs SMDB
        /// </summary>
        EverdriveSMDB = 1 << 16,

        /// <summary>
        /// JSON
        /// </summary>
        Json = 1 << 17,

        #endregion

        #region SFV-similar Formats

        /// <summary>
        /// CRC32 hash list
        /// </summary>
        RedumpSFV = 1 << 18,

        /// <summary>
        /// MD5 hash list
        /// </summary>
        RedumpMD5 = 1 << 19,

#if NET_FRAMEWORK
        /// <summary>
        /// RIPEMD160 hash list
        /// </summary>
        RedumpRIPEMD160 =       1 << 20,
#endif

        /// <summary>
        /// SHA-1 hash list
        /// </summary>
        RedumpSHA1 = 1 << 21,

        /// <summary>
        /// SHA-256 hash list
        /// </summary>
        RedumpSHA256 = 1 << 22,

        /// <summary>
        /// SHA-384 hash list
        /// </summary>
        RedumpSHA384 = 1 << 23,

        /// <summary>
        /// SHA-512 hash list
        /// </summary>
        RedumpSHA512 = 1 << 24,

        #endregion

        // Specialty combinations
        ALL = Int32.MaxValue,
    }

    /// <summary>
    /// Available hashing types
    /// </summary>
    [Flags]
    public enum Hash
    {
        CRC = 1 << 0,
        MD5 = 1 << 1,
#if NET_FRAMEWORK
        RIPEMD160 = 1 << 2,
#endif
        SHA1 = 1 << 3,
        SHA256 = 1 << 4,
        SHA384 = 1 << 5,
        SHA512 = 1 << 6,

        // Special combinations
        Standard = CRC | MD5 | SHA1,
#if NET_FRAMEWORK
        DeepHashes = RIPEMD160 | SHA256 | SHA384 | SHA512,
        SecureHashes = MD5 | RIPEMD160 | SHA1 | SHA256 | SHA384 | SHA512,
#else
        DeepHashes = SHA256 | SHA384 | SHA512,
        SecureHashes = MD5 | SHA1 | SHA256 | SHA384 | SHA512,
#endif
    }

    /// <summary>
    /// Determine which format to output Stats to
    /// </summary>
    [Flags]
    public enum StatReportFormat
    {
        /// <summary>
        /// Only output to the console
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Console-formatted
        /// </summary>
        Textfile = 1 << 0,

        /// <summary>
        /// ClrMamePro HTML
        /// </summary>
        HTML = 1 << 1,

        /// <summary>
        /// Comma-Separated Values (Standardized)
        /// </summary>
        CSV = 1 << 2,

        /// <summary>
        /// Semicolon-Separated Values (Standardized)
        /// </summary>
        SSV = 1 << 3,

        /// <summary>
        /// Tab-Separated Values (Standardized)
        /// </summary>
        TSV = 1 << 4,

        All = Int32.MaxValue,
    }

    #endregion

    #region DatItem related

    /// <summary>
    /// Determines which type of duplicate a file is
    /// </summary>
    [Flags]
    public enum DupeType
    {
        // Type of match
        Hash = 1 << 0,
        All = 1 << 1,

        // Location of match
        Internal = 1 << 2,
        External = 1 << 3,
    }

    /// <summary>
    /// Determine the status of the item
    /// </summary>
    [Flags]
    public enum ItemStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0x00,

        None = 1 << 0,
        Good = 1 << 1,
        BadDump = 1 << 2,
        Nodump = 1 << 3,
        Verified = 1 << 4,
    }

    /// <summary>
    /// Determine what type of machine it is
    /// </summary>
    [Flags]
    public enum MachineType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0x00,

        None = 1 << 0,
        Bios = 1 << 1,
        Device = 1 << 2,
        Mechanical = 1 << 3,
    }

    #endregion
}
