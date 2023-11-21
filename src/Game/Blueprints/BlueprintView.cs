using DoubleQoL.Global;
using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities.Blueprints;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UiFramework.Styles;
using Mafi.Unity.UserInterface;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DoubleQoL.Game.Blueprints {

    /// <summary>
    /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Blueprints.BlueprintView"/>.
    /// </summary>
    internal class BlueprintView : IUiElementWithUpdater, IUiElement {
        public static readonly int WIDTH = 160;
        public static readonly Vector2 SIZE = new Vector2(WIDTH, 160f);
        private static readonly ColorRgba BG_ICON_COLOR = 4348266;
        public GameObject GameObject => m_container.GameObject;

        public RectTransform RectTransform => m_container.RectTransform;
        public IUiUpdater Updater { get; }

        private readonly BlueprintsView m_owner;
        private readonly UnlockedProtosDbForUi m_unlockedProtosDb;
        private readonly ViewsCacheHomogeneous<IconWithCount> m_iconsCache;
        private readonly GridContainer m_gridContainer;
        private readonly Txt m_title;
        private readonly TxtField m_txtInput;
        private readonly Btn m_container;
        private readonly Btn m_textEditBtn;
        private readonly Btn m_textSaveBtn;
        private readonly IconContainer m_bg;
        private readonly IconContainer m_missingProtosIcon;
        private readonly IconContainer m_downgradeIcon;
        private readonly Panel m_hoveredBorder;
        private readonly Panel m_titleBar;
        private readonly Panel m_lockedOverlay;
        private readonly Panel m_selectedBorder;
        private readonly Tooltip m_descTooltip;
        private readonly Tooltip m_missingProtosTooltip;
        private readonly Tooltip m_downgradeIconTooltip;
        private readonly Tooltip m_lockedIconTooltip;
        internal bool m_isParentFolder;
        private bool m_isHovered;
        private bool m_isInRenamingSession;
        private readonly Set<Proto> m_lockedProtosCache;
        private readonly Dict<IProto, IProto> m_downgradesMap;
        private readonly StringBuilder m_sb;
        private readonly StackContainer likePanel;
        private readonly Txt m_likeTxt;
        private readonly Btn _likeBtn;
        public Option<IBlueprintItem> Item => ((IBlueprintItem)Blueprint.ValueOrNull ?? BlueprintsFolder.ValueOrNull).CreateOption();

        public Option<IBlueprint> Blueprint { get; private set; }

        public Option<IBlueprintsFolder> BlueprintsFolder { get; private set; }

        public bool IsLocked => m_lockedOverlay.IsVisible();

        public string Title => m_title.Text;
        private readonly bool IsLocal;
        private bool test = false;
        private BlueprintsView.BlueprintItemConditions itemConditions;

        public BlueprintView(IUiElement parent, BlueprintsView owner, UnlockedProtosDbForUi unlockedProtosDb, UiBuilder builder, Action<BlueprintView> onClick, Action<BlueprintView> onDoubleClick, bool isLocal) {
            IsLocal = isLocal;
            m_lockedProtosCache = new Set<Proto>();
            m_downgradesMap = new Dict<IProto, IProto>();
            m_sb = new StringBuilder();
            m_owner = owner;
            m_unlockedProtosDb = unlockedProtosDb;
            m_container = builder.NewBtn("Container", parent).SetButtonStyle(builder.Style.Global.ListMenuBtnDarker())
                .OnClick(() => onClick(this))
                .OnDoubleClick(() => { if (!m_lockedOverlay.IsVisible()) onDoubleClick(this); })
                .SetOnMouseEnterLeaveActions(() => {
                    m_isHovered = true;
                    m_textEditBtn.SetVisibility(isLocal && !m_isParentFolder && !m_isInRenamingSession);
                }, () => {
                    m_isHovered = false;
                    m_textEditBtn.Hide();
                });
            m_descTooltip = m_container.AddToolTipAndReturn();
            m_bg = builder.NewIconContainer("Bg").PutTo(m_container, Offset.All(20f));
            m_hoveredBorder = builder.NewPanel("border").PutTo(m_container).SetBorderStyle(new BorderStyle(10461087)).Hide();
            m_selectedBorder = builder.NewPanel("border").PutTo(m_container).SetBorderStyle(new BorderStyle(10123554)).Hide();
            m_gridContainer = builder.NewGridContainer("Container").SetCellSize(((WIDTH - 10) / 3 - 2).Vector2()).SetCellSpacing(2f).SetDynamicHeightMode(3).SetInnerPadding(Offset.All(5f) + Offset.Top(5f)).PutToLeftTopOf(this, Vector2.zero);
            m_iconsCache = new ViewsCacheHomogeneous<IconWithCount>(() => new IconWithCount(m_gridContainer, builder));
            m_lockedOverlay = builder.NewPanel("LockedOverlay").SetBackground(new ColorRgba(3355443, 120)).PutTo(m_container);
            IconContainer centerMiddleOf = builder.NewIconContainer("LockedIcon").SetIcon(Assets.Unity.UserInterface.General.Locked128_png, new ColorRgba(11447982, 200)).PutToCenterMiddleOf(m_lockedOverlay, 40.Vector2());
            m_lockedIconTooltip = builder.AddTooltipFor(centerMiddleOf);
            m_titleBar = builder.NewPanel("TitleBar", m_container).PutToCenterBottomOf(m_container, new Vector2(0.0f, 25f), Offset.LeftRight(5f) + Offset.Bottom(5f));
            m_title = builder.NewTxt(nameof(Title), m_titleBar).SetTextStyle(builder.Style.Global.TextMedium).SetAlignment(TextAnchor.LowerCenter).PutTo(m_titleBar, Offset.Right(20f));
            m_textEditBtn = builder.NewBtn("Rename", m_titleBar).SetButtonStyle(builder.Style.Global.IconBtnWhite()).SetIcon(Assets.Unity.UserInterface.General.Rename_svg, Offset.LeftRight(3f)).SetEnabled(isLocal).OnClick(StartRenamingSession).PutToRightBottomOf(m_titleBar, 18.Vector2()).Hide();
            likePanel = builder.NewStackContainer("likePanel").SetStackingDirection(StackContainer.Direction.LeftToRight).SetSizeMode(StackContainer.SizeMode.Dynamic).SetItemSpacing(2f).SetVisibility(false).PutToRightBottomOf(m_container, new Vector2(0.0f, 25f), Offset.Right(2f));
            m_likeTxt = builder.NewTxt(nameof(m_likeTxt), likePanel).SetTextStyle(builder.Style.Global.TextMedium).SetAlignment(TextAnchor.MiddleRight).AppendTo(likePanel).SetText("0");
            _likeBtn = builder.NewBtn("Star", likePanel).SetButtonStyle(builder.Style.Global.IconBtnBrightOrange()).SetIcon(IconPaths.General_Star, Offset.LeftRight(3f)).OnClick(() => { m_owner.OnLike(itemConditions); test = !test; }).AppendTo(likePanel, 25f);
            m_txtInput = builder.NewTxtField("SaveNameInput", m_container).SetStyle(builder.Style.Global.LightTxtFieldStyle).SetCharLimit(60).PutToBottomOf(m_container, 25f, Offset.Left(5f) + Offset.Right(25f) + Offset.Bottom(5f)).Hide();
            m_missingProtosIcon = builder.NewIconContainer("MissingProtosWarning").SetIcon(Assets.Unity.UserInterface.General.Warning128_png, builder.Style.Global.OrangeText).PutToRightTopOf(this, 22.Vector2(), Offset.TopRight(5f, 5f));
            m_missingProtosTooltip = builder.AddTooltipFor(m_missingProtosIcon);
            m_downgradeIcon = builder.NewIconContainer("Downgrade").SetIcon(Assets.Unity.UserInterface.General.Downgrade_svg, 12352540).PutToRightTopOf(this, 22.Vector2(), Offset.TopRight(5f, 5f));
            m_downgradeIconTooltip = builder.AddTooltipFor(m_downgradeIcon);
            m_textSaveBtn = builder.NewBtn("Save", m_txtInput).SetButtonStyle(builder.Style.Global.IconBtnWhite()).SetIcon(Assets.Unity.UserInterface.General.Save_svg, Offset.LeftRight(3f)).OnClick(commitRenamingSession).PutToRightOf(m_txtInput, 20f, Offset.Right(-22f)).Hide();
            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();
            updaterBuilder.Observe(() => itemConditions?.Data?.Like_count).Do((e) => { if (e != null) m_likeTxt.SetText(e + ""); });
            Updater = updaterBuilder.Build();
            if (IsLocal) m_container.SetupDragDrop(onBeginDrag, onDrag, onEndDrag);
        }

        public void SetBlueprint(IBlueprint blueprint) {
            m_isParentFolder = false;
            this.SetScale(1.Vector2());
            m_container.SetDragEnabled(IsLocal);
            BlueprintsFolder = Option.None;
            Blueprint = blueprint.CreateOption();
            updateData();
            updateTitle();
            UpdateDesc();
            m_bg.SetIcon(Assets.Unity.UserInterface.General.Blueprint_svg, BG_ICON_COLOR).PutTo(m_container, Offset.All(20f));
            m_lockedProtosCache.Clear();
            m_downgradesMap.Clear();
            m_sb.Clear();
            if (blueprint.ProtosThatFailedToLoad.HasValue) m_missingProtosTooltip.SetText(string.Format("{0}\n\n{1}\n{2}", Tr.BlueprintContentMissing__Info, Tr.BlueprintContentMissing__ListTitle, 1).FormatInvariant(blueprint.GameVersion, blueprint.ProtosThatFailedToLoad.Value));
            else {
                foreach (Proto allDistinctProto in blueprint.AllDistinctProtos) {
                    if (!(allDistinctProto == null) && !m_unlockedProtosDb.IsUnlocked(allDistinctProto)) {
                        Option<Proto> unlockedDowngradeFor = m_unlockedProtosDb.GetNearestUnlockedDowngradeFor(allDistinctProto);
                        if (unlockedDowngradeFor.HasValue) m_downgradesMap[allDistinctProto] = unlockedDowngradeFor.Value;
                        else m_lockedProtosCache.Add(allDistinctProto);
                    }
                }
                if (m_lockedProtosCache.IsNotEmpty) {
                    m_sb.AppendLine(Tr.BlueprintProtosLocked__NotAvailable.TranslatedString);
                    foreach (Proto proto in m_lockedProtosCache) {
                        m_sb.Append(" - ");
                        m_sb.AppendLine(proto.Strings.Name.TranslatedString);
                    }
                    m_lockedIconTooltip.SetText(m_sb.ToString());
                }
                else if (m_downgradesMap.IsNotEmpty) {
                    m_sb.AppendLine(Tr.BlueprintProtosLocked__CanDowngrade.TranslatedString);
                    foreach (KeyValuePair<IProto, IProto> downgrades in m_downgradesMap) {
                        m_sb.Append(" - ");
                        m_sb.Append(downgrades.Key.Strings.Name.TranslatedString);
                        m_sb.Append(" -> ");
                        m_sb.AppendLine(downgrades.Value.Strings.Name.TranslatedString);
                    }
                    m_downgradeIconTooltip.SetText(m_sb.ToString());
                }
            }
            m_lockedOverlay.SetVisibility(m_lockedProtosCache.IsNotEmpty);
            m_missingProtosIcon.SetVisibility(blueprint.ProtosThatFailedToLoad.HasValue);
            m_downgradeIcon.SetVisibility(m_downgradesMap.IsNotEmpty && !m_lockedOverlay.IsVisible() && !m_missingProtosIcon.IsVisible());
            m_gridContainer.StartBatchOperation();
            m_gridContainer.ClearAll();
            m_iconsCache.ReturnAll();
            int i = 0;
            foreach (KeyValuePair<Proto, int> mostFrequentProto in blueprint.MostFrequentProtos) {
                if (i < 6) {
                    Proto key = mostFrequentProto.Key;
                    if (key is LayoutEntityProto layoutEntityProto) {
                        IconWithCount view = m_iconsCache.GetView();
                        m_gridContainer.Append(view);
                        view.SetIconAndCount(layoutEntityProto.Graphics.IconPath, mostFrequentProto.Value);
                        ++i;
                    }
                    else if (key is TransportProto transportProto) {
                        IconWithCount view = m_iconsCache.GetView();
                        m_gridContainer.Append(view);
                        view.SetIconAndCount(transportProto.Graphics.IconPath, mostFrequentProto.Value);
                    }
                }
                else break;
            }
            m_gridContainer.FinishBatchOperation();
        }

        public void SetBlueprintsFolder(IBlueprintsFolder folder, bool isParentFolder = false) {
            m_isParentFolder = isParentFolder;
            this.SetScale(1.Vector2());
            m_container.SetDragEnabled(IsLocal && !m_isParentFolder);
            Blueprint = Option.None;
            BlueprintsFolder = folder.CreateOption();
            updateData();
            updateTitle();
            UpdateDesc();
            m_bg.SetIcon(m_isParentFolder ? Assets.Unity.UserInterface.General.NavigateUp_svg : (folder.IsEmpty ? Assets.Unity.UserInterface.General.FolderEmpty_svg : Assets.Unity.UserInterface.General.Folder_svg), BG_ICON_COLOR);
            m_bg.PutTo(m_container, Offset.All(40f));
            m_missingProtosIcon.Hide();
            m_downgradeIcon.Hide();
            m_lockedOverlay.Hide();
            m_gridContainer.StartBatchOperation();
            m_gridContainer.ClearAll();
            m_iconsCache.ReturnAll();
            if (m_isParentFolder) m_gridContainer.FinishBatchOperation();
            else {
                if (folder.Blueprints.IsNotEmpty()) {
                    IconWithCount view = m_iconsCache.GetView();
                    m_gridContainer.Append(view);
                    view.SetIconAndCount(Assets.Unity.UserInterface.General.Blueprint_svg, folder.Blueprints.Count);
                }
                foreach (Proto previewProto in folder.PreviewProtos) {
                    if (previewProto is LayoutEntityProto layoutEntityProto) {
                        IconWithCount view = m_iconsCache.GetView();
                        m_gridContainer.Append(view);
                        view.SetIconAndCount(layoutEntityProto.Graphics.IconPath, 0);
                    }
                    else if (previewProto is TransportProto transportProto) {
                        IconWithCount view = m_iconsCache.GetView();
                        m_gridContainer.Append(view);
                        view.SetIconAndCount(transportProto.Graphics.IconPath, 0);
                    }
                }
                m_gridContainer.FinishBatchOperation();
            }
        }

        private void updateData() {
            if (!IsLocal && m_owner._server.HasValue) {
                itemConditions = new BlueprintsView.BlueprintItemConditions(m_owner.BlueprintManager, m_owner._server.Value);
                itemConditions.SetView(this);
                m_likeTxt.SetText(itemConditions.Data?.Like_count + "" ?? "0");
                _likeBtn.SetEnabled(!itemConditions.IsOwned && itemConditions.IsUpload);
                updateLikeVisibility();
            }
            //m_owner._blueprintsLibrary.ConvertItemToString(Item.Value);
        }

        public string ConvertItemString() {
            if (!Item.HasValue) return null;
            return m_owner._blueprintsLibrary.ConvertItemToString(Item.Value);
        }

        private void updateLikeVisibility() {
            likePanel.SetVisibility(!IsLocal && itemConditions != null && itemConditions.HasItem && !itemConditions.IsParentFolder && !itemConditions.IsUnknown);
        }

        public void SetHovered(bool isHovered) => m_hoveredBorder.SetVisibility(isHovered);

        public void SetIsSelected(bool isSelected) => m_selectedBorder.SetVisibility(isSelected);

        private void updateTitle() {
            if (m_isInRenamingSession) stopRenamingSession();
            if (Item.HasValue) m_title.SetText(m_isParentFolder ? "" : Item.Value.Name);
            m_textEditBtn.SetVisibility(!m_isParentFolder && m_isHovered && IsLocal);
            m_titleBar.SetWidth(((float)(m_title.GetPreferedWidth() + 18.0 + 2.0)).Min(WIDTH - 10));
        }

        internal void UpdateDesc() => m_descTooltip.SetText(Item.ValueOrNull?.Desc ?? "");

        private void onBeginDrag() {
            if (!IsLocal) return;
            Assert.That(m_isParentFolder).IsFalse();
            m_owner.OnDragStart(this);
        }

        private void onDrag() => m_owner.OnDragMove((Vector2)(m_container.RectTransform.position - new Vector3(0.0f, m_container.GetHeight() / 2f)));

        private void onEndDrag() => m_owner.OnDragDone();

        private void stopRenamingSession() {
            if (!IsLocal) return;
            m_isInRenamingSession = false;
            m_txtInput.Hide();
            m_textSaveBtn.Hide();
            m_textEditBtn.SetVisibility(m_isHovered && IsLocal);
            m_title.Show();
            updateLikeVisibility();
        }

        public void StartRenamingSession() {
            if (!IsLocal) return;
            if (m_isInRenamingSession) return;
            m_isInRenamingSession = true;
            m_txtInput.Show();
            m_txtInput.Focus();
            m_txtInput.SetText(m_title.Text);
            m_textSaveBtn.Show();
            m_textEditBtn.Hide();
            m_title.Hide();
            likePanel.SetVisibility(false);
        }

        private void commitRenamingSession() {
            if (Blueprint.HasValue) m_owner._blueprintsLibrary.RenameItem(Blueprint.Value, m_txtInput.GetText());
            else if (BlueprintsFolder.HasValue) m_owner._blueprintsLibrary.RenameItem(BlueprintsFolder.Value, m_txtInput.GetText());
            updateTitle();
        }

        internal bool CancelRenamingIfCan() {
            if (!m_isInRenamingSession) return false;
            stopRenamingSession();
            return true;
        }

        internal bool CommitRenamingIfCan() {
            if (!m_isInRenamingSession) return false;
            commitRenamingSession();
            return true;
        }

        private class IconWithCount : IUiElement {
            private readonly Panel m_container;
            private readonly IconContainer m_icon;
            private readonly Txt m_count;

            public GameObject GameObject => m_container.GameObject;

            public RectTransform RectTransform => m_container.RectTransform;

            public IconWithCount(IUiElement parent, UiBuilder builder) {
                m_container = builder.NewPanel("Container", parent);
                m_icon = builder.NewIconContainer("Icon", parent).PutTo(m_container, Offset.Right(5f) + Offset.Bottom(5f));
                m_count = builder.NewTxt("Txt").SetTextStyle(builder.Style.Global.Title).SetAlignment(TextAnchor.LowerLeft).AddOutline().PutToBottomOf(m_container, 25f, Offset.Left(2f));
            }

            public void SetIconAndCount(string iconPath, int count) {
                m_icon.SetIcon(iconPath);
                m_count.SetVisibility(count > 0);
                m_count.SetText(count.ToStringCached() + " x");
            }
        }
    }
}