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
            private GUIContent _buttonContent;
            private string _rootRepoPath;
            private List<string> _subRepoPaths;
            private bool _isGitReady;

            protected override string Name => "Git Status";
            protected override string Tooltip => "View and switch Git branches. A '*' indicates uncommitted changes.";

            public override void OnInit()
            {
                  this.Width = 100;
                  _buttonContent = new GUIContent();
                  _isGitReady = GitUtils.IsGitInstalled;
                  RefreshStatus();

                  EditorApplication.projectChanged -= RefreshStatus;
                  EditorApplication.projectChanged += RefreshStatus;
            }

            public override void OnDrawInToolbar()
            {
                  using (new EditorGUI.DisabledScope(!_isGitReady))
                  {
                        if (EditorGUILayout.DropdownButton(_buttonContent, FocusType.Keyboard, ToolbarStyles.CommandPopupStyle, GUILayout.Width(this.Width)))
                        {
                              BuildGitMenu().ShowAsContext();
                        }
                  }
            }

            private void RefreshStatus()
            {
                  if (!_isGitReady)
                  {
                        _buttonContent.text = " Git: N/A";
                        _buttonContent.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                        _buttonContent.tooltip = "Git command not found. Is Git installed and in your system's PATH?";

                        return;
                  }

                  List<string> allRepos = GitUtils.FindGitRepositories();
                  string projectRootPath = Directory.GetParent(Application.dataPath)!.FullName;

                  _rootRepoPath = allRepos.Find(p => p == projectRootPath);
                  _subRepoPaths = allRepos.Where(p => p != projectRootPath).ToList();

                  int totalRepos = _subRepoPaths.Count + (!string.IsNullOrEmpty(_rootRepoPath) ? 1 : 0);

                  if (totalRepos > 0)
                  {
                        _buttonContent.text = $" Git: {totalRepos}";
                        _buttonContent.image = EditorGUIUtility.IconContent("d_CacheServerConnected").image;
                        _buttonContent.tooltip = $"{totalRepos} Git repositories found in the project.";
                  }
                  else
                  {
                        _buttonContent.text = "Git: (None)";
                        _buttonContent.image = EditorGUIUtility.IconContent("d_CacheServerDisconnected").image;
                        _buttonContent.tooltip = "No Git repository found in the project.";
                  }
            }

            private GenericMenu BuildGitMenu()
            {
                  var menu = new GenericMenu();

                  if (!_isGitReady)
                  {
                        menu.AddDisabledItem(new GUIContent("Git not found on this system"));

                        return menu;
                  }

                  if (!string.IsNullOrEmpty(_rootRepoPath))
                  {
                        string currentBranch = GitUtils.GetCurrentBranch(_rootRepoPath);
                        List<string> allBranches = GitUtils.GetLocalBranches(_rootRepoPath);
                        bool isDirty = GitUtils.HasUncommittedChanges(_rootRepoPath);

                        string rootMenuName = $"Unity{(isDirty ? "*" : "")}";

                        foreach (string branch in allBranches)
                        {
                              menu.AddItem(new GUIContent($"{rootMenuName}/{branch}"), branch == currentBranch, () => GitUtils.SwitchBranch(_rootRepoPath, branch));
                        }
                  }

                  if (_subRepoPaths.Any())
                  {
                        if (!string.IsNullOrEmpty(_rootRepoPath))
                        {
                              menu.AddSeparator("");
                        }

                        foreach (string repoPath in _subRepoPaths)
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