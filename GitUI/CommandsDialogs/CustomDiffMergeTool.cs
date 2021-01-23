using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class CustomDiffMergeTool
    {
        public CustomDiffMergeTool(ToolStripMenuItem menuItem, System.EventHandler click)
        {
            _menuItem = menuItem;
            _click = click;
        }

        private ToolStripMenuItem _menuItem { get; set; }
        private System.EventHandler _click;

        // static methods
        private static CustomDiffMergeToolCache DiffToolCache { get; } = new();
        private static CustomDiffMergeToolCache MergeToolCache { get; } = new();

        /// <summary>
        /// Clear the existing caches
        /// </summary>
        public static void Clear()
        {
            DiffToolCache.Clear();
            MergeToolCache.Clear();
        }

        /// <summary>
        /// Load the availble DiffMerge tools and apply to the menus
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="menus">The menus to update</param>
        /// <param name="components">The calling Form components, to dispose correctly</param>
        /// <param name="isDiff">True if diff, false if merge</param>
        /// <param name="delay">The delay before starting the operation</param>
        public static void LoadCustomDiffMergeTools(GitModule module, IList<CustomDiffMergeTool> menus, IContainer components, bool isDiff, int delay = 1000)
        {
            InitMenus(menus);

            if (isDiff && !AppSettings.ShowAvailableDiffTools)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // get the tools, possibly with a delay as requesting requires considerable time
                // cache is shared
                IEnumerable<string> tools = await (isDiff ? DiffToolCache : MergeToolCache)
                    .GetToolsAsync(module, isDiff, delay);

                if (tools.Count() <= 1)
                {
                    // No need to show the menu
                    return;
                }

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (var menu in menus)
                {
                    menu._menuItem.DropDown = new ContextMenuStrip(components);
                    foreach (var tool in tools)
                    {
                        var item = new ToolStripMenuItem(tool) { Tag = tool };

                        item.Click += menu._click;
                        menu._menuItem.DropDown.Items.Add(item);
                        if (menu._menuItem.DropDownItems.Count == 1)
                        {
                            item.Font = new Font(item.Font, FontStyle.Bold);
                            if (menu._menuItem.ShortcutKeys != Keys.None)
                            {
                                item.ShortcutKeys = menu._menuItem.ShortcutKeys;
                            }
                            else
                            {
                                item.ShortcutKeyDisplayString = menu._menuItem.ShortcutKeyDisplayString;
                            }
                        }
                    }

                    if (isDiff)
                    {
                        // Allow disabling for difftools
                        menu._menuItem.DropDown.Items.Add(new ToolStripSeparator());
                        var disableItem = new ToolStripMenuItem
                        {
                            Text = ResourceManager.Strings.DisableMenuItem
                        };

                        disableItem.Click += (o, s) =>
                        {
                            // Disables the dropdown in the current form only
                            // RevDiff will not be affected from other forms, requires a restart
                            // To overcome this limitation other forms would require a reference to RevDiff
                            AppSettings.ShowAvailableDiffTools = false;
                            InitMenus(menus);
                        };
                        menu._menuItem.DropDown.Items.Add(disableItem);
                    }
                }
            }).FileAndForget();
            return;

            static void InitMenus(IList<CustomDiffMergeTool> menus)
            {
                foreach (var menu in menus)
                {
                    menu._menuItem.DropDown = null;
                }
            }
        }
    }
}
