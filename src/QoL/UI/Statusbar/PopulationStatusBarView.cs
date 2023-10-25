using DoubleQoL.Game.Patcher;
using DoubleQoL.QoL.UI.Statusbar.Component;
using Mafi;
using Mafi.Collections;
using Mafi.Core.GameLoop;
using Mafi.Core.Population;
using Mafi.Core.Syncers;
using Mafi.Unity.InputControl.TopStatusBar;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;

namespace DoubleQoL.QoL.UI.Statusbar {

    [GlobalDependency(RegistrationMode.AsEverything)]
    internal class PopulationStatusBarView : AStatusBarView {
        protected override float Order => -1f; // well
        protected override SyncFrequency SyncFreq => SyncFrequency.Critical;
        protected override bool IsEnabled => PopulationStatusBarPatcher.Instance?.Enabled ?? false;
        private readonly PopsHealthManager _popsHealthManager;

        public PopulationStatusBarView(IGameLoopEvents gameLoop, StatusBar statusBar, PopsHealthManager popsHealthManager) : base(gameLoop, statusBar) {
            _popsHealthManager = popsHealthManager;
        }

        private void StartBatchEdits() {
            InfoTileExp.ClearAll();
            InfoTileExp.StartBatchOperation();
        }

        private void FinishBatchOperation() {
            InfoTileExp.FinishBatchOperation();
        }

        /// <summary>
        /// This method is adapted from code provided in <see cref="Mafi.Unity.InputControl.Population.SettlementSummaryWindow"/>.
        /// </summary>
        protected override void OnRegisteringUi(UiBuilder builder, UpdaterBuilder updaterBuilder) {
            InfoTileExp = new InfoTileExpended(builder, "Assets/Unity/UserInterface/General/Health.svg").Build();
            Dict<HealthPointsCategoryProto, HealthPointsEntry> healthIncomes = new Dict<HealthPointsCategoryProto, HealthPointsEntry>();
            Dict<HealthPointsCategoryProto, HealthPointsEntry> healthDemands = new Dict<HealthPointsCategoryProto, HealthPointsEntry>();
            updaterBuilder.Observe(() => _popsHealthManager.HealthStats.LastMonthRecords, CompareFixedOrder<HealthStatistics.Entry>.Instance).Do(entries => {
                StartBatchEdits();
                foreach (HealthPointsEntry healthPointsEntry in healthIncomes.Values) healthPointsEntry.Reset();
                foreach (HealthPointsEntry healthPointsEntry in healthDemands.Values) healthPointsEntry.Reset();
                foreach (HealthStatistics.Entry entry in entries) {
                    if (entry.Max.IsNegative) healthDemands.GetOrAdd(entry.Category, cat => new HealthPointsEntry(cat)).Add(entry);
                    else healthIncomes.GetOrAdd(entry.Category, cat => new HealthPointsEntry(cat)).Add(entry);
                }
                foreach (HealthPointsEntry healthPointsEntry in healthIncomes.Values)
                    if (healthPointsEntry.Count != 0) InfoTileExp.Append(new IconTextView(builder, "Assets/Unity/UserInterface/General/Health.svg", healthPointsEntry.GetName(), healthPointsEntry.Exchanged.ToIntPercentRounded()).Build());
                foreach (HealthPointsEntry healthPointsEntry in healthDemands.Values)
                    if (healthPointsEntry.Count != 0) InfoTileExp.Append(new IconTextView(builder, "Assets/Unity/UserInterface/General/Health.svg", healthPointsEntry.GetName(), healthPointsEntry.Exchanged.ToIntPercentRounded()).Build());

                FinishBatchOperation();
            });

            TextWithIcon healthSubText = new TextWithIcon(builder, 15);
            healthSubText.SetTextStyle(builder.Style.Global.TextMediumBold).SetIcon("Assets/Unity/UserInterface/General/PopulationSmall.svg");
            InfoTileExp.PopInfoTile.AddCustomSubTextWithIcon(healthSubText);

            updaterBuilder.Observe(() => _popsHealthManager.HealthStats.HealthLastMonth).Observe(() => _popsHealthManager.BirthStats.BirthRateLastMonth).Do((popHealth, birthRate) => {
                InfoTileExp.PopInfoTile.SetText(popHealth.ToIntPercentRounded().ToString());
                if (popHealth <= PopsHealthManager.MIN_HEALTH - 10.Percent()) InfoTileExp.PopInfoTile.SetCriticalColor();
                else if (popHealth <= PopsHealthManager.MIN_HEALTH) InfoTileExp.PopInfoTile.SetWarningColor();
                else InfoTileExp.PopInfoTile.SetStandardColor();
                if (birthRate.IsPositive) {
                    healthSubText.SetPrefixText(string.Format("+{0}", birthRate));
                    healthSubText.SetColor(InfoTileExp.PopInfoTile.SuccessClr);
                }
                else if (birthRate.IsNegative) {
                    healthSubText.SetPrefixText(string.Format("{0}", birthRate));
                    healthSubText.SetColor(birthRate > -0.5.Percent() ? InfoTileExp.PopInfoTile.WarningClr : InfoTileExp.PopInfoTile.CritricalClr);
                }
                healthSubText.SetVisibility(birthRate.IsNotZero);
            });
        }

        /// <summary>
        /// This class is adapted from code provided in <see cref="Mafi.Unity.InputControl.Population.SettlementSummaryWindow"/>.
        /// </summary>
        private class HealthPointsEntry {
            public readonly HealthPointsCategoryProto Category;
            public Percent Exchanged;
            public Percent Max;
            public int Count;

            public HealthPointsEntry(HealthPointsCategoryProto category) {
                Category = category;
            }

            public void Reset() {
                Exchanged = Percent.Zero;
                Max = Percent.Zero;
                Count = 0;
            }

            public void Add(HealthStatistics.Entry entry) {
                ++Count;
                Exchanged += entry.Change;
                Max += entry.Max;
            }

            public string GetName() => Category.Title.TranslatedString;
        }
    }
}