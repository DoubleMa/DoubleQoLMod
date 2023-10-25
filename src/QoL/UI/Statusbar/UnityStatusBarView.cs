using DoubleQoL.Extensions;
using DoubleQoL.Game.Patcher;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi;
using Mafi.Collections;
using Mafi.Core.GameLoop;
using Mafi.Core.Population;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UserInterface;

namespace DoubleQoL.QoL.UI.Statusbar {

    [GlobalDependency(RegistrationMode.AsEverything)]
    internal class UnityStatusBarView : AStatusBarView {
        protected override float Order => 0f;
        protected override SyncFrequency SyncFreq => SyncFrequency.Critical;
        protected override bool IsEnabled => PopulationStatusBarPatcher.Instance?.Enabled ?? false;

        private readonly UpointsManager _upointsManager;

        public UnityStatusBarView(IGameLoopEvents gameLoop, StatusBar statusBar, UpointsManager upointsManager) : base(gameLoop, statusBar) {
            _upointsManager = upointsManager;
        }

        private void StartBatchEdits() {
            InfoTileExp.ClearAll();
            InfoTileExp.StartBatchOperation();
        }

        private void FinishBatchOperation() {
            InfoTileExp.FinishBatchOperation();
        }

        /// <summary>
        /// This method is adapted from code provided in <see cref="Mafi.Unity.InputControl.Population.SettlementSummaryWindow/>.
        /// </summary>
        protected override void OnRegisteringUi(UiBuilder builder, UpdaterBuilder updaterBuilder) {
            InfoTileExp = new InfoTileExpended(builder, "Assets/Unity/UserInterface/General/Unity.svg").Build();

            Dict<UpointsCategoryProto, UpointsEntry> upointsIncomes = new Dict<UpointsCategoryProto, UpointsEntry>();
            Dict<UpointsCategoryProto, UpointsEntry> upointsDemands = new Dict<UpointsCategoryProto, UpointsEntry>();

            updaterBuilder.Observe(() => _upointsManager.Stats.ThisMonthRecords, CompareFixedOrder<UpointsStats.Entry>.Instance).Do(entries => {
                StartBatchEdits();
                foreach (UpointsEntry upointsEntry in upointsIncomes.Values) upointsEntry.Reset();
                foreach (UpointsEntry upointsEntry in upointsDemands.Values)
                    upointsEntry.Reset();
                foreach (UpointsStats.Entry entry in entries) {
                    if (!entry.Category.IsOneTimeAction) {
                        if (entry.Max.IsNegative) upointsDemands.GetOrAdd(entry.Category, cat => new UpointsEntry(cat)).Add(entry);
                        else upointsIncomes.GetOrAdd(entry.Category, cat => new UpointsEntry(cat)).Add(entry);
                    }
                }
                foreach (UpointsEntry upointsEntry in upointsIncomes.Values)
                    if (upointsEntry.Count != 0) InfoTileExp.Append(new IconTextView(builder, "Assets/Unity/UserInterface/General/Unity.svg", upointsEntry.GetName(), upointsEntry.Exchanged.Value.ToFloat(), (e) => e.AddSignAndFormat()).Build());
                foreach (UpointsEntry upointsEntry in upointsDemands.Values)
                    if (upointsEntry.Count != 0) InfoTileExp.Append(new IconTextView(builder, "Assets/Unity/UserInterface/General/Unity.svg", upointsEntry.GetName(), upointsEntry.Exchanged.Value.ToFloat(), (e) => e.AddSignAndFormat()).Build());

                FinishBatchOperation();
            });

            updaterBuilder.Observe(() => _upointsManager.Quantity).Observe(() => _upointsManager.PossibleDiffForLastMonth).Do((quantity, lastDiff) => {
                InfoTileExp.PopInfoTile.SetText(_upointsManager.Quantity.Upoints().FormatWithUnitySuffix().Value);
                if (lastDiff.IsNegative) {
                    if (quantity.IsZero) InfoTileExp.PopInfoTile.SetCriticalColor();
                    else InfoTileExp.PopInfoTile.SetStandardColor();
                    InfoTileExp.PopInfoTile.SetSubText(Tr.QuantityPerMonth.Format(lastDiff.Format()).Value, InfoTileExp.PopInfoTile.CritricalClr);
                }
                else {
                    InfoTileExp.PopInfoTile.SetStandardColor();
                    InfoTileExp.PopInfoTile.SetSubText(Tr.QuantityPerMonth.Format("+" + lastDiff.Format()).Value, InfoTileExp.PopInfoTile.SuccessClr);
                }
            });
        }

        /// <summary>
        /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Population.SettlementSummaryWindow"/>.
        /// </summary>
        private class UpointsEntry {
            public readonly UpointsCategoryProto Category;
            public Upoints Exchanged;
            public Upoints Max;
            public int Count;
            public readonly Lyst<UpointsStats.Entry> Entries;

            public UpointsEntry(UpointsCategoryProto category) {
                Entries = new Lyst<UpointsStats.Entry>();
                Category = category;
            }

            public void Reset() {
                Exchanged = Upoints.Zero;
                Max = Upoints.Zero;
                Count = 0;
                Entries.Clear();
            }

            public void Add(UpointsStats.Entry entry) {
                ++Count;
                Exchanged += entry.Exchanged;
                Max += entry.Max;
                Entries.Add(entry);
            }

            public string GetName() => Category.HideCount || Count <= 1 ? Category.Title.TranslatedString : string.Format("{0}x {1}", (object)Count, (object)Category.Title.TranslatedString);
        }
    }
}