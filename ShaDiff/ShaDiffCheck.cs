using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ShaDiff
{
    internal class ShaDiffCheck
    {
        string sourceDirectory;
        string targetDirectory;

        string[] sourceFiles;
        string[] targetFiles;
        HashSet<string> targetFilesHashSet;

        List<string> deletedTargetFiles = new List<string>();
        List<string> newTargetFiles = new List<string>();
        List<string> changedTargetFiles = new List<string>();

        readonly SHA256 sha256;

        public ShaDiffCheck(string sourceDirectory, string targetDirectory)
        {
            this.sourceDirectory = sourceDirectory;
            this.targetDirectory = targetDirectory;

            sha256 = SHA256.Create();
        }

        public void Check()
        {
            if (!ValidateDirectory(sourceDirectory) ||
                !ValidateDirectory(targetDirectory))
            {
                return;
            }

            sourceDirectory = Path.GetFullPath(sourceDirectory);
            targetDirectory = Path.GetFullPath(targetDirectory);

            sourceFiles = GetAllFiles(sourceDirectory);
            targetFiles = GetAllFiles(targetDirectory);

            targetFilesHashSet = new HashSet<string>(targetFiles);

            CheckFiles();

            ReportFiles(sourceDirectory, newTargetFiles, "New files:");
            ReportFiles(sourceDirectory, deletedTargetFiles, "Deleted files:");
            ReportFiles(sourceDirectory, changedTargetFiles, "Changed files:");
        }

        static bool ValidateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }

            Console.WriteLine("Directory not found '{0}'", path);

            return false;
        }

        static string[] GetAllFiles(string path)
        {
            var files = new List<string>();

            foreach (string fileName in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                string relativeFileName = fileName.Substring(path.Length + 1);

                files.Add(relativeFileName);
            }

            return files.ToArray();
        }

        void CheckFiles()
        {
            foreach (string relativeFileName in sourceFiles)
            {
                if (!targetFilesHashSet.Remove(relativeFileName))
                {
                    deletedTargetFiles.Add(relativeFileName);
                }
                else if (!CheckFile(relativeFileName))
                {
                    changedTargetFiles.Add(relativeFileName);
                }
            }

            newTargetFiles.AddRange(targetFilesHashSet);
        }

        bool CheckFile(string relativeFileName)
        {
            string sourceFileName = Path.Combine(sourceDirectory, relativeFileName);
            string targetFileName = Path.Combine(targetDirectory, relativeFileName);

            var sourceFileInfo = new FileInfo(sourceFileName);
            var targetFileInfo = new FileInfo(targetFileName);

            if (targetFileInfo.Length != sourceFileInfo.Length)
            {
                return false;
            }

            byte[] sourceFileHash = GetFileHash(sourceFileInfo);
            byte[] targetFileHash = GetFileHash(targetFileInfo);

            return IsHashMatch(sourceFileHash, targetFileHash);
        }

        byte[] GetFileHash(FileInfo fileInfo)
        {
            using (FileStream fileStream = fileInfo.Open(FileMode.Open))
            {
                return sha256.ComputeHash(fileStream);
            }
        }

        static bool IsHashMatch(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
            {
                return false;
            }

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }

            return true;
        }
        static void ReportFiles(string parentDirectory, IEnumerable<string> relativeFileNames, string message)
        {
            if (!relativeFileNames.Any())
            {
                return;
            }

            Console.WriteLine(message);

            foreach (string relativeFileName in relativeFileNames)
            {
                string fullPath = Path.Combine(parentDirectory, relativeFileName);
                Console.WriteLine("    {0}", fullPath);
            }

            Console.WriteLine();
        }
    }
}