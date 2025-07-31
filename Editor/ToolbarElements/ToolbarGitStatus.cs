using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpalStudio.CustomToolbar.Editor.Core;
using OpalStudio.CustomToolbar.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace OpalStudio.CustomToolbar.Editor.ToolbarElements
{
      sealed internal class ToolbarGitStatus : BaseToolbarElement
      {
            private GUIContent buttonContent;
            private string rootRepoPath;
            private List<string> subRepoPaths;

            protected override string Name => "Git Status";
            protected override string Tooltip => "View and switch Git branches. A '*' indicates uncommitted changes.";

            public override void OnInit()
            {
                  this.Width = 100;
                  buttonContent = new GUIContent();
                  RefreshStatus();

                  EditorApplication.projectChanged -= RefreshStatus;
                  EditorApplication.projectChanged += RefreshStatus;
            }

            public override void OnDrawInToolbar()
            {
                  if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                  {
                        BuildGitMenu().ShowAsContext();
                  }
            }

            private void RefreshStatus()
            {
                  List<string> allRepos = GitUtils.FindGitRepositories();
                  string projectRootPath = Directory.GetParent(Application.dataPath)!.FullName;

                  rootRepoPath = allRepos.Find(p => p == projectRootPath);
                  subRepoPaths = allRepos.Where(p => p != projectRootPath).ToList();

                  int totalRepos = subRepoPaths.Count + (!string.IsNullOrEmpty(rootRepoPath) ? 1 : 0);

                  if (totalRepos > 0)
                  {
                        buttonContent.text = $" Git: {totalRepos}";
                        buttonContent.image = EditorGUIUtility.IconContent("d_CacheServerConnected").image;
                        buttonContent.tooltip = $"{totalRepos} Git repositories found in the project.";
                  }
                  else
                  {
                        buttonContent.text = "Git: (None)";
                        buttonContent.image = EditorGUIUtility.IconContent("d_CacheServerDisconnected").image;
                        buttonContent.tooltip = "No Git repository found in the project.";
                  }
            }

            private GenericMenu BuildGitMenu()
            {
                  var menu = new GenericMenu();

                  if (!string.IsNullOrEmpty(rootRepoPath))
                  {
                        string currentBranch = GitUtils.GetCurrentBranch(rootRepoPath);
                        List<string> allBranches = GitUtils.GetLocalBranches(rootRepoPath);
                        bool isDirty = GitUtils.HasUncommittedChanges(rootRepoPath);

                        string rootMenuName = $"Unity{(isDirty ? "*" : "")}";

                        foreach (string branch in allBranches)
                        {
                              menu.AddItem(new GUIContent($"{rootMenuName}/{branch}"), branch == currentBranch, () => GitUtils.SwitchBranch(rootRepoPath, branch));
                        }
                  }

                  if (subRepoPaths.Any())
                  {
                        if (!string.IsNullOrEmpty(rootRepoPath))
                        {
                              menu.AddSeparator("");
                        }

                        foreach (string repoPath in subRepoPaths)
                        {
                              string repoName = Path.GetFileName(repoPath);
                              string currentBranch = GitUtils.GetCurrentBranch(repoPath);
                              List<string> allBranches = GitUtils.GetLocalBranches(repoPath);
                              bool isDirty = GitUtils.HasUncommittedChanges(repoPath);

                              if (!allBranches.Any())
                              {
                                    continue;
                              }

                              string repoMenuName = $"{repoName}{(isDirty ? "*" : "")}";

                              foreach (string branch in allBranches)
                              {
                                    menu.AddItem(new GUIContent($"{repoMenuName}/{branch}"), branch == currentBranch, () => GitUtils.SwitchBranch(repoPath, branch));
                              }
                        }
                  }

                  if (menu.GetItemCount() == 0)
                  {
                        menu.AddDisabledItem(new GUIContent("No Git repository found"));
                  }

                  return menu;
            }
      }
}