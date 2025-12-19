using System;
using System.IO;
using System.Threading;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace PhantasmaPhoenix.Unity.Core.Logging
{
    public static class Log
    {
        public enum Level
        {
            Disabled,
            Logic,
            Networking,
            Debug1,
            Debug2,
            Debug3
        }

        public enum UnityDebugLogMode
        {
            Normal,
            Warning,
            Error
        }

        public static bool IsInitialized => LogStreamWriter != null;
        public static string FilePath;
        private static FileStream LogFileStream;
        private static StreamWriter LogStreamWriter;
        private static Level MaxLevel = Level.Networking;
        private static bool OverwriteOldContent = false;
        private static bool CompactMode = false;
        private static bool ConsoleOutput = false;
        private static bool UtcTimestamp = true;
        private static bool MultilinePadding = true;
        private static bool AddTid = false;
        // Thread-local guard so Unity log hooks can detect when Log.Write is emitting to Unity Debug,
        // avoiding re-logging and duplicate entries while keeping external Unity logs intact.
        [ThreadStatic] private static int WriteDepth;

        public static bool IsWriting => WriteDepth > 0;

        private static object Locker = new object();

        private static bool OpenLogFileStream(string filePath, FileMode fileMode)
        {
            try
            {
                LogFileStream = File.Open(filePath, fileMode, FileAccess.Write, FileShare.Read);
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }

        public static void Init(string fileName, Level maxLevel, bool forceWorkingFolderUsage = false, bool overwriteOldContent = false, bool addTid = false)
        {
            if (IsInitialized)
                return;

            MaxLevel = maxLevel;

            OverwriteOldContent = overwriteOldContent;

            AddTid = addTid;

#if UNITY_5_3_OR_NEWER
            FilePath = Application.persistentDataPath;
            if (forceWorkingFolderUsage)
            {
#endif
            FilePath = Path.GetFullPath(".");
#if UNITY_5_3_OR_NEWER
            }
#endif

            // Opening log stream.
            FileMode _fileMode = FileMode.Append;

            if (OverwriteOldContent)
            {
                _fileMode = FileMode.Create;
                OverwriteOldContent = false;
            }

            string filePath = "";
            for(var i = 0; i < 1000; i++)
            {
                if (i == 0)
                {
                    filePath = Path.Combine(FilePath, fileName);
                }
                else
                {
                    var extension = Path.GetExtension(fileName);
                    filePath = Path.Combine(FilePath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName) + "-" + i, extension));
                }
                
                if (OpenLogFileStream(filePath, _fileMode))
                    break;
            }
            FilePath = filePath;

            LogStreamWriter = new StreamWriter(LogFileStream);
        }

        public static void SwitchToCompactMode(bool compactMode)
        {
            CompactMode = compactMode;
        }

        public static void SwitchConsoleOutput(bool consoleOutput)
        {
            ConsoleOutput = consoleOutput;
        }

        public static void SwitchUtcTimestamp(bool utcTimestamp)
        {
            UtcTimestamp = utcTimestamp;
        }

        public static void DisableMultilinePadding(bool disableMultilinePadding)
        {
            MultilinePadding = !disableMultilinePadding;
        }

        // WriteWarning() and WriteError() are two Write() wrappers,
        // corresponding to Unity Debug.LogWarning() and Debug.LogError().
        // They are made for better visibility in code.
        public static void WriteWarning(string message, Level level = Level.Logic)
        {
            Write(message, level, UnityDebugLogMode.Warning);
        }

        public static void WriteFatalError(string message, Level level = Level.Logic)
        {
            Write(message, level, UnityDebugLogMode.Error);
        }

        public static void Write(string message, Level level = Level.Logic, UnityDebugLogMode unityDebugLogMode = UnityDebugLogMode.Normal)
        {
            WriteDepth++;
            try
            {
                if (IsInitialized && MaxLevel != Level.Disabled && level <= MaxLevel)
                {
                    DateTime _now = DateTime.Now;

                    string _additional_padding = "";
                    switch (level)
                    {
                        case Level.Logic:
                            break;
                        case Level.Networking:
                            _additional_padding = "<-> ";
                            break;
                        case Level.Debug1:
                            _additional_padding = new String(' ', 8);
                            break;
                        case Level.Debug2:
                            _additional_padding = new String(' ', 12);
                            break;
                        case Level.Debug3:
                            _additional_padding = new String(' ', 16);
                            break;
                    }

                    // Prefix length: 28 symbols.
                    string _timestamp_prefix_local = "[" + _now.ToString("yyyy.MM.dd HH:mm:ss:ffff") + ((AddTid && Thread.CurrentThread.ManagedThreadId != 1) ? " tid: " + Thread.CurrentThread.ManagedThreadId.ToString() : "") + "]: " + _additional_padding;
                    string _empty_prefix = new String(' ', 28) + _additional_padding;
                    string _timestamp_prefix_utc = UtcTimestamp ? "[" + _now.ToUniversalTime().ToString("yyyy.MM.dd HH:mm") + " UTC    ]  " + _additional_padding : _empty_prefix;

                    string preparedMessage = "";
                    int _line_count = 0;
                    foreach (var _line in message.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        _line_count++;

                        switch (_line_count)
                        {
                            case 1:
                                preparedMessage += _timestamp_prefix_local + _line + Environment.NewLine;
                                break;
                            case 2:
                                preparedMessage += (MultilinePadding ? _timestamp_prefix_utc : "") + _line + Environment.NewLine;
                                break;
                            default:
                                preparedMessage += (MultilinePadding ? _empty_prefix : "") + _line + Environment.NewLine;
                                break;
                        }
                    }

                    if (!CompactMode)
                    {
                        if (_line_count < 2 && UtcTimestamp)
                        {
                            preparedMessage += _timestamp_prefix_utc + Environment.NewLine;
                        }

                        preparedMessage += "" + Environment.NewLine;
                    }

                    lock (Locker)
                    {
                        if (ConsoleOutput)
                            Console.Write(preparedMessage);
                        LogStreamWriter.Write(preparedMessage);
                        LogStreamWriter.Flush();
                    }
                }

#if UNITY_5_3_OR_NEWER
                switch (unityDebugLogMode)
                {
                    case UnityDebugLogMode.Normal:
                        Debug.Log(message);
                        break;
                    case UnityDebugLogMode.Warning:
                        Debug.LogWarning(message);
                        break;
                    case UnityDebugLogMode.Error:
                        Debug.LogError(message);
                        break;
                }
#endif
            }
            finally
            {
                WriteDepth--;
            }
        }

        public static void WriteRaw(string message, Level level = Level.Logic, UnityDebugLogMode unityDebugLogMode = UnityDebugLogMode.Normal)
        {
            WriteDepth++;
            try
            {
                if (IsInitialized && MaxLevel != Level.Disabled && level <= MaxLevel)
                {
                    lock (Locker)
                    {
                        if (ConsoleOutput)
                            Console.Write(message);
                        LogStreamWriter.Write(message);
                        LogStreamWriter.Flush();
                    }
                }

#if UNITY_5_3_OR_NEWER
                switch (unityDebugLogMode)
                {
                    case UnityDebugLogMode.Normal:
                        Debug.Log(message);
                        break;
                    case UnityDebugLogMode.Warning:
                        Debug.LogWarning(message);
                        break;
                    case UnityDebugLogMode.Error:
                        Debug.LogError(message);
                        break;
                }
#endif
            }
            finally
            {
                WriteDepth--;
            }
        }
    }
}
