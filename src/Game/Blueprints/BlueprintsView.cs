using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Global;
using DoubleQoL.QoL.UI;
using DoubleQoL.QoL.UI.Blueprint;
using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Collections.ReadonlyCollections;
using Mafi.Core;
using Mafi.Core.Buildings.Offices;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Components.Tabs;
using Mafi.Unity.UserInterface.Components;
using Steamworks;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static DoubleQoL.QoL.UI.Blueprint.BlueprintService;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintsTabs"/>.
    /// </summary>
    internal abstract class BlueprintsView : Tab, IInputUpdate {
        public static readonly int SPACING = 2;
        public static readonly int COLUMNS_COUNT = 5;
        public static readonly Vector2 SIZE = new Vector2(BlueprintView.WIDTH * COLUMNS_COUNT + (COLUMNS_COUNT - 1) * SPACING + 10 + 16, 630f);

        private readonly IUnityInputMgr m_inputMgr;
        private readonly LazyResolve<BlueprintsController> m_controller;
        internal readonly QoLBlueprintsLibrary _blueprintsLibrary;
        private readonly UnlockedProtosDbForUi m_unlockedProtosDb;
        private readonly BlueprintCreationController m_blueprintCreationController;
        private readonly CaptainOfficeManager m_captainOfficeManager;
        private readonly BlueprintsWindowView _parent;
        private DeleteBlueprintConfirmDialog m_confirmDeleteDialog;
        private BlueprintCopyPasteDialog m_blueprintCopyPasteDialog;
        private BlueprintDescriptionDialog m_blueprintDescDialog;
        private ViewsCacheHomogeneous<BlueprintView> m_viewsCache;
        private GridContainer m_gridContainer;
        private StackContainer m_rightTopBar;
        private ScrollableContainer m_scrollableContainer;
        private Panel m_insertPanel;
        private Panel m_officeNotAvailable;
        private Txt m_locationView;
        private Txt m_officeNotAvailableTxt;
        private Btn m_placeItBtn;
        private Btn m_infoBtn;
        private TxtField m_searchField;
        private Tooltip m_infoBtnTooltip;
        private readonly string m_captainOfficeName;
        private Option<BlueprintView> m_selectedItem;
        private Option<BlueprintView> m_viewBeingDragged;
        private Option<BlueprintView> m_lastHoveredView;
        private Option<IBlueprintItem> m_newItem;
        private Option<ServerInfo> _server;
        public abstract bool IsLocal { get; }
        private IBlueprintsFolder CurrentFolder { get; set; }
        private int m_indexToMoveTo;
        internal StupidBlueprintManager BlueprintManager;

        internal BlueprintsView(IUnityInputMgr inputMgr,
            LazyResolve<BlueprintsController> controller,
            NewInstanceOf<QoLBlueprintsLibrary> blueprintsLibrary,
            UnlockedProtosDbForUi unlockedProtosDb,
            ProtosDb protosDb,
            BlueprintCreationController blueprintCreationController,
            CaptainOfficeManager captainOfficeManager,
            BlueprintsWindowView parent) : base("Blueprints") {
            BlueprintManager = new StupidBlueprintManager();
            _parent = parent;
            m_inputMgr = inputMgr;
            m_controller = controller;
            _blueprintsLibrary = blueprintsLibrary.Instance;
            if (!IsLocal) _blueprintsLibrary.InvokeMethod("TestOnly_DisableSerialization");
            else {
                CurrentFolder = _blueprintsLibrary.Root;
                _blueprintsLibrary.SaveCurrentRoot(_blueprintsLibrary.Root);
            }
            m_unlockedProtosDb = unlockedProtosDb;
            m_blueprintCreationController = blueprintCreationController;
            m_captainOfficeManager = captainOfficeManager;

            m_captainOfficeName = protosDb.All<CaptainOfficeProto>().FirstOrDefault(x => x.Upgrade.PreviousTier.IsNone)?.Strings.Name.TranslatedString ?? "";
            ShowAfterSync = true;
        }

        public void SetServer(ServerInfo server) {
            if (IsLocal) return;
            _server = server;
            LoadData();
        }

        private void LoadData() {
            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync() {
            try {
                if (IsLocal) return;
                var respond = await _server.Value.GetAllBlueprints();
                if (!respond.IsSuccess) return;
                _blueprintsLibrary.Clear();
                BlueprintManager.Clear();
                m_searchField.ClearInput();
                CurrentFolder = _blueprintsLibrary.Root;
                foreach (var blueprint in respond.Data.Blueprints) {
                    if (!BlueprintManager.GetRootItem(blueprint.Owner_id, out var folder)) {
                        folder = _blueprintsLibrary.AddNewFolder(_blueprintsLibrary.Root);
                        folder.InvokeSetter("Name", respond.Data.Owners[blueprint.Owner_id + ""]);
                        BlueprintManager.AddRoot(folder, blueprint.Owner_id);
                    }
                    if (_blueprintsLibrary.TryAddBlueprintFromString((IBlueprintsFolder)folder, blueprint.Data, out var result)) {
                        BlueprintManager.AddItem(result, blueprint);
                    }
                }
                _blueprintsLibrary.SaveCurrentRoot();
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error: {ex.Message}");
            }
        }

        protected override void BuildUi() {
            Panel mainPanel = Builder.NewPanel("main", this).SetSize(SIZE);
            m_confirmDeleteDialog = new DeleteBlueprintConfirmDialog(Builder, new Action(OnDeleteConfirm));
            m_blueprintCopyPasteDialog = new BlueprintCopyPasteDialog(Builder, new Func<string, bool>(OnStringImportClick));
            if (!IsLocal) m_blueprintCopyPasteDialog.SetMessage(new LocStrFormatted("Failed to parse and upload the string"));
            m_blueprintDescDialog = new BlueprintDescriptionDialog(Builder, new Action<string>(OnDescriptionApply));
            m_viewsCache = new ViewsCacheHomogeneous<BlueprintView>(() => new BlueprintView(m_gridContainer, this, m_unlockedProtosDb, Builder, new Action<BlueprintView>(OnItemSelected), new Action<BlueprintView>(OnBlueprintDoubleClick), IsLocal));
            int num1 = 30;
            int num2 = 52;
            StackContainer leftTopOf = Builder.NewStackContainer("TopLeftBar").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(10f).PutToLeftTopOf(mainPanel, new Vector2(0.0f, 30f), Offset.All(10f));
            m_searchField = Builder.NewTxtField("searchfield").SetPlaceholderText("Search").EnableSelectionOnFocus().SetOnValueChangedAction(StartSearch).SetVisibility(!IsLocal).AppendTo(leftTopOf, num2 * 5);
            Btn fromSelectionBtn = Builder.NewBtnPrimary("FromSelection", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.SelectArea_svg, Offset.All(4f)).AddToolTip(Tr.Blueprint_NewFromSelectionTooltip).OnClick(() => m_blueprintCreationController.ActivateForSelection(new Action<ImmutableArray<EntityConfigData>>(OnNewBlueprintRequested))).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn newFolderBtn = Builder.NewBtnGeneral("NewFolder", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.NewFolder_svg, Offset.All(4f)).AddToolTip(Tr.NewFolder__Tooltip).OnClick(new Action(CreateNewFolder)).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn fromStrBtn = Builder.NewBtnGeneral("FromString", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.ImportFromString_svg, Offset.All(4f)).AddToolTip(Tr.Blueprint_NewFromStringTooltip).OnClick(() => m_blueprintCopyPasteDialog.ShowForStringImport()).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn uploadBtn = Builder.NewBtnGeneral("UploadString", leftTopOf).SetIcon(IconPaths.General_Upload, Offset.All(4f)).AddToolTip("Upload a blueprint or folder").OnClick(() => m_blueprintCopyPasteDialog.ShowForStringUpload()).SetVisibility(!IsLocal).AppendTo(leftTopOf, num2);
            Btn refreshBtn = Builder.NewBtnGeneral("Refresh", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.Repeat_svg, Offset.All(4f)).AddToolTip("Refresh Store").OnClick(LoadData).SetVisibility(!IsLocal).AppendTo(leftTopOf, num2);
            m_rightTopBar = Builder.NewStackContainer("TopRightBar").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(10f).PutToRightTopOf(mainPanel, new Vector2(0.0f, 30f), Offset.All(10f));
            m_placeItBtn = Builder.NewBtnPrimary("Place").SetIcon(Assets.Unity.UserInterface.General.Build_svg).OnClick(() => {
                if (m_selectedItem.HasValue && !m_selectedItem.Value.IsLocked && m_selectedItem.Value.Blueprint.HasValue) m_controller.Value.StartBlueprintPlacement(m_selectedItem.Value.Blueprint.Value);
            }).AddToolTip(Tr.Blueprint_PlaceItTooltip).AppendTo(m_rightTopBar, num2);

            Btn m_toStrBtn = Builder.NewBtnGeneral("ToString").SetIcon(Assets.Unity.UserInterface.General.ExportToString_svg).AddToolTip(Tr.Blueprint_ExportToStringTooltip).OnClick(new Action(ExportSelectedItemToString)).AppendTo(m_rightTopBar, num2);
            Btn m_updateDescBtn = Builder.NewBtnGeneral("UpdateDesc").SetIcon(Assets.Unity.UserInterface.General.EditDescription_svg).AddToolTip(Tr.UpdateDescription__Tooltip).OnClick(new Action(RequestDescriptionEdit)).SetVisibility(IsLocal).AppendTo(m_rightTopBar, num2);
            Btn m_deleteBtn = Builder.NewBtnDanger("Delete").SetIcon(Assets.Unity.UserInterface.General.Trash128_png).AddToolTip(Tr.BlueprintDelete__Tooltip).OnClick(new Action(StartBlueprintDelete)).SetVisibility(IsLocal).AppendTo(m_rightTopBar, num2);
            Btn m_downloadBtn = Builder.NewBtnGeneral("Download").SetIcon(IconPaths.General_Download).AddToolTip("Download the blueprint or folder").OnClick(new Action(DownloadSelectedItem)).SetVisibility(!IsLocal).AppendTo(m_rightTopBar, num2);
            Btn m_starBtn = Builder.NewBtnGeneral("Star").SetIcon(IconPaths.General_Star).AddToolTip("Give it a Star (Coming Soon)").OnClick(new Action(StarSelectedBlueprint)).SetVisibility(!IsLocal).AppendTo(m_rightTopBar, num2); m_infoBtn = Builder.NewBtnGeneral("Info").SetIcon(Assets.Unity.UserInterface.General.Info128_png).SetEnabled(false).AppendTo(m_rightTopBar, num2); ;
            m_infoBtnTooltip = m_infoBtn.AddToolTipAndReturn();
            m_locationView = Builder.NewTxt("Navigation").SetTextStyle(Builder.Style.Global.TextMediumBold).SetAlignment(TextAnchor.MiddleLeft).PutToTopOf(mainPanel, 20f, Offset.Top(50f) + Offset.Left(30f));

            Builder.NewPanel("Home").SetBackground(Assets.Unity.UserInterface.General.Home_svg).OnClick(() => CurrentFolder = _blueprintsLibrary.Root).PutToLeftOf(m_locationView, 18f, Offset.Left(-22f));
            Txt m_saveStatusTxt = Builder.NewTxt("Status", mainPanel).SetTextStyle(Builder.Style.Global.TextMediumBold).SetAlignment(TextAnchor.MiddleLeft).SetVisibility(IsLocal).PutToRightTopOf(mainPanel, new Vector2(0.0f, 20f), Offset.Top(50f) + Offset.Right(41f));
            Tooltip statusTooltip = Builder.AddTooltipFor(Builder.NewPanel("Info", m_saveStatusTxt).SetBackground(Assets.Unity.UserInterface.General.Info128_png).SetVisibility(IsLocal).PutToRightMiddleOf(m_saveStatusTxt, 15.Vector2(), Offset.Right(-18f)));
            m_scrollableContainer = Builder.NewScrollableContainer("ScrollableContainer").AddVerticalScrollbar().PutTo(mainPanel, Offset.Top(num1 + 40));
            m_gridContainer = Builder.NewGridContainer("Container").SetCellSize(BlueprintView.SIZE).SetCellSpacing(SPACING).SetDynamicHeightMode(COLUMNS_COUNT).SetInnerPadding(Offset.All(5f));
            m_scrollableContainer.AddItemTopLeft(m_gridContainer);
            m_officeNotAvailable = Builder.NewPanel("NotAvailableOverlay", mainPanel).SetBackground(new ColorRgba(2894116, 240)).PutTo(mainPanel).Hide();
            m_officeNotAvailableTxt = Builder.NewTxt("Text", m_officeNotAvailable).SetTextStyle(Builder.Style.Global.TextMediumBold.Extend(color: Builder.Style.Global.OrangeText, fontSize: 18)).SetAlignment(TextAnchor.MiddleCenter).SetText(Tr.Blueprints_BuildingRequired.Format(m_captainOfficeName)).AddOutline().PutTo(m_officeNotAvailable, Offset.LeftRight(10f));
            ColorRgba colorRgba = 12158750;
            m_insertPanel = Builder.NewPanel("LeftEdge", m_gridContainer).SetBackground(colorRgba).SetWidth(2f).Hide();
            Builder.NewIconContainer("LeftArrow", m_insertPanel).SetIcon(Assets.Unity.UserInterface.General.Next_svg, colorRgba).PutToLeftOf(m_insertPanel, 22f, Offset.Left(-32f)).AddOutline();
            Builder.NewIconContainer("RightArrow", m_insertPanel).SetIcon(Assets.Unity.UserInterface.General.Return_svg, colorRgba).PutToRightOf(m_insertPanel, 22f, Offset.Right(-32f)).AddOutline();
            mainPanel.OnClick(() => {
                m_selectedItem.ValueOrNull?.CommitRenamingIfCan();
                ClearSelectedItem();
            });
            mainPanel.PutTo(this);
            this.SetSize(SIZE);
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            updaterBuilder.Observe(() => CurrentFolder.Folders, CompareFixedOrder<IBlueprintsFolder>.Instance).Observe(() => CurrentFolder.Blueprints, CompareFixedOrder<IBlueprint>.Instance).Do(new Action<Lyst<IBlueprintsFolder>, Lyst<IBlueprint>>(UpdateItems));
            updaterBuilder.Observe(() => _blueprintsLibrary.LibraryStatus).Observe(() => _blueprintsLibrary.NumberOfBackupsAvailable).Do((status, backupsTotal) => {
                ColorRgba color2 = Builder.Style.Global.TextMediumBold.Color;
                ColorRgba dangerClr = Builder.Style.Global.DangerClr;
                bool enabled = status != BlueprintsLibrary.Status.LoadingInProgress;
                fromSelectionBtn.SetEnabled(enabled);
                newFolderBtn.SetEnabled(enabled);
                fromStrBtn.SetEnabled(!IsLocal || enabled);
                string str1 = "";
                switch (status) {
                    case BlueprintsLibrary.Status.NoLibraryFound:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.LoadingInProgress:
                        m_saveStatusTxt.SetText(Tr.LoadInProgress);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.LoadFailedDueToFormat:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToLoad);
                        str1 = Tr.BlueprintLibStatus__FailedToLoadOnFormat.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.LoadFailedNoAccess:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToLoad);
                        str1 = Tr.BlueprintLibStatus__FailedToLoadOnPermission.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.LoadSuccess:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveInProgress:
                        m_saveStatusTxt.SetText(Tr.SaveInProgress);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveFailed:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToSave);
                        str1 = Tr.BlueprintLibStatus__FailedToSaveTooltip.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.SaveDone:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveDoneBackupFailed:
                        m_saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToBackup);
                        str1 = Tr.BlueprintLibStatus__FailedToBackupTooltip.TranslatedString;
                        setColor(dangerClr);
                        break;

                    default:
                        Logging.Log.Error(string.Format("Unknown enum state {0}", status));
                        break;
                }
                string str2 = Tr.FileLocation.Format(_blueprintsLibrary.PathToFile).Value;
                if (backupsTotal > 0) str2 += string.Format(" ({0})", Tr.Blueprint__NumberOfBackups.Format(backupsTotal));
                statusTooltip.SetText(str1.IsEmpty() ? str2 : str1 + " " + str2);
                m_saveStatusTxt.SetWidth(m_saveStatusTxt.GetPreferedWidth());
            });
            updaterBuilder.Observe(() => m_captainOfficeManager.CaptainOffice).Observe(() => m_captainOfficeManager.IsOfficeActive).Do((office, isActive) => {
                m_officeNotAvailable.SetVisibility(!isActive);
                if (isActive) return;
                string str = office.HasValue ? office.Value.Prototype.Strings.Name.TranslatedString : m_captainOfficeName;
                m_officeNotAvailableTxt.SetText(Tr.Blueprints_BuildingRequired.Format(str));
            });
            AddUpdater(updaterBuilder.Build());
            AddUpdater(m_viewsCache.Updater);
            void setColor(ColorRgba color) => m_saveStatusTxt.SetColor(color);
        }

        private void RequestDescriptionEdit() {
            if (m_selectedItem.IsNone) return;
            m_blueprintDescDialog.ShowForEdit(m_selectedItem.Value.Item.ValueOrNull?.Desc ?? "");
        }

        private void OnNewBlueprintRequested(ImmutableArray<EntityConfigData> data) {
            m_newItem = _blueprintsLibrary.AddBlueprint(CurrentFolder, data).As<IBlueprintItem>();
            m_inputMgr.ActivateNewController(m_controller.Value);
        }

        private void CreateNewFolder() => m_newItem = _blueprintsLibrary.AddNewFolder(CurrentFolder).CreateOption().As<IBlueprintItem>();

        private void UpdateItems(
          IIndexable<IBlueprintsFolder> folders,
          IIndexable<IBlueprint> blueprints) {
            ClearSelectedItem();
            m_gridContainer.StartBatchOperation();
            m_gridContainer.ClearAll();
            m_viewsCache.ReturnAll();
            if (CurrentFolder.ParentFolder.HasValue) {
                BlueprintView view = m_viewsCache.GetView();
                view.SetBlueprintsFolder(CurrentFolder.ParentFolder.Value, true);
                m_gridContainer.Append(view);
            }
            foreach (IBlueprintsFolder folder in folders) {
                BlueprintView view = m_viewsCache.GetView();
                view.SetBlueprintsFolder(folder);
                m_gridContainer.Append(view);
            }
            foreach (IBlueprint blueprint in blueprints) {
                BlueprintView view = m_viewsCache.GetView();
                view.SetBlueprint(blueprint);
                m_gridContainer.Append(view);
            }
            m_gridContainer.FinishBatchOperation();
            m_locationView.SetVisibility(CurrentFolder.ParentFolder.HasValue);
            Option<IBlueprintsFolder> parentFolder = CurrentFolder.ParentFolder;
            if (!parentFolder.HasValue) return;
            string text = " > " + CurrentFolder.Name;
            IBlueprintsFolder blueprintsFolder = CurrentFolder.ParentFolder.Value;
            while (true) {
                parentFolder = blueprintsFolder.ParentFolder;
                if (parentFolder.HasValue) {
                    text = " > " + blueprintsFolder.Name + text;
                    blueprintsFolder = blueprintsFolder.ParentFolder.Value;
                }
                else break;
            }
            m_locationView.SetText(text);
        }

        private void OnItemSelected(BlueprintView blueprintView) {
            ClearSelectedItem();
            m_selectedItem = blueprintView;
            blueprintView.SetIsSelected(true);
            UpdateRightBars();
            m_placeItBtn.SetEnabled(!m_selectedItem.Value.IsLocked);
        }

        private void ClearSelectedItem() {
            m_selectedItem.ValueOrNull?.CancelRenamingIfCan();
            m_selectedItem.ValueOrNull?.SetIsSelected(false);
            m_selectedItem = Option.None;
            UpdateRightBars();
        }

        private void UpdateRightBars() {
            m_rightTopBar.SetVisibility(m_selectedItem.HasValue);
            if (m_selectedItem.IsNone) return;
            bool hasValue = m_selectedItem.Value.Blueprint.HasValue;
            m_rightTopBar.SetItemVisibility(m_placeItBtn, hasValue);
            m_rightTopBar.SetItemVisibility(m_infoBtn, hasValue);
            m_infoBtnTooltip.SetText(hasValue ? Tr.MadeInVersion.Format(m_selectedItem.Value.Blueprint.Value.GameVersion ?? "").Value : "");
        }

        private void OnBlueprintDoubleClick(BlueprintView blueprintView) {
            if (blueprintView.Blueprint.HasValue) m_controller.Value.StartBlueprintPlacement(blueprintView.Blueprint.Value);
            else if (blueprintView.BlueprintsFolder.HasValue) CurrentFolder = blueprintView.BlueprintsFolder.Value;
        }

        private void ExportSelectedItemToString() {
            if (m_selectedItem.IsNone) return;
            if (m_selectedItem.Value.Blueprint.HasValue) m_blueprintCopyPasteDialog.ShowForStringExport(_blueprintsLibrary.ConvertToString(m_selectedItem.Value.Blueprint.Value));
            else if (m_selectedItem.Value.BlueprintsFolder.HasValue) m_blueprintCopyPasteDialog.ShowForStringExport(_blueprintsLibrary.ConvertToString(m_selectedItem.Value.BlueprintsFolder.Value));
        }

        private void StartBlueprintDelete() {
            if (m_selectedItem.IsNone || !IsLocal) return;
            if ((m_selectedItem.Value.Item.ValueOrNull is IBlueprintsFolder valueOrNull) && valueOrNull.IsEmpty) OnDeleteConfirm();
            else m_confirmDeleteDialog.SetNameAndShow(m_selectedItem.Value.Title);
        }

        private void DownloadSelectedItem() {
            if (m_selectedItem.IsNone || IsLocal) return;
            var bl = _parent.BlueprintsLocalView._blueprintsLibrary;
            if ((m_selectedItem.Value.Item.ValueOrNull is IBlueprintItem valueOrNull)) bl.TryAddBlueprintFromString(bl.Root, _blueprintsLibrary.ConvertItemString(valueOrNull), out _);
        }

        private void StarSelectedBlueprint() {
            if (m_selectedItem.IsNone || IsLocal) return;

            if ((m_selectedItem.Value.Item.ValueOrNull is IBlueprint valueOrNull)) {
            }
        }

        private bool OnStringImportClick(string data) {
            if (string.IsNullOrEmpty(data)) return false;
            if (IsLocal) return _blueprintsLibrary.TryAddBlueprintFromString(CurrentFolder, data, out _);
            try {
                string id = SteamClient.SteamId + "";
                string name = SteamClient.Name;
                _blueprintsLibrary.TryParseFromString(data, out string error, out var result);
                if (!string.IsNullOrEmpty(error)) return false;
                Task.Run(async () => {
                    var response = await _server.Value.UploadBlueprint(data);
                    if (response.IsSuccess) {
                        await LoadDataAsync();
                        m_blueprintCopyPasteDialog.OnUploadSuccess();
                    }
                    else m_blueprintCopyPasteDialog.OnUploadFailed(response.Error);
                });
                return true;
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error: {ex.Message}");
            }
            return false;
        }

        private void OnDeleteConfirm() {
            if (m_selectedItem.IsNone || !m_selectedItem.Value.Item.HasValue || !IsLocal) return;
            _blueprintsLibrary.DeleteItem(CurrentFolder, m_selectedItem.Value.Item.Value);
        }

        private void OnDescriptionApply(string newDescription) {
            if (m_selectedItem.IsNone || !m_selectedItem.Value.Item.HasValue) return;
            _blueprintsLibrary.SetDescription(m_selectedItem.Value.Item.Value, newDescription);
            m_selectedItem.Value.UpdateDesc();
        }

        private void StartSearch() {
            if (IsLocal) return;
            var text = m_searchField.GetText();
            if (string.IsNullOrEmpty(text)) {
                _blueprintsLibrary.RestoreSavedRoot();
                return;
            }
            _blueprintsLibrary.UpdateRoot(_blueprintsLibrary.SearchItems(text));
            CurrentFolder = _blueprintsLibrary.Root;
        }

        public bool InputUpdate(IInputScheduler inputScheduler) {
            if (IsVisible && m_newItem.HasValue) {
                foreach (BlueprintView existingOne in m_viewsCache.AllExistingOnes()) {
                    if (m_newItem == existingOne.Item) {
                        OnItemSelected(existingOne);
                        existingOne.StartRenamingSession();
                        m_newItem = Option.None;
                        m_scrollableContainer.ScrollToElement(existingOne.RectTransform);
                        return true;
                    }
                }
            }
            if (m_blueprintCopyPasteDialog.IsVisible() && m_blueprintCopyPasteDialog.InputUpdate()) return true;
            if (m_confirmDeleteDialog.IsVisible() && m_confirmDeleteDialog.InputUpdate()) return true;
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (m_blueprintCopyPasteDialog.IsVisible()) {
                    m_blueprintCopyPasteDialog.Hide();
                    return true;
                }
                if (m_confirmDeleteDialog.IsVisible()) {
                    m_confirmDeleteDialog.Hide();
                    return true;
                }
                if (m_blueprintDescDialog.IsVisible()) {
                    m_blueprintDescDialog.Hide();
                    return true;
                }
                foreach (BlueprintView existingOne in m_viewsCache.AllExistingOnes()) if (existingOne.CancelRenamingIfCan()) return true;
            }
            if (m_blueprintCopyPasteDialog.IsVisible() || m_confirmDeleteDialog.IsVisible() || m_blueprintDescDialog.IsVisible()) return true;
            if (Input.GetKeyDown(KeyCode.Backspace) && CurrentFolder.ParentFolder.HasValue) {
                CurrentFolder = CurrentFolder.ParentFolder.Value;
                return true;
            }
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
                foreach (BlueprintView existingOne in m_viewsCache.AllExistingOnes()) if (existingOne.CommitRenamingIfCan()) return true;
            if (m_selectedItem.HasValue) {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    m_selectedItem.Value.StartRenamingSession();
                    return true;
                }
                if (Input.GetKeyDown(KeyCode.Delete)) {
                    StartBlueprintDelete();
                    return true;
                }
            }
            return false;
        }

        public void OnDragStart(BlueprintView viewDragged) {
            if (!IsLocal) return;
            m_viewBeingDragged = (Option<BlueprintView>)viewDragged;
            m_viewBeingDragged.Value.SetScale(new Vector2(0.7f, 0.7f));
            m_viewBeingDragged.Value.SetParent(this);
        }

        public void OnDragDone() {
            if (m_viewBeingDragged.IsNone || !IsLocal) return;
            if (m_indexToMoveTo >= 0) {
                _blueprintsLibrary.TryReorderItem(m_viewBeingDragged.Value.Item.Value, CurrentFolder, m_indexToMoveTo);
                m_indexToMoveTo = -1;
            }
            else if (m_lastHoveredView.HasValue) {
                m_lastHoveredView.Value.SetHovered(false);
                Option<IBlueprintsFolder> blueprintsFolder = m_lastHoveredView.Value.BlueprintsFolder;
                IBlueprintsFolder valueOrNull1 = blueprintsFolder.ValueOrNull;
                if (valueOrNull1 != null) {
                    IBlueprint valueOrNull2 = m_viewBeingDragged.Value.Blueprint.ValueOrNull;
                    blueprintsFolder = m_viewBeingDragged.Value.BlueprintsFolder;
                    IBlueprintsFolder valueOrNull3 = blueprintsFolder.ValueOrNull;
                    if (valueOrNull2 != null) _blueprintsLibrary.MoveBlueprint(valueOrNull2, CurrentFolder, valueOrNull1);
                    else if (valueOrNull3 != null) _blueprintsLibrary.MoveFolder(valueOrNull3, valueOrNull1);
                }
            }
            m_lastHoveredView = Option<BlueprintView>.None;
            m_viewBeingDragged = (Option<BlueprintView>)Option.None;
            m_insertPanel.Hide();
            UpdateItems(CurrentFolder.Folders, CurrentFolder.Blueprints);
            ClearSelectedItem();
        }

        public void OnDragMove(Vector2 screenPoint) {
            if (m_viewBeingDragged.IsNone || !IsLocal) return;
            BlueprintView parent = m_viewsCache.AllExistingOnes().FirstOrDefault(x => x != m_viewBeingDragged && (RectTransformUtility.RectangleContainsScreenPoint(x.RectTransform, screenPoint) || RectTransformUtility.RectangleContainsScreenPoint(x.RectTransform, screenPoint - new Vector2(2f, 0.0f))));
            if (parent == null) {
                m_lastHoveredView.ValueOrNull?.SetHovered(false);
                m_lastHoveredView = (Option<BlueprintView>)Option.None;
                m_insertPanel.Hide();
                m_indexToMoveTo = -1;
            }
            else {
                float num1 = screenPoint.x - parent.RectTransform.position.x;
                float num2 = (num1 - parent.RectTransform.rect.width * parent.RectTransform.lossyScale.x).Abs();
                bool flag1 = m_viewBeingDragged.Value.BlueprintsFolder.HasValue && parent.BlueprintsFolder.HasValue || m_viewBeingDragged.Value.Blueprint.HasValue && parent.Blueprint.HasValue;
                if (CurrentFolder.ParentFolder.HasValue && parent == m_viewsCache.AllExistingOnes().First()) flag1 = false;
                int num3 = parent.BlueprintsFolder.HasValue ? 20 : 60;
                bool flag2 = num1 < num3;
                bool flag3 = num2 < num3;
                if (flag1 && flag2 | flag3) {
                    m_lastHoveredView.ValueOrNull?.SetHovered(false);
                    m_lastHoveredView = (Option<BlueprintView>)Option.None;
                    m_indexToMoveTo = m_viewsCache.AllExistingOnes().IndexOf(parent);
                    if (CurrentFolder.ParentFolder.HasValue) --m_indexToMoveTo;
                    if (flag2) m_insertPanel.PutToLeftOf(parent, m_insertPanel.GetWidth(), Offset.Left(-m_insertPanel.GetWidth())).Show();
                    else {
                        m_insertPanel.PutToRightOf(parent, m_insertPanel.GetWidth(), Offset.Right(-m_insertPanel.GetWidth())).Show();
                        ++m_indexToMoveTo;
                    }
                    m_insertPanel.SetParent(m_gridContainer);
                    m_insertPanel.SendToFront();
                }
                else {
                    m_indexToMoveTo = -1;
                    m_insertPanel.Hide();
                    if (parent.BlueprintsFolder.IsNone) {
                        m_lastHoveredView.ValueOrNull?.SetHovered(false);
                        m_lastHoveredView = (Option<BlueprintView>)Option.None;
                    }
                    else {
                        if (parent == m_lastHoveredView || !(parent != m_lastHoveredView)) return;
                        m_lastHoveredView.ValueOrNull?.SetHovered(false);
                        m_lastHoveredView = (Option<BlueprintView>)parent;
                        parent.SetHovered(true);
                    }
                }
            }
        }
    }
}