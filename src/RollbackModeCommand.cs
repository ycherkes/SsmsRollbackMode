using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using Task = System.Threading.Tasks.Task;
using Window = EnvDTE.Window;

namespace SsmsRollbackMode
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RollbackModeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e52498c3-26f0-4337-ab27-0eec318691b0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private static AsyncPackage _package;
        private static OleMenuCommandService _commandService;
        private static _DTE _dte;
        
        private static string BeginTranStatement => "begin tran\r\n";
        private static string RollbackTranStatement => "\r\nrollback tran";

        /// <summary>
        /// Initializes a new instance of the <see cref="RollbackModeCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RollbackModeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandId = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(ExecuteAsync, menuCommandId) {Checked = General.Default.IsRollbackModeOn};
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RollbackModeCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in EnableDisableRollbackAllCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            _package = package;

            _commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new RollbackModeCommand(package, _commandService);

            _dte = await package.GetServiceAsync(typeof(_DTE)) as _DTE;

            _dte.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;
            _dte.Events.WindowEvents.WindowMoved += WindowEventsOnWindowMoved;
            _dte.Events.WindowEvents.WindowCreated += WindowEvents_WindowCreated;
            
            QueryExecuteEvent = _dte.Events.CommandEvents["{52692960-56BC-4989-B5D3-94C47A513E8D}", 1];
            QueryExecuteEvent.BeforeExecute += OnBeforeQueryExecuted;
            QueryExecuteEvent.AfterExecute += QueryExecuteEventOnAfterExecute;
        }

        private static void WindowEventsOnWindowMoved(Window window, int top, int left, int width, int height)
        {
            UpdateActiveTabColor(window);
        }

        private static void WindowEvents_WindowCreated(Window window)
        {
            UpdateActiveTabColor(window);
        }

        private static void WindowEvents_WindowActivated(Window gotFocus, Window lostFocus)
        {
            UpdateActiveTabColor(gotFocus, false);
            UpdateTabColorData(lostFocus, Colors.BlueViolet, true, false);
        }

        private static void UpdateActiveTabColor(Window window, bool useDelay = true)
        {
            UpdateTabColorData(window, Colors.DarkSeaGreen, useDelay: useDelay, clear: !General.Default.IsRollbackModeOn);
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private static async void ExecuteAsync(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            General.Default.IsRollbackModeOn = !General.Default.IsRollbackModeOn;
            General.Default.Save();

            var command = (MenuCommand) sender;
            command.Checked = General.Default.IsRollbackModeOn;

            if (_dte.ActiveDocument != null)
                UpdateActiveTabColor(_dte.ActiveDocument.ActiveWindow, false);
        }

        private static void OnBeforeQueryExecuted(string guid, int id, object customin, object customout, ref bool canceldefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if(!General.Default.IsRollbackModeOn) return;

                if (_dte.UndoContext.IsOpen)
                {
                    VsShellUtilities.ShowMessageBox(
                        _package,
                        "Another query is in progress",
                        "RollbackModeCommand",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                    canceldefault = true;
                }

                _dte.UndoContext.Open("Wrap with transaction");

                var document = _dte.ActiveDocument;

                var textDocument = (TextDocument)document.Object("TextDocument");

                var selection = textDocument?.Selection != null && !textDocument.Selection.IsEmpty;

                var startPoint = selection
                    ? textDocument.Selection.TopPoint
                    : textDocument.StartPoint;

                var editPoint = startPoint.CreateEditPoint();
                
                editPoint.Insert(BeginTranStatement);

                if (selection)
                {
                    var swapSelection = textDocument.Selection.IsActiveEndGreater;

                    if (swapSelection)
                    {
                        textDocument.Selection.SwapAnchor();
                    }

                    textDocument.Selection.CharLeft(true, BeginTranStatement.Length-1);

                    if(swapSelection)
                    {
                        textDocument.Selection.SwapAnchor();
                    }
                }

                var endPoint = selection
                    ? textDocument.Selection.BottomPoint
                    : textDocument.EndPoint;

                var endEditPoint = endPoint.CreateEditPoint();
                endEditPoint.Insert(RollbackTranStatement);
            }
            catch (Exception e)
            {
                
            }
        }

        private static void QueryExecuteEventOnAfterExecute(string guid, int id, object customin, object customout)
        {
            try
            {
                if (!General.Default.IsRollbackModeOn) return;

                if(_dte.UndoContext.IsOpen)
                    _dte.UndoContext.SetAborted();

            }
            catch (Exception e)
            {
            }
        }

        private static CommandEvents QueryExecuteEvent { get; set; }

        private static void UpdateTabColorData(Window window, Color color, bool clear = false, bool useDelay = true)
        {
            if (!useDelay)
            {
                UpdateTabColorDataInternal(window, color, clear);
                return;
            }

            void OnTick(object sender, EventArgs e)
            {
                var timer = (Timer)sender;
                timer.Tick -= OnTick;
                timer.Stop();
                timer.Dispose();
                UpdateTabColorDataInternal(window, color, clear);
            }

            var timers = new[] {new Timer {Interval = 100}, new Timer {Interval = 700}};

            foreach (var timer in timers)
            {
                timer.Tick += OnTick;
                timer.Start();
            }
        }

        internal static void UpdateTabColorDataInternal(Window docWindow, Color color, bool clear = false)
        {
            var tabBorders = GetWindowTabBorders(docWindow).ToArray();
            SetColor(color, clear, tabBorders);
        }

        private static IEnumerable<Border> GetWindowTabBorders(Window docWindow)
        {
            foreach (var window in Application.Current.Windows)
            {
                var documentGroupControls = WpfHelper.GetObjectsByTypeName(window as DependencyObject,
                    "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DocumentGroupControl");

                foreach (var documentGroupControl in documentGroupControls)
                {
                    if (documentGroupControl == null) continue;

                    var documentTabItems = WpfHelper.GetObjectsByTypeName(documentGroupControl,
                        "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DocumentTabItem");

                    foreach (var documentTabItem in documentTabItems)
                    {
                        if (!(documentTabItem is HeaderedContentControl headeredContentControl)) continue;

                        var header = headeredContentControl.Header;

                        if (header == null) continue;

                        var headerTitle = header.GetType().GetProperty("Title");

                        if (headerTitle == null) continue;

                        var headerTitleValue = headerTitle.GetValue(header, null);
                        var headerTitleString = headerTitleValue.GetType().GetProperty("Title");

                        if (headerTitleString == null
                            || !string.Equals(headerTitleString.GetValue(headerTitleValue, null).ToString().Trim(),
                                docWindow.Caption.Trim(), StringComparison.Ordinal))
                        {
                            continue;
                        }

                        var borders = WpfHelper.GetObjectsByTypeName(documentTabItem, "System.Windows.Controls.Border").ToArray();

                        if (borders.Length <= 4) continue;

                        yield return borders[2] as Border;

                        yield break;
                    }
                }
            }
        }

        private static void SetColor(Color color, bool clear, IReadOnlyCollection<Border> tabBorders)
        {
            if (tabBorders.Count <= 0) return;

            foreach (var tabBorder in tabBorders)
            {
                if (!clear)
                {
                    tabBorder.Background = new SolidColorBrush(color);
                    tabBorder.BorderBrush = tabBorder.Background;
                }
                else
                {
                    tabBorder.ClearValue(Control.BackgroundProperty);
                    tabBorder.ClearValue(Control.BorderBrushProperty);
                }
            }
        }
    }
}