using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OpalStudio.CustomToolbar.Editor.Utils
{
      public static class GitUtils
      {
            private static bool? isGitInstalled;
            public static bool IsGitInstalled
            {
                  get
                  {
                        if (isGitInstalled == null)
                        {
                              try
                              {
                                    var process = new Process
                                    {
                                                StartInfo = new ProcessStartInfo
                                                {
                                                            FileName = "git",
                                                            Arguments = "--version",
                                                            RedirectStandardOutput = true,
                                                            RedirectStandardError = true,
                                                            UseShellExecute = false,
                                                            CreateNoWindow = true,
                                                }
                                    };
                                    process.Start();
                                    process.WaitForExit();
                                    isGitInstalled = process.ExitCode == 0;
                              }
                              catch
                              {
                                    isGitInstalled = false;
                              }
                        }

                        return isGitInstalled.Value;
                  }
            }

            private static string RunGitCommand(string workingDir, string args)
            {
                  if (!IsGitInstalled)
                  {
                        return null;
                  }

                  try
                  {
                        var process = new Process
                        {
                                    StartInfo = new ProcessStartInfo
                                    {
                                                FileName = "git",
                                                Arguments = args,
                                                WorkingDirectory = workingDir,
                                                RedirectStandardOutput = true,
                                                RedirectStandardError = true,
                                                UseShellExecute = false,
                                                CreateNoWindow = true,
                                    }
                        };
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                              Debug.LogWarning($"Git Error: {error}");
                        }

                        return output.Trim();
                  }
                  catch (Exception e)
                  {
                        Debug.LogError($"Git command failed. Is Git installed and in your system's PATH? \nException: {e.Message}");

                        return null;
                  }
            }

            public static string GetCurrentBranch(string repoPath)
            {
                  return RunGitCommand(repoPath, "branch --show-current");
            }

            public static List<string> GetLocalBranches(string repoPath)
            {
                  string output = RunGitCommand(repoPath, "branch --format=\"%(refname:short)\"");

                  return string.IsNullOrEmpty(output)
                              ? new List<string>()
                              : output.Split('\n').Select(static b => b.Trim()).Where(static b => !string.IsNullOrEmpty(b)).ToList();
            }

            public static void SwitchBranch(string repoPath, string branchName)
            {
                  RunGitCommand(repoPath, $"switch {branchName}");

                  UnityEditor.AssetDatabase.Refresh();
            }

            public static List<string> FindGitRepositories()
            {
                  string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
                  var repositories = new List<string>();

                  if (Directory.Exists(Path.Combine(projectRoot, ".git")))
                  {
                        repositories.Add(projectRoot);
                  }

                  string[] potentialRepos = Directory.GetDirectories(Application.dataPath, ".git", SearchOption.AllDirectories);

                  foreach (string repo in potentialRepos)
                  {
                        repositories.Add(Directory.GetParent(repo)!.FullName);
                  }

                  return repositories.Distinct().ToList();
            }

            public static bool HasUncommittedChanges(string repoPath)
            {
                  string output = RunGitCommand(repoPath, "status --porcelain");

                  return !string.IsNullOrEmpty(output);
            }
      }
}