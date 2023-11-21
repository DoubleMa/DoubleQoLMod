using DoubleQoL.Config;
using DoubleQoL.Extensions;
using DoubleQoL.Global;
using DoubleQoL.Global.Utils;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintsTabs"/>.
    /// </summary>
    internal abstract class BlueprintsView : Tab, IInputUpdate {
        public static readonly int SPACING = 2;
        public static readonly int COLUMNS_COUNT = 5;
        public static readonly Vector2 SIZE = new Vector2(BlueprintView.WIDTH * COLUMNS_COUNT + (COLUMNS_COUNT - 1) * SPACING + 10 + 16, 630f);

        private readonly IUnityInputMgr _inputMgr;
        private readonly LazyResolve<BlueprintsController> _controller;
        internal readonly QoLBlueprintsLibrary _blueprintsLibrary;
        private readonly UnlockedProtosDbForUi _unlockedProtosDb;
        private readonly BlueprintCreationController _blueprintCreationController;
        private readonly CaptainOfficeManager _captainOfficeManager;
        private readonly BlueprintsWindowView _parent;
        private DeleteBlueprintConfirmDialog _confirmDeleteDialog;
        private BlueprintCopyPasteDialog _blueprintCopyPasteDialog;
        private BlueprintDescriptionDialog _blueprintDescDialog;
        private ViewsCacheHomogeneous<BlueprintView> _viewsCache;
        private GridContainer _gridContainer;
        private ScrollableContainer _scrollableContainer;
        private Panel _insertPanel;
        private Panel _officeNotAvailable;
        private Txt _locationView;
        private Txt _officeNotAvailableTxt;
        private TxtField _searchField;
        private readonly string _captainOfficeName;
        private readonly BlueprintItemConditions _selectedItem;
        private Option<BlueprintView> _viewBeingDragged;
        private Option<BlueprintView> _lastHoveredView;
        private Option<IBlueprintItem> _newItem;
        internal Option<ServerInfo> _server;
        private bool Loading { get; set; }
        private bool ErrorLoading { get; set; }
        private string ErrorMessage { get; set; }
        private readonly bool showError = false;
        public abstract bool IsLocal { get; }
        private IBlueprintsFolder CurrentFolder { get; set; }
        private int _indexToMoveTo;
        internal StupidBlueprintManager BlueprintManager;

        private StackContainer _rightTopBar;
        private Btn _placeItBtn;
        private Btn _infoBtn;
        private Btn _toStrBtn;
        private Btn _updateDescBtn;
        private Btn _deleteBtn;
        private Btn _downloadBtn;

        private readonly Dictionary<Btn, Tooltip> buttonTooltips;

        internal BlueprintsView(IUnityInputMgr inputMgr,

            LazyResolve<BlueprintsController> controller,
            NewInstanceOf<QoLBlueprintsLibrary> blueprintsLibrary,
            UnlockedProtosDbForUi unlockedProtosDb,
            ProtosDb protosDb,
            BlueprintCreationController blueprintCreationController,
            CaptainOfficeManager captainOfficeManager,
            BlueprintsWindowView parent) : base("Blueprints") {
            _blueprintsLibrary = blueprintsLibrary.Instance;
            buttonTooltips = new Dictionary<Btn, Tooltip>();
            BlueprintManager = new StupidBlueprintManager(_blueprintsLibrary.ConvertItemToString);
            _selectedItem = new BlueprintItemConditions(BlueprintManager);
            _parent = parent;
            _inputMgr = inputMgr;
            _controller = controller;
            if (!IsLocal) _blueprintsLibrary.InvokeMethod("TestOnly_DisableSerialization");
            else CurrentFolder = _blueprintsLibrary.Root;
            _unlockedProtosDb = unlockedProtosDb;
            _blueprintCreationController = blueprintCreationController;
            _captainOfficeManager = captainOfficeManager;
            _captainOfficeName = protosDb.All<CaptainOfficeProto>().FirstOrDefault(x => x.Upgrade.PreviousTier.IsNone)?.Strings.Name.TranslatedString ?? "";
            ShowAfterSync = true;
        }

        public void SetServer(ServerInfo server) {
            if (IsLocal) return;
            _server = server;
            _selectedItem.SetServer(server);
            LoadData();
        }

        private void LoadData() {
            Task.Run(async () => {
                Loading = true;
                ErrorLoading = !(await LoadDataAsync());
                Loading = false;
            });
        }

        private async Task<bool> LoadDataAsync() {
            try {
                _ = _blueprintsLibrary.Root;
                Loading = true;
                var respond = await _server.Value.GetAllBlueprints();
                if (!respond.IsSuccess) throw new Exception(respond.Error);
                HandleRespond(respond.Data);
                return true;
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
                Logging.Log.Warning($"Error: {ex.Message}");
            }
            finally {
                _blueprintsLibrary.SaveCurrentRoot();
                CurrentFolder = _blueprintsLibrary.Root;
            }
            return false;
        }

        private void HandleRespond(BlueprintService.BlueprintsData data) {
            _searchField.ClearInput();
            _blueprintsLibrary.Clear();
            BlueprintManager.Clear();
            foreach (var blueprint in data.Blueprints) {
                if (!BlueprintManager.GetRootItem(blueprint.Owner_id, out var folder)) {
                    folder = _blueprintsLibrary.AddNewFolder(_blueprintsLibrary.Root);
                    folder.InvokeSetter("Name", data.Owners.FirstOrDefault(owner => owner.Key == blueprint.Owner_id + "").Value);
                    BlueprintManager.AddRoot(folder, blueprint.Owner_id);
                }
                if (_blueprintsLibrary.TryAddBlueprintFromString((IBlueprintsFolder)folder, blueprint.Data, out var result)) BlueprintManager.AddItem(result, blueprint);
            }
        }

        protected override void BuildUi() {
            buttonTooltips.Clear();
            Panel mainPanel = Builder.NewPanel("main", this).SetSize(SIZE);
            _confirmDeleteDialog = new DeleteBlueprintConfirmDialog(Builder, OnDeleteConfirm);
            _blueprintCopyPasteDialog = new BlueprintCopyPasteDialog(Builder, OnStringImportClick);
            if (!IsLocal) _blueprintCopyPasteDialog.SetMessage(new LocStrFormatted("Failed to parse and upload the string"));
            _blueprintDescDialog = new BlueprintDescriptionDialog(Builder, OnDescriptionApply);
            _viewsCache = new ViewsCacheHomogeneous<BlueprintView>(() => new BlueprintView(_gridContainer, this, _unlockedProtosDb, Builder, OnItemSelected, OnBlueprintDoubleClick, IsLocal));
            int num1 = 30;
            int num2 = 52;
            StackContainer leftTopOf = Builder.NewStackContainer("TopLeftBar").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(10f).PutToLeftTopOf(mainPanel, new Vector2(0.0f, 30f), Offset.All(10f));
            _searchField = Builder.NewTxtField("searchfield").SetPlaceholderText("Search").EnableSelectionOnFocus().SetOnValueChangedAction(StartSearch).SetVisibility(!IsLocal).AppendTo(leftTopOf, num2 * 5);
            Btn fromSelectionBtn = Builder.NewBtnPrimary("FromSelection", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.SelectArea_svg, Offset.All(4f)).AddToolTip(Tr.Blueprint_NewFromSelectionTooltip).OnClick(() => _blueprintCreationController.ActivateForSelection(OnNewBlueprintRequested)).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn newFolderBtn = Builder.NewBtnGeneral("NewFolder", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.NewFolder_svg, Offset.All(4f)).AddToolTip(Tr.NewFolder__Tooltip).OnClick(CreateNewFolder).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn fromStrBtn = Builder.NewBtnGeneral("FromString", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.ImportFromString_svg, Offset.All(4f)).AddToolTip(Tr.Blueprint_NewFromStringTooltip).OnClick(() => _blueprintCopyPasteDialog.ShowForStringImport()).SetVisibility(IsLocal).AppendTo(leftTopOf, num2);
            Btn uploadBtn = Builder.NewBtnGeneral("UploadString", leftTopOf).SetIcon(IconPaths.General_Upload, Offset.All(4f)).AddToolTip("Upload a blueprint or folder").OnClick(() => _blueprintCopyPasteDialog.ShowForStringUpload()).SetVisibility(!IsLocal).AppendTo(leftTopOf, num2);
            Btn refreshBtn = Builder.NewBtnGeneral("Refresh", leftTopOf).SetIcon(Assets.Unity.UserInterface.General.Repeat_svg, Offset.All(4f)).AddToolTip("Refresh Store").OnClick(() => LoadData()).SetVisibility(!IsLocal).SendToFront().AppendTo(leftTopOf, num2);
            _rightTopBar = Builder.NewStackContainer("TopRightBar").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(10f).PutToRightTopOf(mainPanel, new Vector2(0.0f, 30f), Offset.All(10f));
            _placeItBtn = Builder.NewBtnPrimary("Place").SetIcon(Assets.Unity.UserInterface.General.Build_svg).OnClick(() => {
                if (_selectedItem.View.HasValue && !_selectedItem.IsLocked && _selectedItem.IsBlueprint) _controller.Value.StartBlueprintPlacement(_selectedItem.View.Value.Blueprint.Value);
            }).AppendTo(_rightTopBar, num2);

            _toStrBtn = Builder.NewBtnGeneral("ToString").SetIcon(Assets.Unity.UserInterface.General.ExportToString_svg).OnClick(ExportSelectedItemToString).AppendTo(_rightTopBar, num2);
            _updateDescBtn = Builder.NewBtnGeneral("UpdateDesc").SetIcon(Assets.Unity.UserInterface.General.EditDescription_svg).SetEnabled(IsLocal).OnClick(RequestDescriptionEdit).AppendTo(_rightTopBar, num2);
            _deleteBtn = Builder.NewBtnDanger("Delete").SetIcon(Assets.Unity.UserInterface.General.Trash128_png).OnClick(StartBlueprintDelete).AppendTo(_rightTopBar, num2);
            _downloadBtn = Builder.NewBtnGeneral("Download").SetIcon(IconPaths.General_Download).AddToolTip("Download the blueprint or folder").OnClick(DownloadSelectedItem).SetVisibility(!IsLocal).AppendTo(_rightTopBar, num2);
            _infoBtn = Builder.NewBtnGeneral("Info").SetIcon(Assets.Unity.UserInterface.General.Info128_png).SetEnabled(false).AppendTo(_rightTopBar, num2);

            GetOrAddTooltip(_placeItBtn).SetText(Tr.Blueprint_PlaceItTooltip);
            GetOrAddTooltip(_toStrBtn).SetText(Tr.Blueprint_ExportToStringTooltip);
            GetOrAddTooltip(_updateDescBtn).SetText(IsLocal ? Tr.UpdateDescription__Tooltip.ToString() : "Update your blueprint or folder (Coming soon)");
            GetOrAddTooltip(_downloadBtn).SetText("Download the blueprint or folder");

            Builder.NewTxt("ExampleText").SetTextStyle(Builder.Style.Global.BoldText.Extend(color: Builder.Style.Global.OrangeText, fontSize: 15)).SetAlignment(TextAnchor.MiddleCenter).SetText("This is an example server and will be removed eventually").AddOutline().SetVisibility(_server.Value?.Url.Contains("examplestore") == true).PutToTopOf(mainPanel, 20f, Offset.Top(50f) + Offset.Left(30f));
            _locationView = Builder.NewTxt("Navigation").SetTextStyle(Builder.Style.Global.TextMediumBold).SetAlignment(TextAnchor.MiddleLeft).PutToTopOf(mainPanel, 20f, Offset.Top(50f) + Offset.Left(30f));
            Builder.NewPanel("Home").SetBackground(Assets.Unity.UserInterface.General.Home_svg).OnClick(() => CurrentFolder = _blueprintsLibrary.Root).PutToLeftOf(_locationView, 18f, Offset.Left(-22f));
            Txt _saveStatusTxt = Builder.NewTxt("Status", mainPanel).SetTextStyle(Builder.Style.Global.TextMediumBold).SetAlignment(TextAnchor.MiddleLeft).SetVisibility(IsLocal).PutToRightTopOf(mainPanel, new Vector2(0.0f, 20f), Offset.Top(50f) + Offset.Right(41f));
            Tooltip statusTooltip = Builder.AddTooltipFor(Builder.NewPanel("Info", _saveStatusTxt).SetBackground(Assets.Unity.UserInterface.General.Info128_png).SetVisibility(IsLocal).PutToRightMiddleOf(_saveStatusTxt, 15.Vector2(), Offset.Right(-18f)));
            _scrollableContainer = Builder.NewScrollableContainer("ScrollableContainer").AddVerticalScrollbar().PutTo(mainPanel, Offset.Top(num1 + 40));
            _gridContainer = Builder.NewGridContainer("Container").SetCellSize(BlueprintView.SIZE).SetCellSpacing(SPACING).SetDynamicHeightMode(COLUMNS_COUNT).SetInnerPadding(Offset.All(5f));
            _scrollableContainer.AddItemTopLeft(_gridContainer);
            _officeNotAvailable = Builder.NewPanel("NotAvailableOverlay", mainPanel).SetBackground(new ColorRgba(2894116, 240)).PutTo(mainPanel).Hide();
            _officeNotAvailable.OnClick(() => { if (!IsLocal && ErrorLoading) LoadData(); });
            _officeNotAvailableTxt = Builder.NewTxt("Text", _officeNotAvailable).SetTextStyle(Builder.Style.Global.TextMediumBold.Extend(color: Builder.Style.Global.OrangeText, fontSize: 18)).SetAlignment(TextAnchor.MiddleCenter).SetText(Tr.Blueprints_BuildingRequired.Format(_captainOfficeName)).AddOutline().PutTo(_officeNotAvailable, Offset.LeftRight(10f));
            ColorRgba colorRgba = 12158750;
            _insertPanel = Builder.NewPanel("LeftEdge", _gridContainer).SetBackground(colorRgba).SetWidth(2f).Hide();
            Builder.NewIconContainer("LeftArrow", _insertPanel).SetIcon(Assets.Unity.UserInterface.General.Next_svg, colorRgba).PutToLeftOf(_insertPanel, 22f, Offset.Left(-32f)).AddOutline();
            Builder.NewIconContainer("RightArrow", _insertPanel).SetIcon(Assets.Unity.UserInterface.General.Return_svg, colorRgba).PutToRightOf(_insertPanel, 22f, Offset.Right(-32f)).AddOutline();
            Builder.NewTxt("ExampleText2").SetTextStyle(Builder.Style.Global.BoldText.Extend(color: Builder.Style.Global.OrangeText, fontSize: 15)).SetAlignment(TextAnchor.MiddleCenter).SetText("This is an example server and will be removed eventually").AddOutline().SetVisibility(_server.Value?.Url.Contains("examplestore") == true).PutToBottomOf(mainPanel, 16f);

            mainPanel.OnClick(() => {
                _selectedItem.View.ValueOrNull?.CommitRenamingIfCan();
                ClearSelectedItem();
            });
            mainPanel.PutTo(this);
            this.SetSize(SIZE);
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            updaterBuilder.Observe(() => CurrentFolder?.Folders, CompareFixedOrder<IBlueprintsFolder>.Instance).Observe(() => CurrentFolder?.Blueprints, CompareFixedOrder<IBlueprint>.Instance).Do(UpdateItems);
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
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.LoadingInProgress:
                        _saveStatusTxt.SetText(Tr.LoadInProgress);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.LoadFailedDueToFormat:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToLoad);
                        str1 = Tr.BlueprintLibStatus__FailedToLoadOnFormat.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.LoadFailedNoAccess:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToLoad);
                        str1 = Tr.BlueprintLibStatus__FailedToLoadOnPermission.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.LoadSuccess:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveInProgress:
                        _saveStatusTxt.SetText(Tr.SaveInProgress);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveFailed:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToSave);
                        str1 = Tr.BlueprintLibStatus__FailedToSaveTooltip.TranslatedString;
                        setColor(dangerClr);
                        break;

                    case BlueprintsLibrary.Status.SaveDone:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__Synchronized);
                        setColor(color2);
                        break;

                    case BlueprintsLibrary.Status.SaveDoneBackupFailed:
                        _saveStatusTxt.SetText(Tr.BlueprintLibStatus__FailedToBackup);
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
                _saveStatusTxt.SetWidth(_saveStatusTxt.GetPreferedWidth());
            });
            updaterBuilder.Observe(() => _captainOfficeManager.CaptainOffice).Observe(() => _captainOfficeManager.IsOfficeActive).Observe(() => Loading).Do((office, isActive, loading) => {
                _officeNotAvailable.SetVisibility(!isActive || loading || ErrorLoading);
                if (!isActive) {
                    string str = office.HasValue ? office.Value.Prototype.Strings.Name.TranslatedString : _captainOfficeName;
                    _officeNotAvailableTxt.SetText(Tr.Blueprints_BuildingRequired.Format(str));
                }
                else if (loading) {
                    _officeNotAvailableTxt.SetText("Loading blueprints from the server");
                }
                else if (ErrorLoading) {
                    _officeNotAvailableTxt.SetText($"Error Loading blueprints from the '{_server.Value?.Name}' server, please try again later" + (showError ? $"\n{ErrorMessage}" : ""));
                }
            });
            AddUpdater(updaterBuilder.Build());
            AddUpdater(_viewsCache.Updater);
            void setColor(ColorRgba color) => _saveStatusTxt.SetColor(color);
        }

        public Tooltip GetOrAddTooltip(Btn button) {
            if (buttonTooltips.ContainsKey(button)) return buttonTooltips[button];
            Tooltip newTooltip = button.AddToolTipAndReturn();
            buttonTooltips.Add(button, newTooltip);
            return newTooltip;
        }

        private void RequestDescriptionEdit() {
            if (!_selectedItem.HasValue) return;
            _blueprintDescDialog.ShowForEdit(_selectedItem.View.Value.Item.ValueOrNull?.Desc ?? "");
        }

        private void OnNewBlueprintRequested(ImmutableArray<EntityConfigData> data) {
            _newItem = _blueprintsLibrary.AddBlueprint(CurrentFolder, data).As<IBlueprintItem>();
            _inputMgr.ActivateNewController(_controller.Value);
        }

        private void CreateNewFolder() => _newItem = _blueprintsLibrary.AddNewFolder(CurrentFolder).CreateOption().As<IBlueprintItem>();

        private void UpdateItems(IIndexable<IBlueprintsFolder> folders, IIndexable<IBlueprint> blueprints) {
            ClearSelectedItem();
            _gridContainer.StartBatchOperation();
            _gridContainer.ClearAll();
            _viewsCache.ReturnAll();
            if (CurrentFolder.ParentFolder.HasValue) {
                BlueprintView view = _viewsCache.GetView();
                view.SetBlueprintsFolder(CurrentFolder.ParentFolder.Value, true);
                _gridContainer.Append(view);
            }
            foreach (IBlueprintsFolder folder in folders) {
                BlueprintView view = _viewsCache.GetView();
                view.SetBlueprintsFolder(folder);
                _gridContainer.Append(view);
            }
            foreach (IBlueprint blueprint in blueprints) {
                BlueprintView view = _viewsCache.GetView();
                view.SetBlueprint(blueprint);
                _gridContainer.Append(view);
            }
            _gridContainer.FinishBatchOperation();
            _locationView.SetVisibility(CurrentFolder.ParentFolder.HasValue);
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
            _locationView.SetText(text);
        }

        private void OnItemSelected(BlueprintView blueprintView) {
            ClearSelectedItem();
            _selectedItem.SetView(blueprintView);
            blueprintView.SetIsSelected(true);
            UpdateRightBars();
            _placeItBtn.SetEnabled(!_selectedItem.IsLocked);
        }

        private void ClearSelectedItem() {
            _selectedItem.View.ValueOrNull?.CancelRenamingIfCan();
            _selectedItem.View.ValueOrNull?.SetIsSelected(false);
            _selectedItem.SetView(null);
            UpdateRightBars();
        }

        private void UpdateRightBars() {
            _rightTopBar.SetVisibility(_selectedItem.HasValue && !_selectedItem.IsParentFolder);
            if (!_selectedItem.HasValue) return;
            _rightTopBar.SetItemVisibility(_placeItBtn, _selectedItem.IsBlueprint);
            _rightTopBar.SetItemVisibility(_infoBtn, _selectedItem.IsBlueprint);
            GetOrAddTooltip(_infoBtn).SetText(_selectedItem.IsBlueprint ? Tr.MadeInVersion.Format(_selectedItem.View.Value.Blueprint.Value.GameVersion ?? "").Value : "");
            UpdateRightBarsForStore();
        }

        private void UpdateRightBarsForStore() {
            if (IsLocal) return;
            _rightTopBar.SetItemVisibility(_toStrBtn, false);
            _rightTopBar.SetItemVisibility(_updateDescBtn, !_selectedItem.IsRoot && _selectedItem.IsOwned);
            _rightTopBar.SetItemVisibility(_deleteBtn, !_selectedItem.IsRoot && _selectedItem.IsOwned);
            _rightTopBar.SetItemVisibility(_downloadBtn, !_selectedItem.IsRoot && !_selectedItem.IsOwned);
        }

        private void OnBlueprintDoubleClick(BlueprintView blueprintView) {
            if (blueprintView.Blueprint.HasValue) _controller.Value.StartBlueprintPlacement(blueprintView.Blueprint.Value);
            else if (blueprintView.BlueprintsFolder.HasValue) CurrentFolder = blueprintView.BlueprintsFolder.Value;
        }

        private void ExportSelectedItemToString() {
            if (!_selectedItem.HasValue) return;
            if (_selectedItem.IsBlueprint) _blueprintCopyPasteDialog.ShowForStringExport(_blueprintsLibrary.ConvertToString(_selectedItem.View.Value.Blueprint.Value));
            else if (_selectedItem.IsFolder) _blueprintCopyPasteDialog.ShowForStringExport(_blueprintsLibrary.ConvertToString(_selectedItem.View.Value.BlueprintsFolder.Value));
        }

        private void StartBlueprintDelete() {
            if (!_selectedItem.HasValue || (!IsLocal && !_selectedItem.IsOwned)) return;
            if ((_selectedItem.View.Value.Item.ValueOrNull is IBlueprintsFolder valueOrNull) && valueOrNull.IsEmpty) OnDeleteConfirm();
            else _confirmDeleteDialog.SetNameAndShow(_selectedItem.View.Value.Title);
        }

        private void DownloadSelectedItem() {
            if (IsLocal || !_selectedItem.HasValue) return;
            var bl = _parent.BlueprintsLocalView._blueprintsLibrary;
            if ((_selectedItem.View.Value.Item.ValueOrNull is IBlueprintItem valueOrNull)) bl.TryAddBlueprintFromString(bl.Root, _blueprintsLibrary.ConvertItemToString(valueOrNull), out _);
        }

        private bool OnStringImportClick(string data) {
            if (string.IsNullOrEmpty(data)) return false;
            if (IsLocal) return _blueprintsLibrary.TryAddBlueprintFromString(CurrentFolder, data, out _);
            return StartUpload(data);
        }

        private bool StartUpload(string data) {
            try {
                if (string.IsNullOrEmpty(data) || !_blueprintsLibrary.TryParseFromString(data, out _, out var result)) return false;
                Task.Run(async () => {
                    var response = await _server.Value.UploadBlueprint(data);
                    if (response.IsSuccess) {
                        LoadData();
                        _blueprintCopyPasteDialog.OnUploadSuccess();
                    }
                    else _blueprintCopyPasteDialog.OnUploadFailed(response.Error);
                });
                return true;
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error: {ex.Message}");
            }
            return false;
        }

        private void OnDeleteConfirm() {
            if (!_selectedItem.HasItem) return;
            if (IsLocal) _blueprintsLibrary.DeleteItem(CurrentFolder, _selectedItem.Item);
            else if (!_selectedItem.IsOwned) return;
            try {
                Task.Run(async () => {
                    var response = await _server.Value.DeleteBlueprint(_selectedItem.Data.Id + "");
                    if (response.IsSuccess) LoadData(); // need to change this to update the Local data without a new request
                });
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error: {ex.Message}");
            }
        }

        internal void OnLike(BlueprintItemConditions item) {
            if (item.IsOwned) return;
            try {
                Task.Run(async () => {
                    var response = await _server.Value.LikeBlueprint(item.Data.Id + "");
                    if (response.IsSuccess) BlueprintManager.Like(item.Item, response.Data.Liked ? 1 : -1);
                });
            }
            catch (Exception ex) {
                Logging.Log.Warning($"Error: {ex.Message}");
            }
        }

        private void OnDescriptionApply(string newDescription) {
            if (!IsLocal || !_selectedItem.HasItem) return;
            _blueprintsLibrary.SetDescription(_selectedItem.Item, newDescription);
            _selectedItem.View.Value.UpdateDesc();
        }

        private void StartSearch() {
            if (IsLocal) return;
            var text = _searchField.GetText();
            if (string.IsNullOrEmpty(text)) {
                _blueprintsLibrary.RestoreSavedRoot();
                return;
            }
            _blueprintsLibrary.UpdateRoot(_blueprintsLibrary.SearchItems(text)); //this is stupid I need to find a better way
            CurrentFolder = _blueprintsLibrary.Root;
        }

        public bool InputUpdate(IInputScheduler inputScheduler) {
            if (IsVisible && _newItem.HasValue) {
                foreach (BlueprintView existingOne in _viewsCache.AllExistingOnes()) {
                    if (_newItem == existingOne.Item) {
                        OnItemSelected(existingOne);
                        existingOne.StartRenamingSession();
                        _newItem = Option.None;
                        _scrollableContainer.ScrollToElement(existingOne.RectTransform);
                        return true;
                    }
                }
            }
            if (_blueprintCopyPasteDialog.IsVisible() && _blueprintCopyPasteDialog.InputUpdate()) return true;
            if (_confirmDeleteDialog.IsVisible() && _confirmDeleteDialog.InputUpdate()) return true;
            if (InputHelper.IsControlDown() && Input.GetKeyDown(KeyCode.C)) {
                if (_selectedItem.HasItem) {
                    GUIUtility.systemCopyBuffer = _blueprintsLibrary.ConvertItemToString(_selectedItem.Item);
                    return true;
                }
            }
            if (InputHelper.IsControlDown()) {
                if (Input.GetKeyDown(KeyCode.C) && _selectedItem.HasItem) {
                    GUIUtility.systemCopyBuffer = _blueprintsLibrary.ConvertItemToString(_selectedItem.Item);
                    return true;
                }
                else if (_parent.IsTabActive(this)) {
                    if (Input.GetKeyDown(KeyCode.V)) {
                        var toAdd = GUIUtility.systemCopyBuffer;
                        if (!string.IsNullOrEmpty(toAdd)) {
                            if (IsLocal) _blueprintsLibrary.TryAddBlueprintFromString(CurrentFolder, toAdd, out _);
                            else StartUpload(toAdd);
                            return true;
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.R)) {
                        LoadData();
                        return true;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (_blueprintCopyPasteDialog.IsVisible()) {
                    _blueprintCopyPasteDialog.Hide();
                    return true;
                }
                if (_confirmDeleteDialog.IsVisible()) {
                    _confirmDeleteDialog.Hide();
                    return true;
                }
                if (_blueprintDescDialog.IsVisible()) {
                    _blueprintDescDialog.Hide();
                    return true;
                }
                foreach (BlueprintView existingOne in _viewsCache.AllExistingOnes()) if (existingOne.CancelRenamingIfCan()) return true;
            }
            if (_blueprintCopyPasteDialog.IsVisible() || _confirmDeleteDialog.IsVisible() || _blueprintDescDialog.IsVisible()) return true;
            if ((Input.GetKeyDown(KeyCode.Backspace) || Input.GetMouseButtonDown(3)) && CurrentFolder.ParentFolder.HasValue) {
                CurrentFolder = CurrentFolder.ParentFolder.Value;
                return true;
            }
            if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
                foreach (BlueprintView existingOne in _viewsCache.AllExistingOnes()) if (existingOne.CommitRenamingIfCan()) return true;
            if (_selectedItem.HasValue) {
                if (Input.GetKeyDown(KeyCode.F2)) {
                    _selectedItem.View.Value.StartRenamingSession();
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
            _viewBeingDragged = (Option<BlueprintView>)viewDragged;
            _viewBeingDragged.Value.SetScale(new Vector2(0.7f, 0.7f));
            _viewBeingDragged.Value.SetParent(this);
        }

        public void OnDragDone() {
            if (_viewBeingDragged.IsNone || !IsLocal) return;
            if (_indexToMoveTo >= 0) {
                _blueprintsLibrary.TryReorderItem(_viewBeingDragged.Value.Item.Value, CurrentFolder, _indexToMoveTo);
                _indexToMoveTo = -1;
            }
            else if (_lastHoveredView.HasValue) {
                _lastHoveredView.Value.SetHovered(false);
                Option<IBlueprintsFolder> blueprintsFolder = _lastHoveredView.Value.BlueprintsFolder;
                IBlueprintsFolder valueOrNull1 = blueprintsFolder.ValueOrNull;
                if (valueOrNull1 != null) {
                    IBlueprint valueOrNull2 = _viewBeingDragged.Value.Blueprint.ValueOrNull;
                    blueprintsFolder = _viewBeingDragged.Value.BlueprintsFolder;
                    IBlueprintsFolder valueOrNull3 = blueprintsFolder.ValueOrNull;
                    if (valueOrNull2 != null) _blueprintsLibrary.MoveBlueprint(valueOrNull2, CurrentFolder, valueOrNull1);
                    else if (valueOrNull3 != null) _blueprintsLibrary.MoveFolder(valueOrNull3, valueOrNull1);
                }
            }
            _lastHoveredView = Option<BlueprintView>.None;
            _viewBeingDragged = (Option<BlueprintView>)Option.None;
            _insertPanel.Hide();
            UpdateItems(CurrentFolder.Folders, CurrentFolder.Blueprints);
            ClearSelectedItem();
        }

        public void OnDragMove(Vector2 screenPoint) {
            if (_viewBeingDragged.IsNone || !IsLocal) return;
            BlueprintView parent = _viewsCache.AllExistingOnes().FirstOrDefault(x => x != _viewBeingDragged && (RectTransformUtility.RectangleContainsScreenPoint(x.RectTransform, screenPoint) || RectTransformUtility.RectangleContainsScreenPoint(x.RectTransform, screenPoint - new Vector2(2f, 0.0f))));
            if (parent == null) {
                _lastHoveredView.ValueOrNull?.SetHovered(false);
                _lastHoveredView = (Option<BlueprintView>)Option.None;
                _insertPanel.Hide();
                _indexToMoveTo = -1;
            }
            else {
                float num1 = screenPoint.x - parent.RectTransform.position.x;
                float num2 = (num1 - parent.RectTransform.rect.width * parent.RectTransform.lossyScale.x).Abs();
                bool flag1 = _viewBeingDragged.Value.BlueprintsFolder.HasValue && parent.BlueprintsFolder.HasValue || _viewBeingDragged.Value.Blueprint.HasValue && parent.Blueprint.HasValue;
                if (CurrentFolder.ParentFolder.HasValue && parent == _viewsCache.AllExistingOnes().First()) flag1 = false;
                int num3 = parent.BlueprintsFolder.HasValue ? 20 : 60;
                bool flag2 = num1 < num3;
                bool flag3 = num2 < num3;
                if (flag1 && flag2 | flag3) {
                    _lastHoveredView.ValueOrNull?.SetHovered(false);
                    _lastHoveredView = (Option<BlueprintView>)Option.None;
                    _indexToMoveTo = _viewsCache.AllExistingOnes().IndexOf(parent);
                    if (CurrentFolder.ParentFolder.HasValue) --_indexToMoveTo;
                    if (flag2) _insertPanel.PutToLeftOf(parent, _insertPanel.GetWidth(), Offset.Left(-_insertPanel.GetWidth())).Show();
                    else {
                        _insertPanel.PutToRightOf(parent, _insertPanel.GetWidth(), Offset.Right(-_insertPanel.GetWidth())).Show();
                        ++_indexToMoveTo;
                    }
                    _insertPanel.SetParent(_gridContainer);
                    _insertPanel.SendToFront();
                }
                else {
                    _indexToMoveTo = -1;
                    _insertPanel.Hide();
                    if (parent.BlueprintsFolder.IsNone) {
                        _lastHoveredView.ValueOrNull?.SetHovered(false);
                        _lastHoveredView = (Option<BlueprintView>)Option.None;
                    }
                    else {
                        if (parent == _lastHoveredView || !(parent != _lastHoveredView)) return;
                        _lastHoveredView.ValueOrNull?.SetHovered(false);
                        _lastHoveredView = (Option<BlueprintView>)parent;
                        parent.SetHovered(true);
                    }
                }
            }
        }

        internal class BlueprintItemConditions {
            public Option<BlueprintView> View;
            private readonly StupidBlueprintManager BlueprintManager;
            private ServerInfo Server;

            internal bool HasValue;
            internal bool IsLocked;
            internal bool IsParentFolder;
            internal bool IsFolder;
            internal bool IsBlueprint;
            internal bool IsRoot;
            internal bool IsUpload;
            internal bool IsUnknown;
            internal bool IsOwned;
            internal bool HasItem;
            internal IBlueprintItem Item;
            internal BlueprintService.BlueprintData Data;

            public BlueprintItemConditions(StupidBlueprintManager blueprintManager) {
                BlueprintManager = blueprintManager;
            }

            public BlueprintItemConditions(StupidBlueprintManager blueprintManager, ServerInfo server) {
                BlueprintManager = blueprintManager;
                Server = server;
            }

            public void SetServer(ServerInfo server) {
                Server = server;
            }

            public void SetView(BlueprintView view) {
                View = view;
                HasValue = View.HasValue;
                if (!HasValue) return;
                HasItem = View.Value.Item.HasValue;
                Item = View.Value.Item.Value;
                IsLocked = View.Value.IsLocked;
                IsParentFolder = View.Value.m_isParentFolder;
                IsFolder = View.Value.BlueprintsFolder.HasValue;
                IsBlueprint = View.Value.Blueprint.HasValue;
                if (Server != null && HasItem && (BlueprintManager.GetItemData(Item, out Data) != StupidBlueprintManager.ItemType.Unknown)) {
                    IsRoot = BlueprintManager.IsRoot(Item);
                    IsUpload = !IsRoot && BlueprintManager.IsUpload(Item);
                    IsUnknown = !IsRoot && !IsUpload;
                    IsOwned = !IsUnknown && !string.IsNullOrEmpty(Server.UserId) && Data?.Owner_id + "" == Server.UserId;
                }
                else {
                    IsRoot = false;
                    IsUpload = false;
                    IsOwned = false;
                    IsUnknown = true;
                }
            }

            //public override string ToString() => $"Name: {Item?.Name}, isLocked: {IsLocked}, isParentFolder: {IsParentFolder}, isFolder: {IsFolder}, isBlueprint: {IsBlueprint}, isRoot: {IsRoot}, isUpload: {IsUpload}, isUnknown: {IsUnknown}, isOwned: {IsOwned}";
            public override string ToString() {
                string test = $"Name: {Item?.Name}\n";
                test += $"isLocked: {IsLocked}\n";
                test += $"isParentFolder: {IsParentFolder}\n";
                test += $"isFolder: {IsFolder}\n";
                test += $"isBlueprint: {IsBlueprint}\n";
                test += $"isRoot: {IsRoot}\n";
                test += $"isUpload: {IsUpload}\n";
                test += $"isUnknown: {IsUnknown}\n";
                test += $"isOwned: {IsOwned}\n";
                return test;
            }
        }
    }
}