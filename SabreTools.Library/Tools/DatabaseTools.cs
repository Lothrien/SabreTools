﻿using System;
using System.IO;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Logging;
using Microsoft.Data.Sqlite;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// All general database operations
    /// </summary>
    public static class DatabaseTools
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Add a header to the database
        /// </summary>
        /// <param name="header">String representing the header bytes</param>
        /// <param name="SHA1">SHA-1 of the deheadered file</param>
        /// <param name="type">Name of the source skipper file</param>
        public static void AddHeaderToDatabase(string header, string SHA1, string source)
        {
            // Ensure the database exists
            EnsureDatabase(Constants.HeadererFileName, Constants.HeadererConnectionString);

            // Open the database connection
            SqliteConnection dbc = new SqliteConnection(Constants.HeadererConnectionString);
            dbc.Open();

            string query = $"SELECT * FROM data WHERE sha1='{SHA1}' AND header='{header}'";
            SqliteCommand slc = new SqliteCommand(query, dbc);
            SqliteDataReader sldr = slc.ExecuteReader();
            bool exists = sldr.HasRows;

            if (!exists)
            {
                query = $"INSERT INTO data (sha1, header, type) VALUES ('{SHA1}', '{header}', '{source}')";
                slc = new SqliteCommand(query, dbc);
                logger.Verbose($"Result of inserting header: {slc.ExecuteNonQuery()}");
            }

            // Dispose of database objects
            slc.Dispose();
            sldr.Dispose();
            dbc.Dispose();
        }

        /// <summary>
        /// Ensure that the databse exists and has the proper schema
        /// </summary>
        /// <param name="db">Name of the databse</param>
        /// <param name="connectionString">Connection string for SQLite</param>
        public static void EnsureDatabase(string db, string connectionString)
        {
            // Make sure the file exists
            if (!File.Exists(db))
                File.Create(db);

            // Open the database connection
            SqliteConnection dbc = new SqliteConnection(connectionString);
            dbc.Open();

            // Make sure the database has the correct schema
            try
            {
                string query = @"
CREATE TABLE IF NOT EXISTS data (
    'sha1'		TEXT		NOT NULL,
    'header'	TEXT		NOT NULL,
    'type'		TEXT		NOT NULL,
    PRIMARY KEY (sha1, header, type)
)";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                slc.ExecuteNonQuery();
                slc.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                dbc.Dispose();
            }
        }

        /// <summary>
        /// Retrieve headers from the database
        /// </summary>
        /// <param name="SHA1">SHA-1 of the deheadered file</param>
        /// <returns>List of strings representing the headers to add</returns>
        public static List<string> RetrieveHeadersFromDatabase(string SHA1)
        {
            // Ensure the database exists
            EnsureDatabase(Constants.HeadererFileName, Constants.HeadererConnectionString);

            // Open the database connection
            SqliteConnection dbc = new SqliteConnection(Constants.HeadererConnectionString);
            dbc.Open();

            // Create the output list of headers
            List<string> headers = new List<string>();

            string query = $"SELECT header, type FROM data WHERE sha1='{SHA1}'";
            SqliteCommand slc = new SqliteCommand(query, dbc);
            SqliteDataReader sldr = slc.ExecuteReader();

            if (sldr.HasRows)
            {
                while (sldr.Read())
                {
                    logger.Verbose($"Found match with rom type '{sldr.GetString(1)}'");
                    headers.Add(sldr.GetString(0));
                }
            }
            else
            {
                logger.Warning("No matching header could be found!");
            }

            // Dispose of database objects
            slc.Dispose();
            sldr.Dispose();
            dbc.Dispose();

            return headers;
        }
    }
}
