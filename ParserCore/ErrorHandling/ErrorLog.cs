using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;

namespace WaywardGamers.KParser
{
	/// <summary>
	/// Error logging class.
	/// </summary>
	public class Logger
	{
		#region Static Singleton Members
		private static Logger instance;
		public static Logger Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Logger();
				}

				return instance;
			}
		}
		#endregion

		#region Member Variables
		private string logFileName;
		private string breakString;

        private Properties.Settings programSettings;
		#endregion

		#region Constructor
		/// <summary>
		/// Construct a new instance of the Logger.
		/// </summary>
		private Logger()
		{
            programSettings = new WaywardGamers.KParser.Properties.Settings();
            FileInfo assemInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            logFileName = Path.Combine(assemInfo.DirectoryName, "error.log");
			breakString = "---------------------------------------------------------------";

            TrimLogFile();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The path and filename of the file to save log information to.
		/// </summary>
		public string LogFileName
		{
			get
			{
				return logFileName;
			}
			set
			{
				logFileName = value;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Log arbitrary text to the log file, primarily for debugging purposes.
		/// </summary>
		/// <param name="label">The text to go before the message to give
		/// it a distinguishable label.</param>
		/// <param name="message">The text to be logged.</param>
		public void Log(string label, string message)
		{
			this.Log(label, message, ErrorLevel.Info);
		}

		/// <summary>
		/// Log arbitrary text to the log file, primarily for debugging purposes.
		/// </summary>
		/// <param name="label">The text to go before the message to give
		/// it a distinguishable label.</param>
		/// <param name="message">The text to be logged.</param>
		public void Log(string label, string message, ErrorLevel severity)
		{
            programSettings.Reload();

			// If error logging is turned off, just return.
            if (programSettings.ErrorLoggingLevel == ErrorLevel.None)
				return;

            if (severity >= programSettings.ErrorLoggingLevel)
			{
				try
				{
                    using (StreamWriter sw = File.AppendText(logFileName))
                    {
                        WriteHeader(sw, label, message, severity);
                        WriteFooter(sw);
                    }
                }
				catch (Exception)
				{
					MessageBox.Show("Error writing log.");
				}
			}
		}

        /// <summary>
		/// Shortcut versions for the call to log exceptions.
		/// </summary>
		/// <param name="e">The exception to be logged.</param>
		public void Log(Exception e)
		{
			Log(e, "");
		}

		/// <summary>
		/// Log any other exceptions.
		/// </summary>
		/// <param name="e">The exception to be logged.</param>
		/// <param name="message">Optional message that can be passed
		/// in at the time the exception is caught.</param>
		public void Log(Exception e, string message)
		{
			try
			{
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    WriteHeader(sw, e.GetType().ToString(), message, ErrorLevel.Error);
                    sw.Write(e.ToString());
                    sw.WriteLine();
                    WriteFooter(sw);
                }
			}
			catch (Exception)
			{
				MessageBox.Show("Error writing log.");
			}
		}

		/// <summary>
		/// Log any exceptions that cause the program to terminate.
		/// </summary>
		/// <param name="e">The exception to be logged.</param>
		public void FatalLog(Exception e)
		{
			try
			{
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    sw.WriteLine("******* FATAL EXCEPTION !!!! *********\n");
                    WriteHeader(sw, e.GetType().ToString(), "", ErrorLevel.Error);
                    sw.Write(e.ToString());
                    sw.WriteLine();
                    WriteFooter(sw);
                }
			}
			catch (Exception)
			{
				MessageBox.Show("FATAL EXCEPTION (unable to write log file):\n" + e.Message);
			}
		}

		/// <summary>
		/// Trim excess log file data from the log to prevent unbounded growth.
		/// </summary>
		public void TrimLogFile()
		{
            programSettings.Reload();

			Queue logQueue = new Queue();
			Array logArray;
			StreamReader sr;
			DateTime timestamp;
			int findMarker;
			TimeSpan timeSpan;
            int keepFromLine;

			if (File.Exists(logFileName) == true)
			{
				try
				{
					using (sr = new StreamReader(logFileName)) 
					{
						string line;
						// Read lines from file and place in queue for processing.
						while ((line = sr.ReadLine()) != null) 
						{
							logQueue.Enqueue(line);
						}
					}

					// If no lines in file, quit.
					if (logQueue.Count == 0)
						return;

					// Copy to array for processing
					logArray = logQueue.ToArray();
                    keepFromLine = 0;
					
					for (int i = 0; i < logArray.Length; i++)
					{
                        if (DateTime.TryParse(logArray.GetValue(i).ToString(), out timestamp) == true)
                        {
 							timeSpan = DateTime.Now - timestamp;

							// Determine if the log entry is outside our retention period.
                            if (timeSpan > TimeSpan.FromDays(programSettings.DaysToRetainErrorLogs))
							{
								findMarker = Array.IndexOf(logArray, breakString, i);

								// If we can't find another marker, we've reached the end of the file.
								if (findMarker < 0)
								{
									i = logArray.Length;
									break;
								}

								// Update loop position
                                keepFromLine = findMarker + 1;
                                i = findMarker;
							}
							else
							{
								// End loop when we find a log inside the time limit
								break;
							}
                        }
					}

                    using (StreamWriter sw = File.CreateText(logFileName))
                    {
                        for (int j = keepFromLine; j < logArray.Length; j++)
                        {
                            sw.WriteLine(logArray.GetValue(j).ToString());
                        }
                    }
				}
				catch (Exception)
				{
				}
			}
		}
		#endregion

		#region Private Methods
        /// <summary>
        /// Write the preliminary log text, including timestamp and version info.
        /// </summary>
        /// <param name="sw">The stream to write the log to.</param>
        /// <param name="title">Title associated with log.</param>
        /// <param name="message">Message to be included with log.</param>
        /// <param name="severity">The severity of the error being logged.</param>
        private void WriteHeader(StreamWriter sw, string label, string message, ErrorLevel severity)
        {
            int count = 0;

            sw.WriteLine("{0:f}", DateTime.Now);
            sw.WriteLine(Assembly.GetCallingAssembly().FullName);
            sw.WriteLine("Error severity level: {0}", severity.ToString());
            sw.WriteLine();

            if ((label != null) && (label != ""))
            {
                sw.WriteLine(label);
                count++;
            }

            if ((message != null) && (message != ""))
            {
                sw.WriteLine(string.Format("Message: {0}", message));
                count++;
            }

            if (count > 0)
                sw.WriteLine();
        }

        /// <summary>
        /// Write the footer for the log to divide log entries.
        /// </summary>
        /// <param name="sw">The stream to write the log to.</param>
        private void WriteFooter(StreamWriter sw)
        {
            sw.WriteLine(breakString);
            sw.WriteLine();
        }
		#endregion
	}
}